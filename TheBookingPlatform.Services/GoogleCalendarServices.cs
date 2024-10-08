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
    public class GoogleCalendarServices
    {
        #region Singleton
        public static GoogleCalendarServices Instance
        {
            get
            {
                if (instance == null) instance = new GoogleCalendarServices();
                return instance;
            }
        }
        private static GoogleCalendarServices instance { get; set; }
        private GoogleCalendarServices()
        {
        }
        #endregion

        public GoogleCalendarIntegration GetGoogleCalendarServicesWRTBusiness(string Business)
        {
            using (var context = new DSContext())
            {

                return context.GoogleCalendarIntegrations.Where(x => x.Business == Business).FirstOrDefault();

            }
        }



        public GoogleCalendarIntegration GetGoogleCalendarIntegration(int ID)
        {
            using (var context = new DSContext())
            {

                return context.GoogleCalendarIntegrations.Find(ID);

            }
        }

        public void SaveGoogleCalendarIntegration(GoogleCalendarIntegration GoogleCalendarIntegration)
        {
            using (var context = new DSContext())
            {
                context.GoogleCalendarIntegrations.Add(GoogleCalendarIntegration);
                context.SaveChanges();
            }
        }

        public void UpdateGoogleCalendarIntegration(GoogleCalendarIntegration GoogleCalendarIntegration)
        {
            using (var context = new DSContext())
            {
                context.Entry(GoogleCalendarIntegration).State = EntityState.Modified;
                context.SaveChanges();
            }
        }

        public string DeleteGoogleCalendarIntegration(int ID)
        {
            using (var context = new DSContext())
            {

                var GoogleCalendarIntegration = context.GoogleCalendarIntegrations.Find(ID);
                context.GoogleCalendarIntegrations.Remove(GoogleCalendarIntegration);
                context.SaveChanges();
                return "Deleted Successfully";
            }

        }
    }
}
