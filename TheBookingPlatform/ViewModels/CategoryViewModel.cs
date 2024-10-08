using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using TheBookingPlatform.Entities;

namespace TheBookingPlatform.ViewModels
{
    public class CategoriesListingViewModel
    {
        public List<Category> Categories { get; set; }
        public string SearchTerm { get; set; }
    }

    public class CategoriesActionViewModel
    {
        public int ID { get; set; }
        public string Business { get; set; }

        public string Name { get; set; }
    }
}