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
    public class FileServices
    {
        #region Singleton
        public static FileServices Instance
        {
            get
            {
                if (instance == null) instance = new FileServices();
                return instance;
            }
        }
        private static FileServices instance { get; set; }
        private FileServices()
        {
        }
        #endregion

        public List<File> GetFile(string SearchTerm = "")
        {
            using (var context = new DSContext())
            {
                if (SearchTerm != "")
                {
                    return context.Files.Where(p => p.Name != null && p.Name.ToLower()
                                            .Contains(SearchTerm.ToLower()))
                                            .OrderBy(x => x.Name)
                                            .ToList();
                }
                else
                {
                    return context.Files.ToList();
                }
            }
        }

        public List<File> GetFileWRTBusiness(string Business,int CustomerID)
        {
            using (var context = new DSContext())
            {
             
                
                    return context.Files.Where(X=>X.Business == Business && X.CustomerID == CustomerID).ToList();
                
            }
        }

        public File GetFile(int ID)
        {
            using (var context = new DSContext())
            {

                return context.Files.Find(ID);

            }
        }

        public void SaveFile(File File)
        {
            using (var context = new DSContext())
            {
                context.Files.Add(File);
                context.SaveChanges();
            }
        }

        public void UpdateFile(File File)
        {
            using (var context = new DSContext())
            {
                context.Entry(File).State = EntityState.Modified;
                context.SaveChanges();
            }
        }

        public void DeleteFile(int ID)
        {
            using (var context = new DSContext())
            {

                var File = context.Files.Find(ID);
                context.Files.Remove(File);
                context.SaveChanges();
            }
        }
    }
}
