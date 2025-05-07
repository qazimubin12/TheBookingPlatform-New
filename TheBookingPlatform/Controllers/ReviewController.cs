using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Stripe;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Windows.Media;
using TheBookingPlatform.Entities;
using TheBookingPlatform.Services;
using TheBookingPlatform.ViewModels;

namespace TheBookingPlatform.Controllers
{
    public class ReviewController : Controller
    {
        // GET: Review

        private AMSignInManager _signInManager;
        private AMUserManager _userManager;
        public AMSignInManager SignInManager
        {
            get
            {
                return _signInManager ?? HttpContext.GetOwinContext().Get<AMSignInManager>();
            }
            private set
            {
                _signInManager = value;
            }
        }
        public AMUserManager UserManager
        {
            get
            {
                return _userManager ?? HttpContext.GetOwinContext().GetUserManager<AMUserManager>();
            }
            private set
            {
                _userManager = value;
            }
        }
        private AMRolesManager _rolesManager;
        public AMRolesManager RolesManager
        {
            get
            {
                return _rolesManager ?? HttpContext.GetOwinContext().GetUserManager<AMRolesManager>();
            }
            private set
            {
                _rolesManager = value;
            }
        }
        public ReviewController()
        {
        }
        public ReviewController(AMUserManager userManager, AMSignInManager signInManager)
        {
            UserManager = userManager;
            SignInManager = signInManager;
        }

        [HttpGet]
        public JsonResult GetMoreReviews(int skip = 0, int take = 50,string Selectedoption = "")
        {
            var reviewModel = new List<ReviewModel>();
            var loggedInUser = UserManager.FindById(User.Identity.GetUserId());
            if (Selectedoption == "Reviewed")
            {
                var reviews = ReviewServices.Instance.GetReviewWRTBusiness(loggedInUser.Company, "").Where(x => x.Rating > 0).OrderByDescending(r => r.Date).Skip(skip).Take(take).ToList();
                var customers = CustomerServices.Instance.GetCustomerWRTBusiness(reviews.Select(x => x.CustomerID).ToList());
                var employees = EmployeeServices.Instance.GetBulkEmployees(reviews.Select(x => x.EmployeeID).ToList());
                foreach (var item in reviews)
                {

                    var customer = customers.Where(x => x.ID == item.CustomerID).FirstOrDefault();
                    var employee = employees.Where(x => x.ID == item.EmployeeID).FirstOrDefault();
                    reviewModel.Add(new ReviewModel { Review = item, CustomerName = customer.FirstName + " " + customer.LastName, EmployeeName = employee?.Name, Type = "Reviewed", Date = item.Date.ToString("yyyy-MM-dd") });

                }

            }
            else if (Selectedoption == "Not Reviewed")
            {
                var reviews = ReviewServices.Instance.GetReviewWRTBusiness(loggedInUser.Company, "").Where(x => x.Rating == 0).OrderByDescending(r => r.Date).Skip(skip).Take(take).ToList();
                var customers = CustomerServices.Instance.GetCustomerWRTBusiness(reviews.Select(x => x.CustomerID).ToList());
                var employees = EmployeeServices.Instance.GetBulkEmployees(reviews.Select(x => x.CustomerID).ToList());
                foreach (var item in reviews)
                {

                    var customer = customers.Where(x => x.ID == item.CustomerID).FirstOrDefault();
                    var employee = employees.Where(x => x.ID == item.EmployeeID).FirstOrDefault();
                    reviewModel.Add(new ReviewModel { Review = item, CustomerName = customer.FirstName + " " + customer.LastName, EmployeeName = employee?.Name, Type = "NotReviewed", Date = item.Date.ToString("yyyy-MM-dd") });

                }

            }
            else
            {
                var reviews = ReviewServices.Instance.GetReviewWRTBusiness(loggedInUser.Company, "").OrderByDescending(r => r.Date).Skip(skip).Take(take).ToList();
                var customers = CustomerServices.Instance.GetCustomerWRTBusiness(reviews.Select(x => x.CustomerID).ToList());
                var employees = EmployeeServices.Instance.GetBulkEmployees(reviews.Select(x => x.EmployeeID).ToList());
                foreach (var item in reviews)
                {
                    if (item.Rating > 0)
                    {

                        var customer = customers.Where(x => x.ID == item.CustomerID).FirstOrDefault();
                        var employee = employees.Where(x => x.ID == item.EmployeeID).FirstOrDefault();
                        reviewModel.Add(new ReviewModel { Review = item, CustomerName = customer.FirstName + " " + customer.LastName, EmployeeName = employee?.Name, Type = "Reviewed", Date = item.Date.ToString("yyyy-MM-dd") });
                    }
                    else
                    {
                        var customer = customers.Where(x => x.ID == item.CustomerID).FirstOrDefault();
                        var employee = employees.Where(x => x.ID == item.EmployeeID).FirstOrDefault();
                        reviewModel.Add(new ReviewModel { Review = item, CustomerName = customer.FirstName + " " + customer.LastName, EmployeeName =   employee.Name, Type = "NotReviewed",Date = item.Date.ToString("yyyy-MM-dd") });
                    }
                }

            }

            //model.Reviews = reviewModel;
            return Json(reviewModel, JsonRequestBehavior.AllowGet);
        }


