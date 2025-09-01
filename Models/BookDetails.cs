namespace BookCoverDownloader.Models
{
    class BookDetails
    {
        public required KeyContainer[] works { get; set; } = [];
        public string title { get; set; } = string.Empty;
        public KeyContainer[] authors { get; set; } = [];
        public string publish_date { get; set; } = string.Empty;
        public string[] publishers { get; set; } = [];
        public long[] covers { get; set; } = [];
        public KeyContainer[] languages { get; set; } = [];
        public short number_of_pages { get; set; } = 0;
        public string[] isbn_10 { get; set; } = [];
        public required string[] isbn_13 { get; set; } = [];
    }
}
