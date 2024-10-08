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
    public class OpeningHourServices
    {
        #region Singleton
        public static OpeningHourServices Instance
        {
            get
            {
                if (instance == null) instance = new OpeningHourServices();
                return instance;
            }
        }
        private static OpeningHourServices instance { get; set; }
        private OpeningHourServices()
        {
        }
        #endregion

        public List<OpeningHour> GetOpeningHour(string SearchTerm = "")
        {
            using (var context = new DSContext())
            {

                var daysOfWeekOrder = new Dictionary<string, int>
        {
            { "Monday", 1 },
            { "Tuesday", 2 },
            { "Wednesday", 3 },
            { "Thursday", 4 },
            { "Friday", 5 },
            { "Saturday", 6 },
            { "Sunday", 7 }
        };

                IQueryable<OpeningHour> query = context.OpeningHours;

                if (!string.IsNullOrEmpty(SearchTerm))
                {
                    query = query.Where(p => p.Day != null && p.Day.ToLower().Contains(SearchTerm.ToLower()));
                }

                List<OpeningHour> openingHours = query.ToList();

                // Order the results by the custom day of the week mapping
                openingHours = openingHours.OrderBy(p => daysOfWeekOrder.ContainsKey(p.Day) ? daysOfWeekOrder[p.Day] : int.MaxValue).ToList();

                return openingHours;
            }



        }



        public List<OpeningHour> GetOpeningHoursWRTBusiness(string Business,string SearchTerm = "")
        {
            using (var context = new DSContext())
            {

                var daysOfWeekOrder = new Dictionary<string, int>
        {
            { "Monday", 1 },
            { "Tuesday", 2 },
            { "Wednesday", 3 },
            { "Thursday", 4 },
            { "Friday", 5 },
            { "Saturday", 6 },
            { "Sunday", 7 }
        };

                IQueryable<OpeningHour> query = context.OpeningHours.Where(x=>x.Business == Business);

                if (!string.IsNullOrEmpty(SearchTerm))
                {
                    query = query.Where(p => p.Day != null && p.Day.ToLower().Contains(SearchTerm.ToLower()));
                }

                List<OpeningHour> openingHours = query.ToList();

                // Order the results by the custom day of the week mapping
                openingHours = openingHours.OrderBy(p => daysOfWeekOrder.ContainsKey(p.Day) ? daysOfWeekOrder[p.Day] : int.MaxValue).ToList();

                return openingHours;
            }

        }

        public OpeningHour GetOpeningHourWRTBusiness(string Business,string Day)
        {
            using (var context = new DSContext())
            {

                return context.OpeningHours.Where(x=>x.Business == Business && x.Day == Day).FirstOrDefault();

            }
        }
        public OpeningHour GetOpeningHour(int ID)
        {
            using (var context = new DSContext())
            {

                return context.OpeningHours.Find(ID);

            }
        }

        public void SaveOpeningHour(OpeningHour OpeningHour)
        {
            using (var context = new DSContext())
            {
                context.OpeningHours.Add(OpeningHour);
                context.SaveChanges();
            }
        }

        public void UpdateOpeningHour(OpeningHour OpeningHour)
        {
            using (var context = new DSContext())
            {
                context.Entry(OpeningHour).State = EntityState.Modified;
                context.SaveChanges();
            }
        }

        public void DeleteOpeningHour(int ID)
        {
            using (var context = new DSContext())
            {

                var OpeningHour = context.OpeningHours.Find(ID);
                context.OpeningHours.Remove(OpeningHour);
                context.SaveChanges();
            }
        }
    }
}
