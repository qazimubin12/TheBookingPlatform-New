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
    public class ServiceSessionServices
    {
        #region Singleton
        public static ServiceSessionServices Instance
        {
            get
            {
                if (instance == null) instance = new ServiceSessionServices();
                return instance;
            }
        }
        private static ServiceSessionServices instance { get; set; }
        private ServiceSessionServices()
        {
        }
        #endregion


        public List<ServiceSession> GetServiceSessionWRTBusiness(string Business)
        {
            using (var context = new DSContext())
            {

                return context.ServiceSessions.Where(x => x.Business == Business).ToList();

            }
        }

        public List<ServiceSession> GetServiceSessionWRTBusiness(string Business, int AppointmentID)
        {
            using (var context = new DSContext())
            {
                return context.ServiceSessions.Where(x => x.Business == Business && x.AppointmentID == AppointmentID).ToList();

            }
        }






        public ServiceSession GetServiceSession(int ID)
        {
            using (var context = new DSContext())
            {

                return context.ServiceSessions.Find(ID);

            }
        }

        public void SaveServiceSession(ServiceSession ServiceSession)
        {
            try
            {
                using (var context = new DSContext())
                {
                    context.ServiceSessions.Add(ServiceSession);
                    context.SaveChanges();
                }
            }
            catch (Exception ex)
            {

                throw;
            }

        }

        public void UpdateServiceSession(ServiceSession ServiceSession)
        {
            using (var context = new DSContext())
            {
                context.Entry(ServiceSession).State = EntityState.Modified;
                context.SaveChanges();
            }
        }

        public string DeleteServiceSession(int ID)
        {
            using (var context = new DSContext())
            {

                var ServiceSession = context.ServiceSessions.Find(ID);
                context.ServiceSessions.Remove(ServiceSession);
                context.SaveChanges();
                return "Deleted Successfully";

            }
        }
    }
}
