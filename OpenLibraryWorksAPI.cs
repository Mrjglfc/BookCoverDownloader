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

            try
            {
                Stream jsonData = await client.GetStreamAsync(url);

                WorkDetails? authorStruct = await JsonSerializer.DeserializeAsync<WorkDetails>(jsonData);
                if (authorStruct != null && authorStruct.authors != null)
                {
                    AuthorsContainer container = authorStruct.authors[0];
                    return container.author.key;
                }
                
                return string.Empty;
            }
            catch (Exception e)
            {
                Logger.Log(LogSection.OpenLibraryWorksAPI, e.Message);
                return string.Empty;
            }
        }
    }
}
