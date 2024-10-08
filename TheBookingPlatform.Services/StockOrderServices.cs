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
    public class StockOrderServices
    {
        #region Singleton
        public static StockOrderServices Instance
        {
            get
            {
                if (instance == null) instance = new StockOrderServices();
                return instance;
            }
        }
        private static StockOrderServices instance { get; set; }
        private StockOrderServices()
        {
        }
        #endregion

        public List<StockOrder> GetStockOrder(string SearchTerm = "")
        {
            using (var context = new DSContext())
            {
                if (SearchTerm != "")
                {
                    return context.StockOrders.Where(p => p.SupplierName != null && p.SupplierName.ToLower()
                                            .Contains(SearchTerm.ToLower()))
                                            .OrderBy(x => x.SupplierName)
                                            .ToList();
                }
                else
                {
                    return context.StockOrders.OrderBy(x=>x.SupplierName).ToList();
                }
            }
        }

        public List<StockOrder> GetStockOrderWRTBusiness(string Business,string SearchTerm = "")
        {
            using (var context = new DSContext())
            {
                if (SearchTerm != "")
                {
                    return context.StockOrders.Where(p => p.Business == Business && p.SupplierName != null && p.SupplierName.ToLower()
                                            .Contains(SearchTerm.ToLower()))
                                            .OrderBy(x => x.SupplierName)
                                            .ToList();
                }
                else
                {
                    return context.StockOrders.Where(x=>x.Business == Business).OrderBy(x => x.SupplierName).ToList();
                }
            }
        }

        public StockOrder GetStockOrder(int ID)
        {
            using (var context = new DSContext())
            {

                return context.StockOrders.Find(ID);

            }
        }

        public void SaveStockOrder(StockOrder StockOrder)
        {
            using (var context = new DSContext())
            {
                context.StockOrders.Add(StockOrder);
                context.SaveChanges();
            }
        }

        public void UpdateStockOrder(StockOrder StockOrder)
        {
            using (var context = new DSContext())
            {
                context.Entry(StockOrder).State = EntityState.Modified;
                context.SaveChanges();
            }
        }

        public void DeleteStockOrder(int ID)
        {
            using (var context = new DSContext())
            {

                var StockOrder = context.StockOrders.Find(ID);
                context.StockOrders.Remove(StockOrder);
                context.SaveChanges();
            }
        }
    }
}
