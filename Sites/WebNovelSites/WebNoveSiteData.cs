public class WebNovelSiteData : SiteData
{
    public List<WebNovelPage> WebNovelPages { get; set; } = new List<WebNovelPage>();
}
public class WebNovelPage
{
    public string Url { get; set; } = string.Empty;
    public bool FullUpdate { get; set; } = false;
    public bool SaveImages { get; set; } = false;
}