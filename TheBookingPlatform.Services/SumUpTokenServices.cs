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
    public class SumUpTokenServices
    {
        #region Singleton
        public static SumUpTokenServices Instance
        {
            get
            {
                if (instance == null) instance = new SumUpTokenServices();
                return instance;
            }
        }
        private static SumUpTokenServices instance { get; set; }
        private SumUpTokenServices()
        {
        }
        #endregion

        public List<SumUpToken> GetSumUpToken(string SearchTerm = "")
        {
            using (var context = new DSContext())
            {
                
                
                    return context.SumUpTokens.ToList();
                
            }
        }

        

        public SumUpToken GetSumUpToken(int ID)
        {
            using (var context = new DSContext())
            {

                return context.SumUpTokens.Find(ID);

            }
        }

        public void SaveSumUpToken(SumUpToken SumUpToken)
        {
            using (var context = new DSContext())
            {
                context.SumUpTokens.Add(SumUpToken);
                context.SaveChanges();
            }
        }

        public void UpdateSumUpToken(SumUpToken SumUpToken)
        {
            using (var context = new DSContext())
            {
                context.Entry(SumUpToken).State = EntityState.Modified;
                context.SaveChanges();
            }
        }

        public void DeleteSumUpToken(int ID)
        {
            using (var context = new DSContext())
            {

                var SumUpToken = context.SumUpTokens.Find(ID);
                context.SumUpTokens.Remove(SumUpToken);
                context.SaveChanges();
            }
        }
    }
}
