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
    public class GiftCardServices
    {
        #region Singleton
        public static GiftCardServices Instance
        {
            get
            {
                if (instance == null) instance = new GiftCardServices();
                return instance;
            }
        }
        private static GiftCardServices instance { get; set; }
        private GiftCardServices()
        {
        }
        #endregion
        public class ValidationModel
        {
            public int MainID { get; set; }
            public int Count { get; set; }
        }


        public List<GiftCard> GetGiftCard(string SearchTerm = "")
        {
            using (var context = new DSContext())
            {
                if (SearchTerm != "")
                {
                    return context.GiftCards.Where(p => p.Name != null && p.Name.ToLower()
                                            .Contains(SearchTerm.ToLower()))
                                            .OrderBy(x => x.Name)
                                            .ToList();
                }
                else
                {
                    return context.GiftCards.OrderBy(x => x.Name).ToList();
                }
            }
        }

        public GiftCard GetGiftCard(int ID)
        {
            using (var context = new DSContext())
            {

                return context.GiftCards.Find(ID);

            }
        }

        public void SaveGiftCard(GiftCard GiftCard)
        {
            using (var context = new DSContext())
            {
                try
                {
                    context.GiftCards.Add(GiftCard);
                    context.SaveChanges();
                }
                catch (Exception ex)
                {

                    throw;
                }
               
            }
        }

        public void UpdateGiftCard(GiftCard GiftCard)
        {
            using (var context = new DSContext())
            {
                context.Entry(GiftCard).State = EntityState.Modified;
                context.SaveChanges();
            }
        }

        public string DeleteGiftCard(int ID)
        {
            using (var context = new DSContext())
            {
                var query = $"SELECT A.ID AS MainID, COUNT(B.ID) AS Count FROM GiftCards A LEFT JOIN GiftCardAssignments B ON CONCAT(',', B.GiftCardID, ',') LIKE CONCAT('%,', A.ID, ',%') GROUP BY A.ID HAVING COUNT(B.ID) > 0;";
                var DataForValidation = context.Database.SqlQuery<ValidationModel>(query).ToList();
                if (DataForValidation.Any(x => x.MainID == ID))
                {
                    return "Cannot Delete it, because this is linked with gift card assignments";
                }
                else
                {
                    var GiftCard = context.GiftCards.Find(ID);
                    context.GiftCards.Remove(GiftCard);
                    context.SaveChanges();
                    return "Deleted Successfully";
                }
            }
        }







        public List<GiftCardAssignment> GetGiftCardAssignment(string SearchTerm = "")
        {
            using (var context = new DSContext())
            {
                if (SearchTerm != "")
                {
                    return context.GiftCardAssignments.Where(p => p.AssignedCode != null && p.AssignedCode.ToLower()
                                            .Contains(SearchTerm.ToLower()))
                                            .OrderBy(x => x.AssignedCode)
                                            .ToList();
                }
                else
                {
                    return context.GiftCardAssignments.OrderBy(x => x.AssignedCode).ToList();
                }
            }
        }

        public List<GiftCardAssignment> GetGiftCardAssignmentsWRTBusiness(string Business,int CustomerID)
        {
            using (var context = new DSContext())
            {
              
                    return context.GiftCardAssignments.Where(x=>x.Business== Business && x.CustomerID == CustomerID).OrderBy(x => x.AssignedCode).ToList();
                
            }
        }

        public List<GiftCardAssignment> GetGiftCardAssignmentByGiftCardID(int GiftCardID)
        {
            using (var context = new DSContext())
            {

                return context.GiftCardAssignments.Where(x => x.GiftCardID == GiftCardID).ToList();
                
            }
        }

        public GiftCardAssignment GetGiftCardAssignment(int ID)
        {
            using (var context = new DSContext())
            {

                return context.GiftCardAssignments.Find(ID);

            }
        }

        public void SaveGiftCardAssignment(GiftCardAssignment GiftCardAssignment)
        {
            using (var context = new DSContext())
            {
                context.GiftCardAssignments.Add(GiftCardAssignment);
                context.SaveChanges();
            }
        }

        public void UpdateGiftCardAssignment(GiftCardAssignment GiftCardAssignment)
        {
            using (var context = new DSContext())
            {
                context.Entry(GiftCardAssignment).State = EntityState.Modified;
                context.SaveChanges();
            }
        }

        public void DeleteGiftCardAssignment(int ID)
        {
            using (var context = new DSContext())
            {

                var GiftCardAssignment = context.GiftCardAssignments.Find(ID);
                context.GiftCardAssignments.Remove(GiftCardAssignment);
                context.SaveChanges();
            }
        }
    }
}
