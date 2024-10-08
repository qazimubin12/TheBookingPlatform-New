using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheBookingPlatform.Entities
{
    public class Coupon:BaseEntity
    {
        public DateTime DateCreated { get; set; }
        public DateTime ExpiryDate { get; set; }
        public float Discount { get; set; }
        public string CouponCode { get; set; }
        public int UsageCount { get; set; }
        public string CouponName { get; set; }
        public string CouponDescription { get; set; }
        public bool IsDisabled { get; set; }
    }
}
