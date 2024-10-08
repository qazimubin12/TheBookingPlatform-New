using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheBookingPlatform.Entities
{
    public class CouponSwitch:BaseEntity
    {
        public int CouponID { get; set; }
        public bool BlastingStatus { get; set; }
    }
}
