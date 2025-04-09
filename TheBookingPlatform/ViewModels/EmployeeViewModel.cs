using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using TheBookingPlatform.Entities;

namespace TheBookingPlatform.ViewModels
{
    public class EmployeeListingViewModel
    {
        public List<EmployeeModel> Employees { get; set; }
        public string SearchTerm { get; set; }
        public List<string> CalendarHistoryAccessList { get; set; }
        public List<string> Types { get; set; }
    }
    public class EmployeeModel
    {
        public Employee Employee { get; set; }
        public List<Service> Services { get; set; }
        public int ManageAccessCount { get; set; }
    }

    public class EmployeeActionViewModel
    {
        public List<ServiceModel> ServicesList { get; set; }
        public string Business { get; set; }

        public List<User> Users { get; set; }
        public int ID { get; set; }
        public bool IsActive { get; set; }
        public string Photo { get; set; }
        public bool DisableRegister { get; set; }
        public string Name { get; set; }
        public string Gender { get; set; }
        public bool AllowOnlineBooking { get; set; }
        public string Description { get; set; }
        public string Specialization { get; set; }
        public string LinkedEmployee { get; set; }

        public float Percentage { get; set; }

        public string Type { get; set; }
        public float ExpYears { get;  set; }
        public string Experience { get;  set; }
    }

}