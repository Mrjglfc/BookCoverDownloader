using Microsoft.Extensions.Configuration;

namespace BookCoverDownloader
{
    internal class OpenLibraryCoversAPI(IConfiguration config)
    {
        string? ol_covers_isbn_baseURL = config.GetValue<string>("OpenLibrary_Covers_ISBN_BaseURL");
        string? webServerURL = config.GetValue<string>("WebServerURL");

        internal async Task<bool> ImageDownloader(string isbn, string coverURL, string authorName, string size)
        {
            Logger.Log(LogSection.OpenLibraryCoversAPI, $"Downloading cover from: {coverURL}");

            using HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Add("User-Agent", config.GetValue<string>("CustomUserAgent"));

            try
            {
                byte[] imageBytes = await client.GetByteArrayAsync($"{coverURL}?default=false");
                string coverPathOnDisk = GenerateWebServerCoverURL(isbn, authorName, size);
                Logger.Log(LogSection.OpenLibraryCoversAPI, $"Saving cover to: {coverPathOnDisk}");
                await File.WriteAllBytesAsync(coverPathOnDisk, imageBytes);
                return true;
            }
            catch(HttpRequestException e)
            {
                Logger.Log(LogSection.OpenLibraryCoversAPI,  e.Message);
                return false;
            }
        }

        internal string[] GenerateCoverURL(ReadOnlySpan<char> isbn)
        {
            return [$"{ol_covers_isbn_baseURL}{isbn}-M.jpg", $"{ol_covers_isbn_baseURL}{isbn}-L.jpg"];
        }

        private string GenerateWebServerCoverURL(string isbn, string author, string size)
        {
            if (webServerURL == null) return string.Empty;
            
            if (!Directory.Exists($"{webServerURL}{author}"))
            {
                Directory.CreateDirectory($"{webServerURL}{author}");
            }

            string coverFilePath = $"{webServerURL}{author}\\{isbn}";
            if (size == "1")
            {
                coverFilePath += "_sm";
            }

            coverFilePath += ".jpg";
            return coverFilePath;
        }

        internal bool CheckCoverExistsOnDisk(string isbn, string author, string size)
        {
            string fileDiskPath = GenerateWebServerCoverURL(isbn, author, size);
            return File.Exists(fileDiskPath);
        }
    }
}
