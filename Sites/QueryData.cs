using System.Text.Json.Serialization;

public class QueryData
{

    public Selector[] TitleSelector { get; set; } = Array.Empty<Selector>();
    public Selector[] DescriptionSelector { get; set; } = Array.Empty<Selector>();
    public Selector[] CoverSelector { get; set; } = Array.Empty<Selector>();

    public Selector[] StatusSelector { get; set; } = Array.Empty<Selector>();
    public Selector[] TypeSelector { get; set; } = Array.Empty<Selector>();
    public Selector[] AuthorSelector { get; set; } = Array.Empty<Selector>();
    public Selector[] RatingSelector { get; set; } = Array.Empty<Selector>();
    public Selector[] GenreSelector { get; set; } = Array.Empty<Selector>();

    public Selector[] ChapterLinksSelector { get; set; } = Array.Empty<Selector>();
    public Selector[] ChapterTitlesSelector { get; set; } = Array.Empty<Selector>();

    public Selector[] ChapterContentsSelector { get; set; } = Array.Empty<Selector>();


}


public enum SelectorType
{
    None,
    Single,
    Multi,
}

public enum DataAttribute
{
    None,
    InnerHtml,
    InnerText,
    Src,
    DataSrc,
    Srcset,
    Href,
    DataBackground,
    BackgroundImage

}

public class Selector
{
    public string Query { get; set; } = string.Empty;

    [JsonConverter(typeof(EnumStringConverter<SelectorType>))]
    public SelectorType Type { get; set; }

    [JsonConverter(typeof(EnumStringConverter<DataAttribute>))]
    public DataAttribute DataFrom { get; set; }
}