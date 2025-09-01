namespace BookCoverDownloader.Models
{
    class AuthorDetails
    {
        public long[] photos { get; set; } = [];
        public string bio { get; set; } = string.Empty;
        public required string name { get; set; } 
        public string[] alternate_names { get; set; } = [];
        public string birth_date { get; set; } = string.Empty;
        public LinksInfo[] links { get; set; } = [];
    }
}
