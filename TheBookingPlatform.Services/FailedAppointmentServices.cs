using TheBookingPlatform.Database;
using TheBookingPlatform.Entities;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Mail;
using System.Configuration;
using System.Net;

namespace TheBookingPlatform.Services
{
    public class FailedAppointmentServices
    {
        #region Singleton
        public static FailedAppointmentServices Instance
        {
            get
            {
                if (instance == null) instance = new FailedAppointmentServices();
                return instance;
            }
        }
        private static FailedAppointmentServices instance { get; set; }
        private FailedAppointmentServices()
        {
        }
        #endregion



        public List<FailedAppointment> GetFailedAppointment()
        {
            using (var context = new DSContext())
            {
                
                    return context.FailedAppointments.ToList();
                
            }
        }

        public List<FailedAppointment> GetFailedAppointmentWRTBusiness(string Business)
        {
            using (var context = new DSContext())
            {
                
                    return context.FailedAppointments.Where(x=>x.Business == Business).ToList();
                
            }
        }

        public FailedAppointment GetFailedAppointment(int ID)
        {
            using (var context = new DSContext())
            {

                return context.FailedAppointments.Find(ID);

            }
        }

        public void SaveFailedAppointment(FailedAppointment FailedAppointment)
        {
            using (var context = new DSContext())
            {
                context.FailedAppointments.Add(FailedAppointment);
                context.SaveChanges();
            }
        }

        public void UpdateFailedAppointment(FailedAppointment FailedAppointment)
        {
            using (var context = new DSContext())
            {
                context.Entry(FailedAppointment).State = EntityState.Modified;
                context.SaveChanges();
            }
        }

        public void DeleteFailedAppointment(int ID)
        {
            using (var context = new DSContext())
            {

                var FailedAppointment = context.FailedAppointments.Find(ID);
                context.FailedAppointments.Remove(FailedAppointment);
                context.SaveChanges();
            }
        }
    }
}
