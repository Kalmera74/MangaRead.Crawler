# MangaRead Crawler

This is a generic crawler for manga sites in development. This is not a finished product and I'll be updating it regularly. This project is being develop in conjunction with [MangaRead.Backend](https://github.com/Kalmera74/MangaRead.Backend). However, It can bu adapted to be used on any back-end service easily. It supports all kinds of sites (static, dynamic etc) as well as WebNovel sites. However, currently only static manga sites are implemented, other sites can be implemented easily

## How To Run Crawler

Crawler reads all the SiteData json files from the config then run each site asynchronously depending on the settings (if the parallel settings are set to 1 it will run sequentially).
To add new sites for crawling, you need to create new site data json and add it to the config file.

To run the project run the following command

```
dotnet run
```

## Deployment

Project includes a build script called **build.sh** it will build, deploy and manage the services for the project for you. It basically does the following

1. **Build and Publish the API**
   The script first cleans any previous builds to avoid conflicts, restores all project dependencies, and compiles the API project. After building, it publishes the compiled output to a specific directory in a format suitable for running in production. This ensures the API is up-to-date and ready to execute.

2. **Database Migrations**
   Before running the API, the script applies any pending database schema changes using the ORM’s migration system. This ensures that the database structure matches what the API expects, preventing runtime errors due to missing tables or columns.

3. **System Service Setup**
   The script creates a configuration file for the system’s service manager. This allows the API to run as a background service that automatically starts on system boot, restarts if it crashes, and logs events for monitoring.

4. **Start and Reload Service**
   Once the service file is created, the script reloads the system manager to recognize the new service, enables it to start on boot, and starts the service immediately. This step ensures the API is running and properly managed by the system.

5. **Command-Line Shortcuts**
   To make managing the service easier, the script adds aliases to the user’s shell. These shortcuts allow you to quickly check the status, start, stop, or restart the API service without typing long commands.

## Configuration

### Manga Site Configuration

Each site data json file define how to scrape the corresponding site to get the required manga information as well as the chapter images. Crawler uses Xpath to query the page.
Note that right now Crawler only supports Static sites.

### sitedata.json

#### SiteType

- **Type**: `enum`
- **Description**: The type of the site being crawled. This can help determine the method used for data extraction (e.g., Static, Dynamic, or API).

#### RootUrl

- **Type**: `URL`
- **Description**: The root of the site for logging purposes

#### ChapterRootUrl

- **Type**: `URL`
- **Description**: THe common root for all the chapter pages

#### PageData

- **Selector**: A selector is comprise of the following attributes
  - **Query**: XPath expression used to find the title element in the HTML.
  - **Type**: `enum` - Indicates if the selector extracts a "Single" or "Multi" result. Multi means that query will return multiple nodes
  - **DataFrom**: `enum` - Specifies where to extract data from the selected element (e.g., InnerHtml, InnerText, Src, Href, DataBackground, BackgroundImage).

This section contains selectors for extracting different types of information from the manga page. Each selector for the PageData field can hold multiple Selectors to cover cases
where some sites serves non-uniform DOM. For example, TitleSelector could have multiple Selector to query the title of the manga.

- **TitleSelector**:

  - **Type**: `List<Selector>`
  - **Description**: Contains the query to extract the title of the manga.

- **DescriptionSelector**:

  - **Type**: `List<Selector>`
  - **Description**: Contains the query to extract the description of the manga.

- **CoverSelector**:

  - **Type**: `List<Selector>`
  - **Description**: Contains the query to extract the cover image of the manga.

- **StatusSelector**:

  - **Type**: `List<Selector>`
  - **Description**: Contains the query to extract the current status of the manga (e.g., ongoing, completed).

- **TypeSelector**:

  - **Type**: `List<Selector>`
  - **Description**: Contains the query to extract the type of the manga (e.g., webtoon, manga). Can be empty. Which then prompts the crawler to assign it `N/A` unless it can extract the type from Genre section (some sites use type as a genre ) or used the OverrideTye parameter in the MangaPage section to manually enter a type per page that is to be crawled

- **AuthorSelector**:

  - **Type**: `List<Selector>`
  - **Description**: Contains the query to extract the author's name.

