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
    public class CalendarManageController : Controller
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
        public CalendarManageController()
        {
        }
        public CalendarManageController(AMUserManager userManager, AMSignInManager signInManager)
        {
            UserManager = userManager;
            SignInManager = signInManager;
        }


        // GET: CalendarManage
        public ActionResult Index()
        {
            CalendarManageListingViewModel model = new CalendarManageListingViewModel();
            var loggedInUser = UserManager.FindById(User.Identity.GetUserId());
            var listofCalendarManageModel = new List<CalendarManageModel>();
            var listOfCalendarManges = CalendarManageServices.Instance.GetCalendarManage().Where(x=>x.Business == loggedInUser.Company).ToList();
            
            
            foreach (var item in listOfCalendarManges)
            {
                var user = UserManager.FindById(item.UserID);
                var ManagesOfList = new List<Employee>();
                if (item.ManageOf != null && item.ManageOf != "")
                {
                    foreach (var empID in item.ManageOf.Split(',').ToList())
                    {
                        var employee = EmployeeServices.Instance.GetEmployee(int.Parse(empID));
                        ManagesOfList.Add(EmployeeServices.Instance.GetEmployee(int.Parse(empID)));

                    }
                }
                listofCalendarManageModel.Add(new CalendarManageModel { User = user, ManageOfs = ManagesOfList });
            }

            if (listOfCalendarManges.Count() != 0)
            {
                var exceptUsers = UserManager.Users.ToList()
                   .Where(user =>
                       user.Role != "Super Admin"  // Filter out Super Admin users
                       && user.Company == loggedInUser.Company
                       && !listofCalendarManageModel.Where(x=>x.User != null).Select(x=>x.User.Id).Contains(user.Id)
                       ).ToList();
                foreach (var item in exceptUsers)
                {
                    listofCalendarManageModel.Add(new CalendarManageModel { User = item, ManageOfs = null });
                }
            }
            else
            {
                var exceptUsers = UserManager.Users.ToList()
                  .Where(user =>
                      user.Role != "Super Admin"  // Filter out Super Admin users
                      && user.Company == loggedInUser.Company).ToList();
                foreach (var item in exceptUsers)
                {
                    listofCalendarManageModel.Add(new CalendarManageModel { User = item, ManageOfs = null });
                }
            }

            model.CalendarManageModels = listofCalendarManageModel;
            return View(model);
        }


        [HttpGet]
        public ActionResult AssignCalendars(string ID)
        {
            CalendarManageActionViewModel model = new CalendarManageActionViewModel();
            model.User = UserManager.FindById(ID);
            var company = CompanyServices.Instance.GetCompany().Where(x => x.Business == model.User.Company).FirstOrDefault();
            var calendarManage = CalendarManageServices.Instance.GetCalendarManage().Where(x => x.Business == model.User.Company && x.UserID == model.User.Id).FirstOrDefault();
            if (calendarManage != null && calendarManage.ManageOf != null)
            {

                var listofCalendarManageModel = calendarManage.ManageOf.Split(',').ToList();
                var AssignedEmployeeList = new List<Employee>();
                foreach (var item in listofCalendarManageModel)
                {
                    var employee = EmployeeServices.Instance.GetEmployee(int.Parse(item));
                    if (employee != null)
                    {
                        AssignedEmployeeList.Add(employee);
                    }
                }
                var exceptUsers = EmployeeServices.Instance.GetEmployee()
                                .Where(emp =>
                                   emp.Business == model.User.Company  // Filter users with Business equal to "SomeBusiness"
                                    && !listofCalendarManageModel.Any(s => s.Contains(emp.ID.ToString()))  // Users not in listofCalendarManageModel
                                )
                                .ToList();
                var employeerequests = EmployeeRequestServices.Instance.GetEmployeeRequestByBusiness(company.ID);
                foreach (var item in employeerequests)
                {
                    if (item.Accepted)
                    {

                        var employee = EmployeeServices.Instance.GetEmployee(item.EmployeeID);
                        if (!exceptUsers.Select(x => x.ID).Contains(employee.ID) && !AssignedEmployeeList.Select(x => x.ID).Contains(item.EmployeeID))
                        {
                            exceptUsers.Add(employee);
                        }
                    }
                }
                model.ExceptUsers = exceptUsers;
                model.ManageOf = calendarManage.ManageOf;
                model.AssignedUsers = AssignedEmployeeList;

            }
            else
            {
                var exceptUsers = EmployeeServices.Instance.GetEmployee()
                              .Where(emp =>
                                   emp.Business == model.User.Company)  // Filter users with Business equal to "SomeBusiness"

                              .ToList();
                var employeerequests = EmployeeRequestServices.Instance.GetEmployeeRequestByBusiness(company.ID);
                foreach (var item in employeerequests)
                {
                    if (item.Accepted)
                    {
                        var timetables = TimeTableServices.Instance.GetTimeTableByEmployeeID(item.EmployeeID);
                        var employee = EmployeeServices.Instance.GetEmployee(item.EmployeeID);
                        if (!exceptUsers.Select(x => x.ID).Contains(employee.ID))
                        {
                            exceptUsers.Add(employee);
                        }
                    }
                }
                model.ExceptUsers = exceptUsers;
            }
            return PartialView("_AssignCalendars", model);
        }

        [HttpPost]
        public ActionResult AssignCalendar(CalendarManageActionViewModel model)
        {
            var loggedInUser = UserManager.FindById(User.Identity.GetUserId());
            var userId = model.UserID;
            var calendarManage = CalendarManageServices.Instance.GetCalendarManage().Where(x=>x.UserID == userId && x.Business == loggedInUser.Company).FirstOrDefault();
            if(calendarManage != null) 
            {
                calendarManage.ManageOf = model.ManageOf;
                CalendarManageServices.Instance.UpdateCalendarManage(calendarManage);
                return Json(new { success = true },JsonRequestBehavior.AllowGet);
            }
            else
            {
                var calendarManagenew = new CalendarManage();
                calendarManagenew.Business = loggedInUser.Company;
                calendarManagenew.ManageOf = model.ManageOf;
                calendarManagenew.UserID = model.UserID;
                CalendarManageServices.Instance.SaveCalendarManage(calendarManagenew);
                return Json(new { success = true }, JsonRequestBehavior.AllowGet);
            }
            
        }
    }
}