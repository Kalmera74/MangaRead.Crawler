public class MangaSiteData : SiteData
{
    public List<MangaPage> MangaPages { get; set; } = new List<MangaPage>();
}
public class MangaPage
{
    public string Url { get; set; } = string.Empty;
    public string? OverrideType { get; set; }
    public bool FullUpdate { get; set; } = false;
    public bool SaveImages { get; set; } = false;
}