- **RatingSelector**:

  - **Type**: `List<Selector>`
  - **Description**: Contains the query to extract the rating of the manga. Can be empty, which will prompt the crawler to assign a random rating. Note; right now API does not accept rating, so this is only for future compatibility

- **GenreSelector**:

  - **Type**: `List<Selector>`
  - **Description**: Contains the query to extract the genres associated with the manga.

- **ChapterLinksSelector**:

  - **Type**: `List<Selector>`
  - **Description**: Contains the query to extract links to the chapters of the manga.

- **ChapterTitlesSelector**:

  - **Type**: `List<Selector>`
  - **Description**: Contains the query to extract titles of the chapters.

- **ChapterContentsSelector**:
  - **Type**: `List<Selector>`
  - **Description**: Contains multiple queries to extract the contents of the chapters, including images and backgrounds.

#### MangaPages

This section defines the specific pages to crawl for manga data.

- **Url**:

  - **Type**: `string`
  - **Description**: The URL of the manga series page to crawl.

-**OverrideType**

- **Type**: `string`
- **Default**: `null`
- **Description**: It is null by default however if it is filled it'll override the manga type. It is primarily used for sites where they don't have type field on page

- **FullUpdate**:

  - **Type**: `bool`
  - **Default**: `false`
  - **Description**: Indicates whether to perform a full update of the data. Set to `true` if the existing data should be completely replaced.

- **SaveImages**:
  - **Type**: `bool`
  - **Default**: `false`
  - **Description**: Indicates whether to save images found in the manga chapters. Set to `true` if images should be saved locally.

#### Example Configuration

Here’s an example of how the JSON structure may look for a specific manga site:

```json
{
  "SiteType": "Static",
  "RootUrl": "https://asuracomic.net",
  "ChapterRootUrl": "https://asuracomic.net/series",
  "PageData": {
    "TitleSelector": [
      {
        "Query": "//h1[contains(@class, 'entry-title')]",
        "Type": "Single",
        "DataFrom": "InnerText"
      },
      {
        "Query": "//div//div[2]//p",
        "Type": "Single",
        "DataFrom": "InnerText"
      }
    ],
    "DescriptionSelector": [
      {
        "Query": "//div[contains(@class, 'entry-content entry-content-single')]/p",
        "Type": "Single",
        "DataFrom": "InnerText"
      }
    ],
    "CoverSelector": [
      {
        "Query": "//img[contains(@class, 'wp-post-image')]",
        "Type": "Single",
        "DataFrom": "Src"
      }
    ],
    ...
  },
  "MangaPages": [
    {
      "Url": "https://mangatest.com/read/some-manga/",
      "FullUpdate": false,
      "SaveImages": false
    }
  ]
}
```

### appsettings.json

appsettings.json is used to configure the crawler operation. It is used to configure the concurrency, API and API Clients, and the Site Data Settings json

#### CrawlSettings

This section contains settings related to crawling behavior.

- **ParallelCrawlersOnStart**:

  - **Type**: `int`
  - **Default**: `1`
  - **Description**: The number of crawlers to start initially. Adjust this value based on your system's capability and the load of the websites being crawled. Each crawler corresponds to site

- **MaxParallelCrawlers**:

  - **Type**: `int`
  - **Default**: `1`
  - **Description**: The maximum number of crawlers that can run in parallel at any given time.

- **APISettings**:

  - **Address**:
    - **Type**: `string`
    - **Default**: `"http://localhost:8080/api/v1/"`
    - **Description**: The base URL for the API that the application will communicate with. Update this to the address of your API server.

- **HttpClientSettings**:

  - **CrawlDelay**:

    - **Type**: `int`
    - **Default**: `10000` (in milliseconds)
    - **Description**: The delay (in milliseconds) between successive crawl requests. Adjust to avoid overloading the target servers. Set to a higher value if needed.

  - **CrawlTimeout**:

    - **Type**: `int`
    - **Default**: `30000` (in milliseconds)
    - **Description**: The time limit (in milliseconds) for each crawl request. Set this to ensure the application does not hang indefinitely.

  - **UserAgent**:

    - **Type**: `string`
    - **Default**: `"Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/89.207.132.170 Safari/537.36"`
    - **Description**: The User-Agent string sent with HTTP requests to identify the crawler. You can customize this to mimic different browsers.

  - **UseProxy**:

    - **Type**: `bool`
    - **Default**: `false`
    - **Description**: Set to `true` if you want to use a proxy for crawling.

  - **ProxyList**:
    - **Type**: `List<string>`
    - **Default**: `[]`
    - **Description**: A list of proxy addresses (in string format) to be used for crawling. If `UseProxy` is `true`, specify the proxies here.

