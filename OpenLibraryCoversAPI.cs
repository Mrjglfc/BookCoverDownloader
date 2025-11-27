using BookCoverDownloader.Enums;
using Microsoft.Extensions.Configuration;
using OpenLibraryNET;

namespace BookCoverDownloader
{
    internal class OpenLibraryCoversApi(IConfiguration config)
    {
        private readonly string? _olCoversIsbnBaseUrl = config.GetValue<string>("OpenLibrary_Covers_ISBN_BaseURL");
        private readonly string? _webServerUrl = config.GetValue<string>("WebServerURL");

        /// <summary>
        /// Saves the ISBN Edition Cover to Disk
        /// </summary>
        /// <param name="isbn"></param>
        /// <param name="authorName"></param>
        /// <param name="size"></param>
        /// <param name="imageBytes"></param>
        /// <returns></returns>
        internal async Task<bool> SaveCoverToDisk(string isbn, string authorName, string size, byte[] imageBytes)
        {
            string coverPathOnDisk = GenerateWebServerCoverUrl(isbn, authorName, size);

            try
            {
                await File.WriteAllBytesAsync(coverPathOnDisk, imageBytes);
                Logger.Log(LogSection.OpenLibraryCoversAPI, $"Saved cover to: {coverPathOnDisk}");
                return true;
            }
            catch (OperationCanceledException e)
            {
                Logger.Log(LogSection.OpenLibraryCoversAPI, $"{size} Cover failed to save for ISBN: {isbn} | AuthorName: {authorName} | filePath: {coverPathOnDisk}");
                Logger.Log(LogSection.OpenLibraryCoversAPI, e.Message);
                return false;
            }
        }

        internal string[] GenerateCoverUrl(ReadOnlySpan<char> isbn)
        {
            return [$"{_olCoversIsbnBaseUrl}{isbn}-M.jpg", $"{_olCoversIsbnBaseUrl}{isbn}-L.jpg"];
        }

        internal string GenerateAuthorImageURL(string authorName)
        {
            return $"{_webServerUrl}{authorName}/{authorName}.jpg";
        }

        private string GenerateWebServerCoverUrl(string isbn, string author, string size)
        {
            if (_webServerUrl == null) return string.Empty;
            
            if (!Directory.Exists($"{_webServerUrl}{author}"))
            {
                Directory.CreateDirectory($"{_webServerUrl}{author}");
                // TODO: We should trigger a download of the Author Profile Picture here
            }

            string coverFilePath = $"{_webServerUrl}{author}\\{isbn}";
            if (size == CoverSizing.SMALL)
            {
                coverFilePath += "_sm";
            }

            coverFilePath += ".jpg";
            return coverFilePath;
        }

        internal bool CheckCoverExistsOnDisk(string isbn, string author, string size)
        {
            string fileDiskPath = GenerateWebServerCoverUrl(isbn, author, size);

            if (File.Exists(fileDiskPath))
            {
                Logger.Log(LogSection.OpenLibraryCoversAPI, $"{size} Cover found on disk for ISBN: {isbn} | Author: {author}");
                return true; 
            }
            else 
            {
                Logger.Log(LogSection.OpenLibraryCoversAPI, $"{size} Cover does not exist on disk for ISBN: {isbn} | Author: {author}");
                return false; 
            }
        }
    }
}
