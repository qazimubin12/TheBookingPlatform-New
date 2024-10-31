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
    public class InvoiceServices
    {
        #region Singleton
        public static InvoiceServices Instance
        {
            get
            {
                if (instance == null) instance = new InvoiceServices();
                return instance;
            }
        }
        private static InvoiceServices instance { get; set; }
        private InvoiceServices()
        {
        }
        #endregion

        public List<Invoice> GetInvoice(string SearchTerm = "")
        {
            using (var context = new DSContext())
            {
                if (SearchTerm != "")
                {
                    return context.Invoices.Where(p => p.OrderNo != null && p.OrderNo.ToLower()
                                            .Contains(SearchTerm.ToLower()))
                                            .OrderBy(x => x.OrderNo)
                                            .ToList();
                }
                else
                {
                    return context.Invoices.ToList();
                }
            }
        }


        public List<Invoice> GetInvoices(string business,List<int> AppointmentIDs)
        {
            using (var context = new DSContext())
            {
               
                
                    return context.Invoices.Where(x=>x.Business == business && AppointmentIDs.Contains(x.AppointmentID)).ToList();
                
            }

        }
        public List<Invoice> GetInvoice(string Business,int CustomerID ,string SearchTerm = "")
        {
            using (var context = new DSContext())
            {
                if (SearchTerm != "")
                {
                    return context.Invoices.Where(p => p.Business == Business && p.CustomerID == CustomerID && p.OrderNo != null && p.OrderNo.ToLower()
                                            .Contains(SearchTerm.ToLower()))
                                            .OrderBy(x => x.OrderNo)
                                            .ToList();
                }
                else
                {
                    return context.Invoices.Where(x=>x.Business == Business && x.CustomerID == CustomerID).ToList();
                }
            }
        }


        public Invoice GetInvoice(int ID)
        {
            using (var context = new DSContext())
            {

                return context.Invoices.Find(ID);

            }
        }

        public void SaveInvoice(Invoice Invoice)
        {
            using (var context = new DSContext())
            {
                context.Invoices.Add(Invoice);
                context.SaveChanges();
            }
        }

        public void UpdateInvoice(Invoice Invoice)
        {
            using (var context = new DSContext())
            {
                context.Entry(Invoice).State = EntityState.Modified;
                context.SaveChanges();
            }
        }

        public void DeleteInvoice(int ID)
        {
            using (var context = new DSContext())
            {

                var Invoice = context.Invoices.Find(ID);
                context.Invoices.Remove(Invoice);
                context.SaveChanges();
            }
        }
    }
}
