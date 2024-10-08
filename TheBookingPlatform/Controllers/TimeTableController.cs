using Microsoft.Ajax.Utilities;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using System.Xml;
using TheBookingPlatform.Entities;
using TheBookingPlatform.Services;
using TheBookingPlatform.ViewModels;

namespace TheBookingPlatform.Controllers
{
    public class TimeTableController : Controller
    {
        // GET: TimeTable
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

        public TimeTableController()
        {
        }

        public TimeTableController(AMUserManager userManager, AMSignInManager signInManager)
        {
            UserManager = userManager;
            SignInManager = signInManager;
        }

        #endregion

        public static DateTime GetNextDayOfWeek(DateTime currentWeekStart, DayOfWeek targetDayOfWeek)
        {
            int daysToAdd = ((int)targetDayOfWeek - (int)currentWeekStart.DayOfWeek + 7) % 7;
            return currentWeekStart.AddDays(daysToAdd);
        }
        public string GetNextDayStatus(DateTime CurrentWeekStart,DateTime rosterStartDate, string targetDayString)
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
            if (isEvenWeek && difference.TotalDays > 0)
            {
                isNextTargetDayInCurrentWeek = true;
            }
           
            
            

            return isNextTargetDayInCurrentWeek ? "YES" : "NO";
        }
        public ActionResult Index()
        {
            var daysOfWeek = new List<string> { "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday", "Sunday" };
            TimeTableListingViewModel model = new TimeTableListingViewModel();
            var LoggedInUser = UserManager.FindById(User.Identity.GetUserId());

            // Get the current date and time
            DateTime currentDate = DateTime.Now;

            // Calculate the number of days to subtract to reach Monday
            int daysToSubtract = (int)currentDate.DayOfWeek - 1; // Monday is the first day of the week, so we subtract 1

            // Adjust the current date to Monday
            DateTime currentWeekStart = currentDate.AddDays(-daysToSubtract);

            // Calculate the end of the week (Sunday)
            DateTime currentWeekEnd = currentWeekStart.AddDays(6);

            model.CurrentWeekStart = currentWeekStart;
            model.CurrentWeekEnd = currentWeekEnd;

            if (LoggedInUser.Role == "Super Admin")
            {
                var employees = EmployeeServices.Instance.GetEmployee().Where(x=>x.IsActive == true).ToList();
                var listofTimeTable = new List<TimeTableModel>();
                foreach (var item in employees)
                {
                    var timetable = TimeTableServices.Instance.GetTimeTableByEmployeeID(item.ID);
                    float HoursWorked = 0;
                    foreach (var tt in timetable)
                    {
                        TimeSpan hours = DateTime.Parse(tt.TimeEnd) - DateTime.Parse(tt.TimeStart);
                        HoursWorked += float.Parse(hours.TotalHours.ToString());
                    }
                    var roster = TimeTableRosterServices.Instance.GetTimeTableRosterByEmpID(item.ID);
                    var listofFullTimeTable = new List<FullTimeTableModel>();
                    foreach (var time in timetable)
                    {
                        if (time.Type == "Bi-Weekly")
                        {
                            if (roster != null)
                            {
                                if (GetNextDayStatus(currentWeekStart, roster.RosterStartDate,time.Day.ToString()) == "YES")
                                {
                                    listofFullTimeTable.Add(new FullTimeTableModel { IsWorking = true, TimeTable = time });
                                }
                                else
                                {
                                    listofFullTimeTable.Add(new FullTimeTableModel { IsWorking = false, TimeTable = time });
                                }
                            }
                        }
                        else
                        {
                            listofFullTimeTable.Add(new FullTimeTableModel { IsWorking = true, TimeTable = time });
                        }

                    }
                    listofTimeTable.Add(new TimeTableModel { Employee = item, TimeTable = listofFullTimeTable, HoursWorked = HoursWorked });
                }
                model.TimeTables = listofTimeTable;
                model.OpeningHours = OpeningHourServices.Instance.GetOpeningHour();
                model.Employees = employees;

            }
            else
            {
                var employees = EmployeeServices.Instance.GetEmployee().Where(x=> x.IsActive == true && x.Business == LoggedInUser.Company).ToList();
                var listofTimeTable = new List<TimeTableModel>();
                foreach (var item in employees)
                {
                    var timetable = TimeTableServices.Instance.GetTimeTableByEmployeeID(item.ID);
                    float HoursWorked = 0;
                    foreach (var tt in timetable)
                    {
                        TimeSpan hours = DateTime.Parse(tt.TimeEnd) - DateTime.Parse(tt.TimeStart);
                        HoursWorked += float.Parse(hours.TotalHours.ToString());
                    }
                    var listofFullTimeTable = new List<FullTimeTableModel>();

                    var roster = TimeTableRosterServices.Instance.GetTimeTableRosterByEmpID(item.ID);
                    foreach (var time in timetable)
                    {
                        if (time.Repeat)
                        {
                            if (time.RepeatEnd == "Custom Date" && time.DateOfRepeatEnd != "")
                            {
                                var DateToCheckBetween = DateTime.Parse(time.DateOfRepeatEnd);
                                if (IsDateInRange(DateToCheckBetween, model.CurrentWeekStart, model.CurrentWeekEnd))
                                {
                                    if (time.Type == "Bi-Weekly")
                                    {
                                        if (roster != null)
                                        {
                                            if (GetNextDayStatus(model.CurrentWeekStart, roster.RosterStartDate, time.Day.ToString()) == "YES")
                                            {
                                                listofFullTimeTable.Add(new FullTimeTableModel { IsWorking = true, TimeTable = time });
                                            }
                                            else
                                            {
                                                listofFullTimeTable.Add(new FullTimeTableModel { IsWorking = false, TimeTable = time });
                                            }
                                        }
                                    }
                                    else
                                    {
                                        listofFullTimeTable.Add(new FullTimeTableModel { IsWorking = true, TimeTable = time });
                                    }
                                }
                                else
                                {
                                    listofFullTimeTable.Add(new FullTimeTableModel { IsWorking = false, TimeTable = time });

                                }
                            }
                            else
                            {
                                if (time.Type == "Bi-Weekly")
                                {
                                    if (roster != null)
                                    {
                                        if (GetNextDayStatus(model.CurrentWeekStart, roster.RosterStartDate, time.Day.ToString()) == "YES")
                                        {
                                            listofFullTimeTable.Add(new FullTimeTableModel { IsWorking = true, TimeTable = time });
                                        }
                                        else
                                        {
                                            listofFullTimeTable.Add(new FullTimeTableModel { IsWorking = false, TimeTable = time });
                                        }
                                    }
                                }
                                else
                                {
                                    listofFullTimeTable.Add(new FullTimeTableModel { IsWorking = true, TimeTable = time });
                                }
                            }
                        }
                        else
                        {
                            if (time.Type == "Bi-Weekly")
                            {
                                if (roster != null)
                                {
                                    if (GetNextDayStatus(currentWeekStart, roster.RosterStartDate, time.Day.ToString()) == "YES")
                                    {
                                        listofFullTimeTable.Add(new FullTimeTableModel { IsWorking = true, TimeTable = time });
                                    }
                                    else
                                    {
                                        listofFullTimeTable.Add(new FullTimeTableModel { IsWorking = false, TimeTable = time });
                                    }
                                }
                            }
                            else
                            {
                                listofFullTimeTable.Add(new FullTimeTableModel { IsWorking = true, TimeTable = time });
                            }
                        }

                    }
                    listofTimeTable.Add(new TimeTableModel { Employee = item, TimeTable = listofFullTimeTable, HoursWorked = HoursWorked });
                }
                // Define a custom order for days of the week
                model.TimeTables = listofTimeTable;
                model.OpeningHours = OpeningHourServices.Instance.GetOpeningHoursWRTBusiness(LoggedInUser.Company);
                model.Employees = employees;
            }
            return View(model);
        }

