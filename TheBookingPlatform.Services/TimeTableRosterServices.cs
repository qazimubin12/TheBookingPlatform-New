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
    public class TimeTableRosterServices
    {
        #region Singleton
        public static TimeTableRosterServices Instance
        {
            get
            {
                if (instance == null) instance = new TimeTableRosterServices();
                return instance;
            }
        }
        private static TimeTableRosterServices instance { get; set; }
        private TimeTableRosterServices()
        {
        }
        #endregion

        public List<TimeTableRoster> GetTimeTableRoster()
        {
            using (var context = new DSContext())
            {
                
                    return context.TimeTableRosters.OrderBy(x=>x.ID).ToList();
                
            }
        }

        public TimeTableRoster GetTimeTableRosterByEmpID(int EmployeeID)
        {
            using (var context = new DSContext())
            {

                return context.TimeTableRosters.Where(x => x.EmployeeID == EmployeeID).FirstOrDefault();

            }
        }
        public TimeTableRoster GetTimeTableRoster(int ID)
        {
            using (var context = new DSContext())
            {

                return context.TimeTableRosters.Find(ID);

            }
        }

        public void SaveTimeTableRoster(TimeTableRoster TimeTableRoster)
        {
            using (var context = new DSContext())
            {
                context.TimeTableRosters.Add(TimeTableRoster);
                context.SaveChanges();
            }
        }

        public void UpdateTimeTableRoster(TimeTableRoster TimeTableRoster)
        {
            using (var context = new DSContext())
            {
                context.Entry(TimeTableRoster).State = EntityState.Modified;
                context.SaveChanges();
            }
        }

        public void DeleteTimeTableRoster(int ID)
        {
            using (var context = new DSContext())
            {

                var TimeTableRoster = context.TimeTableRosters.Find(ID);
                context.TimeTableRosters.Remove(TimeTableRoster);
                context.SaveChanges();
            }
        }
    }
}
