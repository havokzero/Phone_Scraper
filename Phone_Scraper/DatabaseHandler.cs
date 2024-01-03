using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Phone_Scraper
{
    public class DatabaseHandler
    {
        private string connectionString;

        public DatabaseHandler(string dbFilePath)
        {
            connectionString = $"Data Source={dbFilePath};";
            InitializeDatabase();
        }

        private void InitializeDatabase()
        {
            using var connection = new SqliteConnection(connectionString);
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText =
            @"
            CREATE TABLE IF NOT EXISTS PhonebookEntries (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                Name TEXT,
                PrimaryPhone TEXT,
                PrimaryAddress TEXT,
                Comments TEXT
            );
            -- Add more tables or columns as necessary
        ";
            command.ExecuteNonQuery();
        }

        public void InsertPhonebookEntry(PhonebookEntry entry)
        {
            using var connection = new SqliteConnection(connectionString);
            connection.Open();
            var command = connection.CreateCommand();
            command.CommandText =
                @"INSERT INTO PhonebookEntries (Name, PrimaryPhone, PrimaryAddress, Comments) VALUES ($name, $phone, $address, $comments)";
            command.Parameters.AddWithValue("$name", entry.Name);
            command.Parameters.AddWithValue("$phone", entry.PrimaryPhone);
            command.Parameters.AddWithValue("$address", entry.PrimaryAddress);
            command.Parameters.AddWithValue("$comments", entry.Comments);
            command.ExecuteNonQuery();
        }
       
        // Add more methods as necessary for database operations
    }
}

