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
    public class HistoryServices
    {
        #region Singleton
        public static HistoryServices Instance
        {
            get
            {
                if (instance == null) instance = new HistoryServices();
                return instance;
            }
        }
        private static HistoryServices instance { get; set; }
        private HistoryServices()
        {
        }
        #endregion

        public List<History> GetHistories(string SearchTerm = "")
        {
            using (var context = new DSContext())
            {
                if (SearchTerm != "")
                {
                    return context.Histories.AsNoTracking().Where(p => p.Note != null && p.Note.ToLower()
                                            .Contains(SearchTerm.ToLower()))
                                            .OrderBy(x => x.Note)
                                            .ToList();
                }
                else
                {
                    return context.Histories.AsNoTracking().ToList();
                }
            }
        }

        public List<History> GetHistoriesWRTBusinessandCustomer(string businessName, string CustomerName)
        {
            using (var context = new DSContext())
            {

                return context.Histories.AsNoTracking().Where(x => x.CustomerName == CustomerName && x.Business == businessName).ToList();

            }
        }

        public List<History> GetHistoriesWRTBusiness(string Business,string SearchTerm = "")
        {
            using (var context = new DSContext())
            {
                if (SearchTerm != "")
                {
                    return context.Histories.AsNoTracking().Where(p => p.Business == Business && p.Note != null && p.Note.ToLower()
                                            .Contains(SearchTerm.ToLower()))
                                            .OrderBy(x => x.Date)
                                            .ToList();
                }
                else
                {
                    return context.Histories.AsNoTracking().Where(x=>x.Business == Business).ToList();
                }
            }
        }


        public List<History> GetHistoriesByCustomer(string CustomerName,string BusinessName)
        {
            using (var context = new DSContext())
            {
                
                    return context.Histories.AsNoTracking().Where(x=>x.Business == BusinessName && x.CustomerName == CustomerName).ToList();
                
            }
        }
        public History GetHistory(int ID)
        {
            using (var context = new DSContext())
            {

                return context.Histories.Find(ID);

            }
        }

        public void SaveHistory(History File)
        {
            using (var context = new DSContext())
            {
                context.Histories.Add(File);
                context.SaveChanges();
            }
        }

        public void UpdateHistory(History File)
        {
            using (var context = new DSContext())
            {
                context.Entry(File).State = EntityState.Modified;
                context.SaveChanges();
            }
        }

        public void DeleteHistory(int ID)
        {
            using (var context = new DSContext())
            {

                var history = context.Histories.Find(ID);
                context.Histories.Remove(history);
                context.SaveChanges();
            }
        }
    }
}
