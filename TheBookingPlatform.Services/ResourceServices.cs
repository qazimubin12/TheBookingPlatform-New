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
    public class ResourceServices
    {
        #region Singleton
        public static ResourceServices Instance
        {
            get
            {
                if (instance == null) instance = new ResourceServices();
                return instance;
            }
        }
        private static ResourceServices instance { get; set; }
        private ResourceServices()
        {
        }
        #endregion

        public List<Resource> GetResource(string SearchTerm = "")
        {
            using (var context = new DSContext())
            {
                if (SearchTerm != "")
                {
                    return context.Resources.Where(p => p.Name != null && p.Name.ToLower()
                                            .Contains(SearchTerm.ToLower()))
                                            .OrderBy(x => x.Name)
                                            .ToList();
                }
                else
                {
                    return context.Resources.ToList();
                }
            }
        }

        public Resource GetResource(int ID)
        {
            using (var context = new DSContext())
            {

                return context.Resources.Find(ID);

            }
        }

        public void SaveResource(Resource Resource)
        {
            using (var context = new DSContext())
            {
                context.Resources.Add(Resource);
                context.SaveChanges();
            }
        }

        public void UpdateResource(Resource Resource)
        {
            using (var context = new DSContext())
            {
                context.Entry(Resource).State = EntityState.Modified;
                context.SaveChanges();
            }
        }

        public void DeleteResource(int ID)
        {
            using (var context = new DSContext())
            {

                var Resource = context.Resources.Find(ID);
                context.Resources.Remove(Resource);
                context.SaveChanges();
            }
        }
    }
}
