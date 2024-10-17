
using MangaRead.Crawler.DTOs.Readables.Manga;

public class MangaClient : Client<MangaDTO, MangaCreateDTO>
{
    public MangaClient(HttpClient client) : base(client)
    {
        CREATE = MangaClientSettingsService.CreateEndpoint;
        GET = MangaClientSettingsService.GetEndpoint;
    }


}