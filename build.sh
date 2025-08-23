#!/bin/bash
set -euo pipefail

# ===========================
# CONFIGURATION
# ===========================
CRAWLER="./src/MangaRead.Crawler"
INFRASTRUCTURE="./src/MangaRead.Infrastructure"
CRAWLER_OUTPUT_DIR="/usr/local/bin/manga-crawler"

# ===========================
# HELPERS
# ===========================
log()    { echo -e "\033[1;32m[+] $1\033[0m"; }
warn()   { echo -e "\033[1;33m[!] $1\033[0m"; }
error()  { echo -e "\033[1;31m[✗] $1\033[0m"; exit 1; }

# ===========================
# TASKS
# ===========================
migration() {
    log "Applying database migrations..."
    dotnet ef database update \
        --project "$INFRASTRUCTURE" \
        --startup-project "$CRAWLER" \
        -- --environment Production
}

clean() {
    log "Cleaning previous builds..."
    dotnet clean
    rm -rf "$CRAWLER_OUTPUT_DIR"
}

restore() {
    clean
    log "Restoring dependencies..."
    dotnet restore
}

build() {
    restore
    migration
    log "Building crawler project..."
    dotnet build "$CRAWLER"
}

publish() {
    build
    log "Publishing crawler to $CRAWLER_OUTPUT_DIR..."
    mkdir -p "$CRAWLER_OUTPUT_DIR"
    export ASPNETCORE_ENVIRONMENT="Production"
    dotnet publish "$CRAWLER" --configuration Release --output "$CRAWLER_OUTPUT_DIR"
}

create_crawler_service() {
    local service_file="/etc/systemd/system/crawler.service"

    if [[ -f "$service_file" ]]; then
        warn "Crawler service already exists: $service_file"
        return
    fi

    log "Creating crawler systemd service..."
    cat <<EOF > "$service_file"
[Unit]
Description=MangaLuckCrawler

[Service]
WorkingDirectory=$CRAWLER_OUTPUT_DIR
ExecStart=dotnet $CRAWLER_OUTPUT_DIR/MangaLuckNeo.Crawler.dll
Restart=no
RestartSec=10
KillSignal=SIGINT
SyslogIdentifier=MangaLuckCrawler
User=root

[Install]
WantedBy=multi-user.target
EOF
}

create_aliases() {
    log "Checking and adding aliases..."
    local aliases=(
        "alias crawler-stat='systemctl status crawler'"
        "alias crawler-start='systemctl start crawler'"
        "alias crawler-stop='systemctl stop crawler'"
        "alias crawler-restart='systemctl restart crawler'"
    )

    for alias in "${aliases[@]}"; do
        if ! grep -qF "$alias" ~/.bashrc; then
            echo "$alias" >> ~/.bashrc
            log "Added alias: $alias"
        else
            warn "Alias already exists: $alias"
        fi
    done

    # Reload aliases in current shell
    source ~/.bashrc
}

# ===========================
# MAIN
# ===========================
publish
create_crawler_service

log "Reloading systemd and enabling crawler service..."
systemctl daemon-reload
systemctl enable crawler
systemctl start crawler || warn "Crawler failed to start"

create_aliases

log "Crawler service created, started, and aliases added."
