using HtmlAgilityPack;
using Polly;
using Polly.Retry;
using Serilog;
using System.Net;
using System.Text.RegularExpressions;

public class StaticMangaSite : Site
{

    private HtmlDocument? _document;


    private readonly static ResiliencePipeline _resilience = new ResiliencePipelineBuilder()
    .AddRetry(
        new RetryStrategyOptions()
        {
            ShouldHandle = new PredicateBuilder().Handle<Exception>(),
            Delay = TimeSpan.FromSeconds(2),
            MaxRetryAttempts = 2,
            BackoffType = DelayBackoffType.Exponential,
            UseJitter = true
        }
        )
        .AddTimeout(TimeSpan.FromSeconds(90))
        .Build();

    public StaticMangaSite(HttpClient client, SiteData? siteData = null) : base(client, siteData)
    {

    }

    public override ScrapedManga? GetMangaInfo()
    {
        if (SiteData == null)
        {
            Log.Error("SiteData is null, aborted scraping manga details");
            return null;
        }

        Log.Information($"Scraping Manga Details");

        string? title = GetDataBySelector(SiteData.QueryData.TitleSelector);
        if (string.IsNullOrEmpty(title))
        {
            Log.Error($"Could not get title from selector: {nameof(SiteData.QueryData.TitleSelector)} for Site: {SiteData.RootUrl}");
            return null;
        }
        title = CleanText(title);

        Log.Information($"Got the title");

        string? description = GetDataBySelector(SiteData.QueryData.DescriptionSelector);
        if (string.IsNullOrEmpty(description))
        {
            Log.Error($"Could not get description from selector: {nameof(SiteData.QueryData.DescriptionSelector)} for Site: {SiteData.RootUrl}");
            return null;
        }
        description = CleanText(description);
        Log.Information($"Got the description");

        string? cover = GetDataBySelector(SiteData.QueryData.CoverSelector);
        if (string.IsNullOrEmpty(cover))
        {
            Log.Error($"Could not get cover from selector: {nameof(SiteData.QueryData.CoverSelector)} for Site: {SiteData.RootUrl}");
            return null;
        }
        Log.Information($"Got the cover");

        string? status = GetDataBySelector(SiteData.QueryData.StatusSelector);
        if (string.IsNullOrEmpty(status))
        {
            Log.Error($"Could not get status from selector: {nameof(SiteData.QueryData.StatusSelector)} for Site: {SiteData.RootUrl}");
            return null;
        }
        status = CleanText(status);
        Log.Information("Got the status");

        string? type = GetDataBySelector(SiteData.QueryData.TypeSelector);
        if (string.IsNullOrEmpty(type))
        {
            Log.Warning($"Could not get type from selector: {nameof(SiteData.QueryData.TypeSelector)} for Site: {SiteData.RootUrl}, assigning default type...");
            type = "N/A";
        }
        type = CleanText(type);
        Log.Information("Got the type");

        string? author = GetDataBySelector(SiteData.QueryData.AuthorSelector);
        if (string.IsNullOrEmpty(author))
        {
            Log.Error($"Could not get author from selector: {nameof(SiteData.QueryData.AuthorSelector)} for Site: {SiteData.RootUrl}");
            return null;
        }
        author = CleanText(author);
        Log.Information("Got the author");

        string? rating = GetDataBySelector(SiteData.QueryData.RatingSelector);
        if (string.IsNullOrEmpty(rating))
        {
            Log.Warning($"Could not get rating from selector: {nameof(SiteData.QueryData.RatingSelector)} for Site: {SiteData.RootUrl}, Assigning random rating...");
            rating = Random.Shared.Next(1, 5).ToString();

        }
        Log.Information("Got the rating");

        string? genres = GetDataBySelector(SiteData.QueryData.GenreSelector);
        if (string.IsNullOrEmpty(genres))
        {
            Log.Error($"Could not get genres from selector: {nameof(SiteData.QueryData.GenreSelector)} for Site: {SiteData.RootUrl}");
            return null;
        }
        genres = CleanText(genres);
        Log.Information("Got the genres");

        var scrapedManga = new ScrapedManga
        (
            title,
            description,
            cover,
            status,
            type,
            rating,
            genres.Split(',').ToArray(),
            [author]
        );

        Log.Information($"Got all the manga details...");
        return scrapedManga;
    }

    private static string CleanText(string? text)
    {
        if (text == null) { return string.Empty; }

        text = text.Replace("/n", "").Replace("/t", "").Trim();
        text = WebUtility.HtmlDecode(text);

        return text;
    }

