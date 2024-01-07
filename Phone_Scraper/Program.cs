using Phone_Scraper.Utility;
using OpenQA.Selenium.Chrome;
using System.Net.Http;
using System;

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

                using (var driver = new ChromeDriver(driverPath, options))
                {
                    scraper = new Scraper(driver);
                    driver.Navigate().GoToUrl("https://www.usphonebook.com/");

                    await scraper.StartScraping(cloudEvader: cloudEvader);

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

        private static bool IsConfirmationReceived()
        {
            // Dummy implementation - replace with your actual logic
            return true;
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
