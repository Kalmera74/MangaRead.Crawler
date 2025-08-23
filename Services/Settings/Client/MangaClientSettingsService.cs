
using Microsoft.Extensions.Configuration;

public static class MangaClientSettingsService
{

    public static string CreateEndpoint => ConfigurationService.GetCrawlConfiguration().GetSection("ClientSettings:MangaClient:Create").Get<string>() ?? string.Empty;
    public static string GetEndpoint => ConfigurationService.GetCrawlConfiguration().GetSection("ClientSettings:MangaClient:Get").Get<string>() ?? string.Empty;
    public static string UpdateEndpoint => ConfigurationService.GetCrawlConfiguration().GetSection("ClientSettings:MangaClient:Update").Get<string>() ?? string.Empty;
}
