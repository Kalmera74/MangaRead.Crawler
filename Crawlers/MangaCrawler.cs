using System.Collections.Concurrent;
using MangaRead.Crawler.DTOs.Author;
using MangaRead.Crawler.DTOs.Genre;
using MangaRead.Crawler.DTOs.Readables.Manga;
using MangaRead.Crawler.DTOs.Readables.Manga.Chapter;
using MangaRead.Crawler.DTOs.Readables.Manga.Chapter.Content;
using MangaRead.Crawler.DTOs.Readables.Manga.Type;
using MangaRead.Crawler.DTOs.Status;
using MangaRead.Crawler.DTOs.System.File;
using MangaRead.Crawler.DTOs.System.Image;
using Microsoft.Extensions.Caching.Memory;
using Polly;
using Polly.Retry;
using Serilog;

public class MangaCrawler : Crawler
{



    private readonly MangaClient _mangaClient;
    private readonly MangaChapterClient _mangaChapterClient;
    private readonly MangaChapterContentClient _mangaChapterContentClient;
    private readonly GenreClient _genreClient;
    private readonly MangaTypeClient _mangaTypeClient;
    private readonly StatusClient _statusClient;
    private readonly AuthorClient _authorClient;
    private readonly ImageClient _imageClient;
    private readonly FileClient _fileClient;

    private string? _typeFromGenre = null;
    private MangaSiteData _siteData;

    private IMemoryCache _genreCache;
    private IMemoryCache _mangaTypeCache;
    private IMemoryCache _statusCache;

    private readonly static ResiliencePipeline _resilience = new ResiliencePipelineBuilder()
    .AddRetry(
        new RetryStrategyOptions()
        {
            ShouldHandle = new PredicateBuilder().Handle<TimeoutException>(),
            Delay = TimeSpan.FromSeconds(2),
            MaxRetryAttempts = 2,
            BackoffType = DelayBackoffType.Exponential,
            UseJitter = true
        }
        )
        .AddTimeout(TimeSpan.FromSeconds(90))
        .Build();


    public MangaCrawler(MangaSiteData siteData, HttpClient httpClient, IMemoryCache genreCache, IMemoryCache mangaTypeCache, IMemoryCache statusCache)
    {

        _mangaClient = new(httpClient);
        _mangaChapterClient = new(httpClient);
        _mangaChapterContentClient = new(httpClient);
        _genreClient = new(httpClient);
        _mangaTypeClient = new(httpClient);
        _statusClient = new(httpClient);
        _authorClient = new(httpClient);
        _imageClient = new(httpClient);
        _fileClient = new(httpClient);

        _genreCache = genreCache;
        _mangaTypeCache = mangaTypeCache;
        _statusCache = statusCache;

        _siteData = siteData;
    }

    public override async Task Crawl(CancellationToken cancellationToken)
    {
        Log.Information($"Starting Crawling...");

        var mangaPages = _siteData.MangaPages;
        var concurrentLimit = CrawlSettingsService.ConcurrentMangasPerSite;
        var semaphore = new SemaphoreSlim(concurrentLimit);

        List<Task> _tasks = new List<Task>();

        foreach (var mangaPage in mangaPages)
        {
            _tasks.Add(CreateSemaphoreTask(mangaPage, semaphore, cancellationToken));
        }


        await Task.WhenAll(_tasks).ConfigureAwait(false);
    }

    private async Task CreateSemaphoreTask(MangaPage mangaPage, SemaphoreSlim semaphore, CancellationToken cancellationToken)
    {
        await semaphore.WaitAsync(cancellationToken);
        try
        {
            var site = MangaSiteFactory.CreateSite(_siteData);
            await CrawlManga(site, mangaPage);
        }
        catch (Exception ex)
        {
            Log.Error(ex, $"Error crawling manga page: {mangaPage.Url}");
        }
        finally
        {
            semaphore.Release();
        }
    }

