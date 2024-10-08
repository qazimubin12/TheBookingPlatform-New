using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using TheBookingPlatform.Entities;

namespace TheBookingPlatform.ViewModels
{
    public class CouponListingViewModel
    {
        public List<Coupon> Coupons { get; set; }
    }

    public class CouponActionViewModel
    {
        public int ID { get; set; }
        public DateTime DateCreated { get; set; }
        public DateTime ExpiryDate { get; set; }
        public string CouponCode { get; set; }
        public int UsageCount { get; set; }
        public float Discount { get; set; }
        public string CouponName { get; set; }
        public string CouponDescription { get; set; }
    }

    public class CouponAssignmentModel
    {
        public Coupon Coupon { get; set; }
        public Customer Customer { get; set; }
        public CouponAssignment CouponAssignment { get; set; }
    }
    public class CouponAssignmentListingViewModel
    {
        public List<CouponAssignmentModel> CouponAssignments { get; set; }
    }

    public class CouponAssignmentActionViewModel
    {
        public int ID { get; set; }
        public List<Coupon> Coupons { get; set; }
        public Customer Customer { get; set; }
        public int CouponID { get; set; }
        public int CustomerID { get; set; }

        public List<Customer> Customers { get; set; }
        public DateTime AssignedDate { get; set; }
        public int Used { get; set; }

    }
}