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
    public class ReminderServices
    {
        #region Singleton
        public static ReminderServices Instance
        {
            get
            {
                if (instance == null) instance = new ReminderServices();
                return instance;
            }
        }
        private static ReminderServices instance { get; set; }
        private ReminderServices()
        {
        }
        #endregion

      

        public List<Reminder> GetReminder(string SearchTerm = "")
        {
            using (var context = new DSContext())
            {
                if (SearchTerm != "")
                {
                    return context.Reminders.ToList();

                }
                else
                {
                    return context.Reminders.OrderBy(x=>x.ID).ToList();
                }
            }
        }

      


        public List<Reminder> GetReminderWRTBusiness(string Business)
        {
            using (var context = new DSContext())
            {

                return context.Reminders.Where(x => x.Business == Business).ToList();

            }
        }

        public Reminder GetReminderWRTAppID(int AppointmnentID)
        {
            using (var context = new DSContext())
            {

                return context.Reminders.Where(x => x.AppointmentID == AppointmnentID).FirstOrDefault();

            }
        }




        public Reminder GetReminder(int ID)
        {
            using (var context = new DSContext())
            {

                return context.Reminders.Find(ID);

            }
        }

        public void SaveReminder(Reminder Reminder)
        {
            using (var context = new DSContext())
            {
                context.Reminders.Add(Reminder);
                context.SaveChanges();
            }
        }

        public void UpdateReminder(Reminder Reminder)
        {
            using (var context = new DSContext())
            {
                context.Entry(Reminder).State = EntityState.Modified;
                context.SaveChanges();
            }
        }

        public string DeleteReminder(int ID)
        {
            using (var context = new DSContext())
            {
                var Reminder = context.Reminders.Find(ID);
                context.Reminders.Remove(Reminder);
                context.SaveChanges();
                return "Deleted Successfully";
            }
        }
    }
}
