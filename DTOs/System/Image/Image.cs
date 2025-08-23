namespace MangaRead.Crawler.DTOs.System.Image;
public record ImageDTO(
    Guid Id,
    string Url
);

public record ImageCreateDTO(
    string Url
);

public record ImageUpdateDTO(
    string Url
);

public record ImageSearchDTO(
    string PartialUrl
);