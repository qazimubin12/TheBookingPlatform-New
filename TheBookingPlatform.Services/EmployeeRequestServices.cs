using TheBookingPlatform.Database;
using TheBookingPlatform.Entities;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;

namespace TheBookingPlatform.Services
{
    public class EmployeeRequestServices
    {

        #region Singleton
        public static EmployeeRequestServices Instance
        {
            get
            {
                if (instance == null) instance = new EmployeeRequestServices();
                return instance;
            }
        }
        private static EmployeeRequestServices instance { get; set; }
        private EmployeeRequestServices()
        {
        }
        #endregion

        public List<EmployeeRequest> GetEmployeeRequest( string SearchTerm = "")
        {
            using (var context = new DSContext())
            {
                if (SearchTerm != "")
                {
                    return context.EmployeeRequests.Where(p => p.Business != null && p.Business.ToLower()
                                            .Contains(SearchTerm))
                                            .ToList();
                }
                else
                {
                    return context.EmployeeRequests.OrderBy(x=>x.Business).ToList();
                }
            }
        }




        public List<EmployeeRequest> GetEmployeeRequestByBusiness(int ID)
        {
            using (var context = new DSContext())
            {
                return context.EmployeeRequests
                    .AsNoTracking() // Disable change tracking for read-only operation
                    .Where(x => x.CompanyIDFor == ID || x.CompanyIDFrom == ID)
                    .OrderBy(x => x.ID)
                    .ToList(); // Execute the query and return the list
            }
        }



        public async Task<List<EmployeeRequest>> GetEmployeeRequestByBusinessAsync(int ID)
        {
            using (var context = new DSContext())
            {
                return await context.EmployeeRequests
                    .Where(x => x.CompanyIDFor == ID || x.CompanyIDFrom == ID)
                    .OrderBy(x => x.ID)
                    .ToListAsync();
            }
        }


        public EmployeeRequest GetEmployeeRequest(int ID)
        {
            using (var context = new DSContext())
            {

                return context.EmployeeRequests.Find(ID);

            }
        }

       

        public void SaveEmployeeRequest(EmployeeRequest EmployeeRequest)
        {
            using (var context = new DSContext())
            {
                context.EmployeeRequests.Add(EmployeeRequest);
                context.SaveChanges();
            }
        }

        public void UpdateEmployeeRequest(EmployeeRequest EmployeeRequest)
        {
            using (var context = new DSContext())
            {
                context.Entry(EmployeeRequest).State = EntityState.Modified;
                context.SaveChanges();
            }
        }

        public void DeleteEmployeeRequest(int ID)
        {
            using (var context = new DSContext())
            {

                var EmployeeRequest = context.EmployeeRequests.Find(ID);
                context.EmployeeRequests.Remove(EmployeeRequest);
                context.SaveChanges();
            }
        }

     
    }
}