        [HttpGet]
        public void Track(int AppointmentID)
        {
            var rebook = RebookReminderServices.Instance.GetRebookReminderWRTBusiness(AppointmentID);
            rebook.Opened = true;
            RebookReminderServices.Instance.UpdateRebookReminder(rebook);
        }

        public ActionResult Index(string SearchTerm ="")
        {
            SettingsViewModel model = new SettingsViewModel();
            var loggedInUser = UserManager.FindById(User.Identity.GetUserId());       
            return View(model);
        }

        [HttpGet]
        public JsonResult GetReviews(string SearchTerm = "")
        {
            var LoggedInUser = UserManager.FindById(User.Identity.GetUserId());
            var reviewsModel = new List<ReviewModel>();
            if (LoggedInUser.Role != "Super Admin")
            {
                var reviews = ReviewServices.Instance.GetReviewWRTBusiness(LoggedInUser.Company, SearchTerm);
                foreach (var item in reviews)
                {
                    var customer = CustomerServices.Instance.GetCustomer(item.CustomerID);
                    var employee = EmployeeServices.Instance.GetEmployee(item.EmployeeID);
                    reviewsModel.Add(new ReviewModel { Review = item, CustomerName = customer.FirstName + " " + customer.LastName, EmployeeName = employee.Name });
                }
            }
            else
            {
                var reviews = ReviewServices.Instance.GetReview(SearchTerm);
                foreach (var item in reviews)
                {
                    var customer = CustomerServices.Instance.GetCustomer(item.CustomerID);
                    var employee = EmployeeServices.Instance.GetEmployee(item.EmployeeID);
                    reviewsModel.Add(new ReviewModel { Review = item, CustomerName = customer.FirstName + " " + customer.LastName, EmployeeName = employee.Name });
                }
            }

            // Return data as JSON
            return Json(reviewsModel, JsonRequestBehavior.AllowGet);
        }



        [HttpGet]
        public ActionResult ProvideFeedback(string businessName,int ID,int AppointmentID,int EmployeeID,int CustomerID)
        {
            var company = CompanyServices.Instance.GetCompany().Where(X=>X.Business == businessName).FirstOrDefault();
            BookingViewModel model = new BookingViewModel();
            var review = ReviewServices.Instance.GetReview(ID);
            if (review.Rating == 0 && review.Rating == null)
            {
                return RedirectToAction("Welcome", "Booking", new {businessName =  businessName});
            }
            else
            {
                review.EmailOpened = true;
                review.EmailClicked = true;
                ReviewServices.Instance.UpdateReview(review);

                model.Company = company;
                model.ReviewID = ID;
                model.Employee = EmployeeServices.Instance.GetEmployee(EmployeeID);
                model.AppointmentID = AppointmentID;
                model.EmployeeID = EmployeeID;
                model.CustomerID = CustomerID;
                return View("ProvideFeedback", "_BookingLayout", model);
            }
        }

        [HttpPost]
        public JsonResult SubmitReview(int ReviewID, string Feedback,float Rating)
        {
            var Review = ReviewServices.Instance.GetReview(ReviewID);
            Review.Feedback = Feedback;
            Review.Rating = Rating;
            ReviewServices.Instance.UpdateReview(Review);
            return Json(new { success = true }, JsonRequestBehavior.AllowGet);

        }


        [HttpGet]
        public ActionResult Delete(int ID)
        {
            ReviewActionViewModel model = new ReviewActionViewModel();
            var Review = ReviewServices.Instance.GetReview(ID);
            model.ID = Review.ID;
            return PartialView("_Delete", model);
        }


        [HttpPost]
        public ActionResult Delete(ReviewActionViewModel model)
        {
            var Review = ReviewServices.Instance.GetReview(model.ID);
            ReviewServices.Instance.DeleteReview(Review.ID);
            return Json(new { success = true }, JsonRequestBehavior.AllowGet);
        }
    }
}