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
    public class ShiftController : Controller
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

        public ShiftController()
        {
        }

        public ShiftController(AMUserManager userManager, AMSignInManager signInManager)
        {
            UserManager = userManager;
            SignInManager = signInManager;
        }

        #endregion
        // GET: Shift

        static bool IsDateInRange(DateTime dateToCheck, DateTime startDate, DateTime endDate)
        {
            // Extracting only the date part without the time part
            DateTime dateOnlyToCheck = dateToCheck.Date;
            DateTime startDateOnly = startDate.Date;
            DateTime endDateOnly = endDate.Date;

            if (dateOnlyToCheck > endDateOnly)
            {
                return true; // If it's greater, it's definitely not in range
            }

            // Checking if the date falls within the range
            return dateOnlyToCheck >= startDateOnly && dateOnlyToCheck <= endDateOnly;

            // Checking if the date falls within the range
        }

        public static DateTime GetNextDayOfWeek(DateTime currentWeekStart, DayOfWeek targetDayOfWeek)
        {
            int daysToAdd = ((int)targetDayOfWeek - (int)currentWeekStart.DayOfWeek + 7) % 7;
            return currentWeekStart.AddDays(daysToAdd);
        }
        public string GetNextDayStatus(DateTime CurrentWeekStart, DateTime rosterStartDate, string targetDayString)
        {
            // Calculate the difference in days between the roster start date and today
            if (!Enum.TryParse(targetDayString, true, out DayOfWeek targetDay))
            {
                return "Invalid day of the week specified.";
            }
            var FinalToBeCheckedDate = GetNextDayOfWeek(CurrentWeekStart, targetDay);


            TimeSpan difference = FinalToBeCheckedDate - rosterStartDate.Date;

            // Calculate the number of weeks passed since the roster start date
            int weeksPassed = (int)(difference.TotalDays / 7);

            // Determine if the current week is odd or even
            bool isEvenWeek = weeksPassed % 2 == 0;
            bool isNextTargetDayInCurrentWeek = false;
            if (isEvenWeek && difference.TotalDays >= 0)
            {
                isNextTargetDayInCurrentWeek = true;
            }




            return isNextTargetDayInCurrentWeek ? "YES" : "NO";
        }
       
        public ActionResult Index(string StartOfWeek="",string EndOfWeek = "")
        {
            ShiftListingViewModel model = new ShiftListingViewModel();
            var Allshifts = new List<ShiftOfEmployeeModel>();
            var loggedInUser = UserManager.FindById(User.Identity.GetUserId());
            var company = CompanyServices.Instance.GetCompany().Where(x => x.Business == loggedInUser.Company).FirstOrDefault();
            var employees = EmployeeServices.Instance.GetEmployeeWRTBusiness(loggedInUser.Company, true);
         

            var employeerequests = EmployeeRequestServices.Instance.GetEmployeeRequestByBusiness(company.ID);
            foreach (var item in employeerequests)
            {
                if (item.Accepted)
                {
                    var employee = EmployeeServices.Instance.GetEmployee(item.EmployeeID);
                    if (!employees.Select(x => x.ID).Contains(employee.ID))
                    {
                        employees.Add(employee);
                    }
                }
            }


            if (StartOfWeek != "" && EndOfWeek != "")
            {
                DateTime startOfWeek = DateTime.Parse(StartOfWeek);
                DateTime endOfWeek = DateTime.Parse(EndOfWeek);

                List<DateTime> weekDates = new List<DateTime>();
                for (DateTime date = startOfWeek; date <= endOfWeek; date = date.AddDays(1))
                {
                    weekDates.Add(date);
                }
                model.weekDates = weekDates;
                foreach (var emp in employees)
                {
                    var shifts = new List<ShiftModel>();
                    var shiftslist = ShiftServices.Instance.GetShiftWRTBusiness(loggedInUser.Company, emp.ID);
                    foreach (var item in shiftslist)
                    {
                        var roster = TimeTableRosterServices.Instance.GetTimeTableRosterByEmpID(item.EmployeeID);
                        var recurringShifts = RecurringShiftServices.Instance.GetRecurringShiftWRTBusiness(loggedInUser.Company, item.ID);
                        if (recurringShifts != null)
                        {
                            if (recurringShifts.RecurEnd == "Custom Date" && recurringShifts.RecurEndDate != "")
                            {
                                var RecurrEndDate = DateTime.Parse(recurringShifts.RecurEndDate);
                                if (IsDateInRange(RecurrEndDate, startOfWeek, endOfWeek))
                                {
                                    if (recurringShifts.Frequency == "Bi-Weekly")
                                    {
                                        if (roster != null)
                                        {
                                            if (GetNextDayStatus(startOfWeek, item.Date, item.Day.ToString()) == "YES")
                                            {
                                                var exceptionShift = ExceptionShiftServices.Instance.GetExceptionShiftWRTBusiness(loggedInUser.Company, item.ID);
                                                shifts.Add(new ShiftModel { Shift = item, RecurShift = recurringShifts, ExceptionShift = exceptionShift });
                                            }
                                        }

                                    }
                                    else
                                    {
                                        var exceptionShift = ExceptionShiftServices.Instance.GetExceptionShiftWRTBusiness(loggedInUser.Company, item.ID);
                                        shifts.Add(new ShiftModel { Shift = item, RecurShift = recurringShifts, ExceptionShift = exceptionShift });
                                    }
                                }
                            }
                            else
                            {
                                if (recurringShifts.Frequency == "Bi-Weekly")
                                {
                                    if (roster != null)
                                    {
                                        if (GetNextDayStatus(startOfWeek, item.Date, item.Day.ToString()) == "YES")
                                        {
                                            var exceptionShift = ExceptionShiftServices.Instance.GetExceptionShiftWRTBusiness(loggedInUser.Company, item.ID);
                                            shifts.Add(new ShiftModel { Shift = item, RecurShift = recurringShifts, ExceptionShift = exceptionShift });
                                        }
                                    }

                                }
                                else
                                {
                                    var exceptionShift = ExceptionShiftServices.Instance.GetExceptionShiftWRTBusiness(loggedInUser.Company, item.ID);
                                    shifts.Add(new ShiftModel { Shift = item, RecurShift = recurringShifts, ExceptionShift = exceptionShift });

                                }
                            }
                        }
                        else
                        {
                            var exceptionShiftByShiftID = ExceptionShiftServices.Instance.GetExceptionShiftWRTBusinessAndShift(loggedInUser.Company, item.ID);
                            if (exceptionShiftByShiftID != null)
                            {
                                shifts.Add(new ShiftModel { Shift = item, RecurShift = recurringShifts, ExceptionShift = exceptionShiftByShiftID });
                            }
                            else
                            {
                                shifts.Add(new ShiftModel { Shift = item, RecurShift = recurringShifts });

                            }

                        }
                    }
                    Allshifts.Add(new ShiftOfEmployeeModel { Employee = emp, Shifts = shifts });
                }
            }
            else
            {
                int diff = (int)DateTime.Now.DayOfWeek - 1;
                if (diff < 0)
                {
                    diff += 7; // Handle the case where it's Sunday
                }

                // Calculate the start of the week (Monday)
                DateTime startOfWeek = DateTime.Now.AddDays(-diff);

                // Calculate the end of the week (Sunday)
                DateTime endOfWeek = startOfWeek.AddDays(6);

                List<DateTime> weekDates = new List<DateTime>();
                for (DateTime date = startOfWeek; date <= endOfWeek; date = date.AddDays(1))
                {
                    weekDates.Add(date);
                }
                model.weekDates = weekDates;
                foreach (var emp in employees)
                {
                    var shifts = new List<ShiftModel>();
                    var shiftslist = ShiftServices.Instance.GetShiftWRTBusiness(loggedInUser.Company, emp.ID);
                    foreach (var item in shiftslist)
                    {
                        var roster = TimeTableRosterServices.Instance.GetTimeTableRosterByEmpID(item.EmployeeID);
                        var recurringShifts = RecurringShiftServices.Instance.GetRecurringShiftWRTBusiness(loggedInUser.Company, item.ID);
                        if (recurringShifts != null)
                        {
                            if (recurringShifts.RecurEnd == "Custom Date" && recurringShifts.RecurEndDate != "")
                            {
                                var RecurrEndDate = DateTime.Parse(recurringShifts.RecurEndDate);
                                if (IsDateInRange(RecurrEndDate, startOfWeek, endOfWeek))
                                {
                                    if (recurringShifts.Frequency == "Bi-Weekly")
                                    {
                                        if (roster != null)
                                        {
                                            if (GetNextDayStatus(startOfWeek, item.Date, item.Day.ToString()) == "YES")
                                            {
                                                var exceptionShift = ExceptionShiftServices.Instance.GetExceptionShiftWRTBusiness(loggedInUser.Company, item.ID);
                                                shifts.Add(new ShiftModel { Shift = item, RecurShift = recurringShifts, ExceptionShift = exceptionShift });
                                            }
                                        }

                                    }
                                    else
                                    {
                                        var exceptionShift = ExceptionShiftServices.Instance.GetExceptionShiftWRTBusiness(loggedInUser.Company, item.ID);
                                        shifts.Add(new ShiftModel { Shift = item, RecurShift = recurringShifts, ExceptionShift = exceptionShift });
                                    }
                                }
                            }
                            else
                            {
                                if (recurringShifts.Frequency == "Bi-Weekly")
                                {
                                    if (roster != null)
                                    {
                                        if (GetNextDayStatus(startOfWeek, item.Date, item.Day.ToString()) == "YES")
                                        {
                                            var exceptionShift = ExceptionShiftServices.Instance.GetExceptionShiftWRTBusiness(loggedInUser.Company, item.ID);
                                            shifts.Add(new ShiftModel { Shift = item, RecurShift = recurringShifts, ExceptionShift = exceptionShift });
                                        }
                                    }

                                }
                                else
                                {
                                    var exceptionShift = ExceptionShiftServices.Instance.GetExceptionShiftWRTBusiness(loggedInUser.Company, item.ID);
                                    shifts.Add(new ShiftModel { Shift = item, RecurShift = recurringShifts, ExceptionShift = exceptionShift });

                                }
                            }
                        }
                        else
                        {
                            var exceptionShiftByShiftID = ExceptionShiftServices.Instance.GetExceptionShiftWRTBusinessAndShift(loggedInUser.Company, item.ID);
                            if (exceptionShiftByShiftID != null)
                            {
                                shifts.Add(new ShiftModel { Shift = item, RecurShift = recurringShifts, ExceptionShift = exceptionShiftByShiftID });
                            }
                            else
                            {
                                shifts.Add(new ShiftModel { Shift = item, RecurShift = recurringShifts });

                            }

                        }
                    }
                    Allshifts.Add(new ShiftOfEmployeeModel { Employee = emp, Shifts = shifts });
                }
            }
            model.Shifts = Allshifts;
            model.Employee = EmployeeServices.Instance.GetEmployeeWRTBusiness(loggedInUser.Company, true);

            return View(model);
        }


        [HttpGet]
        public ActionResult GetNextWeek(DateTime StartDate, DateTime EndDate)
        {

            ShiftListingViewModel model = new ShiftListingViewModel();
            var Allshifts = new List<ShiftOfEmployeeModel>();
            var loggedInUser = UserManager.FindById(User.Identity.GetUserId());
            var company = CompanyServices.Instance.GetCompany().Where(x => x.Business == loggedInUser.Company).FirstOrDefault();
            var employees = EmployeeServices.Instance.GetEmployeeWRTBusiness(loggedInUser.Company, true);
            var StartDateFinal = StartDate.AddDays(7);
            var EndDatefinal = EndDate.AddDays(7);
            DateTime startOfWeek = StartDateFinal;
            DateTime endOfWeek = EndDatefinal;

            List<DateTime> weekDates = new List<DateTime>();
            for (DateTime date = startOfWeek; date <= endOfWeek; date = date.AddDays(1))
            {
                weekDates.Add(date);
            }
            model.weekDates = weekDates;

            var employeerequests = EmployeeRequestServices.Instance.GetEmployeeRequestByBusiness(company.ID);
            foreach (var item in employeerequests)
            {
                if (item.Accepted)
                {
                    var employee = EmployeeServices.Instance.GetEmployee(item.EmployeeID);
                    if (!employees.Select(x => x.ID).Contains(employee.ID))
                    {
                        employees.Add(employee);
                    }
                }
            }
            foreach (var emp in employees)
            {
                var shifts = new List<ShiftModel>();
                var shiftslist = ShiftServices.Instance.GetShiftWRTBusiness(loggedInUser.Company, emp.ID);
                foreach (var item in shiftslist)
                {
                    var roster = TimeTableRosterServices.Instance.GetTimeTableRosterByEmpID(item.EmployeeID);
                    var recurringShifts = RecurringShiftServices.Instance.GetRecurringShiftWRTBusiness(loggedInUser.Company, item.ID);
                    if (recurringShifts != null)
                    {
                        if (recurringShifts.RecurEnd == "Custom Date" && recurringShifts.RecurEndDate != "")
                        {
                            var RecurrEndDate = DateTime.Parse(recurringShifts.RecurEndDate);
                            if (IsDateInRange(RecurrEndDate, startOfWeek, endOfWeek))
                            {
                                if (recurringShifts.Frequency == "Bi-Weekly")
                                {
                                    if (roster != null)
                                    {
                                        if (GetNextDayStatus(startOfWeek, item.Date, item.Day.ToString()) == "YES")
                                        {
                                            var exceptionShift = ExceptionShiftServices.Instance.GetExceptionShiftWRTBusiness(loggedInUser.Company, item.ID);
                                            shifts.Add(new ShiftModel { Shift = item, RecurShift = recurringShifts, ExceptionShift = exceptionShift });
                                        }
                                    }

                                }
                                else
                                {
                                    var exceptionShift = ExceptionShiftServices.Instance.GetExceptionShiftWRTBusiness(loggedInUser.Company, item.ID);
                                    shifts.Add(new ShiftModel { Shift = item, RecurShift = recurringShifts, ExceptionShift = exceptionShift });
                                }
                            }
                        }
                        else
                        {
                            if (recurringShifts.Frequency == "Bi-Weekly")
                            {
                                if (roster != null)
                                {
                                    if (GetNextDayStatus(startOfWeek, item.Date, item.Day.ToString()) == "YES")
                                    {
                                        var exceptionShift = ExceptionShiftServices.Instance.GetExceptionShiftWRTBusiness(loggedInUser.Company, item.ID);
                                        shifts.Add(new ShiftModel { Shift = item, RecurShift = recurringShifts, ExceptionShift = exceptionShift });
                                    }
                                }

                            }
                            else
                            {
                                var exceptionShift = ExceptionShiftServices.Instance.GetExceptionShiftWRTBusiness(loggedInUser.Company, item.ID);
                                shifts.Add(new ShiftModel { Shift = item, RecurShift = recurringShifts, ExceptionShift = exceptionShift });
                            }
                        }
                    }
                    else
                    {
                        var exceptionShiftByShiftID = ExceptionShiftServices.Instance.GetExceptionShiftWRTBusinessAndShift(loggedInUser.Company, item.ID);
                        if (exceptionShiftByShiftID != null)
                        {
                            shifts.Add(new ShiftModel { Shift = item, RecurShift = recurringShifts, ExceptionShift = exceptionShiftByShiftID });
                        }
                        else
                        {
                            shifts.Add(new ShiftModel { Shift = item, RecurShift = recurringShifts });

                        }

                    }
                }
                Allshifts.Add(new ShiftOfEmployeeModel { Employee = emp, Shifts = shifts });
            }
            model.Shifts = Allshifts;
            model.Employee = EmployeeServices.Instance.GetEmployeeWRTBusiness(loggedInUser.Company, true);
            return PartialView("Index", model);
        }


        [HttpGet]
        public ActionResult GetPreviousWeek(DateTime StartDate, DateTime EndDate)
        {
            ShiftListingViewModel model = new ShiftListingViewModel();
            var Allshifts = new List<ShiftOfEmployeeModel>();
            var loggedInUser = UserManager.FindById(User.Identity.GetUserId());
            var company = CompanyServices.Instance.GetCompany().Where(x => x.Business == loggedInUser.Company).FirstOrDefault();
            var employees = EmployeeServices.Instance.GetEmployeeWRTBusiness(loggedInUser.Company, true);
            var StartDateFinal = StartDate.AddDays(-7);
            var EndDatefinal = EndDate.AddDays(-7);
            DateTime startOfWeek = StartDateFinal;
            DateTime endOfWeek = EndDatefinal;

            List<DateTime> weekDates = new List<DateTime>();
            for (DateTime date = startOfWeek; date <= endOfWeek; date = date.AddDays(1))
            {
                weekDates.Add(date);
            }
            model.weekDates = weekDates;
            var employeerequests = EmployeeRequestServices.Instance.GetEmployeeRequestByBusiness(company.ID);
            foreach (var item in employeerequests)
            {
                if (item.Accepted)
                {
                    var employee = EmployeeServices.Instance.GetEmployee(item.EmployeeID);
                    if (!employees.Select(x => x.ID).Contains(employee.ID))
                    {
                        employees.Add(employee);
                    }
                }
            }
            foreach (var emp in employees)
            {
                var shifts = new List<ShiftModel>();
                var shiftslist = ShiftServices.Instance.GetShiftWRTBusiness(loggedInUser.Company, emp.ID);
                foreach (var item in shiftslist)
                {
                    var roster = TimeTableRosterServices.Instance.GetTimeTableRosterByEmpID(item.EmployeeID);
                    var recurringShifts = RecurringShiftServices.Instance.GetRecurringShiftWRTBusiness(loggedInUser.Company, item.ID);
                    if (recurringShifts != null)
                    {
                        if (recurringShifts.RecurEnd == "Custom Date" && recurringShifts.RecurEndDate != "")
                        {
                            var RecurrEndDate = DateTime.Parse(recurringShifts.RecurEndDate);
                            if (IsDateInRange(RecurrEndDate, startOfWeek, endOfWeek))
                            {
                                if (recurringShifts.Frequency == "Bi-Weekly")
                                {
                                    if (roster != null)
                                    {
                                        if (GetNextDayStatus(startOfWeek, item.Date, item.Day.ToString()) == "YES")
                                        {
                                            var exceptionShift = ExceptionShiftServices.Instance.GetExceptionShiftWRTBusiness(loggedInUser.Company, item.ID);
                                            shifts.Add(new ShiftModel { Shift = item, RecurShift = recurringShifts, ExceptionShift = exceptionShift });
                                        }
                                    }

                                }
                                else
                                {
                                    var exceptionShift = ExceptionShiftServices.Instance.GetExceptionShiftWRTBusiness(loggedInUser.Company, item.ID);
                                    shifts.Add(new ShiftModel { Shift = item, RecurShift = recurringShifts, ExceptionShift = exceptionShift });
                                }
                            }
                        }
                        else
                        {
                            if (recurringShifts.Frequency == "Bi-Weekly")
                            {
                                if (roster != null)
                                {
                                    if (GetNextDayStatus(startOfWeek, item.Date, item.Day.ToString()) == "YES")
                                    {
                                        var exceptionShift = ExceptionShiftServices.Instance.GetExceptionShiftWRTBusiness(loggedInUser.Company, item.ID);
                                        shifts.Add(new ShiftModel { Shift = item, RecurShift = recurringShifts, ExceptionShift = exceptionShift });
                                    }
                                }

                            }
                            else
                            {
                                var exceptionShift = ExceptionShiftServices.Instance.GetExceptionShiftWRTBusiness(loggedInUser.Company, item.ID);
                                shifts.Add(new ShiftModel { Shift = item, RecurShift = recurringShifts, ExceptionShift = exceptionShift });
                            }
                        }
                    }
                    else
                    {
                        var exceptionShiftByShiftID = ExceptionShiftServices.Instance.GetExceptionShiftWRTBusinessAndShift(loggedInUser.Company, item.ID);
                        if (exceptionShiftByShiftID != null)
                        {
                            shifts.Add(new ShiftModel { Shift = item, RecurShift = recurringShifts, ExceptionShift = exceptionShiftByShiftID });
                        }
                        else
                        {
                            shifts.Add(new ShiftModel { Shift = item, RecurShift = recurringShifts });

                        }

                    }
                }
                Allshifts.Add(new ShiftOfEmployeeModel { Employee = emp, Shifts = shifts });
            }
            model.Shifts = Allshifts;
            model.Employee = EmployeeServices.Instance.GetEmployeeWRTBusiness(loggedInUser.Company, true);
            return PartialView("Index", model);
        }

        [HttpGet]
        public ActionResult Action(int EmployeeID, string Date, int ID = 0, bool IsException = false)
        {
            ShiftActionViewModel model = new ShiftActionViewModel();
            var loggedInUser = UserManager.FindById(User.Identity.GetUserId());
            model.Employees = EmployeeServices.Instance.GetEmployeeWRTBusiness(loggedInUser.Company, true);
            model.Employee = EmployeeServices.Instance.GetEmployee(EmployeeID);
            model.Date = DateTime.Parse(Date);

            if (ID != 0)
            {
                if (IsException)
                {
                    var exceptionShift = ExceptionShiftServices.Instance.GetExceptionShift(ID);
                    model.StartTime = exceptionShift.StartTime;
                    model.EndTime = exceptionShift.EndTime;
                    model.RecurEndDate = exceptionShift.ExceptionDate.ToString("yyyy-MM-dd");
                    model.OnlyThis = true;
                    model.ID = ID;
                }
                else
                {
                    var shift = ShiftServices.Instance.GetShift(ID);
                    model.EmployeeID = shift.EmployeeID;
                    model.ID = ID;
                    //model.Date = shift.Date;
                    model.Day = shift.Day;
                    model.StartTime = shift.StartTime;
                    model.EndTime = shift.EndTime;
                    model.IsRecurring = shift.IsRecurring;
                    var recurringShift = RecurringShiftServices.Instance.GetRecurringShiftWRTBusiness(shift.Business, shift.ID);
                    if (recurringShift != null)
                    {
                        model.RecurEnd = recurringShift.RecurEnd;
                        model.RecurEndDate = recurringShift.RecurEndDate;
                        model.Frequency = recurringShift.Frequency;
                    }
                }
            }
            else
            {
                var openeinghour = OpeningHourServices.Instance.GetOpeningHoursWRTBusiness(loggedInUser.Company, "").Where(x => x.Day == DateTime.Now.DayOfWeek.ToString()).FirstOrDefault();
                model.StartTime = openeinghour.Time.Split('-')[0].Trim();
                model.EndTime = openeinghour.Time.Split('-')[1].Trim();
            }
            return PartialView("_Action", model);
        }


        [HttpPost]
        public JsonResult Action(DateTime Date, int EmployeeID, int ID, bool IsRecurring, string StartTime, string EndTime,
                string Frequency, string RecurEnd, string RecurEndDate, bool OnlyThis, bool IsNotWorking = false)
        {
            var loggedInUser = UserManager.FindById(User.Identity.GetUserId());
            if (OnlyThis)
            {

                if (IsNotWorking)
                {
                    var shift = ShiftServices.Instance.GetShift(ID);
                    if (shift != null)
                    {
                        var exceptionShift = new ExceptionShift();
                        exceptionShift.Business = loggedInUser.Company;
                        exceptionShift.ExceptionDate = Date;
                        exceptionShift.ShiftID = shift.ID;
                        exceptionShift.StartTime = StartTime;
                        exceptionShift.EndTime = EndTime;
                        exceptionShift.IsNotWorking = IsNotWorking;
                        ExceptionShiftServices.Instance.SaveExceptionShift(exceptionShift);
                    }
                    else
                    {
                        return Json(new { success = false, Message = "No Shift Found to save leave." }, JsonRequestBehavior.AllowGet);
                    }
                }
                else
                {
                    var roster = TimeTableRosterServices.Instance.GetTimeTableRosterByEmpID(EmployeeID);
                    if (roster == null)
                    {
                        roster = new TimeTableRoster();
                        roster.Business = loggedInUser.Company;
                        roster.RosterStartDate = Date;
                        roster.EmployeeID = EmployeeID;
                        TimeTableRosterServices.Instance.SaveTimeTableRoster(roster);

                    }

                    //Exception Shift Save

                    var shift = new Shift();
                    if (ID == 0)
                    {
                        shift.IsRecurring = false;
                        shift.StartTime = StartTime;
                        shift.EndTime = EndTime;
                        shift.Business = loggedInUser.Company;
                        shift.Date = Date;
                        shift.EmployeeID = EmployeeID;
                        shift.Day = Date.DayOfWeek.ToString();
                        ShiftServices.Instance.SaveShift(shift);

                        var exceptionShift = new ExceptionShift();
                        exceptionShift.Business = loggedInUser.Company;
                        exceptionShift.ExceptionDate = DateTime.Parse(RecurEndDate);
                        if (ID == 0)
                        {
                            exceptionShift.ShiftID = shift.ID;
                        }
                        else
                        {
                            exceptionShift.ShiftID = ID;

                        }

                        exceptionShift.StartTime = StartTime;
                        exceptionShift.EndTime = EndTime;
                        ExceptionShiftServices.Instance.SaveExceptionShift(exceptionShift);
                    }
                    else
                    {
                        var exceptionShift = ExceptionShiftServices.Instance.GetExceptionShiftsWRTBusiness(loggedInUser.Company).Where(x => x.ExceptionDate.ToString("yyyy-MM-dd") == Date.ToString("yyyy-MM-dd") && x.ID == ID).FirstOrDefault();
                        //var exceptionShift = ExceptionShiftServices.Instance.GetExceptionShift(ID);
                        if (exceptionShift == null)
                        {
                            exceptionShift = new ExceptionShift();
                            exceptionShift.StartTime = StartTime;
                            exceptionShift.EndTime = EndTime;
                            exceptionShift.ExceptionDate = Date;
                            exceptionShift.Business = loggedInUser.Company;
                            exceptionShift.ShiftID = ID;
                            ExceptionShiftServices.Instance.SaveExceptionShift(exceptionShift);
                        }
                        else
                        {
                            exceptionShift.StartTime = StartTime;
                            exceptionShift.EndTime = EndTime;
                            exceptionShift.IsNotWorking = false;
                            ExceptionShiftServices.Instance.UpdateExceptionShift(exceptionShift);
                        }

                    }

                }





                return Json(new { success = true }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                if (ID != 0)
                {
                    var shift = ShiftServices.Instance.GetShift(ID);
                    if (IsNotWorking)
                    {
                        if (shift != null)
                        {
                            var exceptionShift = new ExceptionShift();
                            exceptionShift.Business = loggedInUser.Company;
                            exceptionShift.ExceptionDate = Date;
                            exceptionShift.ShiftID = shift.ID;
                            exceptionShift.StartTime = StartTime;
                            exceptionShift.EndTime = EndTime;
                            exceptionShift.IsNotWorking = IsNotWorking;
                            ExceptionShiftServices.Instance.SaveExceptionShift(exceptionShift);
                            return Json(new { success = true }, JsonRequestBehavior.AllowGet);

                        }
                        else
                        {
                            return Json(new { success = false, Message = "No Shift Found to save leave." }, JsonRequestBehavior.AllowGet);
                        }
                    }
                    else
                    {
                        if (shift != null)
                        {
                            shift.StartTime = StartTime;
                            shift.EndTime = EndTime;
                            shift.IsRecurring = IsRecurring;
                            shift.Date = Date;
                            shift.Day = Date.DayOfWeek.ToString();

                            ShiftServices.Instance.UpdateShift(shift);

                            var recurringShift = RecurringShiftServices.Instance.GetRecurringShiftWRTBusiness(loggedInUser.Company, shift.ID);
                            if (recurringShift != null)
                            {
                                recurringShift.Frequency = Frequency;
                                recurringShift.RecurEnd = RecurEnd;
                                recurringShift.RecurEndDate = RecurEndDate;
                                recurringShift.ShiftID = shift.ID;
                                RecurringShiftServices.Instance.UpdateRecurringShift(recurringShift);
                            }
                            return Json(new { success = true }, JsonRequestBehavior.AllowGet);
                        }
                        else
                        {
                            return Json(new { success = false }, JsonRequestBehavior.AllowGet);
                        }
                    }
                }
                else
                {
                    var shift = new Shift();
                    shift.Business = loggedInUser.Company;
                    shift.IsRecurring = IsRecurring;
                    shift.Date = Date;
                    shift.Day = Date.DayOfWeek.ToString();
                    shift.StartTime = StartTime;
                    shift.EndTime = EndTime;
                    shift.EmployeeID = EmployeeID;
                    ShiftServices.Instance.SaveShift(shift);

                    if (IsRecurring)
                    {
                        var recurringShift = new RecurringShift();
                        recurringShift.Business = loggedInUser.Company;
                        recurringShift.RecurEnd = RecurEnd;
                        recurringShift.RecurEndDate = RecurEndDate;
                        recurringShift.ShiftID = shift.ID;
                        recurringShift.Frequency = Frequency;
                        recurringShift.IsDeleted = false;
                        RecurringShiftServices.Instance.SaveRecurringShift(recurringShift);
                    }
                    else
                    {
                        var exceptionShift = new ExceptionShift();
                        exceptionShift.Business = loggedInUser.Company;
                        exceptionShift.ExceptionDate = Date;
                        exceptionShift.ShiftID = shift.ID;
                        exceptionShift.StartTime = StartTime;
                        exceptionShift.EndTime = EndTime;
                        exceptionShift.IsNotWorking = IsNotWorking;
                        ExceptionShiftServices.Instance.SaveExceptionShift(exceptionShift);
                    }

                    var roster = TimeTableRosterServices.Instance.GetTimeTableRosterByEmpID(EmployeeID);
                    if (roster == null)
                    {
                        roster = new TimeTableRoster();
                        roster.Business = loggedInUser.Company;
                        roster.RosterStartDate = Date;
                        roster.EmployeeID = EmployeeID;
                        TimeTableRosterServices.Instance.SaveTimeTableRoster(roster);

                    }
                    return Json(new { success = true }, JsonRequestBehavior.AllowGet);

                }
            }
        }



        [HttpGet]
        public JsonResult Delete(bool IsRecur, int ID, bool IsException)
        {
            var loggedInUser = UserManager.FindById(User.Identity.GetUserId());
            var Shift = ShiftServices.Instance.GetShift(ID);
            if (Shift != null)
            {
                if (IsRecur)
                {
                    var recurringShift = RecurringShiftServices.Instance.GetRecurringShiftWRTBusiness(loggedInUser.Company, Shift.ID);
                    if (recurringShift != null)
                    {
                        RecurringShiftServices.Instance.DeleteRecurringShift(recurringShift.ID);
                    }

                    var exceptionShift = ExceptionShiftServices.Instance.GetExceptionShiftWRTBusinessAndShift(loggedInUser.Company, Shift.ID);
                    foreach (var item in exceptionShift)
                    {
                        ExceptionShiftServices.Instance.DeleteExceptionShift(item.ID);

                    }

                    ShiftServices.Instance.DeleteShift(ID);
                    //var exceptionShift = ExceptionShiftServices.Instance.GetExceptionShiftWRTBusiness(loggedInUser.Company, recurringShift.ID);
                    //if(exceptionShift != null) {
                    //    ExceptionShiftServices.Instance.DeleteExceptionShift(exceptionShift.ID);
                    //}


                }
                else if (IsException)
                {

                    //var exceptionShift = ExceptionShiftServices.Instance.GetExceptionShiftWRTBusinessAndShift(loggedInUser.Company, Shift.ID);
                    //foreach (var item in exceptionShift)
                    //{
                    //    ExceptionShiftServices.Instance.DeleteExceptionShift(item.ID);

                    //}
                    var excshift = ExceptionShiftServices.Instance.GetExceptionShift(ID);
                    if (excshift != null)
                    {
                        ExceptionShiftServices.Instance.DeleteExceptionShift(excshift.ID);

                    }


                }
                else
                {
                    var recurringShift = RecurringShiftServices.Instance.GetRecurringShiftWRTBusiness(loggedInUser.Company, Shift.ID);
                    if (recurringShift != null)
                    {
                        RecurringShiftServices.Instance.DeleteRecurringShift(recurringShift.ID);
                    }

                    var exceptionShift = ExceptionShiftServices.Instance.GetExceptionShiftWRTBusinessAndShift(loggedInUser.Company, Shift.ID);
                    foreach (var item in exceptionShift)
                    {
                        ExceptionShiftServices.Instance.DeleteExceptionShift(item.ID);

                    }


                    ShiftServices.Instance.DeleteShift(ID);

                }
                return Json(new { success = true }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                var exceptionShift = ExceptionShiftServices.Instance.GetExceptionShift(ID);
                if (exceptionShift != null)
                {
                    var sh = ShiftServices.Instance.GetShift(exceptionShift.ShiftID);
                    if (sh != null && !sh.IsRecurring)
                    {
                        ShiftServices.Instance.DeleteShift(sh.ID);
                    }

                    ExceptionShiftServices.Instance.DeleteExceptionShift(exceptionShift.ID);
                }
                return Json(new { success = false }, JsonRequestBehavior.AllowGet);

            }
        }
    }
}