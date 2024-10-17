
using MangaRead.Crawler.DTOs.Readables.Manga.Chapter.Content;

public class MangaChapterContentClient : Client<MangaChapterContentDTO, MangaChapterContentCreateDTO>
{


    public MangaChapterContentClient(HttpClient client) : base(client)
    {
        CREATE = MangaChapterContentClientSettingsService.CreateEndpoint;
        GET = MangaChapterContentClientSettingsService.GetEndpoint;
        UPDATE = MangaChapterContentClientSettingsService.UpdateEndpoint;
    }


    public async Task<MangaChapterContentDTO?> UpdateChapterContent(Guid contentId, MangaChapterContentUpdateDTO updatedMangaChapterContent, CancellationToken cancellationToken = default)
    {

        return await PutRequestAsync($"{UPDATE}{contentId}", updatedMangaChapterContent, cancellationToken);
    }


}