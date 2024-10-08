using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using TheBookingPlatform.Entities;

namespace TheBookingPlatform.ViewModels
{
    public class OpeningHourListingViewModel
    {
        public List<OpeningHour> OpeningHours { get; set; }
    }

    public class OpeningHourActionViewModel
    {
        public List<string> DaysOfWeek { get; set; }
        public int ID { get; set; }
        public string Day { get; set; }
        public string Time { get; set; }
        public bool isClosed { get; set; }
    }
}