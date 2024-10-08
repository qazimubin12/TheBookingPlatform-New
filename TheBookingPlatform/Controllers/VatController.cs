using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TheBookingPlatform.Entities;
using TheBookingPlatform.Services;
using TheBookingPlatform.ViewModels;

namespace TheBookingPlatform.Controllers
{
    public class VatController : Controller
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

        public VatController()
        {
        }

        public VatController(AMUserManager userManager, AMSignInManager signInManager)
        {
            UserManager = userManager;
            SignInManager = signInManager;
        }

        #endregion
        // GET: Vat

        [NoCache]
        public ActionResult Index(string SearchTerm = "")
        {
            VatListingViewModel model = new VatListingViewModel();
                        var LoggedInUser = UserManager.FindById(User.Identity.GetUserId()); if(LoggedInUser == null) { return RedirectToAction("Login", "Account"); }
            if (LoggedInUser.Role == "Super Admin")
            {
                model.Vats = VatServices.Instance.GetVat(SearchTerm);
            }
            else
            {
                model.Vats = VatServices.Instance.GetVat(SearchTerm).Where(x=>x.Business == LoggedInUser.Company).ToList();
            }
            return View(model);
        }



        [HttpGet]
        public ActionResult Action(int ID = 0)
        {
            VatActionViewModel model = new VatActionViewModel();
            if (ID != 0)
            {
                var Vat = VatServices.Instance.GetVat(ID);
                model.ID = Vat.ID;
                model.Name = Vat.Name;
                model.Percentage = Vat.Percentage;

            }
            return PartialView("_Action", model);
        }


        [HttpPost]
        public ActionResult Action(VatActionViewModel model)
        {
            if (model.ID != 0)
            {
                var Vat = VatServices.Instance.GetVat(model.ID);
                Vat.ID = model.ID;
                Vat.Name = model.Name;
                Vat.Percentage = model.Percentage;

                VatServices.Instance.UpdateVat(Vat);
            }
            else
            {
                var Vat = new Vat();
                            var LoggedInUser = UserManager.FindById(User.Identity.GetUserId()); if(LoggedInUser == null) { return RedirectToAction("Login", "Account"); }
                if (LoggedInUser.Role != "Super Admin")
                {
                    Vat.Business = LoggedInUser.Company;
                }
                Vat.Name = model.Name;
                Vat.Percentage= model.Percentage;
                VatServices.Instance.SaveVat(Vat);
            }
            return Json(new { success = true }, JsonRequestBehavior.AllowGet);
        }



        [HttpGet]
        public ActionResult Delete(int ID)
        {
            VatActionViewModel model = new VatActionViewModel();
            var Vat = VatServices.Instance.GetVat(ID);
            model.ID = Vat.ID;
            return PartialView("_Delete", model);
        }


        [HttpPost]
        public ActionResult Delete(VatActionViewModel model)
        {
            var Vat = VatServices.Instance.GetVat(model.ID);
            VatServices.Instance.DeleteVat(Vat.ID);

            return Json(new { success = true }, JsonRequestBehavior.AllowGet);
        }
    }
}