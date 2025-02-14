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
    public class StripeInvoicesController : Controller
    {
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

        public StripeInvoicesController()
        {
        }

        public StripeInvoicesController(AMUserManager userManager, AMSignInManager signInManager)
        {
            UserManager = userManager;
            SignInManager = signInManager;
        }

        #endregion
        // GET: StripeInvoices
        public ActionResult Index()
        {
            var LoggedInUser= UserManager.FindById(User.Identity.GetUserId());
            StripeInvoicesListingViewModel model = new StripeInvoicesListingViewModel();
            var payments = PaymentServices.Instance.GetPaymentWRTBusiness(LoggedInUser.Company).OrderBy(x=>x.LastPaidDate);
            var PaymentModels = new List<StripeInvoiceModel>();
            foreach (var item in payments)
            {
                var package = PackageServices.Instance.GetPackage(item.PackageID);
                var user = UserManager.FindById(item.UserID);
                PaymentModels.Add(new StripeInvoiceModel { Payment = item, Package = package, User = user });

            }
            model.Company = CompanyServices.Instance.GetCompanyByName(LoggedInUser.Company);
            model.Payments = PaymentModels;
            return View(model);
        }
    }
}