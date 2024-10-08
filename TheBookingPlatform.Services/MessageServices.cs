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
    public class MessageServices
    {
        #region Singleton
        public static MessageServices Instance
        {
            get
            {
                if (instance == null) instance = new MessageServices();
                return instance;
            }
        }
        private static MessageServices instance { get; set; }
        private MessageServices()
        {
        }
        #endregion

        public List<Message> GetMessage(string SearchTerm = "")
        {
            using (var context = new DSContext())
            {
                if (SearchTerm != "")
                {
                    return context.Messages.Where(p => p.Type != null && p.Type.ToLower()
                                             .Contains(SearchTerm))
                                             .ToList();

                }
                else
                {
                    return context.Messages.OrderBy(x=>x.Type).ToList();
                }
            }
        }

        public List<Message> GetMessageWRTBusiness(string BusiessName,int CustomerID)
        {
            using (var context = new DSContext())
            {
                    return context.Messages.Where(x=>x.Business == BusiessName && x.CustomerID == CustomerID).OrderBy(x => x.Type).ToList();
                
            }
        }

        public Message GetMessage(int ID)
        {
            using (var context = new DSContext())
            {

                return context.Messages.Find(ID);

            }
        }

        public void SaveMessage(Message Message)
        {
            using (var context = new DSContext())
            {
                context.Messages.Add(Message);
                context.SaveChanges();
            }
        }

        public void UpdateMessage(Message Message)
        {
            using (var context = new DSContext())
            {
                context.Entry(Message).State = EntityState.Modified;
                context.SaveChanges();
            }
        }

        public void DeleteMessage(int ID)
        {
            using (var context = new DSContext())
            {

                var Message = context.Messages.Find(ID);
                context.Messages.Remove(Message);
                context.SaveChanges();
            }
        }
    }
}
