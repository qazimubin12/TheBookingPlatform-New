using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using TheBookingPlatform.Entities;

namespace TheBookingPlatform.ViewModels
{
    public class CalendarManageListingViewModel
    {
        public List<CalendarManageModel> CalendarManageModels { get; set; }
    }
    public class CalendarManageActionViewModel
    {
        public List<Employee> AssignedUsers { get; set; }
        public int ID { get; set; }
        public string UserID { get; set; }
        public User User { get; set; }
        public List<Employee> ExceptUsers { get; set; }
        public string ManageOf { get; set; }
    }


    public class CalendarManageModel
    {
        public User User { get; set; }
        public List<Employee> ManageOfs { get; set; }
    }
}