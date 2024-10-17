using System.Text.Json.Serialization;

public enum SiteType
{
    Static,
    Dynamic,
    API
}



public abstract class Site
{
    protected HttpClient _httpClient;
    public Site(HttpClient client, SiteData? siteData = null) { _httpClient = client; SiteData = siteData; }
    public SiteData? SiteData { get; protected set; }
    public abstract Task<bool> LoadPage(Uri page, CancellationToken cancellationToken = default);
    public abstract ScrapedManga? GetMangaInfo();
    public abstract List<ScrapedMangaChapter>? GetMangaChapters();
    public abstract ScrapedMangaChapterContent? GetMangaChapterContents();


}

public abstract class SiteData
{

    [JsonConverter(typeof(EnumStringConverter<SiteType>))]
    public SiteType SiteType { get; set; }
    public string RootUrl { get; set; } = string.Empty;
    public string ChapterRootUrl { get; set; } = string.Empty;
    public QueryData QueryData { get; set; } = new QueryData();

}
