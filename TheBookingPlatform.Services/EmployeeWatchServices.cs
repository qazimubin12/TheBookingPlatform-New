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
    public class EmployeeWatchServices
    {
        #region Singleton
        public static EmployeeWatchServices Instance
        {
            get
            {
                if (instance == null) instance = new EmployeeWatchServices();
                return instance;
            }
        }
        private static EmployeeWatchServices instance { get; set; }
        private EmployeeWatchServices()
        {
        }
        #endregion
        public List<EmployeeWatch> GetEmployeeWatch(string SearchTerm = "")
        {
            using (var context = new DSContext())
            {
                if (SearchTerm != "")
                {
                    return context.EmployeeWatches.Where(p => p.Business != null && p.Business.ToLower()
                                               .Contains(SearchTerm)).OrderBy(x => x.Business)
                                               .ToList();

                }
                else
                {
                    return context.EmployeeWatches.ToList();
                }
            }
        }


        public EmployeeWatch GetEmployeeWatchByEmployeeID(int EmployeeID)
        {
            using (var context = new DSContext())
            {

                return context.EmployeeWatches.Where(x => x.EmployeeID == EmployeeID).FirstOrDefault();

            }
        }

        public EmployeeWatch GetEmployeeWatch(int ID)
        {
            using (var context = new DSContext())
            {

                return context.EmployeeWatches.Find(ID);

            }
        }

        public void SaveEmployeeWatch(EmployeeWatch EmployeeWatch)
        {
            using (var context = new DSContext())
            {
                context.EmployeeWatches.Add(EmployeeWatch);
                context.SaveChanges();
            }
        }

        public void UpdateEmployeeWatch(EmployeeWatch EmployeeWatch)
        {
            using (var context = new DSContext())
            {
                context.Entry(EmployeeWatch).State = EntityState.Modified;
                context.SaveChanges();
            }
        }

        public void DeleteEmployeeWatch(int ID)
        {
            using (var context = new DSContext())
            {

                var EmployeeWatch = context.EmployeeWatches.Find(ID);
                context.EmployeeWatches.Remove(EmployeeWatch);
                context.SaveChanges();
            }
        }

    }
}
