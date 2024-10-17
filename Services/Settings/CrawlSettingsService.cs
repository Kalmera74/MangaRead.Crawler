using Microsoft.Extensions.Configuration;
using System.Text.Json.Serialization;

public static class CrawlSettingsService
{
    public static int ParallelCrawlersOnStart => GetCrawlConfiguration().GetSection("ParallelCrawlersOnStart")?.Get<int>() ?? 1;
    public static int MaxParallelCrawlers => GetCrawlConfiguration().GetSection("MaxParallelCrawlers")?.Get<int>() ?? 1;
    public static int CrawlDelayPerMangaBatchInMs => GetCrawlConfiguration().GetSection("CrawlDelayPerMangaBatchInMs")?.Get<int>() ?? 0;
    public static int CrawlDelayPerChapterInMs => GetCrawlConfiguration().GetSection("CrawlDelayPerChapterInMs")?.Get<int>() ?? 0;
    public static int ConcurrentMangasPerSite => GetCrawlConfiguration().GetSection("ConcurrentMangasPerSite")?.Get<int>() ?? 1;
    public static List<SiteToCrawlData> SitesToCrawl => GetCrawlConfiguration().GetSection("SiteSettings:SitesToCrawl")?.Get<List<SiteToCrawlData>>() ?? new List<SiteToCrawlData>();
    public static IConfiguration GetCrawlConfiguration()
    {
        return ConfigurationService.GetCrawlConfiguration();
    }
}

public class SiteToCrawlData
{
    [JsonConverter(typeof(EnumStringConverter<CrawlerType>))]
    public CrawlerType CrawlerType { get; set; }
    public string DataFile { get; set; } = string.Empty;
}

public enum CrawlerType
{
    MangaCrawler,
    WebNovelCrawler
}
