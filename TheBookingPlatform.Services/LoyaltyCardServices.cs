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
    public class LoyaltyCardServices
    {
        #region Singleton
        public static LoyaltyCardServices Instance
        {
            get
            {
                if (instance == null) instance = new LoyaltyCardServices();
                return instance;
            }
        }
        private static LoyaltyCardServices instance { get; set; }
        private LoyaltyCardServices()
        {
        }
        #endregion

        public class ValidationModel
        {
            public int MainID { get; set; }
            public int Count { get; set; }
        }

        public List<LoyaltyCard> GetLoyaltyCard(string SearchTerm = "")
        {
            using (var context = new DSContext())
            {
                if (SearchTerm != "")
                {
                    return context.LoyaltyCards.Where(p => p.Name != null && p.Name.ToLower()
                                            .Contains(SearchTerm.ToLower()))
                                            .OrderBy(x => x.Name)
                                            .ToList();
                }
                else
                {
                    return context.LoyaltyCards.OrderBy(x => x.Name).ToList();
                }
            }
        }

        public LoyaltyCard GetLoyaltyCard(int ID)
        {
            using (var context = new DSContext())
            {

                return context.LoyaltyCards.Find(ID);

            }
        }

        public void SaveLoyaltyCard(LoyaltyCard LoyaltyCard)
        {
            using (var context = new DSContext())
            {
                context.LoyaltyCards.Add(LoyaltyCard);
                context.SaveChanges();
            }
        }

        public void UpdateLoyaltyCard(LoyaltyCard LoyaltyCard)
        {
            using (var context = new DSContext())
            {
                context.Entry(LoyaltyCard).State = EntityState.Modified;
                context.SaveChanges();
            }
        }

        public string DeleteLoyaltyCard(int ID)
        {
            using (var context = new DSContext())
            {
                var query = $"SELECT A.ID AS MainID, COUNT(B.ID) AS Count FROM LoyaltyCards A LEFT JOIN LoyaltyCardAssignments B ON CONCAT(',', B.LoyaltyCardID, ',') LIKE CONCAT('%,', A.ID, ',%') GROUP BY A.ID HAVING COUNT(B.ID) > 0;";
                var DataForValidation = context.Database.SqlQuery<ValidationModel>(query).ToList();
                if (DataForValidation.Any(x => x.MainID == ID))
                {
                    return "Cannot Delete it, because this is linked with loyalty card assignments";
                }

               
                else
                {
                    var LoyaltyCard = context.LoyaltyCards.Find(ID);
                    context.LoyaltyCards.Remove(LoyaltyCard);
                    context.SaveChanges();
                    return "Deleted Successfully";

                }
            }
        }




        public List<LoyaltyCardPromotion> GetLoyaltyCardPromotions(int LoyaltyCardID)
        {
            using (var context = new DSContext())
            {
                return context.LoyaltyCardPromotions.Where(x => x.LoyaltyCardID == LoyaltyCardID).ToList();
            }
        }
        public List<LoyaltyCardPromotion> GetLoyaltyCardPromotions()
        {
            using (var context = new DSContext())
            {
                return context.LoyaltyCardPromotions.ToList();
            }
        }

        public LoyaltyCardPromotion GetLoyaltyCardPromotion(int ID)
        {
            using (var context = new DSContext())
            {

                return context.LoyaltyCardPromotions.Find(ID);

            }
        }

        public void SaveLoyaltyCardPromotion(LoyaltyCardPromotion LoyaltyCardPromotion)
        {
            using (var context = new DSContext())
            {
                context.LoyaltyCardPromotions.Add(LoyaltyCardPromotion);
                context.SaveChanges();
            }
        }

        public void UpdateLoyaltyCardPromotion(LoyaltyCardPromotion LoyaltyCardPromotion)
        {
            using (var context = new DSContext())
            {
                context.Entry(LoyaltyCardPromotion).State = EntityState.Modified;
                context.SaveChanges();
            }
        }

        public void DeleteLoyaltyCardPromotion(int ID)
        {
            using (var context = new DSContext())
            {

               
                var LoyaltyCardPromotion = context.LoyaltyCardPromotions.Find(ID);
                context.LoyaltyCardPromotions.Remove(LoyaltyCardPromotion);
                context.SaveChanges();
            }
        }










        public List<LoyaltyCardAssignment> GetLoyaltyCardAssignments()
        {
            using (var context = new DSContext())
            {
                return context.LoyaltyCardAssignments.ToList();
            }
        }

        public List<LoyaltyCardAssignment> GetLoyaltyCardAssignmentsWRTbBusinessAndCustomer(string Business,int customerID)
        {
            using (var context = new DSContext())
            {
                return context.LoyaltyCardAssignments.Where(x=>x.Business == Business && x.CustomerID == customerID).ToList();
            }
        }

        public LoyaltyCardAssignment GetLoyaltyCardAssignmentByCustomerID(int CustomerID)
        {
            using (var context = new DSContext())
            {
                return context.LoyaltyCardAssignments.Where(x=>x.CustomerID == CustomerID).FirstOrDefault();    
            }
        }


        public LoyaltyCardAssignment GetLoyaltyCardAssignment(int ID)
        {
            using (var context = new DSContext())
            {

                return context.LoyaltyCardAssignments.Find(ID);

            }
        }

        public void SaveLoyaltyCardAssignment(LoyaltyCardAssignment LoyaltyCardAssignment)
        {
            using (var context = new DSContext())
            {
                context.LoyaltyCardAssignments.Add(LoyaltyCardAssignment);
                context.SaveChanges();
            }
        }

        public void UpdateLoyaltyCardAssignment(LoyaltyCardAssignment LoyaltyCardAssignment)
        {
            using (var context = new DSContext())
            {
                context.Entry(LoyaltyCardAssignment).State = EntityState.Modified;
                context.SaveChanges();
            }
        }

        public void DeleteLoyaltyCardAssignment(int ID)
        {
            using (var context = new DSContext())
            {

                var LoyaltyCardAssignment = context.LoyaltyCardAssignments.Find(ID);
                context.LoyaltyCardAssignments.Remove(LoyaltyCardAssignment);
                context.SaveChanges();
            }
        }














      


    }
}
