using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TheBookingPlatform.Database.Migrations;
using TheBookingPlatform.Entities;
using TheBookingPlatform.Services;
using TheBookingPlatform.ViewModels;

namespace TheBookingPlatform.Controllers
{
    public class PriceChangeController : Controller
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

        public PriceChangeController()
        {
        }

        public PriceChangeController(AMUserManager userManager, AMSignInManager signInManager)
        {
            UserManager = userManager;
            SignInManager = signInManager;
        }

        #endregion
        // GET: PriceChange
        public ActionResult Index(string SearchTerm = "")
        {
            PriceChangeListingViewModel model = new PriceChangeListingViewModel();
            var LoggedInUser = UserManager.FindById(User.Identity.GetUserId());
            var pricechange = new List<PriceChangeModel>();

            if (LoggedInUser.Role != "Super Admin")
            {

                var priceChanges = PriceChangeServices.Instance.GetPriceChangeWRTBusiness(LoggedInUser.Company);
                foreach (var item in priceChanges)
                {
                    var priceChangeSwitch = PriceChangeSwitchServices.Instance.GetPriceChangeSwitchbyPriceChangeID(item.ID);
                    pricechange.Add(new PriceChangeModel { PriceChange = item, PriceChangeSwitch = priceChangeSwitch });
                }
                model.PriceChanges = pricechange;

            }
            else
            {
                var priceChanges = PriceChangeServices.Instance.GetPriceChange();
                foreach (var item in priceChanges)
                {
                    var priceChangeSwitch = PriceChangeSwitchServices.Instance.GetPriceChangeSwitchbyPriceChangeID(item.ID);
                    pricechange.Add(new PriceChangeModel { PriceChange = item, PriceChangeSwitch = priceChangeSwitch });
                }
                model.PriceChanges = pricechange;
            }
            model.SearchTerm = SearchTerm;
            return View(model);
        }

        [HttpGet]
        public ActionResult Action(int ID = 0)
        {
            PriceChangeActionViewModel model = new PriceChangeActionViewModel();
            if (ID != 0)
            {
                var pricechange = PriceChangeServices.Instance.GetPriceChange(ID);
                model.ID = ID;

                if (pricechange.Frequency == "Every Week")
                {
                    model.EveryWeek = pricechange.Every;
                    if (pricechange.Ends.Split('_')[0] == "NumberOfTimes")
                    {
                        model.EndsNumberOfTimesWeek = int.Parse(pricechange.Ends.Split('_')[1]);

                    }
                    else if (pricechange.Ends.Split('_')[0] == "Specific Date")
                    {
                        model.EndsDateWeek = DateTime.Parse(pricechange.Ends.Split('_')[1]);
                    }

                }
                else if (pricechange.Frequency == "Every Day")
                {
                    model.EveryDay = pricechange.Every;
                    if (pricechange.Ends.Split('_')[0] == "NumberOfTimes")
                    {
                        model.EndsNumberOfTimesDay = int.Parse(pricechange.Ends.Split('_')[1]);

                    }
                    else if (pricechange.Ends.Split('_')[0] == "Specific Date")
                    {
                        model.EndsDateDay = DateTime.Parse(pricechange.Ends.Split('_')[1]);
                    }

                }
                else if (pricechange.Frequency == "Every Month")
                {
                    model.EveryMonth = pricechange.Every;
                    if (pricechange.Ends.Split('_')[0] == "NumberOfTimes")
                    {
                        model.EndsNumberOfTimesMonth = int.Parse(pricechange.Ends.Split('_')[1]);

                    }
                    else if (pricechange.Ends.Split('_')[0] == "Specific Date")
                    {
                        model.EndsDateMonth = DateTime.Parse(pricechange.Ends.Split('_')[1]);
                    }
                }
                else
                {
                    model.Every = "";
                }

                model.TypeOfChange = pricechange.TypeOfChange;
                model.Percentage = pricechange.Percentage;
                model.StartDate = pricechange.StartDate;
                model.EndDate = pricechange.EndDate;
                model.Frequency = pricechange.Frequency;
                model.Days = pricechange.Days;
                model.StartTime = DateTime.Parse(pricechange.StartDate.TimeOfDay.ToString());
                model.EndTime = DateTime.Parse(pricechange.EndDate.TimeOfDay.ToString());
                model.Repeat = pricechange.Repeat;

            }
            return View(model);
        }

        [HttpPost]
        public JsonResult SendEmailToClients(int ID)
        {
            var PriceChange = PriceChangeServices.Instance.GetPriceChange(ID);
            if (PriceChange != null &&
                PriceChange.EndDate > DateTime.Now)
            {
                var PriceChangeSwitch = PriceChangeSwitchServices.Instance.GetPriceChangeSwitch().Where(x => x.PriceChangeID == ID).FirstOrDefault();
                if (PriceChangeSwitch != null)
                {
                    PriceChangeSwitch.SwitchStatus = true;
                    PriceChangeSwitchServices.Instance.UpdatePriceChangeSwitch(PriceChangeSwitch);
                }
                else
                {
                    PriceChangeSwitch = new PriceChangeSwitch();
                    PriceChangeSwitch.SwitchStatus = true;
                    PriceChangeSwitch.PriceChangeID = ID;
                    PriceChangeSwitch.Business = PriceChange.Business;
                    PriceChangeSwitchServices.Instance.SavePriceChangeSwitch(PriceChangeSwitch);
                }
                return Json(new { success = true }, JsonRequestBehavior.AllowGet);


            }
            else
            {
                return Json(new { success = false,Message="Price Change haven't started yet, or ended." }, JsonRequestBehavior.AllowGet);

            }
        }


