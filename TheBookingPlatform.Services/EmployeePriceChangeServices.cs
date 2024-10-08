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
    public class EmployeePriceChangeServices
    {
        #region Singleton
        public static EmployeePriceChangeServices Instance
        {
            get
            {
                if (instance == null) instance = new EmployeePriceChangeServices();
                return instance;
            }
        }
        private static EmployeePriceChangeServices instance { get; set; }
        private EmployeePriceChangeServices()
        {
        }
        #endregion

        public List<EmployeePriceChange> GetEmployeePriceChange(int EmployeeID, string SearchTerm = "")
        {
            using (var context = new DSContext())
            {
                if (SearchTerm != "")
                {
                    return context.EmployeePriceChanges.Where(p => p.EmployeeID == EmployeeID && p.TypeOfChange != null && p.TypeOfChange.ToLower()
                                            .Contains(SearchTerm.ToLower()))
                                            .OrderBy(x => x.TypeOfChange)
                                            .ToList();
                }
                else
                {
                    return context.EmployeePriceChanges.Where(x => x.EmployeeID == EmployeeID).OrderBy(x => x.TypeOfChange).ToList();
                }
            }
        }

        public List<EmployeePriceChange> GetEmployeePriceChangeWRTBusiness(int EmployeeID, string Business, string SearchTerm = "")
        {
            using (var context = new DSContext())
            {
                if (SearchTerm != "")
                {
                    return context.EmployeePriceChanges.Where(p => p.EmployeeID == EmployeeID && p.Business == Business && p.TypeOfChange != null && p.TypeOfChange.ToLower()
                                            .Contains(SearchTerm.ToLower()))
                                            .OrderBy(x => x.TypeOfChange)
                                            .ToList();
                }
                else
                {
                    return context.EmployeePriceChanges.Where(X => X.EmployeeID == EmployeeID && X.Business == Business).OrderBy(x => x.TypeOfChange).ToList();
                }
            }
        }

        public EmployeePriceChange GetEmployeePriceChange(int ID)
        {
            using (var context = new DSContext())
            {

                return context.EmployeePriceChanges.Find(ID);

            }
        }

        public void SaveEmployeePriceChange(EmployeePriceChange EmployeePriceChange)
        {
            using (var context = new DSContext())
            {
                context.EmployeePriceChanges.Add(EmployeePriceChange);
                context.SaveChanges();
            }
        }

        public void UpdateEmployeePriceChange(EmployeePriceChange EmployeePriceChange)
        {
            using (var context = new DSContext())
            {
                context.Entry(EmployeePriceChange).State = EntityState.Modified;
                context.SaveChanges();
            }
        }

        public void DeleteEmployeePriceChange(int ID)
        {
            using (var context = new DSContext())
            {

                var EmployeePriceChange = context.EmployeePriceChanges.Find(ID);
                context.EmployeePriceChanges.Remove(EmployeePriceChange);
                context.SaveChanges();
            }
        }
    }

}
