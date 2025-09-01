using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace BookCoverDownloader
{
    public class DatabaseConnection(IConfiguration configuration)
    {
        private string GetCoverISBNsQS = "SELECT ISBN13 FROM dbo.Books WHERE HasCover = 0;";

        public List<string> GetISBNListFromDB()
        {
            List<string> isbnList = [];
            
            using SqlConnection connection = new(configuration.GetValue<string>("ConnectionString"));
            SqlCommand command = new(GetCoverISBNsQS, connection);

            Logger.Log(LogSection.DatabaseConnection, "Obtaining non-retreived Cover ISBNs");
            command.Connection.Open();
            using SqlDataReader reader = command.ExecuteReader();
            
            while (reader.Read())
            {
                isbnList.Add(reader["ISBN13"].ToString());
            }

            return isbnList;
        }
    }
}