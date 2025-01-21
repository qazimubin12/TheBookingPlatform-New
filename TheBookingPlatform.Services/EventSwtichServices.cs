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
    public class EventSwitchServices
    {
        #region Singleton
        public static EventSwitchServices Instance
        {
            get
            {
                if (instance == null) instance = new EventSwitchServices();
                return instance;
            }
        }
        private static EventSwitchServices instance { get; set; }
        private EventSwitchServices()
        {
        }
        #endregion
        public List<EventSwitch> GetEventSwitch(string SearchTerm = "")
        {
            using (var context = new DSContext())
            {
                if (SearchTerm != "")
                {
                    return context.EventSwitches.Where(p => p.Business != null && p.Business.ToLower()
                                               .Contains(SearchTerm)).OrderBy(x => x.Business)
                                               .ToList();

                }
                else
                {
                    return context.EventSwitches.ToList();
                }
            }
        }


        public List<EventSwitch> GetEventSwitchWRTBusiness(string Business)
        {
            using (var context = new DSContext())
            {
                
                    return context.EventSwitches.Where(x=>x.Business == Business).ToList();
                
            }
        }
        public EventSwitch GetEventSwitchbyEventID(int AppointmentID)
        {
            using (var context = new DSContext())
            {

                return context.EventSwitches.Where(x => x.AppointmentID == AppointmentID).FirstOrDefault();

            }
        }
        public EventSwitch GetEventSwitch(int ID)
        {
            using (var context = new DSContext())
            {

                return context.EventSwitches.Find(ID);

            }
        }

        public void SaveEventSwitch(EventSwitch EventSwitch)
        {
            using (var context = new DSContext())
            {
                context.EventSwitches.Add(EventSwitch);
                context.SaveChanges();
            }
        }

        public void UpdateEventSwitch(EventSwitch EventSwitch)
        {
            using (var context = new DSContext())
            {
                context.Entry(EventSwitch).State = EntityState.Modified;
                context.SaveChanges();
            }
        }

        public void DeleteEventSwitch(int ID)
        {
            using (var context = new DSContext())
            {

                var EventSwitch = context.EventSwitches.Find(ID);
                context.EventSwitches.Remove(EventSwitch);
                context.SaveChanges();
            }
        }
















        public List<EventSwitchDate> GetEventSwitchDate(string SearchTerm = "")
        {
            using (var context = new DSContext())
            {
                if (SearchTerm != "")
                {
                    return context.EventSwitchDates.Where(p => p.Business != null && p.Business.ToLower()
                                               .Contains(SearchTerm)).OrderBy(x => x.Business)
                                               .ToList();

                }
                else
                {
                    return context.EventSwitchDates.ToList();
                }
            }
        }


        public EventSwitchDate GetEventSwitchDatebyEventID(int EventID)
        {
            using (var context = new DSContext())
            {

                return context.EventSwitchDates.Where(x => x.EventID == EventID).FirstOrDefault();

            }
        }
        public EventSwitchDate GetEventSwitchDate(int ID)
        {
            using (var context = new DSContext())
            {

                return context.EventSwitchDates.Find(ID);

            }
        }

        public void SaveEventSwitchDate(EventSwitchDate EventSwitchDate)
        {
            using (var context = new DSContext())
            {
                context.EventSwitchDates.Add(EventSwitchDate);
                context.SaveChanges();
            }
        }

        public void UpdateEventSwitchDate(EventSwitchDate EventSwitchDate)
        {
            using (var context = new DSContext())
            {
                context.Entry(EventSwitchDate).State = EntityState.Modified;
                context.SaveChanges();
            }
        }

        public void DeleteEventSwitchDate(int ID)
        {
            using (var context = new DSContext())
            {

                var EventSwitchDate = context.EventSwitchDates.Find(ID);
                context.EventSwitchDates.Remove(EventSwitchDate);
                context.SaveChanges();
            }
        }
    }
}
