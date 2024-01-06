using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Phone_Scraper
{
    public class PhonebookEntry
    {
        public string Name { get; set; }
        public string Age { get; set; }
        public string CurrentAddress { get; set; }
        public string CurrentPhone { get; set; }
        public List<string> PreviousAddresses { get; set; } = new List<string>();
        public List<string> PreviousPhones { get; set; } = new List<string>();
        public List<string> Relatives { get; set; } = new List<string>();
        public List<string> Associates { get; set; } = new List<string>();
        public string Email { get; set; }
        public string? Comments { get; set; }
        public string? RandomCharacters { get; set; }
    }
}