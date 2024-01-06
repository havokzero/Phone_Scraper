using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using System.Threading;
using System.IO;
using Jint;

namespace Phone_Scraper.Utility
{
    public class CloudflareEvader
    {
        /// <summary>
        /// Tries to return a webclient with the neccessary cookies installed to do requests for a cloudflare protected website.
        /// </summary>
        /// <param name="url">The page which is behind cloudflare's anti-dDoS protection</param>
        /// <returns>A WebClient object or null on failure</returns>
        public static WebClient CreateBypassedWebClient(string url)
        {
            var JSEngine = new Jint.Engine(); // Use this JavaScript engine to compute the result.

            var uri = new Uri(url);
            HttpWebRequest req = (HttpWebRequest)WebRequest.Create(uri);
            req.UserAgent = "Mozilla/5.0 (Windows NT 10.0; WOW64; rv:40.0) Gecko/20100101 Firefox/40.0";

            // Declare 'solved' at the beginning of the method to ensure it is accessible throughout.
            long solved = 0;

            try
            {
                using (var res = req.GetResponse())
                {
                    string html = new StreamReader(res.GetResponseStream()).ReadToEnd();
                    // If you've got the page, then no need for Cloudflare bypass
                    return new WebClient(); // Consider returning a more appropriate response or continue solving the challenge if needed
                }
            }
            catch (WebException ex)
            {
                if (ex.Response == null) return null; // Ensure response is not null

                string html = new StreamReader(ex.Response.GetResponseStream()).ReadToEnd();
                var cookieContainer = new CookieContainer();

                if (ex.Response.Headers["Set-Cookie"] != null)
                {
                    var initialCookies = GetAllCookiesFromHeader(ex.Response.Headers["Set-Cookie"], uri.Host);
                    foreach (Cookie cookie in initialCookies)
                    {
                        cookieContainer.Add(cookie);
                    }
                }
                else
                {
                    return null; // If no cookies, can't proceed
                }

                // Extracting Cloudflare's anti-bot challenge values
                var challenge = Regex.Match(html, "name=\"jschl_vc\" value=\"(\\w+)\"").Groups[1].Value;
                var challengePass = Regex.Match(html, "name=\"pass\" value=\"(.+?)\"").Groups[1].Value;
                var builder = Regex.Match(html, @"setTimeout\(function\(\){\s+(var t,r,a,f.+?\r?\n[\s\S]+?a\.value =.+?)\r?\n").Groups[1].Value;

                // Preparing the builder string for execution
                builder = Regex.Replace(builder, @"a\.value =(.+?) \+ .+?;", "$1");
                builder = Regex.Replace(builder, @"\s{3,}[a-z](?: = |\.).+", "");
                builder = Regex.Replace(builder, @"[\n\\']", "");

                // Executing the JavaScript challenge and getting the result
                var result = JSEngine.Execute(builder).GetCompletionValue();
                if (result != null && result.IsNumber())
                {
                    solved = long.Parse(result.ToObject().ToString());
                    solved += uri.Host.Length; // Add the length of the domain to it.
                }
                else
                {
                    Console.WriteLine("Failed to execute JS or JS did not return a number.");
                    return null;
                }

                // Preparing for the second request to validate the challenge answer
                string cookieUrl = $"{uri.Scheme}://{uri.Host}/cdn-cgi/l/chk_jschl";
                var uriBuilder = new UriBuilder(cookieUrl);
                var query = HttpUtility.ParseQueryString(uriBuilder.Query);
                query["jschl_vc"] = challenge;
                query["jschl_answer"] = solved.ToString();
                query["pass"] = challengePass;
                uriBuilder.Query = query.ToString();

                HttpWebRequest cookieReq = (HttpWebRequest)WebRequest.Create(uriBuilder.Uri);
                cookieReq.AllowAutoRedirect = false;
                cookieReq.CookieContainer = cookieContainer;
                cookieReq.Referer = url;
                cookieReq.UserAgent = req.UserAgent;

                // Making the request and receiving the response containing the clearance cookies
                using (var cookieResp = (HttpWebResponse)cookieReq.GetResponse())
                {
                    if (cookieResp.Cookies.Count > 0)
                    {
                        foreach (Cookie cookie in cookieResp.Cookies)
                        {
                            cookieContainer.Add(cookie);
                        }
                    }
                    else if (cookieResp.Headers["Set-Cookie"] != null)
                    {
                        var cookiesParsed = GetAllCookiesFromHeader(cookieResp.Headers["Set-Cookie"], uri.Host);
                        foreach (Cookie cookie in cookiesParsed)
                        {
                            cookieContainer.Add(cookie);
                        }
                    }
                    else
                    {
                        return null; // If no security clearance, can't proceed
                    }
                }

                // Creating a WebClient with the acquired cookies
                WebClient modedWebClient = new WebClientEx(cookieContainer);
                modedWebClient.Headers.Add("User-Agent", req.UserAgent);
                modedWebClient.Headers.Add("Referer", url);
                return modedWebClient;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An unexpected error occurred: {ex.Message}");
                return null;
            }
        }



