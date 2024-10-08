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
    public class GEventSwitchServices
    {
        #region Singleton
        public static GEventSwitchServices Instance
        {
            get
            {
                if (instance == null) instance = new GEventSwitchServices();
                return instance;
            }
        }
        private static GEventSwitchServices instance { get; set; }
        private GEventSwitchServices()
        {
        }
        #endregion
        public List<GEventSwitch> GetGEventSwitch(string SearchTerm = "")
        {
            using (var context = new DSContext())
            {
                if (SearchTerm != "")
                {
                    return context.GEventSwitches.Where(p => p.Business != null && p.Business.ToLower()
                                               .Contains(SearchTerm)).OrderBy(x => x.Business)
                                               .ToList();

                }
                else
                {
                    return context.GEventSwitches.ToList();
                }
            }
        }


     
        public GEventSwitch GetGEventSwitch(int ID)
        {
            using (var context = new DSContext())
            {

                return context.GEventSwitches.Find(ID);

            }
        }

        public void SaveGEventSwitch(GEventSwitch GEventSwitch)
        {
            using (var context = new DSContext())
            {
                context.GEventSwitches.Add(GEventSwitch);
                context.SaveChanges();
            }
        }

        public void UpdateGEventSwitch(GEventSwitch GEventSwitch)
        {
            using (var context = new DSContext())
            {
                context.Entry(GEventSwitch).State = EntityState.Modified;
                context.SaveChanges();
            }
        }

        public void DeleteGEventSwitch(int ID)
        {
            using (var context = new DSContext())
            {

                var GEventSwitch = context.GEventSwitches.Find(ID);
                context.GEventSwitches.Remove(GEventSwitch);
                context.SaveChanges();
            }
        }


    }
}
