using TheBookingPlatform.Database;
using TheBookingPlatform.Entities;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheBookingPlatform.Services
{
    public class PriceChangeServices
    {
        #region Singleton
        public static PriceChangeServices Instance
        {
            get
            {
                if (instance == null) instance = new PriceChangeServices();
                return instance;
            }
        }
        private static PriceChangeServices instance { get; set; }
        private PriceChangeServices()
        {
        }
        #endregion

        public List<PriceChange> GetPriceChange(string SearchTerm = "")
        {
            using (var context = new DSContext())
            {
                if (SearchTerm != "")
                {
                    return context.PriceChanges.Where(p => p.TypeOfChange != null && p.TypeOfChange.ToLower()
                                            .Contains(SearchTerm.ToLower()))                                           
                                            .OrderBy(x => x.TypeOfChange)
                                            .ToList();
                }
                else
                {
                    return context.PriceChanges.OrderBy(x=>x.TypeOfChange).ToList();
                }
            }
        }

        public List<PriceChange> GetPriceChangeWRTBusiness(string Business,string SearchTerm = "")
        {
            using (var context = new DSContext())
            {
                if (SearchTerm != "")
                {
                    return context.PriceChanges.Where(p => p.Business == Business &&  p.TypeOfChange != null && p.TypeOfChange.ToLower()
                                            .Contains(SearchTerm.ToLower()))
                                            .OrderBy(x => x.TypeOfChange)
                                            .ToList();
                }
                else
                {
                    return context.PriceChanges.Where(X=>X.Business == Business).OrderBy(x => x.TypeOfChange).ToList();
                }
            }
        }

        public PriceChange GetPriceChange(int ID)
        {
            using (var context = new DSContext())
            {

                return context.PriceChanges.Find(ID);

            }
        }

        public void SavePriceChange(PriceChange PriceChange)
        {
            using (var context = new DSContext())
            {
                context.PriceChanges.Add(PriceChange);
                context.SaveChanges();
            }
        }

        public void UpdatePriceChange(PriceChange PriceChange)
        {
            using (var context = new DSContext())
            {
                context.Entry(PriceChange).State = EntityState.Modified;
                context.SaveChanges();
            }
        }

        public void DeletePriceChange(int ID)
        {
            using (var context = new DSContext())
            {

                var PriceChange = context.PriceChanges.Find(ID);
                context.PriceChanges.Remove(PriceChange);
                context.SaveChanges();
            }
        }
    }
}
