using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheBookingPlatform.Entities
{
    public class WaitingList:BaseEntity
    {
        public int EmployeeID { get; set; }
        public string Service { get; set; }
        public string Color { get; set; }
        public string ServiceDuration { get; set; }
        public string ServiceDiscount { get; set; }
        public DateTime Date { get; set; } //Start Date
        public DateTime Time { get; set; }   //Start Time - All Service Duration Mins ++
        public string Notes { get; set; }
        public float TotalCost { get; set; }
        public int CustomerID { get; set; }
        public DateTime BookingDate { get; set; }
        public bool Reminder { get; set; }
        public bool NonSelectedEmployee { get; set; }
        public string EmployeeIDs { get; set; }
        public string WaitingListStatus { get; set; }

    }
}