        [HttpPost]
        public JsonResult Action(PriceChangeActionViewModel model)
        {
            var LoggedInUser = UserManager.FindById(User.Identity.GetUserId());

            if (model.ID != 0)
            {

                var pricechange = PriceChangeServices.Instance.GetPriceChange(model.ID);
                pricechange.Days = model.Days;
                pricechange.Business = LoggedInUser.Company;
                pricechange.Frequency = model.Frequency;
                pricechange.Percentage = model.Percentage;
                pricechange.ID = model.ID;
                if (pricechange.Frequency == "Every Week")
                {
                    pricechange.Every = model.EveryWeek;
                    if (model.EndsWeek == "NumberOfTimes")
                    {
                        pricechange.Ends = model.EndsWeek + "_" + model.EndsNumberOfTimesWeek;

                    }
                    else if (model.EndsWeek == "Specific Date")
                    {
                        pricechange.Ends = model.EndsWeek + "_" + model.EndsDateWeek.ToString();
                    }
                    else
                    {
                        pricechange.Ends = model.EndsWeek;
                    }
                }
                else if (pricechange.Frequency == "Every Day")
                {
                    pricechange.Every = model.EveryDay;
                    if (model.EndsDay == "NumberOfTimes")
                    {
                        pricechange.Ends = model.EndsDay + "_" + model.EndsNumberOfTimesDay;

                    }
                    else if (model.EndsDay == "Specific Date")
                    {
                        pricechange.Ends = model.EndsDay + "_" + model.EndsDateDay.ToString();
                    }
                    else
                    {
                        pricechange.Ends = model.EndsDay;
                    }
                }
                else if (pricechange.Frequency == "Every Month")
                {
                    pricechange.Every = model.EveryMonth;
                    if (model.EndsMonth == "NumberOfTimes")
                    {
                        pricechange.Ends = model.EndsMonth + "_" + model.EndsNumberOfTimesMonth;

                    }
                    else if (model.EndsMonth == "Specific Date")
                    {
                        pricechange.Ends = model.EndsMonth + "_" + model.EndsDateMonth.ToString();
                    }
                    else
                    {
                        pricechange.Ends = model.EndsMonth;
                    }
                }
                else
                {
                    pricechange.Every = "";
                    pricechange.Ends = "Never";
                }
                pricechange.Repeat = model.Repeat;
                pricechange.StartDate = model.StartDate.Date + model.StartTime.TimeOfDay;
                pricechange.EndDate = model.EndDate.Date + model.EndTime.TimeOfDay;
                pricechange.TypeOfChange = model.TypeOfChange;

                PriceChangeServices.Instance.UpdatePriceChange(pricechange);
            }
            else
            {
                var pricechange = new PriceChange();
                pricechange.Days = model.Days;
                pricechange.Frequency = model.Frequency;
                pricechange.Percentage = model.Percentage;
                if (pricechange.Frequency == "Every Week")
                {
                    pricechange.Every = model.EveryWeek;
                    if (model.EndsWeek == "NumberOfTimes")
                    {
                        pricechange.Ends = model.EndsWeek + "_" + model.EndsNumberOfTimesWeek;

                    }
                    else if (model.EndsWeek == "Specific Date")
                    {
                        pricechange.Ends = model.EndsWeek + "_" + model.EndsDateWeek.ToString();
                    }
                    else
                    {
                        pricechange.Ends = model.EndsWeek;
                    }
                }
                else if (pricechange.Frequency == "Every Day")
                {
                    pricechange.Every = model.EveryDay;
                    if (model.EndsDay == "NumberOfTimes")
                    {
                        pricechange.Ends = model.EndsDay + "_" + model.EndsNumberOfTimesDay;

                    }
                    else if (model.EndsDay == "Specific Date")
                    {
                        pricechange.Ends = model.EndsDay + "_" + model.EndsDateDay.ToString();
                    }
                    else
                    {
                        pricechange.Ends = model.EndsDay;
                    }
                }
                else if (pricechange.Frequency == "Every Month")
                {
                    pricechange.Every = model.EveryMonth;
                    if (model.EndsMonth == "NumberOfTimes")
                    {
                        pricechange.Ends = model.EndsMonth + "_" + model.EndsNumberOfTimesMonth;

                    }
                    else if (model.EndsMonth == "Specific Date")
                    {
                        pricechange.Ends = model.EndsMonth + "_" + model.EndsDateMonth.ToString();
                    }
                    else
                    {
                        pricechange.Ends = model.EndsMonth;
                    }
                }
                else
                {
                    pricechange.Every = "";
                    pricechange.Ends = "Never";
                }
                pricechange.Repeat = model.Repeat;
                pricechange.StartDate = model.StartDate.Date + model.StartTime.TimeOfDay;
                pricechange.EndDate = model.EndDate.Date + model.EndTime.TimeOfDay;

                pricechange.Business = LoggedInUser.Company;
                pricechange.TypeOfChange = model.TypeOfChange;
                PriceChangeServices.Instance.SavePriceChange(pricechange);
            }
            return Json(new { success = true }, JsonRequestBehavior.AllowGet);
        }


        [HttpGet]
        public ActionResult Delete(int ID)
        {
            PriceChangeActionViewModel model = new PriceChangeActionViewModel();
            var pricechange = PriceChangeServices.Instance.GetPriceChange(ID);
            model.ID = pricechange.ID;
            return PartialView("_Delete", model);
        }

        [HttpPost]
        public ActionResult Delete(PriceChangeActionViewModel model)
        {
            var pricechange = PriceChangeServices.Instance.GetPriceChange(model.ID);

            PriceChangeServices.Instance.DeletePriceChange(pricechange.ID);

            return Json(new { success = true }, JsonRequestBehavior.AllowGet);
        }

    }
}