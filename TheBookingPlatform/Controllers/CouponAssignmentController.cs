using Microsoft.AspNet.Identity.Owin;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TheBookingPlatform.Services;
using TheBookingPlatform.ViewModels;

namespace TheBookingPlatform.Controllers
{
    public class CouponAssignmentController : Controller
    {
        // GET: CouponAssignmentAssignment
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
        public CouponAssignmentController()
        {
        }



        public CouponAssignmentController(AMUserManager userManager, AMSignInManager signInManager)
        {
            UserManager = userManager;
            SignInManager = signInManager;
        }
        // GET: CouponAssignment
        public ActionResult Index()
        {
            CouponAssignmentListingViewModel model = new CouponAssignmentListingViewModel();
            var LoggedInUser = UserManager.FindById(User.Identity.GetUserId());
            var ListofCouponAssignemt = new List<CouponAssignmentModel>();
            if (LoggedInUser.Role != "Super Admin")
            {
                var couponAssignment = CouponServices.Instance.GetCouponAssignmentsWRTBusiness(LoggedInUser.Company);
                foreach (var item in couponAssignment)
                {
                    var coupon = CouponServices.Instance.GetCoupon(item.CouponID);
                    if (coupon != null)
                    {
                        var customer = CustomerServices.Instance.GetCustomer(item.CustomerID);
                        ListofCouponAssignemt.Add(new CouponAssignmentModel { Coupon = coupon, CouponAssignment = item, Customer = customer });
                    }
                }
                model.CouponAssignments = ListofCouponAssignemt;
            }
            else
            {
                var couponAssignment = CouponServices.Instance.GetCouponAssignment();
                foreach (var item in couponAssignment)
                {
                    var coupon = CouponServices.Instance.GetCoupon(item.CouponID);
                    if (coupon != null)
                    {
                        var customer = CustomerServices.Instance.GetCustomer(item.CustomerID);
                        ListofCouponAssignemt.Add(new CouponAssignmentModel { Coupon = coupon, CouponAssignment = item, Customer = customer });
                    }
                }
                model.CouponAssignments = ListofCouponAssignemt;

            }

            return View(model);
        }

        [HttpGet]
        public ActionResult Action(int ID = 0)
        {
            CouponAssignmentActionViewModel model = new CouponAssignmentActionViewModel();
            var LoggedInUser = UserManager.FindById(User.Identity.GetUserId());
            if (LoggedInUser.Role != "Super Admin")
            {
                model.Customers = CustomerServices.Instance.GetCustomerWRTBusiness(LoggedInUser.Company);
                model.Coupons = CouponServices.Instance.GetCouponWRTBusiness(LoggedInUser.Company);
            }
            else
            {
                model.Customers = CustomerServices.Instance.GetCustomer();
                model.Coupons = CouponServices.Instance.GetCoupon();
            }
            if (ID != 0)
            {
                var CouponAssignment = CouponServices.Instance.GetCouponAssignment(ID);
                if (CouponAssignment != null)
                {
                    model.ID = CouponAssignment.ID;
                    model.CouponID = CouponAssignment.CouponID;
                    model.CustomerID = CouponAssignment.CustomerID;
                    model.Customer = CustomerServices.Instance.GetCustomer(model.CustomerID);
                    model.AssignedDate = CouponAssignment.AssignedDate;
                    model.Used = CouponAssignment.Used;
                }
            }
            return View(model);
        }

        [HttpPost]
        public JsonResult Action(CouponAssignmentActionViewModel model)
        {
            var LoggedInUser = UserManager.FindById(User.Identity.GetUserId());

            if (model.ID != 0)
            {
                var CouponAssignment = CouponServices.Instance.GetCouponAssignment(model.ID);
                CouponAssignment.ID = model.ID;
                CouponAssignment.CouponID = model.CouponID;
                CouponAssignment.CustomerID = model.CustomerID;
                CouponAssignment.AssignedDate = model.AssignedDate;
                CouponAssignment.Business = LoggedInUser.Company;
                CouponAssignment.Used = model.Used;
                CouponServices.Instance.UpdateCouponAssignment(CouponAssignment);
                return Json(new { success = true }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                var CouponAssignment = new Entities.CouponAssignment();
                CouponAssignment.CouponID = model.CouponID;
                CouponAssignment.CustomerID = model.CustomerID;
                CouponAssignment.AssignedDate = model.AssignedDate;
                CouponAssignment.Used = model.Used;
                CouponAssignment.Business = LoggedInUser.Company;
                CouponServices.Instance.SaveCouponAssignment(CouponAssignment);
                return Json(new { success = true }, JsonRequestBehavior.AllowGet);
            }



        }


        [HttpGet]
        public ActionResult Delete(int ID)
        {
            CouponAssignmentActionViewModel model = new CouponAssignmentActionViewModel();
            var CouponAssignment = CouponServices.Instance.GetCouponAssignment(ID);
            model.ID = CouponAssignment.ID;
            return PartialView("_Delete", model);
        }

        [HttpPost]
        public ActionResult Delete(CouponAssignmentActionViewModel model)
        {
            var CouponAssignment = CouponServices.Instance.GetCouponAssignment(model.ID);

            CouponServices.Instance.DeleteCouponAssignment(CouponAssignment.ID);

            return Json(new { success = true }, JsonRequestBehavior.AllowGet);
        }
    }
}