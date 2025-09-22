using BookCoverDownloader.Enums;
using Microsoft.Extensions.Configuration;
using OpenLibraryNET;

namespace BookCoverDownloader
{
    internal class Program
    {
        private static async Task Main()
        {
            IConfiguration config = BuildConfig();
            DatabaseConnection databaseConnection = new(config);
            OpenLibraryCoversApi coversAPI = new(config);

            // use new OpenLibrary API
            OpenLibraryClient client = new();
            client.BackingClient.DefaultRequestHeaders.Add("User-Agent", config.GetValue<string>("CustomUserAgent"));

            List<string> isbns = databaseConnection.GetISBNListFromDB();
            
            foreach (string isbn in isbns)
            {
                Console.WriteLine();
                Logger.Log(LogSection.Main, $"Getting OpenLibrary Edition: {isbn}");
                OLEdition edition = await client.GetEditionAsync(isbn, OpenLibraryNET.Utility.BookIdType.ISBN);

                if (edition == null || edition.Data == null)
                {
                    Logger.Log(LogSection.Main, $"No Edition Data found for ISBN: {isbn}");
                    continue;
                }

                string authorName = edition.Data.AuthorKeys[0].Split("/")[^1].Replace("_", " ");

                if (authorName == null || authorName == string.Empty)
                {
                    Logger.Log(LogSection.Main, $"Author name was not found in Edition Data: {isbn}");
                    continue;
                }

                bool isSmallCoverOnDisk = coversAPI.CheckCoverExistsOnDisk(isbn, authorName, CoverSizing.SMALL);
                bool isMediumCoverOnDisk = coversAPI.CheckCoverExistsOnDisk(isbn, authorName, CoverSizing.MEDIUM);

                byte[] coverMedium = await client.Image.GetCoverAsync(OpenLibraryNET.Utility.CoverIdType.ISBN, isbn, OpenLibraryNET.Utility.ImageSize.Medium);

                bool isSmallCoverDownloaded;
                if (!isSmallCoverOnDisk && coverMedium != null)
                {
                    isSmallCoverDownloaded = await coversAPI.SaveCoverToDisk(isbn, authorName, "1", coverMedium);
                }
                else
                {
                    isSmallCoverDownloaded = false;
                }

                byte[] coverLarge = await client.Image.GetCoverAsync(OpenLibraryNET.Utility.CoverIdType.ISBN, isbn, OpenLibraryNET.Utility.ImageSize.Large);
                bool isMediumCoverDownloaded;
                if (!isMediumCoverOnDisk && coverLarge != null)
                {
                    isMediumCoverDownloaded = await coversAPI.SaveCoverToDisk(isbn, authorName, "2", coverLarge);
                }
                else
                {
                    isMediumCoverDownloaded = false;
                }

                if (isSmallCoverDownloaded && isMediumCoverDownloaded)
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
