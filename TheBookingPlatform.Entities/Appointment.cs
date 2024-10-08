using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheBookingPlatform.Entities
{
    public class Appointment : BaseEntity
    {
        public int EmployeeID { get; set; }
        public string Service { get; set; }
        public string Color { get; set; }
        public string ServiceDuration { get; set; }
        public string ServiceDiscount { get; set; }
        public DateTime Date { get; set; } //Start Date
        public DateTime Time { get; set; }   //Start Time - All Service Duration Mins ++
        public DateTime EndTime { get; set; } //End time

        //If Repeat Appointment
        public bool IsRepeat { get; set; }
        public string Frequency { get; set; }
        public string Every { get; set; }
        public string Days { get; set; }
        public string Ends { get; set; } //Combined with Number of times using _



        public int FirstRepeatedID { get; set; }
        //Other
        public string Notes { get; set; }
        public float TotalCost { get; set; }
        public float Discount { get; set; } //In Percentage
        public bool IsWalkIn { get; set; }       //If Walk In  is true //No Customer Selected If False Customer Selected
        public int CustomerID { get; set; }
        public DateTime BookingDate { get; set; }
        public string Label { get; set; }
        public float Deposit { get; set; }
        public string DepositMethod { get; set; }
        public string Status { get; set; }
        public bool IsCancelled { get; set; }
        public bool IsPaid { get; set; }
        public int OnlinePriceChange { get; set; }
        public int EmployeePriceChange { get; set; }
        public bool Reminder { get; set; }
        public bool DELETED { get; set; }
        public string DeletedTime { get; set; }
        public bool AnyAvailableEmployeeSelected { get; set; }
        public bool BookedFromRebook { get; set; }
        public string PaymentSession { get; set; }
        public string GoogleCalendarEventID { get; set; }
        public int CouponID { get; set; }
        public int CouponAssignmentID { get; set; }
        public bool FromGCAL { get; set; }
    }
}
