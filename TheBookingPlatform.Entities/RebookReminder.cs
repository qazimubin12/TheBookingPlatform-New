using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheBookingPlatform.Entities
{
    public class RebookReminder:BaseEntity
    {
        public int AppointmentID { get; set; }
        public string CustomerName { get; set; }
        public string CustomerEmail { get; set; }
        public DateTime Date { get; set; }
        public bool Sent { get; set; }
        public bool Opened { get; set; }
        public bool Clicked { get; set; }
    }
}
