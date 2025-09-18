using Microsoft.Extensions.Configuration;

namespace BookCoverDownloader
{
    internal class OpenLibraryCoversApi(IConfiguration config)
    {
        private readonly string? _olCoversIsbnBaseUrl = config.GetValue<string>("OpenLibrary_Covers_ISBN_BaseURL");
        private readonly string? _webServerUrl = config.GetValue<string>("WebServerURL");

        internal async Task<bool> ImageDownloader(string isbn, string coverUrl, string authorName, string size)
        {
            Logger.Log(LogSection.OpenLibraryCoversAPI, $"Downloading cover from: {coverUrl}");

            using HttpClient client = new();
            client.DefaultRequestHeaders.Add("User-Agent", config.GetValue<string>("CustomUserAgent"));

            try
            {
                byte[] imageBytes = await client.GetByteArrayAsync($"{coverUrl}?default=false");
                string coverPathOnDisk = GenerateWebServerCoverUrl(isbn, authorName, size);
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

        internal string[] GenerateCoverUrl(ReadOnlySpan<char> isbn)
        {
            return [$"{_olCoversIsbnBaseUrl}{isbn}-M.jpg", $"{_olCoversIsbnBaseUrl}{isbn}-L.jpg"];
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
            if (size == "1")
            {
                coverFilePath += "_sm";
            }

            coverFilePath += ".jpg";
            return coverFilePath;
        }

        internal bool CheckCoverExistsOnDisk(string isbn, string author, string size)
        {
            string fileDiskPath = GenerateWebServerCoverUrl(isbn, author, size);
            return File.Exists(fileDiskPath);
        }
    }
}
