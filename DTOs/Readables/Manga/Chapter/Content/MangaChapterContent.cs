using MangaRead.Crawler.DTOs.System.Image;

namespace MangaRead.Crawler.DTOs.Readables.Manga.Chapter.Content;
public record MangaChapterContentDTO
(
    Guid Id,
    ImageDTO Image
);
public record MangaChapterContentCreateDTO
(
    Guid ChapterId,
    ImageDTO Image
);
public record MangaChapterContentUpdateDTO
(
    Guid? ChapterId,
    int? Order,
    ImageDTO? Image
);