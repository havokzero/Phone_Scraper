using HtmlAgilityPack;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace Phone_Scraper
{
    public Scraper(IWebDriver webDriver)
    {
        private IWebDriver driver;
            driver = webDriver;

        // Define your Regex patterns
        private static readonly Regex phoneRegex = new Regex(@"(\+?[1-9][0-9]{0,2}[\s\(\)\-\.\,\/\|]*)?(\(?\d{3}\)?[\s\-\.\,\/\|]*\d{3}[\s\-\.\,\/\|]*\d{4})");
        private static readonly Regex nameRegex = new Regex(@"(Name:|Name\s?:)\s?(.*?)($|\n)");
        private static readonly Regex addressRegex = new Regex(@"\d{1,5}\s\w+\s\w*(?:\s\w+)?\s(?:Avenue|Lane|Road|Boulevard|Drive|Street|Ave|Dr|Rd|Blvd|Ln|St)\.?");
        private static readonly Regex urlRegex = new Regex(@"https?:\/\/\S+");

        public Scraper()
        {
            driver = new ChromeDriver();  // Make sure the ChromeDriver is in your system's PATH
        }

        public interface IWebsiteScraper
        {
            Task<PhonebookEntry> Scrape(string url);
        }

        public async Task<PhonebookEntry> Scrape(string url)
        {
            // Fetch the page HTML using HtmlWeb
            var web = new HtmlWeb();
            var doc = web.Load(url);

            // Use HtmlAgilityPack and XPath to extract data
            // These XPaths are hypothetical; you'll need to determine the actual ones based on your target page's structure
            var nameNode = doc.DocumentNode.SelectSingleNode("//div[@class='name']");
            var phoneNode = doc.DocumentNode.SelectSingleNode("//div[@class='phone']");
            var addressNode = doc.DocumentNode.SelectSingleNode("//div[@class='address']");

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