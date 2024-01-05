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
using Phone_Scraper;
using SQLitePCL;


// Assuming PhonebookEntry is defined in PhonebookEntry.cs
// Assuming Scraper is defined in Scraper.cs
// Assuming DatabaseHandler is defined in DatabaseHandler.cs

namespace Phone_Scraper
//public class Program
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            string driverPath = Path.Combine(Directory.GetCurrentDirectory(), "Driver");
            // Initialize SQLitePCL.raw
            Batteries.Init();

            // Set up the Chrome WebDriver
            IWebDriver driver = new ChromeDriver(driverPath);

            // Initialize the scraper with the driver
            var scraper = new Scraper(driver);

            // Define the list of URLs you want to scrape
            List<string> targetUrls = new List<string>
    {
        "https://www.usphonebook.com/some-specific-page1",
        "https://www.usphonebook.com/some-specific-page2",
        // Add more URLs as needed
    };

            foreach (var targetUrl in targetUrls)
            {
                // Scrape the page
                PhonebookEntry phonebookEntry = await scraper.Scrape(targetUrl);

                // Initialize the database handler
                var dbHandler = new DatabaseHandler("Database/phonebook.db"); // Correct path to the database file

                // Insert the entry into the database
                await dbHandler.InsertPhonebookEntryAsync(phonebookEntry); // await the async method
            }

            // Close the driver once done
            driver.Quit();
        }
    }
}