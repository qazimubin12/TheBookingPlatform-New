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
    public class ReferralServices
    {
        #region Singleton
        public static ReferralServices Instance
        {
            get
            {
                if (instance == null) instance = new ReferralServices();
                return instance;
            }
        }
        private static ReferralServices instance { get; set; }
        private ReferralServices()
        {
        }
        #endregion

        public List<Referral> GetReferralWRTBusinessREF(string Business, int ReferredBy)
        {
            using (var context = new DSContext())
            {
                return context.Referrals.Where(X => X.Business == Business && X.ReferredBy == ReferredBy).ToList();
            }
        }

        public List<Referral> GetReferralWRTBusiness(string Business, int CustomerID)
        {
            using (var context = new DSContext())
            {
                return context.Referrals.Where(X => X.Business == Business && X.CustomerID == CustomerID).ToList();
            }
        }

        public Referral GetReferral(int ID)
        {
            using (var context = new DSContext())
            {

                return context.Referrals.Find(ID);

            }
        }

        public void SaveReferral(Referral Referral)
        {
            using (var context = new DSContext())
            {
                context.Referrals.Add(Referral);
                context.SaveChanges();
            }
        }

        public void UpdateReferral(Referral Referral)
        {
            using (var context = new DSContext())
            {
                context.Entry(Referral).State = EntityState.Modified;
                context.SaveChanges();
            }
        }

        public void DeleteReferral(int ID)
        {
            using (var context = new DSContext())
            {

                var Referral = context.Referrals.Find(ID);
                context.Referrals.Remove(Referral);
                context.SaveChanges();
            }
        }
    }

}