    private async Task CrawlManga(Site site, MangaPage mangaPage)
    {
        var uri = new Uri(mangaPage.Url);

        var mangaPageLoadResponse = await _resilience.ExecuteAsync(async cancellationToken => await site.LoadPage(uri, cancellationToken).ConfigureAwait(false)).ConfigureAwait(false);

        if (!mangaPageLoadResponse)
        {
            Log.Error($"Could not load manga: {mangaPage.Url}, skipping manga");
            return;
        }

        var mangaData = site.GetMangaInfo();

        if (mangaData == null)
        {
            Log.Error($"Could not get manga: {mangaPage.Url}, skipping manga");
            return;
        }


        var chapters = site.GetMangaChapters();

        if (chapters == null)
        {
            Log.Error($"Could not get chapters for manga: {mangaData.Title}, skipping manga...");
            return;
        }


        Log.Information($"Getting/Creating manga: {mangaData.Title}");

        var coverImageSubFolder = mangaPage.SaveImages ? mangaData.Title.ToSlug() : null;
        var manga = await GetOrCreateManga(mangaData, mangaPage, coverImageSubFolder).ConfigureAwait(false);

        if (manga == null)
        {

            return;
        }

        await CrawlChapters(site, mangaPage, mangaData, chapters, manga).ConfigureAwait(false);
    }

    private async Task CrawlChapters(Site site, MangaPage mangaPage, ScrapedManga mangaData, List<ScrapedMangaChapter> chapters, MangaDTO manga)
    {

        var existingChaptersSet = new HashSet<string>(manga.Chapters?.Where(c => !string.IsNullOrEmpty(c.Title)).Select(c => c.Title.ToSlug()) ?? Enumerable.Empty<string>());
        var missingChapters = mangaPage.FullUpdate ? chapters : chapters.Where(c => !string.IsNullOrEmpty(c.Title) && !existingChaptersSet.Contains(c.Title.ToSlug()));


        if (!missingChapters.Any())
        {
            Log.Information($"Manga: {mangaData.Title} has no new chapters, skipping...");
            return;
        }

        Log.Information($"Manga: {mangaData.Title} has {missingChapters.Count()} missing chapters, getting new chapters...");


        foreach (var chapter in missingChapters)
        {

            Log.Information($"Loading chapter: {chapter.Title} for manga: {mangaData.Title}...");

            var chapterUri = new Uri(chapter.Url);

            var chapterPageLoadResponse = await _resilience.ExecuteAsync(async cancellationToken => await site.LoadPage(chapterUri, cancellationToken).ConfigureAwait(false)).ConfigureAwait(false);

            if (!chapterPageLoadResponse)
            {
                Log.Error($"Could not load chapter: {chapter.Title} for manga: {mangaData.Title}, skipping manga: {mangaData.Title}");
                break;
            }


            Log.Information($"Getting chapter content for chapter: {chapter.Title} for manga: {mangaData.Title}...");
            var chapterData = site.GetMangaChapterContents();

            if (chapterData == null)
            {
                Log.Error($"Could not get chapter contents for manga: {mangaData.Title}, skipping manga: {mangaData.Title}");
                break;
            }

            Log.Information($"Got chapter content for chapter: {chapter.Title} for manga: {mangaData.Title}");
            Log.Information($"Creating/Getting chapter: {chapter.Title} for manga: {mangaData.Title}...");
            var mangaChapter = await CreateOrGetMangaChapter(manga, chapter).ConfigureAwait(false);

            if (mangaChapter == null)
            {
                Log.Error($"Could not create/get chapter: {chapter.Title} for manga: {mangaData.Title}, skipping manga: {mangaData.Title}");
                break;
            }

            Log.Information($"Chapter: {chapter.Title} created/retrieved successfully");
            Log.Information($"Creating/Updating chapter content for chapter: {chapter.Title} for manga: {mangaData.Title}...");

            var subFolder = mangaPage.SaveImages ? mangaChapter.Slug : null;
            var mangaChapterContent = await CreateOrUpdateMangaChapterContent(mangaChapter, chapterData, mangaPage.SaveImages, subFolder).ConfigureAwait(false);

            if (mangaChapterContent == null)
            {
                Log.Error($"Could not create/update chapter content for chapter: {chapter.Title} for manga: {mangaData.Title}, skipping manga: {mangaData.Title}");
                break;
            }
            Log.Information($"Chapter content for chapter: {chapter.Title} created/updated successfully");

            await Task.Delay(CrawlSettingsService.CrawlDelayPerChapterInMs).ConfigureAwait(false);
        }
    }

