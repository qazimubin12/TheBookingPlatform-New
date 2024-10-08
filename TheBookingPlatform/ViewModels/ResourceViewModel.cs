using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using TheBookingPlatform.Entities;

namespace TheBookingPlatform.ViewModels
{
    public class ResourceListingViewModel
    {
        public List<Resource> Resources { get; set; }
        public string SearchTerm { get; set; }
    }

    public class ResourceActionViewModel
    {
        public string Business { get; set; }
        public string Availability { get; set; }

        public int ID { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }
        //public string Services { get; set; }//IDS
        public List<int> ServicesINTS { get; set; }

        public List<Service> ServicesList { get; set; }

    }

}