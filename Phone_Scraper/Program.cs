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
using OpenQA.Selenium.DevTools;
using Phone_Scraper;


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
                
                // Assuming you've set up the path to your user_agents.json file
                string userAgentsFilePath = Path.Combine(Directory.GetCurrentDirectory(), "Utility", "user_agents.json");

                // Load the user agents database using the UserAgents class
                var userAgentsDb = UserAgents.LoadUserAgents(userAgentsFilePath);

                foreach (var uaInfo in userAgentsDb.UserAgents)
                {
                    Console.WriteLine($"OS: {uaInfo.OperatingSystem}, Regex: {uaInfo.Regex}");
                    // And so on for other details...
                }

                using (var driver = new ChromeDriver(driverPath, options))
                {
                    // Initialize the scraper with the driver
                    scraper = new Scraper(driver);
                    driver.Navigate().GoToUrl("https://www.usphonebook.com/");

                    // ... existing code ...

                    // Correct the call to match the updated StartScraping method signature
                    await scraper.StartScraping(cloudEvader: cloudEvader);

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
                   // scraper.CloseDriver();
                }
            }
        }

        private static bool IsConfirmationReceived()
        {
            // Implement your logic to check for confirmation here
            // Return true when confirmation is received, false otherwise
            // You can check for an element, text, or any other criteria on https://pixelscan.net/
            return true; // Replace with your actual logic
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