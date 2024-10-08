using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using TheBookingPlatform.Entities;

namespace TheBookingPlatform.ViewModels
{
    //public class EmployeeServiceListingViewModel
    //{
    //    public string SearchTerm { get; set; }
    //}
    public class EmployeeServiceModel
    {
        public Employee Employee { get; set; }
        public string ServiceData { get; set; }
    }


    public class EmployeeServiceActionViewModel
    {
        public List<EmployeeServiceModel> EmployeeServices { get; set; }
        public int EmployeeID { get; set; }
        public Employee EmployeeFull { get; set; }
        public string ServiceData { get; set; }
        public List<Employee> Employees { get; set; }
        public List<ServiceModel> Services { get; set; }
    }
}