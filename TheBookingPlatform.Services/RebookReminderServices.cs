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
    public class RebookReminderServices
    {
        #region Singleton
        public static RebookReminderServices Instance
        {
            get
            {
                if (instance == null) instance = new RebookReminderServices();
                return instance;
            }
        }
        private static RebookReminderServices instance { get; set; }
        private RebookReminderServices()
        {
        }
        #endregion

        
       

        public List<RebookReminder> GetRebookRemindersWRTBusiness(string Business,string SearchTerm = "")
        {
            using (var context = new DSContext())
            {
               
                    return context.RebookReminders.Where(x=>x.Business == Business).OrderBy(x => x.Date).ToList();
                
            }
        }


        public List<RebookReminder> GetRebookReminderWRTBusiness(string Business)
        {
            using (var context = new DSContext())
            {

                return context.RebookReminders.Where(x => x.Business == Business).ToList();

            }
        }
        public List<RebookReminder> GetRebookReminderWRTBusiness(string Business,DateTime StartDate,DateTime EndDate)
        {
            using (var context = new DSContext())
            {

                return context.RebookReminders
                           .Where(x => x.Business == Business && x.Date >= StartDate && x.Date <= EndDate)
                           .ToList();
            }
        }

        public RebookReminder GetRebookReminderWRTBusiness(int AppointmentID)
        {
            using (var context = new DSContext())
            {

                return context.RebookReminders.Where(x =>  x.AppointmentID == AppointmentID).FirstOrDefault();

            }
        }

       

        public RebookReminder GetRebookReminder(int ID)
        {
            using (var context = new DSContext())
            {

                return context.RebookReminders.Find(ID);

            }
        }

        public void SaveRebookReminder(RebookReminder RebookReminder)
        {
            using (var context = new DSContext())
            {
                context.RebookReminders.Add(RebookReminder);
                context.SaveChanges();
            }
        }

        public void UpdateRebookReminder(RebookReminder RebookReminder)
        {
            using (var context = new DSContext())
            {
                context.Entry(RebookReminder).State = EntityState.Modified;
                context.SaveChanges();
            }
        }

    }
}