- **SiteSettings**:

  - **SitesToCrawl**:

    - **Type**: `List<SiteToCrawlData>`
    - **Description**: List of sites to crawl, where each entry has the following properties:

      - **CrawlerType**:

        - **Type**: `enum`
        - **Description**: The type of crawler to use (e.g., `"MangaCrawler"` or `WebNovelCrawler`). Specify the appropriate crawler for the site.

      - **DataFile**:
        - **Type**: `string`
        - **Description**: Path to the configuration file for the site (e.g., `"./Configs/RadiantScans.json"`). Ensure this file exists and is correctly formatted.

#### ClientSettings

This section contains the endpoints for various clients used in the application.

- **AuthorClient**:

  - **Create**: `string` - Endpoint to create authors (e.g., `"authors/"`).
  - **Get**: `string` - Endpoint to get authors by slug (e.g., `"authors/slugged/"`).

- **FileClient**:

  - **Create**: `string` - Endpoint to create downloaded files (e.g., `"files/downloaded/"`).

- **GenreClient**:

  - **Create**: `string` - Endpoint to create genres (e.g., `"genres/"`).
  - **Get**: `string` - Endpoint to get genres by slug (e.g., `"genres/slugged/"`).

- **ImageClient**:

  - **Create**: `string` - Endpoint to create images (e.g., `"images/"`).
  - **Get**: `string` - Endpoint to search images by url (e.g., `"images/search/"`).

- **MangaChapterClient**:

  - **Create**: `string` - Endpoint to create manga chapters (e.g., `"manga-chapters/"`).
  - **Get**: `string` - Endpoint to get manga chapters (e.g., `"manga-chapters/"`).

- **MangaChapterContentClient**:

  - **Create**: `string` - Endpoint to create manga chapter contents (e.g., `"manga-chapter-contents/"`).
  - **Get**: `string` - Endpoint to get manga chapter contents (e.g., `"manga-chapter-contents/"`).
  - **Update**: `string` - Endpoint to update manga chapter contents (e.g., `"manga-chapter-contents/"`).

- **MangaClient**:

  - **Create**: `string` - Endpoint to create mangas (e.g., `"mangas/"`).
  - **Get**: `string` - Endpoint to get mangas by slug (e.g., `"mangas/slugged/"`).

- **MangaTypeClient**:

  - **Create**: `string` - Endpoint to create manga types (e.g., `"manga-types/"`).
  - **Get**: `string` - Endpoint to get manga types by slug (e.g., `"manga-types/slugged/"`).

- **StatusClient**:
  - **Create**: `string` - Endpoint to create statuses (e.g., `"statuses/"`).
  - **Get**: `string` - Endpoint to get statuses by slug (e.g., `"statuses/slugged/"`).

```json
{
  "CrawlSettings": {
    "ParallelCrawlersOnStart": 2,
    "MaxParallelCrawlers": 4,
    "APISettings": {
      "Address": "http://localhost:8080/api/v1/"
    },
    "HttpClientSettings": {
      "CrawlDelay": 5000,
      "CrawlTimeout": 15000,
      "UserAgent": "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/89.207.132.170 Safari/537.36",
      "UseProxy": true,
      "ProxyList": ["http://proxy1:8080", "http://proxy2:8080"]
    },
    "SiteSettings": {
      "SitesToCrawl": [
        {
          "CrawlerType": "MangaCrawler",
          "DataFile": "./Configs/MangaConfig.json"
        }
      ]
    },
    "ClientSettings": {
       "AuthorClient": {
        "Create": "authors/",
        "Get": "authors/slugged/"
      },
      "FileClient": {
        "Create": "files/downloaded/"
      },
      ...
    }
  }
}
```