    private async Task<List<MangaChapterContentDTO>?> CreateOrUpdateMangaChapterContent(MangaChapterDTO chapter, ScrapedMangaChapterContent chapterContent, bool saveImages = false, string? subFolder = null)
    {
        var images = await GetOrCreateImages(chapterContent.Images, saveImages, subFolder);

        if (images == null)
        {
            Log.Error($"Could not get images for content for chapter: {chapter.Title}");
            return null;
        }

        if (chapter.Content.Length > 0 && chapter.Content.Length == images.Count)
        {
            // * Update the existing content
            return await UpdateMangaChapterContent(chapter, images).ConfigureAwait(false);
        }

        // * Content is empty or different, create new content
        return await CreateMangaChapterContent(chapter, images).ConfigureAwait(false);
    }

    private async Task<List<MangaChapterContentDTO>?> UpdateMangaChapterContent(MangaChapterDTO chapter, List<ImageDTO> images)
    {
        List<MangaChapterContentDTO> chapterContent = new();
        for (int i = 0; i < chapter.Content.Length; i++)
        {
            var content = chapter.Content[i];
            var image = images[i];

            var contentToUpdate = new MangaChapterContentUpdateDTO
            (
                content.Id,
                null,
                image
            );

            var updatedContent = await _resilience.ExecuteAsync(async cancellationToken => await _mangaChapterContentClient.UpdateChapterContent(content.Id, contentToUpdate, cancellationToken).ConfigureAwait(false)).ConfigureAwait(false);
            if (updatedContent == null)
            {
                Log.Error($"Could not update content for chapter: {chapter.Title}");
                return null;
            }
            chapterContent.Add(updatedContent);
        }
        return chapterContent;
    }

    private async Task<List<MangaChapterContentDTO>?> CreateMangaChapterContent(MangaChapterDTO chapter, List<ImageDTO> images)
    {

        List<MangaChapterContentDTO> chapterContent = new();
        for (int i = 0; i < images.Count; i++)
        {
            var image = images[i];
            var content = new MangaChapterContentCreateDTO
            (
                chapter.Id,
                image
            );

            var createdContent = await _resilience.ExecuteAsync(async cancellationToken => await _mangaChapterContentClient.Create(content, cancellationToken).ConfigureAwait(false)).ConfigureAwait(false);
            if (createdContent == null)
            {
                Log.Error($"Could not create content for chapter: {chapter.Title}");
                return null;
            }
            chapterContent.Add(createdContent);
        }

        return chapterContent;
    }

    private async Task<List<ImageDTO>?> GetOrCreateImages(string[] imageUrls, bool saveImages = false, string? subFolder = null)
    {

        List<ImageDTO> images = new();
        foreach (var imageUrl in imageUrls)
        {
            string url = imageUrl;

            if (saveImages)
            {
                var downloadFile = new FileDownloadDTO(url, subFolder, null, null, FileType.Image);

                var savedFile = await _resilience.ExecuteAsync(async cancellationToken => await _fileClient.DownloadFile(downloadFile, cancellationToken).ConfigureAwait(false)).ConfigureAwait(false);

                if (savedFile == null)
                {
                    Log.Error($"Could not download image: {imageUrl}");
                    Log.Warning($"Saving the URL : {imageUrl} of it instead as an ImageDTO");
                }
                else
                {
                    url = savedFile.FilePath;
                }
            }

            Log.Information($"Getting/Creating image: {url}...");
            var newImage = new ImageCreateDTO(url);
            var image = await _resilience.ExecuteAsync(async cancellationToken => await _imageClient.Create(newImage, cancellationToken).ConfigureAwait(false)).ConfigureAwait(false);

            if (image == null)
            {
                Log.Error($"Could not get/create image: {url}, skipping the rest of the images");
                return null;
            }

            images.Add(image);
        }

        return images;
    }

