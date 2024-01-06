using HtmlAgilityPack;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using static Phone_Scraper.Scraper;
using Phone_Scraper.Utility;
using System.Net;
using System.Security.Cryptography.X509Certificates;

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

        private async Task<string> MakeHttpRequest(string requestUri)
        {
            try
            {
                using (HttpClient httpClient = new HttpClient())
                {
                    // Send an HTTP GET request
                    HttpResponseMessage response = await httpClient.GetAsync(requestUri);

                    if (response.IsSuccessStatusCode)
                    {
                        // Read the response body if the request was successful
                        string responseBody = await response.Content.ReadAsStringAsync();
                        return responseBody;
                    }
                    else
                    {
                        // Log the status code if the request failed
                        Console.WriteLine($"HTTP request failed with status code {response.StatusCode}");
                        return null;
                    }
                }
            }
            catch (HttpRequestException ex)
            {
                // Log any HttpRequestExceptions that occur
                Console.WriteLine($"HTTP request error: {ex.Message}");
                return null;
            }
            catch (Exception ex)
            {
                // Log any other exceptions that occur
                Console.WriteLine($"An error occurred during HTTP request: {ex.Message}");
                return null;
            }
        }

        private void ExtractLinks(string pageUrl)
        {
            try
            {
                // Navigate to the page
                driver.Navigate().GoToUrl(pageUrl);

                // Find all anchor tags on the page
                var links = driver.FindElements(By.TagName("a"));

                foreach (var link in links)
                {
                    // Extract the href attribute from each link
                    string href = link.GetAttribute("href");

                    // Perform a simple check to ensure it's an absolute URL and not a fragment or JavaScript code
                    if (!string.IsNullOrEmpty(href) && Uri.IsWellFormedUriString(href, UriKind.Absolute))
                    {
                        string absoluteUrl = href;

                        // Optional: You might want to ensure it's not already visited or processed
                        if (!visitedUrls.Contains(absoluteUrl))
                        {
                            // Enqueue the URL for future scraping
                            urlsToCrawl.Enqueue(absoluteUrl);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error occurred while extracting links from {pageUrl}: {e.Message}");
            }
        }


        public async Task StartScraping(IEnumerable<string> seedUrls)
        {
            WebClient client = CloudflareEvader.CreateBypassedWebClient("Https://usphonebook.com/");
            if (client == null)
            {
                Console.WriteLine("Failed to create a bypassed WebClient. Cloudflare might have blocked the request, or there might be an issue with the CloudflareEvader.");
                return;
            }
            try
            {
                client = CloudflareEvader.CreateBypassedWebClient("Https://usphonebook.com/");
                if (client == null)
                {
                    Console.WriteLine("Cloudflare bypass failed. WebClient is null.");
                    return; // Exit if the client is not correctly initialized
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error initializing Cloudflare bypass: {ex.Message}");
                return; // Exit if there's an error during initialization
            }
            Console.WriteLine("Solved! We're clear to go");

            // Iterate through the seed URLs
            foreach (var seedUrl in seedUrls)
            {
                urlsToCrawl.Enqueue(seedUrl);
            }

            while (urlsToCrawl.Count > 0)
            {
                string currentUrl = urlsToCrawl.Dequeue();
                if (visitedUrls.Contains(currentUrl)) continue; // Skip the URL if it's been visited

                visitedUrls.Add(currentUrl); // Mark URLs as visited 

                // Extract firstname, lastname, & Unique ID from current url
                var urlSegments = new Uri(currentUrl).Segments;
                if (urlSegments.Length >= 4)
                {
                    string firstname = urlSegments[1].Trim('/');
                    string lastname = urlSegments[2].Trim('/');
                    string uniqueid = urlSegments[3].Trim('/');

                    // Generate URL dynamically
                    string baseUrl = "https://www.usphonebook.com/{0}-{1}/{2}";
                    string url = string.Format(baseUrl, firstname, lastname, uniqueid);

                    // Use URL variable in the HTTP request or the scraping logic
                    string responseContent = await client.DownloadStringTaskAsync(url);

                    if (!string.IsNullOrEmpty(responseContent))
                    {
                        Console.WriteLine(responseContent); // Process responses as they come
                    }

                    await Scrape(currentUrl); // Scrape the current URL
                    ExtractLinks(currentUrl); // Extract new links from the current page and add them to the queue
                }
            }
            driver.Quit();
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

        private async Task<Task> ExtractAdditionalDetailsAsync(string pageSource, PhonebookEntry entry)
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
            return Task.CompletedTask;
        }
    }
}