        [HttpGet]
        public ActionResult Action(int EmployeeID)
        {
            TimeTableActionViewModel model = new TimeTableActionViewModel();    
            model.Employee = EmployeeServices.Instance.GetEmployee(EmployeeID);
            model.TimeTable = TimeTableServices.Instance.GetTimeTableByEmployeeID(EmployeeID);
            
            if(model.TimeTable.Count() > 0)
            {
                model.ID = model.TimeTable[0].ID;
               
            }
            var roster = TimeTableRosterServices.Instance.GetTimeTableRosterByEmpID(EmployeeID);
            if (roster != null)
            {
                model.RosterStartDate = roster.RosterStartDate;
            }
            else
            {
                model.RosterStartDate = DateTime.Now;
            }
            return PartialView("_Action", model);
        }

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
      
        
        [HttpGet]
        public ActionResult GetNextWeek(DateTime CurrentWeekStart,DateTime CurrentWeekEnd)
        {
            TimeTableListingViewModel model = new TimeTableListingViewModel();
            var LoggedInUser = UserManager.FindById(User.Identity.GetUserId());

            
            // Calculate the start and end dates of the current 
            model.CurrentWeekEnd =   CurrentWeekEnd.AddDays(7); 
            model.CurrentWeekStart = CurrentWeekStart.AddDays(7);
            if (LoggedInUser.Role == "Super Admin")
            {
                var employees = EmployeeServices.Instance.GetEmployee().Where(x => x.IsActive == true).ToList();
                var listofTimeTable = new List<TimeTableModel>();
                foreach (var item in employees)
                {
                    var timetable = TimeTableServices.Instance.GetTimeTableByEmployeeID(item.ID);
                    float HoursWorked = 0;
                    foreach (var tt in timetable)
                    {
                        TimeSpan hours = DateTime.Parse(tt.TimeEnd) - DateTime.Parse(tt.TimeStart);
                        HoursWorked += float.Parse(hours.TotalHours.ToString());
                    }
                    var listofFullTimeTable = new List<FullTimeTableModel>();
                    var roster = TimeTableRosterServices.Instance.GetTimeTableRosterByEmpID(item.ID);
                    foreach (var time in timetable)
                    {
                        if (time.Type == "Bi-Weekly")
                        {
                            if (roster != null)
                            {
                                if (GetNextDayStatus(model.CurrentWeekStart, roster.RosterStartDate, time.Day.ToString()) == "YES")
                                {
                                    listofFullTimeTable.Add(new FullTimeTableModel { IsWorking = true, TimeTable = time });
                                }
                                else
                                {
                                    listofFullTimeTable.Add(new FullTimeTableModel { IsWorking = false, TimeTable = time });
                                }
                            }
                        }
                        else
                        {
                            listofFullTimeTable.Add(new FullTimeTableModel { IsWorking = true, TimeTable = time });
                        }

                    }
                    listofTimeTable.Add(new TimeTableModel { Employee = item, TimeTable = listofFullTimeTable, HoursWorked = HoursWorked });
                }
                model.TimeTables = listofTimeTable;
                model.OpeningHours = OpeningHourServices.Instance.GetOpeningHour();
                model.Employees = employees;

            }
            else
            {
                var employees = EmployeeServices.Instance.GetEmployee().Where(x => x.IsActive == true && x.Business == LoggedInUser.Company).ToList();
                var listofTimeTable = new List<TimeTableModel>();
                foreach (var item in employees)
                {
                    var timetable = TimeTableServices.Instance.GetTimeTableByEmployeeID(item.ID);
                    float HoursWorked = 0;
                    foreach (var tt in timetable)
                    {
                        TimeSpan hours = DateTime.Parse(tt.TimeEnd) - DateTime.Parse(tt.TimeStart);
                        HoursWorked += float.Parse(hours.TotalHours.ToString());
                    }
                    var listofFullTimeTable = new List<FullTimeTableModel>();
                    var roster = TimeTableRosterServices.Instance.GetTimeTableRosterByEmpID(item.ID);
                    foreach (var time in timetable)
                    {
                        if (time.Repeat)
                        {
                            if(time.RepeatEnd == "Custom Date" && time.DateOfRepeatEnd != "")
                            {
                                var DateToCheckBetween = DateTime.Parse(time.DateOfRepeatEnd);
                                if (IsDateInRange(DateToCheckBetween, model.CurrentWeekStart, model.CurrentWeekEnd))
                                {
                                    if (time.Type == "Bi-Weekly")
                                    {
                                        if (roster != null)
                                        {
                                            if (GetNextDayStatus(model.CurrentWeekStart, roster.RosterStartDate, time.Day.ToString()) == "YES")
                                            {
                                                listofFullTimeTable.Add(new FullTimeTableModel { IsWorking = true, TimeTable = time });
                                            }
                                            else
                                            {
                                                listofFullTimeTable.Add(new FullTimeTableModel { IsWorking = false, TimeTable = time });
                                            }
                                        }
                                    }
                                    else
                                    {
                                        listofFullTimeTable.Add(new FullTimeTableModel { IsWorking = true, TimeTable = time });
                                    }
                                }
                                else
                                {
                                    listofFullTimeTable.Add(new FullTimeTableModel { IsWorking = false, TimeTable = time });

                                }
                            }
                            else
                            {
                                if (time.Type == "Bi-Weekly")
                                {
                                    if (roster != null)
                                    {
                                        if (GetNextDayStatus(model.CurrentWeekStart, roster.RosterStartDate, time.Day.ToString()) == "YES")
                                        {
                                            listofFullTimeTable.Add(new FullTimeTableModel { IsWorking = true, TimeTable = time });
                                        }
                                        else
                                        {
                                            listofFullTimeTable.Add(new FullTimeTableModel { IsWorking = false, TimeTable = time });
                                        }
                                    }
                                }
                                else
                                {
                                    listofFullTimeTable.Add(new FullTimeTableModel { IsWorking = true, TimeTable = time });
                                }
                            }
                        }
                        else
                        {
                            if (time.Type == "Bi-Weekly")
                            {
                                if (roster != null)
                                {
                                    if (GetNextDayStatus(model.CurrentWeekStart, roster.RosterStartDate, time.Day.ToString()) == "YES")
                                    {
                                        listofFullTimeTable.Add(new FullTimeTableModel { IsWorking = true, TimeTable = time });
                                    }
                                    else
                                    {
                                        listofFullTimeTable.Add(new FullTimeTableModel { IsWorking = false, TimeTable = time });
                                    }
                                }
                            }
                            else
                            {
                                listofFullTimeTable.Add(new FullTimeTableModel { IsWorking = true, TimeTable = time });
                            }
                        }

                    }
                    listofTimeTable.Add(new TimeTableModel { Employee = item, TimeTable = listofFullTimeTable, HoursWorked = HoursWorked });
                }
                // Define a custom order for days of the week
                model.TimeTables = listofTimeTable;
                model.OpeningHours = OpeningHourServices.Instance.GetOpeningHour().Where(x => x.Business == LoggedInUser.Company).ToList();
                model.Employees = employees;
            }
            return PartialView("Index",model);
        }


