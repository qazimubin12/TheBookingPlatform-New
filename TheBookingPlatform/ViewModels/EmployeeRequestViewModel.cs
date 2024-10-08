using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using TheBookingPlatform.Entities;

namespace TheBookingPlatform.ViewModels
{
    public class EmployeeRequestListingViewModel
    {
        public List<EmployeeRequestModel> EmployeeRequests { get; set; }
        public List<FranchiseRequestModel> FranchiseRequests { get; set; }
        public string LoggedInCompany { get; set; }


        public GoogleCalendarIntegration GoogleCalendarIntegration { get; set; }
    }

    public class EmployeeRequestModel
    {
        public EmployeeRequest EmployeeRequest { get; set; }
        public string Employee { get; set; }
        public string CompanyFrom { get; set; }
        public string CompanyFor { get; set; }

    }

    public class EmployeeRequestActionViewModel
    {
        public List<Employee> Employees { get; set; }
        public List<Company> Companies { get; set; } //Child Companies for that Logged In User Company
        public int ID { get; set; }
        public int EmployeeID { get; set; }
        public bool Accepted { get; set; }
        public string Status { get; set; }
        public int CompanyIDFrom { get; set; }
        public int CompanyIDFor { get; set; } //Logged In User Company
    }
}