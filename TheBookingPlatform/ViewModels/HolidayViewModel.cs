using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using TheBookingPlatform.Entities;

namespace TheBookingPlatform.ViewModels
{
    public class HolidayListingViewModel
    {
        public List<Holiday> Holidays { get; set; }
        public string SearchTerm { get; set; }
    }

    public class HolidayActionViewModel
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public DateTime Date { get; set; }
    }
}