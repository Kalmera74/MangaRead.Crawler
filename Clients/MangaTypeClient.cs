
using MangaRead.Crawler.DTOs.Readables.Manga.Type;

public class MangaTypeClient : Client<MangaTypeDTO, MangaTypeCreateDTO>
{
    public MangaTypeClient(HttpClient client) : base(client)
    {
        CREATE = MangaTypeClientSettingsService.CreateEndpoint;
        GET = MangaTypeClientSettingsService.GetEndpoint;
    }

}