    private async Task<MangaChapterDTO?> CreateOrGetMangaChapter(MangaDTO manga, ScrapedMangaChapter chapter)
    {
        var metaTitle = $"{manga.Title} {chapter.Title}";
        var metaDescription = $"Read {manga.Title} {chapter.Title} online for free";

        var chapterToCreate = new MangaChapterCreateDTO(chapter.Title, manga.Id, metaTitle, metaDescription);

        Log.Information($"Getting/Creating chapter: {chapter.Title}...");
        var createdChapter = await _resilience.ExecuteAsync(async cancellationToken => await _mangaChapterClient.Create(chapterToCreate, cancellationToken).ConfigureAwait(false)).ConfigureAwait(false);

        if (createdChapter == null)
        {
            Log.Error($"Could not get/create chapter: {chapter.Title}, skipping rest of chapters for manga: {manga.Title}");
            return null;
        }
        return createdChapter;
    }
    private async Task<MangaDTO?> GetOrCreateManga(ScrapedManga mangaData, MangaPage? mangaPage, string? subFolder = null)
    {
        List<GenreDTO>? genres = null;
        MangaTypeDTO? mangaType = null;
        StatusDTO? status = null;
        List<AuthorDTO>? authors = null;
        ImageDTO? coverImage = null;

        genres = await GetOrCreateGenres(mangaData).ConfigureAwait(false);
        if (genres == null)
        {
            return null;
        }

        string? overrideType;
        if (mangaPage?.OverrideType != null)
        {
            overrideType = (string?)mangaPage.OverrideType;
        }
        else
        {
            overrideType = mangaData.Type.Equals("N/A") && !string.IsNullOrEmpty(_typeFromGenre) ? _typeFromGenre : null;
        }

        mangaType = await GetOrCreateMangaType(mangaData, overrideType).ConfigureAwait(false);
        if (mangaType == null)
        {
            return null;
        }

        status = await GetOrCreateStatus(mangaData).ConfigureAwait(false);
        if (status == null)
        {
            return null;
        }

        authors = await GetOrCreateAuthors(mangaData).ConfigureAwait(false);
        if (authors == null)
        {
            return null;
        }

        var cover = new string[] { mangaData.Cover };

        bool saveImages = mangaPage?.SaveImages ?? false;
        List<ImageDTO>? imageDTOs = await GetOrCreateImages(cover, saveImages, subFolder).ConfigureAwait(false);
        coverImage = imageDTOs?.FirstOrDefault();

        if (coverImage == null)
        {
            return null;
        }

        var genreIds = genres.Select(x => x.Id).ToArray();
        var authorIds = authors.Select(x => x.Id).ToArray();

        var newManga = new MangaCreateDTO
        (
            mangaData.Title,
            mangaData.Description,
            coverImage.Id,
            genreIds,
            status.Id,
            mangaType.Id,
            authorIds,
            null
        );

        Log.Information($"Getting/Creating manga: {mangaData.Title}...");
        var manga = await _resilience.ExecuteAsync(async cancellationToken => await _mangaClient.Create(newManga, cancellationToken).ConfigureAwait(false)).ConfigureAwait(false);

        if (manga == null)
        {
            Log.Error($"Could not get/create create manga: {mangaData.Title}, skipping manga");
            return null;
        }
        Log.Information($"Got/Created manga: {mangaData.Title}");
        return manga;
    }

    private async Task<List<AuthorDTO>?> GetOrCreateAuthors(ScrapedManga mangaData)
    {
        List<AuthorDTO>? authors = new();
        foreach (var authorName in mangaData.Authors)
        {

            Log.Information($"Getting/Creating author: {authorName}...");

            var newAuthor = new AuthorCreateDTO(authorName);
            var author = await _resilience.ExecuteAsync(async cancellationToken => await _authorClient.Create(newAuthor, cancellationToken).ConfigureAwait(false)).ConfigureAwait(false);

            if (author == null)
            {
                Log.Error($"Could not get/create author: {authorName}, skipping author");
                return null;
            }
            authors.Add(author);

        }

        return authors;
    }

