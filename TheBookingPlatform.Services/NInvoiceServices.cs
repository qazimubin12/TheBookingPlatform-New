using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TheBookingPlatform.Database;
using TheBookingPlatform.Entities;

namespace TheBookingPlatformV3.Services
{
    public class NInvoiceServices
    {
        #region Singleton
        public static NInvoiceServices Instance
        {
            get
            {
                if (instance == null) instance = new NInvoiceServices();
                return instance;
            }
        }
        private static NInvoiceServices instance { get; set; }
        private NInvoiceServices()
        {
        }
        #endregion

        public List<NInvoice> GetNInvoice(string SearchTerm = "")
        {
            using (var context = new DSContext())
            {
                if (SearchTerm != "")
                {
                    return context.NInvoices.Where(p => p.Business != null && p.Business.ToLower()
                                            .Contains(SearchTerm.ToLower()))
                                            .OrderBy(x => x.IssueDate)
                                            .ToList();
                }
                else
                {
                    return context.NInvoices.ToList();
                }
            }
        }

        public List<NInvoice> GetNInvoiceWRTBusiness(string Business)
        {
            using (var context = new DSContext())
            {


                return context.NInvoices.Where(X => X.Business == Business).ToList();

            }
        }


        public NInvoice GetNInvoice(int ID)
        {
            using (var context = new DSContext())
            {

                return context.NInvoices.Find(ID);

            }
        }

        public void SaveNInvoice(NInvoice NInvoice)
        {
            using (var context = new DSContext())
            {
                context.NInvoices.Add(NInvoice);
                context.SaveChanges();
            }
        }

        public void UpdateNInvoice(NInvoice NInvoice)
        {
            using (var context = new DSContext())
            {
                context.Entry(NInvoice).State = EntityState.Modified;
                context.SaveChanges();
            }
        }

        public void DeleteNInvoice(int ID)
        {
            using (var context = new DSContext())
            {

                var NInvoice = context.NInvoices.Find(ID);
                context.NInvoices.Remove(NInvoice);
                context.SaveChanges();
            }
        }
    }
}
