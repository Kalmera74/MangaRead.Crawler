using MangaRead.Crawler.DTOs.Readables.Manga.Chapter;

public class MangaChapterClient : Client<MangaChapterDTO, MangaChapterCreateDTO>
{
    public MangaChapterClient(HttpClient client) : base(client)
    {
        CREATE = MangaChapterClientSettingsService.CreateEndpoint;
        GET = MangaChapterClientSettingsService.GetEndpoint;

    }

}