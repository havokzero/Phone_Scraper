using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using Phone_Scraper;
using SQLitePCL;

namespace Phone_Scraper
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            string driverPath = Path.Combine(Directory.GetCurrentDirectory(), "Driver");

            // Initialize SQLitePCL.raw
            Batteries.Init();

            // Set up the Chrome WebDriver
            var driver = new ChromeDriver(driverPath);

            // Initialize the scraper with the driver
            var scraper = new Scraper(driver);

            // Create a single HttpClient instance to reuse
            var httpClient = new HttpClient();

            // Define the base URL
            string baseUrl = "https://www.usphonebook.com/";

            // List to store URLs to be crawled
            var urlsToCrawl = new Queue<string>();

            // Add the initial URL to start crawling
            string startingUrl = "https://www.usphonebook.com"; // Change this to your desired starting URL
            urlsToCrawl.Enqueue(startingUrl);

            while (urlsToCrawl.Any())
            {
                // Get the next URL to crawl
                string currentUrl = urlsToCrawl.Dequeue();

                // Visit the URL
                driver.Navigate().GoToUrl(currentUrl);

                // Extract and process information from the page
                PhonebookEntry phonebookEntry = await scraper.Scrape(currentUrl);

                // Initialize the database handler
                var dbHandler = new DatabaseHandler("Database/phonebook.db"); // Correct path to the database file

                // Insert the entry into the database
                await dbHandler.InsertPhonebookEntryAsync(phonebookEntry); // await the async method

                // Find and add links on the current page to the queue for crawling
                var links = driver.FindElements(By.XPath("//a[contains(@href, '/')]"));
                foreach (var link in links)
                {
                    string href = link.GetAttribute("href");
                    string absoluteUrl = new Uri(new Uri(baseUrl), href).AbsoluteUri;

                    // Ensure the URL is from the same domain and not already in the queue
                    if (absoluteUrl.StartsWith(baseUrl) && !urlsToCrawl.Contains(absoluteUrl))
                    {
                        urlsToCrawl.Enqueue(absoluteUrl);
                    }
                }

                // Add a delay between crawling pages to avoid overloading the website
                Thread.Sleep(TimeSpan.FromSeconds(1));
            }

            // Close the driver once done
            driver.Quit();

            // Dispose of the HttpClient
            httpClient.Dispose();
        }
    }
}
