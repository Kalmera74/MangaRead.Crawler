
public static class MangaSiteFactory
{
    public static Site CreateSite(MangaSiteData siteData)
    {
        switch (siteData.SiteType)
        {
            case SiteType.Static:
                return new StaticMangaSite(HttpClientService.GetSiteHttpClient(), siteData);
            case SiteType.Dynamic:
                return new DynamicMangaSite(HttpClientService.GetSiteHttpClient(), siteData);
            case SiteType.API:
                return new APIMangaSite(HttpClientService.GetSiteHttpClient(), siteData);
            default:
                throw new NotImplementedException();
        }
    }
}