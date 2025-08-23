
using Microsoft.Extensions.Configuration;

public static class MangaChapterContentClientSettingsService
{
    public static string CreateEndpoint => ConfigurationService.GetCrawlConfiguration().GetSection("ClientSettings:MangaChapterContentClient:Create").Get<string>() ?? string.Empty;
    public static string GetEndpoint => ConfigurationService.GetCrawlConfiguration().GetSection("ClientSettings:MangaChapterContentClient:Get").Get<string>() ?? string.Empty;
    public static string UpdateEndpoint => ConfigurationService.GetCrawlConfiguration().GetSection("ClientSettings:MangaChapterContentClient:Update").Get<string>() ?? string.Empty;
}