        [HttpGet]
        public ActionResult GetPreviousWeek(DateTime CurrentWeekStart, DateTime CurrentWeekEnd)
        {
            TimeTableListingViewModel model = new TimeTableListingViewModel();
            var LoggedInUser = UserManager.FindById(User.Identity.GetUserId());


            // Calculate the start and end dates of the current 
            model.CurrentWeekEnd = CurrentWeekEnd.AddDays(-7);
            model.CurrentWeekStart = CurrentWeekStart.AddDays(-7);
            if (LoggedInUser.Role == "Super Admin")
            {
                var employees = EmployeeServices.Instance.GetEmployee().Where(x => x.IsActive == true).ToList();
                var listofTimeTable = new List<TimeTableModel>();
                foreach (var item in employees)
                {
                    var timetable = TimeTableServices.Instance.GetTimeTableByEmployeeID(item.ID);
                    float HoursWorked = 0;
                    foreach (var tt in timetable)
                    {
                        TimeSpan hours = DateTime.Parse(tt.TimeEnd) - DateTime.Parse(tt.TimeStart);
                        HoursWorked += float.Parse(hours.TotalHours.ToString());
                    }
                    var listofFullTimeTable = new List<FullTimeTableModel>();
                    var roster = TimeTableRosterServices.Instance.GetTimeTableRosterByEmpID(item.ID);
                    foreach (var time in timetable)
                    {
                        if (time.Type == "Bi-Weekly")
                        {
                            if (roster != null)
                            {
                                if (GetNextDayStatus(model.CurrentWeekStart, roster.RosterStartDate, time.Day.ToString()) == "YES")
                                {
                                    listofFullTimeTable.Add(new FullTimeTableModel { IsWorking = true, TimeTable = time });
                                }
                                else
                                {
                                    listofFullTimeTable.Add(new FullTimeTableModel { IsWorking = false, TimeTable = time });
                                }
                            }
                        }
                        else
                        {
                            listofFullTimeTable.Add(new FullTimeTableModel { IsWorking = true, TimeTable = time });
                        }

                    }
                    listofTimeTable.Add(new TimeTableModel { Employee = item, TimeTable = listofFullTimeTable, HoursWorked = HoursWorked });
                }
                model.TimeTables = listofTimeTable;
                model.OpeningHours = OpeningHourServices.Instance.GetOpeningHour();
                model.Employees = employees;

            }
            else
            {
                var employees = EmployeeServices.Instance.GetEmployee().Where(x => x.IsActive == true && x.Business == LoggedInUser.Company).ToList();
                var listofTimeTable = new List<TimeTableModel>();
                foreach (var item in employees)
                {
                    var timetable = TimeTableServices.Instance.GetTimeTableByEmployeeID(item.ID);
                    float HoursWorked = 0;
                    foreach (var tt in timetable)
                    {
                        TimeSpan hours = DateTime.Parse(tt.TimeEnd) - DateTime.Parse(tt.TimeStart);
                        HoursWorked += float.Parse(hours.TotalHours.ToString());
                    }
                    var listofFullTimeTable = new List<FullTimeTableModel>();
                    var roster = TimeTableRosterServices.Instance.GetTimeTableRosterByEmpID(item.ID);
                    foreach (var time in timetable)
                    {
                        if (time.Repeat)
                        {
                            if (time.RepeatEnd == "Custom Date" && time.DateOfRepeatEnd != "")
                            {
                                var DateToCheckBetween = DateTime.Parse(time.DateOfRepeatEnd);
                                if (IsDateInRange(DateToCheckBetween, model.CurrentWeekStart, model.CurrentWeekEnd))
                                {
                                    if (time.Type == "Bi-Weekly")
                                    {
                                        if (roster != null)
                                        {
                                            if (GetNextDayStatus(model.CurrentWeekStart, roster.RosterStartDate, time.Day.ToString()) == "YES")
                                            {
                                                listofFullTimeTable.Add(new FullTimeTableModel { IsWorking = true, TimeTable = time });
                                            }
                                            else
                                            {
                                                listofFullTimeTable.Add(new FullTimeTableModel { IsWorking = false, TimeTable = time });
                                            }
                                        }
                                    }
                                    else
                                    {
                                        listofFullTimeTable.Add(new FullTimeTableModel { IsWorking = true, TimeTable = time });
                                    }
                                }
                                else
                                {
                                    listofFullTimeTable.Add(new FullTimeTableModel { IsWorking = false, TimeTable = time });

                                }
                            }
                            else
                            {
                                if (time.Type == "Bi-Weekly")
                                {
                                    if (roster != null)
                                    {
                                        if (GetNextDayStatus(model.CurrentWeekStart, roster.RosterStartDate, time.Day.ToString()) == "YES")
                                        {
                                            listofFullTimeTable.Add(new FullTimeTableModel { IsWorking = true, TimeTable = time });
                                        }
                                        else
                                        {
                                            listofFullTimeTable.Add(new FullTimeTableModel { IsWorking = false, TimeTable = time });
                                        }
                                    }
                                }
                                else
                                {
                                    listofFullTimeTable.Add(new FullTimeTableModel { IsWorking = true, TimeTable = time });
                                }
                            }
                        }
                        else
                        {
                            if (time.Type == "Bi-Weekly")
                            {
                                if (roster != null)
                                {
                                    if (GetNextDayStatus(model.CurrentWeekStart, roster.RosterStartDate, time.Day.ToString()) == "YES")
                                    {
                                        listofFullTimeTable.Add(new FullTimeTableModel { IsWorking = true, TimeTable = time });
                                    }
                                    else
                                    {
                                        listofFullTimeTable.Add(new FullTimeTableModel { IsWorking = false, TimeTable = time });
                                    }
                                }
                            }
                            else
                            {
                                listofFullTimeTable.Add(new FullTimeTableModel { IsWorking = true, TimeTable = time });
                            }
                        }

                    }
                    listofTimeTable.Add(new TimeTableModel { Employee = item, TimeTable = listofFullTimeTable, HoursWorked = HoursWorked });
                }
                // Define a custom order for days of the week
                model.TimeTables = listofTimeTable;
                model.OpeningHours = OpeningHourServices.Instance.GetOpeningHour().Where(x => x.Business == LoggedInUser.Company).ToList();
                model.Employees = employees;
            }
            return PartialView("Index", model);
        }



