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
    public class StockHistoryServices
    {
        #region Singleton
        public static StockHistoryServices Instance
        {
            get
            {
                if (instance == null) instance = new StockHistoryServices();
                return instance;
            }
        }
        private static StockHistoryServices instance { get; set; }
        private StockHistoryServices()
        {
        }
        #endregion

        public List<StockHistory> GetStockHistoryByProductID(int ProductID)
        {
            using (var context = new DSContext())
            {
              
                    return context.StockHistories.OrderBy(x=>x.ProductID == ProductID).ToList();
                
            }
        }

        public StockHistory GetStockHistory(int ID)
        {
            using (var context = new DSContext())
            {

                return context.StockHistories.Find(ID);

            }
        }

        public void SaveStockHistory(StockHistory StockHistory)
        {
            using (var context = new DSContext())
            {
                context.StockHistories.Add(StockHistory);
                context.SaveChanges();
            }
        }

        public void UpdateStockHistory(StockHistory StockHistory)
        {
            using (var context = new DSContext())
            {
                context.Entry(StockHistory).State = EntityState.Modified;
                context.SaveChanges();
            }
        }

        public void DeleteStockHistory(int ID)
        {
            using (var context = new DSContext())
            {

                var StockHistory = context.StockHistories.Find(ID);
                context.StockHistories.Remove(StockHistory);
                context.SaveChanges();
            }
        }
    }
}
