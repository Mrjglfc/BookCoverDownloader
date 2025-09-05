using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.Reflection.Metadata.Ecma335;

namespace BookCoverDownloader
{
    public class DatabaseConnection(IConfiguration configuration)
    {
        private string GetCoverISBNsQS = "SELECT ISBN13 FROM dbo.Books WHERE HasCover = 0;";
        private string UpdateCoverISBNsQS = "UPDATE dbo.Books SET HasCover = 1 WHERE ISBN13 = ";

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

        public bool UpdateHasCover(string isbn)
        {
            using SqlConnection connection = new(configuration.GetValue<string>("ConnectionString"));
            SqlCommand command = new($"{UpdateCoverISBNsQS}{isbn};", connection);

            Logger.Log(LogSection.DatabaseConnection, $"Updating ISBN: {isbn} HasCover entry");
            command.Connection.Open();
            int rowsAffected = command.ExecuteNonQuery();

            return rowsAffected switch
            {
                1 => true,
                0 => false,
                _ => false,
            };
        }
    }
}