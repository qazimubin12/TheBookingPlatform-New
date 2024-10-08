using Microsoft.AspNet.Identity.Owin;
using Microsoft.AspNet.Identity;
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
    public class HolidayController : Controller
    {
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
        public HolidayController()
        {
        }

        public HolidayController(AMUserManager userManager, AMSignInManager signInManager)
        {
            UserManager = userManager;
            SignInManager = signInManager;
        }


        // GET: Holiday
        public ActionResult Index(string SearchTerm = "")
        {
            HolidayListingViewModel model = new HolidayListingViewModel();
            var loggedInUser = UserManager.FindById(User.Identity.GetUserId());
            model.SearchTerm = SearchTerm;
            if (loggedInUser.Role == "Super Admin")
            {
                model.Holidays = HolidayServices.Instance.GetHoliday(SearchTerm);
            }
            else
            {
                model.Holidays = HolidayServices.Instance.GetHolidayWRTBusiness(loggedInUser.Company, SearchTerm);
            }
            return View(model);
        }


        [HttpGet]
        public ActionResult Action(int ID = 0)
        {
            HolidayActionViewModel model = new HolidayActionViewModel();
            var loggedInUser = UserManager.FindById(User.Identity.GetUserId());

            if (ID != 0)
            {
                var holiday = HolidayServices.Instance.GetHoliday(ID);
                model.Name = holiday.Name;
                model.ID = holiday.ID;
                model.Date = holiday.Date;


            }
            return View(model);
        }


        [HttpPost]
        public JsonResult Action(HolidayActionViewModel model)
        {
            var loggedInUser = UserManager.FindById(User.Identity.GetUserId());

            if (model.ID != 0)
            {
                var holiday = HolidayServices.Instance.GetHoliday(model.ID);
                holiday.Name = model.Name;
                holiday.Date = model.Date;
                holiday.ID = model.ID;
                HolidayServices.Instance.UpdateHoliday(holiday);
                return Json(new { success = true }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                var holiday = new Holiday();
                holiday.Name = model.Name;
                holiday.Date = model.Date;
                holiday.Business = loggedInUser.Company;
                HolidayServices.Instance.SaveHoliday(holiday);
                return Json(new { success = true }, JsonRequestBehavior.AllowGet);
            }
        }


        [HttpGet]
        public ActionResult Delete(int ID)
        {
            HolidayActionViewModel model = new HolidayActionViewModel();
            var holiday = HolidayServices.Instance.GetHoliday(ID);
            model.ID = holiday.ID;
            model.Name = holiday.Name;
            return PartialView("_Delete", model);
        }


        [HttpPost]
        public JsonResult Delete(HolidayActionViewModel model)
        {
            var holiday = HolidayServices.Instance.GetHoliday(model.ID);
            if (holiday != null)
            {
                HolidayServices.Instance.DeleteHoliday(holiday.ID);
                return Json(new { success = true, Message = "Deleted Successfully" }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(new { success = false, Message = "No Holiday Found" }, JsonRequestBehavior.AllowGet);
            }
        }
    }
}