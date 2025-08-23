namespace MangaRead.Crawler.DTOs.Readables.WebNovel.Chapter.Content;
public record WebNovelChapterContentDTO
(
    Guid Id,
    string Title,
    string? MetaTitle,
    string? MetaDescription,
    string Body);
public record WebNovelChapterContentCreateDTO
(
    Guid ChapterId,
    string Title,
    string? MetaTitle,
    string? MetaDescription,
    string Body);
public record WebNovelChapterContentUpdateDTO
(
    Guid? ChapterId,
    string? Title,
    string? MetaTitle,
    string? MetaDescription,
    string? Body
);