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
    public class ExceptionShiftServices
    {
        #region Singleton
        public static ExceptionShiftServices Instance
        {
            get
            {
                if (instance == null) instance = new ExceptionShiftServices();
                return instance;
            }
        }
        private static ExceptionShiftServices instance { get; set; }
        private ExceptionShiftServices()
        {
        }
        #endregion


        public List<ExceptionShift> GetExceptionShiftsWRTBusiness(string Business)
        {
            using (var context = new DSContext())
            {

                return context.ExceptionShifts.Where(x => x.Business == Business).ToList();

            }
        }

        public List<ExceptionShift> GetExceptionShiftWRTBusiness(string Business, int RecurringShiftID)
        {
            using (var context = new DSContext())
            {
                try
                {
                    return context.ExceptionShifts.Where(x => x.Business == Business && x.ShiftID == RecurringShiftID).ToList();

                }
                catch (Exception ex)
                {

                    throw;
                }

            }
        }


        public List<ExceptionShift> GetExceptionShiftWRTBusinessAndShift(string Business, int ShiftID)
        {
            using (var context = new DSContext())
            {
                return context.ExceptionShifts.Where(x => x.Business == Business && x.ShiftID == ShiftID).ToList();

            }
        }



       
        public ExceptionShift GetExceptionShift(int ID)
        {
            using (var context = new DSContext())
            {

                return context.ExceptionShifts.Find(ID);

            }
        }

        public void SaveExceptionShift(ExceptionShift ExceptionShift)
        {
            try
            {
                using (var context = new DSContext())
                {
                    context.ExceptionShifts.Add(ExceptionShift);
                    context.SaveChanges();
                }
            }
            catch (Exception ex)
            {

                throw;
            }

        }

        public void UpdateExceptionShift(ExceptionShift ExceptionShift)
        {
            using (var context = new DSContext())
            {
                context.Entry(ExceptionShift).State = EntityState.Modified;
                context.SaveChanges();
            }
        }

        public string DeleteExceptionShift(int ID)
        {
            using (var context = new DSContext())
            {

                var ExceptionShift = context.ExceptionShifts.Find(ID);
                context.ExceptionShifts.Remove(ExceptionShift);
                context.SaveChanges();
                return "Deleted Successfully";

            }
        }
    }
}
