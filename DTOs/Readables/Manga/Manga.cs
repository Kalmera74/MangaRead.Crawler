using MangaRead.Crawler.DTOs.Badge;
using MangaRead.Crawler.DTOs.Readables.Manga.Chapter;

namespace MangaRead.Crawler.DTOs.Readables.Manga;
public record MangaDTO(
    Guid Id,
    string Title,
    string Description,
    string CoverImage,
    string Slug,
    int ViewCount,
    float Rating,
    int SeasonCount,
    bool IsPublished,
    MangaChapterDTO[]? Chapters,
    BadgeDTO[] Badges,
    MangaNavigationItem[] Genres,
    MangaNavigationItem Status,
    MangaNavigationItem Type,
    MangaNavigationItem[] Authors
    );

public record SimpleMangaDTO(
    Guid Id,
    string Title,
    string Slug,
    string Description,
    string CoverImage,
    float Rating,
    bool IsPublished,
    MangaNavigationItem[] Genres,
    MangaNavigationItem Status,
    MangaNavigationItem Type,
    MangaNavigationItem[] Authors,
    int ViewCount,
    int SeasonCount
);

public record MangaCreateDTO(
    string Title,
    string Description,
    Guid CoverImage,
    Guid[] Genres,
    Guid Status,
    Guid Type,
    Guid[] Authors,
    Guid? WebNovel
);

public record MangaUpdateDTO(
    string? Title,
    string? Description,
    Guid? CoverImage,
    Guid[]? Genres,
    Guid? Status,
    Guid? Type,
    Guid[]? Authors,
    Guid? WebNovel
);

public record MangaSearchDTO(
    string Title
);

public record MangaFilterDTO(
    Guid[]? Genres,
    Guid[]? Types,
    Guid[]? Status
);


public record MangaNavigationItem(
    Guid Id,
    string Name
);

