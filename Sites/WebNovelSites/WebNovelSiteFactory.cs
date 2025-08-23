

public static class WebNovelSiteFactory
{
    public static Site CreateSite(WebNovelSiteData siteData)
    {
        switch (siteData.SiteType)
        {
            case SiteType.Static:
                return new StaticWebNovelSite(HttpClientService.GetSiteHttpClient(), siteData);
            case SiteType.Dynamic:
                return new DynamicWebNovelSite(HttpClientService.GetSiteHttpClient(), siteData);
            case SiteType.API:
                return new APIWebNovelSite(HttpClientService.GetSiteHttpClient(), siteData);
            default:
                throw new NotImplementedException();
        }
    }
}
