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
    public class RecurringShiftServices
    {
        #region Singleton
        public static RecurringShiftServices Instance
        {
            get
            {
                if (instance == null) instance = new RecurringShiftServices();
                return instance;
            }
        }
        private static RecurringShiftServices instance { get; set; }
        private RecurringShiftServices()
        {
        }
        #endregion


        public List<RecurringShift> GetRecurringShiftWRTBusiness(string Business)
        {
            using (var context = new DSContext())
            {

                return context.RecurringShifts.Where(x => x.Business == Business && x.IsDeleted == false).ToList();

            }
        }

        public RecurringShift GetRecurringShiftWRTBusiness(string Business, int ShiftID)
        {
            using (var context = new DSContext())
            {
                return context.RecurringShifts.Where(x => x.Business == Business && x.ShiftID == ShiftID && x.IsDeleted == false).FirstOrDefault();

            }
        }





        public RecurringShift GetRecurringShift(int ID)
        {
            using (var context = new DSContext())
            {

                return context.RecurringShifts.Find(ID);

            }
        }

        public void SaveRecurringShift(RecurringShift RecurringShift)
        {
            try
            {
                using (var context = new DSContext())
                {
                    context.RecurringShifts.Add(RecurringShift);
                    context.SaveChanges();
                }
            }
            catch (Exception ex)
            {

                throw;
            }

        }

        public void UpdateRecurringShift(RecurringShift RecurringShift)
        {
            using (var context = new DSContext())
            {
                context.Entry(RecurringShift).State = EntityState.Modified;
                context.SaveChanges();
            }
        }

        public string DeleteRecurringShift(int ID)
        {
            using (var context = new DSContext())
            {

                var RecurringShift = context.RecurringShifts.Find(ID);
                context.RecurringShifts.Remove(RecurringShift);
                context.SaveChanges();
                return "Deleted Successfully";

            }
        }
    }
}
