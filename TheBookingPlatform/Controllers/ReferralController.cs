using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TheBookingPlatform.Services;
using TheBookingPlatform.ViewModels;

namespace TheBookingPlatform.Controllers
{
    public class ReferralController : Controller
    {
        // GET: Referral
        #region UserManagerRegion
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

        public ReferralController()
        {
        }

        public ReferralController(AMUserManager userManager, AMSignInManager signInManager)
        {
            UserManager = userManager;
            SignInManager = signInManager;
        }

        #endregion
        public ActionResult Index()
        {
            ReferralListingViewModel model = new ReferralListingViewModel();
            var listofreferrals = new List<ReferralListModel>();
            var LoggedInUser = UserManager.FindById(User.Identity.GetUserId());
            var referrals = ReferralServices.Instance.GetReferralWRTBusiness(LoggedInUser.Company);
            foreach ( var item in referrals)
            {
                var customer = CustomerServices.Instance.GetCustomer(item.CustomerID);
                var referredby = CustomerServices.Instance.GetCustomer(item.ReferredBy);
                listofreferrals.Add(new ReferralListModel { Customer = customer,ReferredCustomer = referredby,Referral = item });
            }
            return View(model);
        }
    }
}