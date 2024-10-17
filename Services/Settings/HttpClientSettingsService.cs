using Microsoft.Extensions.Configuration;

public static class HttpClientSettingsService
{
    public static string APIAddress => ConfigurationService.GetAPIConfiguration().GetSection("Address")?.Get<string>() ?? "http://localhost:8080/api/v1/";
    public static int TimeoutInMs => GetCrawlConfiguration().GetSection("HttpClientSettings:CrawlTimeout")?.Get<int>() ?? 1000;
    public static string UserAgent => GetCrawlConfiguration().GetSection("HttpClientSettings:UserAgent")?.Get<string>() ?? "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/89.0.142.86 Safari/537.36";
    public static bool UseProxy => (GetCrawlConfiguration().GetSection("HttpClientSettings:UseProxy")?.Get<bool>() ?? false) && _canUseProxy;
    public static List<string> ProxyList => GetCrawlConfiguration().GetSection("HttpClientSettings:ProxyList")?.Get<List<string>>() ?? new List<string>();

    private static bool _canUseProxy => ProxyList.Any();
    public static IConfiguration GetCrawlConfiguration()
    {
        return ConfigurationService.GetCrawlConfiguration();
    }

}