    private async Task<StatusDTO?> GetOrCreateStatus(ScrapedManga mangaData)
    {

        Log.Information($"Getting/Creating status: {mangaData.Status}...");

        if (_statusCache.TryGetValue(mangaData.Status, out StatusDTO? cachedStatus))
        {
            Log.Information($"Got cached status: {mangaData.Status}");
            return cachedStatus;
        }

        var newStatus = new StatusCreateDTO(mangaData.Status);
        var status = await _resilience.ExecuteAsync(async cancellationToken => await _statusClient.Create(newStatus, cancellationToken).ConfigureAwait(false)).ConfigureAwait(false);

        if (status == null)
        {
            Log.Error($"Could not get/create status: {mangaData.Status}, skipping manga: {mangaData.Title}");
            return null;
        }

        _statusCache.Set(mangaData.Status, status);

        return status;
    }

    private async Task<MangaTypeDTO?> GetOrCreateMangaType(ScrapedManga mangaData, string? overrideType = null)
    {
        var mangaTypeName = mangaData.Type;

        if (!string.IsNullOrEmpty(overrideType))
        {
            mangaTypeName = overrideType;
        }

        if (_mangaTypeCache.TryGetValue(mangaTypeName, out MangaTypeDTO? cachedMangaType))
        {
            Log.Information($"Got cached manga type: {mangaTypeName}");
            return cachedMangaType;
        }

        Log.Information($"Getting/Creating manga type: {mangaTypeName}...");

        var newMangaType = new MangaTypeCreateDTO(mangaTypeName);
        var mangaType = await _resilience.ExecuteAsync(async cancellationToken => await _mangaTypeClient.Create(newMangaType, cancellationToken).ConfigureAwait(false)).ConfigureAwait(false);

        if (mangaType == null)
        {
            Log.Error($"Could not get/create manga type: {mangaData.Type}, skipping manga: {mangaData.Title}");
            return null;
        }

        _mangaTypeCache.Set(mangaTypeName, mangaType);

        return mangaType;

    }

    private async Task<List<GenreDTO>?> GetOrCreateGenres(ScrapedManga mangaData)
    {
        var genres = new ConcurrentBag<GenreDTO>();

        var tasks = mangaData.Genres.Select(async genreName =>
        {
            var genre = await GetGenreTask(mangaData, genreName).ConfigureAwait(false);
            if (genre != null)
            {
                genres.Add(genre);
            }
        });

        await Task.WhenAll(tasks).ConfigureAwait(false);

        return genres.ToList();
    }

    private async Task<GenreDTO?> GetGenreTask(ScrapedManga mangaData, string genreName)
    {
        if (_mangaTypeCache.TryGetValue(genreName, out MangaTypeDTO? cachedMangaType))
        {
            _typeFromGenre = cachedMangaType?.Name;
            return null;
        }

        if (_genreCache.TryGetValue(genreName, out GenreDTO? cachedGenre))
        {
            Log.Information($"Got cached genre: {genreName}");
            return cachedGenre;
        }

        var isGenreType = await _resilience.ExecuteAsync(async cancellationToken => await _mangaTypeClient.GetBy(genreName.ToSlug(), cancellationToken).ConfigureAwait(false)).ConfigureAwait(false);

        if (isGenreType != null)
        {
            _typeFromGenre = isGenreType.Name;
            _mangaTypeCache.Set(genreName, isGenreType);
            return null;
        }

        Log.Information($"Getting/Creating genre: {genreName}...");

        var newGenre = new GenreCreateDTO(genreName);
        var genre = await _resilience.ExecuteAsync(async cancellationToken => await _genreClient.Create(newGenre, cancellationToken).ConfigureAwait(false)).ConfigureAwait(false);

        if (genre == null)
        {
            Log.Error($"Could not get/create genre: {genreName}, skipping manga: {mangaData.Title}");
            return null;
        }

        _genreCache.Set(genreName, genre);
        return genre;
    }
}

