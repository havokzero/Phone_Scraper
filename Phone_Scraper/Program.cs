using Phone_Scraper.Utility;
using OpenQA.Selenium.Chrome;
using System.Net.Http;
using System;
using System.IO;
using System.Threading.Tasks;
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
            int bufferHeight = Math.Min(1000, short.MaxValue - 1);
            Console.SetBufferSize(bufferWidth, bufferHeight);

            Scraper scraper = null;

            try
            {
                string driverPath = Path.Combine(Directory.GetCurrentDirectory(), "Driver");
                var options = new ChromeOptions();

                // Retrieve a random user agent string from the UserAgents list
                string randomUserAgent = UserAgents.GetRandomUserAgent();
                options.AddArgument($"--user-agent={randomUserAgent}");
                // Enable headless mode
                //options.AddArgument("--headless");

                string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
                string seedUrlsPath = Path.Combine(baseDirectory, "Utility", "SeedUrls.json");

                // Load the seed URLs from the JSON file
                List<string> seedUrls = Scraper.GetSeedURLs(seedUrlsPath);

                using (var driver = new ChromeDriver(driverPath, options))
                {
                    scraper = new Scraper(driver);

                    // Iterate through each seed URL and navigate to it
                    foreach (var seedUrl in seedUrls)
                    {
                        driver.Navigate().GoToUrl(seedUrl);
                        await scraper.StartScraping(cloudEvader: cloudEvader);
                    }

                    Console.WriteLine("Scraping completed. Press any key to exit...");
                    Console.ReadKey();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error occurred in the scraping process: {e.Message}");
            }
            finally
            {
                if (scraper != null)
                {
                    // scraper.CloseDriver(); // Uncomment this if you have a CloseDriver method to close the browser
                }
            }
        }

    }
}
