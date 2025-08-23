namespace MangaRead.Crawler.DTOs.Readables.WebNovel.Chapter;

public record WebNovelChapterDTO
(
    Guid Id,
    string Title,
    string Slug,
    Guid? PreviousChapterId,
    Guid? NextChapterId,
    Guid? ContentId,
    Guid WebNovelId
);

public record WebNovelChapterCreateDTO
(
    string Title,
    Guid WebNovelId
);

public record WebNovelChapterUpdateDTO
(
    string? Title,
    Guid? PreviousChapterId,
    Guid? NextChapterId,
    Guid? ContentId,
    Guid? WebNovelId

);