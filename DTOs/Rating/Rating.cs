namespace MangaRead.Crawler.DTOs.Rating;
public record RatingDTO(
    Guid Id,
    Guid UserId,
    float StartCount
);
public record RatingCreateDTO(
    float StartCount,
    Guid UserId
);
public record RatingUpdateDTO(
    float StartCount
);

public record RatingSearchDTO(
    float MinStarCount,
    float MaxStarCount
);