using Microsoft.Extensions.Configuration;

public static class MangaTypeClientSettingsService
{
    public static string CreateEndpoint => ConfigurationService.GetCrawlConfiguration().GetSection("ClientSettings:MangaTypeClient:Create").Get<string>() ?? string.Empty;
    public static string GetEndpoint => ConfigurationService.GetCrawlConfiguration().GetSection("ClientSettings:MangaTypeClient:Get").Get<string>() ?? string.Empty;
    public static string UpdateEndpoint => ConfigurationService.GetCrawlConfiguration().GetSection("ClientSettings:MangaTypeClient:Update").Get<string>() ?? string.Empty;
}
