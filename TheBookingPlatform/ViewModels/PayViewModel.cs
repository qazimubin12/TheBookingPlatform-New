using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using TheBookingPlatform.Entities;

namespace TheBookingPlatform.ViewModels
{
    public class PayViewModel
    {
        public User User { get; set; }
        public List<Package> Packages { get; set; }
    }
}