using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheBookingPlatform.Entities
{
    public class OpeningHour:BaseEntity
    {
        public string Day { get; set; }
        public string Time { get; set; }
        public bool isClosed { get; set; }
    }
}
