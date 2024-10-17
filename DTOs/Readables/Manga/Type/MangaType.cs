namespace MangaRead.Crawler.DTOs.Readables.Manga.Type;
public record MangaTypeDTO(
    Guid Id,
    string Name,
    string Slug

);
public record MangaTypeSearchDTO(
    string Name
);

public record MangaTypeCreateDTO(
    string Name
);

public record MangaTypeUpdateDTO(
    string Name


);
