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
    public class ReviewServices
    {
        #region Singleton
        public static ReviewServices Instance
        {
            get
            {
                if (instance == null) instance = new ReviewServices();
                return instance;
            }
        }
        private static ReviewServices instance { get; set; }
        private ReviewServices()
        {
        }
        #endregion
       
        public List<Review> GetReview(string SearchTerm = "")
        {
            using (var context = new DSContext())
            {
                if (SearchTerm != "")
                {
                    return context.Reviews.Where(p => p.Feedback != null && p.Feedback.ToLower()
                                            .Contains(SearchTerm.ToLower()))
                                            .OrderBy(x => x.Date)
                                            .ToList();
                }
                else
                {
                    return context.Reviews.OrderBy(x => x.Date).ToList();
                }
            }
        }
       
        public List<Review> GetReviewWRTBusiness(string Business, string SearchTerm = "")
        {
            using (var context = new DSContext())
            {
                if (SearchTerm != "")
                {
                    return context.Reviews.AsNoTracking().Where(p => p.Business == Business && p.CustomerID != 0 && p.Feedback != null && p.Feedback.ToLower()
                                            .Contains(SearchTerm.ToLower()))
                                            .OrderByDescending(x => x.Date)
                                            .ToList();
                }
                else
                {
                    return context.Reviews.AsNoTracking().Where(x => x.Business == Business && x.CustomerID != 0).OrderByDescending(x => x.Date).ToList();
                }
            }
        }






        public Review GetReview(int ID)
        {
            using (var context = new DSContext())
            {

                return context.Reviews.Find(ID);

            }
        }

        public void SaveReview(Review Review)
        {
            try
            {
                using (var context = new DSContext())
                {
                    context.Reviews.Add(Review);
                    context.SaveChanges();
                }
            }
            catch (Exception ex)
            {

                throw;
            }
           
        }

        public void UpdateReview(Review Review)
        {
            using (var context = new DSContext())
            {
                context.Entry(Review).State = EntityState.Modified;
                context.SaveChanges();
            }
        }

        public string DeleteReview(int ID)
        {
            using (var context = new DSContext())
            {

                var Review = context.Reviews.Find(ID);
                context.Reviews.Remove(Review);
                context.SaveChanges();
                return "Deleted Successfully";

            }
        }
    }
}
