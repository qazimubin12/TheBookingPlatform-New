using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheBookingPlatform.Entities
{
    public class LoyaltyCardPromotion:BaseEntity
    {
        public int LoyaltyCardID { get; set; }
        public float Percentage { get; set; }
        public string Services { get; set; }
        public DateTime Date { get; set; }
    }
}
