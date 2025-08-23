


public class APIMangaSite : Site
{
    public APIMangaSite(HttpClient client, SiteData? siteData = null) : base(client, siteData)
    {
    }

    public override ScrapedMangaChapterContent? GetMangaChapterContents()
    {
        throw new NotImplementedException();
    }

    public override List<ScrapedMangaChapter>? GetMangaChapters()
    {
        throw new NotImplementedException();
    }

    public override ScrapedManga? GetMangaInfo()
    {
        throw new NotImplementedException();
    }

    public override Task<bool> LoadPage(Uri page, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}