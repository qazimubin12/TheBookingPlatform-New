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
    public class SupplierServices
    {
        #region Singleton
        public static SupplierServices Instance
        {
            get
            {
                if (instance == null) instance = new SupplierServices();
                return instance;
            }
        }
        private static SupplierServices instance { get; set; }
        private SupplierServices()
        {
        }
        #endregion

        public class ValidationModel
        {
            public int MainID { get; set; }
            public int Count { get; set; }
        }

        public List<Supplier> GetSupplier(string SearchTerm = "")
        {
            using (var context = new DSContext())
            {
                if (SearchTerm != "")
                {
                    return context.Suppliers.Where(p => p.Name != null && p.Name.ToLower()
                                            .Contains(SearchTerm.ToLower()))
                                            .OrderBy(x => x.Name)
                                            .ToList();
                }
                else
                {
                    return context.Suppliers.ToList();
                }
            }
        }

        public List<Supplier> GetSupplierWRTBusiness(string Business,string SearchTerm = "")
        {
            using (var context = new DSContext())
            {
                if (SearchTerm != "")
                {
                    return context.Suppliers.Where(p => p.Business == Business && p.Name != null && p.Name.ToLower()
                                            .Contains(SearchTerm.ToLower()))
                                            .OrderBy(x => x.Name)
                                            .ToList();
                }
                else
                {
                    return context.Suppliers.Where(x=>x.Business == Business).ToList();
                }
            }
        }

        public Supplier GetSupplier(int ID)
        {
            using (var context = new DSContext())
            {

                return context.Suppliers.Find(ID);

            }
        }

        public void SaveSupplier(Supplier Supplier)
        {
            using (var context = new DSContext())
            {
                context.Suppliers.Add(Supplier);
                context.SaveChanges();
            }
        }

        public void UpdateSupplier(Supplier Supplier)
        {
            using (var context = new DSContext())
            {
                context.Entry(Supplier).State = EntityState.Modified;
                context.SaveChanges();
            }
        }

        public string DeleteSupplier(int ID)
        {
            using (var context = new DSContext())
            {
                var query = $"SELECT A.ID AS MainID, COUNT(B.ID) AS Count FROM Suppliers A LEFT JOIN Products B ON CONCAT(',', B.SupplierID, ',') LIKE CONCAT('%,', A.ID, ',%') GROUP BY A.ID HAVING COUNT(B.ID) > 0;";
                var DataForValidation = context.Database.SqlQuery<ValidationModel>(query).ToList();
                if (DataForValidation.Any(x => x.MainID == ID))
                {
                    return "Cannot Delete it, because this is linked with another products";
                }

                query = $"SELECT A.ID AS MainID, COUNT(B.ID) AS Count FROM Suppliers A LEFT JOIN StockOrders B ON CONCAT(',', B.SupplierID, ',') LIKE CONCAT('%,', A.ID, ',%') GROUP BY A.ID HAVING COUNT(B.ID) > 0;";
                DataForValidation = context.Database.SqlQuery<ValidationModel>(query).ToList();
                if (DataForValidation.Any(x => x.MainID == ID))
                {
                    return "Cannot Delete it, because this is linked with another Stock Orders";
                }

                else
                {
                    var Supplier = context.Suppliers.Find(ID);
                    context.Suppliers.Remove(Supplier);
                    context.SaveChanges();
                    return "Deleted Successfully";

                }
            }
        }
    }
}
