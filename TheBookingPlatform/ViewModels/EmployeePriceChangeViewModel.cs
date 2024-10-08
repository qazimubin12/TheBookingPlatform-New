using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using TheBookingPlatform.Entities;

namespace TheBookingPlatform.ViewModels
{
    public class EmployeePriceChangeListingViewModel
    {
        public List<EmployeePriceChange> EmployeePriceChanges { get; set; }
        public Employee Employee { get; set; }
        public string SearchTerm { get; set; }
    }
    public class EmployeePriceChangeActionViewModel
    {
        public List<Employee> Employees { get; set; }
        public int ID { get; set; }
        public int EmployeeID { get; set; }
        public string TypeOfChange { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public float Percentage { get; set; }
        public bool Repeat { get; set; }
        public string Frequency { get; set; }
        public string Every { get; set; }
        public string Days { get; set; }
        public string Ends { get; set; }
        public string EveryWeek { get; set; }
        public int EndsNumberOfTimesWeek { get; set; }
        public DateTime EndsDateWeek { get; set; }
        public string EveryDay { get; set; }
        public int EndsNumberOfTimesDay { get; set; }
        public DateTime EndsDateDay { get; set; }
        public DateTime EndsDateMonth { get; set; }
        public int EndsNumberOfTimesMonth { get; set; }
        public string EveryMonth { get; set; }
        public DateTime EndTime { get; set; }
        public DateTime StartTime { get; set; }
        public string EndsWeek { get; set; }
        public string EndsDay { get; set; }
        public string EndsMonth { get; set; }
    }
}