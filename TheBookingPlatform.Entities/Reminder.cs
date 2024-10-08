using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheBookingPlatform.Entities
{
    public class Reminder:BaseEntity
    {
        public int CustomerID { get; set; }
        public DateTime Date { get; set; }
        public int EmployeeID { get; set; }
        public string Service { get; set; }
        public int AppointmentID { get; set; }
        public bool Paid { get; set; }
        public bool IsCancelled { get; set; }
        public bool Deleted { get; set; }
        public bool Sent { get; set; }
    }
}
