using Microsoft.Data.Sqlite;
using Newtonsoft.Json;
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
                Age TEXT,
                CurrentAddress TEXT,
                CurrentPhone TEXT,
                PreviousAddresses TEXT, -- This could be a JSON string or a comma-separated list
                PreviousPhones TEXT, -- This could be a JSON string or a comma-separated list
                Relatives TEXT, -- This could be a JSON string or a comma-separated list
                Associates TEXT, -- This could be a JSON string or a comma-separated list
                Email TEXT
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
                using var connection = new SqliteConnection(connectionString);
                await connection.OpenAsync();

                // Convert lists to JSON strings for storing
                string previousAddresses = JsonConvert.SerializeObject(entry.PreviousAddresses);
                string previousPhones = JsonConvert.SerializeObject(entry.PreviousPhones);
                string relatives = JsonConvert.SerializeObject(entry.Relatives);
                string associates = JsonConvert.SerializeObject(entry.Associates);

                using var command = connection.CreateCommand();
                command.CommandText = @"
            INSERT INTO PhonebookEntries (Name, Age, CurrentAddress, CurrentPhone, PreviousAddresses, PreviousPhones, Relatives, Associates, Email, Comments, RandomCharacters)
            VALUES ($name, $age, $currentAddress, $currentPhone, $previousAddresses, $previousPhones, $relatives, $associates, $email, $comments, $randomCharacters)";

                // Bind the parameters
                command.Parameters.AddWithValue("$name", entry.Name ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("$age", entry.Age ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("$currentAddress", entry.CurrentAddress ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("$currentPhone", entry.CurrentPhone ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("$previousAddresses", previousAddresses ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("$previousPhones", previousPhones ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("$relatives", relatives ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("$associates", associates ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("$email", entry.Email ?? (object)DBNull.Value);
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
                command.CommandText = @"
            INSERT INTO PhonebookEntries (
                Name, Age, CurrentAddress, CurrentPhone, PreviousAddresses, PreviousPhones, Relatives, Associates, Email, Comments, RandomCharacters
            ) VALUES (
                $name, $age, $currentAddress, $currentPhone, $previousAddresses, $previousPhones, $relatives, $associates, $email, $comments, $randomCharacters
            )";

                foreach (var entry in entries)
                {
                    // Convert lists to JSON strings for storing
                    string previousAddresses = JsonConvert.SerializeObject(entry.PreviousAddresses);
                    string previousPhones = JsonConvert.SerializeObject(entry.PreviousPhones);
                    string relatives = JsonConvert.SerializeObject(entry.Relatives);
                    string associates = JsonConvert.SerializeObject(entry.Associates);

                    // Clear previous parameters
                    command.Parameters.Clear();

                    // Bind the parameters
                    command.Parameters.AddWithValue("$name", entry.Name ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("$age", entry.Age ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("$currentAddress", entry.CurrentAddress ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("$currentPhone", entry.CurrentPhone ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("$previousAddresses", previousAddresses ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("$previousPhones", previousPhones ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("$relatives", relatives ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("$associates", associates ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("$email", entry.Email ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("$comments", entry.Comments ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("$randomCharacters", entry.RandomCharacters ?? (object)DBNull.Value);

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
                Age TEXT,
                CurrentAddress TEXT,
                CurrentPhone TEXT,
                PreviousAddresses TEXT,
                PreviousPhones TEXT,
                Relatives TEXT,
                Associates TEXT,
                Email TEXT,
                Comments TEXT,
                RandomCharacters TEXT
            );
        ";
                command.ExecuteNonQuery();

                // Step 4: Copy data from the backup table to the new table
                // Note: This step assumes all previous columns exist in the backup. If not, only copy the columns that exist.
                command.CommandText = @"
            INSERT INTO PhonebookEntries (Id, Name, Age, CurrentAddress, CurrentPhone, PreviousAddresses, PreviousPhones, Relatives, Associates, Email, Comments, RandomCharacters)
            SELECT Id, Name, Age, CurrentAddress, CurrentPhone, PreviousAddresses, PreviousPhones, Relatives, Associates, Email, Comments, RandomCharacters FROM PhonebookEntries_backup;
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