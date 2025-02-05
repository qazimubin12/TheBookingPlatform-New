using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheBookingPlatform.Entities
{
    public class Buffer:BaseEntity
    {
        public int AppointmentID { get; set; }
        public string Description { get; set; }
        public DateTime Time { get; set; }
        public DateTime EndTime { get; set; }
        public DateTime Date { get; set; }
        public int ServiceID { get; set; }
    }
}
