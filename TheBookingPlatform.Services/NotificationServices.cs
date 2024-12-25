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
    public class NotificationServices
    {
        #region Singleton
        public static NotificationServices Instance
        {
            get
            {
                if (instance == null) instance = new NotificationServices();
                return instance;
            }
        }
        private static NotificationServices instance { get; set; }
        private NotificationServices()
        {
        }
        #endregion



        public List<Notification> GetNotification()
        {
            using (var context = new DSContext())
            {
                return context.Notifications.ToList();
            }
        }

        public List<Notification> GetNotificationWRTBusiness(string Code)
        {
            using (var context = new DSContext())
            {
                return context.Notifications.Where(x => x.Code == Code).ToList();
            }
        }

       public List<Notification> GetNotificationWRTBusiness2(string Business)
        {
            using (var context = new DSContext())
            {
                return context.Notifications.Where(x => x.Business == Business).ToList();
            }
        }

      
        
        public Notification GetNotification(int ID)
        {
            using (var context = new DSContext())
            {

                return context.Notifications.Find(ID);

            }
        }

        public void SaveNotification(Notification Notification)
        {
            using (var context = new DSContext())
            {
                context.Notifications.Add(Notification);
                context.SaveChanges();
            }
        }

        public void UpdateNotification(Notification Notification)
        {
            using (var context = new DSContext())
            {
                context.Entry(Notification).State = EntityState.Modified;
                context.SaveChanges();
            }
        }

        public void DeleteNotification(int ID)
        {
            using (var context = new DSContext())
            {

                var Notification = context.Notifications.Find(ID);
                context.Notifications.Remove(Notification);
                context.SaveChanges();
            }
        }
    }
}
