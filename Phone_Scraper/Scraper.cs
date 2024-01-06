using HtmlAgilityPack;
using OpenQA.Selenium;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using Phone_Scraper.Utility;
using System.Net;

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

        public Scraper(IWebDriver webDriver)
        {
            driver = webDriver ?? throw new ArgumentNullException(nameof(webDriver));
        }

        public async Task StartScraping(IEnumerable<string> seedUrls)
        {
            WebClient client = CloudflareEvader.CreateBypassedWebClient("Https://usphonebook.com/");
            Console.WriteLine("Solved! We're clear to go");

            foreach (var seedUrl in seedUrls)
            {
                urlsToCrawl.Enqueue(seedUrl);
            }

            while (urlsToCrawl.Count > 0)
            {
                string currentUrl = urlsToCrawl.Dequeue();
                if (!visitedUrls.Contains(currentUrl))
                {
                    visitedUrls.Add(currentUrl);
                    await Scrape(currentUrl);
                    ExtractLinks(currentUrl);
                }
            }
            driver.Quit();
        }

        public async Task<PhonebookEntry> Scrape(string url)
        {
            var entry = new PhonebookEntry();
            try
            {
                driver.Navigate().GoToUrl(url);
                var doc = new HtmlWeb().Load(driver.PageSource);

                // Your scraping logic here

                // Example of extracting name, phone, address using XPath
                var nameNode = doc.DocumentNode.SelectSingleNode("//div[@class='name']");
                var phoneNode = doc.DocumentNode.SelectSingleNode("//div[@class='phone']");
                var addressNode = doc.DocumentNode.SelectSingleNode("//div[@class='address']");

                // Assigning primary details to entry
                entry.Name = nameNode?.InnerText.Trim();
                entry.PrimaryPhone = phoneNode?.InnerText.Trim();
                entry.PrimaryAddress = addressNode?.InnerText.Trim();

                // Initialize additional details
                entry.AdditionalPhones = new List<string>();
                entry.AdditionalAddresses = new List<string>();
                entry.Comments = string.Empty;

                // Extracting additional details
                await ExtractAdditionalDetailsAsync(doc.DocumentNode.InnerHtml, entry);
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error occurred while scraping {url}: {e.Message}");
            }

            return entry;
        }

        private void ExtractLinks(string currentUrl)
        {
            try
            {
                var links = driver.FindElements(By.XPath("//a[contains(@href, '/')]"));
                foreach (var link in links)
                {
                    string href = link.GetAttribute("href");
                    if (href.Length <= 2000)
                    {
                        string absoluteUrl = new Uri(new Uri("https://www.usphonebook.com"), href).AbsoluteUri;
                        if (absoluteUrl.StartsWith("https://www.usphonebook.com") && !visitedUrls.Contains(absoluteUrl))
                        {
                            urlsToCrawl.Enqueue(absoluteUrl);
                        }
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
                if (!string.IsNullOrEmpty(phone) && (entry.AdditionalPhones == null || !entry.AdditionalPhones.Contains(phone)))
                {
                    if (entry.AdditionalPhones == null) entry.AdditionalPhones = new List<string>();
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
                    entry.Comments = entry.Comments == null ? name : entry.Comments + "; " + name;
                }
            }

            // Extracting additional addresses
            var addressMatches = addressRegex.Matches(pageSource);
            foreach (Match match in addressMatches)
            {
                var address = match.Value.Trim();
                if (!string.IsNullOrEmpty(address) && (entry.AdditionalAddresses == null || !entry.AdditionalAddresses.Contains(address)))
                {
                    if (entry.AdditionalAddresses == null) entry.AdditionalAddresses = new List<string>();
                    entry.AdditionalAddresses.Add(address);
                }
            }
            // Since this method is async, it's already returning a Task, no need to return anything explicitly.
        }
    }
}
