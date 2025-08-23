using Microsoft.Extensions.Caching.Memory;

public static class CrawlerFactory
{
    private static IMemoryCache GenreCache = new MemoryCache(new MemoryCacheOptions());
    private static IMemoryCache MangaTypeCache = new MemoryCache(new MemoryCacheOptions());
    private static IMemoryCache StatusCache = new MemoryCache(new MemoryCacheOptions());
    public static Crawler CreateCrawler(CrawlerType crawlerType, SiteData siteData, HttpClient httpClient)
    {
        return crawlerType switch
        {
            CrawlerType.MangaCrawler => new MangaCrawler((MangaSiteData)siteData, httpClient, GenreCache, MangaTypeCache, StatusCache),
            CrawlerType.WebNovelCrawler => new WebNovelCrawler((WebNovelSiteData)siteData, httpClient),
            _ => throw new NotImplementedException($"Crawler type {crawlerType} not implemented"),
        };
    }

}