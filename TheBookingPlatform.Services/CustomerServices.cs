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
    public class CustomerServices
    {
        #region Singleton
        public static CustomerServices Instance
        {
            get
            {
                if (instance == null) instance = new CustomerServices();
                return instance;
            }
        }
        private static CustomerServices instance { get; set; }
        private CustomerServices()
        {
        }
        #endregion

        public class ValidationModel
        {
            public int MainID { get; set; }
            public int Count { get; set; }
        }
        public class PagedResult<T>
        {
            public int TotalCount { get; set; }
            public int PageSize { get; set; }
            public int PageNumber { get; set; }
            public List<T> Items { get; set; }
        }


        public List<Customer> GetCustomer(string SearchTerm = "")
        {
            using (var context = new DSContext())
            {
                if (SearchTerm != "")
                {
                    return context.Customers.AsNoTracking()
                    .Where(p => (p.FirstName != null && p.FirstName.ToLower().Contains(SearchTerm.ToLower())) ||
                    (p.LastName != null && p.LastName.ToLower().Contains(SearchTerm.ToLower())) ||
                    (p.FirstName != null && p.LastName != null &&
                    (p.FirstName.ToLower() + " " + p.LastName.ToLower()).Contains(SearchTerm.ToLower())))
                    .OrderBy(x => x.FirstName)
                    .ToList();

                }
                else
                {
                    return context.Customers.AsNoTracking().OrderBy(x=>x.FirstName).ToList();
                }
            }
        }

        public List<Customer> GetCustomersWRTBusiness(string Business,string SearchTerm = "")
        {
            using (var context = new DSContext())
            {
                if (SearchTerm != "")
                {
                    return context.Customers.AsNoTracking()
                    .Where(p => 
                    (p.FirstName != null && p.FirstName.ToLower().Contains(SearchTerm.ToLower())) ||
                    (p.LastName != null && p.LastName.ToLower().Contains(SearchTerm.ToLower())) ||
                    (p.FirstName != null && p.LastName != null &&
                    (p.FirstName.ToLower() + " " + p.LastName.ToLower()).Contains(SearchTerm.ToLower())))
                    .OrderBy(x => x.FirstName)
                    .ToList().Where(x=>x.Business == Business).ToList();

                }
                else
                {
                    return context.Customers.AsNoTracking().Where(x=>x.Business == Business).OrderBy(x => x.FirstName).ToList();
                }
            }
        }


        public PagedResult<Customer> GetCustomers( string SearchTerm = "", int pageNumber = 1, int pageSize = 10)
        {
            using (var context = new DSContext())
            {
                var query = context.Customers.AsNoTracking().ToList();

                // Apply search functionality
                if (!string.IsNullOrEmpty(SearchTerm))
                {
                    var lowerSearchTerm = SearchTerm.ToLower();
                    query = query.Where(p =>
                        (p.FirstName != null && p.FirstName.ToLower().Contains(lowerSearchTerm)) ||
                        (p.LastName != null && p.LastName.ToLower().Contains(lowerSearchTerm)) ||
                        (p.FirstName != null && p.LastName != null &&
                        (p.FirstName.ToLower() + " " + p.LastName.ToLower()).Contains(lowerSearchTerm))).ToList();
                }

                var totalCustomers = query.Count();
                var customers = query.OrderBy(x => x.FirstName)
                                     .Skip((pageNumber - 1) * pageSize)
                                     .Take(pageSize)
                                     .ToList();

                return new PagedResult<Customer>
                {
                    TotalCount = totalCustomers,
                    PageSize = pageSize,
                    PageNumber = pageNumber,
                    Items = customers
                };
            }
        }


        public PagedResult<Customer> GetCustomersWRTBusiness(string Business, string SearchTerm = "", int pageNumber = 1, int pageSize = 10)
        {
            using (var context = new DSContext())
            {
                var query = context.Customers.AsNoTracking().Where(x => x.Business == Business);

                // Apply search functionality
                if (!string.IsNullOrEmpty(SearchTerm))
                {
                    var lowerSearchTerm = SearchTerm.ToLower();
                    query = query.Where(p =>
                        (p.FirstName != null && p.FirstName.ToLower().Contains(lowerSearchTerm)) ||
                        (p.LastName != null && p.LastName.ToLower().Contains(lowerSearchTerm)) ||
                        (p.FirstName != null && p.LastName != null &&
                        (p.FirstName.ToLower() + " " + p.LastName.ToLower()).Contains(lowerSearchTerm)));
                }

                var totalCustomers = query.Count();
                var customers = query.OrderBy(x => x.FirstName)
                                     .Skip((pageNumber - 1) * pageSize)
                                     .Take(pageSize)
                                     .ToList();

                return new PagedResult<Customer>
                {
                    TotalCount = totalCustomers,
                    PageSize = pageSize,
                    PageNumber = pageNumber,
                    Items = customers
                };
            }
        }

        public List<Customer> GetCustomerWRTBusiness(string Business)
        {
            using (var context = new DSContext())
            {

                return context.Customers.AsNoTracking().Where(x => x.Business == Business).ToList();

            }
        }
        public Customer GetCustomerWRTBusinessAndReferral(string Business,string ReferralCode)
        {
            using (var context = new DSContext())
            {

                return context.Customers.AsNoTracking().Where(x => x.Business == Business && x.ReferralCode == ReferralCode ).FirstOrDefault();

            }
        }

       
        public int GetCustomerCount(string business)
        {
            using (var context = new DSContext())
            {

                return context.Customers.Where(X=>X.Business ==business).Count();

            }
        }
           
        public int GetCustomerCount()
        {
            using (var context = new DSContext())
            {

                return context.Customers.Count();

            }
        }

        public Customer GetCustomerWRTBusiness(string Business, string Email,string Password)
        {
            using (var context = new DSContext())
            {

                return context.Customers.AsNoTracking().Where(x => x.Business == Business && x.Email.Trim() == Email.Trim() && x.Password == Password).FirstOrDefault();

            }
        }

        public Customer GetCustomerWRTBusiness(string Business,string Email)
        {
            using (var context = new DSContext())
            {

                return context.Customers.AsNoTracking().Where(x=>x.Business == Business && x.Email.Trim().ToLower() == Email.Trim().ToLower()).FirstOrDefault();

            }
        }


        public Customer GetCustomer(int ID)
        {
            using (var context = new DSContext())
            {

                return context.Customers.Find(ID);

            }
        }

        public void SaveCustomer(Customer Customer)
        {
            using (var context = new DSContext())
            {
                context.Customers.Add(Customer);
                context.SaveChanges();
            }
        }

        public void UpdateCustomer(Customer Customer)
        {
            using (var context = new DSContext())
            {
                context.Entry(Customer).State = EntityState.Modified;
                context.SaveChanges();
            }
        }

        public string DeleteCustomer(int ID)
        {
            using (var context = new DSContext())
            {
                var query = $"SELECT A.ID AS MainID, COUNT(B.ID) AS Count FROM Customers A LEFT JOIN Appointments B ON CONCAT(',', B.CustomerID, ',') LIKE CONCAT('%,', A.ID, ',%') GROUP BY A.ID HAVING COUNT(B.ID) > 0;";
                var DataForValidation = context.Database.SqlQuery<ValidationModel>(query).ToList();
                if (DataForValidation.Any(x => x.MainID == ID))
                {
                    return "Cannot Delete it, because this is linked with appointments";
                }


                query = $"SELECT A.ID AS MainID, COUNT(B.ID) AS Count FROM Customers A LEFT JOIN LoyaltyCardAssignments B ON CONCAT(',', B.CustomerID, ',') LIKE CONCAT('%,', A.ID, ',%') GROUP BY A.ID HAVING COUNT(B.ID) > 0;";
                DataForValidation = context.Database.SqlQuery<ValidationModel>(query).ToList();
                if (DataForValidation.Any(x => x.MainID == ID))
                {
                    return "Cannot Delete it, because this is linked with loyalty card assignments";
                }

                query = $"SELECT A.ID AS MainID, COUNT(B.ID) AS Count FROM Customers A LEFT JOIN GiftCardAssignments B ON CONCAT(',', B.CustomerID, ',') LIKE CONCAT('%,', A.ID, ',%') GROUP BY A.ID HAVING COUNT(B.ID) > 0;";
                DataForValidation = context.Database.SqlQuery<ValidationModel>(query).ToList();
                if (DataForValidation.Any(x => x.MainID == ID))
                {
                    return "Cannot Delete it, because this is linked with gift card assignments";
                }

                query = $"SELECT A.ID AS MainID, COUNT(B.ID) AS Count FROM Customers A LEFT JOIN Files B ON CONCAT(',', B.CustomerID, ',') LIKE CONCAT('%,', A.ID, ',%') GROUP BY A.ID HAVING COUNT(B.ID) > 0;";
                DataForValidation = context.Database.SqlQuery<ValidationModel>(query).ToList();
                if (DataForValidation.Any(x => x.MainID == ID))
                {
                    return "Cannot Delete it, because this is linked with files";
                }
                else
                {
                    var Customer = context.Customers.Find(ID);
                    context.Customers.Remove(Customer);
                    context.SaveChanges();
                    return "Deleted Successfully";
                }
            }
        }
    }
}
