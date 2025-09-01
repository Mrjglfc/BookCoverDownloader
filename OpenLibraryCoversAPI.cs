using Microsoft.Extensions.Configuration;

namespace BookCoverDownloader
{
    internal class OpenLibraryCoversAPI(IConfiguration config)
    {
        string? ol_covers_isbn_baseURL = config.GetValue<string>("OpenLibrary_Covers_ISBN_BaseURL");
        string? webServerURL = config.GetValue<string>("WebServerURL");

        internal async Task ImageDownloader(string isbn, string coverURL, string authorName, string size)
        {
            using var client = new HttpClient();
            client.DefaultRequestHeaders.Add("User-Agent", config.GetValue<string>("CustomUserAgent"));

            Logger.Log(LogSection.OpenLibraryCoversAPI, $"Downloading cover from: {coverURL}");
            byte[] imageBytes = await client.GetByteArrayAsync(coverURL);

            string coverPathOnDisk = GenerateWebServerCoverURL(isbn, authorName, size);
            Logger.Log(LogSection.OpenLibraryCoversAPI, $"Saving cover to: {coverPathOnDisk}");
            await File.WriteAllBytesAsync(coverPathOnDisk, imageBytes);
        }

        internal string[] GenerateCoverURL(ReadOnlySpan<char> isbn)
        {
            return [$"{ol_covers_isbn_baseURL}{isbn}-M.jpg", $"{ol_covers_isbn_baseURL}{isbn}-L.jpg"];
        }

        private string GenerateWebServerCoverURL(string isbn, string author, string size)
        {
            if (webServerURL != null)
            {
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

            return string.Empty;
        }
    }
}