    public override List<ScrapedMangaChapter>? GetMangaChapters()
    {
        if (SiteData == null)
        {
            Log.Error("SiteData is null, aborted scraping manga chapters");
            return null;
        }

        var chapterTitles = GetDataBySelector(SiteData.QueryData.ChapterTitlesSelector);
        if (string.IsNullOrEmpty(chapterTitles))
        {
            Log.Error($"Could not get chapter titles from selector: {nameof(SiteData.QueryData.ChapterTitlesSelector)}");
            return null;
        }
        Log.Information($"Got the chapter titles");

        var chapterLinks = GetDataBySelector(SiteData.QueryData.ChapterLinksSelector);
        if (string.IsNullOrEmpty(chapterLinks))
        {
            Log.Error($"Could not get chapter urls from selector: {nameof(SiteData.QueryData.ChapterLinksSelector)}");
            return null;
        }
        Log.Information($"Got the chapter urls");

        var chapters = new List<ScrapedMangaChapter>();
        var chapterLinksList = chapterLinks.Split(',').ToList();
        var chapterTitlesList = chapterTitles.Split(',').ToList();
        for (int i = 0; i < chapterLinksList.Count; i++)
        {
            var rawLink = chapterLinksList[i];
            if (!Uri.IsWellFormedUriString(rawLink, UriKind.Absolute))
            {
                var chapterRootUrl = SiteData.ChapterRootUrl;
                if (!chapterRootUrl.EndsWith('/'))
                {
                    chapterRootUrl += '/';
                }
                var rootUri = new Uri(chapterRootUrl, UriKind.Absolute);
                rawLink = new Uri(rootUri, rawLink).ToString();
            }
            chapters.Add
            (
                new ScrapedMangaChapter
                (
                    chapterTitlesList[i],
                    rawLink
                )
            );
        }

        var first = chapters.First().Title.ToLower();
        var last = chapters.Last().Title.ToLower();

        first = first.Replace("chapter", "").Trim().Replace(".", ",");
        last = last.Replace("chapter", "").Trim().Replace(".", ",");

        if (last.Length > 1 || last.Length == 0)
        {
            last = chapters[^2].Title.ToLower().Replace("chapter", "").Trim().Replace(".", ",");
        }

        float.TryParse(first, out float parsedFirst);
        float.TryParse(last, out float parsedLast);

        if (parsedFirst > parsedLast)
        {
            chapters.Reverse();
        }


        Log.Information($"Got all the chapter details...");
        return chapters;
    }

    public override ScrapedMangaChapterContent? GetMangaChapterContents()
    {
        if (SiteData == null)
        {
            Log.Error("SiteData is null, aborted scraping manga chapters");
            return null;
        }

        var images = GetDataBySelector(SiteData.QueryData.ChapterContentsSelector);
        if (string.IsNullOrEmpty(images))
        {
            Log.Error($"Could not get images from selector: {nameof(SiteData.QueryData.ChapterContentsSelector)}");
            return null;
        }

        var mangaChapterContent = new ScrapedMangaChapterContent
            (
                images.Split(',').ToArray()
            );

        Log.Information($"Got the images");
        return mangaChapterContent;
    }

    public override async Task<bool> LoadPage(Uri page, CancellationToken cancellationToken = default)
    {
        Log.Information($"Loading Page: {page}");

        var response = await _resilience.ExecuteAsync(async cancellationToken => await _httpClient.GetAsync(page, cancellationToken).ConfigureAwait(false)).ConfigureAwait(false);
        if (!response.IsSuccessStatusCode)
        {
            Log.Error($"Could not load page: {page}. Reason: {response.ReasonPhrase}");
            return false;
        }

        _document = new HtmlDocument();
        _document.Load(await response.Content.ReadAsStreamAsync().ConfigureAwait(false));

        Log.Information($"Loaded Page.");

        return true;
    }
    private string? GetDataBySelector(Selector[] selectors)
    {
        if (_document == null)
        {
            Log.Error("Document is null, aborted scraping");
            return null;
        }

        if (selectors == null || selectors.Length == 0)
        {
            Log.Error("No selectors provided, aborted scraping");
            return null;
        }

        try
        {

            foreach (var selector in selectors)
            {
                if (string.IsNullOrEmpty(selector.Query)) { continue; }

                var nodes = _document.DocumentNode.SelectNodes(selector.Query);
                if (nodes != null && nodes.Count != 0)
                {
                    if (selector.Type == SelectorType.Single)
                    {
                        return GetValueFromNode(nodes.First(), selector.DataFrom);
                    }

                    var attributeValues = nodes.Select(x => GetValueFromNode(x, selector.DataFrom))
                    .Where(value => !string.IsNullOrEmpty(value))
                    .ToList();
                    if (attributeValues.Count != 0)
                    {
                        return string.Join(", ", attributeValues);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Log.Error($"Unexpected error occurred while evaluating XPath selector, Reason: {ex.Message}");
            return null;
        }

        return null;
    }

    private string GetValueFromNode(HtmlNode htmlNode, DataAttribute dataFrom)
    {
        return dataFrom switch
        {
            DataAttribute.InnerText => htmlNode.InnerText,
            DataAttribute.InnerHtml => htmlNode.InnerHtml,
            DataAttribute.BackgroundImage => GetRegexUrl(htmlNode.GetAttributeValue("style", "")),
            DataAttribute.DataBackground => htmlNode.GetAttributeValue("data-background", ""),
            DataAttribute.DataSrc => htmlNode.GetAttributeValue("data-src", ""),
            _ => htmlNode.GetAttributeValue(dataFrom.ToString(), "")

        };
    }

    private string GetRegexUrl(string v)
    {
        var regex = new Regex(@"url\((.*?)\)");
        var match = regex.Match(v);
        if (match.Success)
        {
            return match.Groups[1].Value;
        }
        return string.Empty;
    }
}
