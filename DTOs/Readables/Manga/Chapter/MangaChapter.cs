using MangaRead.Crawler.DTOs.Readables.Manga.Chapter.Content;

namespace MangaRead.Crawler.DTOs.Readables.Manga.Chapter;
public record MangaChapterDTO
(
    Guid Id,
    string Title,
    string Slug,
    bool IsPublished,
    string? MetaTitle,
    string? MetaDescription,
    Guid? PreviousChapterId,
    Guid? NextChapterId,
    MangaChapterContentDTO[] Content,
    Guid MangaId
);

public record SimpleMangaChapterDTO
(
    Guid Id,
    string Title,
    string Slug,
    bool IsPublished,
    Guid MangaId
);

public record MangaChapterCreateDTO
(
    string Title,
    Guid MangaId,
    string? MetaTitle,
    string? MetaDescription
);

public record MangaChapterUpdateDTO
(
    string? Title,
    string? MetaTitle,
    string? MetaDescription,
    Guid? PreviousChapterId,
    Guid? NextChapterId,
    MangaChapterContentDTO[]? Content,
    Guid? MangaId

);