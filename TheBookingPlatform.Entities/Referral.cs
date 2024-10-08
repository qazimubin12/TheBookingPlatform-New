using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheBookingPlatform.Entities
{
    public class Referral : BaseEntity
    {
        public string ReferralCode { get; set; }
        public int ReferredBy { get; set; }
        public int CustomerID { get; set; }
        public float GrandTotal { get; set; }
        public int AppointmentID { get; set; }

    }
}
