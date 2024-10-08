using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using TheBookingPlatform.Entities;

namespace TheBookingPlatform.ViewModels
{
    public class PayRollViewModel
    {
        public List<ServiceCategory> ServiceCategories { get; set; }
        public List<Service> Services { get; set; }
        public List<Employee> Employees { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string VStartDate { get; set; }
        public string VEndDate { get; set; }
        public Employee Employee { get; set; }
        public List<string> Statuses { get; set; }
        public int Year { get; set; }
        public int EmployeeID { get; set; }
        public string Type { get; set; }
        public float Amount { get; set; }
        public string FinalAmount { get;  set; }
        public Company Company { get;  set; }


        public string EmployeeName { get; set; }
        public string EmployeeSpecialization { get; set; }
        public float Percentage { get; set; }
        public string CompanyName { get; set; }
        public string CompanyPhoneNumber { get; set; }

        public string CompanyAddress { get; set; }
        public string CompanyLogo { get;  set; }
        
    }

    public class PayRollNewViewModel
    {
        public float Amount { get; set; }
        public Employee Employee { get; set; }
        public Company Company { get; set; }
        public float Percentage { get; set; }
        public string FinalAmount { get; set; }
        //        return Json(new { success = true, Amount = Amount, Employee = employee, Percentage = model.Percentage, FinalAmount = FinalAmount
        //}, JsonRequestBehavior.AllowGet);
    }


    public class PayRollModel
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public float Percentage { get; set; }
        public int EmployeeID { get; set; }
        public float Amount { get; set; }
        public Employee Employee { get; set; }
        public Company Company { get; set; }
        public string FinalAmount { get; set; }
        public List<string> Status { get; set; }
        public bool isCancelled { get; set; }
    }
}