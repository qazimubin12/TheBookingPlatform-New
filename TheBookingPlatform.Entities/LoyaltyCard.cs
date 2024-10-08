using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheBookingPlatform.Entities
{
    public class LoyaltyCard:BaseEntity
    {
        public string Name { get; set; }
        public int Days { get; set; }
        public bool  IsActive { get; set; }
        public DateTime Date { get; set; }

    }
}
