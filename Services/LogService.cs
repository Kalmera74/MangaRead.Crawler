using Serilog;

public static class LogService
{
    public static void ConfigureLogs()
    {
        var config = ConfigurationService.GetLogConfiguration();
        Log.Logger = new LoggerConfiguration().ReadFrom.Configuration(config).CreateLogger();
    }
}