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
            OpenLibraryCoversAPI coversApi = new(config);
            DatabaseConnection databaseConnection = new(config);

            Logger.Log(LogSection.Main,"Accessing Database");
            List<string> isbns = databaseConnection.GetISBNListFromDB();
            
            foreach (string isbn in isbns)
            {
                Logger.Log(LogSection.Main, $"\nWorking with ISBN: {isbn}");
                
                if (!ISBNValidator.IsValidIsbn13(isbn))
                {
                    continue;
                }
                
                BookDetails? book = await detailsApi.GetBookDetails(isbn);
                
                if (book == null)
                {
                    Logger.Log(LogSection.Main, "Error retrieving Book Details. Possible 404");
                    continue;
                }

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

                AuthorDetails? author = await authorsApi.GetAuthorsDetails(authorKey);

                if (author == null) continue;
                
                string[] coverUrls = coversApi.GenerateCoverURL(isbn);

                await coversApi.ImageDownloader(isbn, coverUrls[0], author.name, CoverSizing.SMALL);
                await coversApi.ImageDownloader(isbn, coverUrls[1], author.name, CoverSizing.MEDIUM);
            }
        }

        private static IConfiguration BuildConfig()
        {
            IConfigurationBuilder builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddUserSecrets<Program>();

            IConfiguration config = builder.Build();
            return config;
        }
    }
}
