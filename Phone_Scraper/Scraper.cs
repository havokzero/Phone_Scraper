﻿using HtmlAgilityPack;
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
using Newtonsoft.Json;
using System.IO;
using System.Net.Http;

namespace Phone_Scraper
{

    public interface IWebsiteScraper
    {
        Task StartScraping(IEnumerable<string> seedUrls);
        Task<PhonebookEntry> Scrape(string url);
    }

    public class Scraper : IWebsiteScraper
    {

        private HttpClient httpClient;
        private IWebDriver driver;
        private HashSet<string> visitedUrls = new HashSet<string>();
        private Queue<string> urlsToCrawl = new Queue<string>();

        public Scraper(IWebDriver webDriver)
        {
            driver = webDriver ?? throw new ArgumentNullException(nameof(webDriver));
            httpClient = new HttpClient();
        }

        // Define your Regex patterns
        private static readonly Regex phoneRegex = new Regex(@"(\+?[1-9][0-9]{0,2}[\s\(\)\-\.\,\/\|]*)?(\(?\d{3}\)?[\s\-\.\,\/\|]*\d{3}[\s\-\.\,\/\|]*\d{4})");
        private static readonly Regex nameRegex = new Regex(@"(Name:|Name\s?:)\s?(.*?)($|\n)");
        private static readonly Regex addressRegex = new Regex(@"\d{1,5}\s\w+\s\w*(?:\s\w+)?\s(?:Avenue|Lane|Road|Boulevard|Drive|Street|Ave|Dr|Rd|Blvd|Ln|St)\.?");
        private static readonly Regex urlRegex = new Regex(@"https?:\/\/\S+", RegexOptions.IgnoreCase);
        private static readonly Regex relativeRegex = new Regex(@"Relatives\s*:\s*(.+?)(?=<\/div>|$)");
        private static readonly Regex associateRegex = new Regex(@"Associates\s*:\s*(.+?)(?=<\/div>|$)");
        private static readonly Regex emailRegex = new Regex(@"^(?!\.)(""([^""\r\\]|\\[""\r\\])*""|([-a-z0-9!#$%&'*+/=?^_`{|}~]|(?<!\.)\.)*)(?<!\.)@[a-z0-9][\w\.-]*[a-z0-9]\.[a-z][a-z\.]*[a-z]$", RegexOptions.IgnoreCase);
        
