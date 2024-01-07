using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using Leaf.xNet;

namespace Phone_Scraper.Utility
{
    public class CloudEvader
    {
        private static readonly HttpClient httpClient = new HttpClient();

        public static async Task<HttpClient> CreateBypassedWebClient(string url)
        {
            var JSEngine = new Jint.Engine(); // JavaScript engine for computing the challenge answer

            // Set basic headers for the HttpClient
            httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; WOW64; rv:40.0) Gecko/20100101 Firefox/40.0");

            var uri = new Uri(url);
            try
            {
                var initialResponse = await httpClient.GetAsync(uri);
                string initialHtml = await initialResponse.Content.ReadAsStringAsync();

                if (IsChallengePage(initialHtml))
                {
                    // If it's a challenge page, solve it
                    string challengeAnswer = SolveChallenge(initialHtml, uri.Host);
                    long solvedAnswer = long.Parse(JSEngine.Execute(challengeAnswer).GetCompletionValue().ToString());

                    // Make a request to validate the challenge answer
                    await ValidateChallenge(solvedAnswer, uri);

                    // Return the modified HttpClient with the appropriate cookies and headers set
                    return httpClient;
                }

                // If not a challenge page, return the HttpClient as is
                return httpClient;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
                return null; // Return null on error
            }
        }

        public static bool IsCloudflareChallenge(string responseContent, HttpWebResponse response = null)
        {
            try
            {
                // Check if the response content contains a known Cloudflare challenge element
                bool isChallenge = responseContent.Contains("captcha-image");

                // Check for specific status codes, page content, or other indicators
                if (response != null)
                {
                    if (response.StatusCode == System.Net.HttpStatusCode.ServiceUnavailable)
                    {
                        // Additional checks can be made here
                        return true;
                    }

                    // Check HTTP headers
                    if (response.Headers["Server"] == "cloudflare" || response.Headers["CF-RAY"] != null)
                    {
                        return true;
                    }

                    // Add more checks based on headers, URL, or other indicators
                }

                // You can add more checks here based on your specific requirements

                return isChallenge;
            }
            catch (Exception ex)
            {
                // Handle any exceptions here if needed
                Console.WriteLine($"Error in IsCloudflareChallenge: {ex.Message}");
                return false; // Return false in case of an exception
            }
        }

        private static bool IsChallengePage(string htmlContent)
        {
            // Check if the HTML content contains typical Cloudflare challenge elements or patterns
            // You may need to customize this logic based on the structure of challenge pages you encounter

            // Check for the presence of a <form> element with a specific action attribute
            if (htmlContent.Contains("<form id=\"challenge-form\" action=\"/cdn-cgi/l/chk_jschl\" method=\"get\">"))
            {
                return true;
            }

            // Check for the presence of a JavaScript challenge script
            if (htmlContent.Contains("name=\"jschl_vc\"") && htmlContent.Contains("name=\"pass\""))
            {
                return true;
            }

            // Check for the presence of a challenge message
            if (htmlContent.Contains("Please complete the security check to access"))
            {
                return true;
            }

            // Check for the presence of a captcha element
            if (htmlContent.Contains("class=\"g-recaptcha\""))
            {
                return true;
            }

            // Check for other patterns or elements specific to challenge pages
            // For example, you can check for the presence of additional scripts or challenge-specific CSS classes

            // If none of the checks match, it's not a challenge page
            return false;
        }

        private static string SolveChallenge(string challengePageHtml, string host)
        {
            // Implement your logic to solve the challenge here, return the answer as a string
            // You can use the code you provided earlier
            // Extracting Cloudflare's anti-bot challenge values
            var challenge = Regex.Match(challengePageHtml, "name=\"jschl_vc\" value=\"(\\w+)\"").Groups[1].Value;
            var challengePass = Regex.Match(challengePageHtml, "name=\"pass\" value=\"(.+?)\"").Groups[1].Value;
            var builder = Regex.Match(challengePageHtml, @"setTimeout\(function\(\){\s+(var t,r,a,f.+?\r?\n[\s\S]+?a\.value =.+?)\r?\n").Groups[1].Value;

            // Preparing the builder string for execution
            builder = Regex.Replace(builder, @"a\.value =(.+?) \+ .+?;", "$1");
            builder = Regex.Replace(builder, @"\s{3,}[a-z](?: = |\.).+", "");
            builder = Regex.Replace(builder, @"[\n\\']", "");

            // Return the solved challenge answer
            return $"{challenge} + {builder} + {challengePass}";
        }

        private static async Task ValidateChallenge(long solvedAnswer, Uri uri)
        {
            // Implement your validation logic here, using the provided solvedAnswer
            // Construct the validation URL with the solved challenge answer
            string validationUrl = $"{uri.Scheme}://{uri.Host}/cdn-cgi/l/chk_jschl";

            // Construct the query parameters
            var query = HttpUtility.ParseQueryString(string.Empty);
            query["jschl_vc"] = "Extracted_Value"; // The jschl_vc value extracted from the challenge page
            query["pass"] = "Extracted_Value"; // The pass value extracted from the challenge page
            query["jschl_answer"] = solvedAnswer.ToString(); // The solved answer

            // Append the query to the validation URL
            var validationUriBuilder = new UriBuilder(validationUrl)
            {
                Query = query.ToString()
            };

            // Wait a few seconds to mimic browser wait time
            await Task.Delay(4000);

            // Send the validation request
            var validationResponse = await httpClient.GetAsync(validationUriBuilder.Uri);

            // Read and set the cookies from the validation response if needed
            // Typically Cloudflare sets a clearance cookie upon successful JavaScript challenge validation
        }

        // Rest of the cookie handling and utility methods would go here
        // GetAllCookiesFromHeader, ConvertCookieHeaderToArrayList, ConvertCookieArraysToCookieCollection, etc.
    }
}


    
