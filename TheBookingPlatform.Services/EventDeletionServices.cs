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
    public class EventDeletionServices
    {
        #region Singleton
        public static EventDeletionServices Instance
        {
            get
            {
                if (instance == null) instance = new EventDeletionServices();
                return instance;
            }
        }
        private static EventDeletionServices instance { get; set; }
        private EventDeletionServices()
        {
        }
        #endregion
        public List<EventDeletion> GetEventDeletion(string SearchTerm = "")
        {
            using (var context = new DSContext())
            {
                if (SearchTerm != "")
                {
                    return context.EventDeletions.Where(p => p.Business != null && p.Business.ToLower()
                                               .Contains(SearchTerm)).OrderBy(x => x.Business)
                                               .ToList();

                }
                else
                {
                    return context.EventDeletions.ToList();
                }
            }
        }


        public List<EventDeletion> GetEventDeletionWRTBusiness(string Business)
        {
            using (var context = new DSContext())
            {

                return context.EventDeletions.Where(x => x.Business == Business).ToList();

            }
        }
        
        public EventDeletion GetEventDeletion(int ID)
        {
            using (var context = new DSContext())
            {

                return context.EventDeletions.Find(ID);

            }
        }

        public void SaveEventDeletion(EventDeletion EventDeletion)
        {
            using (var context = new DSContext())
            {
                context.EventDeletions.Add(EventDeletion);
                context.SaveChanges();
            }
        }

        public void UpdateEventDeletion(EventDeletion EventDeletion)
        {
            using (var context = new DSContext())
            {
                context.Entry(EventDeletion).State = EntityState.Modified;
                context.SaveChanges();
            }
        }

        public void DeleteEventDeletion(int ID)
        {
            using (var context = new DSContext())
            {

                var EventDeletion = context.EventDeletions.Find(ID);
                context.EventDeletions.Remove(EventDeletion);
                context.SaveChanges();
            }
        }


    }
}
