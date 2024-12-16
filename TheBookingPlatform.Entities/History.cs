using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheBookingPlatform.Entities
{
    public class History:BaseEntity
    {
        public string CustomerName { get; set; }
        public string EmployeeName { get; set; }
        public string Note { get; set; }
        public DateTime Date { get; set; }
        public string Type { get; set; } = "General";
        public int AppointmentID { get; set; }
    }
}
