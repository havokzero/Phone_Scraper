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
    public interface IWebsiteScraper
    {
        Task StartScraping(IEnumerable<string> seedUrls);
        Task<PhonebookEntry> Scrape(string url);
    }

    public class Scraper : IWebsiteScraper
    {
        private IWebDriver driver;
        private HashSet<string> visitedUrls = new HashSet<string>();
        private Queue<string> urlsToCrawl = new Queue<string>();
       

        // Define your Regex patterns
        private static readonly Regex phoneRegex = new Regex(@"(\+?[1-9][0-9]{0,2}[\s\(\)\-\.\,\/\|]*)?(\(?\d{3}\)?[\s\-\.\,\/\|]*\d{3}[\s\-\.\,\/\|]*\d{4})");
        private static readonly Regex nameRegex = new Regex(@"(Name:|Name\s?:)\s?(.*?)($|\n)");
        private static readonly Regex addressRegex = new Regex(@"\d{1,5}\s\w+\s\w*(?:\s\w+)?\s(?:Avenue|Lane|Road|Boulevard|Drive|Street|Ave|Dr|Rd|Blvd|Ln|St)\.?");
        private static readonly Regex urlRegex = new Regex(@"https?:\/\/\S+");

        public Scraper(IWebDriver webDriver)
        {
            driver = webDriver ?? throw new ArgumentNullException(nameof(webDriver));
        }

        public async Task StartScraping(IEnumerable<string> seedUrls)
        {
            // Enqueue the seed URLs to start the crawling process
            foreach (var url in seedUrls)
            {
                urlsToCrawl.Enqueue(url);
            }

            // Continue processing until there are no more URLs to crawl
            while (urlsToCrawl.Count > 0)
            {
                string currentUrl = urlsToCrawl.Dequeue();
                if (visitedUrls.Contains(currentUrl)) continue; // Skip the URL if it's already been visited

                visitedUrls.Add(currentUrl); // Mark the URL as visited
                await Scrape(currentUrl); // Scrape the current URL

                ExtractLinks(); // Extract new links from the current page and add them to the queue
            }

            driver.Quit(); // Quit the driver once done with all URLs
        }

        public async Task<PhonebookEntry> Scrape(string url)
        {
            var entry = new PhonebookEntry();

            try
            {
                // Navigate to the URL
                driver.Navigate().GoToUrl(url);

                // Load the page source into HtmlDocument
                var doc = new HtmlWeb().Load(driver.PageSource);

                // The rest of the scraping logic to populate the entry goes here

                // For example, extracting name, phone, address, etc. from the page
                var urlSegments = new Uri(url).Segments;
                if (urlSegments.Length >= 3)
                {
                    // The name is usually the segment at index 1, and the random characters at index 2
                    string name = urlSegments[1].Trim('/');
                    string randomCharacters = urlSegments[2].Trim('/');

                    // Assign name and random characters to entry
                    entry.Name = name;
                    entry.RandomCharacters = randomCharacters;
                }

                // Extract primary details using XPath
                var nameNode = doc.DocumentNode.SelectSingleNode("//div[@class='name']");
                var phoneNode = doc.DocumentNode.SelectSingleNode("//div[@class='phone']");
                var addressNode = doc.DocumentNode.SelectSingleNode("//div[@class='address']");

                // Assign primary details to entry
                entry.Name = nameNode?.InnerText.Trim();
                entry.PrimaryPhone = phoneNode?.InnerText.Trim();
                entry.PrimaryAddress = addressNode?.InnerText.Trim();

                // Initialize additional details
                entry.AdditionalPhones = new List<string>();
                entry.AdditionalAddresses = new List<string>();
                entry.Comments = string.Empty;

                // Extract additional details using the private method
                await ExtractAdditionalDetailsAsync(doc.DocumentNode.InnerHtml, entry);
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error occurred while scraping {url}: {e.Message}");
            }

            return entry;
        }

        private void ExtractLinks()
        {
            try
            {
                var links = driver.FindElements(By.XPath("//a[contains(@href, '/')]"));
                foreach (var link in links)
                {
                    string href = link.GetAttribute("href");
                    if (href.Length > 2000) continue; // Skip overly long URLs

                    string absoluteUrl = new Uri(new Uri("https://www.usphonebook.com"), href).AbsoluteUri;
                    if (absoluteUrl.StartsWith("https://www.usphonebook.com") && !visitedUrls.Contains(absoluteUrl))
                    {
                        urlsToCrawl.Enqueue(absoluteUrl);
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error occurred while extracting links: {e.Message}");
            }
        }

        private async Task ExtractAdditionalDetailsAsync(string pageSource, PhonebookEntry entry)
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