        // Method to close or dispose of the WebDriver
        public void CloseDriver()
        {
            // If the driver is not null, close and dispose of it
            driver?.Quit();
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

        public static List<string> GetSeedURLs(string filePath)
        {
            string json = File.ReadAllText(filePath);
            var seedUrls = JsonConvert.DeserializeObject<List<string>>(json);
            return seedUrls;
        }

        public async Task StartScraping(IEnumerable<string> seedUrls)
        {
            // Initialize httpClient
            httpClient = new HttpClient();

            // Load seed URLs and iterate through them
            foreach (var seedUrl in seedUrls)
            {
                Console.WriteLine($"Now scraping {seedUrl}");
                urlsToCrawl.Enqueue(seedUrl);
            }
            string responseContent = null;

            while (urlsToCrawl.Count > 0)
            {
                string currentUrl = urlsToCrawl.Dequeue();
                if (visitedUrls.Contains(currentUrl)) continue; // Skip the URL if it's been visited

                visitedUrls.Add(currentUrl); // Mark URLs as visited 
                Console.WriteLine($"Processing: {currentUrl}");

                var urlSegments = new Uri(currentUrl).Segments;
                string urlToScrape = "";

                // Adjusting URL based on its type (name-based or phone number-based)
                if (urlSegments.Length >= 4 && !urlSegments[1].StartsWith("NXX-NXX-XXXX"))
                {
                    // Handle name-based URLs
                    string firstname = urlSegments[1].Trim('/');
                    string lastname = urlSegments[2].Trim('/');
                    string uniqueid = urlSegments[3].Trim('/');
                    string baseUrl = "https://www.usphonebook.com/{0}-{1}/{2}";
                    urlToScrape = string.Format(baseUrl, firstname, lastname, uniqueid);
                }
                else if (urlSegments.Length >= 2 && urlSegments[1].StartsWith("NXX-NXX-XXXX"))
                {
                    // Handle phone number-based URLs
                    urlToScrape = currentUrl; // The current URL is already the full URL in this case
                }

                try
                {
                    // Scrape and Extract new links from the current page
                    var entry = await Scrape(urlToScrape);
                    ExtractLinks(urlToScrape); // Ensure ExtractLinks method is properly handling the URLs

                    // Check if Cloudflare challenge is detected
                    bool isCloudflareChallenge = false;

                    do
                    {
                        // Make the HTTP request using a bypassed WebClient if it's a Cloudflare challenge
                        if (isCloudflareChallenge)
                        {
                            httpClient = await CloudEvader.CreateBypassedWebClient(urlToScrape);
                            if (httpClient == null)
                            {
                                Console.WriteLine("Failed to create a bypassed WebClient. Cloudflare might have blocked the request.");
                                return;
                            }
                        }

                        // Perform the HTTP request
                        HttpResponseMessage response = await httpClient.GetAsync(urlToScrape);
                        responseContent = await response.Content.ReadAsStringAsync();

                        // Check if the response indicates a Cloudflare captcha challenge
                        isCloudflareChallenge = await CloudEvader.IsCloudflareChallenge(responseContent);

                        if (isCloudflareChallenge)
                        {
                            Console.WriteLine("Detected Cloudflare captcha challenge. Retrying with CloudEvader...");
                            // Implement your retry or bypass strategy here.
                        }
                    } while (isCloudflareChallenge);

                    if (!string.IsNullOrEmpty(responseContent))
                    {
                        Console.WriteLine(responseContent); // Process responses as they come
                    }

                    // Scrape the current URL and Extract new links from the current page and add them to the queue
                    await Scrape(urlToScrape);
                    ExtractLinks(urlToScrape); // Ensure ExtractLinks method is properly handling the URLs and adding new ones to the queue
                }
                catch (HttpRequestException ex)
                {
                    // Handle specific web exceptions or retry logic here if needed
                    Console.WriteLine($"Failed to access {urlToScrape}: {ex.Message}");
                }
            }
            driver.Quit(); // Ensure this is the desired behavior as this will close the entire browser session
        }

        // Dummy method to detect Cloudflare's challenge
        private bool IsCloudflareChallenge(WebException webEx)
        {
            // Implement actual logic to detect Cloudflare's challenge page
            // This could involve checking for specific status codes, page content, etc.
            if (webEx.Response is HttpWebResponse response)
            {
                // Often Cloudflare challenge pages are served with a 503 Service Unavailable
                if (response.StatusCode == HttpStatusCode.ServiceUnavailable)
                {
                    // Further checks can be made here, for instance, parsing the response HTML
                    // and looking for specific Cloudflare challenge indicators.
                    // For now, it's a basic check.
                    return true;
                }
            }
            return false;
        }

        public async Task<PhonebookEntry> Scrape(string url)
        {
            var entry = new PhonebookEntry();

            try
            {
                // Navigate to the URL
                driver.Navigate().GoToUrl(url);

                // Load the page source into HtmlDocument
                var doc = new HtmlAgilityPack.HtmlDocument();
                doc.LoadHtml(driver.PageSource);

                // Name and Age
                var nameAndAgeNode = doc.DocumentNode.SelectSingleNode("//h3/span[contains(@class,'fa-user')]");
                if (nameAndAgeNode != null)
                {
                    var nameAndAgeText = nameAndAgeNode.InnerText.Trim();
                    // Assuming format "John Mcdonald 56 years old"
                    var parts = nameAndAgeText.Split(new[] { ' ' }, 3); // Split by space but only into three parts
                    if (parts.Length == 3)
                    {
                        entry.Name = parts[0] + " " + parts[1]; // John Mcdonald
                                                                // Extract age from "56 years old", or similar
                        entry.Age = parts[2].Split(' ')[0]; // 56
                    }
                }

                // Current Address
                var currentAddressNode = doc.DocumentNode.SelectSingleNode("//p[contains(@class,'ls_contacts__text')]");
                if (currentAddressNode != null)
                {
                    entry.CurrentAddress = currentAddressNode.InnerText.Trim();
                }

                // Current Phone Number
                var phoneNumberNode = doc.DocumentNode.SelectSingleNode("//span[@itemprop='telephone']");
                if (phoneNumberNode != null)
                {
                    entry.CurrentPhone = phoneNumberNode.InnerText.Trim();
                }

                // Previous Addresses
                var prevAddressNodes = doc.DocumentNode.SelectNodes("//ul[@class='shown']/li/a");
                if (prevAddressNodes != null)
                {
                    foreach (var node in prevAddressNodes)
                    {
                        entry.PreviousAddresses.Add(node.InnerText.Trim());
                    }
                }

                // Previous Phone Numbers
                var prevPhoneNodes = doc.DocumentNode.SelectNodes("//ul[@class='shown']/li[contains(@itemtype, 'Person')]/a[@itemprop='telephone']");
                if (prevPhoneNodes != null)
                {
                    foreach (var node in prevPhoneNodes)
                    {
                        entry.PreviousPhones.Add(node.InnerText.Trim());
                    }
                }

                // Relatives
                var relativeNodes = doc.DocumentNode.SelectNodes("//div[@class='relative-card']/p/a/span[@itemprop='name']");
                if (relativeNodes != null)
                {
                    foreach (var node in relativeNodes)
                    {
                        entry.Relatives.Add(node.InnerText.Trim());
                    }
                }

                // Associates
                var associateNodes = doc.DocumentNode.SelectNodes("//div[@class='section-relative']/div[@class='relative-card']/p/a/span");
                if (associateNodes != null)
                {
                    foreach (var node in associateNodes)
                    {
                        entry.Associates.Add(node.InnerText.Trim());
                    }
                }

                // Email
                var emailNode = doc.DocumentNode.SelectSingleNode("//a[contains(@href, 'mailto')]");
                if (emailNode != null)
                {
                    entry.Email = emailNode.InnerText.Trim();
                }
                Console.WriteLine($"Scraped data from {url}");
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
                if (!string.IsNullOrEmpty(phone) && !entry.PreviousPhones.Contains(phone))
                {
                    entry.PreviousPhones.Add(phone);
                }
            }

            // Extracting additional addresses
            var addressMatches = addressRegex.Matches(pageSource);
            foreach (Match match in addressMatches)
            {
                var address = match.Value.Trim();
                if (!string.IsNullOrEmpty(address) && !entry.PreviousAddresses.Contains(address))
                {
                    entry.PreviousAddresses.Add(address);
                }
            }

            // Assuming you have similar regex for relatives and associates
            // Extracting additional relatives
            var relativeMatches = relativeRegex.Matches(pageSource);
            foreach (Match match in relativeMatches)
            {
                var relative = match.Value.Trim();
                if (!string.IsNullOrEmpty(relative) && !entry.Relatives.Contains(relative))
                {
                    entry.Relatives.Add(relative);
                }
            }

            // Extracting additional associates
            var associateMatches = associateRegex.Matches(pageSource);
            foreach (Match match in associateMatches)
            {
                var associate = match.Value.Trim();
                if (!string.IsNullOrEmpty(associate) && !entry.Associates.Contains(associate))
                {
                    entry.Associates.Add(associate);
                }
            }
        }
    }
}