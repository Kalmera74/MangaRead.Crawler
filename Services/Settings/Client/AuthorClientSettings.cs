using Microsoft.Extensions.Configuration;

public static class AuthorClientSettings
{
    public static string CreateEndpoint => ConfigurationService.GetCrawlConfiguration().GetSection("ClientSettings:AuthorClient:Create").Get<string>() ?? string.Empty;
    public static string GetEndpoint => ConfigurationService.GetCrawlConfiguration().GetSection("ClientSettings:AuthorClient:Get").Get<string>() ?? string.Empty;
    public static string UpdateEndpoint => ConfigurationService.GetCrawlConfiguration().GetSection("ClientSettings:AuthorClient:Update").Get<string>() ?? string.Empty;
}
