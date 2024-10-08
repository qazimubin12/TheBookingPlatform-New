using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TheBookingPlatform.Database;
using TheBookingPlatform.Entities;

namespace TheBookingPlatform.Services
{
    public class CouponServices
    {
        #region Singleton
        public static CouponServices Instance
        {
            get
            {
                if (instance == null) instance = new CouponServices();
                return instance;
            }
        }
        private static CouponServices instance { get; set; }
        private CouponServices()
        {
        }
        #endregion



        public List<Coupon> GetCoupon(string SearchTerm = "")
        {
            using (var context = new DSContext())
            {
                if (SearchTerm != "")
                {
                    return context.Coupons.Where(p => p.CouponName != null && p.CouponName.ToLower()
                                            .Contains(SearchTerm.ToLower()))
                                            .OrderBy(x => x.CouponName)
                                            .ToList();
                }
                else
                {
                    return context.Coupons.OrderBy(x => x.CouponName).ToList();
                }
            }
        }

        public List<Coupon> GetCouponWRTBusiness(string Business, string SearchTerm = "")
        {
            using (var context = new DSContext())
            {
                if (SearchTerm != "")
                {
                    return context.Coupons.Where(p => p.Business == Business && p.CouponName != null && p.CouponName.ToLower()
                                            .Contains(SearchTerm.ToLower()))
                                            .OrderBy(x => x.CouponName)
                                            .ToList();
                }
                else
                {
                    return context.Coupons.Where(x => x.Business == Business).OrderBy(x => x.CouponName).ToList();
                }
            }
        }

        public Coupon GetCoupon(int ID)
        {
            using (var context = new DSContext())
            {

                return context.Coupons.Find(ID);

            }
        }

        public void SaveCoupon(Coupon Coupon)
        {
            using (var context = new DSContext())
            {
                try
                {
                    context.Coupons.Add(Coupon);
                    context.SaveChanges();
                }
                catch (Exception ex)
                {

                    throw;
                }

            }
        }

        public void UpdateCoupon(Coupon Coupon)
        {
            using (var context = new DSContext())
            {
                context.Entry(Coupon).State = EntityState.Modified;
                context.SaveChanges();
            }
        }

        public string DeleteCoupon(int ID)
        {
            using (var context = new DSContext())
            {
                var Coupon = context.Coupons.Find(ID);
                context.Coupons.Remove(Coupon);
                context.SaveChanges();
                return "Deleted Successfully";

            }
        }







        public List<CouponAssignment> GetCouponAssignment()
        {
            using (var context = new DSContext())
            {

                return context.CouponAssignments.ToList();

            }
        }



        public List<CouponAssignment> GetCouponAssignmentsWRTBusiness(string Business, int CustomerID)
        {
            using (var context = new DSContext())
            {

                return context.CouponAssignments.Where(x => x.Business == Business && x.CustomerID == CustomerID).ToList();

            }
        }
        public List<CouponAssignment> GetCouponAssignmentsWRTBusinessAndCouponID(string Business, int CouponID)
        {
            using (var context = new DSContext())
            {

                return context.CouponAssignments.Where(x => x.Business == Business && x.CouponID == CouponID).ToList();

            }
        }

        public CouponAssignment GetCouponAssignmentsWRTBusinessAndCouponID(string Business, int CouponID,int CustomerID)
        {
            using (var context = new DSContext())
            {

                return context.CouponAssignments.Where(x => x.Business == Business && x.CouponID == CouponID && x.CustomerID == CustomerID).FirstOrDefault();

            }
        }

        public List<CouponAssignment> GetCouponAssignmentsWRTBusiness(string Business)
        {
            using (var context = new DSContext())
            {

                return context.CouponAssignments.Where(x => x.Business == Business).ToList();

            }
        }

        public List<CouponAssignment> GetCouponAssignmentByCouponID(int CouponID)
        {
            using (var context = new DSContext())
            {

                return context.CouponAssignments.Where(x => x.CouponID == CouponID).ToList();

            }
        }

        public CouponAssignment GetCouponAssignment(int ID)
        {
            using (var context = new DSContext())
            {

                return context.CouponAssignments.Find(ID);

            }
        }

        public void SaveCouponAssignment(CouponAssignment CouponAssignment)
        {
            using (var context = new DSContext())
            {
                context.CouponAssignments.Add(CouponAssignment);
                context.SaveChanges();
            }
        }

        public void UpdateCouponAssignment(CouponAssignment CouponAssignment)
        {
            using (var context = new DSContext())
            {
                context.Entry(CouponAssignment).State = EntityState.Modified;
                context.SaveChanges();
            }
        }

        public void DeleteCouponAssignment(int ID)
        {
            using (var context = new DSContext())
            {

                var CouponAssignment = context.CouponAssignments.Find(ID);
                context.CouponAssignments.Remove(CouponAssignment);
                context.SaveChanges();
            }
        }
    }

}