        //[HttpGet]
        //public JsonResult GetNextWeek(DateTime CurrentWeekStart, DateTime CurrentWeekEnd)
        //{
        //    TimeTableListingViewModel model = new TimeTableListingViewModel();
        //    var LoggedInUser = UserManager.FindById(User.Identity.GetUserId());

        //    if (LoggedInUser.Role == "Super Admin")
        //    {
        //        var employees = EmployeeServices.Instance.GetEmployee().Where(x => x.IsActive == true).ToList(); 
        //        var listofTimeTable = new List<TimeTableModel>();

        //        foreach (var item in employees)
        //        {
        //            float HoursWorked = 0;
        //            var timetable = TimeTableServices.Instance.GetTimeTableByEmployeeID(item.ID).ToList();
        //            var TimeTableFinalCheck = new List<TimeTable>();
        //            foreach (var tt in timetable)
        //            {

        //                if (tt.Type == "Weekly")
        //                {
        //                    TimeTableFinalCheck.Add(tt);

        //                    TimeSpan tspan = DateTime.Parse(tt.TimeEnd) - DateTime.Parse(tt.TimeStart);
        //                    HoursWorked += tspan.Hours;
        //                }
        //                else if (tt.Type == "Bi-Weekly")
        //                {
        //                    DateTime currentDate = CurrentWeekStart;
        //                    var histories = HistoryServices.Instance.GetHistories().Where(x => x.EmployeeName == item.Name).ToList();
        //                    foreach (var history in histories)
        //                    {

        //                        TimeSpan timeDifference = currentDate - history.Date;
        //                        int weeksAgo = (int)Math.Floor(timeDifference.TotalDays / 7);
        //                        if (weeksAgo > 0)
        //                        {
        //                            bool isTwoWeeksAgo = weeksAgo % 2 == 0;

        //                            if (!isTwoWeeksAgo)
        //                            {
        //                                TimeTableFinalCheck.Add(tt);

        //                                TimeSpan tspan = DateTime.Parse(tt.TimeEnd) - DateTime.Parse(tt.TimeStart);
        //                                HoursWorked += tspan.Hours;
        //                                break;
        //                            }
        //                        }
                               
        //                    }
        //                }



        //            }
        //            listofTimeTable.Add(new TimeTableModel { Employee = item, TimeTable = TimeTableFinalCheck,HoursWorked = HoursWorked });
        //        }

        //        model.TimeTables = listofTimeTable;
        //        model.OpeningHours = OpeningHourServices.Instance.GetOpeningHour();
        //        model.Employees = employees;
        //    }
        //    else
        //    {
        //        var employees = EmployeeServices.Instance.GetEmployee().Where(x => x.IsActive == true && x.Business == LoggedInUser.Company).ToList();
        //        var listofTimeTable = new List<TimeTableModel>();

        //        // Calculate start and end date for the next week
              
        //        DateTime today = DateTime.Now;

        //        foreach (var item in employees)
        //        {
        //            float hoursWorked = 0;
        //            var timetable = TimeTableServices.Instance.GetTimeTableByEmployeeID(item.ID).ToList();
        //            var TimeTableFinalCheck = new List<TimeTable>();
        //            foreach (var tt in timetable)
        //            {

        //                if (tt.Type == "Weekly")
        //                {
        //                    TimeTableFinalCheck.Add(tt);

        //                    TimeSpan tspan = DateTime.Parse(tt.TimeEnd) - DateTime.Parse(tt.TimeStart);
        //                    hoursWorked += tspan.Hours;
        //                }
        //                else if (tt.Type == "Bi-Weekly")
        //                {
        //                    DateTime currentDate = CurrentWeekStart;
        //                    var histories = HistoryServices.Instance.GetHistories().Where(x => x.EmployeeName == item.Name).ToList();
        //                    foreach (var history in histories)
        //                    {

        //                        TimeSpan timeDifference = currentDate - history.Date;
        //                        int weeksAgo = (int)Math.Floor(timeDifference.TotalDays / 7);
        //                        if (weeksAgo > 0)
        //                        {
        //                            bool isTwoWeeksAgo = weeksAgo % 2 == 0;

        //                            if (!isTwoWeeksAgo)
        //                            {
        //                                TimeTableFinalCheck.Add(tt);

        //                                TimeSpan tspan = DateTime.Parse(tt.TimeEnd) - DateTime.Parse(tt.TimeStart);
        //                                hoursWorked += tspan.Hours;
        //                                break;
        //                            }
        //                        }
                               
        //                    }
        //                }



