namespace MangaRead.Crawler.DTOs.Genre;
public record GenreDTO
(
    Guid Id,
    string Name,
    string Slug
);
public record GenreCreateDTO
(
    string Name
);
public record GenreUpdateDTO
(
    string? Name
);
public record GenreSearchDTO
(
    string Name
);