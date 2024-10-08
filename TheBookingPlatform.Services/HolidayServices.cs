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
    public class HolidayServices
    {
        #region Singleton
        public static HolidayServices Instance
        {
            get
            {
                if (instance == null) instance = new HolidayServices();
                return instance;
            }
        }
        private static HolidayServices instance { get; set; }
        private HolidayServices()
        {
        }
        #endregion


        public List<Holiday> GetHoliday(string SearchTerm = "")
        {
            using (var context = new DSContext())
            {
                if (SearchTerm != "")
                {
                    return context.Holidays.Where(p => p.Name != null && p.Name.ToLower()
                                            .Contains(SearchTerm.ToLower()))
                                            .OrderBy(x => x.Name)
                                            .ToList();
                }
                else
                {
                    return context.Holidays.ToList();
                }
            }
        }
        public List<Holiday> GetHolidayWRTBusiness(string Business, string SearchTerm = "")
        {
            using (var context = new DSContext())
            {
                if (SearchTerm != "")
                {
                    return context.Holidays.Where(p => p.Business == Business && p.Name != null && p.Name.ToLower()
                                            .Contains(SearchTerm.ToLower()))
                                            .OrderBy(x => x.Name)
                                            .ToList();
                }
                else
                {
                    return context.Holidays.Where(x => x.Business == Business).ToList();
                }
            }
        }

        public Holiday GetHoliday(int ID)
        {
            using (var context = new DSContext())
            {

                return context.Holidays.Find(ID);

            }
        }

        public void SaveHoliday(Holiday Holiday)
        {
            using (var context = new DSContext())
            {
                context.Holidays.Add(Holiday);
                context.SaveChanges();
            }
        }

        public void UpdateHoliday(Holiday Holiday)
        {
            using (var context = new DSContext())
            {
                context.Entry(Holiday).State = EntityState.Modified;
                context.SaveChanges();
            }
        }

        public string DeleteHoliday(int ID)
        {
            using (var context = new DSContext())
            {

                var Holiday = context.Holidays.Find(ID);
                context.Holidays.Remove(Holiday);
                context.SaveChanges();
                return "Deleted Successfully";

            }

        }
    }
}
