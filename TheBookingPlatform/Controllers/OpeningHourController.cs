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
    public class OpeningHourController : Controller
    {
        // GET: OpeningHour
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

        public OpeningHourController()
        {
        }

        public OpeningHourController(AMUserManager userManager, AMSignInManager signInManager)
        {
            UserManager = userManager;
            SignInManager = signInManager;
        }

        #endregion
        public ActionResult Index()
        {
            OpeningHourListingViewModel model = new OpeningHourListingViewModel();
            var LoggedInUser = UserManager.FindById(User.Identity.GetUserId());
            if (LoggedInUser != null)
            {
                if (LoggedInUser.Role != "Super Admin")
                {
                    model.OpeningHours = OpeningHourServices.Instance.GetOpeningHour().Where(x=>x.Business == LoggedInUser.Company).ToList();

                }
                else
                {
                    model.OpeningHours = OpeningHourServices.Instance.GetOpeningHour();
                }
                return View(model);
            }
            else
            {
                return RedirectToAction("Login", "Account");
            }
        }


        [HttpGet]
        public ActionResult Action(int ID = 0)
        {
            OpeningHourActionViewModel model = new OpeningHourActionViewModel();
            var DaysofWeek = new List<string>();
            var LoggedInUser = UserManager.FindById(User.Identity.GetUserId());
            
            var OpeningHoursAll = OpeningHourServices.Instance.GetOpeningHour().Where(x => x.Business == LoggedInUser.Company).Select(x=>x.Day).ToList();
            DaysofWeek.Add("Monday");
            DaysofWeek.Add("Tuesday");
            DaysofWeek.Add("Wednesday");
            DaysofWeek.Add("Thursday");
            DaysofWeek.Add("Friday");
            DaysofWeek.Add("Saturday");
            DaysofWeek.Add("Sunday");
            if (ID != 0)
            {
                model.DaysOfWeek = DaysofWeek;
                var openingHour = OpeningHourServices.Instance.GetOpeningHour(ID);
                model.Day = openingHour.Day;
                model.isClosed = openingHour.isClosed;
                model.Time = openingHour.Time;
                model.ID = openingHour.ID;

            }
            else
            {
                model.DaysOfWeek = DaysofWeek.Where(day => !OpeningHoursAll.Contains(day)).ToList();

            }
            return View(model);
        }


        [HttpPost]
        public ActionResult Action(OpeningHourActionViewModel model)
        {
            var LoggedInUser = UserManager.FindById(User.Identity.GetUserId());

            if (model.ID !=  0)
            {
                var openinghour = OpeningHourServices.Instance.GetOpeningHour(model.ID);
                openinghour.ID = model.ID;
                openinghour.isClosed = model.isClosed;
                openinghour.Day = model.Day;
                openinghour.Time = model.Time;
                OpeningHourServices.Instance.UpdateOpeningHour(openinghour);
            }
            else
            {
                var openinghour = new OpeningHour();
                openinghour.Business = LoggedInUser.Company;
                openinghour.isClosed = model.isClosed;
                openinghour.Day = model.Day;
                openinghour.Time = model.Time;
                OpeningHourServices.Instance.SaveOpeningHour(openinghour);
            }
            return Json(new { success = true }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult Delete(int ID)
        {
            OpeningHourActionViewModel model = new OpeningHourActionViewModel();
            var OpeningHour = OpeningHourServices.Instance.GetOpeningHour(ID);
            model.ID = OpeningHour.ID;
            return PartialView("_Delete", model);
        }


        [HttpPost]
        public ActionResult Delete(OpeningHourActionViewModel model)
        {
            var OpeningHour = OpeningHourServices.Instance.GetOpeningHour(model.ID);

            OpeningHourServices.Instance.DeleteOpeningHour(OpeningHour.ID);

            return Json(new { success = true }, JsonRequestBehavior.AllowGet);
        }
    }
}