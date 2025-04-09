using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheBookingPlatform.Entities
{
    public class Employee:BaseEntity
    {
        public int DisplayOrder { get; set; }
        public string Name { get; set; }
        public string Gender { get; set; }
    
        public string Photo { get; set; }
        public bool AllowOnlineBooking { get; set; }
        public string Description { get; set; }
        public string Specialization { get; set; }
        public bool IsDeleted { get; set; }
        public string LinkedEmployee { get; set; }
        public bool IsActive { get; set; } = true;
        public string LimitCalendarHistory { get; set; }
        public string Type { get; set; }
        public float Percentage { get; set; }
        public string GoogleCalendarName { get; set; }
        public string GoogleCalendarID { get; set; }
        public string WatchChannelID { get; set; }
        public float ExpYears { get; set; }
        public string Experience { get; set; }
    }
}
