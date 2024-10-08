using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheBookingPlatform.Entities
{
    public class TimeTableRoster:BaseEntity
    {
        public int EmployeeID { get; set; }
        public DateTime RosterStartDate { get; set; }
    }
}
