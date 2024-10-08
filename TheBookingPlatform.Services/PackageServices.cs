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
    public class PackageServices
    {
        #region Singleton
        public static PackageServices Instance
        {
            get
            {
                if (instance == null) instance = new PackageServices();
                return instance;
            }
        }
        private static PackageServices instance { get; set; }
        private PackageServices()
        {
        }
        #endregion



        public List<Package> GetPackage(string SearchTerm = "")
        {
            using (var context = new DSContext())
            {
                if (SearchTerm != "")
                {
                    return context.Packages.Where(p => p.Name != null && p.Name.ToLower()
                                            .Contains(SearchTerm.ToLower()))
                                            .OrderBy(x => x.Name)
                                            .ToList();
                }
                else
                {
                    return context.Packages.OrderBy(x => x.Name).ToList();
                }
            }
        }


        public Package GetPackage(int ID)
        {
            using (var context = new DSContext())
            {

                return context.Packages.Find(ID);

            }
        }

        public void SavePackage(Package Package)
        {
            using (var context = new DSContext())
            {
                try
                {
                    context.Packages.Add(Package);
                    context.SaveChanges();
                }
                catch (Exception ex)
                {

                    throw;
                }

            }
        }

        public void UpdatePackage(Package Package)
        {
            using (var context = new DSContext())
            {
                context.Entry(Package).State = EntityState.Modified;
                context.SaveChanges();
            }
        }

        public string DeletePackage(int ID)
        {
            using (var context = new DSContext())
            {
                var Package = context.Packages.Find(ID);
                context.Packages.Remove(Package);
                context.SaveChanges();
                return "Deleted Successfully";

            }
        }



    }

}
