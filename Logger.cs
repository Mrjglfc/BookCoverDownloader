namespace BookCoverDownloader
{
    internal class Logger
    {
        internal static void Log(LogSection section, string message)
        {
            Console.WriteLine($"{DateTime.Now.TimeOfDay} [{section}]: {message}");
        }
    }

    internal enum LogSection
    {
        Main = 0,
        OpenLibraryDetailsAPI,
        OpenLibraryWorksAPI,
        OpenLibraryAuthorsAPI,
        OpenLibraryCoversAPI,
        DatabaseConnection
    }
}
