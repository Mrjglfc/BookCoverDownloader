using BookCoverDownloader.Enums;
using Microsoft.Extensions.Configuration;
using OpenLibraryNET;
using OpenLibraryNET.Utility;

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

                OLEdition edition = await ObtainEdition(client, isbn);

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

                byte[] coverMedium = await DownloadCover(client, isbn, ImageSize.Medium);
                byte[] coverLarge = await DownloadCover(client, isbn, ImageSize.Large);

                bool isSmallCoverDownloaded = await SaveCoverToDisk(coversAPI, isbn, authorName, CoverSizing.SMALL, isSmallCoverOnDisk, coverMedium);
                bool isMediumCoverDownloaded = await SaveCoverToDisk(coversAPI, isbn, authorName, CoverSizing.MEDIUM, isMediumCoverOnDisk, coverLarge);

                if (isSmallCoverDownloaded && isMediumCoverDownloaded)
                {
                    databaseConnection.UpdateHasCover(isbn);
                }
            }
        }

        private static async Task<bool> SaveCoverToDisk(OpenLibraryCoversApi coversAPI, string isbn, string authorName, string size, bool isCoverOnDisk, byte[] coverData)
        {
            if (!isCoverOnDisk && coverData != null)
            {
                return await coversAPI.SaveCoverToDisk(isbn, authorName, size, coverData);
            }

            return isCoverOnDisk;
        }

        private static async Task<byte[]> DownloadCover(OpenLibraryClient client, string isbn, ImageSize size)
        {
            byte[] coverData;
            try
            {
                coverData = await client.Image.GetCoverAsync(OpenLibraryNET.Utility.CoverIdType.ISBN, isbn, size);
            }
            catch (HttpRequestException e)
            {
                Logger.Log(LogSection.Main, e.Message);
                return [];
            }

            return coverData;
        }

        private static async Task<OLEdition> ObtainEdition(OpenLibraryClient client, string isbn)
        {
            OLEdition edition = new();
            try
            {
                edition = await client.GetEditionAsync(isbn, OpenLibraryNET.Utility.BookIdType.ISBN);
            }
            catch (NullReferenceException e)
            {
                Logger.Log(LogSection.Main, e.Message);
                return edition;
            }

            return edition;
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
