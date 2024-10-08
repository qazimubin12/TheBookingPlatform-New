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
    public class VatServices
    {
        #region Singleton
        public static VatServices Instance
        {
            get
            {
                if (instance == null) instance = new VatServices();
                return instance;
            }
        }
        private static VatServices instance { get; set; }
        private VatServices()
        {
        }
        #endregion

        public List<Vat> GetVat(string SearchTerm = "")
        {
            using (var context = new DSContext())
            {
                if (SearchTerm != "")
                {
                    return context.Vats.Where(p => p.Name != null && p.Name.ToLower()
                                            .Contains(SearchTerm.ToLower()))
                                            .OrderBy(x => x.Name)
                                            .ToList();
                }
                else
                {
                    return context.Vats.ToList();
                }
            }
        }

        public List<Vat> GetVatWRTBusiness(string Business,string SearchTerm = "")
        {
            using (var context = new DSContext())
            {
                if (SearchTerm != "")
                {
                    return context.Vats.Where(p => p.Business == Business && p.Name != null && p.Name.ToLower()
                                            .Contains(SearchTerm.ToLower()))
                                            .OrderBy(x => x.Name)
                                            .ToList();
                }
                else
                {
                    return context.Vats.Where(x=>x.Business == Business).ToList();
                }
            }
        }

        public Vat GetVat(int ID)
        {
            using (var context = new DSContext())
            {

                return context.Vats.Find(ID);

            }
        }

        public void SaveVat(Vat Vat)
        {
            using (var context = new DSContext())
            {
                context.Vats.Add(Vat);
                context.SaveChanges();
            }
        }

        public void UpdateVat(Vat Vat)
        {
            using (var context = new DSContext())
            {
                context.Entry(Vat).State = EntityState.Modified;
                context.SaveChanges();
            }
        }

        public void DeleteVat(int ID)
        {
            using (var context = new DSContext())
            {

                var Vat = context.Vats.Find(ID);
                context.Vats.Remove(Vat);
                context.SaveChanges();
            }
        }
    }
}
