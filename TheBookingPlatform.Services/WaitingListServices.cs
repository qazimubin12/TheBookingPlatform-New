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
    public class WaitingListServices
    {
        #region Singleton
        public static WaitingListServices Instance
        {
            get
            {
                if (instance == null) instance = new WaitingListServices();
                return instance;
            }
        }
        private static WaitingListServices instance { get; set; }
        private WaitingListServices()
        {
        }
        #endregion
       

        public List<WaitingList> GetWaitingList()
        {
            using (var context = new DSContext())
            {

                return context.WaitingLists.OrderBy(x => x.BookingDate).ToList();

            }
        }


        public List<WaitingList> GetWaitingList(string Business,string NotCreatedStatus)
        {
            using (var context = new DSContext())
            {

                return context.WaitingLists.Where(x=>x.Business == Business && x.WaitingListStatus != NotCreatedStatus).OrderBy(x => x.BookingDate).ToList();

            }
        }

        public List<WaitingList> GetWaitingList(string Business,int Day,int Month,int Year, string NotCreatedStatus)
        {
            using (var context = new DSContext())
            {

                return context.WaitingLists.Where(x => x.Date.Day == Day && x.Date.Month == Month && x.Date.Year == Year && x.Business == Business && x.WaitingListStatus != NotCreatedStatus).OrderBy(x => x.BookingDate).ToList();

            }
        }


        public async Task<List<WaitingList>> GetWaitingListAsync(string Business, int Day, int Month, int Year, string NotCreatedStatus)
        {
            using (var context = new DSContext())
            {
                // Use a single DateTime object for comparison to reduce the complexity of the Where clause
                var targetDate = new DateTime(Year, Month, Day);

                // Query optimization
                return await context.WaitingLists
                    .AsNoTracking() // Disable change tracking for better performance
                    .Where(x => x.Date.Year == Year
                                && x.Date.Month == Month
                                && x.Date.Day == Day
                                && x.Business == Business
                                && x.WaitingListStatus != NotCreatedStatus)
                    .OrderBy(x => x.BookingDate)
                    .ToListAsync();
            }
        }

        public async Task<List<WaitingList>> GetWaitingListAsyncWRTEmployeeID(string Business, int Day, int Month, int Year, string NotCreatedStatus,int EmployeeID)
        {
            using (var context = new DSContext())
            {
                // Use a single DateTime object for comparison to reduce the complexity of the Where clause
                var targetDate = new DateTime(Year, Month, Day);

                // Query optimization
                return await context.WaitingLists
                    .AsNoTracking() // Disable change tracking for better performance
                    .Where(x => x.Date.Year == Year
                                &&x.EmployeeID == EmployeeID
                                && x.Date.Month == Month
                                && x.Date.Day == Day
                                && x.Business == Business
                                && x.WaitingListStatus != NotCreatedStatus)
                    .OrderBy(x => x.BookingDate)
                    .ToListAsync();
            }
        }


        public async Task<List<WaitingList>> GetWaitingListAsync(string Business, int Day, int Month, int Year, string NotCreatedStatus, int EmployeeID)
        {
            using (var context = new DSContext())
            {
                // Create a DateTime object to simplify date comparison
                var targetDate = new DateTime(Year, Month, Day);

                // Query optimization
                return await context.WaitingLists
                    .AsNoTracking() // Disable change tracking for better performance
                    .Where(x => x.Date.Year == Year
                                && x.Date.Month == Month
                                && x.Date.Day == Day
                                && x.EmployeeID == EmployeeID
                                && x.Business == Business
                                && x.WaitingListStatus != NotCreatedStatus)
                    .OrderBy(x => x.BookingDate)
                    .ToListAsync();
            }
        }


        public async Task<List<WaitingList>> GetWaitingListAsync(int Day, int Month, int Year, string NotCreatedStatus)
        {
            using (var context = new DSContext())
            {

                return await context.WaitingLists
                        .Where(x => x.Date.Day == Day && x.Date.Month == Month && x.Date.Year == Year  && x.WaitingListStatus != NotCreatedStatus)
                        .OrderBy(x => x.BookingDate)
                        .ToListAsync();
            }
        }




        public List<WaitingList> GetWaitingList(string Business, int Day, int Month, int Year, string NotCreatedStatus,int EmployeeID)
        {
            using (var context = new DSContext())
            {

                return context.WaitingLists.Where(x => x.Date.Day == Day && x.Date.Month == Month && x.Date.Year == Year && x.Business == Business && x.WaitingListStatus != NotCreatedStatus && x.EmployeeID == EmployeeID).OrderBy(x => x.BookingDate).ToList();

            }
        }

        public List<WaitingList> GetWaitingList(string Business, int Day, int Month, int Year, string NotCreatedStatus, bool NonSelectedEmployee)
        {
            using (var context = new DSContext())
            {

                return context.WaitingLists.Where(x => x.Date.Day == Day && x.Date.Month == Month && x.Date.Year == Year && x.Business == Business && x.WaitingListStatus != NotCreatedStatus && x.NonSelectedEmployee == NonSelectedEmployee).OrderBy(x => x.BookingDate).ToList();

            }
        }
        public List<WaitingList> GetWaitingList(int Day, int Month, int Year, string NotCreatedStatus)
        {
            using (var context = new DSContext())
            {

                return context.WaitingLists.Where(x => x.Date.Day == Day && x.Date.Month == Month && x.Date.Year == Year && x.WaitingListStatus != NotCreatedStatus).OrderBy(x => x.BookingDate).ToList();

            }
        }




        public WaitingList GetWaitingList(int ID)
        {
            using (var context = new DSContext())
            {

                return context.WaitingLists.Find(ID);

            }
        }

        public void SaveWaitingList(WaitingList WaitingList)
        {
            try
            {
                using (var context = new DSContext())
                {
                    context.WaitingLists.Add(WaitingList);
                    context.SaveChanges();
                }
            }
            catch (Exception ex)
            {

                throw;
            }
           
        }

        public void UpdateWaitingList(WaitingList WaitingList)
        {
            using (var context = new DSContext())
            {
                context.Entry(WaitingList).State = EntityState.Modified;
                context.SaveChanges();
            }
        }

        public string DeleteWaitingList(int ID)
        {
            using (var context = new DSContext())
            {

                var WaitingList = context.WaitingLists.Find(ID);
                context.WaitingLists.Remove(WaitingList);
                context.SaveChanges();
                return "Deleted Successfully";

            }
        }
    }
}
