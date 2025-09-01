using BookCoverDownloader.Models;
using Microsoft.Extensions.Configuration;
using System.Text.Json;

namespace BookCoverDownloader
{
    internal class OpenLibraryAuthorsAPI(IConfiguration config)
    {
        private readonly string? BASE_URL = config.GetValue<string>("OpenLibrary_BaseURL");

        public async Task<AuthorDetails?> GetAuthorsDetails(string authorKey)
        {
            using var client = new HttpClient();
            client.DefaultRequestHeaders.Add("User-Agent", config.GetValue<string>("CustomUserAgent"));

            string url = $"{BASE_URL}{authorKey}.json";
            Logger.Log(LogSection.OpenLibraryAuthorsAPI, $"Getting Author Data from: {url}");

            try
            {
                Stream jsonData = await client.GetStreamAsync(url);
                return await JsonSerializer.DeserializeAsync<AuthorDetails>(jsonData);
            }
            catch
            {
                return default;
            }
        }
    }
}
