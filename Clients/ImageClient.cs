
using MangaRead.Crawler.DTOs.System.Image;

public class ImageClient : Client<ImageDTO, ImageCreateDTO>
{
    public ImageClient(HttpClient client) : base(client)
    {
        CREATE = ImageClientSettingsService.CreateEndpoint;
        GET = ImageClientSettingsService.GetEndpoint;
    }


    public override async Task<ImageDTO?> GetBy(string getIdentifier, CancellationToken cancellationToken)
    {
        var imageSearch = new ImageSearchDTO(getIdentifier);
        var images = await PostRequestAsync<List<ImageDTO>, ImageSearchDTO>(GET, imageSearch);
        return images?.FirstOrDefault();
    }

}