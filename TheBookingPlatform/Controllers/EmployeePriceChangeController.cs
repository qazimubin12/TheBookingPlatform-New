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
    public class EmployeePriceChangeController : Controller
    {
        // GET: EmployeePriceChange
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
        public EmployeePriceChangeController()
        {
        }



        public EmployeePriceChangeController(AMUserManager userManager, AMSignInManager signInManager)
        {
            UserManager = userManager;
            SignInManager = signInManager;
        }


        // GET: EmployeePriceChange
        public ActionResult Index(int EmployeeID, string SearchTerm = "")
        {
            EmployeePriceChangeListingViewModel model = new EmployeePriceChangeListingViewModel();
            model.SearchTerm = SearchTerm;
            model.Employee = EmployeeServices.Instance.GetEmployee(EmployeeID);
            var loggedInUser = UserManager.FindById(User.Identity.GetUserId());
            if (loggedInUser.Role == "Super Admin")
            {
                model.EmployeePriceChanges = EmployeePriceChangeServices.Instance.GetEmployeePriceChange(EmployeeID, SearchTerm);
            }
            else
            {
                model.EmployeePriceChanges = EmployeePriceChangeServices.Instance.GetEmployeePriceChangeWRTBusiness(EmployeeID, loggedInUser.Company, SearchTerm);

            }
            return View(model);
        }


        [HttpGet]
        public ActionResult Action(int EmployeeID, int ID = 0)
        {
            EmployeePriceChangeActionViewModel model = new EmployeePriceChangeActionViewModel();
            var loggedInUser = UserManager.FindById(User.Identity.GetUserId());
            if (loggedInUser.Role == "Super Admin")
            {
                model.Employees = EmployeeServices.Instance.GetEmployee();
            }
            else
            {
                model.Employees = EmployeeServices.Instance.GetEmployeeWRTBusiness(loggedInUser.Company, true);

            }
            if (ID != 0)
            {
                var EmployeePriceChange = EmployeePriceChangeServices.Instance.GetEmployeePriceChange(ID);
                model.ID = ID;
                model.EmployeeID = EmployeePriceChange.EmployeeID;
                if (EmployeePriceChange.Frequency == "Every Week")
                {
                    model.EveryWeek = EmployeePriceChange.Every;
                    if (EmployeePriceChange.Ends.Split('_')[0] == "NumberOfTimes")
                    {
                        model.EndsNumberOfTimesWeek = int.Parse(EmployeePriceChange.Ends.Split('_')[1]);

                    }
                    else if (EmployeePriceChange.Ends.Split('_')[0] == "Specific Date")
                    {
                        model.EndsDateWeek = DateTime.Parse(EmployeePriceChange.Ends.Split('_')[1]);
                    }

                }
                else if (EmployeePriceChange.Frequency == "Every Day")
                {
                    model.EveryDay = EmployeePriceChange.Every;
                    if (EmployeePriceChange.Ends.Split('_')[0] == "NumberOfTimes")
                    {
                        model.EndsNumberOfTimesDay = int.Parse(EmployeePriceChange.Ends.Split('_')[1]);

                    }
                    else if (EmployeePriceChange.Ends.Split('_')[0] == "Specific Date")
                    {
                        model.EndsDateDay = DateTime.Parse(EmployeePriceChange.Ends.Split('_')[1]);
                    }

                }
                else if (EmployeePriceChange.Frequency == "Every Month")
                {
                    model.EveryMonth = EmployeePriceChange.Every;
                    if (EmployeePriceChange.Ends.Split('_')[0] == "NumberOfTimes")
                    {
                        model.EndsNumberOfTimesMonth = int.Parse(EmployeePriceChange.Ends.Split('_')[1]);

                    }
                    else if (EmployeePriceChange.Ends.Split('_')[0] == "Specific Date")
                    {
                        model.EndsDateMonth = DateTime.Parse(EmployeePriceChange.Ends.Split('_')[1]);
                    }
                }
                else
                {
                    model.Every = "";
                }


                model.TypeOfChange = EmployeePriceChange.TypeOfChange;
                model.Percentage = EmployeePriceChange.Percentage;
                model.StartDate = EmployeePriceChange.StartDate;
                model.EndDate = EmployeePriceChange.EndDate;
                model.Frequency = EmployeePriceChange.Frequency;
                model.Days = EmployeePriceChange.Days;
                model.StartTime = DateTime.Parse(EmployeePriceChange.StartDate.TimeOfDay.ToString());
                model.EndTime = DateTime.Parse(EmployeePriceChange.EndDate.TimeOfDay.ToString());
                model.Repeat = EmployeePriceChange.Repeat;

            }
            else
            {
                model.EmployeeID = EmployeeID;
            }
            return View(model);
        }


        [HttpPost]
        public JsonResult Action(EmployeePriceChangeActionViewModel model)
        {
            var LoggedInUser = UserManager.FindById(User.Identity.GetUserId());

            if (model.ID != 0)
            {

                var EmployeePriceChange = EmployeePriceChangeServices.Instance.GetEmployeePriceChange(model.ID);
                EmployeePriceChange.Days = model.Days;
                EmployeePriceChange.Business = LoggedInUser.Company;
                EmployeePriceChange.Frequency = model.Frequency;
                EmployeePriceChange.Percentage = model.Percentage;
                EmployeePriceChange.ID = model.ID;
                if (EmployeePriceChange.Frequency == "Every Week")
                {
                    EmployeePriceChange.Every = model.EveryWeek;
                    if (model.EndsWeek == "NumberOfTimes")
                    {
                        EmployeePriceChange.Ends = model.EndsWeek + "_" + model.EndsNumberOfTimesWeek;

                    }
                    else if (model.EndsWeek == "Specific Date")
                    {
                        EmployeePriceChange.Ends = model.EndsWeek + "_" + model.EndsDateWeek.ToString();
                    }
                    else
                    {
                        EmployeePriceChange.Ends = model.EndsWeek;
                    }
                }
                else if (EmployeePriceChange.Frequency == "Every Day")
                {
                    EmployeePriceChange.Every = model.EveryDay;
                    if (model.EndsDay == "NumberOfTimes")
                    {
                        EmployeePriceChange.Ends = model.EndsDay + "_" + model.EndsNumberOfTimesDay;

                    }
                    else if (model.EndsDay == "Specific Date")
                    {
                        EmployeePriceChange.Ends = model.EndsDay + "_" + model.EndsDateDay.ToString();
                    }
                    else
                    {
                        EmployeePriceChange.Ends = model.EndsDay;
                    }
                }
                else if (EmployeePriceChange.Frequency == "Every Month")
                {
                    EmployeePriceChange.Every = model.EveryMonth;
                    if (model.EndsMonth == "NumberOfTimes")
                    {
                        EmployeePriceChange.Ends = model.EndsMonth + "_" + model.EndsNumberOfTimesMonth;

                    }
                    else if (model.EndsMonth == "Specific Date")
                    {
                        EmployeePriceChange.Ends = model.EndsMonth + "_" + model.EndsDateMonth.ToString();
                    }
                    else
                    {
                        EmployeePriceChange.Ends = model.EndsMonth;
                    }
                }
                else
                {
                    EmployeePriceChange.Every = "";
                    EmployeePriceChange.Ends = "Never";
                }
                EmployeePriceChange.Repeat = model.Repeat;
                EmployeePriceChange.StartDate = model.StartDate.Date + model.StartTime.TimeOfDay;
                EmployeePriceChange.EndDate = model.EndDate.Date + model.EndTime.TimeOfDay;
                EmployeePriceChange.TypeOfChange = model.TypeOfChange;
                EmployeePriceChange.EmployeeID = model.EmployeeID;
                EmployeePriceChangeServices.Instance.UpdateEmployeePriceChange(EmployeePriceChange);
            }
            else
            {
                var EmployeePriceChange = new TheBookingPlatform.Entities.EmployeePriceChange();
                EmployeePriceChange.Days = model.Days;
                EmployeePriceChange.Frequency = model.Frequency;
                EmployeePriceChange.Percentage = model.Percentage;
                if (EmployeePriceChange.Frequency == "Every Week")
                {
                    EmployeePriceChange.Every = model.EveryWeek;
                    if (model.EndsWeek == "NumberOfTimes")
                    {
                        EmployeePriceChange.Ends = model.EndsWeek + "_" + model.EndsNumberOfTimesWeek;

                    }
                    else if (model.EndsWeek == "Specific Date")
                    {
                        EmployeePriceChange.Ends = model.EndsWeek + "_" + model.EndsDateWeek.ToString();
                    }
                    else
                    {
                        EmployeePriceChange.Ends = model.EndsWeek;
                    }
                }
                else if (EmployeePriceChange.Frequency == "Every Day")
                {
                    EmployeePriceChange.Every = model.EveryDay;
                    if (model.EndsDay == "NumberOfTimes")
                    {
                        EmployeePriceChange.Ends = model.EndsDay + "_" + model.EndsNumberOfTimesDay;

                    }
                    else if (model.EndsDay == "Specific Date")
                    {
                        EmployeePriceChange.Ends = model.EndsDay + "_" + model.EndsDateDay.ToString();
                    }
                    else
                    {
                        EmployeePriceChange.Ends = model.EndsDay;
                    }
                }
                else if (EmployeePriceChange.Frequency == "Every Month")
                {
                    EmployeePriceChange.Every = model.EveryMonth;
                    if (model.EndsMonth == "NumberOfTimes")
                    {
                        EmployeePriceChange.Ends = model.EndsMonth + "_" + model.EndsNumberOfTimesMonth;

                    }
                    else if (model.EndsMonth == "Specific Date")
                    {
                        EmployeePriceChange.Ends = model.EndsMonth + "_" + model.EndsDateMonth.ToString();
                    }
                    else
                    {
                        EmployeePriceChange.Ends = model.EndsMonth;
                    }
                }
                else
                {
                    EmployeePriceChange.Every = "";
                    EmployeePriceChange.Ends = "Never";
                }
                EmployeePriceChange.Repeat = model.Repeat;
                EmployeePriceChange.StartDate = model.StartDate.Date + model.StartTime.TimeOfDay;
                EmployeePriceChange.EndDate = model.EndDate.Date + model.EndTime.TimeOfDay;
                EmployeePriceChange.EmployeeID = model.EmployeeID;
                EmployeePriceChange.Business = LoggedInUser.Company;
                EmployeePriceChange.TypeOfChange = model.TypeOfChange;
                EmployeePriceChangeServices.Instance.SaveEmployeePriceChange(EmployeePriceChange);
            }
            return Json(new { success = true }, JsonRequestBehavior.AllowGet);
        }


        [HttpGet]
        public ActionResult Delete(int ID)
        {
            EmployeePriceChangeActionViewModel model = new EmployeePriceChangeActionViewModel();
            var EmployeePriceChange = EmployeePriceChangeServices.Instance.GetEmployeePriceChange(ID);
            model.ID = EmployeePriceChange.ID;
            return PartialView("_Delete", model);
        }

        [HttpPost]
        public ActionResult Delete(EmployeePriceChangeActionViewModel model)
        {
            var EmployeePriceChange = EmployeePriceChangeServices.Instance.GetEmployeePriceChange(model.ID);

            EmployeePriceChangeServices.Instance.DeleteEmployeePriceChange(EmployeePriceChange.ID);

            return Json(new { success = true }, JsonRequestBehavior.AllowGet);
        }
    }
}