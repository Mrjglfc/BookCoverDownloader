using BookCoverDownloader.Models;
using Microsoft.Extensions.Configuration;
using System.Text.Json;

namespace BookCoverDownloader
{
    internal class OpenLibraryDetailsAPI(IConfiguration config)
    {
        // https://openlibrary.org/isbn/9780140328721.json
        private readonly string? BASE_URL = config.GetValue<string>("OpenLibrary_Books_BaseURL");

        public async Task<BookDetails?> GetBookDetails(string isbn13)
        {
            using var client = new HttpClient();
            client.DefaultRequestHeaders.Add("User-Agent", config.GetValue<string>("CustomUserAgent"));
            string url = $"{BASE_URL}{isbn13}.json";

            Logger.Log(LogSection.OpenLibraryDetailsAPI, $"Getting Book Data from: {url}");
            Stream jsonData;

            try
            {
                jsonData = await client.GetStreamAsync(url);
                BookDetails? book = await JsonSerializer.DeserializeAsync<BookDetails>(jsonData);
                return book;
            }
            catch (Exception e)
            {
                Logger.Log(LogSection.OpenLibraryDetailsAPI, e.Message);
                return null;
            }
        }
    }
}
