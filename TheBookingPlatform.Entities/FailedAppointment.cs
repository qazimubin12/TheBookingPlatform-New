using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheBookingPlatform.Entities
{
    public class FailedAppointment:BaseEntity
    {
        public int AppointmentID { get; set; }
        public bool Failed { get; set; }
    }
}
