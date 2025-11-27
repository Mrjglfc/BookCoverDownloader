namespace BookCoverDownloader.Models
{
    public class AuthorModel(string authorOLID, string authorName)
    {
        public string OLID { get; set; } = authorOLID;
        public string Name { get; set; } = authorName;
    }
}