        public static CookieCollection GetAllCookiesFromHeader(string strHeader, string strHost)
        {
            ArrayList al = new ArrayList();
            CookieCollection cc = new CookieCollection();
            if (strHeader != string.Empty)
            {
                al = ConvertCookieHeaderToArrayList(strHeader);
                cc = ConvertCookieArraysToCookieCollection(al, strHost);
            }
            return cc;
        }

        private static ArrayList ConvertCookieHeaderToArrayList(string strCookHeader)
        {
            strCookHeader = strCookHeader.Replace("\r", "");
            strCookHeader = strCookHeader.Replace("\n", "");
            string[] strCookTemp = strCookHeader.Split(',');
            ArrayList al = new ArrayList();
            int i = 0;
            int n = strCookTemp.Length;
            while (i < n)
            {
                if (strCookTemp[i].IndexOf("expires=", StringComparison.OrdinalIgnoreCase) > 0)
                {
                    al.Add(strCookTemp[i] + "," + strCookTemp[i + 1]);
                    i = i + 1;
                }
                else
                    al.Add(strCookTemp[i]);
                i = i + 1;
            }
            return al;
        }

        private static CookieCollection ConvertCookieArraysToCookieCollection(ArrayList al, string strHost)
        {
            CookieCollection cc = new CookieCollection();

            int alcount = al.Count;
            string strEachCook;
            string[] strEachCookParts;
            for (int i = 0; i < alcount; i++)
            {
                strEachCook = al[i].ToString();
                strEachCookParts = strEachCook.Split(';');
                int intEachCookPartsCount = strEachCookParts.Length;
                string strCNameAndCValue = string.Empty;
                string strPNameAndPValue = string.Empty;
                string strDNameAndDValue = string.Empty;
                string[] NameValuePairTemp;
                Cookie cookTemp = new Cookie();

                for (int j = 0; j < intEachCookPartsCount; j++)
                {
                    if (j == 0)
                    {
                        strCNameAndCValue = strEachCookParts[j];
                        if (strCNameAndCValue != string.Empty)
                        {
                            int firstEqual = strCNameAndCValue.IndexOf("=");
                            string firstName = strCNameAndCValue.Substring(0, firstEqual);
                            string allValue = strCNameAndCValue.Substring(firstEqual + 1, strCNameAndCValue.Length - (firstEqual + 1));
                            cookTemp.Name = firstName;
                            cookTemp.Value = allValue;
                        }
                        continue;
                    }
                    if (strEachCookParts[j].IndexOf("path", StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        strPNameAndPValue = strEachCookParts[j];
                        if (strPNameAndPValue != string.Empty)
                        {
                            NameValuePairTemp = strPNameAndPValue.Split('=');
                            if (NameValuePairTemp[1] != string.Empty)
                                cookTemp.Path = NameValuePairTemp[1];
                            else
                                cookTemp.Path = "/";
                        }
                        continue;
                    }

                    if (strEachCookParts[j].IndexOf("domain", StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        strPNameAndPValue = strEachCookParts[j];
                        if (strPNameAndPValue != string.Empty)
                        {
                            NameValuePairTemp = strPNameAndPValue.Split('=');

                            if (NameValuePairTemp[1] != string.Empty)
                                cookTemp.Domain = NameValuePairTemp[1];
                            else
                                cookTemp.Domain = strHost;
                        }
                        continue;
                    }
                }

                if (cookTemp.Path == string.Empty)
                    cookTemp.Path = "/";
                if (cookTemp.Domain == string.Empty)
                    cookTemp.Domain = strHost;
                cc.Add(cookTemp);
            }
            return cc;
        }
    }
                    
    public class WebClientEx : WebClient
    {
        public WebClientEx(CookieContainer container)
        {
            this.container = container ?? throw new ArgumentNullException(nameof(container));
        }

        public CookieContainer CookieContainer
        {
            get { return container; }
            set { container = value; }
        }

        private CookieContainer container = new CookieContainer();

        protected override WebRequest GetWebRequest(Uri address)
        {
            WebRequest r = base.GetWebRequest(address);
            var request = r as HttpWebRequest;
            if (request != null)
            {
                request.CookieContainer = container;
            }
            return r;
        }

        protected override WebResponse GetWebResponse(WebRequest request, IAsyncResult result)
        {
            WebResponse response = base.GetWebResponse(request, result);
            ReadCookies(response);
            return response;
        }

        protected override WebResponse GetWebResponse(WebRequest request)
        {
            WebResponse response = base.GetWebResponse(request);
            ReadCookies(response);
            return response;
        }

        private void ReadCookies(WebResponse r)
        {
            var response = r as HttpWebResponse;
            if (response != null)
            {
                CookieCollection cookies = response.Cookies;
                container.Add(cookies);
            }
        }
    }
}

