using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheBookingPlatform.Entities
{
    public class CouponAssignment:BaseEntity
    {
        public int CouponID { get; set; }
        public int CustomerID { get; set; }
        public DateTime AssignedDate { get; set; }
        public int Used { get; set; }
        public bool IsSentToClient { get; set; }
    }
}
