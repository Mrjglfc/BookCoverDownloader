using BookCoverDownloader.Enums;
using BookCoverDownloader.Models;
using Microsoft.Extensions.Configuration;

namespace BookCoverDownloader
{
    internal class Program
    {
        private static async Task Main()
        {
            IConfiguration config = BuildConfig();

            OpenLibraryDetailsAPI detailsApi = new(config);
            OpenLibraryWorksAPI worksApi = new(config);
            OpenLibraryAuthorsAPI authorsApi = new(config);
            OpenLibraryCoversApi coversApi = new(config);
            DatabaseConnection databaseConnection = new(config);

            Logger.Log(LogSection.Main,"Accessing Database");
            List<string> isbns = databaseConnection.GetISBNListFromDB();
            
            foreach (string isbn in isbns)
            {
                Console.WriteLine();
                Logger.Log(LogSection.Main, $"Working with ISBN: {isbn}");
                
                if (!ISBNValidator.IsValidIsbn13(isbn))
                {
                    Logger.Log(LogSection.Main, $"ISBN [{isbn}] is not valid, skipping search...");
                    continue;
                }
                
                BookDetails? book = await detailsApi.GetBookDetails(isbn);
                
                if (book == null)
                {
                    continue;
                }

                Logger.Log(LogSection.Main, $"Found book information for ISBN: {isbn} | Title: {book.title}");

                string authorKey;
                if (book.authors.Length == 0)
                {
                    Logger.Log(LogSection.Main, "Author field was not found in Book JSON Data. Trying backup from Works API");
                    authorKey = await worksApi.GetWorksDetails(book.works[0].key);
                }
                else
                {
                    authorKey = book.authors[0].key;
                }

                if (authorKey == string.Empty)
                    continue;

                AuthorDetails? author = await authorsApi.GetAuthorsDetails(authorKey);

                if (author == null)
                {
                    Logger.Log(LogSection.Main, "Author was not found, unable to complete request, skipping search...");
                    continue;
                }

                Logger.Log(LogSection.Main, $"Found Author information for ISBN: {isbn} | Name: {author.name}");
                
                string[] coverUrls = coversApi.GenerateCoverUrl(isbn);

                bool isSmallCoverOnDisk = coversApi.CheckCoverExistsOnDisk(isbn, author.name, CoverSizing.SMALL);
                bool isMediumCoverOnDisk = coversApi.CheckCoverExistsOnDisk(isbn, author.name, CoverSizing.MEDIUM);

                bool isSmallCoverDownloaded = true;
                if (!isSmallCoverOnDisk)
                {
                    isSmallCoverDownloaded = await coversApi.ImageDownloader(isbn, coverUrls[0], author.name, CoverSizing.SMALL);
                }
                
                bool isMediumCoverDownloaded = true;
                if (!isMediumCoverOnDisk)
                {
                    isMediumCoverDownloaded = await coversApi.ImageDownloader(isbn, coverUrls[1], author.name, CoverSizing.MEDIUM);
                }

                if(isSmallCoverDownloaded && isMediumCoverDownloaded)
                {
                    databaseConnection.UpdateHasCover(isbn);
                }
            }
        }

        private static IConfiguration BuildConfig()
        {
            IConfigurationBuilder builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddUserSecrets<Program>();

            return builder.Build();
        }
    }
}
