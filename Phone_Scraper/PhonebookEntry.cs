using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Phone_Scraper
{
    public class PhonebookEntry
    {
        public string? Name { get; set; }
        public string? PrimaryPhone { get; set; }
        public string? PrimaryAddress { get; set; }
       // public string Relatives { get; set; }
        public List<string>? AdditionalPhones { get; set; }
        public List<string>? AdditionalAddresses { get; set; }
        public string? Comments { get; set; }
        public string? RandomCharacters { get; set; }
    }
}
