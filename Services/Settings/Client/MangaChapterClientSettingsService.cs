
using Microsoft.Extensions.Configuration;

public static class MangaChapterClientSettingsService
{
    public static string CreateEndpoint => ConfigurationService.GetCrawlConfiguration().GetSection("ClientSettings:MangaChapterClient:Create").Get<string>() ?? string.Empty;
    public static string GetEndpoint => ConfigurationService.GetCrawlConfiguration().GetSection("ClientSettings:MangaChapterClient:Get").Get<string>() ?? string.Empty;
    public static string UpdateEndpoint => ConfigurationService.GetCrawlConfiguration().GetSection("ClientSettings:MangaChapterClient:Update").Get<string>() ?? string.Empty;
}
