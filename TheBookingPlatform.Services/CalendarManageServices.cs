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
    public class CalendarManageServices
    {
        #region Singleton
        public static CalendarManageServices Instance
        {
            get
            {
                if (instance == null) instance = new CalendarManageServices();
                return instance;
            }
        }
        private static CalendarManageServices instance { get; set; }
        private CalendarManageServices()
        {
        }
        #endregion

        public List<CalendarManage> GetCalendarManage()
        {
            using (var context = new DSContext())
            {
                return context.CalendarManages.ToList();
            }
        }


        public CalendarManage GetCalendarManage(string Business,string UserID)
        {
            using (var context = new DSContext())
            {

                return context.CalendarManages.Where(x=>x.Business == Business && x.UserID == UserID).FirstOrDefault();

            }
        }
        public List<CalendarManage> GetCalendarManage(string Business)
        {
            using (var context = new DSContext())
            {

                return context.CalendarManages.Where(x=>x.Business == Business).ToList();

            }
        }

        public CalendarManage GetCalendarManage(int ID)
        {
            using (var context = new DSContext())
            {

                return context.CalendarManages.Find(ID);

            }
        }

        public void SaveCalendarManage(CalendarManage CalendarManage)
        {
            using (var context = new DSContext())
            {
                context.CalendarManages.Add(CalendarManage);
                context.SaveChanges();
            }
        }

        public void UpdateCalendarManage(CalendarManage CalendarManage)
        {
            using (var context = new DSContext())
            {
                context.Entry(CalendarManage).State = EntityState.Modified;
                context.SaveChanges();
            }
        }

        public void DeleteCalendarManage(int ID)
        {
            using (var context = new DSContext())
            {

                var CalendarManage = context.CalendarManages.Find(ID);
                context.CalendarManages.Remove(CalendarManage);
                context.SaveChanges();
            }
        }
    }
}
