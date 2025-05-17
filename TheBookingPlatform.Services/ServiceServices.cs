using TheBookingPlatform.Database;
using TheBookingPlatform.Entities;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Remoting.Contexts;

namespace TheBookingPlatform.Services
{
    public class ServiceServices
    {
        #region Singleton
        public static ServiceServices Instance
        {
            get
            {
                if (instance == null) instance = new ServiceServices();
                return instance;
            }
        }
        private static ServiceServices instance { get; set; }
        private ServiceServices()
        {
        }
        #endregion
        public class ValidationModel
        {
            public int MainID { get; set; }
            public int Count { get; set; }
        }

        public List<Service> GetService(string SearchTerm = "")
        {
            using (var context = new DSContext())
            {
                if (SearchTerm != "")
                {
                    return context.Services.Where(p => p.Name != null && p.Name.ToLower().Contains(SearchTerm.ToLower()))
                                            .OrderBy(x => x.DisplayOrder)
                                            .ToList();
                }
                else
                {
                    return context.Services.OrderBy(x=>x.DisplayOrder).ToList();
                }
            }
        }

        public List<string> GetBestSellerServices(string Business)
        {
            List<string> bestSellerServices = new List<string>();

            using (var context = new DSContext())
            {
                var query = $"WITH SplitServices AS ( SELECT TRY_CAST(value AS INT) AS ServiceID FROM Appointments CROSS APPLY STRING_SPLIT(Service, ',') WHERE Color != 'darkgray' AND business = '{Business}' AND DATEPART(MONTH, [Date]) = MONTH(GETDATE()) AND DATEPART(YEAR, [Date]) = YEAR(GETDATE()) ) SELECT TOP 3 s.ServiceID FROM SplitServices s JOIN Services srv ON s.ServiceID = srv.ID WHERE srv.isActive = 1 GROUP BY s.ServiceID";
                // Assuming DataForValidation is of type List<string>
                var result = context.Database.SqlQuery<int>(query, Business).ToList();
                bestSellerServices = result.Select(id => id.ToString()).ToList();

            }

            return bestSellerServices;
        }
        public List<Service> GetService(string Business,string Category,string SearchTerm = "")
        {
            using (var context = new DSContext())
            {
                if (SearchTerm != "")
                {
                    return context.Services.Where(p => p.Business == Business && p.Category.Trim() == Category.Trim() && p.Name != null && p.Name.ToLower().Contains(SearchTerm.ToLower()))
                                            .OrderBy(x => x.DisplayOrder)
                                            .ToList();
                }
                else
                {
                    return context.Services.Where(p=> p.Business == Business && p.Category.Trim() == Category.Trim() ).OrderBy(x => x.DisplayOrder).ToList();
                }
            }
        }

        public List<Service> GetServiceWRTCategory(string Category)
        {
            using (var context = new DSContext())
            {


                return context.Services.Where(p => p.Category.Trim() == Category.Trim()).OrderBy(x => x.DisplayOrder).ToList();

            }
        }
        public List<Service> GetServiceWRTCategory(string Business,string Category)
        {
            using (var context = new DSContext())
            {


                return context.Services.Where(p => p.Business == Business && p.Category.Trim() == Category.Trim()).OrderBy(x => x.DisplayOrder).ToList();

            }
        }

        public List<string> GetAbsenseServiceIDs(string Business)
        {
            using (var context = new DSContext())
            {

                return context.Services.Where(X => X.Business == Business && X.Category == "ABSENSE").Select(x => x.ID.ToString()).ToList();
            }
        }

        public List<Service> GetServiceWRTCategory(string Business, string Category,bool CanBookOnline)
        {
            using (var context = new DSContext())
            {


                return context.Services.Where(p => p.Business == Business && p.Category.Trim() == Category.Trim() && p.CanBookOnline == CanBookOnline).OrderBy(x => x.DisplayOrder).ToList();

            }
        }

        public List<Service> GetService(string Business, string SearchTerm = "")
        {
            using (var context = new DSContext())
            {
                if (SearchTerm != "")
                {
                    return context.Services.Where(p => p.Business == Business  && p.Name != null && p.Name.ToLower().Contains(SearchTerm.ToLower()))
                                            .OrderBy(x => x.DisplayOrder)
                                            .ToList();
                }
                else
                {
                    return context.Services.Where(p => p.Business == Business ).OrderBy(x => x.DisplayOrder).ToList();
                }
            }
        }

        public Service GetService(int ID)
        {
            using (var context = new DSContext())
            {

                return context.Services.Find(ID);

            }
        }

        public void SaveService(Service Service)
        {
            using (var context = new DSContext())
            {
                context.Services.Add(Service);
                context.SaveChanges();
            }
        }

        public void UpdateService(Service Service)
        {
            using (var context = new DSContext())
            {
                context.Entry(Service).State = EntityState.Modified;
                context.SaveChanges();
            }
        }

        public string DeleteService(int ID)
        {
            using (var context = new DSContext())
            {
                var query = $"SELECT A.ID AS MainID, COUNT(B.ID) AS Count FROM Services A LEFT JOIN EmployeeServices B ON CONCAT(',', B.ServiceID, ',') LIKE CONCAT('%,', A.ID, ',%') GROUP BY A.ID HAVING COUNT(B.ID) > 0;";
                var DataForValidation = context.Database.SqlQuery<ValidationModel>(query).ToList();
                if (DataForValidation.Any(x => x.MainID == ID))
                {
                    return "Cannot Delete it, because this is linked with another employee assigned service";
                }

                query = $"SELECT A.ID AS MainID, COUNT(B.ID) AS Count FROM Services A LEFT JOIN Appointments B ON CONCAT(',', B.Service, ',') LIKE CONCAT('%,', A.ID, ',%') GROUP BY A.ID HAVING COUNT(B.ID) > 0;";
                DataForValidation = context.Database.SqlQuery<ValidationModel>(query).ToList();
                if (DataForValidation.Any(x => x.MainID == ID))
                {
                    return "Cannot Delete it, because this is linked with another appointment";
                }


                query = $"SELECT A.ID AS MainID, COUNT(B.ID) AS Count FROM Services A LEFT JOIN LoyaltyCardPromotions B ON CONCAT(',', B.Services, ',') LIKE CONCAT('%,', A.ID, ',%') GROUP BY A.ID HAVING COUNT(B.ID) > 0;";
                DataForValidation = context.Database.SqlQuery<ValidationModel>(query).ToList();
                if (DataForValidation.Any(x => x.MainID == ID))
                {
                    return "Cannot Delete it, because this is linked with another loyalty card promotions";
                }





                else
                {
                    var Service = context.Services.Find(ID);
                    context.Services.Remove(Service);
                    context.SaveChanges();
                    return "Deleted Successfully";
                }
            }
        }
    }
}
