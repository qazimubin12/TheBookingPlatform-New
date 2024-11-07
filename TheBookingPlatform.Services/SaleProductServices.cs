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
    public class SaleProductServices
    {
        #region Singleton
        public static SaleProductServices Instance
        {
            get
            {
                if (instance == null) instance = new SaleProductServices();
                return instance;
            }
        }
        private static SaleProductServices instance { get; set; }
        private SaleProductServices()
        {
        }
        #endregion



        public List<SaleProduct> GetSaleProduct()
        {
            using (var context = new DSContext())
            {

                return context.SaleProducts.ToList();

            }
        }

        public List<SaleProduct> GetSaleProductWRTBusiness(string Business)
        {
            using (var context = new DSContext())
            {
               
                    return context.SaleProducts.Where(x => x.Business == Business).OrderBy(x => x.ID).ToList();
                
            }
        }

        public List<SaleProduct> GetSaleProductWRTBusiness(string Business, int ReferenceID)
        {
            using (var context = new DSContext())
            {

                return context.SaleProducts.Where(x => x.Business == Business && x.ReferenceID == ReferenceID).OrderBy(x => x.ID).ToList();

            }
        }

        public List<SaleProduct> GetSaleProductWRTBusiness(string Business, List<int> ReferenceIDs)
        {
            using (var context = new DSContext())
            {

                return context.SaleProducts.Where(x => x.Business == Business && ReferenceIDs.Contains(x.ReferenceID)).OrderBy(x => x.ID).ToList();

            }
        }




        public SaleProduct GetSaleProduct(int ID)
        {
            using (var context = new DSContext())
            {

                return context.SaleProducts.Find(ID);

            }
        }

        public void SaveSaleProduct(SaleProduct SaleProduct)
        {
            using (var context = new DSContext())
            {
                try
                {
                    context.SaleProducts.Add(SaleProduct);
                    context.SaveChanges();
                }
                catch (Exception ex)
                {

                    throw;
                }

            }
        }

        public void UpdateSaleProduct(SaleProduct SaleProduct)
        {
            using (var context = new DSContext())
            {
                context.Entry(SaleProduct).State = EntityState.Modified;
                context.SaveChanges();
            }
        }

        public string DeleteSaleProduct(int ID)
        {
            using (var context = new DSContext())
            {
                var SaleProduct = context.SaleProducts.Find(ID);
                context.SaleProducts.Remove(SaleProduct);
                context.SaveChanges();
                return "Deleted Successfully";

            }
        }







        
    }

}
