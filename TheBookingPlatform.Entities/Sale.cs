using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheBookingPlatform.Entities
{
    public class Sale:BaseEntity
    {
        public DateTime Date { get; set; }
        public int CustomerID { get; set; }
        public int AppointmentID { get; set; }
        public string Type { get; set; } //Via Appointment //Via Sale
        public string Remarks { get; set; }
    }
}
