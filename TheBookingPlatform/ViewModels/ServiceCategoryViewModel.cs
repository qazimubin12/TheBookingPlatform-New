using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using TheBookingPlatform.Entities;

namespace TheBookingPlatform.ViewModels
{
    public class ServiceCategoriesListingViewModel
    {
        public List<ServiceCategory> ServiceCategories { get; set; }
        public string SearchTerm { get; set; }
    }

    public class ServiceCategoriesActionViewModel
    {
        public int ID { get; set; }
        public string Business { get; set; }

        public string Name { get; set; }
    }
}