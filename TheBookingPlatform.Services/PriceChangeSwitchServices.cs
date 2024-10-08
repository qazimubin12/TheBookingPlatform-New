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
    public class PriceChangeSwitchServices
    {
        #region Singleton
        public static PriceChangeSwitchServices Instance
        {
            get
            {
                if (instance == null) instance = new PriceChangeSwitchServices();
                return instance;
            }
        }
        private static PriceChangeSwitchServices instance { get; set; }
        private PriceChangeSwitchServices()
        {
        }
        #endregion
        public List<PriceChangeSwitch> GetPriceChangeSwitch(string SearchTerm = "")
        {
            using (var context = new DSContext())
            {
                if (SearchTerm != "")
                {
                    return context.PriceChangeSwitches.Where(p => p.Business != null && p.Business.ToLower()
                                               .Contains(SearchTerm)).OrderBy(x => x.Business)
                                               .ToList();

                }
                else
                {
                    return context.PriceChangeSwitches.ToList();
                }
            }
        }


        public PriceChangeSwitch GetPriceChangeSwitchbyPriceChangeID(int PriceChangeID)
        {
            using (var context = new DSContext())
            {

                return context.PriceChangeSwitches.Where(x => x.PriceChangeID == PriceChangeID).FirstOrDefault();

            }
        }
        public PriceChangeSwitch GetPriceChangeSwitch(int ID)
        {
            using (var context = new DSContext())
            {

                return context.PriceChangeSwitches.Find(ID);

            }
        }

        public void SavePriceChangeSwitch(PriceChangeSwitch PriceChangeSwitch)
        {
            using (var context = new DSContext())
            {
                context.PriceChangeSwitches.Add(PriceChangeSwitch);
                context.SaveChanges();
            }
        }

        public void UpdatePriceChangeSwitch(PriceChangeSwitch PriceChangeSwitch)
        {
            using (var context = new DSContext())
            {
                context.Entry(PriceChangeSwitch).State = EntityState.Modified;
                context.SaveChanges();
            }
        }

        public void DeletePriceChangeSwitch(int ID)
        {
            using (var context = new DSContext())
            {

                var PriceChangeSwitch = context.PriceChangeSwitches.Find(ID);
                context.PriceChangeSwitches.Remove(PriceChangeSwitch);
                context.SaveChanges();
            }
        }
    }
}
