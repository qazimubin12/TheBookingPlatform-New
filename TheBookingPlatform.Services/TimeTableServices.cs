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
    public class TimeTableServices
    {
        #region Singleton
        public static TimeTableServices Instance
        {
            get
            {
                if (instance == null) instance = new TimeTableServices();
                return instance;
            }
        }
        private static TimeTableServices instance { get; set; }
        private TimeTableServices()
        {
        }
        #endregion

        public List<TimeTable> GetTimeTable()
        {
            using (var context = new DSContext())
            {
                
                    return context.TimeTables.OrderBy(x=>x.ID).ToList();
                
            }
        }

        public List<TimeTable> GetTimeTableByEmployeeID(int EmpID)
        {
            using (var context = new DSContext())
            {
                var dayOrder = new List<string>
{
    "Monday",
    "Tuesday",
    "Wednesday",
    "Thursday",
    "Friday",
    "Saturday",
    "Sunday"
};
                //var listofTimeTable = context.TimeTables.Where(x => x.EmployeeID == EmpID).OrderBy(x => dayOrder.IndexOf(x.Day)).ToList();
                var listofTimeTable = context.TimeTables
           .Where(x => x.EmployeeID == EmpID)
           .ToList() // Materialize the query to a list
           .OrderBy(x => dayOrder.IndexOf(x.Day))
           .ToList(); // Materialize the sorted result to a list

                return listofTimeTable;

            }
        }

        public TimeTable GetTimeTable(int ID)
        {
            using (var context = new DSContext())
            {

                return context.TimeTables.Find(ID);

            }
        }

        public void SaveTimeTable(TimeTable TimeTable)
        {
            using (var context = new DSContext())
            {
                context.TimeTables.Add(TimeTable);
                context.SaveChanges();
            }
        }

        public void UpdateTimeTable(TimeTable TimeTable)
        {
            using (var context = new DSContext())
            {
                context.Entry(TimeTable).State = EntityState.Modified;
                context.SaveChanges();
            }
        }

        public void DeleteTimeTable(int ID)
        {
            using (var context = new DSContext())
            {

                var TimeTable = context.TimeTables.Find(ID);
                context.TimeTables.Remove(TimeTable);
                context.SaveChanges();
            }
        }
    }
}
