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
    public class SaleServices
    {
        #region Singleton
        public static SaleServices Instance
        {
            get
            {
                if (instance == null) instance = new SaleServices();
                return instance;
            }
        }
        private static SaleServices instance { get; set; }
        private SaleServices()
        {
        }
        #endregion



        public List<Sale> GetSale()
        {
            using (var context = new DSContext())
            {

                return context.Sales.ToList();

            }
        }

        public List<Sale> GetSaleWRTBusiness(string Business)
        {
            using (var context = new DSContext())
            {
               
                    return context.Sales.Where(x => x.Business == Business).OrderBy(x => x.ID).ToList();
                
            }
        }

        public List<Sale> GetSaleWRTBusiness(string Business, int CustomerID,string Type)
        {
            using (var context = new DSContext())
            {

                return context.Sales.Where(x => x.Business == Business && x.CustomerID == CustomerID && x.Type == Type).OrderBy(x => x.ID).ToList();

            }
        }


        public List<Sale> GetSaleWRTBusiness(string Business, int AppointmentID)
        {
            using (var context = new DSContext())
            {

                return context.Sales.Where(x => x.Business == Business && x.AppointmentID == AppointmentID).OrderBy(x => x.ID).ToList();

            }
        }




        public Sale GetSale(int ID)
        {
            using (var context = new DSContext())
            {

                return context.Sales.Find(ID);

            }
        }

        public void SaveSale(Sale Sale)
        {
            using (var context = new DSContext())
            {
                try
                {
                    context.Sales.Add(Sale);
                    context.SaveChanges();
                }
                catch (Exception ex)
                {

                    throw;
                }

            }
        }

        public void UpdateSale(Sale Sale)
        {
            using (var context = new DSContext())
            {
                context.Entry(Sale).State = EntityState.Modified;
                context.SaveChanges();
            }
        }

        public string DeleteSale(int ID)
        {
            using (var context = new DSContext())
            {
                var Sale = context.Sales.Find(ID);
                context.Sales.Remove(Sale);
                context.SaveChanges();
                return "Deleted Successfully";

            }
        }







        
    }

}
