using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using TheBookingPlatform.Entities;

namespace TheBookingPlatform.ViewModels
{
    public class VatListingViewModel
    {
        public List<Vat> Vats { get; set; }
        public string SearchTerm { get; set; }
    }

    public class VatActionViewModel
    {
        public string Business { get; set; }

        public int ID { get; set; }
        public float Percentage { get; set; }

        public string Name { get; set; }
    }
}