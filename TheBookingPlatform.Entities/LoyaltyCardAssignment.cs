using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheBookingPlatform.Entities
{
    public class LoyaltyCardAssignment:BaseEntity
    {
        public int CustomerID { get; set; }
        public int LoyaltyCardID { get; set; }
        public string CardNumber { get; set; }
        public int Days { get; set; }
        public float CashBack { get; set; }
        public DateTime Date { get; set; }
    }
}
