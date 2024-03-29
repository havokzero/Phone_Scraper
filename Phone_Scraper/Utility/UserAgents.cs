namespace Phone_Scraper.Utility
{
    public class UserAgents
    {
        public static readonly List<string> UserAgentStrings = new List<string>
        {
            "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/88.0.4324.150 Safari/537.36",
            "Mozilla/5.0 (Macintosh; Intel Mac OS X 10_15_7) AppleWebKit/605.1.15 (KHTML, like Gecko) Version/14.0.3 Safari/605.1.15",
            "Mozilla/5.0 (iPad; CPU OS 14_4 like Mac OS X) AppleWebKit/605.1.15 (KHTML, like Gecko) Version/14.0.3 Mobile/15E148 Safari/604.1",
            "Mozilla/5.0 (iPhone; CPU iPhone OS 14_4 like Mac OS X) AppleWebKit/605.1.15 (KHTML, like Gecko) Version/14.0.3 Mobile/15E148 Safari/604.1",
            "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/88.0.4324.104 Safari/537.36 Edg/88.0.705.50",
            "Mozilla/5.0 (Windows NT 10.0; WOW64; Trident/7.0; rv:11.0) like Gecko",
            "Mozilla/5.0 (Macintosh; Intel Mac OS X 10_15_7) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/88.0.4324.96 Safari/537.36",
            "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:85.0) Gecko/20100101 Firefox/85.0",
            "Mozilla/5.0 (Macintosh; Intel Mac OS X 10_15_7; rv:85.0) Gecko/20100101 Firefox/85.0",
            "Mozilla/5.0 (Linux; Android 11; Pixel 5) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/88.0.4324.93 Mobile Safari/537.36",
            "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/100.0.0.0 Safari/537.36",
            "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Firefox/100.0.0.0 Safari/537.36",
            "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Edge/100.0.0.0 Safari/537.36",
            "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Opera/100.0.0.0 Safari/537.36",
            "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:100.0) Gecko/20100101 Firefox/100.0",
            "Mozilla/5.0 (Macintosh; Intel Mac OS X 10_15_7) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/100.0.0.0 Safari/537.36",
            "Mozilla/5.0 (Macintosh; Intel Mac OS X 10_15_7) AppleWebKit/537.36 (KHTML, like Gecko) Firefox/100.0.0.0 Safari/537.36",
            "Mozilla/5.0 (Macintosh; Intel Mac OS X 10_15_7) AppleWebKit/537.36 (KHTML, like Gecko) Edge/100.0.0.0 Safari/537.36",
            "Mozilla/5.0 (Macintosh; Intel Mac OS X 10_15_7) AppleWebKit/537.36 (KHTML, like Gecko) Opera/100.0.0.0 Safari/537.36",
            "Mozilla/5.0 (Macintosh; Intel Mac OS X 10_15_7; rv:100.0) Gecko/20100101 Firefox/100.0",
            "Mozilla/5.0 (Linux; Android 12; Pixel 6 Pro) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/100.0.0.0 Mobile Safari/537.36",
            "Mozilla/5.0 (Linux; Android 12; Pixel 6 Pro) AppleWebKit/537.36 (KHTML, like Gecko) Firefox/100.0.0.0 Mobile Safari/537.36",
            "Mozilla/5.0 (Linux; Android 12; Pixel 6 Pro) AppleWebKit/537.36 (KHTML, like Gecko) Edge/100.0.0.0 Mobile Safari/537.36",
            "Mozilla/5.0 (Linux; Android 12; Pixel 6 Pro) AppleWebKit/537.36 (KHTML, like Gecko) Opera/100.0.0.0 Mobile Safari/537.36",
            "Mozilla/5.0 (Linux; Android 12; Pixel 6 Pro; rv:100.0) Gecko/20100101 Firefox/100.0",
            "Mozilla/5.0 (iPad; CPU OS 12_0 like Mac OS X) AppleWebKit/605.1.15 (KHTML, like Gecko) Version/12.0 Mobile/15E148 Safari/604.1",
            "Mozilla/5.0 (iPhone; CPU iPhone OS 12_0 like Mac OS X) AppleWebKit/605.1.15 (KHTML, like Gecko) Version/12.0 Mobile/15E148 Safari/604.1",
            "Mozilla/5.0 (iPod touch; CPU iPhone OS 12_0 like Mac OS X) AppleWebKit/605.1.15 (KHTML, like Gecko) Version/12.0 Mobile/15E148 Safari/604.1",
            "Mozilla/5.0 (Windows Phone 10) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/100.0.0.0 Mobile Safari/537.36",
            "Mozilla/5.0 (Windows Phone 10) AppleWebKit/537.36 (KHTML, like Gecko) Edge/100.0.0.0 Mobile Safari/537.36",
            "Googlebot/2.1 (+http://www.google.com/bot.html)",
            "Googlebot-Mobile/2.1 (+http://www.google.com/bot.html)",
            "Mozilla/5.0 (compatible; bingbot/2.0; +http://www.bing.com/bingbot.htm)",
            // Additional user agents can be added here as needed
        };

        public static string GetRandomUserAgent()
        {
            var random = new Random();
            int index = random.Next(UserAgentStrings.Count);
            return UserAgentStrings[index];
        }
    }
}


