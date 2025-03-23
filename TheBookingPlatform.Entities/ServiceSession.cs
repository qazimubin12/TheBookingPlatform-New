using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheBookingPlatform.Entities
{
    public class ServiceSession:BaseEntity
    {
        public int AppointmentID { get; set; }
        public int Remaining { get; set; }
        public int Done { get; set; }
        public DateTime Date { get; set; }

    }
}
