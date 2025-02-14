using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheBookingPlatform.Entities
{
    public class RequestedEmployee:BaseEntity
    {
        public int EmployeeID { get; set; }
        public string GoogleCalendarID { get; set; }
        public string WatchID { get; set; }
        public string WatchName { get; set; }
        public DateTime ExpirationDate { get; set; }
    }
}
