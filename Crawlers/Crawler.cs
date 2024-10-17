public abstract class Crawler
{

    public abstract Task Crawl(CancellationToken cancellationToken = default);
}