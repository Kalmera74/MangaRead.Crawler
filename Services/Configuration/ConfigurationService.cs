using Microsoft.Extensions.Configuration;

public static class ConfigurationService
{
    private static IConfiguration Configuration { get; set; }

    static ConfigurationService()
    {


        var builder = new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .AddJsonFile($"appsettings.Development.json", optional: true, reloadOnChange: true)
            .AddJsonFile($"appsettings.Production.json", optional: true, reloadOnChange: true);

        Configuration = builder.Build();
    }

    public static IConfiguration GetLogConfiguration()
    {
        return Configuration;
    }
    public static IConfiguration GetCrawlConfiguration()
    {
        return Configuration.GetSection("CrawlSettings");
    }


    public static IConfiguration GetHttpClientConfiguration()
    {
        return Configuration.GetSection("CrawlSettings:HttpClientSettings");
    }
    public static IConfiguration GetAPIConfiguration()
    {
        return Configuration.GetSection("CrawlSettings:APISettings");
    }
}
