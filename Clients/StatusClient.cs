

using MangaRead.Crawler.DTOs.Status;

public class StatusClient : Client<StatusDTO, StatusCreateDTO>
{
    public StatusClient(HttpClient client) : base(client)
    {
        CREATE = StatusClientSettingsService.CreateEndpoint;
        GET = StatusClientSettingsService.GetEndpoint;
    }

}