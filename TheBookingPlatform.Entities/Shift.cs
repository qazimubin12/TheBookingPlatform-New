using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheBookingPlatform.Entities
{
    public class Shift:BaseEntity
    {
        public int EmployeeID { get; set; }
        public DateTime Date { get; set; }
        public string Day { get; set; }
        public string StartTime { get; set; }
        public string EndTime { get; set; }
        public bool IsRecurring { get; set; }

    }
}