/*
 * public const string SampleUserAgent1 = 
 *                          "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/100.0.0.0 Safari/537.36",
                            "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Firefox/100.0.0.0 Safari/537.36",
                            "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Edge/100.0.0.0 Safari/537.36",
                            "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Opera/100.0.0.0 Safari/537.36",
                            "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:100.0) Gecko/20100101 Firefox/100.0",
                            "Mozilla/5.0 (Macintosh; Intel Mac OS X 10_15_7) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/100.0.0.0 Safari/537.36",
                            "Mozilla/5.0 (Macintosh; Intel Mac OS X 10_15_7) AppleWebKit/537.36 (KHTML, like Gecko) Firefox/100.0.0.0 Safari/537.36",
                            "Mozilla/5.0 (Macintosh; Intel Mac OS X 10_15_7) AppleWebKit/537.36 (KHTML, like Gecko) Edge/100.0.0.0 Safari/537.36",
                            "Mozilla/5.0 (Macintosh; Intel Mac OS X 10_15_7) AppleWebKit/537.36 (KHTML, like Gecko) Opera/100.0.0.0 Safari/537.36",
                            "Mozilla/5.0 (Macintosh; Intel Mac OS X 10_15_7; rv:100.0) Gecko/20100101 Firefox/100.0",
                            "Mozilla/5.0 (Linux; Android 12; Pixel 6 Pro) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/100.0.0.0 Mobile Safari/537.36",
                            "Mozilla/5.0 (Linux; Android 12; Pixel 6 Pro) AppleWebKit/537.36 (KHTML, like Gecko) Firefox/100.0.0.0 Mobile Safari/537.36",
                            "Mozilla/5.0 (Linux; Android 12; Pixel 6 Pro) AppleWebKit/537.36 (KHTML, like Gecko) Edge/100.0.0.0 Mobile Safari/537.36",
                            "Mozilla/5.0 (Linux; Android 12; Pixel 6 Pro) AppleWebKit/537.36 (KHTML, like Gecko) Opera/100.0.0.0 Mobile Safari/537.36",
                            "Mozilla/5.0 (Linux; Android 12; Pixel 6 Pro; rv:100.0) Gecko/20100101 Firefox/100.0",
                            "Mozilla/5.0 (iPad; CPU OS 12_0 like Mac OS X) AppleWebKit/605.1.15 (KHTML, like Gecko) Version/12.0 Mobile/15E148 Safari/604.1",
                            "Mozilla/5.0 (iPhone; CPU iPhone OS 12_0 like Mac OS X) AppleWebKit/605.1.15 (KHTML, like Gecko) Version/12.0 Mobile/15E148 Safari/604.1",
                            "Mozilla/5.0 (iPod touch; CPU iPhone OS 12_0 like Mac OS X) AppleWebKit/605.1.15 (KHTML, like Gecko) Version/12.0 Mobile/15E148 Safari/604.1",
                            "Mozilla/5.0 (Windows Phone 10) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/100.0.0.0 Mobile Safari/537.36",
                            "Mozilla/5.0 (Windows Phone 10) AppleWebKit/537.36 (KHTML, like Gecko) Edge/100.0.0.0 Mobile Safari/537.36",
                            "Googlebot/2.1 (+http://www.google.com/bot.html)",
                            "Googlebot-Mobile/2.1 (+http://www.google.com/bot.html)",
                            "Mozilla/5.0 (compatible; bingbot/2.0; +http://www.bing.com/bingbot.htm)"
 */

