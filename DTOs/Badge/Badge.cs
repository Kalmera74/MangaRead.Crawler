namespace MangaRead.Crawler.DTOs.Badge;
public enum BadgeType
{
    New,
    Hot
}


public record BadgeDTO(
    string Name
);