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
    public class ShiftServices
    {
        #region Singleton
        public static ShiftServices Instance
        {
            get
            {
                if (instance == null) instance = new ShiftServices();
                return instance;
            }
        }
        private static ShiftServices instance { get; set; }
        private ShiftServices()
        {
        }
        #endregion


        public List<Shift> GetShiftWRTBusiness(string Business)
        {
            using (var context = new DSContext())
            {

                return context.Shifts.Where(x => x.Business == Business).ToList();

            }
        }

        public List<Shift> GetShiftWRTBusiness(string Business, int EmployeeID)
        {
            using (var context = new DSContext())
            {
                return context.Shifts.Where(x => x.Business == Business && x.EmployeeID == EmployeeID).ToList();

            }
        }


        public List<Shift> GetShiftWRTBusiness(string Business, int EmployeeID, DateTime startDate, DateTime EndDate)
        {
            using (var context = new DSContext())
            {
                return context.Shifts.Where(x => x.Business == Business && x.Date >= startDate && x.Date <= EndDate && x.EmployeeID == EmployeeID).ToList();

            }
        }





        public Shift GetShift(int ID)
        {
            using (var context = new DSContext())
            {

                return context.Shifts.Find(ID);

            }
        }

        public void SaveShift(Shift Shift)
        {
            try
            {
                using (var context = new DSContext())
                {
                    context.Shifts.Add(Shift);
                    context.SaveChanges();
                }
            }
            catch (Exception ex)
            {

                throw;
            }

        }

        public void UpdateShift(Shift Shift)
        {
            using (var context = new DSContext())
            {
                context.Entry(Shift).State = EntityState.Modified;
                context.SaveChanges();
            }
        }

        public string DeleteShift(int ID)
        {
            using (var context = new DSContext())
            {

                var Shift = context.Shifts.Find(ID);
                context.Shifts.Remove(Shift);
                context.SaveChanges();
                return "Deleted Successfully";

            }
        }
    }
}
