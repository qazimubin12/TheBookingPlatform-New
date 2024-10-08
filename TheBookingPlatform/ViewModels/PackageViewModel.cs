using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using TheBookingPlatform.Entities;

namespace TheBookingPlatform.ViewModels
{
    public class PackageListingViewModel
    {
        public List<Package> Packages { get; set; }
        public string SearchTerm { get; set; }
    }

    public class PackageActionViewModel
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public decimal Price { get; set; }
        public string Description { get; set; }
        public string APIKEY { get; set; }
        public int NoOfUsers { get; set; }

        public List<string> Features { get; set; }

        public decimal VAT { get; set; }
        public List<string> FeaturesList { get;  set; }
    }
}