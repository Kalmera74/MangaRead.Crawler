
using MangaRead.Crawler.DTOs.Genre;

public class GenreClient : Client<GenreDTO, GenreCreateDTO>
{


    public GenreClient(HttpClient client) : base(client)
    {
        CREATE = GenreClientSettingsService.CreateEndpoint;
        GET = GenreClientSettingsService.GetEndpoint;
    }


}