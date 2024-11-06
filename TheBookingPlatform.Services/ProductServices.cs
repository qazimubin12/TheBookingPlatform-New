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
    public class ProductServices
    {
        #region Singleton
        public static ProductServices Instance
        {
            get
            {
                if (instance == null) instance = new ProductServices();
                return instance;
            }
        }
        private static ProductServices instance { get; set; }
        private ProductServices()
        {
        }
        #endregion
        public class ValidationModel
        {
            public int MainID { get; set; }
            public int Count { get; set; }
        }
        public List<Product> GetProductWRTBusiness(string Business,string SearchTerm = "")
        {
            using (var context = new DSContext())
            {
                if (SearchTerm != "")
                {
                    return context.Products.Where(p => p.Business == Business && p.Name != null && p.Name.ToLower()
                                            .Contains(SearchTerm.ToLower()))
                                            .OrderBy(x => x.Name)
                                            .ToList();
                }
                else
                {
                    return context.Products.Where(x=>x.Business == Business).ToList();
                }
            }
        }

        public List<Product> GetProducts(string SearchTerm = "")
        {
            using (var context = new DSContext())
            {
                if (SearchTerm != "")
                {
                    return context.Products.Where(p =>  p.Name != null && p.Name.ToLower()
                                            .Contains(SearchTerm.ToLower()))
                                            .OrderBy(x => x.Name)
                                            .ToList();
                }
                else
                {
                    return context.Products.ToList();
                }
            }
        }

        public List<Product> GetProductsWRTSupplierID(int SupplierID)
        {
            using (var context = new DSContext())
            {
                return context.Products.Where(x => x.SupplierID == SupplierID).ToList();
            }
        }



        public List<Product> GetProductsWRTSupplierID(string Business,int SupplierID)
        {
            using (var context = new DSContext())
            {
                return context.Products.Where(x => x.Business == Business && x.SupplierID == SupplierID).ToList();
            }
        }
        
        public List<Product> GetProductsWRTSupplierID(string Business)
        {
            using (var context = new DSContext())
            {
                return context.Products.Where(x => x.Business == Business).ToList();
            }
        }

        public Product GetProduct(string Name)
        {
            using (var context = new DSContext())
            {

                return context.Products.Where(x=>x.Name == Name).FirstOrDefault();

            }
        }
        public Product GetProduct(int ID)
        {
            using (var context = new DSContext())
            {

                return context.Products.Find(ID);

            }
        }

        public void SaveProduct(Product Product)
        {
            using (var context = new DSContext())
            {
                context.Products.Add(Product);
                context.SaveChanges();
            }
        }

        public void UpdateProduct(Product Product)
        {
            using (var context = new DSContext())
            {
                context.Entry(Product).State = EntityState.Modified;
                context.SaveChanges();
            }
        }

        public string DeleteProduct(int ID)
        {
            using (var context = new DSContext())
            {
               
                    var Product = context.Products.Find(ID);
                    context.Products.Remove(Product);
                    context.SaveChanges();
                    return "Deleted Successfully";


                
            }
        }
    }
}