        //            }
        //            listofTimeTable.Add(new TimeTableModel { Employee = item, TimeTable = TimeTableFinalCheck,HoursWorked = hoursWorked });
        //        }

        //        model.TimeTables = listofTimeTable;
        //        model.OpeningHours = OpeningHourServices.Instance.GetOpeningHour()
        //            .Where(x => x.Business == LoggedInUser.Company)
        //            .ToList();
        //        model.Employees = employees;
        //    }

        //    return Json(model, JsonRequestBehavior.AllowGet);
        //}


        [HttpGet]
        public ActionResult ActionNew(int EmployeeID,int ID)
        {
            TimeTableActionViewModel model = new TimeTableActionViewModel();
            model.ID = ID;
            model.TimeTable = TimeTableServices.Instance.GetTimeTableByEmployeeID(EmployeeID);
            model.Employee = EmployeeServices.Instance.GetEmployee(EmployeeID);
            var roster= TimeTableRosterServices.Instance.GetTimeTableRosterByEmpID(EmployeeID);
            if(roster != null)
            {
                model.RosterStartDate = roster.RosterStartDate;
            }
            else
            {
                model.RosterStartDate = DateTime.Now;
            }
            return PartialView("_Action", model);
        }



        [HttpPost]
        public ActionResult Action(TimeTableActionViewModel model)
        {

            var timetableRoster = TimeTableRosterServices.Instance.GetTimeTableRosterByEmpID(model.EmployeeID);
            if(timetableRoster == null)
            {
                var loggedInUser = UserManager.FindById(User.Identity.GetUserId());
                timetableRoster = new TimeTableRoster();
                timetableRoster.RosterStartDate = DateTime.Now;
                timetableRoster.EmployeeID= model.EmployeeID;
                timetableRoster.Business = loggedInUser.Company;
                TimeTableRosterServices.Instance.SaveTimeTableRoster(timetableRoster);
            }
         

            if (model.ID == 0)
            {
                // Input date
                DateTime inputDate = model.StartDate; // 24th June 2023, which is a Saturday

                // Calculate the start date (Monday of the same week)
                DateTime startDate = inputDate.AddDays(-(7 - (int)inputDate.DayOfWeek + 1));

                // Calculate the end date (Saturday of the same week)
                DateTime endDate = startDate.AddDays(6);

                for (int i = 0; i < 7; i++)
                {
                    var timeTable = new TimeTable();
                    
                    var LoggedInUser = UserManager.FindById(User.Identity.GetUserId());
                    var ToBeSavedDate = startDate.AddDays(i);
                    timeTable.Date = ToBeSavedDate;
                    if (i == 0)
                    {
                        if (model.Monday == true && model.MondayTime != null)
                        {
                            var TimeStart = model.MondayTime.Split('-').ToList()[0];
                            var TimeEnd = model.MondayTime.Split('-').ToList()[1];
                            timeTable.TimeStart = TimeStart;
                            timeTable.TimeEnd = TimeEnd;
                            timeTable.Type = model.MondayType;
                            timeTable.Repeat = model.MondayRepeatCB;
                            timeTable.RepeatEnd = model.MondayRepeatEnd;
                            timeTable.DateOfRepeatEnd = model.MondayDateOfRepeatEnd;
                            timeTable.Day = "Monday";

                        }
                        else
                        {
                            continue;
                        }

                        timeTable.EmployeeID = model.EmployeeID;
                        timeTable.StartDate = model.StartDate;
                        timeTable.Business = LoggedInUser.Company;
                        TimeTableServices.Instance.SaveTimeTable(timeTable);


                    }
                    if (i == 1)
                    {
                        if (model.Tuesday == true && model.TuesdayTime != null)
                        {
                            var TimeStart = model.TuesdayTime.Split('-').ToList()[0];
                            var TimeEnd = model.TuesdayTime.Split('-').ToList()[1];
                            timeTable.TimeStart = TimeStart;
                            timeTable.TimeEnd = TimeEnd;
                            timeTable.Day = "Tuesday";
                            timeTable.Repeat = model.TuesdayRepeatCB;
                            timeTable.RepeatEnd = model.TuesdayRepeatEnd;
                            timeTable.DateOfRepeatEnd = model.TuesdayDateOfRepeatEnd;
                            timeTable.Type = model.TuesdayType;
                        }
                        else
                        {
                            continue;
                        }

                        timeTable.EmployeeID = model.EmployeeID;
                        timeTable.StartDate = model.StartDate;

                        timeTable.Business = LoggedInUser.Company;
                        TimeTableServices.Instance.SaveTimeTable(timeTable);


                    }
                    if (i == 2)
                    {
                        if (model.Wednesday == true && model.WednesdayTime != null)
                        {
                            var TimeStart = model.WednesdayTime.Split('-').ToList()[0];
                            var TimeEnd = model.WednesdayTime.Split('-').ToList()[1];
                            timeTable.TimeStart = TimeStart;
                            timeTable.TimeEnd = TimeEnd;
                            timeTable.Day = "Wednesday";
                            timeTable.Repeat = model.WednesdayRepeatCB;
                            timeTable.RepeatEnd = model.WednesdayRepeatEnd;
                            timeTable.DateOfRepeatEnd = model.WednesdayDateOfRepeatEnd;
                            timeTable.Type = model.WednesdayType;
                        }
                        else
                        {
                            continue;
                        }

                        timeTable.EmployeeID = model.EmployeeID;
                        timeTable.StartDate = model.StartDate;
                        timeTable.Business = LoggedInUser.Company;
                        TimeTableServices.Instance.SaveTimeTable(timeTable);


                    }
                    if (i == 3)
                    {
                        if (model.Thursday == true && model.ThursdayTime != null)
                        {
                            
                            var TimeStart = model.ThursdayTime.Split('-').ToList()[0];
                            var TimeEnd = model.ThursdayTime.Split('-').ToList()[1];
                            timeTable.TimeStart = TimeStart;
                            timeTable.TimeEnd = TimeEnd;
                            timeTable.Day = "Thursday";
                            timeTable.Repeat = model.ThursdayRepeatCB;
                            timeTable.RepeatEnd = model.ThursdayRepeatEnd;
                            timeTable.DateOfRepeatEnd = model.ThursdayDateOfRepeatEnd;
                            timeTable.Type = model.ThursdayType;
                        }
                        else
                        {
                            continue;
                        }

                        timeTable.EmployeeID = model.EmployeeID;
                        timeTable.StartDate = model.StartDate;
                        timeTable.Business = LoggedInUser.Company;
                        TimeTableServices.Instance.SaveTimeTable(timeTable);


                    }
                    if (i == 4)
                    {
                        if (model.Friday == true && model.FridayTime != null)
                        {
                            var TimeStart = model.FridayTime.Split('-').ToList()[0];
                            var TimeEnd = model.FridayTime.Split('-').ToList()[1];
                            timeTable.TimeStart = TimeStart;
                            timeTable.TimeEnd = TimeEnd;
                            timeTable.Day = "Friday";
                            timeTable.Repeat = model.FridayRepeatCB;
                            timeTable.RepeatEnd = model.FridayRepeatEnd;
                            timeTable.DateOfRepeatEnd = model.FridayDateOfRepeatEnd;
                            timeTable.Type = model.FridayType;
                        }
                        else
                        {
                            continue;
                        }

                        timeTable.EmployeeID = model.EmployeeID;
                        timeTable.StartDate = model.StartDate;
                        timeTable.Business = LoggedInUser.Company;
                        TimeTableServices.Instance.SaveTimeTable(timeTable);


                    }
                    if (i == 5)
                    {
                        if (model.Saturday == true && model.SaturdayTime != null) 
                        {
                            var TimeStart = model.SaturdayTime.Split('-').ToList()[0];
                            var TimeEnd = model.SaturdayTime.Split('-').ToList()[1];
                            timeTable.TimeStart = TimeStart;
                            timeTable.Day = "Saturday";

                            timeTable.Repeat = model.SaturdayRepeatCB;
                            timeTable.RepeatEnd = model.SaturdayRepeatEnd;
                            timeTable.DateOfRepeatEnd = model.SaturdayDateOfRepeatEnd;
                            timeTable.TimeEnd = TimeEnd;
                            timeTable.Type = model.SaturdayType;
                        }
                        else
                        {
                            continue;
                        }

                        timeTable.EmployeeID = model.EmployeeID;
                        timeTable.StartDate = model.StartDate;
                        timeTable.Business = LoggedInUser.Company;
                        TimeTableServices.Instance.SaveTimeTable(timeTable);


                    }
                    if (i == 6)
                    {
                        if (model.Sunday == true && model.SundayTime != null)
                        {
                            var TimeStart = model.SundayTime.Split('-').ToList()[0];
                            var TimeEnd = model.SundayTime.Split('-').ToList()[1];
                            timeTable.TimeStart = TimeStart;
                            timeTable.TimeEnd = TimeEnd;
                            timeTable.Day = "Sunday";
                            timeTable.Repeat = model.SundayRepeatCB;
                            timeTable.RepeatEnd = model.SundayRepeatEnd;
                            timeTable.DateOfRepeatEnd = model.SundayDateOfRepeatEnd;

                            timeTable.Type = model.SundayType;
                        }
                        else
                        {
                            continue;
                        }

                        timeTable.EmployeeID = model.EmployeeID;
                        timeTable.StartDate = model.StartDate;
                        timeTable.Business = LoggedInUser.Company;
                        TimeTableServices.Instance.SaveTimeTable(timeTable);


                    }


                }
            }
            else
            {
                #region UpdateRegion
                var timeTables = TimeTableServices.Instance.GetTimeTableByEmployeeID(model.EmployeeID);
                var AlreadyHaveDays = timeTables.Select(x=>x.Day).ToList();
                foreach (var timeTable in timeTables)
                {

                    var timeTableinDB = TimeTableServices.Instance.GetTimeTable(timeTable.ID);

                    if (timeTableinDB.Day == "Monday")
                    {
                        if (model.Monday)
                        {
                            timeTableinDB.EmployeeID = model.EmployeeID;
                            timeTableinDB.StartDate = model.StartDate;
                            var TimeStart = model.MondayTime.Split('-').ToList()[0];
                            var TimeEnd = model.MondayTime.Split('-').ToList()[1];
                            timeTableinDB.TimeStart = TimeStart;
                            timeTableinDB.TimeEnd = TimeEnd;
                            timeTableinDB.Type = model.MondayType;
                            timeTableinDB.Repeat = model.MondayRepeatCB;
                            timeTableinDB.RepeatEnd = model.MondayRepeatEnd;
                            timeTableinDB.DateOfRepeatEnd = model.MondayDateOfRepeatEnd;
                            TimeTableServices.Instance.UpdateTimeTable(timeTableinDB);

                        }
                        else
                        {
                            TimeTableServices.Instance.DeleteTimeTable(timeTableinDB.ID);
                        }
                    }
                    else if(timeTableinDB.Day == "Tuesday")
                    {
                        if (model.Tuesday)
                        {
                            timeTableinDB.EmployeeID = model.EmployeeID;
                            timeTableinDB.StartDate = model.StartDate;
                            var TimeStart = model.TuesdayTime.Split('-').ToList()[0];
                            var TimeEnd = model.TuesdayTime.Split('-').ToList()[1];
                            timeTableinDB.TimeStart = TimeStart;
                            timeTableinDB.TimeEnd = TimeEnd;
                            timeTableinDB.Type = model.TuesdayType;
                            timeTableinDB.Repeat = model.TuesdayRepeatCB;
                            timeTableinDB.RepeatEnd = model.TuesdayRepeatEnd;
                            timeTableinDB.DateOfRepeatEnd = model.TuesdayDateOfRepeatEnd;


                            TimeTableServices.Instance.UpdateTimeTable(timeTableinDB);
                        }
                        else
                        {
                      
                            TimeTableServices.Instance.DeleteTimeTable(timeTableinDB.ID);
                        }
                    }
                    else if (timeTableinDB.Day == "Wednesday")
                    {
                        if (model.Wednesday)
                        {
                            var TimeStart = model.WednesdayTime.Split('-').ToList()[0];
                            var TimeEnd = model.WednesdayTime.Split('-').ToList()[1];
                            timeTableinDB.TimeStart = TimeStart;
                            timeTableinDB.TimeEnd = TimeEnd;
                            timeTableinDB.Type = model.WednesdayType;
                            timeTableinDB.Repeat = model.WednesdayRepeatCB;
                            timeTableinDB.RepeatEnd = model.WednesdayRepeatEnd;
                            timeTableinDB.DateOfRepeatEnd = model.WednesdayDateOfRepeatEnd;


                            TimeTableServices.Instance.UpdateTimeTable(timeTableinDB);
                        }
                        else
                        {
                            TimeTableServices.Instance.DeleteTimeTable(timeTableinDB.ID);
                        }
                    }
                    else if (timeTableinDB.Day == "Thursday")
                    {
                        if (model.Thursday)
                        {
                            var TimeStart = model.ThursdayTime.Split('-').ToList()[0];
                            var TimeEnd = model.ThursdayTime.Split('-').ToList()[1];
                            timeTableinDB.TimeStart = TimeStart;
                            timeTableinDB.TimeEnd = TimeEnd;
                            timeTableinDB.Type = model.ThursdayType;
                            timeTableinDB.Repeat = model.ThursdayRepeatCB;
                            timeTableinDB.RepeatEnd = model.ThursdayRepeatEnd;
                            timeTableinDB.DateOfRepeatEnd = model.ThursdayDateOfRepeatEnd;


                            TimeTableServices.Instance.UpdateTimeTable(timeTableinDB);
                        }
                        else
                        {
                            TimeTableServices.Instance.DeleteTimeTable(timeTableinDB.ID);
                        }
                    }
                    else if (timeTableinDB.Day == "Friday")
                    {
                        if (model.Friday)
                        {
                            var TimeStart = model.FridayTime.Split('-').ToList()[0];
                            var TimeEnd = model.FridayTime.Split('-').ToList()[1];
                            timeTableinDB.TimeStart = TimeStart;
                            timeTableinDB.TimeEnd = TimeEnd;
                            timeTableinDB.Repeat = model.FridayRepeatCB;
                            timeTableinDB.RepeatEnd = model.FridayRepeatEnd;
                            timeTableinDB.DateOfRepeatEnd = model.FridayDateOfRepeatEnd;


                            timeTableinDB.Type = model.FridayType;
                            TimeTableServices.Instance.UpdateTimeTable(timeTableinDB);
                        }
                        else
                        {
                            TimeTableServices.Instance.DeleteTimeTable(timeTableinDB.ID);
                        }
                    }
                    else if (timeTableinDB.Day == "Saturday")
                    {
                        if (model.Saturday)
                        {
                            var TimeStart = model.SaturdayTime.Split('-').ToList()[0];
                            var TimeEnd = model.SaturdayTime.Split('-').ToList()[1];
                            timeTableinDB.TimeStart = TimeStart;
                            timeTableinDB.TimeEnd = TimeEnd;

                            timeTableinDB.Repeat = model.SaturdayRepeatCB;
                            timeTableinDB.RepeatEnd = model.SaturdayRepeatEnd;
                            timeTableinDB.DateOfRepeatEnd = model.SaturdayDateOfRepeatEnd;
                            timeTableinDB.Type = model.SaturdayType;
                            TimeTableServices.Instance.UpdateTimeTable(timeTableinDB);
                        }
                        else
                        {
                            TimeTableServices.Instance.DeleteTimeTable(timeTableinDB.ID);
                        }
                    }
                    else if (timeTableinDB.Day == "Sunday")
                    {
                        if (model.Sunday)
                        {
                            var TimeStart = model.SundayTime.Split('-').ToList()[0];
                            var TimeEnd = model.SundayTime.Split('-').ToList()[1];
                            timeTableinDB.TimeStart = TimeStart;
                            timeTableinDB.TimeEnd = TimeEnd;
                            timeTableinDB.Type = model.SundayType;
                            timeTableinDB.Repeat = model.SundayRepeatCB;
                            timeTableinDB.RepeatEnd = model.SundayRepeatEnd;
                            timeTableinDB.DateOfRepeatEnd = model.SundayDateOfRepeatEnd;

                            TimeTableServices.Instance.UpdateTimeTable(timeTableinDB);
                        }
                        else
                        {
                            TimeTableServices.Instance.DeleteTimeTable(timeTableinDB.ID);
                        }
                    }


                }
                #endregion



                #region SaveNewRegion
                if (model.Monday == true && !AlreadyHaveDays.Contains("Monday"))
                {
                    var timetable = new TimeTable();
                    DateTime inputDate = model.StartDate;
                    DateTime startDate = inputDate.AddDays(-(7 - (int)inputDate.DayOfWeek + 1));
                    var LoggedInUser = UserManager.FindById(User.Identity.GetUserId());
                    var ToBeSavedDate = startDate.AddDays(0);
                    timetable.Date = ToBeSavedDate;
                    timetable.Day = "Monday";
                    var TimeStart = model.MondayTime.Split('-').ToList()[0];
                    var TimeEnd = model.MondayTime.Split('-').ToList()[1];
                    timetable.TimeStart = TimeStart;
                    timetable.TimeEnd = TimeEnd;
                    timetable.Type = model.MondayType;
                    timetable.EmployeeID = model.EmployeeID;
                    timetable.StartDate = model.StartDate;
                    timetable.Business = LoggedInUser.Company;
                    timetable.Repeat = model.MondayRepeatCB;
                    timetable.RepeatEnd = model.MondayRepeatEnd;
                    timetable.DateOfRepeatEnd = model.MondayDateOfRepeatEnd;


                    TimeTableServices.Instance.SaveTimeTable(timetable);


                }
                if (model.Tuesday == true && !AlreadyHaveDays.Contains("Tuesday"))
                {
                    var timetable = new TimeTable();
                    DateTime inputDate = model.StartDate;
                    DateTime startDate = inputDate.AddDays(-(7 - (int)inputDate.DayOfWeek + 1));
                    var LoggedInUser = UserManager.FindById(User.Identity.GetUserId());
                    var ToBeSavedDate = startDate.AddDays(1);
                    timetable.Date = ToBeSavedDate;
                    timetable.Day = "Tuesday";
                    var TimeStart = model.TuesdayTime.Split('-').ToList()[0];
                    var TimeEnd = model.TuesdayTime.Split('-').ToList()[1];
                    timetable.TimeStart = TimeStart;
                    timetable.TimeEnd = TimeEnd;
                    timetable.Type = model.TuesdayType;
                    timetable.EmployeeID = model.EmployeeID;
                    timetable.StartDate = model.StartDate;
                    timetable.Business = LoggedInUser.Company;
                    timetable.Repeat = model.TuesdayRepeatCB;
                    timetable.RepeatEnd = model.TuesdayRepeatEnd;
                    timetable.DateOfRepeatEnd = model.TuesdayDateOfRepeatEnd;


                    TimeTableServices.Instance.SaveTimeTable(timetable);
                }
                if (model.Wednesday == true && !AlreadyHaveDays.Contains("Wednesday"))
                {
                    var timetable = new TimeTable();
                    DateTime inputDate = model.StartDate;
                    DateTime startDate = inputDate.AddDays(-(7 - (int)inputDate.DayOfWeek + 1));
                    var LoggedInUser = UserManager.FindById(User.Identity.GetUserId());
                    var ToBeSavedDate = startDate.AddDays(2);
                    timetable.Date = ToBeSavedDate;
                    timetable.Day = "Wednesday";
                    var TimeStart = model.WednesdayTime.Split('-').ToList()[0];
                    var TimeEnd = model.WednesdayTime.Split('-').ToList()[1];
                    timetable.TimeStart = TimeStart;
                    timetable.TimeEnd = TimeEnd;
                    timetable.Type = model.WednesdayType;
                    timetable.EmployeeID = model.EmployeeID;
                    timetable.StartDate = model.StartDate;
                    timetable.Business = LoggedInUser.Company;
                    timetable.Repeat = model.WednesdayRepeatCB;
                    timetable.RepeatEnd = model.WednesdayRepeatEnd;
                    timetable.DateOfRepeatEnd = model.WednesdayDateOfRepeatEnd;


                    TimeTableServices.Instance.SaveTimeTable(timetable);
                }
                if (model.Thursday == true && !AlreadyHaveDays.Contains("Thursday"))
                {
                    var timetable = new TimeTable();
                    DateTime inputDate = model.StartDate;
                    DateTime startDate = inputDate.AddDays(-(7 - (int)inputDate.DayOfWeek + 1));
                    var LoggedInUser = UserManager.FindById(User.Identity.GetUserId());
                    var ToBeSavedDate = startDate.AddDays(3);
                    timetable.Date = ToBeSavedDate;
                    timetable.Day = "Thursday";
                    var TimeStart = model.ThursdayTime.Split('-').ToList()[0];
                    var TimeEnd = model.ThursdayTime.Split('-').ToList()[1];
                    timetable.TimeStart = TimeStart;
                    timetable.TimeEnd = TimeEnd;
                    timetable.Type = model.ThursdayType;
                    timetable.EmployeeID = model.EmployeeID;
                    timetable.Repeat = model.ThursdayRepeatCB;
                    timetable.RepeatEnd = model.ThursdayRepeatEnd;
                    timetable.DateOfRepeatEnd = model.ThursdayDateOfRepeatEnd;


                    timetable.StartDate = model.StartDate;
                    timetable.Business = LoggedInUser.Company;
                    TimeTableServices.Instance.SaveTimeTable(timetable);
                }
                if (model.Friday == true && !AlreadyHaveDays.Contains("Friday"))
                {
                    var timetable = new TimeTable();
                    DateTime inputDate = model.StartDate;
                    DateTime startDate = inputDate.AddDays(-(7 - (int)inputDate.DayOfWeek + 1));
                    var LoggedInUser = UserManager.FindById(User.Identity.GetUserId());
                    var ToBeSavedDate = startDate.AddDays(4);
                    timetable.Date = ToBeSavedDate;
                    timetable.Day = "Friday";
                    var TimeStart = model.FridayTime.Split('-').ToList()[0];
                    var TimeEnd = model.FridayTime.Split('-').ToList()[1];
                    timetable.TimeStart = TimeStart;
                    timetable.TimeEnd = TimeEnd;
                    timetable.Type = model.FridayType;
                    timetable.EmployeeID = model.EmployeeID;
                    timetable.StartDate = model.StartDate;
                    timetable.Repeat = model.FridayRepeatCB;
                    timetable.RepeatEnd = model.FridayRepeatEnd;
                    timetable.DateOfRepeatEnd = model.FridayDateOfRepeatEnd;

                    timetable.Business = LoggedInUser.Company;
                    TimeTableServices.Instance.SaveTimeTable(timetable);
                }
                if (model.Saturday == true && !AlreadyHaveDays.Contains("Saturday"))
                {
                    var timetable = new TimeTable();
                    DateTime inputDate = model.StartDate;
                    DateTime startDate = inputDate.AddDays(-(7 - (int)inputDate.DayOfWeek + 1));
                    var LoggedInUser = UserManager.FindById(User.Identity.GetUserId());
                    var ToBeSavedDate = startDate.AddDays(5);
                    timetable.Date = ToBeSavedDate;
                    timetable.Day = "Saturday";
                    var TimeStart = model.SaturdayTime.Split('-').ToList()[0];
                    var TimeEnd = model.SaturdayTime.Split('-').ToList()[1];
                    timetable.TimeStart = TimeStart;
                    timetable.TimeEnd = TimeEnd;
                    timetable.Type = model.SaturdayType;
                    timetable.EmployeeID = model.EmployeeID;
                    timetable.StartDate = model.StartDate;
                    timetable.Business = LoggedInUser.Company;
                    timetable.Repeat = model.SaturdayRepeatCB;
                    timetable.RepeatEnd = model.SaturdayRepeatEnd;
                    timetable.DateOfRepeatEnd = model.SaturdayDateOfRepeatEnd;


                    TimeTableServices.Instance.SaveTimeTable(timetable);
                }
                if (model.Sunday == true && !AlreadyHaveDays.Contains("Sunday"))
                {
                    var timetable = new TimeTable();
                    DateTime inputDate = model.StartDate;
                    DateTime startDate = inputDate.AddDays(-(7 - (int)inputDate.DayOfWeek + 1));
                    var LoggedInUser = UserManager.FindById(User.Identity.GetUserId());
                    var ToBeSavedDate = startDate.AddDays(6);
                    timetable.Date = ToBeSavedDate;
                    timetable.Day = "Sunday";
                    var TimeStart = model.SundayTime.Split('-').ToList()[0];
                    var TimeEnd = model.SundayTime.Split('-').ToList()[1];
                    timetable.TimeStart = TimeStart;
                    timetable.TimeEnd = TimeEnd;
                    timetable.Type = model.SundayType;
                    timetable.EmployeeID = model.EmployeeID;
                    timetable.Repeat = model.SundayRepeatCB;
                    timetable.RepeatEnd = model.SundayRepeatEnd;
                    timetable.DateOfRepeatEnd = model.SundayDateOfRepeatEnd;
                    timetable.StartDate = model.StartDate;


                    timetable.Business = LoggedInUser.Company;
                    TimeTableServices.Instance.SaveTimeTable(timetable);
                }
                #endregion



            }
            return Json(new {success=true},JsonRequestBehavior.AllowGet);
        }




    }
}