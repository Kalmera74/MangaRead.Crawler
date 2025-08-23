
using Microsoft.Extensions.Configuration;

public static class FileClientSettingsService
{

    public static string CreateEndpoint => ConfigurationService.GetCrawlConfiguration().GetSection("ClientSettings:FileClient:Create").Get<string>() ?? string.Empty;
    public static string GetEndpoint => ConfigurationService.GetCrawlConfiguration().GetSection("ClientSettings:FileClient:Get").Get<string>() ?? string.Empty;
    public static string UpdateEndpoint => ConfigurationService.GetCrawlConfiguration().GetSection("ClientSettings:FileClient:Update").Get<string>() ?? string.Empty;
}
