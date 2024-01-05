using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Phone_Scraper
{
    public class DatabaseHandler
    {
        private string connectionString;

        public DatabaseHandler(string dbFilePath)
        {
            // Set the path for the database within the Database folder of the project
            string dbFolder = Path.Combine(Directory.GetCurrentDirectory(), "Database");
            string dbFileName = "phonebook.db";
            string fullPath = Path.Combine(dbFolder, dbFileName);

            // Ensure the directory for the database file exists
            if (!Directory.Exists(dbFolder))
            {
                Directory.CreateDirectory(dbFolder);
            }

            connectionString = $"Data Source={fullPath};";
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
            ";
            command.ExecuteNonQuery();
        }

        public async Task InsertPhonebookEntryAsync(PhonebookEntry entry)
        {
            try
            {
                using var connection = new SqliteConnection(connectionString);
                await connection.OpenAsync();
                using var command = connection.CreateCommand();
                command.CommandText = @"
            INSERT INTO PhonebookEntries (Name, PrimaryPhone, PrimaryAddress, Comments)
            VALUES ($name, $phone, $address, $comments)";

                // Ensure that null values are handled as DBNull.Value
                command.Parameters.AddWithValue("$name", entry.Name ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("$phone", entry.PrimaryPhone ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("$address", entry.PrimaryAddress ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("$comments", entry.Comments ?? (object)DBNull.Value);

                await command.ExecuteNonQueryAsync();
            }
            catch (SqliteException ex)
            {
                Console.WriteLine($"SQLite Error: {ex.Message}");
                throw;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"General Error: {ex.Message}");
                throw;
            }
        }

        public async Task InsertMultiplePhonebookEntriesAsync(IEnumerable<PhonebookEntry> entries)
        {
            try
            {
                using var connection = new SqliteConnection(connectionString);
                await connection.OpenAsync();
                using var transaction = connection.BeginTransaction();
                var command = connection.CreateCommand();
                command.CommandText = @"INSERT INTO PhonebookEntries (Name, PrimaryPhone, PrimaryAddress, Comments)
                            VALUES ($name, $phone, $address, $comments)";
                foreach (var entry in entries)
                {
                    command.Parameters.Clear();
                    command.Parameters.AddWithValue("$name", entry.Name);
                    command.Parameters.AddWithValue("$phone", entry.PrimaryPhone);
                    command.Parameters.AddWithValue("$address", entry.PrimaryAddress);
                    command.Parameters.AddWithValue("$comments", entry.Comments);
                    await command.ExecuteNonQueryAsync();
                }
                await transaction.CommitAsync();
            }
            catch (SqliteException ex)
            {
                Console.WriteLine($"SQLite Error: {ex.Message}");
                throw;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"General Error: {ex.Message}");
                throw;
            }
        }

        // Add more methods as necessary for database operations
    }
}
