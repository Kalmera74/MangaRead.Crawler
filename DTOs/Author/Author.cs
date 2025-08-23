namespace MangaRead.Crawler.DTOs.Author;
public record AuthorDTO(
    Guid Id,
    string Name,
    string Slug
);
public record AuthorCreateDTO(
    string Name
);
public record AuthorUpdateDTO(
    string? Name,
    IEnumerable<Guid>? Mangas,
    IEnumerable<Guid>? WebNovels
);
public record AuthorSearchDTO(
    string Name
);
