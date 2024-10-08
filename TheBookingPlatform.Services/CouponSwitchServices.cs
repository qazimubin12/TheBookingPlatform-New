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
    public class CouponSwitchServices
    {
        #region Singleton
        public static CouponSwitchServices Instance
        {
            get
            {
                if (instance == null) instance = new CouponSwitchServices();
                return instance;
            }
        }
        private static CouponSwitchServices instance { get; set; }
        private CouponSwitchServices()
        {
        }
        #endregion
        public List<CouponSwitch> GetCouponSwitch(string SearchTerm = "")
        {
            using (var context = new DSContext())
            {
                if (SearchTerm != "")
                {
                    return context.CouponSwitches.Where(p => p.Business != null && p.Business.ToLower()
                                               .Contains(SearchTerm)).OrderBy(x => x.Business)
                                               .ToList();

                }
                else
                {
                    return context.CouponSwitches.ToList();
                }
            }
        }

      

        public CouponSwitch GetCouponSwitch(int ID)
        {
            using (var context = new DSContext())
            {

                return context.CouponSwitches.Find(ID);

            }
        }

        public void SaveCouponSwitch(CouponSwitch CouponSwitch)
        {
            using (var context = new DSContext())
            {
                context.CouponSwitches.Add(CouponSwitch);
                context.SaveChanges();
            }
        }

        public void UpdateCouponSwitch(CouponSwitch CouponSwitch)
        {
            using (var context = new DSContext())
            {
                context.Entry(CouponSwitch).State = EntityState.Modified;
                context.SaveChanges();
            }
        }

        public void DeleteCouponSwitch(int ID)
        {
            using (var context = new DSContext())
            {

                var CouponSwitch = context.CouponSwitches.Find(ID);
                context.CouponSwitches.Remove(CouponSwitch);
                context.SaveChanges();
            }
        }
    }
}
