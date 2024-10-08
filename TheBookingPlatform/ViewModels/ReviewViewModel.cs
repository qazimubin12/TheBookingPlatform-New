using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using TheBookingPlatform.Entities;

namespace TheBookingPlatform.ViewModels
{
    public class ReviewViewModel
    {
        public Company Company { get; set; }
        public Employee Employee { get; set; }
        
        public int ID { get; set; }
        public int CustomerID { get; set; }
        public float Rating { get; set; }
        public int EmployeeID { get; set; }
        public DateTime Date { get; set; }
        public string Feedback { get; set; }
        public int AppointmentID { get; set; }
        public bool FeedbackReminder { get; set; }
        public bool EmailOpened { get; set; }
        public bool EmailClicked { get; set; }
    }

    public class ReviewActionViewModel
    {
        public List<Customer> Customers { get; set; }
        public List<Employee> Employees { get; set; }
        public int ID { get; set; }
        public int CustomerID { get; set; }
        public float Rating { get; set; }
        public int EmployeeID { get; set; }
        public DateTime Date { get; set; }
        public string Feedback { get; set; }
        public int AppointmentID { get; set; }
        public bool FeedbackReminder { get; set; }
        public bool EmailOpened { get; set; }
        public bool EmailClicked { get; set; }
    }
}