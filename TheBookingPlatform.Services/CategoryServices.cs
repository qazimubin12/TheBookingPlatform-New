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
    public class CategoryServices
    {
        #region Singleton
        public static CategoryServices Instance
        {
            get
            {
                if (instance == null) instance = new CategoryServices();
                return instance;
            }
        }
        private static CategoryServices instance { get; set; }
        private CategoryServices()
        {
        }
        #endregion
        public class ValidationModel
        {
            public int MainID { get; set; }
            public int Count { get; set; }
        }

        public List<Category> GetCategory(string SearchTerm = "")
        {
            using (var context = new DSContext())
            {
                if (SearchTerm != "")
                {
                    return context.Categories.Where(p => p.Name != null && p.Name.ToLower()
                                            .Contains(SearchTerm.ToLower()))
                                            .OrderBy(x => x.Name)
                                            .ToList();
                }
                else
                {
                    return context.Categories.ToList();
                }
            }
        }
        public List<Category> GetCategoryWRTBusiness(string Business,string SearchTerm = "")
        {
            using (var context = new DSContext())
            {
                if (SearchTerm != "")
                {
                    return context.Categories.Where(p => p.Business == Business && p.Name != null && p.Name.ToLower()
                                            .Contains(SearchTerm.ToLower()))
                                            .OrderBy(x => x.Name)
                                            .ToList();
                }
                else
                {
                    return context.Categories.Where(x=>x.Business == Business).ToList();
                }
            }
        }

        public Category GetCategory(int ID)
        {
            using (var context = new DSContext())
            {

                return context.Categories.Find(ID);

            }
        }

        public void SaveCategory(Category Category)
        {
            using (var context = new DSContext())
            {
                context.Categories.Add(Category);
                context.SaveChanges();
            }
        }

        public void UpdateCategory(Category Category)
        {
            using (var context = new DSContext())
            {
                context.Entry(Category).State = EntityState.Modified;
                context.SaveChanges();
            }
        }

        public string DeleteCategory(int ID)
        {
            using (var context = new DSContext())
            {
                var query = $"SELECT A.ID AS MainID, COUNT(B.ID) AS Count FROM Categories A LEFT JOIN Products B ON CONCAT(',', B.CategoryID, ',') LIKE CONCAT('%,', A.ID, ',%') GROUP BY A.ID HAVING COUNT(B.ID) > 0;";
                var DataForValidation = context.Database.SqlQuery<ValidationModel>(query).ToList();
                if (DataForValidation.Any(x => x.MainID == ID))
                {
                    return "Cannot Delete it, because this is linked with another products";
                }

                else
                {
                    var Category = context.Categories.Find(ID);
                    context.Categories.Remove(Category);
                    context.SaveChanges();
                    return "Deleted Successfully";

                }
            }
        }
    }
}
