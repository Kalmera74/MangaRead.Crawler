# Manga Read Crawler

MangaRead Crawler is an extensible, backend-agnostic crawler for manga and web novel sites.  
It supports static, dynamic, and API-based sites (currently only static support is implemented).

This project is developed alongside [MangaRead.Backend](https://github.com/Kalmera74/MangaRead.Backend), but can be easily adapted to any backend service.

---

## Features

- Crawl static manga sites using XPath
- Parallel or sequential crawling (configurable concurrency)
- Fully configurable via JSON (`SiteData.json`, `appsettings.json`)
- Extensible architecture – new sites can be added without code changes
- Proxy support
- Resiliency (Polly)
- Multiple attribute source support for non-uniform sites
- Planned support for dynamic & API-based sites

---

##  Quickstart

```bash
git clone https://github.com/Kalmera74/MangaRead.Crawler
cd MangaRead.Crawler
dotnet run
```

---

By default, the crawler loads site configurations from Configs/ and runs each crawler asynchronously. It can crawl multiple sites with multiple pages at the same time (they all can be configured)

To add a new site, create a SiteData.json file and reference it in appsettings.json.

---

## Deployment

This project includes a `build.sh` script that automates deployment:
1. Builds and publishes the Crawler
2. Configures as a system service (auto-restart, logging)
3. Starts/reloads the service
4. Adds CLI shortcuts for management


## Configuration

Configuration is split across two files:

SiteData.json → Defines how to scrape a specific site (selectors, page data, etc.).

appsettings.json → Defines crawler runtime behavior (concurrency, API settings, proxy, etc.).

## sitedata.json

Each site data json file define how to scrape the corresponding site to get the required manga information as well as the chapter images. Crawler uses Xpath to query the page.
Note that right now Crawler only supports Static sites.

#### SiteType

- **Type**: `enum`
- **Description**: The type of the site being crawled. This can help determine the method used for data extraction (e.g., Static, Dynamic, or API).

#### RootUrl

- **Type**: `URL`
- **Description**: The root of the site for logging purposes

#### ChapterRootUrl

- **Type**: `URL`
- **Description**: THe common root for all the chapter pages

#### PageData

- **Selector**: A selector is comprise of the following attributes
  - **Query**: XPath expression used to find the title element in the HTML.
  - **Type**: `enum` - Indicates if the selector extracts a "Single" or "Multi" result. Multi means that query will return multiple nodes
  - **DataFrom**: `enum` - Specifies where to extract data from the selected element (e.g., InnerHtml, InnerText, Src, Href, DataBackground, BackgroundImage).

This section contains selectors for extracting different types of information from the manga page. Each selector for the PageData field can hold multiple Selectors to cover cases
where some sites serves non-uniform DOM. For example, TitleSelector could have multiple Selector to query the title of the manga.

- **TitleSelector**:

  - **Type**: `List<Selector>`
  - **Description**: Contains the query to extract the title of the manga.

- **DescriptionSelector**:

  - **Type**: `List<Selector>`
  - **Description**: Contains the query to extract the description of the manga.

- **CoverSelector**:

  - **Type**: `List<Selector>`
  - **Description**: Contains the query to extract the cover image of the manga.

- **StatusSelector**:

  - **Type**: `List<Selector>`
  - **Description**: Contains the query to extract the current status of the manga (e.g., ongoing, completed).

- **TypeSelector**:

  - **Type**: `List<Selector>`
  - **Description**: Contains the query to extract the type of the manga (e.g., webtoon, manga). Can be empty. Which then prompts the crawler to assign it `N/A` unless it can extract the type from Genre section (some sites use type as a genre ) or used the OverrideTye parameter in the MangaPage section to manually enter a type per page that is to be crawled

- **AuthorSelector**:

  - **Type**: `List<Selector>`
  - **Description**: Contains the query to extract the author's name.

- **RatingSelector**:

  - **Type**: `List<Selector>`
  - **Description**: Contains the query to extract the rating of the manga. Can be empty, which will prompt the crawler to assign a random rating. Note; right now API does not accept rating, so this is only for future compatibility

- **GenreSelector**:

  - **Type**: `List<Selector>`
  - **Description**: Contains the query to extract the genres associated with the manga.

- **ChapterLinksSelector**:

  - **Type**: `List<Selector>`
  - **Description**: Contains the query to extract links to the chapters of the manga.

- **ChapterTitlesSelector**:

  - **Type**: `List<Selector>`
  - **Description**: Contains the query to extract titles of the chapters.

- **ChapterContentsSelector**:
  - **Type**: `List<Selector>`
  - **Description**: Contains multiple queries to extract the contents of the chapters, including images and backgrounds.

#### MangaPages

This section defines the specific pages to crawl for manga data.

- **Url**:

  - **Type**: `string`
  - **Description**: The URL of the manga series page to crawl.

-**OverrideType**

- **Type**: `string`
- **Default**: `null`
- **Description**: It is null by default however if it is filled it'll override the manga type. It is primarily used for sites where they don't have type field on page

- **FullUpdate**:

  - **Type**: `bool`
  - **Default**: `false`
  - **Description**: Indicates whether to perform a full update of the data. Set to `true` if the existing data should be completely replaced.

- **SaveImages**:
  - **Type**: `bool`
  - **Default**: `false`
  - **Description**: Indicates whether to save images found in the manga chapters. Set to `true` if images should be saved locally.

## **Example Site Configuration**

```json
{
  "SiteType": "Static",
  "RootUrl": "https://asuracomic.net",
  "ChapterRootUrl": "https://asuracomic.net/series",
  "PageData": {
    "TitleSelector": [
      {
        "Query": "//h1[contains(@class, 'entry-title')]",
        "Type": "Single",
        "DataFrom": "InnerText"
      },
      {
        "Query": "//div//div[2]//p",
        "Type": "Single",
        "DataFrom": "InnerText"
      }
    ],
    "DescriptionSelector": [
      {
        "Query": "//div[contains(@class, 'entry-content entry-content-single')]/p",
        "Type": "Single",
        "DataFrom": "InnerText"
      }
    ],
    "CoverSelector": [
      {
        "Query": "//img[contains(@class, 'wp-post-image')]",
        "Type": "Single",
        "DataFrom": "Src"
      }
    ],
    ...
  },
  "MangaPages": [
    {
      "Url": "https://mangatest.com/read/some-manga/",
      "FullUpdate": false,
      "SaveImages": false
    }
  ]
}
```

## appsettings.json

appsettings.json is used to configure the crawler operation. It is used to configure the concurrency, API and API Clients,proxy, logging and the Site Data Settings json

#### CrawlSettings

This section contains settings related to crawling behavior.

- **ParallelCrawlersOnStart**:

  - **Type**: `int`
  - **Default**: `1`
  - **Description**: The number of crawlers to start initially. Adjust this value based on your system's capability and the load of the websites being crawled. Each crawler corresponds to site

- **MaxParallelCrawlers**:

  - **Type**: `int`
  - **Default**: `1`
  - **Description**: The maximum number of crawlers that can run in parallel at any given time.

- **APISettings**:

  - **Address**:
    - **Type**: `string`
    - **Default**: `"http://localhost:8080/api/v1/"`
    - **Description**: The base URL for the API that the application will communicate with. Update this to the address of your API server.

- **HttpClientSettings**:

  - **CrawlDelay**:

    - **Type**: `int`
    - **Default**: `10000` (in milliseconds)
    - **Description**: The delay (in milliseconds) between successive crawl requests. Adjust to avoid overloading the target servers. Set to a higher value if needed.

  - **CrawlTimeout**:

    - **Type**: `int`
    - **Default**: `30000` (in milliseconds)
    - **Description**: The time limit (in milliseconds) for each crawl request. Set this to ensure the application does not hang indefinitely.

  - **UserAgent**:

    - **Type**: `string`
    - **Default**: `"Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/89.207.132.170 Safari/537.36"`
    - **Description**: The User-Agent string sent with HTTP requests to identify the crawler. You can customize this to mimic different browsers.

  - **UseProxy**:

    - **Type**: `bool`
    - **Default**: `false`
    - **Description**: Set to `true` if you want to use a proxy for crawling.

  - **ProxyList**:
    - **Type**: `List<string>`
    - **Default**: `[]`
    - **Description**: A list of proxy addresses (in string format) to be used for crawling. If `UseProxy` is `true`, specify the proxies here.

- **SiteSettings**:

  - **SitesToCrawl**:

    - **Type**: `List<SiteToCrawlData>`
    - **Description**: List of sites to crawl, where each entry has the following properties:

      - **CrawlerType**:

        - **Type**: `enum`
        - **Description**: The type of crawler to use (e.g., `"MangaCrawler"` or `WebNovelCrawler`). Specify the appropriate crawler for the site.

      - **DataFile**:
        - **Type**: `string`
        - **Description**: Path to the configuration file for the site (e.g., `"./Configs/RadiantScans.json"`). Ensure this file exists and is correctly formatted.

#### ClientSettings

This section contains the endpoints for various clients used in the application.

- **AuthorClient**:

  - **Create**: `string` - Endpoint to create authors (e.g., `"authors/"`).
  - **Get**: `string` - Endpoint to get authors by slug (e.g., `"authors/slugged/"`).

- **FileClient**:

  - **Create**: `string` - Endpoint to create downloaded files (e.g., `"files/downloaded/"`).

- **GenreClient**:

  - **Create**: `string` - Endpoint to create genres (e.g., `"genres/"`).
  - **Get**: `string` - Endpoint to get genres by slug (e.g., `"genres/slugged/"`).

- **ImageClient**:

  - **Create**: `string` - Endpoint to create images (e.g., `"images/"`).
  - **Get**: `string` - Endpoint to search images by url (e.g., `"images/search/"`).

- **MangaChapterClient**:

  - **Create**: `string` - Endpoint to create manga chapters (e.g., `"manga-chapters/"`).
  - **Get**: `string` - Endpoint to get manga chapters (e.g., `"manga-chapters/"`).

- **MangaChapterContentClient**:

  - **Create**: `string` - Endpoint to create manga chapter contents (e.g., `"manga-chapter-contents/"`).
  - **Get**: `string` - Endpoint to get manga chapter contents (e.g., `"manga-chapter-contents/"`).
  - **Update**: `string` - Endpoint to update manga chapter contents (e.g., `"manga-chapter-contents/"`).

- **MangaClient**:

  - **Create**: `string` - Endpoint to create mangas (e.g., `"mangas/"`).
  - **Get**: `string` - Endpoint to get mangas by slug (e.g., `"mangas/slugged/"`).

- **MangaTypeClient**:

  - **Create**: `string` - Endpoint to create manga types (e.g., `"manga-types/"`).
  - **Get**: `string` - Endpoint to get manga types by slug (e.g., `"manga-types/slugged/"`).

- **StatusClient**:
  - **Create**: `string` - Endpoint to create statuses (e.g., `"statuses/"`).
  - **Get**: `string` - Endpoint to get statuses by slug (e.g., `"statuses/slugged/"`).

## **Example Crawl Configuration**

```json
{
  "CrawlSettings": {
    "ParallelCrawlersOnStart": 2,
    "MaxParallelCrawlers": 4,
    "APISettings": {
      "Address": "http://localhost:8080/api/v1/"
    },
    "HttpClientSettings": {
      "CrawlDelay": 5000,
      "CrawlTimeout": 15000,
      "UserAgent": "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/89.207.132.170 Safari/537.36",
      "UseProxy": true,
      "ProxyList": ["http://proxy1:8080", "http://proxy2:8080"]
    },
    "SiteSettings": {
      "SitesToCrawl": [
        {
          "CrawlerType": "MangaCrawler",
          "DataFile": "./Configs/MangaConfig.json"
        }
      ]
    },
    "ClientSettings": {
       "AuthorClient": {
        "Create": "authors/",
        "Get": "authors/slugged/"
      },
      "FileClient": {
        "Create": "files/downloaded/"
      },
      ...
    }
  }
}
```
