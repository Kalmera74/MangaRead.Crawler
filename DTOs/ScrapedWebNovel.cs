public record ScrapedWebNovel(
    string Title,
    string Description,
    string Cover,
    string Status,
    string Rating,
    string[] Genres,
    string[] Authors

);

public record ScrapedWebNovelChapter
(
    string Title,
    string Url

);

public record ScrapedWebNovelChapterContent
(
    string Title,
    string Content
);