using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using Phone_Scraper;

namespace Phone_Scraper
{
    public class Program
    {
        // Static HttpClient shared across all requests
        private static readonly HttpClient httpClient = new HttpClient();

        public static async Task Main(string[] args)
        {
            // Set up the Chrome WebDriver path
            string driverPath = Path.Combine(Directory.GetCurrentDirectory(), "Driver");

            // Initialize the Chrome WebDriver
            var driver = new ChromeDriver(driverPath);

            // Initialize the scraper with the driver
            var scraper = new Scraper(driver);

            // Define the starting URLs for the scraper
            List<string> seedUrls = new List<string>
            {
                "https://www.usphonebook.com/john-mcdonald/U3YTM3cTN0gTMxcTN3MTM2kTM50yR",
                "https://www.usphonebook.com/john-mcdonald/UzYDMwUzNzkzM3kDN4QjMyUzNx0yR",
                "https://www.usphonebook.com/linda-mcdonald/UwEjM0gDMzYzN1gzM4ITN0IzM20yR",
                "https://www.usphonebook.com/david-sharpe/UMDO4MzNyQTM5YDMxUDN3gTOzEzR"
            };

            try
            {
                // Start the scraping process with the seed URLs
                await scraper.StartScraping(seedUrls);
            }
            catch (Exception e)
            {
                // Log any exceptions that occur during the scraping process
                Console.WriteLine($"Error occurred in the scraping process: {e.Message}");
            }
            // No need for finally block to quit driver, as Scraper.cs handles it
        }

        // A separate method for HTTP requests for better control and error handling
        private static async Task MakeHttpRequest(string requestUri)
        {
            try
            {
                // Send an HTTP GET request
                HttpResponseMessage response = await httpClient.GetAsync(requestUri);

                if (response.IsSuccessStatusCode)
                {
                    // Read the response body if the request was successful
                    string responseBody = await response.Content.ReadAsStringAsync();
                    Console.WriteLine(responseBody); // or process the response as needed
                }
                else
                {
                    // Log the status code if the request failed
                    Console.WriteLine($"HTTP request failed with status code {response.StatusCode}");
                }
            }
            catch (ObjectDisposedException ode)
            {
                // Log any ObjectDisposedExceptions if they occur
                Console.WriteLine("HttpClient has been disposed: " + ode.Message);
            }
            catch (Exception ex)
            {
                // Log any other exceptions that occur
                Console.WriteLine($"An error occurred during HTTP request: {ex.Message}");
            }
        }
    }
}
