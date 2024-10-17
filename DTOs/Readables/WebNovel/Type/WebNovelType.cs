namespace MangaRead.Crawler.DTOs.Readables.WebNovel.Type;
public record WebNovelTypeDTO(
    Guid Id,
    string Name,
    string Slug

);
public record WebNovelTypeSearchDTO(
    string Name
);

public record WebNovelTypeCreateDTO(
    string Name
);

public record WebNovelTypeUpdateDTO(
    string Name


);
