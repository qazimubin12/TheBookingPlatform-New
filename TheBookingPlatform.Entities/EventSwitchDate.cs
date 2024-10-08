using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheBookingPlatform.Entities
{
    public class EventSwitchDate:BaseEntity
    {
        public DateTime Date { get; set; }
        public int EventID { get; set; }
    }
}
