using Serilog;
using System.Text.Json;
namespace MangaRead.Crawler
{
    internal class Program
    {
        static async Task Main(string[] args)
        {

            LogService.ConfigureLogs();

            List<Task> _tasks = new List<Task>();

            var crawlData = CrawlSettingsService.SitesToCrawl;

            if (!crawlData.Any())
            {
                Log.Error("No sites to crawl, aborting");
                return;
            }

            SemaphoreSlim semaphoreSlim = new SemaphoreSlim(CrawlSettingsService.ParallelCrawlersOnStart, CrawlSettingsService.MaxParallelCrawlers);
            Log.Information($"Created Semaphore with  initial concurrency: {CrawlSettingsService.ParallelCrawlersOnStart} and max concurrency: {CrawlSettingsService.MaxParallelCrawlers}");

            foreach (var data in crawlData)
            {
                _tasks.Add(RunCrawlerAsync(data, semaphoreSlim));
            }

            await Task.WhenAll(_tasks).ConfigureAwait(false);
        }

        private static async Task RunCrawlerAsync(SiteToCrawlData siteToCrawl, SemaphoreSlim semaphoreSlim, CancellationToken cancellationToken = default)
        {
            Log.Information($"Waiting Semaphore for file: {siteToCrawl.DataFile}");
            await semaphoreSlim.WaitAsync(cancellationToken);
            try
            {
                Log.Information($"Acquired Semaphore for file: {siteToCrawl.DataFile}");
                var rf = await File.ReadAllTextAsync(siteToCrawl.DataFile);


                SiteData? siteData = SerializeSiteData(rf, siteToCrawl.CrawlerType);

                Log.Information($"Reading config file: {siteToCrawl.DataFile}");

                if (siteData == null)
                {
                    Log.Error($"Could not read config file: {siteToCrawl.DataFile}, skipping file");
                    return;
                }

                var apiHttpClient = HttpClientService.GetApiHttpClient();
                var cw = CrawlerFactory.CreateCrawler(siteToCrawl.CrawlerType, siteData, apiHttpClient);

                await cw.Crawl(cancellationToken);
            }
            catch (JsonException ex)
            {
                Log.Error(ex, $"Failed to deserialize JSON from config file: {siteToCrawl.DataFile}");
                return;
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"Error crawling site: {siteToCrawl.DataFile}");
            }
            finally
            {
                Log.Information($"Released Semaphore for file: {siteToCrawl.DataFile}");
                semaphoreSlim.Release();
            }
        }

        private static SiteData? SerializeSiteData(string rf, CrawlerType crawlerType)
        {
            return crawlerType switch
            {
                CrawlerType.MangaCrawler => JsonSerializer.Deserialize<MangaSiteData>(rf),
                CrawlerType.WebNovelCrawler => JsonSerializer.Deserialize<WebNovelSiteData>(rf),
                _ => throw new NotImplementedException($"Crawler type {crawlerType} not implemented"),
            };
        }
    }

}
