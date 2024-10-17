using Serilog;
using System.Net;

public static class HttpClientService
{
    public static HttpClient GetApiHttpClient()
    {
        HttpClient httpClient = new HttpClient();

        httpClient.Timeout = TimeSpan.FromMilliseconds(HttpClientSettingsService.TimeoutInMs);
        httpClient.DefaultRequestHeaders.Add("User-Agent", HttpClientSettingsService.UserAgent);

        httpClient.BaseAddress = new Uri(HttpClientSettingsService.APIAddress);

        return httpClient;
    }
    public static HttpClient GetSiteHttpClient()
    {
        HttpClient httpClient = HttpClientSettingsService.UseProxy ? GetProxyHttpClient(HttpClientSettingsService.ProxyList) : new HttpClient();

        httpClient.Timeout = TimeSpan.FromMilliseconds(HttpClientSettingsService.TimeoutInMs);
        httpClient.DefaultRequestHeaders.Add("User-Agent", HttpClientSettingsService.UserAgent);

        httpClient.BaseAddress = new Uri(HttpClientSettingsService.APIAddress);

        return httpClient;
    }


    private static HttpClient GetProxyHttpClient(List<string> proxyList)
    {
        foreach (var proxyAddress in proxyList)
        {
            try
            {
                Log.Information($"Trying proxy: {proxyAddress}");
                HttpClientHandler handler = new HttpClientHandler
                {
                    UseProxy = true,
                    UseDefaultCredentials = false,
                    ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => true,
                    Proxy = new WebProxy
                    {
                        Address = new Uri(proxyAddress),
                        BypassProxyOnLocal = false,
                        UseDefaultCredentials = false
                    }
                };

                using (var testClient = new HttpClient(handler))
                {
                    var testRequest = new HttpRequestMessage(HttpMethod.Get, "http://www.google.com");
                    var response = testClient.SendAsync(testRequest).Result;


                    if (response.IsSuccessStatusCode)
                    {
                        Log.Information($"Proxy works! Using proxy: {proxyAddress}");
                        return new HttpClient(new HttpClientHandler
                        {
                            UseProxy = true,
                            UseDefaultCredentials = false,
                            ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => true,
                            Proxy = new WebProxy
                            {
                                Address = new Uri(proxyAddress),
                                BypassProxyOnLocal = false,
                                UseDefaultCredentials = false
                            }
                        });
                    }
                }
            }
            catch
            {
                Log.Warning($"Failed to use proxy: {proxyAddress}, trying next one...");
                continue;
            }
        }

        Log.Warning("Failed to use any proxy, using default HttpClient");
        return new HttpClient();
    }

}