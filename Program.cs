using BookCoverDownloader.Enums;
using BookCoverDownloader.Models;
using Microsoft.Extensions.Configuration;
using OpenLibraryNET;
using OpenLibraryNET.Utility;
using System.Diagnostics;

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

                Debug.Assert(edition.Data != null, $"No Edition Data found for ISBN: {isbn}");

                string[] splitName = edition.Data.AuthorKeys[0].Split("/");
                AuthorModel authorModel = new(splitName);

                Debug.Assert(authorModel.Name != string.Empty, $"Author name was not found in Edition Data: {isbn}");

                bool isSmallCoverOnDisk = coversAPI.CheckCoverExistsOnDisk(isbn, authorModel.Name, CoverSizing.SMALL);
                bool isMediumCoverOnDisk = coversAPI.CheckCoverExistsOnDisk(isbn, authorModel.Name, CoverSizing.MEDIUM);

                byte[] coverMedium = await DownloadCover(client, isbn, ImageSize.Medium);
                byte[] coverLarge = await DownloadCover(client, isbn, ImageSize.Large);

                Debug.Assert(coverMedium.Length != 0 || coverLarge.Length != 0, "Cover bytes returned was empty, not saving data.");

                bool isSmallCoverDownloaded = await SaveCoverToDisk(coversAPI, isbn, authorModel.Name, CoverSizing.SMALL, isSmallCoverOnDisk, coverMedium);
                bool isMediumCoverDownloaded = await SaveCoverToDisk(coversAPI, isbn, authorModel.Name, CoverSizing.MEDIUM, isMediumCoverOnDisk, coverLarge);

                if (isSmallCoverDownloaded && isMediumCoverDownloaded)
                {
                    databaseConnection.UpdateHasCover(isbn);
                }

                string authorFilePath = coversAPI.GenerateAuthorImageURL(authorModel.Name);

                if(!File.Exists(authorFilePath))
                {
                    byte[] authorImageBytes = await DownloadAuthorImage(client, authorModel.OLID);
                    await SaveAuthorImage(authorFilePath, authorImageBytes);
                }
            }
        }

        /// <summary>
        /// Saves a .jpg file of the Author Profile Picture as sourced from Open Library
        /// </summary>
        /// <param name="coversAPI"></param>
        /// <param name="client"></param>
        /// <param name="authorModel"></param>
        /// <returns></returns>
        private static async Task SaveAuthorImage(string authorFilePath, byte[] authorImageBytes)
        {
            if (authorImageBytes.Length != 0)
            {
                await File.WriteAllBytesAsync(authorFilePath, authorImageBytes);
            }
        }

        private static async Task<bool> SaveCoverToDisk(OpenLibraryCoversApi coversAPI, string isbn, string authorName, string size, bool isCoverOnDisk, byte[] coverData)
        {
            return !isCoverOnDisk ? await coversAPI.SaveCoverToDisk(isbn, authorName, size, coverData) : isCoverOnDisk;
        }

        /// <summary>
        /// Downloads the Edition cover
        /// </summary>
        /// <param name="client"></param>
        /// <param name="isbn"></param>
        /// <param name="size"></param>
        /// <returns>Edition cover data as Byte[]</returns>
        private static async Task<byte[]> DownloadCover(OpenLibraryClient client, string isbn, ImageSize size)
        {
            byte[] coverData;
            try
            {
                coverData = await client.Image.GetCoverAsync(CoverIdType.ISBN, isbn, size);
            }
            catch (HttpRequestException e)
            {
                Logger.Log(LogSection.Main, e.Message);
                return [];
            }

            return coverData;
        }

        /// <summary>
        /// Obtains a record of the Book Edition if found on OpenLibrary
        /// </summary>
        /// <param name="client"></param>
        /// <param name="isbn"></param>
        /// <returns>OLEdition Record</returns>
        private static async Task<OLEdition> ObtainEdition(OpenLibraryClient client, string isbn)
        {
            OLEdition edition = new();
            try
            {
                edition = await client.GetEditionAsync(isbn, BookIdType.ISBN);
            }
            catch (NullReferenceException e)
            {
                Logger.Log(LogSection.Main, e.Message);
                return edition;
            }

            return edition;
        }

        /// <summary>
        /// Downloads Author Image as byte[] data
        /// </summary>
        /// <param name="client"></param>
        /// <param name="authorOLID"></param>
        /// <returns></returns>
        private static async Task<byte[]> DownloadAuthorImage(OpenLibraryClient client, string authorOLID)
        {
            byte[] authorImageBytes;
            try
            {
                authorImageBytes = await client.Image.GetAuthorPhotoAsync("olid", authorOLID, "M");
            }
            catch (HttpRequestException e)
            {
                Logger.Log(LogSection.Main, e.Message);
                return [];
            }

            return authorImageBytes;
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
