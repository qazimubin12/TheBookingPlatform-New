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
    public class EmployeeServiceServices
    {
        #region Singleton
        public static EmployeeServiceServices Instance
        {
            get
            {
                if (instance == null) instance = new EmployeeServiceServices();
                return instance;
            }
        }
        private static EmployeeServiceServices instance { get; set; }
        private EmployeeServiceServices()
        {
        }
        #endregion

      

        public List<EmployeeService> GetEmployeeService()
        {
            using (var context = new DSContext())
            {
                return context.EmployeeServices.ToList();
            }
        }

        public List<EmployeeService> GetEmployeeServiceWRTBusiness(string Business)
        {
            using (var context = new DSContext())
            {
                return context.EmployeeServices.Where(x=>x.Business == Business).ToList();
            }
        }

        public List<EmployeeService> GetEmployeeServiceWRTBusiness(string Business,int EmployeeID)
        {
            using (var context = new DSContext())
            {
                return context.EmployeeServices.Where(x => x.Business == Business && x.EmployeeID == EmployeeID).ToList();
            }
        }


        public List<EmployeeService> GetEmployeeServiceWRTEmployeeID(int EmployeeID)
        {
            using (var context = new DSContext())
            {
                return context.EmployeeServices.Where(x => x.EmployeeID == EmployeeID).ToList();
            }
        }
        public List<EmployeeService> GetEmployeeService(string Business,int EmployeeID)
        {
            using (var context = new DSContext())
            {
                return context.EmployeeServices.Where(x=>x.Business == Business && x.EmployeeID == EmployeeID).ToList();
            }
        }

        public EmployeeService GetEmployeeService(int EmployeeID, int ServiceID)
        {
            using (var context = new DSContext())
            {

                return context.EmployeeServices.Where(x => x.EmployeeID == EmployeeID && x.ServiceID == ServiceID).FirstOrDefault();

            }
        }
        public EmployeeService GetEmployeeService(int ID)
        {
            using (var context = new DSContext())
            {

                return context.EmployeeServices.Find(ID);

            }
        }

        public void SaveEmployeeService(EmployeeService EmployeeService)
        {
            using (var context = new DSContext())
            {
                context.EmployeeServices.Add(EmployeeService);
                context.SaveChanges();
            }
        }

        public void UpdateEmployeeService(EmployeeService EmployeeService)
        {
            using (var context = new DSContext())
            {
                context.Entry(EmployeeService).State = EntityState.Modified;
                context.SaveChanges();
            }
        }

        public void DeleteEmployeeService(int ID)
        {
            using (var context = new DSContext())
            {

                var EmployeeService = context.EmployeeServices.Find(ID);
                context.EmployeeServices.Remove(EmployeeService);
                context.SaveChanges();
            }
        }
    }
}
