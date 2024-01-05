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
    // Ensure IWebsiteScraper is defined in the same or accessible namespace
    public interface IWebsiteScraper
    {
        Task<PhonebookEntry> Scrape(string url);
    }

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
            //driver = webDriver ?? throw new ArgumentNullException(nameof(webDriver));
            // Initialize the WebDriver here (e.g., ChromeDriver)
            driver = new ChromeDriver();
        }

        public async Task<PhonebookEntry> Scrape(string url)
        {
            var entry = new PhonebookEntry();

            try
            {
                driver.Navigate().GoToUrl(url);
                // Implement appropriate wait here (implicit, explicit, or FluentWait)

                var doc = new HtmlWeb().Load(driver.PageSource);

                // Extract primary details using XPath
                var nameNode = doc.DocumentNode.SelectSingleNode("//div[@class='name']");
                var phoneNode = doc.DocumentNode.SelectSingleNode("//div[@class='phone']");
                var addressNode = doc.DocumentNode.SelectSingleNode("//div[@class='address']");

                // Assign primary details to entry
                entry.Name = nameNode?.InnerText.Trim();
                entry.PrimaryPhone = phoneNode?.InnerText.Trim();
                entry.PrimaryAddress = addressNode?.InnerText.Trim();
                entry.AdditionalPhones = new List<string>();
                entry.AdditionalAddresses = new List<string>();
                entry.Comments = string.Empty;

                // Extract additional details using the private method
                await ExtractAdditionalDetails(doc.DocumentNode.InnerHtml, entry);
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error occurred: {e.Message}");
            }
            finally
            {
                driver.Quit(); // Ensure resources are released
            }

            return entry;
        }

        private void ExtractAdditionalDetails(string pageSource, PhonebookEntry entry)
        {
            // Extracting additional phone numbers
            var phoneMatches = phoneRegex.Matches(pageSource);
            foreach (Match match in phoneMatches)
            {
                var phone = match.Value.Trim();
                if (!string.IsNullOrEmpty(phone) && !entry.AdditionalPhones.Contains(phone))
                {
                    entry.AdditionalPhones.Add(phone);
                }
            }


            // Extracting additional names, if necessary
            var nameMatches = nameRegex.Matches(pageSource);
            foreach (Match match in nameMatches)
            {
                var name = match.Groups[2].Value.Trim();
                if (!string.IsNullOrEmpty(name) && name != entry.Name)
                {
                    // Add to comments or as additional names
                    entry.Comments += name + "; "; // Customize as needed
                }
            }

            // Extracting additional addresses
            var addressMatches = addressRegex.Matches(pageSource);
            foreach (Match match in addressMatches)
            {
                var address = match.Value.Trim();
                if (!string.IsNullOrEmpty(address) && !entry.AdditionalAddresses.Contains(address))
                {
                    entry.AdditionalAddresses.Add(address);
                }
            }
        }
    }
}

 /*   public interface IWebsiteScraper
    {
        Task<PhonebookEntry> Scrape(string url);
    }

    // Define PhonebookEntry class here or in a separate file as you have it
        public class PhonebookEntry
{
        public string Name { get; set; }
        public string PrimaryPhone { get; set; }
        public string PrimaryAddress { get; set; }
        public List<string> AdditionalPhones { get; set; }
        public List<string> AdditionalAddresses { get; set; }
        public string Comments { get; set; }
        // Add other properties or methods as necessary
}
 */