// User agent regex patterns
/*public const string WindowsUserAgentRegex = "!Windows NT ([.0-9]+)!i";
public const string OSXUserAgentRegex = "!OS X ([0-9]+)[_.]([0-9]+)!i";
public const string LinuxUserAgentRegex = "!X11|Linux!i";
public const string InternetExplorerUserAgentRegex = "!MSIE ([.0-9]+)!i";
public const string InternetExplorerTridentUserAgentRegex = "!Trident/[.0-9]+;.+rv:([.0-9]+)!i";
public const string FirefoxUserAgentRegex = "!Firefox/([.0-9]+)!i";
public const string ChromeUserAgentRegex = "!Chrome/([.0-9]+)!i";
public const string SafariUserAgentRegex = "!Version/([.0-9]+) Safari/[.0-9]+!i";
public const string EdgeUserAgentRegex = "!Edge/([.0-9]+)!i";
public const string OperaUserAgentRegex = "!OPR/([.0-9]+)!i";
public const string YandexUserAgentRegex = "!YaBrowser/([.0-9]+)!i";
public const string AndroidTabletUserAgentRegex = "!Android ([1456789][0-9]?[.0-9]*)!i";
public const string AndroidTabletChromeRegex = "~Chrome/([.0-9]+)~";
public const string AndroidTabletAndroidBrowserRegex = "~Version/([.0-9]+)~i";
public const string AndroidTabletSamsungBrowserRegex = "~SamsungBrowser/([.0-9]+)~i";
public const string AndroidTabletFirefoxRegex = "~Firefox/([.0-9]+)~i";
public const string AndroidTabletOperaRegex = "~OPR/([.0-9]+)~i";
public const string AndroidTabletMobileRegex = "~Mobile~i";
public const string AndroidTabletAndroidVersionRegex = "!Android (3[.0-9]+)!i";
public const string AndroidTabletAndroidBrowserVersionRegex = "~Version/([.0-9]+)~i";
public const string AndroidTabletChromeVersionRegex = "~Chrome/([.0-9]+)~i";
public const string AndroidMobileVersionRegex = "!Android (2[.0-9]+)!i";
public const string AndroidMobileAndroidBrowserVersionRegex = "~Version/([.0-9]+)~i";
public const string AndroidMobileChromeVersionRegex = "~Chrome/([.0-9]+)~i";
public const string iOSVersionRegex = "!(?:iPad|iPod|iPhone).+OS ([0-9]+)_([0-9]+)!i";
public const string iOSVersionSafariRegex = "!Version/([.0-9]+).+Safari/[.0-9]+!i";
public const string iOSVersionChromeRegex = "!CriOS/([.0-9]+).+Safari/[.0-9]+!i";
public const string iOSVersionFirefoxRegex = "!FxiOS/([.0-9]+).+Safari/[.0-9]+!i";
public const string iOSMobileRegex = "!iPhone|iPod!";
public const string iOSTabletRegex = "!iPad!";
public const string WindowsPhoneRegex = "!Windows Phone (?:OS )?([.0-9]+)!i";
public const string GoogleBotRegex = "!google\\.com/bot!";
public const string GoogleBotMobileRegex = "!Googlebot-Mobile!";
public const string BingBotRegex = "!bing\\.com/bingbot!";
public const string BingBotMobileRegex = "!Phone!";
*/
// User agent info
/*   public static readonly List<UserAgentInfo> UserAgentInfos = new List<UserAgentInfo>
{
   new UserAgentInfo
   {
       Regex = WindowsUserAgentRegex,
       On = "Windows",
       Ov = "$1",
       Dc = "NT"
   },
   new UserAgentInfo
   {
       Regex = OSXUserAgentRegex,
       On = "OS X",
       Ov = "$1.$2",
       Dc = null
   },
   new UserAgentInfo
   {
       Regex = LinuxUserAgentRegex,
       On = "Linux",
       Ov = null,
       Dc = null
   },
   new UserAgentInfo
   {
       Regex = InternetExplorerUserAgentRegex,
       On = "Internet Explorer",
       Ov = "$1",
       Dc = null
   },
   new UserAgentInfo
   {
       Regex = InternetExplorerTridentUserAgentRegex,
       On = "Internet Explorer",
       Ov = "$1",
       Dc = "Trident"
   },
   new UserAgentInfo
   {
       Regex = FirefoxUserAgentRegex,
       On = "Firefox",
       Ov = "$1",
       Dc = null
   },
   new UserAgentInfo
   {
       Regex = ChromeUserAgentRegex,
       On = "Chrome",
       Ov = "$1",
       Dc = null
   },
   new UserAgentInfo
   {
       Regex = SafariUserAgentRegex,
       On = "Safari",
       Ov = "$1",
       Dc = "Version"
   },
   new UserAgentInfo
   {
       Regex = EdgeUserAgentRegex,
       On = "Edge",
       Ov = "$1",
       Dc = null
   },
   new UserAgentInfo
   {
       Regex = OperaUserAgentRegex,
       On = "Opera",
       Ov = "$1",
       Dc = null
   },
   new UserAgentInfo
   {
       Regex = YandexUserAgentRegex,
       On = "Yandex Browser",
       Ov = "$1",
       Dc = null
   },
   new UserAgentInfo
   {
       Regex = AndroidTabletUserAgentRegex,
       On = "Android Tablet",
       Ov = "$1",
       Dc = null
   },
   new UserAgentInfo
   {
       Regex = AndroidTabletChromeRegex,
       On = "Chrome",
       Ov = "$1",
       Dc = null
   },
   new UserAgentInfo
   {
       Regex = AndroidTabletAndroidBrowserRegex,
       On = "Android Browser",
       Ov = "$1",
       Dc = "Version"
   },
   new UserAgentInfo
   {
       Regex = AndroidTabletSamsungBrowserRegex,
       On = "Samsung Browser",
       Ov = "$1",
       Dc = "Version"
   },
   new UserAgentInfo
   {
       Regex = AndroidTabletFirefoxRegex,
       On = "Firefox",
       Ov = "$1",
       Dc = null
   },
   new UserAgentInfo
   {
       Regex = AndroidTabletOperaRegex,
       On = "Opera",
       Ov = "$1",
       Dc = null
   },
   new UserAgentInfo
   {
       Regex = AndroidTabletMobileRegex,
       On = "Mobile",
       Ov = null,
       Dc = null
   },
   new UserAgentInfo
   {
       Regex = AndroidTabletAndroidVersionRegex,
       On = "Android",
       Ov = "$1",
       Dc = null
   },
   new UserAgentInfo
   {
       Regex = AndroidTabletAndroidBrowserVersionRegex,
       On = "Android Browser",
       Ov = "$1",
       Dc = "Version"
   },
   new UserAgentInfo
   {
       Regex = AndroidTabletChromeVersionRegex,
       On = "Chrome",
       Ov = "$1",
       Dc = null
   },
   new UserAgentInfo
   {
       Regex = AndroidMobileVersionRegex,
       On = "Android",
       Ov = "$1",
       Dc = null
   },
   new UserAgentInfo
   {
       Regex = AndroidMobileAndroidBrowserVersionRegex,
       On = "Android Browser",
       Ov = "$1",
       Dc = "Version"
   },
   new UserAgentInfo
   {
       Regex = AndroidMobileChromeVersionRegex,
       On = "Chrome",
       Ov = "$1",
       Dc = null
   },
   new UserAgentInfo
   {
       Regex = iOSVersionRegex,
       On = "iOS",
       Ov = "$1.$2",
       Dc = null
   },
   new UserAgentInfo
   {
       Regex = iOSVersionSafariRegex,
       On = "iOS Safari",
       Ov = "$1",
       Dc = "Version"
   },
   new UserAgentInfo
   {
       Regex = iOSVersionChromeRegex,
       On = "iOS Chrome",
       Ov = "$1",
       Dc = "CriOS"
   },
   new UserAgentInfo
   {
       Regex = iOSVersionFirefoxRegex,
       On = "iOS Firefox",
       Ov = "$1",
       Dc = "FxiOS"
   },
   new UserAgentInfo
   {
       Regex = iOSMobileRegex,
       On = "iOS Mobile",
       Ov = null,
       Dc = null
   },
   new UserAgentInfo
   {
       Regex = iOSTabletRegex,
       On = "iOS Tablet",
       Ov = null,
       Dc = null
   },
   new UserAgentInfo
   {
       Regex = WindowsPhoneRegex,
       On = "Windows Phone",
       Ov = "$1",
       Dc = null
   },
   new UserAgentInfo
   {
       Regex = GoogleBotRegex,
       On = "Googlebot",
       Ov = null,
       Dc = null
   },
   new UserAgentInfo
   {
       Regex = GoogleBotMobileRegex,
       On = "Googlebot-Mobile",
       Ov = null,
       Dc = null
   },
   new UserAgentInfo
   {
       Regex = BingBotRegex,
       On = "Bingbot",
       Ov = null,
       Dc = null
   },
   new UserAgentInfo
   {
       Regex = BingBotMobileRegex,
       On = "Bingbot Mobile",
       Ov = null,
       Dc = null
   },
};*/       