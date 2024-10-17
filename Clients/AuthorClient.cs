
using MangaRead.Crawler.DTOs.Author;

public class AuthorClient : Client<AuthorDTO, AuthorCreateDTO>
{

    public AuthorClient(HttpClient client) : base(client)
    {
        CREATE = AuthorClientSettings.CreateEndpoint;
        GET = AuthorClientSettings.GetEndpoint;
    }
}