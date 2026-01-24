namespace BookCoverDownloader.Models
{
    public class AuthorModel(string[] splitNames)
    {
        public string OLID { get; set; } = splitNames[^2].Replace("_", " ");
        public string Name { get; set; } = splitNames[^1].Replace("_", " ");
    }
}
