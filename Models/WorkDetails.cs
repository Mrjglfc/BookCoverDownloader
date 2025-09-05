namespace BookCoverDownloader.Models
{
    class WorkDetails
    {
        public string? title { get; set; }
        public AuthorsContainer[]? authors { get; set; }
        public string? key { get; set; }
        public KeyContainer? type { get; set; }
    }

    class AuthorsContainer
    {
        public required KeyContainer author { get; set; }
        public required KeyContainer type { get; set; }
    }
}
