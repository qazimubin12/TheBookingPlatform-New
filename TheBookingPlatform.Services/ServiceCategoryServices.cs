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
    public class ServicesCategoriesServices
    {
        #region Singleton
        public static ServicesCategoriesServices Instance
        {
            get
            {
                if (instance == null) instance = new ServicesCategoriesServices();
                return instance;
            }
        }
        private static ServicesCategoriesServices instance { get; set; }
        private ServicesCategoriesServices()
        {
        }
        #endregion
        public class ValidationModel
        {
            public int Count { get; set; }
        }

        public List<ServiceCategory> GetServiceCategories(string SearchTerm = "")
        {
            using (var context = new DSContext())
            {
                if (SearchTerm != "")
                {
                    return context.ServiceCategories.Where(p => p.Name != null && p.Name.ToLower().Contains(SearchTerm.ToLower()))
                                            .OrderBy(x => x.DisplayOrder)
                                            .ToList();
                }
                else
                {
                    return context.ServiceCategories.OrderBy(x=>x.DisplayOrder).ToList();
                }
            }
        }


        public List<ServiceCategory> GetServiceCategoriesWRTBusiness(string Business,string SearchTerm = "")
        {
            using (var context = new DSContext())
            {
                if (SearchTerm != "")
                {
                    return context.ServiceCategories.Where(p => p.Business == Business && p.Name != null && p.Name.ToLower().Contains(SearchTerm.ToLower()))
                                            .OrderBy(x => x.DisplayOrder)
                                            .ToList();
                }
                else
                {
                    return context.ServiceCategories.Where(p=> p.Business == Business).OrderBy(x => x.DisplayOrder).ToList();
                }
            }
        }

        public List<ServiceCategory> GetServiceCategoriesWRTBusinessAndCategory(string Business,string ABSENSECategory)
        {
            using (var context = new DSContext())
            {
               
                    return context.ServiceCategories.Where(p => p.Business == Business && p.Name != ABSENSECategory).OrderBy(x => x.DisplayOrder).ToList();
                
            }
        }


        public ServiceCategory GetServiceCategory(int ID)
        {
            using (var context = new DSContext())
            {

                return context.ServiceCategories.Find(ID);

            }
        }

        public void SaveServiceCategory(ServiceCategory ServiceCategory)
        {
            using (var context = new DSContext())
            {
                context.ServiceCategories.Add(ServiceCategory);
                context.SaveChanges();
            }
        }

        public void UpdateServiceCategory(ServiceCategory ServiceCategory)
        {
            using (var context = new DSContext())
            {
                context.Entry(ServiceCategory).State = EntityState.Modified;
                context.SaveChanges();
            }
        }

        public string DeleteServiceCategory(int ID, string Name,string Business)
        {

            using (var context = new DSContext())
            {
                var query = $"SELECT Count(*)  as 'Count' from Services where Category = '" +Name+"' and Business = '"+Business+"'";
                var DataForValidation = context.Database.SqlQuery<ValidationModel>(query);
                if (DataForValidation.FirstOrDefault()?.Count > 0)
                {
                    return "Cannot Delete it, because this is linked with another service";  
                }
                else
                {
                    var ServiceCategory = context.ServiceCategories.Find(ID);
                    context.ServiceCategories.Remove(ServiceCategory);
                    context.SaveChanges();
                    return "Deleted Successfully";

                }
            }
        }
    }
}
