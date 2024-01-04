using HtmlAgilityPack;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using static Phone_Scraper.Scraper;

namespace Phone_Scraper
{
    public class Scraper : IWebsiteScraper
    {
        private IWebDriver driver;

        // Define your Regex patterns
        private static readonly Regex phoneRegex = new Regex(@"(\+?[1-9][0-9]{0,2}[\s\(\)\-\.\,\/\|]*)?(\(?\d{3}\)?[\s\-\.\,\/\|]*\d{3}[\s\-\.\,\/\|]*\d{4})");
        private static readonly Regex nameRegex = new Regex(@"(Name:|Name\s?:)\s?(.*?)($|\n)");
        private static readonly Regex addressRegex = new Regex(@"\d{1,5}\s\w+\s\w*(?:\s\w+)?\s(?:Avenue|Lane|Road|Boulevard|Drive|Street|Ave|Dr|Rd|Blvd|Ln|St)\.?");
        private static readonly Regex urlRegex = new Regex(@"https?:\/\/\S+");

        public Scraper(IWebDriver webDriver)
        {
            driver = new ChromeDriver();  // Make sure the ChromeDriver is in your system's PATH
            driver = webDriver;
        }

        public interface IWebsiteScraper
        {
            Task<PhonebookEntry> Scrape(string url);
        }

        public async Task<PhonebookEntry> Scrape(string url)
        {
            var entry = new PhonebookEntry();
            try
            {
                // Navigate to the URL
                driver.Navigate().GoToUrl(url);

                // Wait and ensure the page loads with Selenium or JavaScript Executor if needed

                // Fetch the page HTML using HtmlWeb or Selenium's PageSource
                var web = new HtmlWeb();
                var doc = web.Load(driver.PageSource);

                // Use HtmlAgilityPack and XPath to extract data
                var nameNode = doc.DocumentNode.SelectSingleNode("//div[@class='name']");
                var phoneNode = doc.DocumentNode.SelectSingleNode("//div[@class='phone']");
                var addressNode = doc.DocumentNode.SelectSingleNode("//div[@class='address']");

                // Extracting data using the found nodes
                entry.Name = nameNode?.InnerText.Trim();
                entry.PrimaryPhone = phoneNode?.InnerText.Trim();
                entry.PrimaryAddress = addressNode?.InnerText.Trim();
                entry.AdditionalPhones = new List<string>(); // Fill this as needed
                entry.AdditionalAddresses = new List<string>(); // Fill this as needed
                entry.Comments = string.Empty; // Placeholder for comments

                // Optionally: Use Regex to extract information from the page source
                // E.g., string pageSource = driver.PageSource or doc.DocumentNode.InnerHtml
                // Then match with your Regex

            }
            catch (Exception e)
            {
                // Handle or log exception
                Console.WriteLine($"Error occurred: {e.Message}");
            }
            finally
            {
                driver.Quit(); // Ensure resources are released
            }

            return entry; // Return the constructed entry
        }
    }

    public interface IWebsiteScraper
    {
        Task<PhonebookEntry> Scrape(string url);
    }

            var entry = new PhonebookEntry
            {
                Name = nameNode?.InnerText.Trim(),
                PrimaryPhone = phoneNode?.InnerText.Trim(),
                PrimaryAddress = addressNode?.InnerText.Trim(),
                AdditionalPhones = new List<string>(), // Logic to fill this
                AdditionalAddresses = new List<string>(), // Logic to fill this
                Comments = string.Empty // Placeholder for comments


            };

            // Example of using Regex within Scrape method
            string pageSource = doc.DocumentNode.InnerHtml;
            var phoneMatch = phoneRegex.Match(pageSource);
            var nameMatch = nameRegex.Match(pageSource);
            var addressMatch = addressRegex.Match(pageSource);
            var urlMatch = urlRegex.Match(pageSource);


            // TODO: Logic to handle additional phone numbers, addresses, and duplicate entries

            return new PhonebookEntry(); // Constructed with the extracted data
        }
    }
}