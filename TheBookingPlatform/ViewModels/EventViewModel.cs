using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using TheBookingPlatform.Entities;

namespace TheBookingPlatform.ViewModels
{
    public class EventListingViewModel
    {
        public List<EventArgs> Events { get; set; }
        public string SearchTerm { get; set; }
    }

    public class EventActionViewModel
    {
        public int ID { get; set; }
        public string Business { get; set; }
        public string EventType { get; set; } //Absence //Chore // Note
         
        public string Description { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndDate { get; set; }
        public DateTime EndTime { get; set; }
        public bool IsAllDay { get; set; }
        public string AppliesTo { get; set; }
        public int EmployeeID { get; set; }
        public List<Employee> Employees { get; set; }

        //If Repeat Absence
        public string Frequency { get; set; }
        public string Every { get; set; }
        public string Days { get; set; }
        public string Ends { get; set; }


        public DateTime BookingDate { get; set; }
    }
}