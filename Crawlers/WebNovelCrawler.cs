

public class WebNovelCrawler : Crawler
{
    private WebNovelSiteData _siteData;
    public WebNovelCrawler(WebNovelSiteData siteData, HttpClient httpClient)
    {
        _siteData = siteData;
    }

    public override Task Crawl(CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}