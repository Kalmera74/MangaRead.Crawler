public record ScrapedManga(
    string Title,
    string Description,
    string Cover,
    string Status,
    string Type,
    string Rating,
    string[] Genres,
    string[] Authors

);

public record ScrapedMangaChapter
(
    string Title,
    string Url

);

public record ScrapedMangaChapterContent
(
    string[] Images
);