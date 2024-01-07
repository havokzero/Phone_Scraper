using System;
using System.IO;
using System.Net;
using Phone_Scraper;
using System.Net.Http;
using OpenQA.Selenium;
using System.Threading;
using Newtonsoft.Json.Linq;
using Phone_Scraper.Utility;
using System.Threading.Tasks;
using OpenQA.Selenium.Chrome;
using System.Collections.Generic;
using Newtonsoft.Json;


namespace Phone_Scraper
{
    public class Program
    {
        

        private static readonly HttpClient httpClient = new HttpClient();
        private static CloudEvader cloudEvader;

        public static async Task Main(string[] args)
        {
            int bufferWidth = Math.Max(Console.WindowWidth, 120);
            int bufferHeight = Math.Min(1000, short.MaxValue - 1); // Ensure buffer height is within valid bounds

            Console.SetBufferSize(bufferWidth, bufferHeight);

            // Initialize scraper outside of try block to access it in finally
            Scraper scraper = null;

            try
            {
                // Set up the Chrome WebDriver path
                string driverPath = Path.Combine(Directory.GetCurrentDirectory(), "Driver");
                var options = new ChromeOptions();
                // options.AddArguments("headless"); // Uncomment if you want Chrome to run headless
                using (var driver = new ChromeDriver(driverPath, options))
                {
                    // Initialize the scraper with the driver
                    scraper = new Scraper(driver);

                    // Load seed URLs from JSON file
                    var seedUrls = LoadSeedUrls("SeedUrls.json");

                    // Start the scraping process with the seed URLs
                    await scraper.StartScraping(seedUrls, cloudEvader: cloudEvader);

                    // You might want to add a delay or wait for a user input to observe the browser (for debugging purposes)
                    Console.WriteLine("Scraping completed. Press any key to exit...");
                    Console.ReadKey();
                }
            }
            catch (Exception e)
            {
                // Log any exceptions that occur during the scraping process
                Console.WriteLine($"Error occurred in the scraping process: {e.Message}");
            }
            finally
            {
                // Ensure WebDriver is closed properly
                if (scraper != null)
                {
                    scraper.CloseDriver();
                }
            }
        }

        private static List<string> LoadSeedUrls(string filePath)
        {
            try
            {
                // Read the file as a string
                string jsonText = File.ReadAllText(filePath);

                // Parse the string into a JSON object and convert to list
                var seedUrls = JsonConvert.DeserializeObject<List<string>>(jsonText);
                return seedUrls ?? new List<string>(); // Return empty list if null
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error loading seed URLs from {filePath}: {e.Message}");
                return new List<string>(); // Return empty list on error
            }
        }

        private static async Task MakeHttpRequest(string requestUri)
        {
            try
            {
                HttpResponseMessage response = await httpClient.GetAsync(requestUri);

                if (response.IsSuccessStatusCode)
                {
                    string responseBody = await response.Content.ReadAsStringAsync();
                    Console.WriteLine(responseBody);
                }
                else
                {
                    Console.WriteLine($"HTTP request failed with status code {response.StatusCode}");
                }
            }
            catch (ObjectDisposedException ode)
            {
                Console.WriteLine("HttpClient has been disposed: " + ode.Message);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred during HTTP request: {ex.Message}");
            }
        }
    }
}