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
    public class FranchiseRequestServices
    {

        #region Singleton
        public static FranchiseRequestServices Instance
        {
            get
            {
                if (instance == null) instance = new FranchiseRequestServices();
                return instance;
            }
        }
        private static FranchiseRequestServices instance { get; set; }
        private FranchiseRequestServices()
        {
        }
        #endregion

        public List<FranchiseRequest> GetFranchiseRequest( string SearchTerm = "")
        {
            using (var context = new DSContext())
            {
                if (SearchTerm != "")
                {
                    return context.FranchiseRequests.Where(p => p.Business != null && p.Business.ToLower()
                                            .Contains(SearchTerm))
                                            .ToList();
                }
                else
                {
                    return context.FranchiseRequests.OrderBy(x=>x.Business).ToList();
                }
            }
        }



        public List<FranchiseRequest> GetFranchiseRequestByBusiness(int ID)
        {
            using (var context = new DSContext())
            {

                return context.FranchiseRequests.Where(x =>x.CompanyIDFor == ID || x.CompanyIDFrom == ID).OrderBy(x => x.ID).ToList();

            }
        }


        public FranchiseRequest GetFranchiseRequestByUserID(string UserID, int Business, int LoggedInCompany)
        {
            using (var context = new DSContext())
            {
                return context.FranchiseRequests
                              .Where(x => (x.CompanyIDFrom == Business || x.CompanyIDFor == Business) &&
                                          (x.CompanyIDFrom == LoggedInCompany || x.CompanyIDFor == LoggedInCompany) &&
                                          (x.MappedToUserID == UserID || x.UserID == UserID))
                              .FirstOrDefault();
            }
        }



        public FranchiseRequest GetFranchiseRequest(int ID)
        {
            using (var context = new DSContext())
            {

                return context.FranchiseRequests.Find(ID);

            }
        }

       

        public void SaveFranchiseRequest(FranchiseRequest FranchiseRequest)
        {
            using (var context = new DSContext())
            {
                context.FranchiseRequests.Add(FranchiseRequest);
                context.SaveChanges();
            }
        }

        public void UpdateFranchiseRequest(FranchiseRequest FranchiseRequest)
        {
            using (var context = new DSContext())
            {
                context.Entry(FranchiseRequest).State = EntityState.Modified;
                context.SaveChanges();
            }
        }

        public void DeleteFranchiseRequest(int ID)
        {
            using (var context = new DSContext())
            {

                var FranchiseRequest = context.FranchiseRequests.Find(ID);
                context.FranchiseRequests.Remove(FranchiseRequest);
                context.SaveChanges();
            }
        }

     
    }
}
