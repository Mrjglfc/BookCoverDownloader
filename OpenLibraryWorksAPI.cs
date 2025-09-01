using BookCoverDownloader.Models;
using Microsoft.Extensions.Configuration;
using System.Text.Json;

namespace BookCoverDownloader
{
    internal class OpenLibraryWorksAPI(IConfiguration config)
    {
        private readonly string? BASE_URL = config.GetValue<string>("OpenLibrary_BaseURL");

        public async Task<string> GetWorksDetails(string authorKey)
        {
            using var client = new HttpClient();
            client.DefaultRequestHeaders.Add("User-Agent", config.GetValue<string>("CustomUserAgent"));

            string url = $"{BASE_URL}{authorKey}.json";
            Logger.Log(LogSection.OpenLibraryWorksAPI, $"Getting Works Data from: {url}");
            Stream jsonData = await client.GetStreamAsync(url);

            WorkDetails authorStruct = await JsonSerializer.DeserializeAsync<WorkDetails>(jsonData);
            return authorStruct.authors[0].author.key;
        }
    }
}
