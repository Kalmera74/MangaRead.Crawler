namespace MangaRead.Crawler.DTOs.Status;
public record StatusDTO(
    Guid Id,
    string Name
);
public record StatusCreateDTO(
    string Name
);
public record StatusUpdateDTO(
    string? Name
);
public record StatusSearchDTO(
    string Name
);
