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
    public class RequestedEmployeeServices
    {
        #region Singleton
        public static RequestedEmployeeServices Instance
        {
            get
            {
                if (instance == null) instance = new RequestedEmployeeServices();
                return instance;
            }
        }
        private static RequestedEmployeeServices instance { get; set; }
        private RequestedEmployeeServices()
        {
        }
        #endregion



        public List<RequestedEmployee> GetRequestedEmployee(string SearchTerm = "")
        {
            using (var context = new DSContext())
            {
               
                    return context.RequestedEmployees.ToList();
                
            }
        }

        public List<RequestedEmployee> GetRequestedEmployeeWRTBusiness(string Business, string SearchTerm = "")
        {
            using (var context = new DSContext())
            {

                return context.RequestedEmployees.Where(x => x.Business == Business).ToList();
                
            }
        }

        public RequestedEmployee GetRequestedEmployee(int ID)
        {
            using (var context = new DSContext())
            {

                return context.RequestedEmployees.Find(ID);

            }
        }
        public RequestedEmployee GetRequestedEmployeeWRTEmployeeID(int EmployeeID)
        {
            using (var context = new DSContext())
            {

                return context.RequestedEmployees.Where(x=>x.EmployeeID == EmployeeID).FirstOrDefault();

            }
        } 


        public RequestedEmployee GetRequestedEmployeeWRTEmployeeID(string googleCalendarID)
        {
            using (var context = new DSContext())
            {

                return context.RequestedEmployees.Where(x=>x.GoogleCalendarID == googleCalendarID).FirstOrDefault();

            }
        }

        public void SaveRequestedEmployee(RequestedEmployee RequestedEmployee)
        {
            using (var context = new DSContext())
            {
                try
                {
                    context.RequestedEmployees.Add(RequestedEmployee);
                    context.SaveChanges();
                }
                catch (Exception ex)
                {

                    throw;
                }

            }
        }

        public void UpdateRequestedEmployee(RequestedEmployee RequestedEmployee)
        {
            using (var context = new DSContext())
            {
                context.Entry(RequestedEmployee).State = EntityState.Modified;
                context.SaveChanges();
            }
        }

        public string DeleteRequestedEmployee(int ID)
        {
            using (var context = new DSContext())
            {
                var RequestedEmployee = context.RequestedEmployees.Find(ID);
                context.RequestedEmployees.Remove(RequestedEmployee);
                context.SaveChanges();
                return "Deleted Successfully";

            }
        }



    }

}
