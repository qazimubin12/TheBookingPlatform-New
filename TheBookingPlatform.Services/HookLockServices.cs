using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TheBookingPlatform.Database;
using TheBookingPlatform.Entities;
using HookLock = TheBookingPlatform.Entities.HookLock;
namespace TheBookingPlatform.Services
{
    public class HookLockServices
    {
        #region Singleton
        public static HookLockServices Instance
        {
            get
            {
                if (instance == null) instance = new HookLockServices();
                return instance;
            }
        }
        private static HookLockServices instance { get; set; }
        private HookLockServices()
        {
        }
        #endregion




        public HookLock GetHookLockWRTBusiness(string Business,int EmployeeID)
        {
            using (var context = new DSContext())
            {

                return context.HookLocks.Where(x => x.Business == Business && x.EmployeeID == EmployeeID).FirstOrDefault();

            }
        }

    

        public HookLock GetHookLock(int ID)
        {
            using (var context = new DSContext())
            {

                return context.HookLocks.Find(ID);

            }
        }

        public void SaveHookLock(HookLock HookLock)
        {
            using (var context = new DSContext())
            {
                try
                {
                    context.HookLocks.Add(HookLock);
                    context.SaveChanges();
                }
                catch (Exception ex)
                {

                    throw;
                }

            }
        }

        public void UpdateHookLock(HookLock HookLock)
        {
            using (var context = new DSContext())
            {
                context.Entry(HookLock).State = EntityState.Modified;
                context.SaveChanges();
            }
        }

        public string DeleteHookLock(int ID)
        {
            using (var context = new DSContext())
            {
                var HookLock = context.HookLocks.Find(ID);
                context.HookLocks.Remove(HookLock);
                context.SaveChanges();
                return "Deleted Successfully";

            }
        }

    }

}
