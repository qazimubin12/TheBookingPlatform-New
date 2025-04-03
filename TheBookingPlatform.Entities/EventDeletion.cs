using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheBookingPlatform.Entities
{
    public class EventDeletion:BaseEntity
    {
        public int EventID { get; set; }
        public bool DeleteSwitch { get; set; }
    }
}
