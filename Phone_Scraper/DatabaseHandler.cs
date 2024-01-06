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
                Comments TEXT,
                RandomCharacters TEXT
            );
            ";
            command.ExecuteNonQuery();
        }

        public async Task InsertPhonebookEntryAsync(PhonebookEntry entry)
        {
            try
            {
                // Ensure the schema is up-to-date
                UpdateSchema();

                using var connection = new SqliteConnection(connectionString);
                await connection.OpenAsync();
                using var command = connection.CreateCommand();
                command.CommandText = @"
            INSERT INTO PhonebookEntries (Name, PrimaryPhone, PrimaryAddress, Comments, RandomCharacters)
            VALUES ($name, $phone, $address, $comments, $randomCharacters)";

                // Bind the parameters
                command.Parameters.AddWithValue("$name", entry.Name ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("$phone", entry.PrimaryPhone ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("$address", entry.PrimaryAddress ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("$comments", entry.Comments ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("$randomCharacters", entry.RandomCharacters ?? (object)DBNull.Value);

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
                command.CommandText = @"INSERT INTO PhonebookEntries (Name, PrimaryPhone, PrimaryAddress, Comments, RandomCharacters)
                            VALUES ($name, $phone, $address, $comments, $randomCharacters)";
                foreach (var entry in entries)
                {
                    command.Parameters.Clear();
                    command.Parameters.AddWithValue("$name", entry.Name);
                    command.Parameters.AddWithValue("$phone", entry.PrimaryPhone);
                    command.Parameters.AddWithValue("$address", entry.PrimaryAddress);
                    command.Parameters.AddWithValue("$comments", entry.Comments);
                    command.Parameters.AddWithValue("$randomCharacters", entry.RandomCharacters);
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

        public void UpdateSchema()
        {
            try
            {
                using var connection = new SqliteConnection(connectionString);
                connection.Open();
                using var command = connection.CreateCommand();

                // Step 1: Create a backup table with the existing data
                command.CommandText = @"
            CREATE TABLE IF NOT EXISTS PhonebookEntries_backup AS
            SELECT * FROM PhonebookEntries;
        ";
                command.ExecuteNonQuery();

                // Step 2: Drop the original table
                command.CommandText = "DROP TABLE IF EXISTS PhonebookEntries;";
                command.ExecuteNonQuery();

                // Step 3: Create a new table with the updated schema
                command.CommandText = @"
            CREATE TABLE IF NOT EXISTS PhonebookEntries (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                Name TEXT,
                PrimaryPhone TEXT,
                PrimaryAddress TEXT,
                Comments TEXT,
                RandomCharacters TEXT
            );
        ";
                command.ExecuteNonQuery();

                // Step 4: Copy data from the backup table to the new table
                command.CommandText = @"
            INSERT INTO PhonebookEntries (Id, Name, PrimaryPhone, PrimaryAddress, Comments, RandomCharacters)
            SELECT Id, Name, PrimaryPhone, PrimaryAddress, Comments, RandomCharacters FROM PhonebookEntries_backup;
        ";
                command.ExecuteNonQuery();

                // Drop the backup table
                command.CommandText = "DROP TABLE IF EXISTS PhonebookEntries_backup;";
                command.ExecuteNonQuery();
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