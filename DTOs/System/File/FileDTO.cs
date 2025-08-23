

using Microsoft.AspNetCore.Http;

namespace MangaRead.Crawler.DTOs.System.File;

public enum FileType
{
    Image,
    Video,
    Text,
    Other
}



public record FileDTO(
    string FilePath
);

public record FileCreateDTO(
    IFormFile File,
    string? SubFolder,
    string? Name,
    string? Extension,
    FileType FileType = FileType.Other


);

public record FileDownloadDTO(
    string Url,
    string? SubFolder,
    string? Name,
    string? Extension,
    FileType FileType = FileType.Other
);