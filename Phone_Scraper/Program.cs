using System;
using System.Text.RegularExpressions;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium;
using System.Threading.Tasks;
using RestSharp;
using System.Collections.Generic;
using Phone_Scraper;
using HtmlAgilityPack;
using Microsoft.Data.Sqlite;

// Assuming PhonebookEntry is defined in PhonebookEntry.cs
// Assuming Scraper is defined in Scraper.cs
// Assuming DatabaseHandler is defined in DatabaseHandler.cs

public class Program
{
    public static async Task Main(string[] args)
    {
        // Initialize the scraper and database handler
        var scraper = new Scraper();
        var dbHandler = new DatabaseHandler("path_to_your_database_file.db");

        // Define the URL you want to scrape
        string targetUrl = "https://www.usphonebook.com/some-specific-page";

        // Scrape the page
        PhonebookEntry phonebookEntry = await scraper.Scrape(targetUrl);

        // Insert the entry into the database
        dbHandler.InsertPhonebookEntry(phonebookEntry);

        // TODO: Implement logic to handle additional pages, multiple entries, etc.
    }
}
