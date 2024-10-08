using TheBookingPlatform.Database;
using TheBookingPlatform.Entities;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Mail;
using System.Configuration;
using System.Net;

namespace TheBookingPlatform.Services
{
    public class EmailTemplateServices
    {
        #region Singleton
        public static EmailTemplateServices Instance
        {
            get
            {
                if (instance == null) instance = new EmailTemplateServices();
                return instance;
            }
        }
        private static EmailTemplateServices instance { get; set; }
        private EmailTemplateServices()
        {
        }
        #endregion



        public List<EmailTemplate> GetEmailTemplate(string SearchTerm = "")
        {
            using (var context = new DSContext())
            {
                if (SearchTerm != "")
                {
                    return context.EmailTemplates.Where(p => p.Name != null && p.Name.ToLower()
                                               .Contains(SearchTerm)).OrderBy(x => x.Name)
                                               .ToList();

                }
                else
                {
                    return context.EmailTemplates.ToList();
                }
            }
        }

        public EmailTemplate GetEmailTemplateWRTBusiness(string Business,string Name)
        {
            using (var context = new DSContext())
            {
                
                    return context.EmailTemplates.Where(x=>x.Business == Business && x.Name == Name).FirstOrDefault();
                
            }
        }

        public EmailTemplate GetEmailTemplate(int ID)
        {
            using (var context = new DSContext())
            {

                return context.EmailTemplates.Find(ID);

            }
        }

        public void SaveEmailTemplate(EmailTemplate EmailTemplate)
        {
            using (var context = new DSContext())
            {
                context.EmailTemplates.Add(EmailTemplate);
                context.SaveChanges();
            }
        }

        public void UpdateEmailTemplate(EmailTemplate EmailTemplate)
        {
            using (var context = new DSContext())
            {
                context.Entry(EmailTemplate).State = EntityState.Modified;
                context.SaveChanges();
            }
        }

        public void DeleteEmailTemplate(int ID)
        {
            using (var context = new DSContext())
            {

                var EmailTemplate = context.EmailTemplates.Find(ID);
                context.EmailTemplates.Remove(EmailTemplate);
                context.SaveChanges();
            }
        }
    }
}
