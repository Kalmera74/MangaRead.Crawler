using MangaRead.Crawler.DTOs.Badge;

namespace MangaRead.Crawler.DTOs.Readables.WebNovel;
public record WebNovelDTO(
    Guid Id,
    string Title,
    string Description,
    string CoverImage,
    WebNovelNavigationItem[] Genres,
    WebNovelNavigationItem Status,
    WebNovelNavigationItem[] Author,
    string Slug,
    BadgeDTO[] Badges
);

public record WebNovelCreateDTO(
    Guid[] Authors,
    Guid[] Genres,
    Guid Status,
    Guid CoverImage,
    Guid? Manga,
    string Title,
    string Description,
    string[] Chapters
);

public record WebNovelUpdateDTO(
    string? Title,
    string? Description,
    Guid? CoverImage,
    Guid? Manga,
    Guid[]? Authors,
    Guid[]? Genres,
    Guid? Status
);

public record WebNovelFilterDTO(
    Guid[]? Genres,
    Guid[]? Statuses
);

public record WebNovelSearchDTO(
    string Title
);

public record WebNovelNavigationItem(
    Guid Id,
    string Name
);