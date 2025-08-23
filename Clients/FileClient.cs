
using MangaRead.Crawler.DTOs.System.File;

public class FileClient : Client<FileDTO, FileDownloadDTO>
{
    public FileClient(HttpClient client) : base(client)
    {
        CREATE = FileClientSettingsService.CreateEndpoint;

    }

    public async Task<FileDTO?> DownloadFile(FileDownloadDTO file, CancellationToken cancellationToken = default)
    {
        return await Create(file, cancellationToken);
    }


}