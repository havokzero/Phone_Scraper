using System.Collections.Generic;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using Phone_Scraper.Utility;
using System.Net.Http;
using System.IO;
using Newtonsoft.Json.Linq;

public class UserAgentRule
{
    public string Regex { get; set; }
    public string Ot { get; set; } // Change to Ot to match JSON
}

public class UserAgentInfo
{
    public string Regex { get; set; }
    public string On { get; set; } // Change to On to match JSON
    public string Ov { get; set; } // Change to Ov to match JSON
    public string Dc { get; set; } // Change to Dc to match JSON
    public List<UserAgentRule> Rules { get; set; } = new List<UserAgentRule>();
}

public class UserAgentDatabase
{
    public List<UserAgentInfo> UserAgents { get; set; } = new List<UserAgentInfo>();
}

namespace Phone_Scraper.Utility
{
    public class UserAgents
    {
        public static UserAgentDatabase LoadUserAgents(string filePath)
        {
            string jsonText = File.ReadAllText(filePath);
            return JsonConvert.DeserializeObject<UserAgentDatabase>(jsonText);
        }

        public static void DisplayUserAgents(string filePath)
        {
            // Load user agents from file
            var database = LoadUserAgents(filePath);

            // Iterate over each UserAgentInfo and its rules, printing details to console
            foreach (var ua in database.UserAgents)
            {
                System.Console.WriteLine($"Regex: {ua.Regex}");
                System.Console.WriteLine($"OS: {ua.On}, Version: {ua.Ov}, Device Category: {ua.Dc}");

                foreach (var rule in ua.Rules)
                {
                    System.Console.WriteLine($"  Rule Regex: {rule.Regex}, OS Type: {rule.Ot}");
                }
            }
        }
    }
}
