using Microsoft.Ajax.Utilities;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using System;
using System.Collections.Generic;
using System.Data.Entity.Core.Metadata.Edm;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Net.Mail;
using System.Net;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.Services.Description;
using System.Windows.Media.Animation;
using System.Xml.Linq;
using TheBookingPlatform.Entities;
using TheBookingPlatform.Services;
using TheBookingPlatform.ViewModels;
using System.Configuration;
using System.EnterpriseServices.CompensatingResourceManager;
using OfficeOpenXml;
using System.Data;
using Newtonsoft.Json;
using RestSharp;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using System.Net.Http;
using System.IO;
using File = TheBookingPlatform.Entities.File;
using Service = TheBookingPlatform.Entities.Service;
using System.Data.Entity.Migrations.History;
using Microsoft.Office.Interop.Word;
using Stripe;
using System.Security.Cryptography;


namespace TheBookingPlatform.Controllers
{
    public class AppointmentController : Controller
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

        public AppointmentController()
        {
        }

        public AppointmentController(AMUserManager userManager, AMSignInManager signInManager)
        {
            UserManager = userManager;
            SignInManager = signInManager;
        }

        #endregion
        // GET: Appointment

        public JsonResult SendAppointmentReminder(int ID)
        {
            var apppointment = AppointmentServices.Instance.GetAppointment(ID);
            apppointment.Reminder = true;
            AppointmentServices.Instance.UpdateAppointment(apppointment);
            var customer = CustomerServices.Instance.GetCustomer(apppointment.CustomerID);
            var employee = EmployeeServices.Instance.GetEmployee(apppointment.EmployeeID);
            var company = CompanyServices.Instance.GetCompany().Where(X => X.Business == apppointment.Business).FirstOrDefault();
            if (customer != null)
            {
                string ConcatenatedServices = "";
                foreach (var item in apppointment.Service.Split(',').ToList())
                {
                    var Service = ServiceServices.Instance.GetService(int.Parse(item));
                    if (Service != null)
                    {
                        if (ConcatenatedServices == "")
                        {
                            ConcatenatedServices = String.Join(",", Service.Name);
                        }
                        else
                        {
                            ConcatenatedServices = String.Join(",", ConcatenatedServices, Service.Name);

                        }
                    }
                }

                var emailDetails = EmailTemplateServices.Instance.GetEmailTemplateWRTBusiness(apppointment.Business, "Appointment Reminder");
                if (emailDetails != null && emailDetails.IsActive == true)
                {
                    string emailBody = "<html><body>";
                    emailBody += "<h2 style='font-family: Arial, sans-serif;'>Appointment Reminder</h2>";
                    emailBody += emailDetails.TemplateCode;
                    emailBody = emailBody.Replace("{{Customer_first_name}}", customer.FirstName);
                    emailBody = emailBody.Replace("{{Customer_last_name}}", customer.LastName);
                    emailBody = emailBody.Replace("{{Customer_initial}}", customer.Gender == "Male" ? "Mr." : "Ms.");
                    emailBody = emailBody.Replace("{{date}}", apppointment.Date.ToString("yyyy-MM-dd"));
                    emailBody = emailBody.Replace("{{time}}", apppointment.Time.ToString("H:mm:ss"));
                    emailBody = emailBody.Replace("{{company_name}}", company.Business);
                    emailBody = emailBody.Replace("{{company_email}}", company.NotificationEmail);
                    emailBody = emailBody.Replace("{{company_address}}", company.Address);
                    emailBody = emailBody.Replace("{{services}}", ConcatenatedServices);
                    emailBody = emailBody.Replace("{{employee}}", employee.Name);
                    emailBody = emailBody.Replace("{{employee_specialization}}", employee.Specialization);
                    // Assuming 'serviceName' is the parameter value you want to send
                    string bookingLink = string.Format("https://app.yourbookingplatform.com/{0}/Booking/Welcome", company.Business);
                    emailBody = emailBody.Replace("{{company_address}}", company.Address);
                    emailBody = emailBody.Replace("{{company_logo}}", $"<img class='text-center' style='height:50px;width:auto;' src='{"http://app.yourbookingplatform.com" + company.Logo}'>");
                    emailBody = emailBody.Replace("{{company_phone}}", company.PhoneNumber);

                    emailBody += "</body></html>";
                    if (IsValidEmail(customer.Email))
                    {
                        SendEmail(customer.Email, "Appointment Reminder", emailBody, company);
                    }
                }
            }
            return Json(new { success = true }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult GetEndTime(DateTime StartTime, string Duration)
        {
            if (Duration != null)
            {
                var duration = Duration.Replace("mins", "").Replace("Mins", "");
                var endtime = StartTime.AddMinutes(int.Parse(duration));

                return Json(new { success = true, EndTime = endtime.ToString("HH:mm") }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(new { success = false }, JsonRequestBehavior.AllowGet);

            }
        }

        static bool IsDateInRange(DateTime dateToCheck, DateTime startDate)
        {
            // Extracting only the date part without the time part
            DateTime dateOnlyToCheck = dateToCheck.Date;
            DateTime startDateOnly = startDate.Date;

            if (dateOnlyToCheck > startDateOnly)
            {
                return true; // If it's greater, it's definitely not in range
            }
            else
            {
                return false;
            }

            // Checking if the date falls within the range

            // Checking if the date falls within the range
        }

        [HttpPost]
        public async Task<JsonResult> UpdateIntervalOnUser(int interval)
        {
            JsonResult json = new JsonResult();
            IdentityResult result = null;
            var LoggedInUser = UserManager.FindById(User.Identity.GetUserId());
            LoggedInUser.IntervalCalendar = interval;
            result = await UserManager.UpdateAsync(LoggedInUser);
            json.Data = new { success = result.Succeeded, Message = string.Join(", ", result.Errors) };

            return json;
        }

        [HttpGet]
        public JsonResult CheckEmployeeService(string Services, int EmployeeID)
        {
            var employee = EmployeeServices.Instance.GetEmployee(EmployeeID);
            if (Services != "")
            {
                var finalIDsInt = Services.Split(',').Select(int.Parse).ToList();
                var company = CompanyServices.Instance.GetCompany().Where(x => x.Business == employee.Business).FirstOrDefault();
                if (EmployeeID != 0)
                {
                    var employeeServices = EmployeeServiceServices.Instance.GetEmployeeServiceWRTBusiness(employee.Business, EmployeeID);

                    var employeerequests = EmployeeRequestServices.Instance.GetEmployeeRequestByBusiness(company.ID);
                    foreach (var item in employeerequests)
                    {
                        if (item.Accepted)
                        {
                            var employeeSefvices = EmployeeServiceServices.Instance.GetEmployeeServiceWRTBusiness(item.Business, EmployeeID);
                            employeeServices.AddRange(employeeSefvices);
                        }
                    }
                    var employeeIDs = employeeServices
                    .Where(es => finalIDsInt.Contains(es.ServiceID))
                    .GroupBy(es => es.EmployeeID)
                    .Where(grp => grp.Select(g => g.ServiceID).Distinct().Count() == finalIDsInt.Count)
                    .Select(grp => grp.Key)
                    .ToList();

                    if (employeeIDs.Contains(EmployeeID))
                    {
                        return Json(new { success = true }, JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        return Json(new { success = false, Message = "Beautician does not provide one or more selected services" }, JsonRequestBehavior.AllowGet);

                    }
                }
                else
                {
                    return Json(new { success = false }, JsonRequestBehavior.AllowGet);

                }
            }
            else
            {
                return Json(new { success = true }, JsonRequestBehavior.AllowGet);

            }


        }

        [HttpGet]
        public ActionResult WaitingListAppointment(int WaitingListID)
        {
            AppointmentActionViewModel model = new AppointmentActionViewModel();
            var ServicesList = new List<ServiceModel>();
            var LoggedInUser = UserManager.FindById(User.Identity.GetUserId());
            if (LoggedInUser == null) { return RedirectToAction("Login", "Account"); }
            var company = CompanyServices.Instance.GetCompany().Where(x => x.Business == LoggedInUser.Company).FirstOrDefault();
            if (LoggedInUser.Role != "Super Admin")
            {
                var categories = ServicesCategoriesServices.Instance.GetServiceCategoriesWRTBusiness(LoggedInUser.Company).OrderBy(x => x.DisplayOrder);
                foreach (var item in categories)
                {
                    var ServicesWRTCategory = ServiceServices.Instance.GetServiceWRTCategory(LoggedInUser.Company, item.Name).Where(x => x.IsActive).OrderBy(x => x.DisplayOrder).ToList();
                    ServicesList.Add(new ServiceModel { ServiceCategory = item, Services = ServicesWRTCategory, Company = company });
                }
                model.Employees = EmployeeServices.Instance.GetEmployeeWRTBusiness(LoggedInUser.Company, true).OrderBy(x => x.DisplayOrder).ToList();
            }
            else
            {
                var categories = ServicesCategoriesServices.Instance.GetServiceCategories().OrderBy(x => x.DisplayOrder).ToList();
                foreach (var item in categories)
                {
                    var ServicesWRTCategory = ServiceServices.Instance.GetServiceWRTCategory(item.Name).Where(x => x.IsActive).OrderBy(x => x.DisplayOrder).ToList();
                    ServicesList.Add(new ServiceModel { ServiceCategory = item, Services = ServicesWRTCategory, Company = company });
                }
                model.Employees = EmployeeServices.Instance.GetEmployeeWRTBusiness(true).OrderBy(x => x.DisplayOrder).ToList();
            }
            model.Company = company;
            model.Services = ServicesList.OrderBy(X => X.ServiceCategory.DisplayOrder).ToList();

            var waitingList = WaitingListServices.Instance.GetWaitingList(WaitingListID);
            model.Service = waitingList.Service;
            var ListOfServiceAlloted = new List<ServiceAppViewModel>();
            var serviceList = new List<ServiceAppViewModel>();
            var AppoinmentViewModel = new List<AppointmentModel>();

            var customer = CustomerServices.Instance.GetCustomer(waitingList.CustomerID);
            if (waitingList.Service != null)
            {
                var ServiceListCommand = waitingList.Service.Split(',').ToList();
                var ServiceDuration = waitingList.ServiceDuration.Split(',').ToList();
                var ServiceDiscount = waitingList.ServiceDiscount.Split(',').ToList();
                if (model.Service != null)
                {
                    for (int i = 0; i < ServiceListCommand.Count && i < ServiceDuration.Count && i < ServiceDiscount.Count; i++)
                    {
                        var service = ServiceServices.Instance.GetService(int.Parse(ServiceListCommand[i]));
                        var serviceViewModel = new ServiceAppViewModel
                        {
                            Service = service.Name,
                            Duration = ServiceDuration[i],
                            Price = service.Price,
                            Discount = float.Parse(ServiceDiscount[i]),
                            Category = service.Category,
                            ID = service.ID,
                        };

                        serviceList.Add(serviceViewModel);
                    }

                }
            }
            model.ServiceAlotted = serviceList;
            model.Date = waitingList.Date;
            model.Time = DateTime.Now;
            model.Color = waitingList.Color;
            model.EmployeeID = waitingList.EmployeeID;
            model.Every = "";
            model.WaitingListID = waitingList.ID;
            model.ServiceDuration = waitingList.ServiceDuration;
            model.ServiceDiscount = waitingList.ServiceDiscount;
            model.Notes = waitingList.Notes;
            model.TotalCost = waitingList.TotalCost;
            model.CustomerID = waitingList.CustomerID;
            if (model.CustomerID != 0)
            {
                model.Customer = CustomerServices.Instance.GetCustomer(model.CustomerID);
            }
            model.BookingDate = waitingList.BookingDate;
            model.Service = waitingList.Service;

            model.Company = company;
            return View("Action", model);
        }

        [HttpPost]
        public JsonResult GetEmployee(int employeeId, string date)
        {
            var IsActive = true;
            var employee = EmployeeServices.Instance.GetEmployee(employeeId);
            var timetable = TimeTableServices.Instance.GetTimeTableByEmployeeID(employeeId);
            int workFrequency = 0;
            var roster = TimeTableRosterServices.Instance.GetTimeTableRosterByEmpID(employeeId);
            DateTime currentDate = string.IsNullOrEmpty(date) ? DateTime.Now : DateTime.Parse(date);
            if (timetable.Count() != 0)
            {
                DateTime janeStartDate = timetable.FirstOrDefault().StartDate; // Jane's start date
                if (timetable.Select(x => x.Day).Contains(currentDate.ToString("dddd")))
                {
                    foreach (var item in timetable)
                    {
                        if (currentDate.ToString("dddd") == item.Day)
                        {
                            if (item.Type == "Weekly")
                            {
                                IsActive = true;
                                break;
                            }
                            else
                            {
                                // Calculate the difference in weeks between rosterStartDate and currentDate
                                TimeSpan difference = currentDate - roster.RosterStartDate;
                                int weeksDifference = (int)(difference.TotalDays / 7);

                                // Check if the weeks difference is odd or even
                                bool isEvenWeek = weeksDifference % 2 == 0;
                                if (isEvenWeek && difference.TotalDays > 0)
                                {
                                    IsActive = true;
                                    break;
                                }
                                else
                                {
                                    IsActive = false;
                                }
                            }
                        }
                    }

                }
                else
                {
                    IsActive = false;
                }

            }
            else
            {
                IsActive = false;
            }



            return Json(new { IsActive = IsActive }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult EmailCustomerForWaitinglist(int WaitingListID)
        {
            var waitingList = WaitingListServices.Instance.GetWaitingList(WaitingListID);
            waitingList.Reminder = true;
            WaitingListServices.Instance.UpdateWaitingList(waitingList);
            var customer = CustomerServices.Instance.GetCustomer(waitingList.CustomerID);
            var company = CompanyServices.Instance.GetCompany().Where(x => x.Business == waitingList.Business).FirstOrDefault();

            var emailDetails = EmailTemplateServices.Instance.GetEmailTemplateWRTBusiness(company.Business, "Waiting List");
            if (emailDetails != null && emailDetails.IsActive == true)
            {
                string emailBody = "<html><body>";
                emailBody += "<h2 style='font-family: Arial, sans-serif;'>Waiting List Notification</h2>";
                emailBody += emailDetails.TemplateCode;
                emailBody = emailBody.Replace("{{Customer_first_name}}", customer.FirstName);
                emailBody = emailBody.Replace("{{Customer_last_name}}", customer.LastName);
                emailBody = emailBody.Replace("{{Customer_initial}}", customer.Gender == "Male" ? "Mr." : "Ms.");
                emailBody = emailBody.Replace("{{date}}", waitingList.Date.ToString("yyyy-MM-dd"));
                emailBody = emailBody.Replace("{{time}}", waitingList.Time.ToString("H:mm:ss"));
                emailBody = emailBody.Replace("{{company_name}}", company.Business);
                emailBody = emailBody.Replace("{{company_email}}", company.NotificationEmail);
                // Assuming 'serviceName' is the parameter value you want to send
                string bookingLink = string.Format("https://app.yourbookingplatform.com/{0}/Booking/Welcome", company.Business);
                emailBody = emailBody.Replace("{{booking_button}}", $"<a href='{bookingLink}' class='btn btn-primary'>BOOK NOW</a>");
                emailBody = emailBody.Replace("{{company_address}}", company.Address);
                emailBody = emailBody.Replace("{{company_logo}}", $"<img class='text-center' style='height:50px;width:auto;' src='{"http://app.yourbookingplatform.com" + company.Logo}'>");
                emailBody = emailBody.Replace("{{company_phone}}", company.PhoneNumber);

                emailBody += "</body></html>";
                if (IsValidEmail(customer.Email))
                {
                    SendEmail(customer.Email, "Waiting List", emailBody, company);
                }
            }
            return Json(new { success = true }, JsonRequestBehavior.AllowGet);

        }

        bool IsValidEmail(string email)
        {
            var trimmedEmail = email.Trim();

            if (trimmedEmail.EndsWith("."))
            {
                return false; // suggested by @TK-421
            }
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == trimmedEmail;
            }
            catch
            {
                return false;
            }
        }

        [HttpPost]
        public JsonResult DeleteWaitingList(int WaitingListID)
        {
            WaitingListServices.Instance.DeleteWaitingList(WaitingListID);
            return Json(new { success = true }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult Invoice(int ID)
        {
            InvoiceViewModel model = new InvoiceViewModel();
            var appointment = AppointmentServices.Instance.GetAppointment(ID);
            //model.User = UserManager.FindById(User.Identity.GetUserId());
            model.Company = CompanyServices.Instance.GetCompany().Where(x => x.Business == appointment.Business).FirstOrDefault();
            var customer = CustomerServices.Instance.GetCustomer(appointment.CustomerID);
            var serviceList = new List<ServiceAppInvoiceViewModel>();
            var ServiceListCommand = appointment.Service.Split(',').ToList();
            var ServiceDuration = appointment.ServiceDuration.Split(',').ToList();
            var ServiceDiscount = appointment.ServiceDiscount.Split(',').ToList();
            float VatAmount = 0;
            if (appointment.Service != null)
            {
                for (int i = 0; i < ServiceListCommand.Count && i < ServiceDuration.Count && i < ServiceDiscount.Count; i++)
                {
                    var service = ServiceServices.Instance.GetService(int.Parse(ServiceListCommand[i]));

                    var serviceViewModel = new ServiceAppInvoiceViewModel
                    {

                        Service = service.Name,
                        Duration = ServiceDuration[i],
                        Price = service.Price,
                        Discount = float.Parse(ServiceDiscount[i]),
                        ID = service.ID,

                    };

                    if (service.VAT == null || service.VAT == "No VAT")
                    {
                        VatAmount += 0;
                    }
                    else
                    {

                        var vatPercentage = float.Parse(service.VAT.Replace("%", "").Trim()) / 100;
                        VatAmount += service.Price * vatPercentage;

                    }
                    serviceList.Add(serviceViewModel);
                }

            }
            model.VatAmount = VatAmount;
            model.Services = serviceList;
            model.Customer = customer;
            model.Appointment = appointment;

            model.Invoice = InvoiceServices.Instance.GetInvoice().Where(x => x.AppointmentID == model.Appointment.ID).FirstOrDefault();
            return View(model);
        }

        //[HttpPost]
        //public JsonResult GetWeeksBetween(DateTime date)
        //{
        //    var LoggedInUser = UserManager.FindById(User.Identity.GetUserId());
        //    if (LoggedInUser.Role != "Super Admin")
        //    {
        //        var employees = EmployeeServices.Instance.GetEmployeeWRTBusiness(LoggedInUser.Company, true);
        //        var List = new List<EmployeeTimeTableModel>();
        //        foreach (var item in employees)
        //        {
        //            var timetables = TimeTableServices.Instance.GetTimeTableByEmployeeID(item.ID);
        //            int WeeksBetween = 0;
        //            if (timetables.Count() > 0)
        //            {
        //                WeeksBetween = (int)Math.Ceiling((date - timetables.FirstOrDefault().StartDate).TotalDays / 7);

        //                List.Add(new EmployeeTimeTableModel { Employee = item, TimeTables = timetables, WeekBetween = WeeksBetween });
        //            }
        //            else
        //            {
        //                List.Add(new EmployeeTimeTableModel { Employee = item, TimeTables = timetables, WeekBetween = 0 });

        //            }
        //        }
        //        return Json(new { resources = List });

        //    }
        //    else
        //    {
        //        var employees = EmployeeServices.Instance.GetEmployeeWRTBusiness(true);
        //        var List = new List<EmployeeTimeTableModel>();
        //        foreach (var item in employees)
        //        {
        //            var timetables = TimeTableServices.Instance.GetTimeTableByEmployeeID(item.ID);
        //            int WeeksBetween = 0;
        //            if (timetables.Count() > 0)
        //            {
        //                WeeksBetween = (int)Math.Ceiling((date - timetables.FirstOrDefault().StartDate).TotalDays / 7);

        //                List.Add(new EmployeeTimeTableModel { Employee = item, TimeTables = timetables, WeekBetween = WeeksBetween });
        //            }
        //            else
        //            {
        //                List.Add(new EmployeeTimeTableModel { Employee = item, TimeTables = timetables, WeekBetween = 0 });

        //            }
        //        }
        //        return Json(new { resources = List });

        //    }
        //}


        public string GetNextDayStatus(DateTime CurrentWeekStart, DateTime rosterStartDate, string targetDayString)
        {
            // Calculate the difference in days between the roster start date and today
            if (!Enum.TryParse(targetDayString, true, out DayOfWeek targetDay))
            {
                return "Invalid day of the week specified.";
            }
            var FinalToBeCheckedDate = CurrentWeekStart;


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

        static bool IsDateInRangeNew(DateTime dateToCheck, DateTime startDate)
        {
            // Extracting only the date part without the time part
            DateTime dateOnlyToCheck = dateToCheck.Date;
            DateTime startDateOnly = startDate.Date;

            if (dateOnlyToCheck >= startDateOnly)
            {
                return true; // If it's greater, it's definitely not in range
            }
            else
            {
                return false;
            }

            // Checking if the date falls within the range

            // Checking if the date falls within the range
        }

        [HttpGet]
        [NoCache]
        public ActionResult Calendar(string date = "")
        {

            AppointmentListingViewModel model = new AppointmentListingViewModel();
            var AppointmentModel = new List<AppointmentModel>();
            var WaitingListModel = new List<WaitingListModel>();
            var Allshifts = new List<ShiftOfEmployeeModel>();

            if (date == "")
            {
                model.SelectedDate = DateTime.Now;
                model.GoToDate = DateTime.Now.ToString("yyyy-MM-dd");

            }
            else
            {
                DateTime goToDate = DateTime.Parse(date, new CultureInfo("en-US"));
                model.SelectedDate = goToDate;
                model.GoToDate = goToDate.ToString("yyyy-MM-dd");
            }

            var LoggedInUser = UserManager.FindById(User.Identity.GetUserId());

            if (LoggedInUser == null) { return RedirectToAction("Login", "Account"); }
            model.LoggedInUser = LoggedInUser;
            model.Company = CompanyServices.Instance.GetCompany().Where(x => x.Business == LoggedInUser.Company).FirstOrDefault();
            if (LoggedInUser.Role != "Super Admin")
            {
                model.Holidays = HolidayServices.Instance.GetHolidayWRTBusiness(LoggedInUser.Company, "");
                if (model.Holidays != null)
                {
                    if (model.Holidays.Select(X => X.Date).Contains(model.SelectedDate))
                    {
                        model.TodayOff = true;
                    }
                    else
                    {
                        model.TodayOff = false;
                    }
                }
                else
                {
                    model.TodayOff = false;
                }
                var userAssignedemployees = CalendarManageServices.Instance.GetCalendarManage(LoggedInUser.Company, LoggedInUser.Id);
                var employees = new List<Employee>();
                var List = new List<EmployeeTimeTableModel>();
                if (userAssignedemployees != null)
                {
                    var listofTimeTable = new List<TimeTableModel>();
                    employees = EmployeeServices.Instance.GetEmployeeWRTBusiness(true, userAssignedemployees.ManageOf.Split(',').Select(x => int.Parse(x)).ToList()).OrderBy(x => x.DisplayOrder).ToList();
                    foreach (var emp in employees)
                    {
                        var shifts = new List<ShiftModel>();
                        var pricechange = EmployeePriceChangeServices.Instance
                            .GetEmployeePriceChangeWRTBusiness(emp.ID, LoggedInUser.Company)
                            .Where(x => x.StartDate.Date <= model.SelectedDate.Date && x.EndDate.Date >= model.SelectedDate.Date).FirstOrDefault();
                        var shiftslist = ShiftServices.Instance.GetShiftWRTBusiness(LoggedInUser.Company, emp.ID);
                        foreach (var item in shiftslist)
                        {
                            var roster = TimeTableRosterServices.Instance.GetTimeTableRosterByEmpID(item.EmployeeID);
                            var recurringShifts = RecurringShiftServices.Instance.GetRecurringShiftWRTBusiness(LoggedInUser.Company, item.ID);
                            if (recurringShifts != null)
                            {
                                if (recurringShifts.RecurEnd == "Custom Date" && recurringShifts.RecurEndDate != "")
                                {
                                    var RecurrEndDate = DateTime.Parse(recurringShifts.RecurEndDate);

                                    if (recurringShifts.Frequency == "Bi-Weekly")
                                    {
                                        if (roster != null)
                                        {
                                            if (GetNextDayStatus(model.SelectedDate, item.Date, item.Day.ToString()) == "YES")
                                            {
                                                var exceptionShift = ExceptionShiftServices.Instance.GetExceptionShiftWRTBusiness(LoggedInUser.Company, item.ID);
                                                shifts.Add(new ShiftModel { Shift = item, RecurShift = recurringShifts, ExceptionShift = exceptionShift });
                                            }
                                        }

                                    }
                                    else
                                    {
                                        var exceptionShift = ExceptionShiftServices.Instance.GetExceptionShiftWRTBusiness(LoggedInUser.Company, item.ID);
                                        shifts.Add(new ShiftModel { Shift = item, RecurShift = recurringShifts, ExceptionShift = exceptionShift });
                                    }

                                }
                                else
                                {
                                    if (recurringShifts.Frequency == "Bi-Weekly")
                                    {
                                        if (roster != null)
                                        {
                                            if (GetNextDayStatus(model.SelectedDate, item.Date, item.Day.ToString()) == "YES")
                                            {
                                                var exceptionShift = ExceptionShiftServices.Instance.GetExceptionShiftWRTBusiness(LoggedInUser.Company, item.ID);
                                                shifts.Add(new ShiftModel { Shift = item, RecurShift = recurringShifts, ExceptionShift = exceptionShift });
                                            }
                                        }

                                    }
                                    else
                                    {
                                        var exceptionShift = ExceptionShiftServices.Instance.GetExceptionShiftWRTBusiness(LoggedInUser.Company, item.ID);
                                        shifts.Add(new ShiftModel { Shift = item, RecurShift = recurringShifts, ExceptionShift = exceptionShift });
                                    }
                                }
                            }
                            else
                            {
                                var exceptionShiftByShiftID = ExceptionShiftServices.Instance.GetExceptionShiftWRTBusinessAndShift(LoggedInUser.Company, item.ID);
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
                        Allshifts.Add(new ShiftOfEmployeeModel { Employee = emp, Shifts = shifts, PriceChange = pricechange, DisplayOrder = emp.DisplayOrder });
                    }


                }

                if (Allshifts.Count() != 0)
                {
                    model.Employees = Allshifts.OrderBy(x => x.Employee.DisplayOrder).ToList();
                }
            }
            if (date != "")
            {
                DateTime goToDate = DateTime.Parse(date, new CultureInfo("en-US"));

                DateTime selectedDate = goToDate;
                // Get the day of the week (0 = Sunday, 1 = Monday, ..., 6 = Saturday)
                DayOfWeek dayOfWeek = selectedDate.DayOfWeek;
                // Get the day name
                string dayName = dayOfWeek.ToString();
                var openingHours = OpeningHourServices.Instance.GetOpeningHourWRTBusiness(LoggedInUser.Company, dayName);
                model.TodayOpeningHours = openingHours.Time;
            }
            else
            {
                DateTime selectedDate = DateTime.Now;

                // Get the day of the week (0 = Sunday, 1 = Monday, ..., 6 = Saturday)
                DayOfWeek dayOfWeek = selectedDate.DayOfWeek;

                // Get the day name
                string dayName = dayOfWeek.ToString();
                var openingHours = OpeningHourServices.Instance.GetOpeningHourWRTBusiness(LoggedInUser.Company, dayName);
                model.TodayOpeningHours = openingHours.Time;
            }

            var employee = EmployeeServices.Instance.GetEmployeeWithLinkedUserID(LoggedInUser.Id);
            if (employee != null)
            {
                DateTime gotoDate = DateTime.Parse(model.GoToDate, new CultureInfo("en-US"));
                DateTime now = DateTime.Now;

                switch (employee.LimitCalendarHistory)
                {
                    case "Do not limit":
                        return View(model);

                    case "Do not show previous days":
                        if (gotoDate < now)
                        {
                            model.SelectedDate = DateTime.Now;
                            model.GoToDate = DateTime.Now.ToString("yyyy-MM-dd");

                        }
                        break;

                    case "1 day before":
                        if (gotoDate.Date < now.AddDays(-1).Date)
                        {
                            model.SelectedDate = DateTime.Now;
                            model.GoToDate = DateTime.Now.ToString("yyyy-MM-dd");
                        }
                        break;

                    case "3 days before":
                        if (gotoDate.Date < now.AddDays(-3).Date)
                        {
                            model.SelectedDate = DateTime.Now;
                            model.GoToDate = DateTime.Now.ToString("yyyy-MM-dd");
                        }
                        break;

                    case "7 days before":
                        if (gotoDate.Date < now.AddDays(-7).Date)
                        {
                            model.SelectedDate = DateTime.Now;
                            model.GoToDate = DateTime.Now.ToString("yyyy-MM-dd");
                        }
                        break;

                    case "1 month before":
                        if (gotoDate.Date < now.AddMonths(-1).Date)
                        {
                            model.SelectedDate = DateTime.Now;
                            model.GoToDate = DateTime.Now.ToString("yyyy-MM-dd");
                        }
                        break;

                    case "3 months before":
                        if (gotoDate.Date < now.AddMonths(-3).Date)
                        {
                            model.SelectedDate = DateTime.Now;
                            model.GoToDate = DateTime.Now.ToString("yyyy-MM-dd");
                        }
                        break;

                    case "6 months before":
                        if (gotoDate.Date < now.AddMonths(-6).Date)
                        {
                            model.SelectedDate = DateTime.Now;
                            model.GoToDate = DateTime.Now.ToString("yyyy-MM-dd");
                        }
                        break;
                }
            }

            return View(model);

        }

        #region oldNotOptimizedCode

        [HttpGet]
        public async Task<JsonResult> GetTheDateEvents(DateTime startDate)
        {
            var LoggedInUser = await UserManager.FindByIdAsync(User.Identity.GetUserId());
            var AppointmentModel = new List<AppointmentModel>();
            var WaitingListModel = new List<WaitingListModel>();
            var remindershouldbeSent = false;

            if (startDate.Date >= DateTime.Now.Date)
            {
                remindershouldbeSent = true;
            }
            if (LoggedInUser.Role != "Super Admin")
            {
                var selectedWaitingLists = await WaitingListServices.Instance.GetWaitingListAsync(LoggedInUser.Company, startDate.Day, startDate.Month, startDate.Year, "Created");


                var company = CompanyServices.Instance.GetCompanyByName(LoggedInUser.Company);
                var employeerequests = EmployeeRequestServices.Instance.GetEmployeeRequestByBusiness(company.ID);
                foreach (var item in employeerequests)
                {
                    if (item.Accepted)
                    {
                        var employeeCompany = EmployeeServices.Instance.GetEmployee(item.EmployeeID);
                        selectedWaitingLists.AddRange(await WaitingListServices.Instance.GetWaitingListAsync(employeeCompany.Business, startDate.Day, startDate.Month, startDate.Year, "Created", item.EmployeeID));
                    }
                }

                var groupedData = selectedWaitingLists.SelectMany(waitingList =>
                {
                    var employeeIds = waitingList.EmployeeIDs?.Split(',') ?? new string[] { waitingList.EmployeeID.ToString() };
                    return employeeIds.Select(employeeId => new WaitingListModel
                    {
                        Date = waitingList.Date,
                        EmployeeID = int.Parse(employeeId),
                        Count = 1
                    });
                })
                .GroupBy(newmode => new { newmode.Date, newmode.EmployeeID })
                .Select(group => new WaitingListModel
                {
                    Date = group.Key.Date,
                    EmployeeID = group.Key.EmployeeID,
                    Count = group.Count()
                })
                .ToList();
                //Assign the result to your model
                var WaitingLists = groupedData;



                var appointments = await AppointmentServices.Instance.GetAppointmentBookingWRTBusinessAsync(LoggedInUser.Company, startDate.Day, startDate.Month, startDate.Year, false, false);
                foreach (var item in employeerequests)
                {
                    if (item.Accepted)
                    {
                        var employeeCompany = EmployeeServices.Instance.GetEmployee(item.EmployeeID);
                        if (employeeCompany.Business != LoggedInUser.Company)
                        {
                            var gotAppointments = await AppointmentServices.Instance.GetAppointmentBookingWRTBusinessAsync(employeeCompany.Business, startDate.Day, startDate.Month, startDate.Year, false, false, item.EmployeeID);

                            appointments.AddRange(gotAppointments);
                        }
                        else
                        {
                            var gotAppointments = await AppointmentServices.Instance.GetAppointmentBookingWRTBusinessAsync(item.Business, startDate.Day, startDate.Month, startDate.Year, false, false, item.EmployeeID);

                            appointments.AddRange(gotAppointments);
                        }
                    }
                }
                appointments = appointments.Distinct(new AppointmentComparer()).ToList();
                foreach (var item in appointments)
                {

                    if (item.IsPaid == false && item.DepositMethod == "Online" && item.IsCancelled == false && (DateTime.Now - item.BookingDate).TotalMinutes > 15)
                    {
                        item.IsCancelled = true;
                        AppointmentServices.Instance.UpdateAppointmentNew(item);
                        var employee = EmployeeServices.Instance.GetEmployee(item.EmployeeID);
                        var ToBeInputtedIDs = new Dictionary<GoogleCalendarIntegration, string>();
                        //delete previous one
                        var googleKey = GoogleCalendarServices.Instance.GetGoogleCalendarServicesWRTBusiness(item.Business);
                        ToBeInputtedIDs.Add(googleKey, employee.GoogleCalendarID);
                        var employeeRequest = EmployeeRequestServices.Instance.GetEmployeeRequestsWRTBusiness(item.Business);
                        var requestedEmployee = RequestedEmployeeServices.Instance.GetRequestedEmployeeWRTEmployeeID(employee.ID);
                        if (employeeRequest.Any(x => x.EmployeeID == employee.ID))
                        {
                            if (requestedEmployee != null && requestedEmployee.GoogleCalendarID != null && requestedEmployee.GoogleCalendarID != "")
                            {
                                googleKey = GoogleCalendarServices.Instance.GetGoogleCalendarServicesWRTBusiness(employee.Business);
                                ToBeInputtedIDs.Add(googleKey, requestedEmployee.GoogleCalendarID);
                            }
                        }
                        foreach (var gcalId in ToBeInputtedIDs)
                        {
                            if (gcalId.Key != null && !gcalId.Key.Disabled)
                            {
                                try
                                {
                                    var url = new System.Uri("https://www.googleapis.com/calendar/v3/calendars/" + gcalId.Value + "/events/" + item.GoogleCalendarEventID);
                                    var restClient = new RestClient(url);
                                    var request = new RestRequest();

                                    request.AddQueryParameter("key", "AIzaSyASKpY6I08IVKFMw3muX39uMzPc5sBDaSc");
                                    request.AddHeader("Authorization", "Bearer " + gcalId.Key.AccessToken);
                                    request.AddHeader("Accept", "application/json");
                                    var response = restClient.Delete(request);
                                    if (response.StatusCode == System.Net.HttpStatusCode.NoContent)
                                    {
                                        var history = new History();
                                        history.Date = DateTime.Now;
                                        history.AppointmentID = item.ID;
                                        history.Note = "Appointment got deleted from GCalendar";
                                        history.Business = item.Business;
                                        HistoryServices.Instance.SaveHistory(history);
                                    }
                                }
                                catch (Exception ex)
                                {

                                    continue;
                                }



                            }
                        }
                       


                        continue;
                    }
                    if (item.DepositMethod == "Pin" && item.IsPaid == false)
                    {
                        item.IsPaid = true;
                        AppointmentServices.Instance.UpdateAppointmentNew(item);

                    }
                    int TotalDuration = 0;
                    var customer = CustomerServices.Instance.GetCustomer(item.CustomerID);
                    var serviceList = new List<ServiceModelForCustomerProfile>();
                    if (item.Service != null)
                    {
                        var ServiceListCommand = item.Service.Split(',').ToList();
                        var ServiceDuration = item.ServiceDuration.Split(',').ToList();


                        for (int i = 0; i < ServiceListCommand.Count && i < ServiceDuration.Count; i++)
                        {
                            var Service = ServiceServices.Instance.GetService(int.Parse(ServiceListCommand[i]));
                            if (Service != null)
                            {
                                var serviceViewModel = new ServiceModelForCustomerProfile
                                {

                                    Name = Service.Name,
                                    Duration = ServiceDuration[i],
                                    Price = Service.Price,
                                    Category = Service.Category,
                                    ID = Service.ID
                                };
                                serviceList.Add(serviceViewModel);
                                TotalDuration += int.Parse(serviceViewModel.Duration.ToLower().Replace("mins", "").Replace(" ", "").Trim());

                            }

                        }
                    }
                    else
                    {
                        TotalDuration += int.Parse(Math.Round((item.EndTime.TimeOfDay - item.Time.TimeOfDay).TotalMinutes, 0).ToString());

                    }
                    if (customer == null)
                    {


                        AppointmentModel.Add(new AppointmentModel
                        {
                            Date = item.Date,
                            AppointmentEndTime = item.EndTime,
                            Time = item.Time,
                            Color = item.Color,
                            ID = item.ID,
                            Notes = item.Notes,
                            IsPaid = item.IsPaid,
                            IsCancelled = item.IsCancelled,
                            EmployeeID = item.EmployeeID,
                            CustomerFirstName = "Walk In",
                            IsRepeat = item.IsRepeat,
                            CustomerLastName = "",
                            Services = serviceList,
                            FromGCAL = item.FromGCAL,
                            ReminderSent = remindershouldbeSent && item.Reminder,
                            TotalDuration = TotalDuration,
                            Buffers = BufferServices.Instance.GetBufferWRTBusinessList(item.Business, item.ID)
                        });
                    }
                    else
                    {
                        bool NewCustomer = false;
                        DateTime today = DateTime.Now.Date; // Assuming you want to check appointments before today
                        bool hasPreviousAppointments = AppointmentServices.Instance.HasPreviousAppointments(LoggedInUser.Company, customer.ID, item.ID, today);


                        if (!hasPreviousAppointments)
                        {
                            NewCustomer = true;
                        }
                        AppointmentModel.Add(new AppointmentModel
                        {
                            IsRepeat = item.IsRepeat,
                            AnyEmployeeSelected = item.AnyAvailableEmployeeSelected,
                            NewCustomer = NewCustomer,
                            Date = item.Date,
                            AppointmentEndTime = item.EndTime,
                            Time = item.Time,
                            Color = item.Color,
                            ID = item.ID,
                            Notes = item.Notes,
                            IsPaid = item.IsPaid,
                            IsCancelled = item.IsCancelled,
                            EmployeeID = item.EmployeeID,
                            CustomerFirstName = customer.FirstName,
                            CustomerLastName = customer.LastName,
                            MobileNumber = customer.MobileNumber,
                            Services = serviceList,
                            TotalDuration = TotalDuration,
                            ReminderSent = remindershouldbeSent && item.Reminder,
                            Buffers = BufferServices.Instance.GetBufferWRTBusinessList(item.Business, item.ID)
                        });

                    }
                }
                var AppointmentLists = AppointmentModel;
                return Json(new { success = true, Appointments = AppointmentLists, WaitingLists = WaitingLists }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                var selectedWaitingLists = await WaitingListServices.Instance.GetWaitingListAsync(startDate.Day, startDate.Month, startDate.Year, "Created");

                var groupedData = selectedWaitingLists.SelectMany(waitingList =>
                {
                    var employeeIds = waitingList.EmployeeIDs?.Split(',') ?? new string[] { waitingList.EmployeeID.ToString() };
                    return employeeIds.Select(employeeId => new WaitingListModel
                    {
                        Date = waitingList.Date,
                        EmployeeID = int.Parse(employeeId),
                        Count = 1
                    });
                })
                .GroupBy(newmode => new { newmode.Date, newmode.EmployeeID })
                .Select(group => new WaitingListModel
                {
                    Date = group.Key.Date,
                    EmployeeID = group.Key.EmployeeID,
                    Count = group.Count()
                })
                .ToList();
                //Assign the result to your model
                var WaitingList = groupedData;
                var appointments = AppointmentServices.Instance.GetAppointmentAsync().Result.Where(x => x.Business == LoggedInUser.Company && x.Date.Day == startDate.Day && x.Date.Month == startDate.Month && x.Date.Year == startDate.Year && x.IsCancelled == false && x.DELETED == false).ToList();
                foreach (var item in appointments)
                {

                    int TotalDuration = 0;
                    var customer = CustomerServices.Instance.GetCustomer(item.CustomerID);
                    var serviceList = new List<ServiceModelForCustomerProfile>();
                    if (item.Service != null)
                    {
                        var ServiceListCommand = item.Service.Split(',').ToList();
                        var ServiceDuration = item.ServiceDuration.Split(',').ToList();


                        for (int i = 0; i < ServiceListCommand.Count && i < ServiceDuration.Count; i++)
                        {
                            var Service = ServiceServices.Instance.GetService(int.Parse(ServiceListCommand[i]));
                            if (Service != null)
                            {
                                var serviceViewModel = new ServiceModelForCustomerProfile
                                {

                                    Name = Service.Name,
                                    Duration = ServiceDuration[i],
                                    Price = Service.Price,
                                    Category = Service.Category,
                                    ID = Service.ID
                                };
                                serviceList.Add(serviceViewModel);
                                TotalDuration += int.Parse(serviceViewModel.Duration.ToLower().Replace("mins", "").Replace(" ", "").Trim());

                            }

                        }
                    }
                    else
                    {
                        TotalDuration += int.Parse(Math.Round((item.EndTime.TimeOfDay - item.Time.TimeOfDay).TotalMinutes, 0).ToString());

                    }
                    if (customer == null)
                    {

                        AppointmentModel.Add(new AppointmentModel
                        {
                            Date = item.Date,
                            AppointmentEndTime = item.EndTime,
                            Time = item.Time,
                            Color = item.Color,
                            ID = item.ID,
                            Notes = item.Notes,
                            IsPaid = item.IsPaid,
                            IsCancelled = item.IsCancelled,
                            EmployeeID = item.EmployeeID,
                            Customer = null,
                            Services = serviceList,
                            FromGCAL = item.FromGCAL,
                            TotalDuration = TotalDuration,
                            ReminderSent = remindershouldbeSent && item.Reminder,
                            Buffers = BufferServices.Instance.GetBufferWRTBusinessList(item.Business, item.ID)

                        });
                    }
                    else
                    {

                        AppointmentModel.Add(new AppointmentModel
                        {
                            IsRepeat = item.IsRepeat,
                            Buffers = BufferServices.Instance.GetBufferWRTBusinessList(item.Business, item.ID),
                            Date = item.Date,
                            AppointmentEndTime = item.EndTime,
                            Time = item.Time,
                            Color = item.Color,
                            ID = item.ID,
                            Notes = item.Notes,
                            IsPaid = item.IsPaid,
                            IsCancelled = item.IsCancelled,
                            EmployeeID = item.EmployeeID,
                            CustomerFirstName = customer.FirstName,
                            CustomerLastName = customer.LastName,
                            MobileNumber = customer.MobileNumber,
                            Services = serviceList,
                            TotalDuration = TotalDuration,
                            ReminderSent = remindershouldbeSent && item.Reminder
                        });

                    }
                }
                var AppointmentLists = AppointmentModel;
                return Json(new { success = true, Appointments = AppointmentLists, WaitingLists = WaitingList }, JsonRequestBehavior.AllowGet);
            }
        }

        #endregion




        public class AppointmentComparer : IEqualityComparer<Appointment>
        {
            public bool Equals(Appointment x, Appointment y)
            {
                // Check if IDs are equal
                return x.ID == y.ID;
            }

            public int GetHashCode(Appointment obj)
            {
                // Return the hash code of the ID
                return obj.ID.GetHashCode();
            }
        }

        [NoCache]
        public ActionResult Index(string StartDate = "", string EndDate = "")
        {

            AppointmentListingViewModel model = new AppointmentListingViewModel();
            var AppointmentModel = new List<AppointmentListModel>();
            if (StartDate != "" && EndDate != "")
            {
                model.StartDate = DateTime.Parse(StartDate);
                model.EndDate = DateTime.Parse(EndDate);
            }
            else
            {
                model.StartDate = DateTime.Now;
                model.EndDate = DateTime.Now;
            }
            var LoggedInUser = UserManager.FindById(User.Identity.GetUserId()); if (LoggedInUser == null) { return RedirectToAction("Login", "Account"); }
            model.Company = CompanyServices.Instance.GetCompany().Where(x => x.Business == LoggedInUser.Company).FirstOrDefault();

            if (LoggedInUser.Role != "Super Admin")
            {
                model.OpeningHours = OpeningHourServices.Instance.GetOpeningHoursWRTBusiness(LoggedInUser.Company, "");
                var appointments = AppointmentServices.Instance.GetAllAppointmentWRTBusiness(LoggedInUser.Company, false, model.StartDate, model.EndDate);
                var employeerequests = EmployeeRequestServices.Instance.GetEmployeeRequestByBusiness(model.Company.ID);
                foreach (var item in employeerequests)
                {
                    if (item.Accepted)
                    {
                        var employeeCompany = EmployeeServices.Instance.GetEmployee(item.EmployeeID);
                        appointments.AddRange(AppointmentServices.Instance.GetAppointmentBookingWRTBusiness(employeeCompany.Business, false, false, item.EmployeeID, model.StartDate, model.EndDate));
                    }
                }
                appointments = appointments.Distinct(new AppointmentComparer()).ToList();
                foreach (var item in appointments)
                {
                    var customer = CustomerServices.Instance.GetCustomer(item.CustomerID);
                    if (item.Service != null)
                    {
                        var ServiceListCommand = item.Service.Split(',').ToList();
                        var ServiceDuration = item.ServiceDuration.Split(',').ToList();


                        var serviceList = new List<ServiceModelForCustomerProfile>();
                        for (int i = 0; i < ServiceListCommand.Count && i < ServiceDuration.Count; i++)
                        {
                            var serivce = ServiceServices.Instance.GetService(int.Parse(ServiceListCommand[i]));
                            if (serivce != null)
                            {
                                var serviceViewModel = new ServiceModelForCustomerProfile
                                {
                                    Name = serivce.Name,
                                    Duration = ServiceDuration[i],
                                    ID = serivce.ID
                                };

                                serviceList.Add(serviceViewModel);
                            }
                        }
                        if (customer == null)
                        {

                            var employee = EmployeeServices.Instance.GetEmployee(item.EmployeeID);
                            AppointmentModel.Add(new AppointmentListModel { Business = employee.Business, Color = item.Color, StartDate = item.Date, ID = item.ID, CustomerLastName = " ", EndTime = item.EndTime, StartTime = item.Time, CustomerFirstName = "Walk In", Services = serviceList });
                        }
                        else
                        {
                            var employee = EmployeeServices.Instance.GetEmployee(item.EmployeeID);
                            if (employee != null)
                            {
                                AppointmentModel.Add(new AppointmentListModel { Business = employee.Business, Color = item.Color, StartDate = item.Date, ID = item.ID, CustomerLastName = customer.LastName, EndTime = item.EndTime, StartTime = item.Time, CustomerFirstName = customer.FirstName, Services = serviceList });
                            }

                        }
                    }
                    else
                    {
                        if (customer == null)
                        {

                            var employee = EmployeeServices.Instance.GetEmployee(item.EmployeeID);
                            AppointmentModel.Add(new AppointmentListModel { Business = employee.Business, Color = item.Color, StartDate = item.Date, ID = item.ID, CustomerLastName = " ", EndTime = item.EndTime, StartTime = item.Time, CustomerFirstName = "Walk In" });
                        }
                        else
                        {
                            var employee = EmployeeServices.Instance.GetEmployee(item.EmployeeID);

                            AppointmentModel.Add(new AppointmentListModel { Business = employee.Business, Color = item.Color, StartDate = item.Date, ID = item.ID, CustomerLastName = customer.LastName, EndTime = item.EndTime, StartTime = item.Time, CustomerFirstName = customer.FirstName });

                        }
                    }
                }
                model.Appointments = AppointmentModel;
            }
            else
            {
                model.OpeningHours = OpeningHourServices.Instance.GetOpeningHour();

                var appointments = AppointmentServices.Instance.GetAppointment().Where(x => x.DELETED == false).ToList();
                foreach (var item in appointments)
                {
                    var customer = CustomerServices.Instance.GetCustomer(item.CustomerID);
                    var employee = EmployeeServices.Instance.GetEmployee(item.EmployeeID);
                    var serviceList = new List<ServiceModelForCustomerProfile>();
                    if (item.Service != null)
                    {
                        var ServiceListCommand = item.Service.Split(',').ToList();
                        var ServiceDuration = item.ServiceDuration.Split(',').ToList();


                        for (int i = 0; i < ServiceListCommand.Count && i < ServiceDuration.Count; i++)
                        {
                            var Service = ServiceServices.Instance.GetService(int.Parse(ServiceListCommand[i]));
                            if (Service != null)
                            {
                                var serviceViewModel = new ServiceModelForCustomerProfile
                                {

                                    Name = Service.Name,
                                    ID = Service.ID,
                                    Duration = ServiceDuration[i]
                                };
                                serviceList.Add(serviceViewModel);
                            }

                        }
                    }
                    if (customer == null)
                    {

                        AppointmentModel.Add(new AppointmentListModel { Color = item.Color, StartDate = item.Date, ID = item.ID, CustomerLastName = " ", EndTime = item.EndTime, StartTime = item.Time, CustomerFirstName = "Walk In", Services = serviceList });
                    }
                    else
                    {

                        AppointmentModel.Add(new AppointmentListModel { Color = item.Color, StartDate = item.Date, ID = item.ID, CustomerLastName = customer.LastName, EndTime = item.EndTime, StartTime = item.Time, CustomerFirstName = customer.FirstName, Services = serviceList });

                    }
                }
                model.Appointments = AppointmentModel;
            }
            return View(model);
        }

        [HttpGet]
        public ActionResult ShowCalendar(int employeeID = 0)
        {
            // Your existing code to retrieve data

            AppointmentListingViewModel model = new AppointmentListingViewModel();
            var AppointmentModel = new List<AppointmentModel>();
            var WaitingListModel = new List<WaitingListModel>();
            var Allshifts = new List<ShiftOfEmployeeModel>();
            var SelectedDate = model.SelectedDate;
            var LoggedInUser = UserManager.FindById(User.Identity.GetUserId()); if (LoggedInUser == null) { return RedirectToAction("Login", "Account"); }
            model.LoggedInUser = LoggedInUser;
            model.Company = CompanyServices.Instance.GetCompany().Where(x => x.Business == LoggedInUser.Company).FirstOrDefault();

            if (LoggedInUser.Role != "Super Admin")
            {
                var userAssignedemployees = CalendarManageServices.Instance.GetCalendarManage(LoggedInUser.Company, LoggedInUser.Id);
                var employees = new List<Employee>();
                var List = new List<EmployeeTimeTableModel>();

                if (Allshifts.Count() != 0)
                {
                    model.Employees = Allshifts.OrderBy(x => x.Employee.DisplayOrder).ToList();
                }

            }
            model.EmployeeID = employeeID;

            // Return a partial view with the updated data
            return PartialView("_CalendarPartial", model);
        }

        [HttpPost]
        public JsonResult ShowServices()
        {
            var ServicesList = new List<ServiceModel>();
            var LoggedInUser = UserManager.FindById(User.Identity.GetUserId());

            var Company = CompanyServices.Instance.GetCompany().Where(x => x.Business == LoggedInUser.Company).FirstOrDefault();

            if (LoggedInUser.Role != "Super Admin")
            {
                var categories = ServicesCategoriesServices.Instance.GetServiceCategories().Where(x => x.Business == LoggedInUser.Company).OrderBy(x => x.DisplayOrder).ToList();
                foreach (var item in categories)
                {
                    var ServicesWRTCategory = ServiceServices.Instance.GetService().Where(x => x.Category == item.Name && x.IsActive && x.Business == LoggedInUser.Company).OrderBy(x => x.DisplayOrder).ToList();
                    ServicesList.Add(new ServiceModel { ServiceCategory = item, Services = ServicesWRTCategory, Company = Company });
                }
            }
            else
            {
                var categories = ServicesCategoriesServices.Instance.GetServiceCategories().OrderBy(x => x.DisplayOrder).ToList();
                foreach (var item in categories)
                {
                    var ServicesWRTCategory = ServiceServices.Instance.GetService().Where(x => x.Category == item.Name && x.IsActive).OrderBy(x => x.DisplayOrder).ToList();
                    ServicesList.Add(new ServiceModel { ServiceCategory = item, Services = ServicesWRTCategory, Company = Company });
                }
            }
            return Json(new { success = true }, JsonRequestBehavior.AllowGet);
        }

        #region TryOfGoogleCalendarTwoWAy
        //public class GoogleCalendarNotification
        //{
        //    public string ResourceId { get; set; } // This is the event ID
        //    public string ResourceUri { get; set; }
        //    public string ResourceState { get; set; }
        //}

        //public class GoogleCalendarEvent
        //{
        //    public string Summary { get; set; }
        //    public EventDateTime Start { get; set; }
        //    public EventDateTime End { get; set; }
        //    // Add other properties as needed

        //    public class EventDateTime
        //    {
        //        public string DateTime { get; set; }
        //    }
        //}

        //private async Task FetchEventDetails(string eventId, string googleCalendarID)
        //{
        //    var employee = EmployeeServices.Instance.GetEmployeeWithLinkedGoogleCalendarID(googleCalendarID);
        //    var googleCalendar = GoogleCalendarServices.Instance.GetGoogleCalendarServicesWRTBusiness(employee.Business);
        //    var accessToken = googleCalendar.AccessToken;

        //    using (var client = new HttpClient())
        //    {
        //        client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);

        //        var requestUrl = $"https://www.googleapis.com/calendar/v3/calendars/{googleCalendarID}/events/{eventId}";
        //        var response = await client.GetAsync(requestUrl);
        //        if (response.IsSuccessStatusCode)
        //        {
        //            var eventJson = await response.Content.ReadAsStringAsync();
        //            var eventDetails = JsonConvert.DeserializeObject<GoogleCalendarEvent>(eventJson);

        //            // Log event details in history
        //            var history = new History
        //            {
        //                Note = eventJson,
        //                Business = employee.Business,
        //                Date = DateTime.Now,
        //                EmployeeName = "", // Set the appropriate employee name
        //                Type = "GCAL To Test"
        //            };
        //            HistoryServices.Instance.SaveHistory(history);

        //            // Process the event details as needed
        //            System.Diagnostics.Debug.WriteLine($"Event Summary: {eventDetails.Summary}, Start: {eventDetails.Start.DateTime}");
        //        }
        //        else
        //        {
        //            System.Diagnostics.Debug.WriteLine("Failed to fetch event details.");
        //        }
        //    }
        //}

        //[HttpPost]
        //public async Task<JsonResult> GcalToYBP()
        //{
        //    string requestBody = "";
        //    // Read the request body as a stream
        //    using (var stream = Request.InputStream)
        //    {
        //        using (var reader = new StreamReader(stream))
        //        {
        //            string contentType = Request.Headers.GetValues("Content-Type").FirstOrDefault();

        //            requestBody = reader.ReadToEnd();
        //            if (contentType != null && contentType.Contains("application/json"))
        //            {
        //                // Read and deserialize JSON data
        //                var history = new History
        //                {
        //                    Note = "Json Data",
        //                    Business = requestBody,
        //                    Date = DateTime.Now,
        //                    EmployeeName = "", // Set the appropriate employee name
        //                    Type = "GCAL To Test"
        //                };
        //                HistoryServices.Instance.SaveHistory(history);
        //            }
        //            else if (contentType != null && contentType.Contains("text/plain"))
        //            {
        //                // Read data as plain text
        //                var history = new History
        //                {
        //                    Note = "Plain Data",
        //                    Business = requestBody,
        //                    Date = DateTime.Now,
        //                    EmployeeName = contentType, // Set the appropriate employee name
        //                    Type = "GCAL To Test"
        //                };
        //                HistoryServices.Instance.SaveHistory(history);
        //            }
        //            else
        //            {
        //                // Handle other content types or errors
        //            }


        //        }
        //    }




        //    // Extract the resourceId from the notification and fetch event details
        //    //var eventId = notification.ResourceId;
        //    //await FetchEventDetails("0", GoogleCalendarID);

        //    return Json(new { success = true }, JsonRequestBehavior.AllowGet);
        //}

        public async Task<string> SetUpWatch(string GoogleCalendarID)
        {
            var employee = EmployeeServices.Instance.GetEmployeeWithLinkedGoogleCalendarID(GoogleCalendarID);
            var googleCalendar = GoogleCalendarServices.Instance.GetGoogleCalendarServicesWRTBusiness(employee.Business);

            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", googleCalendar.AccessToken);

                var watchRequestBody = new
                {
                    id = Guid.NewGuid().ToString(), // A unique ID for this channel
                    type = "web_hook",
                    address = $"https://app.yourbookingplatform.com/Booking/TestingGoogleToYBP", // Your webhook endpoint
                    @params = new
                    {
                        ttl = "2147483647" // Max integer value (2^31 - 1 seconds, approx 68 years)
                    }
                };

                var content = new StringContent(JsonConvert.SerializeObject(watchRequestBody), Encoding.UTF8, "application/json");
                var requestUrl = $"https://www.googleapis.com/calendar/v3/calendars/{GoogleCalendarID}/events/watch";
                var response = await client.PostAsync(requestUrl, content);
                var responseBody = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    return "Watch setup successfully";
                }
                else
                {
                    return "Failed to set up watch " + responseBody;
                }
            }
        }

        #endregion



        [HttpGet]
        public JsonResult GetServiceDetails(int ID)
        {
            var service = ServiceServices.Instance.GetService(ID);
            return Json(service, JsonRequestBehavior.AllowGet);
        }


        [HttpGet]
        public JsonResult GetCustomerDetails(int ID)
        {
            var customer = CustomerServices.Instance.GetCustomer(ID);
            return Json(customer, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult GetCustomers()
        {
            var LoggedInUser = UserManager.FindById(User.Identity.GetUserId());

            var customers = new List<CustomerAppointmentModel>();
            if (LoggedInUser.Role != "Super Admin")
            {
                var allCustomers = CustomerServices.Instance.GetCustomersWRTBusiness(LoggedInUser.Company, "");
                foreach (var item in allCustomers)
                {
                    customers.Add(new CustomerAppointmentModel { IsBlocked = item.IsBlocked, Email = item.Email, ID = item.ID, FirstName = item.FirstName, LastName = item.LastName, MobileNumber = item.MobileNumber });
                }
            }
            else
            {
                var allCustomers = CustomerServices.Instance.GetCustomer();
                foreach (var item in allCustomers)
                {
                    customers.Add(new CustomerAppointmentModel { IsBlocked = item.IsBlocked, Email = item.Email, ID = item.ID, FirstName = item.FirstName, LastName = item.LastName, MobileNumber = item.MobileNumber });
                }
            }
            return Json(customers, JsonRequestBehavior.AllowGet);

        }

        public JsonResult GetServices()
        {
            var LoggedInUser = UserManager.FindById(User.Identity.GetUserId());

            var ServicesList = new List<ServiceModel>();
            var company = CompanyServices.Instance.GetCompany().Where(x => x.Business == LoggedInUser.Company).FirstOrDefault();

            if (LoggedInUser.Role != "Super Admin")
            {
                var categories = ServicesCategoriesServices.Instance.GetServiceCategoriesWRTBusinessAndCategory(LoggedInUser.Company, "ABSENSE");
                foreach (var item in categories)
                {
                    var ServicesWRTCategory = ServiceServices.Instance.GetServiceWRTCategory(LoggedInUser.Company, item.Name).Where(x => x.IsActive).OrderBy(x => x.DisplayOrder).ToList();
                    ServicesList.Add(new ServiceModel { ServiceCategory = item, Services = ServicesWRTCategory, Company = company });
                }
            }
            else
            {
                var categories = ServicesCategoriesServices.Instance.GetServiceCategories().Where(x => x.Name != "ABSENSE").OrderBy(x => x.DisplayOrder).ToList();
                foreach (var item in categories)
                {
                    var ServicesWRTCategory = ServiceServices.Instance.GetServiceWRTCategory(item.Name).Where(x => x.IsActive).OrderBy(x => x.DisplayOrder).ToList();
                    ServicesList.Add(new ServiceModel { ServiceCategory = item, Services = ServicesWRTCategory, Company = company });
                }
            }

            return Json(ServicesList, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        [NoCache]
        public ActionResult AppointmentDetailsPartial(int AppointmentID)
        {
            var LoggedInUser = UserManager.FindById(User.Identity.GetUserId());
            AppointmentDetailsViewModel model = new AppointmentDetailsViewModel();
            var appointment = AppointmentServices.Instance.GetAppointment(AppointmentID);
            var customer = CustomerServices.Instance.GetCustomer(appointment.CustomerID);
            var employee = EmployeeServices.Instance.GetEmployee(appointment.EmployeeID);
            var serviceList = new List<ServiceAppViewModel>();
            var ServiceListCommand = appointment.Service.Split(',').ToList();
            var ServiceDuration = appointment.ServiceDuration.Split(',').ToList();
            var ServiceDiscount = appointment.ServiceDiscount.Split(',').ToList();
            model.PriceChange = PriceChangeServices.Instance.GetPriceChange(appointment.OnlinePriceChange);


            model.Company = CompanyServices.Instance.GetCompany().Where(x => x.Business == LoggedInUser.Company).FirstOrDefault();

            if (appointment.Service != null)
            {
                for (int i = 0; i < ServiceListCommand.Count && i < ServiceDuration.Count && i < ServiceDiscount.Count; i++)
                {
                    var service = ServiceServices.Instance.GetService(int.Parse(ServiceListCommand[i]));
                    var serviceViewModel = new ServiceAppViewModel
                    {
                        Service = service.Name,
                        Duration = ServiceDuration[i],
                        Price = service.Price,
                        Discount = float.Parse(ServiceDiscount[i]),
                        Category = service.Category,
                        ID = service.ID,
                    };

                    serviceList.Add(serviceViewModel);
                }

            }
            model.Services = serviceList;
            model.Customer = customer;
            model.Appointment = appointment;
            model.Employee = employee;

            model.Selected = "Appointment Details";




            //Pill Two
            var AppointmentsListOfCustoemrs = new List<AppointmentModel>();
            if (customer != null)
            {
                var appointmentsforCustomer = AppointmentServices.Instance.GetAppointmentBookingWRTBusiness(LoggedInUser.Company, false, appointment.CustomerID);

                foreach (var item in appointmentsforCustomer)
                {

                    var employee2 = EmployeeServices.Instance.GetEmployee(item.EmployeeID);

                    var customer2 = CustomerServices.Instance.GetCustomer(item.CustomerID);
                    var serviceList2 = new List<ServiceModelForCustomerProfile>();
                    if (item.Service != null)
                    {
                        var ServiceListCommand2 = item.Service.Split(',').ToList();
                        var ServiceDuration2 = item.ServiceDuration.Split(',').ToList();


                        for (int i = 0; i < ServiceListCommand2.Count && i < ServiceDuration2.Count; i++)
                        {
                            var Service2 = ServiceServices.Instance.GetService(int.Parse(ServiceListCommand2[i]));
                            if (Service2 != null)
                            {
                                var serviceViewModel2 = new ServiceModelForCustomerProfile()
                                {

                                    Name = Service2.Name,
                                    Duration = ServiceDuration2[i],
                                    Price = Service2.Price,
                                    ID = Service2.ID
                                };
                                serviceList2.Add(serviceViewModel2);
                            }

                        }
                    }
                    if (customer2 == null)
                    {

                        AppointmentsListOfCustoemrs.Add(new AppointmentModel { Date = item.Date, Time = item.Time, Status = item.Status, AppointmentEndTime = item.EndTime, ID = item.ID, EmployeeName = employee2.Name, Customer = null, Services = serviceList2 });
                    }
                    else
                    {

                        AppointmentsListOfCustoemrs.Add(new AppointmentModel { Date = item.Date, Time = item.Time, Status = item.Status, AppointmentEndTime = item.EndTime, ID = item.ID, EmployeeName = employee2?.Name, Customer = customer2, Services = serviceList2 });

                    }
                }
            }
            model.TotalAppointmentsCustomer = AppointmentsListOfCustoemrs;

            if (customer != null)
            {
                //Pill Three
                model.Files = FileServices.Instance.GetFileWRTBusiness(appointment.Business, appointment.CustomerID);
                model.Histories = HistoryServices.Instance.GetHistoriesWRTToAppointmentID(appointment.ID);
                model.Messages = MessageServices.Instance.GetMessageWRTBusiness(appointment.Business, appointment.CustomerID);
                var listOfLoyaltyCardDetails = new List<History>();
                var LCAssigned = LoyaltyCardServices.Instance.GetLoyaltyCardAssignmentByCustomerID(customer.ID);
                var historiesofLoyaltyCard = HistoryServices.Instance.GetHistoriesByCustomer(customer.FirstName + " " + customer.LastName, appointment.Business);
                foreach (var item in historiesofLoyaltyCard)
                {
                    if (item.Type.Contains("LoyaltyCard"))
                    {
                        listOfLoyaltyCardDetails.Add(item);
                    }
                }
                model.LoyaltyCardHistories = listOfLoyaltyCardDetails;
            }

            var ListOfFinalCardAssignments = new List<LoyaltyCardAssignmentModel>();
            if (LoggedInUser.Role == "Super Admin")
            {
                if (customer != null)
                {
                    var LCAs = LoyaltyCardServices.Instance.GetLoyaltyCardAssignments();
                    foreach (var item in LCAs)
                    {
                        var loyaltyCard = LoyaltyCardServices.Instance.GetLoyaltyCard(item.LoyaltyCardID);
                        float CashBack = 0;

                        ListOfFinalCardAssignments.Add(new LoyaltyCardAssignmentModel { LoyaltyCardDays = loyaltyCard.Days, LoyaltyCardName = loyaltyCard.Name, Customer = customer, LoyaltyCardAssignment = item, LoyaltyCardUsage = CashBack });
                    }
                }
            }
            else
            {
                if (customer != null)
                {

                    var LCAs = LoyaltyCardServices.Instance.GetLoyaltyCardAssignmentsWRTbBusinessAndCustomer(LoggedInUser.Company, customer.ID);
                    foreach (var item in LCAs)
                    {
                        var loyaltyCard = LoyaltyCardServices.Instance.GetLoyaltyCard(item.LoyaltyCardID);
                        float CashBack = 0;

                        ListOfFinalCardAssignments.Add(new LoyaltyCardAssignmentModel { LoyaltyCardDays = loyaltyCard.Days, LoyaltyCardName = loyaltyCard.Name, Customer = customer, LoyaltyCardAssignment = item, LoyaltyCardUsage = CashBack });
                    }
                }

            }
            model.LoyaltyCardAssignments = ListOfFinalCardAssignments;
            return PartialView("_DetailsPartial", model);
        }

        [HttpGet]
        [NoCache]
        public ActionResult Checkout(int ID)
        {
            AppointmentDetailsViewModel model = new AppointmentDetailsViewModel();
            var appointment = AppointmentServices.Instance.GetAppointment(ID);
            var customer = CustomerServices.Instance.GetCustomer(appointment.CustomerID);
            var employee = EmployeeServices.Instance.GetEmployee(appointment.EmployeeID);
            var serviceList = new List<ServiceAppViewModel>();
            var ServiceListCommand = appointment.Service.Split(',').ToList();
            var ServiceDuration = appointment.ServiceDuration.Split(',').ToList();
            var ServiceDiscount = appointment.ServiceDiscount.Split(',').ToList();
            var saleonCheckout = new SaleOnCheckout();


            model.Company = CompanyServices.Instance.GetCompany().Where(x => x.Business == appointment.Business).FirstOrDefault();


            if (appointment.Service != null)
            {
                for (int i = 0; i < ServiceListCommand.Count && i < ServiceDuration.Count && i < ServiceDiscount.Count; i++)
                {
                    var service = ServiceServices.Instance.GetService(int.Parse(ServiceListCommand[i]));
                    var serviceViewModel = new ServiceAppViewModel
                    {
                        Service = service.Name,
                        Duration = ServiceDuration[i],
                        Price = service.Price,
                        Category = service.Category,
                        Discount = float.Parse(ServiceDiscount[i]),
                        ID = service.ID,
                    };

                    serviceList.Add(serviceViewModel);
                }

            }
            model.Services = serviceList;
            model.Customer = customer;
            model.PriceChange = PriceChangeServices.Instance.GetPriceChange(appointment.OnlinePriceChange);
            model.EmployeePriceChange = EmployeePriceChangeServices.Instance.GetEmployeePriceChange(appointment.EmployeePriceChange);
            model.Appointment = appointment;
            model.Employee = employee;
            model.Coupon = CouponServices.Instance.GetCoupon(appointment.CouponID);
            var sale = SaleServices.Instance.GetSaleWRTBusiness(appointment.Business, appointment.ID).Where(x => x.Type == "Via Appointment").FirstOrDefault();
            if (sale != null)
            {
                var saleProducts = SaleProductServices.Instance.GetSaleProductWRTBusiness(appointment.Business, sale.ID);
                var listofSaleprodcutSModel = new List<SaleProductModel>();
                foreach (var item in saleProducts)
                {
                    var product = ProductServices.Instance.GetProduct(item.ID);

                    listofSaleprodcutSModel.Add(new SaleProductModel { Product = product, Qty = item.Qty, Total = item.Total });
                }
                saleonCheckout.Sale = sale;
                saleonCheckout.SaleProducts = listofSaleprodcutSModel;
                model.SaleOnCheckOut = saleonCheckout;
            }
            model.Selected = "Checkout";
            return PartialView("_Checkout", model);
        }

        [HttpGet]
        [NoCache]
        public ActionResult AppointmentDetails(int AppointmentID)
        {
            AppointmentDetailsViewModel model = new AppointmentDetailsViewModel();
            var appointment = AppointmentServices.Instance.GetAppointment(AppointmentID);
            var customer = CustomerServices.Instance.GetCustomer(appointment.CustomerID);
            var employee = EmployeeServices.Instance.GetEmployee(appointment.EmployeeID);
            var serviceList = new List<ServiceAppViewModel>();
            var ServiceListCommand = appointment.Service.Split(',').ToList();
            var ServiceDuration = appointment.ServiceDuration.Split(',').ToList();
            var ServiceDiscount = appointment.ServiceDiscount.Split(',').ToList();

            model.Company = CompanyServices.Instance.GetCompany().Where(x => x.Business == appointment.Business).FirstOrDefault();


            if (appointment.Service != null)
            {
                for (int i = 0; i < ServiceListCommand.Count && i < ServiceDuration.Count && i < ServiceDiscount.Count; i++)
                {
                    var service = ServiceServices.Instance.GetService(int.Parse(ServiceListCommand[i]));
                    var serviceViewModel = new ServiceAppViewModel
                    {
                        Service = service.Name,
                        Duration = ServiceDuration[i],
                        Price = service.Price,
                        Discount = float.Parse(ServiceDiscount[i]),
                        Category = service.Category,
                        ID = service.ID,
                    };

                    serviceList.Add(serviceViewModel);
                }

            }
            model.Services = serviceList;
            model.Customer = customer;
            model.Appointment = appointment;
            model.Employee = employee;

            model.Selected = "Appointment Details";
            return View(model);

        }

        [HttpGet]
        [NoCache]
        public ActionResult AppointmentDetailsCheckOutView(int AppointmentID)
        {
            AppointmentDetailsViewModel model = new AppointmentDetailsViewModel();
            var appointment = AppointmentServices.Instance.GetAppointment(AppointmentID);
            var customer = CustomerServices.Instance.GetCustomer(appointment.CustomerID);
            var employee = EmployeeServices.Instance.GetEmployee(appointment.EmployeeID);
            var serviceList = new List<ServiceAppViewModel>();
            var ServiceListCommand = appointment.Service.Split(',').ToList();
            var ServiceDuration = appointment.ServiceDuration.Split(',').ToList();
            var ServiceDiscount = appointment.ServiceDiscount.Split(',').ToList();




            if (appointment.Service != null)
            {
                for (int i = 0; i < ServiceListCommand.Count && i < ServiceDuration.Count && i < ServiceDiscount.Count; i++)
                {
                    var service = ServiceServices.Instance.GetService(int.Parse(ServiceListCommand[i]));
                    var serviceViewModel = new ServiceAppViewModel
                    {
                        Service = service.Name,
                        Duration = ServiceDuration[i],
                        Price = service.Price,
                        Category = service.Category,
                        Discount = float.Parse(ServiceDiscount[i]),
                        ID = service.ID,
                    };

                    serviceList.Add(serviceViewModel);
                }

            }
            model.Services = serviceList;
            model.Customer = customer;
            model.Appointment = appointment;
            model.Employee = employee;
            model.Selected = "Checkout";
            model.Company = CompanyServices.Instance.GetCompany(appointment.Business).FirstOrDefault();

            return View(model);

        }


        [NoCache]
        public ActionResult Action(int ID = 0)
        {
            AppointmentActionViewModel model = new AppointmentActionViewModel();
            var ServicesList = new List<ServiceModel>();
            var LoggedInUser = UserManager.FindById(User.Identity.GetUserId());
            if (LoggedInUser == null) { return RedirectToAction("Login", "Account"); }
            var company = CompanyServices.Instance.GetCompany().Where(x => x.Business == LoggedInUser.Company).FirstOrDefault();
            if (LoggedInUser.Role != "Super Admin")
            {
                var categories = ServicesCategoriesServices.Instance.GetServiceCategoriesWRTBusiness(LoggedInUser.Company, "");
                foreach (var item in categories)
                {
                    var ServicesWRTCategory = ServiceServices.Instance.GetServiceWRTCategory(LoggedInUser.Company, item.Business).Where(x => x.IsActive).ToList();
                    ServicesList.Add(new ServiceModel { ServiceCategory = item, Services = ServicesWRTCategory, Company = company });
                }
                var employees = new List<Employee>();
                employees.AddRange(EmployeeServices.Instance.GetEmployeeWRTBusiness(LoggedInUser.Company, true).OrderBy(x => x.DisplayOrder).ToList());


                var employeeRequests = EmployeeRequestServices.Instance.GetEmployeeRequestByBusiness(company.ID);
                foreach (var item in employeeRequests)
                {
                    var employeeCompany = EmployeeServices.Instance.GetEmployee(item.EmployeeID);
                    if (item.Accepted)
                    {
                        if (!employees.Select(x => x.ID).Contains(employeeCompany.ID))
                        {
                            employees.Add(employeeCompany);
                        }
                    }
                }

                model.Employees = employees;
            }
            else
            {
                var categories = ServicesCategoriesServices.Instance.GetServiceCategories().OrderBy(x => x.DisplayOrder).ToList();
                foreach (var item in categories)
                {
                    var ServicesWRTCategory = ServiceServices.Instance.GetService().Where(x => x.Category == item.Name && x.IsActive).OrderBy(x => x.DisplayOrder).ToList();
                    ServicesList.Add(new ServiceModel { ServiceCategory = item, Services = ServicesWRTCategory, Company = company });
                }


                model.Employees = EmployeeServices.Instance.GetEmployeeWRTBusiness(true).OrderBy(x => x.DisplayOrder).ToList();
            }
            model.Company = company;
            //model.Services = ServicesList.OrderBy(X => X.ServiceCategory.DisplayOrder).ToList();
            if (ID != 0)
            {
                var appointment = AppointmentServices.Instance.GetAppointment(ID);
                model.Service = appointment.Service;
                var ListOfServiceAlloted = new List<ServiceAppViewModel>();
                var serviceList = new List<ServiceAppViewModel>();
                var AppoinmentViewModel = new List<AppointmentModel>();

                var customer = CustomerServices.Instance.GetCustomer(appointment.CustomerID);
                if (appointment.Service != null)
                {
                    var ServiceListCommand = appointment.Service.Split(',').ToList();
                    var ServiceDuration = appointment.ServiceDuration.Split(',').ToList();
                    var ServiceDiscount = appointment.ServiceDiscount.Split(',').ToList();
                    if (model.Service != null)
                    {
                        for (int i = 0; i < ServiceListCommand.Count && i < ServiceDuration.Count && i < ServiceDiscount.Count; i++)
                        {
                            var service = ServiceServices.Instance.GetService(int.Parse(ServiceListCommand[i]));
                            var serviceViewModel = new ServiceAppViewModel
                            {
                                Service = service.Name,
                                Duration = ServiceDuration[i],
                                Price = service.Price,
                                Discount = float.Parse(ServiceDiscount[i]),
                                Category = service.Category,
                                ID = service.ID,
                            };

                            serviceList.Add(serviceViewModel);
                        }

                    }
                }
                model.ServiceAlotted = serviceList;
                model.Date = appointment.Date;
                model.Time = appointment.Time;
                model.Color = appointment.Color;
                model.IsRepeat = appointment.IsRepeat;
                model.ID = appointment.ID;
                model.EmployeeID = appointment.EmployeeID;
                model.Frequency = appointment.Frequency;
                model.Deposit = appointment.Deposit;
                model.OnlinePriceChange = appointment.OnlinePriceChange;
                model.PriceChangeOnline = PriceChangeServices.Instance.GetPriceChange(model.OnlinePriceChange);
                model.DepositMethod = appointment.DepositMethod;
                if (appointment.Frequency == "Every Week")
                {
                    model.EveryWeek = appointment.Every;
                    if (appointment.Ends.Split('_')[0] == "NumberOfTimes")
                    {
                        model.EndsNumberOfTimesWeek = int.Parse(appointment.Ends.Split('_')[1]);

                    }
                    else if (appointment.Ends.Split('_')[0] == "Specific Date")
                    {
                        model.EndsDateWeek = DateTime.Parse(appointment.Ends.Split('_')[1]);
                    }

                }
                else if (appointment.Frequency == "Every Day")
                {
                    model.EveryDay = appointment.Every;
                    if (appointment.Ends.Split('_')[0] == "NumberOfTimes")
                    {
                        model.EndsNumberOfTimesDay = int.Parse(appointment.Ends.Split('_')[1]);

                    }
                    else if (appointment.Ends.Split('_')[0] == "Specific Date")
                    {
                        model.EndsDateDay = DateTime.Parse(appointment.Ends.Split('_')[1]);
                    }

                }
                else if (appointment.Frequency == "Every Month")
                {
                    model.EveryMonth = appointment.Every;
                    if (appointment.Ends.Split('_')[0] == "NumberOfTimes")
                    {
                        model.EndsNumberOfTimesMonth = int.Parse(appointment.Ends.Split('_')[1]);

                    }
                    else if (appointment.Ends.Split('_')[0] == "Specific Date")
                    {
                        model.EndsDateMonth = DateTime.Parse(appointment.Ends.Split('_')[1]);
                    }
                }
                else
                {
                    model.Every = "";
                }

                model.ServiceDuration = appointment.ServiceDuration;
                model.ServiceDiscount = appointment.ServiceDiscount;
                model.Notes = appointment.Notes;
                model.TotalCost = appointment.TotalCost;
                model.IsWalkIn = appointment.IsWalkIn;
                model.CustomerID = appointment.CustomerID;
                if (model.CustomerID != 0)
                {
                    model.Customer = CustomerServices.Instance.GetCustomer(model.CustomerID);
                }
                model.BookingDate = appointment.BookingDate;
                model.Label = appointment.Label;
                model.Status = appointment.Status;
                model.Days = appointment.Days;
                model.Service = appointment.Service;
            }
            model.Company = company;
            return View(model);
        }


        [HttpGet]
        public ActionResult ShowWaitingLists(string Date, int EmployeeID)
        {
            AppointmentListingViewModel model = new AppointmentListingViewModel();
            var mainWaitingListModel = new List<WaitingListDetailedModel>();
            var LoggedInUser = UserManager.FindById(User.Identity.GetUserId());

            var SelectedDate = DateTime.Parse(Date);
            var EmployeeSelectedWaitingList = WaitingListServices.Instance.GetWaitingList(LoggedInUser.Company, SelectedDate.Day, SelectedDate.Month, SelectedDate.Year, "Created", EmployeeID);
            var NonSelectedWaitingList = WaitingListServices.Instance.GetWaitingList(LoggedInUser.Company, SelectedDate.Day, SelectedDate.Month, SelectedDate.Year, "Created", true);

            var CombinedList = EmployeeSelectedWaitingList.Concat(NonSelectedWaitingList).ToList();
            foreach (var item in CombinedList)
            {
                int TotalDuration = 0;
                var customer = CustomerServices.Instance.GetCustomer(item.CustomerID);
                var serviceList = new List<ServiceAppViewModel>();
                if (item.Service != null)
                {
                    var ServiceListCommand = item.Service.Split(',').ToList();
                    var ServiceDuration = item.ServiceDuration.Split(',').ToList();


                    for (int i = 0; i < ServiceListCommand.Count && i < ServiceDuration.Count; i++)
                    {
                        var Service = ServiceServices.Instance.GetService(int.Parse(ServiceListCommand[i]));
                        if (Service != null)
                        {
                            var serviceViewModel = new ServiceAppViewModel
                            {

                                Service = Service.Name,
                                Duration = ServiceDuration[i],
                                Price = Service.Price,
                                Category = Service.Category,
                                ID = Service.ID,
                            };
                            serviceList.Add(serviceViewModel);
                            TotalDuration += int.Parse(serviceViewModel.Duration.ToLower().Replace("mins", "").Replace(" ", "").Trim());

                        }

                    }
                }

                if (customer == null)
                {
                    if (item.NonSelectedEmployee == true)
                    {
                        mainWaitingListModel.Add(new WaitingListDetailedModel { WaitingList = item, Customer = null, Services = serviceList, TotalDuration = TotalDuration });
                    }
                    else
                    {
                        mainWaitingListModel.Add(new WaitingListDetailedModel { WaitingList = item, Customer = null, Services = serviceList, TotalDuration = TotalDuration, Employee = EmployeeServices.Instance.GetEmployee(item.EmployeeID) });
                    }
                }
                else
                {
                    if (item.NonSelectedEmployee == true)
                    {
                        mainWaitingListModel.Add(new WaitingListDetailedModel { WaitingList = item, Customer = customer, Services = serviceList, TotalDuration = TotalDuration });

                    }
                    else
                    {
                        mainWaitingListModel.Add(new WaitingListDetailedModel { WaitingList = item, Customer = customer, Services = serviceList, TotalDuration = TotalDuration, Employee = EmployeeServices.Instance.GetEmployee(item.EmployeeID) });

                    }

                }
            }
            model.MainWaitingLists = mainWaitingListModel;
            model.SelectedEmployee = EmployeeServices.Instance.GetEmployee(EmployeeID);
            return PartialView("_ShowWaitingLists", model);
        }

        [HttpGet]
        [NoCache]
        public ActionResult GetAppointmentDetails(int appointmentId)
        {
            var appointmentModel = new AppointmentModel();
            // Fetch appointment details from your data source by appointmentId
            var appointmentDetails = AppointmentServices.Instance.GetAppointment(appointmentId);
            //appointmentModel.Employee = EmployeeServices.Instance.GetEmployee(appointmentDetails.EmployeeID);
            var employee = EmployeeServices.Instance.GetEmployee(appointmentDetails.EmployeeID);
            appointmentModel.Business = appointmentDetails.Business;
            int TotalDuration = 0;
            var customer = CustomerServices.Instance.GetCustomer(appointmentDetails.CustomerID);
            var serviceList = new List<ServiceAppViewModel>();
            if (appointmentDetails.Service != null)
            {
                var ServiceListCommand = appointmentDetails.Service.Split(',').ToList();
                var ServiceDuration = appointmentDetails.ServiceDuration.Split(',').ToList();


                for (int i = 0; i < ServiceListCommand.Count && i < ServiceDuration.Count; i++)
                {
                    var Service = ServiceServices.Instance.GetService(int.Parse(ServiceListCommand[i]));
                    if (Service != null)
                    {
                        var serviceViewModel = new ServiceAppViewModel
                        {

                            Service = Service.Name,
                            Duration = ServiceDuration[i],
                            Price = Service.Price,
                            Category = Service.Category,
                            ID = Service.ID,
                        };
                        TotalDuration += int.Parse(serviceViewModel.Duration.ToLower().Replace("mins", "").Replace(" ", "").Trim());
                        serviceList.Add(serviceViewModel);
                    }

                }
            }
            if (customer == null)
            {

                appointmentModel.CustomerFirstName = "Walk In";
                appointmentModel.CustomerLastName = "";
                appointmentModel.IsRepeat = appointmentDetails.IsRepeat;
                appointmentModel.FromGCAL = appointmentDetails.FromGCAL;
                appointmentModel.Customer = null;
                appointmentModel.CustomerEmail = "";
                appointmentModel.ServicesNew = serviceList;
                appointmentModel.TotalDuration = TotalDuration;

            }
            else
            {
                appointmentModel.CustomerFirstName = customer.FirstName;
                appointmentModel.FromGCAL = appointmentDetails.FromGCAL;
                appointmentModel.IsRepeat = appointmentDetails.IsRepeat;
                appointmentModel.CustomerLastName = customer.LastName;
                appointmentModel.CustomerEmail = customer.Email;
                appointmentModel.Customer = customer;
                appointmentModel.ServicesNew = serviceList;
                appointmentModel.MobileNumber = customer.MobileNumber;
                appointmentModel.TotalDuration = TotalDuration;

                int NoOfAppointments = InvoiceServices.Instance.GetInvoice(customer.Business, customer.ID, "").Count();
                int NoOfNoShows = AppointmentServices.Instance
                 .GetAllAppointmentWRTBusiness(customer.Business, false, customer.ID)
                 .Where(x => x.Status == "No Show")
                 .Count();
                appointmentModel.NoOfAppointments = NoOfAppointments;
                appointmentModel.NoOfNoShows = NoOfNoShows;
            }
            appointmentModel.IsPaid = appointmentDetails.IsPaid;
            appointmentModel.IsCancelled = appointmentDetails.IsCancelled;
            appointmentModel.ID = appointmentDetails.ID;
            appointmentModel.Notes = appointmentDetails.Notes;
            // Render the appointment details as HTML (you can customize this part)
            appointmentModel.DateString = appointmentDetails.Date.ToString("yyyy-MM-dd");
            appointmentModel.StartTime = appointmentDetails.Time.ToString("HH:mm");
            appointmentModel.EndTime = appointmentDetails.EndTime.ToString("HH:mm");
            var company = CompanyServices.Instance.GetCompany().Where(x => x.Business == appointmentDetails.Business).FirstOrDefault();
            var employeeRequest = EmployeeRequestServices.Instance.GetEmployeeRequestByBusiness(company.ID);
            foreach (var item in employeeRequest)
            {
                if (item.EmployeeID == appointmentModel.EmployeeID)
                {
                    if (item.Accepted)
                    {
                        appointmentModel.Business = item.Business;
                    }
                }
            }
            // Return the HTML to populate the modal
            return Json(appointmentModel, JsonRequestBehavior.AllowGet);
        }

        //public string SendEmail(string toEmail, string subject, string emailBody, Company company)
        //{

        //    try
        //    {
        //        string senderEmail = "support@yourbookingplatform.com";
        //        string senderPassword = "ttpa fcbl mpbn fxdl";

        //        int Port = int.Parse(ConfigurationManager.AppSettings["portforSmtp"]);
        //        string Host = ConfigurationManager.AppSettings["hostForSmtp"];
        //        System.Net.Mail.MailMessage mail = new System.Net.Mail.MailMessage();
        //        mail.To.Add(toEmail);
        //        MailAddress ccAddress = new MailAddress(company.NotificationEmail, company.Business);

        //        mail.CC.Add(ccAddress);
        //        mail.From = new MailAddress(company.NotificationEmail, company.Business, System.Text.Encoding.UTF8);
        //        mail.Subject = subject;
        //        mail.SubjectEncoding = System.Text.Encoding.UTF8;
        //        mail.ReplyTo = new MailAddress(company.NotificationEmail); // Set the ReplyTo address

        //        mail.Body = emailBody;
        //        mail.BodyEncoding = System.Text.Encoding.UTF8;
        //        mail.IsBodyHtml = true;

        //        mail.Priority = MailPriority.High;
        //        SmtpClient client = new SmtpClient();
        //        client.Credentials = new System.Net.NetworkCredential(senderEmail, senderPassword);
        //        client.Port = Port;
        //        client.Host = Host;
        //        client.EnableSsl = true;
        //        client.Send(mail);
        //        return "Done";
        //    }
        //    catch (Exception ex)
        //    {
        //        Session["EmailStatus"] = ex.ToString();
        //        return "Failed";
        //    }


        //}


        [HttpGet]
        public ActionResult UploadFile(int CustomerID, int ID = 0)
        {
            AppointmentDetailsViewModel model = new AppointmentDetailsViewModel();
            model.Customer = CustomerServices.Instance.GetCustomer(CustomerID);
            if (ID != 0)
            {
                var file = FileServices.Instance.GetFile(ID);
                model.Name = file.Name;
                model.Size = file.Size;
                model.URL = file.URL;
                model.DateTime = file.DateTime;
                model.CustomerID = file.CustomerID;
                model.UploadedBy = file.UploadedBy;
                model.FileID = file.ID;

            }
            return PartialView("_UploadFile", model);
        }

        [HttpPost]
        public JsonResult UploadFile(int ID, int CustomerID, string URL, string Name, string Size)
        {
            var LoggedInUser = UserManager.FindById(User.Identity.GetUserId());
            if (ID == 0)
            {
                var newFile = new File();
                newFile.Business = LoggedInUser.Company;
                newFile.Size = Size;
                newFile.CustomerID = CustomerID;
                newFile.Name = Name;
                newFile.URL = URL;
                newFile.DateTime = DateTime.Now;
                newFile.UploadedBy = LoggedInUser.Name;
                FileServices.Instance.SaveFile(newFile);

                var history = new History();
                history.Business = LoggedInUser.Company;
                var customer = CustomerServices.Instance.GetCustomer(CustomerID);
                history.CustomerName = customer.FirstName + " " + customer.LastName;
                history.Date = DateTime.Now;
                history.Note = "File Saved for " + history.CustomerName;
                history.EmployeeName = "";
                HistoryServices.Instance.SaveHistory(history);
            }
            else
            {
                var newFile = FileServices.Instance.GetFile(ID);
                newFile.ID = ID;
                newFile.Business = LoggedInUser.Company;
                newFile.Size = Size;
                newFile.CustomerID = CustomerID;
                newFile.Name = Name;
                newFile.URL = URL;
                newFile.DateTime = DateTime.Now;
                newFile.UploadedBy = LoggedInUser.Name;
                FileServices.Instance.UpdateFile(newFile);

                var history = new History();
                history.Business = LoggedInUser.Company;
                var customer = CustomerServices.Instance.GetCustomer(CustomerID);
                history.CustomerName = customer.FirstName + " " + customer.LastName;
                history.Date = DateTime.Now;
                history.Note = "File Saved for " + history.CustomerName;
                history.EmployeeName = "";
                HistoryServices.Instance.SaveHistory(history);
            }
            return Json(new { success = true }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult GetEmployeeAppointments(int employeeID)
        {
            var AppointmentModel = new List<AppointmentModel>();
            var LoggedInUser = UserManager.FindById(User.Identity.GetUserId());
            if (LoggedInUser.Role != "Super Admin")
            {
                if (employeeID == 0)
                {

                    var appointments = AppointmentServices.Instance.GetAppointment().Where(x => x.Business == LoggedInUser.Company && x.DELETED == false).ToList();


                    foreach (var item in appointments)
                    {
                        var customer = CustomerServices.Instance.GetCustomer(item.CustomerID);
                        var serviceList = new List<ServiceAppViewModel>();
                        if (item.Service != null)
                        {
                            var ServiceListCommand = item.Service.Split(',').ToList();
                            var ServiceDuration = item.ServiceDuration.Split(',').ToList();


                            for (int i = 0; i < ServiceListCommand.Count && i < ServiceDuration.Count; i++)
                            {
                                var Service = ServiceServices.Instance.GetService(int.Parse(ServiceListCommand[i]));
                                if (Service != null)
                                {
                                    var serviceViewModel = new ServiceAppViewModel
                                    {

                                        Service = Service.Name,
                                        Duration = ServiceDuration[i],
                                        Category = Service.Category,
                                        Price = Service.Price,
                                        ID = Service.ID,
                                    };
                                    serviceList.Add(serviceViewModel);
                                }

                            }
                        }
                        //if (customer == null)
                        //{

                        //    AppointmentModel.Add(new AppointmentModel { Appointment = item, Customer = null, Services = serviceList });
                        //}
                        //else
                        //{

                        //    AppointmentModel.Add(new AppointmentModel { Appointment = item, Customer = customer, Services = serviceList });

                        //}
                    }
                }
                else
                {
                    var appointments = AppointmentServices.Instance.GetAppointment().Where(x => x.Business == LoggedInUser.Company && x.EmployeeID == employeeID && x.DELETED == false).ToList();


                    foreach (var item in appointments)
                    {
                        var customer = CustomerServices.Instance.GetCustomer(item.CustomerID);
                        var serviceList = new List<ServiceAppViewModel>();
                        if (item.Service != null)
                        {
                            var ServiceListCommand = item.Service.Split(',').ToList();
                            var ServiceDuration = item.ServiceDuration.Split(',').ToList();


                            for (int i = 0; i < ServiceListCommand.Count && i < ServiceDuration.Count; i++)
                            {
                                var Service = ServiceServices.Instance.GetService(int.Parse(ServiceListCommand[i]));
                                if (Service != null)
                                {
                                    var serviceViewModel = new ServiceAppViewModel
                                    {

                                        Service = Service.Name,
                                        Duration = ServiceDuration[i],
                                        Category = Service.Category,
                                        Price = Service.Price,
                                        ID = Service.ID,
                                    };
                                    serviceList.Add(serviceViewModel);
                                }

                            }
                        }
                        //if (customer == null)
                        //{

                        //    AppointmentModel.Add(new AppointmentModel { Appointment = item, Customer = null, Services = serviceList });
                        //}
                        //else
                        //{

                        //    AppointmentModel.Add(new AppointmentModel { Appointment = item, Customer = customer, Services = serviceList });

                        //}
                    }
                }
            }
            else
            {
                if (employeeID == 0)
                {
                    var appointments = AppointmentServices.Instance.GetAppointment().Where(x => x.DELETED == false);


                    foreach (var item in appointments)
                    {
                        var customer = CustomerServices.Instance.GetCustomer(item.CustomerID);
                        var serviceList = new List<ServiceAppViewModel>();
                        if (item.Service != null)
                        {
                            var ServiceListCommand = item.Service.Split(',').ToList();
                            var ServiceDuration = item.ServiceDuration.Split(',').ToList();


                            for (int i = 0; i < ServiceListCommand.Count && i < ServiceDuration.Count; i++)
                            {
                                var Service = ServiceServices.Instance.GetService(int.Parse(ServiceListCommand[i]));
                                if (Service != null)
                                {
                                    var serviceViewModel = new ServiceAppViewModel
                                    {

                                        Service = Service.Name,
                                        Duration = ServiceDuration[i],
                                        Category = Service.Category,
                                        Price = Service.Price,
                                        ID = Service.ID,
                                    };
                                    serviceList.Add(serviceViewModel);
                                }

                            }
                        }
                        //if (customer == null)
                        //{

                        //    AppointmentModel.Add(new AppointmentModel { Appointment = item, Customer = null, Services = serviceList });
                        //}
                        //else
                        //{

                        //    AppointmentModel.Add(new AppointmentModel { Appointment = item, Customer = customer, Services = serviceList });

                        //}
                    }
                }
                else
                {

                    var appointments = AppointmentServices.Instance.GetAppointment().Where(x => x.EmployeeID == employeeID && x.DELETED == false).ToList();


                    foreach (var item in appointments)
                    {
                        var customer = CustomerServices.Instance.GetCustomer(item.CustomerID);
                        var serviceList = new List<ServiceAppViewModel>();
                        if (item.Service != null)
                        {
                            var ServiceListCommand = item.Service.Split(',').ToList();
                            var ServiceDuration = item.ServiceDuration.Split(',').ToList();


                            for (int i = 0; i < ServiceListCommand.Count && i < ServiceDuration.Count; i++)
                            {
                                var Service = ServiceServices.Instance.GetService(int.Parse(ServiceListCommand[i]));
                                if (Service != null)
                                {
                                    var serviceViewModel = new ServiceAppViewModel
                                    {

                                        Service = Service.Name,
                                        Duration = ServiceDuration[i],
                                        Category = Service.Category,
                                        Price = Service.Price,
                                        ID = Service.ID,
                                    };
                                    serviceList.Add(serviceViewModel);
                                }

                            }
                        }
                        //if (customer == null)
                        //{

                        //    AppointmentModel.Add(new AppointmentModel { Appointment = item, Customer = null, Services = serviceList });
                        //}
                        //else
                        //{

                        //    AppointmentModel.Add(new AppointmentModel { Appointment = item, Customer = customer, Services = serviceList });

                        //}
                    }
                }
            }
            return Json(AppointmentModel, JsonRequestBehavior.AllowGet);

        }

        [HttpPost]
        public ActionResult UpdateAppointment(string id, string start, string end, string EmployeeID)
        {
            var LoggedInUser = UserManager.FindById(User.Identity.GetUserId());
            var appointment = AppointmentServices.Instance.GetAppointment(int.Parse(id));
            var oldDate = appointment.Date;
            var oldtime = appointment.Time;
            var oldEmployee = EmployeeServices.Instance.GetEmployee(appointment.EmployeeID);
            var ID = int.Parse(id);
            //var StartDate = DateTime.Parse(start);
            //var EndDate = DateTime.Parse(end);

            // Extract time portion directly from the input
            string[] startParts = start.Split(' '); // Split the incoming start string
            string startTime = startParts[4]; // Extract the time portion: "12:00:00"

            string[] endParts = end.Split(' '); // Split the incoming end string
            string endTime = endParts[4]; // Extract the time portion: "14:00:00"


            TimeSpan startTimeSpan = TimeSpan.Parse(startTime);
            TimeSpan endTimeSpan = TimeSpan.Parse(endTime);

            DateTime combinedStartDateTime = appointment.Date.Date + startTimeSpan; // Combine date and start time
            DateTime combinedEndDateTime = appointment.Date.Date + endTimeSpan; // Combine date and end time


            appointment.Time = combinedStartDateTime;
            appointment.EndTime = combinedEndDateTime;
            appointment.EmployeeID = int.Parse(EmployeeID);
            AppointmentServices.Instance.UpdateAppointment(appointment);
            CreateBuffer(appointment.ID);
            //var result = AppointmentServices.Instance.UpdateEvent(ID, start, end, int.Parse(EmployeeID));




            var appointmentAgain = AppointmentServices.Instance.GetAppointment(int.Parse(id));
            int year = appointmentAgain.Date.Year;
            int month = appointmentAgain.Date.Month;
            int day = appointmentAgain.Date.Day;
            int starthour = appointmentAgain.Time.Hour;
            int startminute = appointmentAgain.Time.Minute;
            int startseconds = appointmentAgain.Time.Second;

            int endhour = appointmentAgain.EndTime.Hour;
            int endminute = appointmentAgain.EndTime.Minute;
            int endseconds = appointmentAgain.EndTime.Second;



            DateTime startDateNew = new DateTime(year, month, day, starthour, startminute, startseconds);
            DateTime EndDateNew = new DateTime(year, month, day, endhour, endminute, endseconds);




            var company = CompanyServices.Instance.GetCompany(LoggedInUser.Company).FirstOrDefault();

            string ConcatenatedServices = "";
            foreach (var item in appointmentAgain.Service.Split(',').ToList())
            {
                var Service = ServiceServices.Instance.GetService(int.Parse(item));
                if (Service != null)
                {
                    if (ConcatenatedServices == "")
                    {
                        ConcatenatedServices = String.Join(",", Service.Name);
                    }
                    else
                    {
                        ConcatenatedServices = String.Join(",", ConcatenatedServices, Service.Name);

                    }
                }
            }

            var employee = EmployeeServices.Instance.GetEmployee(appointmentAgain.EmployeeID);
            var googleCalendar = GoogleCalendarServices.Instance.GetGoogleCalendarServicesWRTBusiness(company.Business);
            if (googleCalendar != null && !googleCalendar.Disabled)
            {
                if (oldEmployee.ID != appointmentAgain.EmployeeID)
                {
                    GenerateonGoogleCalendar(appointment.ID, ConcatenatedServices, oldEmployee.ID, "SAVING");


                }
                else
                {
                    GenerateonGoogleCalendar(appointment.ID, ConcatenatedServices, 0, "SAVING");
                }
            }
            #region MailingRegion

            var customer = CustomerServices.Instance.GetCustomer(appointment.CustomerID);
            if (customer != null)
            {
                if (customer.Password == null)
                {
                    Random random = new Random();
                    customer.Password = new string(Enumerable.Repeat("abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789", 8).Select(s => s[random.Next(s.Length)]).ToArray());
                    CustomerServices.Instance.UpdateCustomer(customer);
                }

                var historyNew = new History();
                historyNew.Business = appointment.Business;
                historyNew.CustomerName = customer.FirstName + " " + customer.LastName;
                historyNew.Date = DateTime.Now;
                historyNew.Note = "Appointment was moved by " + LoggedInUser.Name + " Previous Date:" + oldDate.ToString("yyyy-MM-dd") + "Time was " + oldtime.ToString("HH:mm") + " to new Date: " + appointment.Date.ToString("yyyy-MM-dd") + " and New Time is: " + appointment.Time.ToString("HH:mm");
                historyNew.EmployeeName = employee.Name;
                historyNew.Name = "Moved";
                historyNew.AppointmentID = appointment.ID;
                HistoryServices.Instance.SaveHistory(historyNew);

                var emailDetails = EmailTemplateServices.Instance.GetEmailTemplateWRTBusiness(LoggedInUser.Company, "Appointment Moved");
                if (emailDetails != null && emailDetails.IsActive == true)
                {
                    string emailBody = "<html><body>";
                    emailBody += "<h2 style='font-family: Arial, sans-serif;'>Appointment Moved</h2>";
                    emailBody += emailDetails.TemplateCode;
                    emailBody = emailBody.Replace("{{Customer_first_name}}", customer.FirstName);
                    emailBody = emailBody.Replace("{{Customer_last_name}}", customer.LastName);
                    emailBody = emailBody.Replace("{{Customer_initial}}", customer.Gender == "Male" ? "Mr." : "Ms.");
                    emailBody = emailBody.Replace("{{date}}", appointmentAgain.Date.ToString("yyyy-MM-dd"));
                    emailBody = emailBody.Replace("{{time}}", appointmentAgain.Time.ToString("H:mm:ss"));
                    emailBody = emailBody.Replace("{{end_time}}", appointmentAgain.EndTime.ToString("H:mm"));
                    emailBody = emailBody.Replace("{{employee_picture}}", $"<img class='text-center' style='height:50px;width:auto;' src='{"http://app.yourbookingplatform.com" + employee.Photo}'>");
                    emailBody = emailBody.Replace("{{previous_date}}", oldDate.ToString("yyyy-MM-dd"));
                    emailBody = emailBody.Replace("{{previous_time}}", oldtime.ToString("H:mm"));
                    emailBody = emailBody.Replace("{{employee}}", employee.Name);
                    emailBody = emailBody.Replace("{{employee_specialization}}", employee.Specialization);
                    emailBody = emailBody.Replace("{{services}}", ConcatenatedServices);
                    emailBody = emailBody.Replace("{{company_name}}", LoggedInUser.Company);
                    emailBody = emailBody.Replace("{{company_email}}", company.NotificationEmail);
                    emailBody = emailBody.Replace("{{password}}", customer.Password);
                    emailBody = emailBody.Replace("{{company_address}}", company.Address);
                    emailBody = emailBody.Replace("{{company_logo}}", $"<img class='text-center' style='height:50px;width:auto;' src='{"http://app.yourbookingplatform.com" + company.Logo}'>");
                    emailBody = emailBody.Replace("{{company_phone}}", company.PhoneNumber);
                    string cancelLink = string.Format("http://app.yourbookingplatform.com/Appointment/CancelByEmail/?AppointmentID={0}", appointment.ID);
                    emailBody = emailBody.Replace("{{cancellink}}", $"<a href='{cancelLink}' class='btn btn-primary'>CANCEL/RESCHEDULE</a>");

                    emailBody += "</body></html>";
                    if (IsValidEmail(customer.Email))
                    {
                        SendEmail(customer.Email, "Appointment Moved", emailBody, company);
                    }
                }
            }
            #endregion

            return Json(new { success = true, message = "Event updated successfully", End = appointmentAgain.EndTime.ToString("HH:mm"), Start = appointment.Date.ToString("yyyy-MM-dd"), Time = appointment.Time.ToString("HH:mm") }, JsonRequestBehavior.AllowGet);
            //}
            //else
            //{
            //    var response = new { success = false, message = "Error updating event" };

            //    return Json(response);
            //}


        }

        public bool IsSlotWithinEmployeeTimeRange(DateTime slotStartTime, DateTime slotEndTime, DateTime employeeStartTime, DateTime employeeEndTime)
        {
            // Ensure slot times are within the employee's available times
            return slotStartTime.TimeOfDay >= employeeStartTime.TimeOfDay && slotEndTime.TimeOfDay <= employeeEndTime.TimeOfDay;
        }

        public List<string> FindAvailableSlots(DateTime employeeStartTime, DateTime employeeEndTime, List<Appointment> appointments, int durationInMinutes, Company Company, List<string> Services, int EmployeeID)
        {
            List<string> availableSlots = new List<string>();
            List<string> NotavailableSlots = new List<string>();
            DateTime currentTime = DateTime.Now;
            var increaseMins = 0;
            foreach (var item in Services)
            {
                var empservice = EmployeeServiceServices.Instance.GetEmployeeService(EmployeeID, int.Parse(item));
                if (empservice != null)
                {
                    if (empservice.BufferEnabled)
                    {
                        increaseMins += int.Parse(empservice.BufferTime.Replace("mins", "").Replace("min", ""));
                    }
                }
            }
            durationInMinutes += increaseMins;

            currentTime = TheBookingPlatform.Models.TimeZoneConverter.ConvertToTimeZone(currentTime, Company.TimeZone).AddMinutes(5);
            var FinalAppointments = new List<Appointment>();
            foreach (var item in appointments)
            {
                if (item.IsPaid == false && item.DepositMethod == "Online" && item.IsCancelled == false && (DateTime.Now - item.BookingDate).TotalMinutes > 15)
                {
                    item.IsCancelled = true;
                }
                else
                {
                    FinalAppointments.Add(item);
                }
            }
            FinalAppointments.Sort((x, y) => x.Time.TimeOfDay.CompareTo(y.Time.TimeOfDay));

            DateTime lastEndTime = employeeStartTime;

            if (FinalAppointments.Count > 0)
            {
                var firstAppointment = FinalAppointments[0];
                var FirstAppointmentEndTime = firstAppointment.EndTime;
                var buffers = BufferServices.Instance.GetBufferWRTBusinessList(firstAppointment.Business, firstAppointment.ID);
                if (buffers != null && buffers.Count() > 0)
                {
                    FirstAppointmentEndTime = buffers.OrderBy(x => x.Time).LastOrDefault().EndTime;
                }
                // Consider slots before the first appointment
                // Step 1: Check if currentTime is greater than or equal to employeeStartTime
                if (currentTime >= employeeStartTime)
                {
                    // Step 2: Check if there is a gap between currentTime and the first appointment
                    if (firstAppointment.Time.TimeOfDay > currentTime.TimeOfDay)
                    {
                        TimeSpan timeGap = firstAppointment.Time.TimeOfDay - currentTime.TimeOfDay;

                        if (timeGap.TotalMinutes >= durationInMinutes)
                        {
                            int slots = (int)(timeGap.TotalMinutes / durationInMinutes);

                            for (int i = 0; i < slots; i++)
                            {
                                DateTime slotStart = currentTime.AddMinutes(i * durationInMinutes);
                                DateTime slotEnd = slotStart.AddMinutes(durationInMinutes);

                                if (IsSlotWithinEmployeeTimeRange(slotStart, slotEnd, employeeStartTime, employeeEndTime))
                                {
                                    if (employeeStartTime.Date.ToString("yyyy-MM-dd") == currentTime.ToString("yyyy-MM-dd"))
                                    {
                                        if (slotStart.TimeOfDay >= currentTime.TimeOfDay)
                                        {
                                            availableSlots.Add(slotStart.ToString("HH:mm") + " - " + slotEnd.ToString("HH:mm"));
                                        }
                                    }
                                    else
                                    {
                                        availableSlots.Add(slotStart.ToString("HH:mm") + " - " + slotEnd.ToString("HH:mm"));
                                    }
                                    lastEndTime = FirstAppointmentEndTime;
                                }
                            }
                        }
                    }
                }
                else if (employeeStartTime.TimeOfDay <= firstAppointment.Time.TimeOfDay)
                {
                    TimeSpan timeGap = firstAppointment.Time.TimeOfDay - employeeStartTime.TimeOfDay;

                    if (timeGap.TotalMinutes >= durationInMinutes)
                    {
                        int slots = (int)(timeGap.TotalMinutes / durationInMinutes);

                        for (int i = 0; i < slots; i++)
                        {
                            DateTime slotStart = employeeStartTime.AddMinutes(i * durationInMinutes);
                            DateTime slotEnd = slotStart.AddMinutes(durationInMinutes);
                            if (IsSlotWithinEmployeeTimeRange(slotStart, slotEnd, employeeStartTime, employeeEndTime))
                            {

                                if (employeeStartTime.Date.ToString("yyyy-MM-dd") == currentTime.ToString("yyyy-MM-dd"))
                                {
                                    if (slotStart.TimeOfDay >= currentTime.TimeOfDay)
                                    {
                                        availableSlots.Add(slotStart.ToString("HH:mm") + " - " + slotEnd.ToString("HH:mm"));

                                    }
                                }
                                else
                                {
                                    availableSlots.Add(slotStart.ToString("HH:mm") + " - " + slotEnd.ToString("HH:mm"));

                                }
                                lastEndTime = FirstAppointmentEndTime;


                            }
                        }
                    }

                }
            }

            foreach (var appointment in FinalAppointments)
            {
                var CurrentAppointmentEndTime = appointment.EndTime;
                var buffers = BufferServices.Instance.GetBufferWRTBusinessList(appointment.Business, appointment.ID);
                if (buffers != null && buffers.Count() > 0)
                {
                    CurrentAppointmentEndTime = buffers.OrderBy(x => x.Time).LastOrDefault().EndTime;
                }


                if (lastEndTime.TimeOfDay <= appointment.Time.TimeOfDay)
                {
                    TimeSpan timeGap = appointment.Time.TimeOfDay - lastEndTime.TimeOfDay;

                    if (timeGap.TotalMinutes >= durationInMinutes)
                    {
                        int slots = (int)(timeGap.TotalMinutes / durationInMinutes);

                        for (int i = 0; i < slots; i++)
                        {
                            DateTime slotStart = lastEndTime.AddMinutes(i * durationInMinutes);
                            DateTime slotEnd = slotStart.AddMinutes(durationInMinutes);
                            if (IsSlotWithinEmployeeTimeRange(slotStart, slotEnd, employeeStartTime, employeeEndTime))
                            {
                                if (slotEnd.TimeOfDay <= appointment.Time.TimeOfDay)
                                {
                                    if (employeeStartTime.Date.ToString("yyyy-MM-dd") == currentTime.ToString("yyyy-MM-dd"))
                                    {
                                        if (slotStart.TimeOfDay >= currentTime.TimeOfDay)
                                        {
                                            availableSlots.Add(slotStart.ToString("HH:mm") + " - " + slotEnd.ToString("HH:mm"));

                                        }
                                    }
                                    else
                                    {
                                        availableSlots.Add(slotStart.ToString("HH:mm") + " - " + slotEnd.ToString("HH:mm"));

                                    }
                                }

                            }
                        }
                    }


                }
                if (employeeStartTime.TimeOfDay <= appointment.Time.TimeOfDay)
                {
                    if (lastEndTime.TimeOfDay < CurrentAppointmentEndTime.TimeOfDay)
                    {
                        lastEndTime = CurrentAppointmentEndTime;
                    }
                }
                else if (employeeEndTime.TimeOfDay >= appointment.Time.TimeOfDay)
                {
                    lastEndTime = CurrentAppointmentEndTime;
                }

            }

            // Consider slots after the last appointment, if any
            if (lastEndTime.TimeOfDay < employeeEndTime.TimeOfDay)
            {
                if (lastEndTime.TimeOfDay > employeeStartTime.TimeOfDay)
                {
                    TimeSpan timeGap = employeeEndTime.TimeOfDay - lastEndTime.TimeOfDay;

                    if (timeGap.TotalMinutes >= durationInMinutes)
                    {
                        int slots = (int)(timeGap.TotalMinutes / durationInMinutes);

                        for (int i = 0; i < slots; i++)
                        {
                            DateTime slotStart = lastEndTime.AddMinutes(i * durationInMinutes);
                            DateTime slotEnd = slotStart.AddMinutes(durationInMinutes);
                            if (IsSlotWithinEmployeeTimeRange(slotStart, slotEnd, employeeStartTime, employeeEndTime))
                            {
                                if (slotEnd.Hour < employeeEndTime.TimeOfDay.Hours)
                                {
                                    if (employeeStartTime.Date.ToString("yyyy-MM-dd") == currentTime.ToString("yyyy-MM-dd"))
                                    {
                                        if (slotStart.TimeOfDay >= currentTime.TimeOfDay)
                                        {
                                            availableSlots.Add(slotStart.ToString("HH:mm") + " - " + slotEnd.ToString("HH:mm"));

                                        }
                                    }
                                    else
                                    {
                                        availableSlots.Add(slotStart.ToString("HH:mm") + " - " + slotEnd.ToString("HH:mm"));

                                    }
                                }
                                else if (slotEnd.Hour == employeeEndTime.TimeOfDay.Hours)
                                {
                                    if (slotEnd.Minute < employeeEndTime.TimeOfDay.Minutes)
                                    {
                                        if (employeeStartTime.Date.ToString("yyyy-MM-dd") == currentTime.ToString("yyyy-MM-dd"))
                                        {
                                            if (slotStart.TimeOfDay >= currentTime.TimeOfDay)
                                            {
                                                availableSlots.Add(slotStart.ToString("HH:mm") + " - " + slotEnd.ToString("HH:mm"));

                                            }
                                        }
                                        else
                                        {
                                            availableSlots.Add(slotStart.ToString("HH:mm") + " - " + slotEnd.ToString("HH:mm"));

                                        }
                                    }
                                    else if (slotEnd.Minute == employeeEndTime.TimeOfDay.Minutes)
                                    {
                                        if (employeeStartTime.Date.ToString("yyyy-MM-dd") == currentTime.ToString("yyyy-MM-dd"))
                                        {
                                            if (slotStart.TimeOfDay >= currentTime.TimeOfDay)
                                            {
                                                availableSlots.Add(slotStart.ToString("HH:mm") + " - " + slotEnd.ToString("HH:mm"));

                                            }
                                        }
                                        else
                                        {
                                            availableSlots.Add(slotStart.ToString("HH:mm") + " - " + slotEnd.ToString("HH:mm"));

                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                else
                {
                    TimeSpan timeGap = employeeEndTime.TimeOfDay - lastEndTime.TimeOfDay;

                    if (timeGap.TotalMinutes >= durationInMinutes)
                    {
                        int slots = (int)(timeGap.TotalMinutes / durationInMinutes);

                        for (int i = 0; i < slots; i++)
                        {
                            DateTime slotStart = lastEndTime.AddMinutes(i * durationInMinutes);
                            DateTime slotEnd = slotStart.AddMinutes(durationInMinutes);
                            if (IsSlotWithinEmployeeTimeRange(slotStart, slotEnd, employeeStartTime, employeeEndTime))
                            {
                                if (slotEnd.Hour < employeeEndTime.TimeOfDay.Hours)
                                {

                                    if (employeeStartTime.Date.ToString("yyyy-MM-dd") == currentTime.ToString("yyyy-MM-dd"))
                                    {
                                        if (slotStart.TimeOfDay >= currentTime.TimeOfDay)
                                        {
                                            availableSlots.Add(slotStart.ToString("HH:mm") + " - " + slotEnd.ToString("HH:mm"));

                                        }
                                    }
                                    else
                                    {
                                        availableSlots.Add(slotStart.ToString("HH:mm") + " - " + slotEnd.ToString("HH:mm"));

                                    }

                                }
                                else if (slotEnd.Hour == employeeEndTime.TimeOfDay.Hours)
                                {
                                    if (slotEnd.Minute < employeeEndTime.TimeOfDay.Minutes)
                                    {
                                        if (employeeStartTime.Date.ToString("yyyy-MM-dd") == currentTime.ToString("yyyy-MM-dd"))
                                        {
                                            if (slotStart.TimeOfDay >= currentTime.TimeOfDay)
                                            {
                                                availableSlots.Add(slotStart.ToString("HH:mm") + " - " + slotEnd.ToString("HH:mm"));

                                            }
                                        }
                                        else
                                        {
                                            availableSlots.Add(slotStart.ToString("HH:mm") + " - " + slotEnd.ToString("HH:mm"));

                                        }
                                    }
                                    else if (slotEnd.Minute == employeeEndTime.TimeOfDay.Minutes)
                                    {
                                        if (employeeStartTime.Date.ToString("yyyy-MM-dd") == currentTime.ToString("yyyy-MM-dd"))
                                        {
                                            if (slotStart.TimeOfDay >= currentTime.TimeOfDay)
                                            {
                                                availableSlots.Add(slotStart.ToString("HH:mm") + " - " + slotEnd.ToString("HH:mm"));

                                            }
                                        }
                                        else
                                        {
                                            availableSlots.Add(slotStart.ToString("HH:mm") + " - " + slotEnd.ToString("HH:mm"));

                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            return availableSlots;
        }

        public EmployeePriceChange GetPriceChange(int EmployeeID, DateTime SelectedDate, TimeSpan slotStart, TimeSpan slotEnd)
        {
            bool ChangeFound = false;
            int ChangeID = 0;
            string TypeOfChange = "";
            var discountpercentage = 0.0;
            var priceChanges = EmployeePriceChangeServices.Instance.GetEmployeePriceChange(EmployeeID, "");
            if (priceChanges.Count() > 0)
            {
                foreach (var item in priceChanges)
                {

                    if (item.StartDate.Date <= SelectedDate.Date && item.EndDate.Date >= SelectedDate.Date
                        && item.StartDate.TimeOfDay <= slotStart && item.EndDate.TimeOfDay >= slotEnd)
                    {

                        discountpercentage = item.Percentage;
                        ChangeFound = true;
                        ChangeID = item.ID;
                        TypeOfChange = item.TypeOfChange;
                        //ListOfTimeSlotsWithDiscount.Add(new TimeSlotModel { TimeSlot = timeSlot, TypeOfChange = item.TypeOfChange, HaveDiscount = true, Percentage = discountpercentage,PriceChangeID = item.ID });
                        break;

                    }
                    else
                    {

                        //ListOfTimeSlotsWithDiscount.Add(new TimeSlotModel { TimeSlot = timeSlot, TypeOfChange = item.TypeOfChange, HaveDiscount = false, Percentage = discountpercentage, PriceChangeID = 0 });
                        ChangeFound = false;
                    }


                }
            }
            else
            {
                //ListOfTimeSlotsWithDiscount.Add(new TimeSlotModel { TimeSlot = timeSlot, TypeOfChange = item.TypeOfChange, HaveDiscount = false, Percentage = discountpercentage,PriceChangeID = 0 });
                ChangeFound = false;

            }

            if (ChangeFound)
            {
                var employeePriceChange = new EmployeePriceChange();
                employeePriceChange.EmployeeID = EmployeeID;
                employeePriceChange.ID = ChangeID;
                employeePriceChange.Percentage = float.Parse(discountpercentage.ToString());
                employeePriceChange.TypeOfChange = TypeOfChange;
                return employeePriceChange;
            }
            else
            {
                var employeePriceChange = new EmployeePriceChange();
                return employeePriceChange;

            }

        }



        [HttpGet]
        public JsonResult CheckRunTimeSlot(string business, string timeslot, string Date, string serviceDuration, int EmployeeID, string serviceIDs)
        {

            var services = serviceIDs.Split(',').ToList();

            var SelectedDate = DateTime.Parse(Date);
            var TimeSlot = timeslot.Replace("at", "").Trim();
            int serviceDurationMinutes = 0;
            var serviceduration = serviceDuration.Split(':')[1].Trim();
            // Parse the initial time
            DateTime firsthalf = DateTime.ParseExact(TimeSlot, "HH:mm", null);

            // Add the service duration in minutes

            // Format the result as a string

            string numberOnly = new string(serviceduration.TakeWhile(char.IsDigit).ToArray());
            if (serviceDuration != "")
            {
                foreach (var item in serviceDuration.Split(',').ToList())
                {
                    serviceDurationMinutes += int.Parse(numberOnly);

                }
            }
            bool isSlotAvailable = false;

            DateTime secondHalf = firsthalf.AddMinutes(serviceDurationMinutes);


            string result = $"{firsthalf:HH:mm} - {secondHalf:HH:mm}";
            var empShifts = new List<ShiftModel>();
            var Company = CompanyServices.Instance.GetCompany().Where(x => x.Business == business).FirstOrDefault();

            var appointments = AppointmentServices.Instance.GetAppointmentBookingWRTBusiness(SelectedDate.Day, SelectedDate.Month, SelectedDate.Year, EmployeeID, false, false).ToList();


            var ListOfTimeSlotsWithDiscount = new List<TimeSlotModel>();

            var roster = TimeTableRosterServices.Instance.GetTimeTableRosterByEmpID(EmployeeID);
            var ListOfEmployeeSlotsCount = new List<SlotsListWithEmployeeIDModel>();

            if (roster != null)
            {
                var shifts = ShiftServices.Instance.GetShiftWRTBusiness(Company.Business, EmployeeID);
                foreach (var shift in shifts)
                {
                    var recurringShifts = RecurringShiftServices.Instance.GetRecurringShiftWRTBusiness(Company.Business, shift.ID);
                    if (recurringShifts != null)
                    {
                        if (recurringShifts.RecurEnd == "Custom Date" && recurringShifts.RecurEndDate != "")
                        {
                            if (IsDateInRangeNew(DateTime.Parse(recurringShifts.RecurEndDate), SelectedDate))
                            {

                                if (recurringShifts.Frequency == "Bi-Weekly")
                                {

                                    if (GetNextDayStatus(SelectedDate, shift.Date, shift.Day.ToString()) == "YES")
                                    {
                                        var exceptionShift = ExceptionShiftServices.Instance.GetExceptionShiftWRTBusiness(Company.Business, shift.ID);
                                        empShifts.Add(new ShiftModel { Shift = shift, RecurShift = recurringShifts, ExceptionShift = exceptionShift });
                                    }


                                }
                                else
                                {
                                    var exceptionShift = ExceptionShiftServices.Instance.GetExceptionShiftWRTBusiness(Company.Business, shift.ID);
                                    empShifts.Add(new ShiftModel { Shift = shift, RecurShift = recurringShifts, ExceptionShift = exceptionShift });
                                }
                            }
                        }
                        else
                        {
                            if (recurringShifts.Frequency == "Bi-Weekly")
                            {
                                if (roster != null)
                                {
                                    if (GetNextDayStatus(SelectedDate, shift.Date, shift.Day.ToString()) == "YES")
                                    {
                                        var exceptionShift = ExceptionShiftServices.Instance.GetExceptionShiftWRTBusiness(Company.Business, shift.ID);
                                        empShifts.Add(new ShiftModel { Shift = shift, RecurShift = recurringShifts, ExceptionShift = exceptionShift });
                                    }
                                }

                            }
                            else
                            {
                                var exceptionShift = ExceptionShiftServices.Instance.GetExceptionShiftWRTBusiness(Company.Business, shift.ID);
                                empShifts.Add(new ShiftModel { Shift = shift, RecurShift = recurringShifts, ExceptionShift = exceptionShift });
                            }
                        }
                    }
                    else
                    {
                        var exceptionShiftByShiftID = ExceptionShiftServices.Instance.GetExceptionShiftWRTBusinessAndShift(Company.Business, shift.ID);
                        if (exceptionShiftByShiftID != null)
                        {
                            empShifts.Add(new ShiftModel { Shift = shift, RecurShift = recurringShifts, ExceptionShift = exceptionShiftByShiftID });
                        }
                        else
                        {
                            empShifts.Add(new ShiftModel { Shift = shift, RecurShift = recurringShifts });

                        }

                    }
                }

                var usethisShift = empShifts.Where(x => x.RecurShift != null && x.Shift.IsRecurring && x.Shift.Day == SelectedDate.DayOfWeek.ToString() && x.Shift.Date.Date <= SelectedDate.Date).Select(X => X.Shift).FirstOrDefault();
                var useExceptionShifts = empShifts
                    .Where(x => x.ExceptionShift != null && x.ExceptionShift.Count() > 0 && !x.Shift.IsRecurring)
                    .SelectMany(x => x.ExceptionShift);

                if (usethisShift == null && useExceptionShifts == null)
                {
                    //okay bye
                }
                else
                {
                    if (usethisShift != null)
                    {
                        bool FoundExceptionToo = false;
                        var FoundedExceptionShift = new ExceptionShift();
                        var empShiftsCount = empShifts.Count();
                        if (empShiftsCount > 0) // Ensure there's at least one element in empShifts
                        {
                            for (int i = 0; i < empShiftsCount; i++)
                            {
                                var firstEmpShift = empShifts[i]; // Accessing empShifts at index i
                                foreach (var item in firstEmpShift.ExceptionShift.ToList())
                                {
                                    if (item.ShiftID == firstEmpShift.Shift.ID && item.ExceptionDate.ToString("yyyy-MM-dd") == SelectedDate.ToString("yyyy-MM-dd") && item.IsNotWorking == false)
                                    {
                                        FoundExceptionToo = true;
                                        FoundedExceptionShift = item;
                                        break;
                                    }
                                    if (item.ShiftID == firstEmpShift.Shift.ID && item.ExceptionDate.ToString("yyyy-MM-dd") == SelectedDate.ToString("yyyy-MM-dd") && item.IsNotWorking == true)
                                    {
                                        FoundExceptionToo = true;
                                        FoundedExceptionShift = item;
                                        break;
                                    }
                                }


                            }

                        }
                        if (FoundExceptionToo)
                        {

                            if (FoundedExceptionShift.ID != 0 && !FoundedExceptionShift.IsNotWorking)
                            {
                                DateTime startTime = SelectedDate.Date
                               .AddHours(DateTime.ParseExact(FoundedExceptionShift.StartTime, "H:mm", CultureInfo.InvariantCulture).Hour)
                               .AddMinutes(DateTime.ParseExact(FoundedExceptionShift.StartTime, "H:mm", CultureInfo.InvariantCulture).Minute);

                                DateTime endTime = SelectedDate.Date
                                    .AddHours(DateTime.ParseExact(FoundedExceptionShift.EndTime, "H:mm", CultureInfo.InvariantCulture).Hour)
                                    .AddMinutes(DateTime.ParseExact(FoundedExceptionShift.EndTime, "H:mm", CultureInfo.InvariantCulture).Minute);


                                var CheckSlots = new List<string>();



                                CheckSlots = FindAvailableSlots(startTime, endTime, appointments, serviceDurationMinutes, Company, services, EmployeeID);



                                if (CheckSlots.Count() != 0)
                                {
                                    if (CheckSlots.Where(x => x == result).Any())
                                    {
                                        isSlotAvailable = true;
                                    }
                                }
                            }



                        }
                        else
                        {
                            DateTime startTime = SelectedDate.Date
                                       .AddHours(DateTime.ParseExact(usethisShift.StartTime, "H:mm", CultureInfo.InvariantCulture).Hour)
                                       .AddMinutes(DateTime.ParseExact(usethisShift.StartTime, "H:mm", CultureInfo.InvariantCulture).Minute);

                            DateTime endTime = SelectedDate.Date
                                .AddHours(DateTime.ParseExact(usethisShift.EndTime, "H:mm", CultureInfo.InvariantCulture).Hour)
                                .AddMinutes(DateTime.ParseExact(usethisShift.EndTime, "H:mm", CultureInfo.InvariantCulture).Minute);

                            var CheckSlots = new List<string>();

                            if (usethisShift.IsRecurring)
                            {
                                var recurringShift = RecurringShiftServices.Instance.GetRecurringShiftWRTBusiness(Company.Business, usethisShift.ID);
                                if (recurringShift != null)
                                {
                                    if (recurringShift.RecurEnd == "Never")
                                    {
                                        if (recurringShift.Frequency == "Bi-Weekly")
                                        {
                                            if (GetNextDayStatus(SelectedDate, usethisShift.Date, SelectedDate.DayOfWeek.ToString()) == "YES")
                                            {
                                                CheckSlots = FindAvailableSlots(startTime, endTime, appointments, serviceDurationMinutes, Company, services, EmployeeID);
                                                if (CheckSlots.Count() != 0)
                                                {
                                                    if (CheckSlots.Where(x => x == result).Any())
                                                    {
                                                        isSlotAvailable = true;
                                                    }
                                                }
                                            }
                                        }
                                        else
                                        {
                                            CheckSlots = FindAvailableSlots(startTime, endTime, appointments, serviceDurationMinutes, Company, services, EmployeeID);
                                            if (CheckSlots.Count() != 0)
                                            {
                                                if (CheckSlots.Where(x => x == result).Any())
                                                {
                                                    isSlotAvailable = true;
                                                }
                                            }
                                        }
                                    }
                                    else
                                    {
                                        if (DateTime.Parse(recurringShift.RecurEndDate) >= SelectedDate)
                                        {
                                            if (recurringShift.Frequency == "Bi-Weekly")
                                            {
                                                if (GetNextDayStatus(SelectedDate, usethisShift.Date, SelectedDate.DayOfWeek.ToString()) == "YES")
                                                {
                                                    CheckSlots = FindAvailableSlots(startTime, endTime, appointments, serviceDurationMinutes, Company, services, EmployeeID);
                                                    if (CheckSlots.Count() != 0)
                                                    {
                                                        if (CheckSlots.Where(x => x == result).Any())
                                                        {
                                                            isSlotAvailable = true;
                                                        }
                                                    }
                                                }
                                            }
                                            else
                                            {
                                                CheckSlots = FindAvailableSlots(startTime, endTime, appointments, serviceDurationMinutes, Company, services, EmployeeID);
                                                if (CheckSlots.Count() != 0)
                                                {
                                                    if (CheckSlots.Where(x => x == result).Any())
                                                    {
                                                        isSlotAvailable = true;
                                                    }
                                                }
                                            }

                                        }
                                    }




                                }

                            }
                            else
                            {
                                CheckSlots = FindAvailableSlots(startTime, endTime, appointments, serviceDurationMinutes, Company, services, EmployeeID);
                                if (CheckSlots.Count() != 0)
                                {
                                    if (CheckSlots.Where(x => x == result).Any())
                                    {
                                        isSlotAvailable = true;
                                    }
                                }
                            }



                        }

                    }
                    else
                    {

                        if (useExceptionShifts != null)
                        {
                            bool FoundExceptionToo = false;
                            var FoundedExceptionShift = new ExceptionShift();
                            var empShiftsCount = empShifts.Count();
                            if (empShiftsCount > 0) // Ensure there's at least one element in empShifts
                            {
                                for (int i = 0; i < empShiftsCount; i++)
                                {
                                    var firstEmpShift = empShifts[i]; // Accessing empShifts at index i
                                    foreach (var item in firstEmpShift.ExceptionShift.ToList())
                                    {
                                        if (item.ExceptionDate.ToString("yyyy-MM-dd") == SelectedDate.ToString("yyyy-MM-dd") && item.IsNotWorking == false)
                                        {
                                            FoundExceptionToo = true;
                                            FoundedExceptionShift = item;
                                            break;
                                        }
                                        if (item.ExceptionDate.ToString("yyyy-MM-dd") == SelectedDate.ToString("yyyy-MM-dd") && item.IsNotWorking == true)
                                        {
                                            FoundExceptionToo = true;
                                            FoundedExceptionShift = item;
                                            break;
                                        }
                                    }


                                }

                            }
                            if (FoundExceptionToo)
                            {

                                if (FoundedExceptionShift.ID != 0 && !FoundedExceptionShift.IsNotWorking)
                                {
                                    DateTime startTime = SelectedDate.Date
                                   .AddHours(DateTime.ParseExact(FoundedExceptionShift.StartTime, "H:mm", CultureInfo.InvariantCulture).Hour)
                                   .AddMinutes(DateTime.ParseExact(FoundedExceptionShift.StartTime, "H:mm", CultureInfo.InvariantCulture).Minute);

                                    DateTime endTime = SelectedDate.Date
                                        .AddHours(DateTime.ParseExact(FoundedExceptionShift.EndTime, "H:mm", CultureInfo.InvariantCulture).Hour)
                                        .AddMinutes(DateTime.ParseExact(FoundedExceptionShift.EndTime, "H:mm", CultureInfo.InvariantCulture).Minute);


                                    var CheckSlots = new List<string>();



                                    CheckSlots = FindAvailableSlots(startTime, endTime, appointments, serviceDurationMinutes, Company, services, EmployeeID);



                                    if (CheckSlots.Count() != 0)
                                    {
                                        if (CheckSlots.Where(x => x == result).Any())
                                        {
                                            isSlotAvailable = true;
                                        }
                                    }
                                }



                            }
                        }

                    }
                }

            }
            return Json(new { isSlotAvailable = isSlotAvailable }, JsonRequestBehavior.AllowGet);
        }





        [HttpPost]
        public JsonResult CheckSlotAvailability(DateTime date, DateTime time, string serviceDuration, int EmployeeID, int ID = 0)
        {
            var LoggedInUser = UserManager.FindById(User.Identity.GetUserId());
            int serviceDurationMinutes = 0;
            if (serviceDuration != "")
            {
                foreach (var item in serviceDuration.Split(',').ToList())
                {
                    serviceDurationMinutes += int.Parse(item.Replace("Mins", "").Replace("mins", "").Trim());

                }
            }
            var Appointments = new List<Appointment>();
            var company = CompanyServices.Instance.GetCompany().Where(X => X.Business == LoggedInUser.Company).FirstOrDefault();
            if (LoggedInUser.Role == "Super Admin")
            {
                if (ID == 0)
                {
                    Appointments = AppointmentServices.Instance.GetAppointment().Where(x => x.EmployeeID == EmployeeID && x.DELETED == false).ToList();
                }
                else
                {
                    Appointments = AppointmentServices.Instance.GetAppointment().Where(x => x.ID != ID && x.EmployeeID == EmployeeID && x.DELETED == false).ToList();
                }
            }
            else
            {
                if (ID == 0)
                {
                    Appointments = AppointmentServices.Instance.GetAppointmentBookingWRTBusiness(LoggedInUser.Company, false, false, EmployeeID);
                    var employeeRequest = EmployeeRequestServices.Instance.GetEmployeeRequestByBusiness(company.ID).Where(x => x.EmployeeID == EmployeeID).ToList();
                    foreach (var item in employeeRequest)
                    {
                        if (item.Accepted)
                        {
                            if (company.ID == item.CompanyIDFrom)
                            {
                                var appointment = AppointmentServices.Instance.GetAppointmentBookingWRTBusiness(item.Business, false, false, item.EmployeeID);
                                Appointments.AddRange(appointment);
                            }
                            else
                            {
                                var companyFrom = CompanyServices.Instance.GetCompany(item.CompanyIDFrom);
                                var appointment = AppointmentServices.Instance.GetAppointmentBookingWRTBusiness(companyFrom.Business, false, false, item.EmployeeID);
                                Appointments.AddRange(appointment);
                            }

                        }
                    }
                }
                else
                {
                    Appointments = AppointmentServices.Instance.GetAppointmentBookingWRTBusiness(LoggedInUser.Company, false, false, EmployeeID).Where(x => x.ID != ID).ToList();
                    var employeeRequest = EmployeeRequestServices.Instance.GetEmployeeRequestByBusiness(company.ID).Where(x => x.EmployeeID == EmployeeID).ToList();
                    foreach (var item in employeeRequest)
                    {
                        if (item.Accepted)
                        {
                            if (company.ID == item.CompanyIDFrom)
                            {
                                var appointment = AppointmentServices.Instance.GetAppointmentBookingWRTBusiness(item.Business, false, false, item.EmployeeID).Where(x => x.ID != ID).ToList();
                                Appointments.AddRange(appointment);
                            }
                            else
                            {
                                var companyFrom = CompanyServices.Instance.GetCompany(item.CompanyIDFrom);
                                var appointment = AppointmentServices.Instance.GetAppointmentBookingWRTBusiness(companyFrom.Business, false, false, item.EmployeeID).Where(x => x.ID != ID).ToList();
                                Appointments.AddRange(appointment);
                            }

                        }
                    }
                }

            }
            DateTime endTime = time.AddMinutes(serviceDurationMinutes);
            bool isSlotAvailable = true;
            foreach (var item in Appointments)
            {
                var appDate = item.Date;   //Appointment Date
                var appTime = item.Time;   //Appointment Start Time
                var appEndTime = item.EndTime; //Appointment End Time


                if (appDate.Date == date.Date)
                {

                    if (appTime.Hour == time.Hour)
                    {
                        if (appTime.Minute > time.Minute)
                        {
                            if (appTime.Minute > endTime.Minute)
                            {
                                isSlotAvailable = true;
                            }
                            else if (appTime.Minute == endTime.Minute)
                            {
                                isSlotAvailable = true;
                            }
                            else
                            {
                                isSlotAvailable = false;
                                break;
                            }
                        }
                        else if (appTime.Minute < time.Minute)
                        {
                            if (appEndTime.Minute > time.Minute)
                            {
                                isSlotAvailable = false;//Because start at Same will result in overlap
                                break;
                            }
                            else
                            {
                                isSlotAvailable = true;
                            }
                        }
                        else
                        {
                            isSlotAvailable = false;//Because start at Same will result in overlap
                            break;
                        }

                        //if (appEndTime.Minute > time.Minute)
                        //{
                        //    isSlotAvailable = true;

                        //}
                        //else
                        //{
                        //    isSlotAvailable = false;//Because start at Same will result in overlap
                        //    break;
                        //}
                    }
                    else if (appTime.Hour > time.Hour)
                    {
                        if (endTime.Hour > appTime.Hour)
                        {
                            isSlotAvailable = false;
                            break;
                        }
                        else if (endTime.Hour < appTime.Hour)
                        {
                            isSlotAvailable = true;

                        }
                        else
                        {
                            if (endTime.Minute > appTime.Minute)
                            {
                                isSlotAvailable = false;
                                break;
                            }
                            else if (endTime.Minute < appTime.Minute)
                            {
                                isSlotAvailable = true;

                            }
                            else
                            {
                                isSlotAvailable = false;
                                break;
                            }
                        }
                    }
                    else if (appTime.Hour < time.Hour)
                    {
                        if (appEndTime.Hour < time.Hour)
                        {
                            isSlotAvailable = true;
                        }
                        else if (appEndTime.Hour > time.Hour)
                        {
                            isSlotAvailable = false;
                            break;
                        }
                        else
                        {
                            if (appEndTime.Minute < time.Minute)
                            {
                                isSlotAvailable = true;

                            }
                            else if (appEndTime.Minute > time.Minute)
                            {
                                isSlotAvailable = false;
                                break;
                            }
                            else
                            {
                                isSlotAvailable = true;


                            }
                        }
                    }
                    else
                    {
                        isSlotAvailable = true;
                    }

                }

            }



            return Json(new { isSlotAvailable });

        }

        [HttpGet]
        public JsonResult GetAbsenceServices()
        {
            var LoggedInUser = UserManager.FindById(User.Identity.GetUserId());
            var ServicesList = new List<ServiceModel>();
            var company = CompanyServices.Instance.GetCompany().Where(x => x.Business == LoggedInUser.Company).FirstOrDefault();

            if (LoggedInUser.Role != "Super Admin")
            {
                var category = ServicesCategoriesServices.Instance.GetServiceCategories().Where(x => x.Business == LoggedInUser.Company && x.Name == "ABSENSE").FirstOrDefault();

                var ServicesWRTCategory = ServiceServices.Instance.GetServiceWRTCategory(LoggedInUser.Company, category.Name).Where(x => x.IsActive).ToList();
                ServicesList.Add(new ServiceModel { ServiceCategory = category, Services = ServicesWRTCategory, Company = company });

            }
            else
            {
                var category = ServicesCategoriesServices.Instance.GetServiceCategories().Where(x => x.Name == "ABSENSE").FirstOrDefault();

                var ServicesWRTCategory = ServiceServices.Instance.GetServiceWRTCategory(LoggedInUser.Company, category.Name).Where(x => x.IsActive).ToList();
                ServicesList.Add(new ServiceModel { ServiceCategory = category, Services = ServicesWRTCategory, Company = company });
            }

            return Json(ServicesList, JsonRequestBehavior.AllowGet);
        }





        [HttpGet]
        [NoCache]
        public ActionResult CancelByEmail(int AppointmentID)
        {
            BookingViewModel model = new BookingViewModel();
            var appointment = AppointmentServices.Instance.GetAppointment(AppointmentID);
            if (!appointment.IsCancelled && appointment.Status != "No Show")
            {
                var company = CompanyServices.Instance.GetCompany().Where(x => x.Business == appointment.Business).FirstOrDefault();
                DateTime combinedDateTime = new DateTime(appointment.Date.Year, appointment.Date.Month, appointment.Date.Day, appointment.Time.Hour, appointment.Time.Minute, appointment.Time.Second);
                TimeSpan difference = combinedDateTime - DateTime.Now;
                model.Appointment = appointment;

                var ServiceListCommand = appointment.Service.Split(',').ToList();
                var ServiceDuration = appointment.ServiceDuration.Split(',').ToList();
                var serviceList = new List<ServiceAppViewModel>();
                for (int i = 0; i < ServiceListCommand.Count && i < ServiceDuration.Count; i++)
                {
                    var serivce = ServiceServices.Instance.GetService(int.Parse(ServiceListCommand[i]));
                    var serviceViewModel = new ServiceAppViewModel
                    {
                        Service = serivce.Name,
                        Duration = ServiceDuration[i],
                        Price = serivce.Price,
                        Category = serivce.Category,
                        ID = serivce.ID,

                    };

                    serviceList.Add(serviceViewModel);
                }
                model.ServicesForCancellation = serviceList;
                model.Company = company;
                model.Employee = EmployeeServices.Instance.GetEmployee(appointment.EmployeeID);
                return View("CancelByEmail", "_BookingLayout", model);
            }
            else
            {
                return RedirectToAction("AppointmentCancelled", "Appointment", new { ID = AppointmentID });
            }




        }

        [HttpPost]
        public ActionResult CancelByEmailPOST(int AppointmentID)
        {
            var appointment = AppointmentServices.Instance.GetAppointment(AppointmentID);
            var company = CompanyServices.Instance.GetCompany().Where(x => x.Business == appointment.Business).FirstOrDefault();

            DateTime combinedDateTime = new DateTime(appointment.Date.Year, appointment.Date.Month, appointment.Date.Day, appointment.Time.Hour, appointment.Time.Minute, appointment.Time.Second);
            TimeSpan difference = combinedDateTime - DateTime.Now;
            var GoCancelIt = false;
            if (!appointment.IsCancelled && appointment.Status != "No Show")
            {
                if (company.CancellationTime == "1 Hour")
                {
                    if (difference.TotalHours <= 1)
                    {
                        GoCancelIt = true;
                    }
                    else
                    {
                        GoCancelIt = false;
                    }
                }
                if (company.CancellationTime == "3 Hours")
                {
                    if (difference.TotalHours <= 3)
                    {
                        GoCancelIt = true;
                    }
                    else
                    {
                        GoCancelIt = false;
                    }
                }
                if (company.CancellationTime == "5 Hours")
                {
                    if (difference.TotalHours <= 5)
                    {
                        GoCancelIt = true;
                    }
                    else
                    {
                        GoCancelIt = false;
                    }
                }
                if (company.CancellationTime == "24 Hours")
                {
                    if (difference.TotalHours <= 24)
                    {
                        GoCancelIt = true;
                    }
                    else
                    {
                        GoCancelIt = false;
                    }
                }
                if (company.CancellationTime == "48 Hours")
                {
                    if (difference.TotalHours <= 48)
                    {
                        GoCancelIt = true;
                    }
                    else
                    {
                        GoCancelIt = false;
                    }
                }

            }


            if (!GoCancelIt)
            {
                appointment.IsCancelled = true;
                appointment.CancelledByEmail = true;
                AppointmentServices.Instance.UpdateAppointment(appointment);
                var reminder = ReminderServices.Instance.GetReminderWRTAppID(appointment.ID);
                reminder.IsCancelled = true;
                ReminderServices.Instance.UpdateReminder(reminder);
                var customer = CustomerServices.Instance.GetCustomer(appointment.CustomerID);
                var employee = EmployeeServices.Instance.GetEmployee(appointment.EmployeeID);
                var ToBeInputtedIDs = new Dictionary<GoogleCalendarIntegration, string>();
                //delete previous one
                RefreshToken(appointment.Business);
                var googleKey = GoogleCalendarServices.Instance.GetGoogleCalendarServicesWRTBusiness(appointment.Business);
                ToBeInputtedIDs.Add(googleKey, employee.GoogleCalendarID);
                var employeeRequest = EmployeeRequestServices.Instance.GetEmployeeRequestsWRTBusiness(appointment.Business);
                var requestedEmployee = RequestedEmployeeServices.Instance.GetRequestedEmployeeWRTEmployeeID(employee.ID);
                if (employeeRequest.Any(x => x.EmployeeID == employee.ID))
                {
                    if (requestedEmployee != null && requestedEmployee.GoogleCalendarID != null && requestedEmployee.GoogleCalendarID != "")
                    {
                        RefreshToken(employee.Business);

                        googleKey = GoogleCalendarServices.Instance.GetGoogleCalendarServicesWRTBusiness(employee.Business);
                        ToBeInputtedIDs.Add(googleKey, requestedEmployee.GoogleCalendarID);
                    }
                }
                var historyNew = new History();
                historyNew.Business = appointment.Business;
                historyNew.CustomerName = customer.FirstName + " " + customer.LastName;
                historyNew.Date = DateTime.Now;
                historyNew.AppointmentID = appointment.ID;
                historyNew.Note = "Appointment was cancelled by Email for the client:" + historyNew.CustomerName;
                historyNew.EmployeeName = employee.Name;
                historyNew.Name = "Cancelled";
                HistoryServices.Instance.SaveHistory(historyNew);

                foreach (var item in ToBeInputtedIDs)
                {
                    if (item.Key != null && !item.Key.Disabled)
                    {
                        if (appointment.GoogleCalendarEventID != null)
                        {
                            var url = new System.Uri("https://www.googleapis.com/calendar/v3/calendars/" + item.Value + "/events/" + appointment.GoogleCalendarEventID);
                            RestClient restClient = new RestClient(url);
                            RestRequest request = new RestRequest();

                            request.AddQueryParameter("key", "AIzaSyASKpY6I08IVKFMw3muX39uMzPc5sBDaSc");
                            request.AddHeader("Authorization", "Bearer " + item.Key.AccessToken);
                            request.AddHeader("Accept", "application/json");

                            var response = restClient.Delete(request);

                        }
                    }

                }





                string ConcatenatedServices = "";
                if (appointment.Service != null)
                {
                    foreach (var item in appointment.Service.Split(',').ToList())
                    {
                        var Service = ServiceServices.Instance.GetService(int.Parse(item));
                        if (Service != null)
                        {
                            if (ConcatenatedServices == "")
                            {
                                ConcatenatedServices = String.Join(",", Service.Name);
                            }
                            else
                            {
                                ConcatenatedServices = String.Join(",", ConcatenatedServices, Service.Name);

                            }
                        }
                    }

                }
                var emailDetails = EmailTemplateServices.Instance.GetEmailTemplateWRTBusiness(company.Business, "Appointment Cancelled");
                if (emailDetails != null && emailDetails.IsActive == true)
                {
                    string emailBody = "<html><body>";
                    emailBody += "<h2 style='font-family: Arial, sans-serif;'>Appointment Cancellation</h2>";
                    emailBody += emailDetails.TemplateCode;
                    emailBody = emailBody.Replace("{{Customer_first_name}}", customer.FirstName);
                    emailBody = emailBody.Replace("{{Customer_last_name}}", customer.LastName);
                    emailBody = emailBody.Replace("{{Customer_initial}}", customer.Gender == "Male" ? "Mr." : "Ms.");
                    emailBody = emailBody.Replace("{{date}}", appointment.Date.ToString("yyyy-MM-dd"));
                    emailBody = emailBody.Replace("{{time}}", appointment.Time.ToString("H:mm"));
                    emailBody = emailBody.Replace("{{end_time}}", appointment.EndTime.ToString("H:mm"));
                    emailBody = emailBody.Replace("{{employee}}", employee.Name);
                    emailBody = emailBody.Replace("{{employee_specialization}}", employee.Specialization);
                    emailBody = emailBody.Replace("{{employee_picture}}", $"<img class='text-center' style='height:50px;width:auto;' src='{"http://app.yourbookingplatform.com" + employee.Photo}'>");

                    emailBody = emailBody.Replace("{{services}}", ConcatenatedServices);
                    emailBody = emailBody.Replace("{{company_name}}", company.Business);
                    emailBody = emailBody.Replace("{{company_email}}", company.NotificationEmail);
                    emailBody = emailBody.Replace("{{company_address}}", company.Address);
                    emailBody = emailBody.Replace("{{company_logo}}", $"<img class='text-center' style='height:50px;width:auto;' src='{"http://app.yourbookingplatform.com" + company.Logo}'>");
                    // Assuming appointment.ID is a string
                    string cancelLink = string.Format("http://app.yourbookingplatform.com/Appointment/CancelByEmail/?AppointmentID={0}", appointment.ID);
                    emailBody = emailBody.Replace("{{cancellink}}", $"<a href='{cancelLink}' class='btn btn-primary'>CANCEL/RESCHEDULE</a>");

                    emailBody = emailBody.Replace("{{company_phone}}", company.PhoneNumber);

                    emailBody += "</body></html>";


                    if (IsValidEmail(customer.Email))
                    {
                        SendEmail(customer.Email, "Appointment Cancellation", emailBody, company);
                    }
                }
                var Message = HandleRefund(appointment.PaymentSession, company.APIKEY, appointment.ID);

                var history = new History();
                history.Note = Message;
                history.CustomerName = customer.FirstName + " " + customer.LastName;
                history.AppointmentID = appointment.ID;
                history.Type = "General";
                history.Business = appointment.Business;
                history.Date = DateTime.Now;
                history.EmployeeName = employee?.Name;
                history.Name = "Appointment Refunded";
                HistoryServices.Instance.SaveHistory(history);
                return Json(new { success = true }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(new { success = false }, JsonRequestBehavior.AllowGet);
            }
        }

        public string HandleRefund(string PaymentSession, string APIKEY, int appointmentID)
        {
            try
            {
                // Set your secret API key
                StripeConfiguration.ApiKey = APIKEY;

                // Retrieve the PaymentIntent
                var paymentIntentService = new PaymentIntentService();
                var paymentIntent = paymentIntentService.Get(PaymentSession);

                // Get the associated Charge ID
                var chargeId = paymentIntent.LatestChargeId;

                if (string.IsNullOrEmpty(chargeId))
                {
                    return "No charge found for this appointment." + "AppointmentID: " + appointmentID;
                }

                // Create a refund for the charge
                var refundService = new RefundService();
                var refundOptions = new RefundCreateOptions
                {
                    Charge = chargeId,
                };

                // Optionally set the amount (in cents)

                refundOptions.Amount = paymentIntent.Amount;



                var refund = refundService.Create(refundOptions);

                return "Refund successful for Appointment ID: " + appointmentID;
            }
            catch (StripeException ex)
            {
                return "Refund Failed with Error: " + ex.Message + " AppointmentID : " + appointmentID;
            }
        }

        [HttpGet]
        public ActionResult AppointmentCancelled(int ID)
        {
            HomeViewModel model = new HomeViewModel();
            var appointment = AppointmentServices.Instance.GetAppointment(ID);
            if (appointment.Status == "No Show")
            {
                model.Error = "No Show";
            }
            else
            {
                model.Error = "";
            }
            model.Company = CompanyServices.Instance.GetCompany().Where(x => x.Business == appointment.Business).FirstOrDefault();
            return View(model);
        }


        [HttpGet]
        public ActionResult CannotCancelled(int ID)
        {
            HomeViewModel model = new HomeViewModel();
            var appointment = AppointmentServices.Instance.GetAppointment(ID);

            model.Company = CompanyServices.Instance.GetCompany().Where(x => x.Business == appointment.Business).FirstOrDefault();
            return View(model);
        }


        [HttpGet]
        public ActionResult CannotReschedule(int ID)
        {
            HomeViewModel model = new HomeViewModel();
            var appointment = AppointmentServices.Instance.GetAppointment(ID);

            model.Company = CompanyServices.Instance.GetCompany().Where(x => x.Business == appointment.Business).FirstOrDefault();
            return View(model);
        }


        public void CreateBuffer(int AppointmentID)
        {
            var appoimtment = AppointmentServices.Instance.GetAppointment(AppointmentID);
            if (appoimtment != null)
            {
                var buffers = BufferServices.Instance.GetBufferWRTBusinessList(appoimtment.Business, appoimtment.ID);
                foreach (var item in buffers)
                {
                    BufferServices.Instance.DeleteBuffer(item.ID);
                }
            }
            var serviceids = appoimtment.Service.Split(',').ToList();
            var serviceList = new List<Entities.Service>();
            var bufferLastEndTime = appoimtment.EndTime;
            foreach (var item in serviceids)
            {
                var service = ServiceServices.Instance.GetService(int.Parse(item));
                serviceList.Add(service);
                var employeeService = EmployeeServiceServices.Instance.GetEmployeeService(appoimtment.EmployeeID, service.ID);
                if (employeeService != null)
                {
                    if (employeeService.BufferEnabled)
                    {
                        var buffer = new Entities.Buffer();
                        buffer.AppointmentID = appoimtment.ID;
                        buffer.Date = appoimtment.Date;
                        buffer.Time = bufferLastEndTime;
                        buffer.EndTime = bufferLastEndTime.AddMinutes(int.Parse(employeeService.BufferTime.Replace("mins", "").Replace("min", "")));
                        buffer.Business = appoimtment.Business;
                        buffer.ServiceID = service.ID;
                        buffer.Description = "Buffer for: " + service.Name;
                        BufferServices.Instance.SaveBuffer(buffer);
                        bufferLastEndTime = buffer.EndTime;
                    }
                }

            }
        }

        [HttpPost]
        public ActionResult Action(AppointmentActionViewModel model)
        {
            try
            {

                var waitingList = WaitingListServices.Instance.GetWaitingList(model.WaitingListID);
                if (waitingList != null)
                {
                    waitingList.WaitingListStatus = "Created";
                    WaitingListServices.Instance.UpdateWaitingList(waitingList);
                }

                var LoggedInUser = UserManager.FindById(User.Identity.GetUserId()); if (LoggedInUser == null) { return RedirectToAction("Login", "Account"); }
                var company = CompanyServices.Instance.GetCompany(LoggedInUser.Company).FirstOrDefault();
                if (model.ID == 0)
                {
                    var customer = CustomerServices.Instance.GetCustomer(model.CustomerID);
                    if (model.IsWaitingList)
                    {
                        if (model.AllEmployees == true)
                        {
                            var AllEmployees = EmployeeServices.Instance.GetEmployeeWRTBusiness(LoggedInUser.Company, true).OrderBy(x => x.DisplayOrder).ToList();
                            foreach (var emp in AllEmployees)
                            {
                                var waitinglist = new WaitingList();
                                waitinglist.Business = LoggedInUser.Company;
                                waitinglist.BookingDate = DateTime.Now;
                                waitinglist.Date = model.Date;
                                waitinglist.Time = model.Time;
                                waitinglist.EmployeeID = emp.ID;


                                waitinglist.Notes = model.Notes;

                                waitinglist.CustomerID = model.CustomerID;

                                string ConcatenatedServices = "";
                                int MinsToBeAddedForEndTime = 0;
                                float TotalCost = 0;
                                if (model.Service != null)
                                {
                                    foreach (var item in model.Service.Split(',').ToList())
                                    {
                                        var Service = ServiceServices.Instance.GetService(int.Parse(item));
                                        if (Service != null)
                                        {
                                            TotalCost += Service.Price;
                                            if (ConcatenatedServices == "")
                                            {
                                                ConcatenatedServices = String.Join(",", Service.Name);
                                            }
                                            else
                                            {
                                                ConcatenatedServices = String.Join(",", ConcatenatedServices, Service.Name);

                                            }
                                        }
                                    }
                                    foreach (var item in model.ServiceDuration.Split(',').ToList())
                                    {
                                        MinsToBeAddedForEndTime += int.Parse(item.Replace("Mins", "").Replace("mins", "").Trim());

                                    }
                                }
                                waitinglist.Service = model.Service;
                                if (model.ServiceDiscount == null)
                                {
                                    // Split the ServiceDuration string by commas
                                    string[] durations = model.ServiceDuration.Split(',');

                                    // Get the number of values
                                    int numberOfServices = durations.Length;

                                    // Create a string with zeros for each service
                                    string zeros = string.Join(",", Enumerable.Repeat("0", numberOfServices));

                                    // Save the zeros string to ServiceDiscount
                                    waitinglist.ServiceDiscount = zeros;
                                }
                                else
                                {
                                    waitinglist.ServiceDiscount = model.ServiceDiscount;
                                }
                                waitinglist.ServiceDuration = model.ServiceDuration;
                                waitinglist.TotalCost = TotalCost - model.Deposit;
                                if (ServiceServices.Instance.GetService(int.Parse(model.Service.Split(',').FirstOrDefault())).Name == "Break")
                                {
                                    waitinglist.Color = "darkgray";
                                }
                                else
                                {
                                    waitinglist.Color = "#F79700";

                                }
                                WaitingListServices.Instance.SaveWaitingList(waitinglist);

                                var history = new History();
                                history.Business = LoggedInUser.Company;
                                history.CustomerName = customer.FirstName + " " + customer.LastName;
                                history.Date = DateTime.Now;
                                history.Note = "Waiting List Created for:" + history.CustomerName + " by " + LoggedInUser.Name;
                                history.EmployeeName = EmployeeServices.Instance.GetEmployee(emp.ID).Name;
                                history.Name = "Waiting List Created";
                                HistoryServices.Instance.SaveHistory(history);
                            }
                            return Json(new { success = true }, JsonRequestBehavior.AllowGet);
                        }
                        else
                        {
                            var waitinglist = new WaitingList();
                            waitinglist.Business = LoggedInUser.Company;
                            waitinglist.BookingDate = DateTime.Now;
                            waitinglist.Date = model.Date;
                            waitinglist.Time = model.Time;
                            waitinglist.EmployeeID = model.EmployeeID;


                            waitinglist.Notes = model.Notes;

                            waitinglist.CustomerID = model.CustomerID;

                            string ConcatenatedServices = "";
                            int MinsToBeAddedForEndTime = 0;
                            float TotalCost = 0;
                            if (model.Service != null)
                            {
                                foreach (var item in model.Service.Split(',').ToList())
                                {
                                    var Service = ServiceServices.Instance.GetService(int.Parse(item));
                                    if (Service != null)
                                    {
                                        TotalCost += Service.Price;
                                        if (ConcatenatedServices == "")
                                        {
                                            ConcatenatedServices = String.Join(",", Service.Name);
                                        }
                                        else
                                        {
                                            ConcatenatedServices = String.Join(",", ConcatenatedServices, Service.Name);

                                        }
                                    }
                                }
                                foreach (var item in model.ServiceDuration.Split(',').ToList())
                                {
                                    MinsToBeAddedForEndTime += int.Parse(item.Replace("Mins", "").Replace("mins", "").Trim());

                                }
                            }
                            waitinglist.Service = model.Service;
                            if (model.ServiceDiscount == null)
                            {
                                // Split the ServiceDuration string by commas
                                string[] durations = model.ServiceDuration.Split(',');

                                // Get the number of values
                                int numberOfServices = durations.Length;

                                // Create a string with zeros for each service
                                string zeros = string.Join(",", Enumerable.Repeat("0", numberOfServices));

                                // Save the zeros string to ServiceDiscount
                                waitinglist.ServiceDiscount = zeros;
                            }
                            else
                            {
                                waitinglist.ServiceDiscount = model.ServiceDiscount;
                            }
                            waitinglist.ServiceDuration = model.ServiceDuration;
                            waitinglist.TotalCost = TotalCost - model.Deposit;
                            if (ServiceServices.Instance.GetService(int.Parse(model.Service.Split(',').FirstOrDefault())).Name == "Break")
                            {
                                waitinglist.Color = "darkgray";
                            }
                            else
                            {
                                waitinglist.Color = "#F79700";

                            }
                            waitinglist.NonSelectedEmployee = false;
                            WaitingListServices.Instance.SaveWaitingList(waitinglist);

                            var history = new History();
                            history.Business = LoggedInUser.Company;
                            history.CustomerName = customer.FirstName + " " + customer.LastName;
                            history.Date = DateTime.Now;
                            history.Note = "Waiting List Created for:" + history.CustomerName + " by " + LoggedInUser.Name;
                            history.Name = "Waiting List Created";
                            history.EmployeeName = EmployeeServices.Instance.GetEmployee(model.EmployeeID).Name;
                            HistoryServices.Instance.SaveHistory(history);


                            return Json(new { success = true }, JsonRequestBehavior.AllowGet);
                        }
                    }
                    else
                    {
                        if (model.AllEmployees == true)
                        {
                            var AllEmployees = EmployeeServices.Instance.GetEmployeeWRTBusiness(LoggedInUser.Company, true).OrderBy(x => x.DisplayOrder).ToList();
                            foreach (var emp in AllEmployees)
                            {
                                var appointment = new Appointment();
                                appointment.Business = LoggedInUser.Company;
                                appointment.BookingDate = DateTime.Now;
                                appointment.Date = model.Date;
                                appointment.Time = model.Time;
                                appointment.Deposit = model.Deposit;
                                appointment.DepositMethod = model.DepositMethod;
                                appointment.EmployeeID = emp.ID;
                                appointment.Days = model.Days;
                                appointment.Frequency = model.Frequency;
                                appointment.Discount = model.Discount;
                                if (appointment.Frequency == "Every Week")
                                {
                                    appointment.Every = model.EveryWeek;
                                    if (model.EndsWeek == "NumberOfTimes")
                                    {
                                        appointment.Ends = model.EndsWeek + "_" + model.EndsNumberOfTimesWeek;

                                    }
                                    else if (model.EndsWeek == "Specific Date")
                                    {
                                        appointment.Ends = model.EndsWeek + "_" + model.EndsDateWeek.ToString();
                                    }
                                    else
                                    {
                                        appointment.Ends = model.EndsWeek;
                                    }
                                }
                                else if (appointment.Frequency == "Every Day")
                                {
                                    appointment.Every = model.EveryDay;
                                    if (model.EndsDay == "NumberOfTimes")
                                    {
                                        appointment.Ends = model.EndsDay + "_" + model.EndsNumberOfTimesDay;

                                    }
                                    else if (model.EndsDay == "Specific Date")
                                    {
                                        appointment.Ends = model.EndsDay + "_" + model.EndsDateDay.ToString();
                                    }
                                    else
                                    {
                                        appointment.Ends = model.EndsDay;
                                    }
                                }
                                else if (appointment.Frequency == "Every Month")
                                {
                                    appointment.Every = model.EveryMonth;
                                    if (model.EndsMonth == "NumberOfTimes")
                                    {
                                        appointment.Ends = model.EndsMonth + "_" + model.EndsNumberOfTimesMonth;

                                    }
                                    else if (model.EndsMonth == "Specific Date")
                                    {
                                        appointment.Ends = model.EndsMonth + "_" + model.EndsDateMonth.ToString();
                                    }
                                    else
                                    {
                                        appointment.Ends = model.EndsMonth;
                                    }
                                }
                                else
                                {
                                    appointment.Every = "";
                                    appointment.Ends = "Never";
                                }

                                appointment.Notes = model.Notes;
                                appointment.IsRepeat = model.IsRepeat;

                                appointment.CustomerID = model.CustomerID;
                                if (appointment.CustomerID == 0)
                                {
                                    appointment.IsWalkIn = true;

                                }
                                else
                                {
                                    appointment.IsWalkIn = false;
                                }
                                appointment.IsPaid = true;
                                appointment.Status = "Pending";
                                string ConcatenatedServices = "";
                                int MinsToBeAddedForEndTime = 0;
                                float TotalCost = 0;
                                if (model.Service != null)
                                {
                                    foreach (var item in model.Service.Split(',').ToList())
                                    {
                                        var Service = ServiceServices.Instance.GetService(int.Parse(item));
                                        if (Service != null)
                                        {
                                            TotalCost += Service.Price;
                                            if (ConcatenatedServices == "")
                                            {
                                                ConcatenatedServices = String.Join(",", Service.Name);
                                            }
                                            else
                                            {
                                                ConcatenatedServices = String.Join(",", ConcatenatedServices, Service.Name);

                                            }
                                        }
                                    }
                                    foreach (var item in model.ServiceDuration.Split(',').ToList())
                                    {
                                        MinsToBeAddedForEndTime += int.Parse(item.Replace("Mins", "").Replace("mins", "").Trim());

                                    }
                                }
                                appointment.EndTime = appointment.Time.AddMinutes(MinsToBeAddedForEndTime);
                                appointment.Service = model.Service;
                                if (model.ServiceDiscount == null)
                                {
                                    // Split the ServiceDuration string by commas
                                    string[] durations = model.ServiceDuration.Split(',');

                                    // Get the number of values
                                    int numberOfServices = durations.Length;

                                    // Create a string with zeros for each service
                                    string zeros = string.Join(",", Enumerable.Repeat("0", numberOfServices));

                                    // Save the zeros string to ServiceDiscount
                                    appointment.ServiceDiscount = zeros;
                                }
                                else
                                {
                                    appointment.ServiceDiscount = model.ServiceDiscount;
                                }
                                appointment.ServiceDuration = model.ServiceDuration;
                                appointment.TotalCost = TotalCost - model.Deposit;
                                if (ServiceServices.Instance.GetService(int.Parse(model.Service.Split(',').FirstOrDefault())).Name == "Break")
                                {
                                    appointment.Color = "darkgray";
                                }
                                else
                                {
                                    appointment.Color = "#F79700";

                                }
                                AppointmentServices.Instance.SaveAppointment(appointment);
                                CreateBuffer(appointment.ID);
                                if (appointment.CustomerID != 0)
                                {
                                    var reminder = new Entities.Reminder();
                                    reminder.Service = appointment.Service;
                                    reminder.Business = appointment.Business;
                                    DateTime combinedDateTime = new DateTime(appointment.Date.Year, appointment.Date.Month, appointment.Date.Day, appointment.Time.Hour, appointment.Time.Minute, appointment.Time.Second);
                                    reminder.Date = combinedDateTime;
                                    reminder.CustomerID = appointment.CustomerID;
                                    reminder.IsCancelled = false;
                                    reminder.Paid = true;
                                    reminder.AppointmentID = appointment.ID;
                                    reminder.EmployeeID = appointment.EmployeeID;

                                    ReminderServices.Instance.SaveReminder(reminder);

                                }

                                var history = new History();
                                history.Business = LoggedInUser.Company;
                                history.CustomerName = "Walk In";
                                history.Date = DateTime.Now;
                                history.AppointmentID = appointment.ID;
                                history.Note = "Offline Appointment Created for:" + history.CustomerName + " by " + LoggedInUser.Name + " ID: " + appointment.ID;
                                history.EmployeeName = EmployeeServices.Instance.GetEmployee(emp.ID).Name;
                                history.Name = "Offline Appointment Created";
                                HistoryServices.Instance.SaveHistory(history);
                                var googleCalendar = GoogleCalendarServices.Instance.GetGoogleCalendarServicesWRTBusiness(LoggedInUser.Company);
                                if (googleCalendar != null && !googleCalendar.Disabled)
                                {
                                    GenerateonGoogleCalendar(appointment.ID, ConcatenatedServices, 0, "SAVING");
                                }
                            }
                            return Json(new { success = true }, JsonRequestBehavior.AllowGet);
                        }
                        else
                        {
                            var appointment = new Appointment();
                            appointment.Business = LoggedInUser.Company;
                            appointment.BookingDate = DateTime.Now;
                            appointment.Date = model.Date;
                            appointment.Time = model.Time;
                            appointment.Deposit = model.Deposit;
                            appointment.DepositMethod = model.DepositMethod;
                            appointment.EmployeeID = model.EmployeeID;
                            appointment.Days = model.Days;
                            appointment.Frequency = model.Frequency;
                            appointment.Discount = model.Discount;
                            if (appointment.Frequency == "Every Week")
                            {
                                appointment.Every = model.EveryWeek;
                                if (model.EndsWeek == "NumberOfTimes")
                                {
                                    appointment.Ends = model.EndsWeek + "_" + model.EndsNumberOfTimesWeek;

                                }
                                else if (model.EndsWeek == "Specific Date")
                                {
                                    appointment.Ends = model.EndsWeek + "_" + model.EndsDateWeek.ToString();
                                }
                                else
                                {
                                    appointment.Ends = model.EndsWeek;
                                }
                            }
                            else if (appointment.Frequency == "Every Day")
                            {
                                appointment.Every = model.EveryDay;
                                if (model.EndsDay == "NumberOfTimes")
                                {
                                    appointment.Ends = model.EndsDay + "_" + model.EndsNumberOfTimesDay;

                                }
                                else if (model.EndsDay == "Specific Date")
                                {
                                    appointment.Ends = model.EndsDay + "_" + model.EndsDateDay.ToString();
                                }
                                else
                                {
                                    appointment.Ends = model.EndsDay;
                                }
                            }
                            else if (appointment.Frequency == "Every Month")
                            {
                                appointment.Every = model.EveryMonth;
                                if (model.EndsMonth == "NumberOfTimes")
                                {
                                    appointment.Ends = model.EndsMonth + "_" + model.EndsNumberOfTimesMonth;

                                }
                                else if (model.EndsMonth == "Specific Date")
                                {
                                    appointment.Ends = model.EndsMonth + "_" + model.EndsDateMonth.ToString();
                                }
                                else
                                {
                                    appointment.Ends = model.EndsMonth;
                                }
                            }
                            else
                            {
                                appointment.Every = "";
                                appointment.Ends = "Never";
                            }

                            appointment.Notes = model.Notes;
                            appointment.IsRepeat = model.IsRepeat;

                            appointment.CustomerID = model.CustomerID;
                            if (appointment.CustomerID == 0)
                            {
                                appointment.IsWalkIn = true;

                            }
                            else
                            {
                                appointment.IsWalkIn = false;
                            }
                            appointment.IsPaid = true;
                            string ConcatenatedServices = "";
                            int MinsToBeAddedForEndTime = 0;
                            float TotalCost = 0;
                            if (model.Service != null)
                            {
                                foreach (var item in model.Service.Split(',').ToList())
                                {
                                    var Service = ServiceServices.Instance.GetService(int.Parse(item));
                                    if (Service != null)
                                    {
                                        TotalCost += Service.Price;
                                        if (ConcatenatedServices == "")
                                        {
                                            ConcatenatedServices = String.Join(",", Service.Name);
                                        }
                                        else
                                        {
                                            ConcatenatedServices = String.Join(",", ConcatenatedServices, Service.Name);

                                        }
                                    }
                                }
                                foreach (var item in model.ServiceDuration.Split(',').ToList())
                                {
                                    MinsToBeAddedForEndTime += int.Parse(item.Replace("Mins", "").Replace("mins", "").Trim());

                                }
                            }
                            appointment.EndTime = appointment.Time.AddMinutes(MinsToBeAddedForEndTime);
                            appointment.Service = model.Service;
                            if (model.ServiceDiscount == null)
                            {
                                // Split the ServiceDuration string by commas
                                string[] durations = model.ServiceDuration.Split(',');

                                // Get the number of values
                                int numberOfServices = durations.Length;

                                // Create a string with zeros for each service
                                string zeros = string.Join(",", Enumerable.Repeat("0", numberOfServices));

                                // Save the zeros string to ServiceDiscount
                                appointment.ServiceDiscount = zeros;
                            }
                            else
                            {
                                appointment.ServiceDiscount = model.ServiceDiscount;
                            }
                            appointment.ServiceDuration = model.ServiceDuration;
                            appointment.TotalCost = TotalCost - model.Deposit;
                            if (ServiceServices.Instance.GetService(int.Parse(model.Service.Split(',').FirstOrDefault())).Name == "Break")
                            {
                                appointment.Color = "darkgray";
                            }
                            else
                            {
                                appointment.Color = "#F79700";

                            }
                            appointment.Status = "Pending";

                            var employee = EmployeeServices.Instance.GetEmployee(model.EmployeeID);
                            appointment.FromGCAL = false;
                            AppointmentServices.Instance.SaveAppointment(appointment);
                            CreateBuffer(appointment.ID);
                            if (appointment.CustomerID != 0)
                            {
                                var reminder = new Entities.Reminder();
                                reminder.Service = appointment.Service;
                                reminder.Business = appointment.Business;
                                DateTime combinedDateTime = new DateTime(appointment.Date.Year, appointment.Date.Month, appointment.Date.Day, appointment.Time.Hour, appointment.Time.Minute, appointment.Time.Second);
                                reminder.Date = combinedDateTime;
                                reminder.CustomerID = appointment.CustomerID;
                                reminder.EmployeeID = appointment.EmployeeID;
                                reminder.AppointmentID = appointment.ID;
                                reminder.IsCancelled = false;
                                reminder.Paid = true;
                                ReminderServices.Instance.SaveReminder(reminder);

                            }

                            if (company == null)
                            {
                                company = CompanyServices.Instance.GetCompany().Where(x => x.Business.Trim() == LoggedInUser.Company.Trim()).FirstOrDefault();
                            }

                            if (customer != null)
                            {
                                var emailDetails = EmailTemplateServices.Instance.GetEmailTemplateWRTBusiness(LoggedInUser.Company, "Appointment Confirmation");
                                if (emailDetails != null && emailDetails.IsActive == true)
                                {
                                    string emailBody = "<html><body>";
                                    emailBody += "<h2 style='font-family: Arial, sans-serif;'>Appointment Confirmation</h2>";
                                    emailBody += emailDetails.TemplateCode;
                                    emailBody = emailBody.Replace("{{Customer_first_name}}", customer.FirstName);
                                    emailBody = emailBody.Replace("{{Customer_last_name}}", customer.LastName);
                                    emailBody = emailBody.Replace("{{Customer_initial}}", customer.Gender == "Male" ? "Mr." : "Ms.");
                                    emailBody = emailBody.Replace("{{date}}", appointment.Date.ToString("yyyy-MM-dd"));
                                    emailBody = emailBody.Replace("{{time}}", appointment.Time.ToString("H:mm"));
                                    emailBody = emailBody.Replace("{{end_time}}", appointment.EndTime.ToString("H:mm"));
                                    emailBody = emailBody.Replace("{{employee}}", employee.Name);
                                    emailBody = emailBody.Replace("{{employee_specialization}}", employee.Specialization);
                                    emailBody = emailBody.Replace("{{employee_picture}}", $"<img class='text-center' style='height:50px;width:auto;' src='{"http://app.yourbookingplatform.com" + employee.Photo}'>");

                                    emailBody = emailBody.Replace("{{services}}", ConcatenatedServices);
                                    emailBody = emailBody.Replace("{{company_name}}", LoggedInUser.Company);
                                    emailBody = emailBody.Replace("{{company_email}}", company.NotificationEmail);
                                    emailBody = emailBody.Replace("{{company_address}}", company.Address);
                                    emailBody = emailBody.Replace("{{company_logo}}", $"<img class='text-center' style='height:50px;width:auto;' src='{"http://app.yourbookingplatform.com" + company.Logo}'>");
                                    // Assuming appointment.ID is a string
                                    string cancelLink = string.Format("http://app.yourbookingplatform.com/Appointment/CancelByEmail/?AppointmentID={0}", appointment.ID);
                                    emailBody = emailBody.Replace("{{cancellink}}", $"<a href='{cancelLink}' class='btn btn-primary'>CANCEL/RESCHEDULE</a>");

                                    emailBody = emailBody.Replace("{{company_phone}}", company.PhoneNumber);

                                    emailBody += "</body></html>";


                                    if (IsValidEmail(customer.Email))
                                    {
                                        SendEmail(customer.Email, "Appointment Confirmation", emailBody, company);
                                    }
                                }

                                var history = new History();
                                history.Business = LoggedInUser.Company;
                                if (customer != null)
                                {
                                    history.CustomerName = customer.FirstName + " " + customer.LastName;
                                }
                                else
                                {
                                    history.CustomerName = "Walk In";
                                }
                                history.Date = DateTime.Now;
                                history.AppointmentID = appointment.ID;
                                history.Note = "Offline Appointment Created for " + history.CustomerName + " by:" + LoggedInUser.Name + " ID: " + appointment.ID;
                                history.EmployeeName = employee.Name;
                                history.Name = "Offline Appointment Created";
                                HistoryServices.Instance.SaveHistory(history);
                            }
                            try
                            {
                                var mesage = "";
                                var googleCalendar = GoogleCalendarServices.Instance.GetGoogleCalendarServicesWRTBusiness(LoggedInUser.Company);
                                if (googleCalendar != null && !googleCalendar.Disabled)
                                {
                                    mesage = GenerateonGoogleCalendar(appointment.ID, ConcatenatedServices, 0, "SAVING");
                                }

                                return Json(new { success = true, Message = mesage }, JsonRequestBehavior.AllowGet);

                            }
                            catch (Exception ex)
                            {

                                return Json(new { success = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
                            }

                            return Json(new { success = true }, JsonRequestBehavior.AllowGet);
                        }

                    }

                }
                else
                {
                    bool Switched = false;
                    var appointment = AppointmentServices.Instance.GetAppointment(model.ID);
                    if (appointment.CustomerID != model.CustomerID)
                    {
                        Switched = true;
                    }
                    var customer = CustomerServices.Instance.GetCustomer(model.CustomerID);

                    var oldDate = appointment.Date.ToString("yyyy-MM-dd");
                    var oldTime = appointment.Time.ToString("H:mm");
                    var oldEmployeeID = appointment.EmployeeID;
                    if (model.AllEmployees == true)
                    {
                        foreach (var emp in model.Employees)
                        {
                            appointment.Business = appointment.Business;
                            appointment.BookingDate = appointment.BookingDate;
                            appointment.Date = model.Date;
                            appointment.Time = model.Time;
                            appointment.Days = model.Days;
                            appointment.Discount = model.Discount;
                            appointment.Deposit = model.Deposit;
                            appointment.DepositMethod = model.DepositMethod;
                            appointment.EmployeeID = emp.ID;
                            appointment.Frequency = model.Frequency;
                            if (appointment.Frequency == "Every Week")
                            {
                                appointment.Every = model.EveryWeek;
                                if (model.EndsWeek == "NumberOfTimes")
                                {
                                    appointment.Ends = model.EndsWeek + "_" + model.EndsNumberOfTimesWeek;

                                }
                                else if (model.EndsWeek == "Specific Date")
                                {
                                    appointment.Ends = model.EndsWeek + "_" + model.EndsDateWeek.ToString();
                                }
                                else
                                {
                                    appointment.Ends = model.EndsWeek;
                                }
                            }
                            else if (appointment.Frequency == "Every Day")
                            {
                                appointment.Every = model.EveryDay;
                                if (model.EndsDay == "NumberOfTimes")
                                {
                                    appointment.Ends = model.EndsDay + "_" + model.EndsNumberOfTimesDay;

                                }
                                else if (model.EndsDay == "Specific Date")
                                {
                                    appointment.Ends = model.EndsDay + "_" + model.EndsDateDay.ToString();
                                }
                                else
                                {
                                    appointment.Ends = model.EndsDay;
                                }
                            }
                            else if (appointment.Frequency == "Every Month")
                            {
                                appointment.Every = model.EveryMonth;
                                if (model.EndsMonth == "NumberOfTimes")
                                {
                                    appointment.Ends = model.EndsMonth + "_" + model.EndsNumberOfTimesMonth;

                                }
                                else if (model.EndsMonth == "Specific Date")
                                {
                                    appointment.Ends = model.EndsMonth + "_" + model.EndsDateMonth.ToString();
                                }
                                else
                                {
                                    appointment.Ends = model.EndsMonth;
                                }
                            }
                            else
                            {
                                appointment.Every = "";
                                appointment.Ends = "Never";
                            }

                            appointment.Notes = model.Notes;
                            appointment.IsRepeat = model.IsRepeat;

                            appointment.CustomerID = model.CustomerID;
                            if (appointment.CustomerID == 0)
                            {
                                appointment.IsWalkIn = true;

                            }
                            else
                            {
                                appointment.IsWalkIn = false;
                            }
                            appointment.IsPaid = true;


                            int MinsToBeAddedForEndTime = 0;
                            float TotalCost = 0;
                            if (model.Service != null)
                            {
                                foreach (var item in model.Service.Split(',').ToList())
                                {
                                    var Service = ServiceServices.Instance.GetService(int.Parse(item));
                                    if (Service != null)
                                    {
                                        TotalCost += Service.Price;
                                    }
                                }
                                foreach (var item in model.ServiceDuration.Split(',').ToList())
                                {
                                    MinsToBeAddedForEndTime += int.Parse(item.Replace("Mins", "").Replace("mins", "").Trim());

                                }
                            }
                            appointment.ServiceDuration = model.ServiceDuration;
                            appointment.EndTime = appointment.Time.AddMinutes(MinsToBeAddedForEndTime);
                            appointment.Service = model.Service;
                            appointment.TotalCost = TotalCost - model.Deposit;
                            var service = ServiceServices.Instance.GetService(int.Parse(model.Service.Split(',').FirstOrDefault())).Name;
                            if (service == "Break")
                            {
                                appointment.Color = "darkgray";
                            }
                            else
                            {
                                appointment.Color = "#F79700";

                            }
                            if (model.ServiceDiscount == null)
                            {
                                // Split the ServiceDuration string by commas
                                string[] durations = model.Service.Split(',');

                                // Get the number of values
                                int numberOfServices = durations.Length;

                                // Create a string with zeros for each service
                                string zeros = string.Join(",", Enumerable.Repeat("0", numberOfServices));

                                // Save the zeros string to ServiceDiscount
                                appointment.ServiceDiscount = zeros;
                            }
                            else
                            {
                                appointment.ServiceDiscount = model.ServiceDiscount;
                            }
                            AppointmentServices.Instance.UpdateAppointment(appointment);
                            CreateBuffer(appointment.ID);
                            var reminder = ReminderServices.Instance.GetReminderWRTAppID(appointment.ID);
                            if (reminder != null)
                            {
                                DateTime combinedDateTime = new DateTime(appointment.Date.Year, appointment.Date.Month, appointment.Date.Day, appointment.Time.Hour, appointment.Time.Minute, appointment.Time.Second);
                                reminder.Date = combinedDateTime;
                                ReminderServices.Instance.UpdateReminder(reminder);
                            }
                            else
                            {
                                reminder = new Entities.Reminder();
                                reminder.Service = appointment.Service;
                                reminder.Business = appointment.Business;
                                DateTime combinedDateTime = new DateTime(appointment.Date.Year, appointment.Date.Month, appointment.Date.Day, appointment.Time.Hour, appointment.Time.Minute, appointment.Time.Second);
                                reminder.Date = combinedDateTime;
                                reminder.CustomerID = appointment.CustomerID;
                                reminder.EmployeeID = appointment.EmployeeID;
                                reminder.Paid = true;
                                reminder.AppointmentID = appointment.ID;
                                reminder.IsCancelled = false;
                                ReminderServices.Instance.SaveReminder(reminder);
                            }
                            var history = new History();
                            history.Business = LoggedInUser.Company;
                            history.CustomerName = "Walk In";
                            history.AppointmentID = appointment.ID;
                            history.Date = DateTime.Now;
                            history.Name = "Appointment Updated";
                            history.Note = "Appointment Updated for:" + history.CustomerName + " by " + LoggedInUser.Name;
                            history.EmployeeName = EmployeeServices.Instance.GetEmployee(emp.ID).Name;
                            HistoryServices.Instance.SaveHistory(history);
                            var googleCalendar = GoogleCalendarServices.Instance.GetGoogleCalendarServicesWRTBusiness(LoggedInUser.Company);
                            if (googleCalendar != null && !googleCalendar.Disabled)
                            {
                                if (oldEmployeeID != appointment.EmployeeID)
                                {
                                    GenerateonGoogleCalendar(appointment.ID, service, oldEmployeeID, "SAVING");

                                }
                                else
                                {
                                    GenerateonGoogleCalendar(appointment.ID, service, 0, "SAVING");
                                }
                            }

                        }
                    }
                    else
                    {
                        appointment.Date = model.Date;
                        appointment.Time = model.Time;
                        appointment.Days = model.Days;
                        appointment.Discount = model.Discount;
                        appointment.Deposit = model.Deposit;
                        appointment.DepositMethod = model.DepositMethod;
                        appointment.FromGCAL = false;
                        appointment.EmployeeID = model.EmployeeID;
                        appointment.Frequency = model.Frequency;
                        if (appointment.Frequency == "Every Week")
                        {
                            appointment.Every = model.EveryWeek;
                            if (model.EndsWeek == "NumberOfTimes")
                            {
                                appointment.Ends = model.EndsWeek + "_" + model.EndsNumberOfTimesWeek;

                            }
                            else if (model.EndsWeek == "Specific Date")
                            {
                                appointment.Ends = model.EndsWeek + "_" + model.EndsDateWeek.ToString();
                            }
                            else
                            {
                                appointment.Ends = model.EndsWeek;
                            }
                        }
                        else if (appointment.Frequency == "Every Day")
                        {
                            appointment.Every = model.EveryDay;
                            if (model.EndsDay == "NumberOfTimes")
                            {
                                appointment.Ends = model.EndsDay + "_" + model.EndsNumberOfTimesDay;

                            }
                            else if (model.EndsDay == "Specific Date")
                            {
                                appointment.Ends = model.EndsDay + "_" + model.EndsDateDay.ToString();
                            }
                            else
                            {
                                appointment.Ends = model.EndsDay;
                            }
                        }
                        else if (appointment.Frequency == "Every Month")
                        {
                            appointment.Every = model.EveryMonth;
                            if (model.EndsMonth == "NumberOfTimes")
                            {
                                appointment.Ends = model.EndsMonth + "_" + model.EndsNumberOfTimesMonth;

                            }
                            else if (model.EndsMonth == "Specific Date")
                            {
                                appointment.Ends = model.EndsMonth + "_" + model.EndsDateMonth.ToString();
                            }
                            else
                            {
                                appointment.Ends = model.EndsMonth;
                            }
                        }
                        else
                        {
                            appointment.Every = "";
                            appointment.Ends = "Never";
                        }

                        appointment.Notes = model.Notes;
                        appointment.IsRepeat = model.IsRepeat;

                        appointment.CustomerID = model.CustomerID;
                        if (appointment.CustomerID == 0)
                        {
                            appointment.IsWalkIn = true;
                        }
                        else
                        {
                            appointment.IsWalkIn = false;
                        }
                        appointment.IsPaid = true;

                        int MinsToBeAddedForEndTime = 0;
                        float TotalCost = 0;
                        if (model.Service != null)
                        {
                            foreach (var item in model.Service.Split(',').ToList())
                            {
                                var Service = ServiceServices.Instance.GetService(int.Parse(item));
                                if (Service != null)
                                {
                                    TotalCost += Service.Price;
                                }
                            }
                            foreach (var item in model.ServiceDuration.Split(',').ToList())
                            {
                                MinsToBeAddedForEndTime += int.Parse(item.Replace("Mins", "").Replace("mins", "").Trim());

                            }
                        }
                        appointment.ServiceDuration = model.ServiceDuration;
                        appointment.EndTime = appointment.Time.AddMinutes(MinsToBeAddedForEndTime);
                        appointment.Service = model.Service;
                        appointment.TotalCost = TotalCost - model.Deposit;
                        if (appointment.DepositMethod != "Online")
                        {
                            if (ServiceServices.Instance.GetService(int.Parse(model.Service.Split(',').FirstOrDefault())).Name == "Break")
                            {
                                appointment.Color = "darkgray";
                            }
                            else
                            {
                                appointment.Color = "#F79700";

                            }
                        }
                        if (model.ServiceDiscount == null)
                        {
                            // Split the ServiceDuration string by commas
                            string[] durations = model.Service.Split(',');

                            // Get the number of values
                            int numberOfServices = durations.Length;

                            // Create a string with zeros for each service
                            string zeros = string.Join(",", Enumerable.Repeat("0", numberOfServices));

                            // Save the zeros string to ServiceDiscount
                            appointment.ServiceDiscount = zeros;
                        }
                        else
                        {
                            appointment.ServiceDiscount = model.ServiceDiscount;
                        }
                        AppointmentServices.Instance.UpdateAppointment(appointment);
                        CreateBuffer(appointment.ID);

                        if (appointment.CustomerID != 0)
                        {
                            var reminder = ReminderServices.Instance.GetReminderWRTAppID(appointment.ID);
                            if (reminder != null)
                            {
                                DateTime combinedDateTime = new DateTime(appointment.Date.Year, appointment.Date.Month, appointment.Date.Day, appointment.Time.Hour, appointment.Time.Minute, appointment.Time.Second);
                                reminder.Date = combinedDateTime;
                                ReminderServices.Instance.UpdateReminder(reminder);
                            }
                            else
                            {
                                reminder = new Entities.Reminder();
                                reminder.Service = appointment.Service;
                                reminder.Business = appointment.Business;
                                DateTime combinedDateTime = new DateTime(appointment.Date.Year, appointment.Date.Month, appointment.Date.Day, appointment.Time.Hour, appointment.Time.Minute, appointment.Time.Second);
                                reminder.Date = combinedDateTime;
                                reminder.CustomerID = appointment.CustomerID;
                                reminder.Paid = true;
                                reminder.IsCancelled = false;
                                reminder.AppointmentID = appointment.ID;
                                reminder.EmployeeID = appointment.EmployeeID;
                                ReminderServices.Instance.SaveReminder(reminder);
                            }
                        }

                        if (customer != null)
                        {
                            var history = new History();
                            history.Business = LoggedInUser.Company;
                            history.CustomerName = customer.FirstName + " " + customer.LastName;
                            history.Date = DateTime.Now;
                            history.Name = "Appointment Updated";
                            history.AppointmentID = appointment.ID;
                            history.Note = "Appointment Updated for:" + history.CustomerName + " by " + LoggedInUser.Name;
                            history.EmployeeName = EmployeeServices.Instance.GetEmployee(appointment.EmployeeID).Name;
                            HistoryServices.Instance.SaveHistory(history);
                        }
                        else
                        {
                            var history = new History();
                            history.Business = LoggedInUser.Company;
                            history.CustomerName = "Walk In";
                            history.Date = DateTime.Now;
                            history.AppointmentID = appointment.ID;
                            history.Name = "Appointment Updated";
                            history.Note = "Appointment Updated for:" + history.CustomerName + " by " + LoggedInUser.Name;
                            history.EmployeeName = EmployeeServices.Instance.GetEmployee(appointment.EmployeeID).Name;
                            HistoryServices.Instance.SaveHistory(history);
                        }
                        string ConcatenatedServices = "";
                        foreach (var item in appointment.Service.Split(',').ToList())
                        {
                            var Service = ServiceServices.Instance.GetService(int.Parse(item));
                            if (Service != null)
                            {
                                if (ConcatenatedServices == "")
                                {
                                    ConcatenatedServices = String.Join(",", Service.Name);
                                }
                                else
                                {
                                    ConcatenatedServices = String.Join(",", ConcatenatedServices, Service.Name);

                                }
                            }
                        }
                        if (Switched)
                        {
                            #region ConfirmationEmailSender
                            var EmailTemplate = "";
                            if (!AppointmentServices.Instance.IsNewCustomer(company.Business, appointment.CustomerID))
                            {
                                EmailTemplate = "First Registration Email";
                            }
                            else
                            {
                                EmailTemplate = "Appointment Confirmation";
                            }





                            var employee = EmployeeServices.Instance.GetEmployee(appointment.EmployeeID);


                            string emailBody = "<html><body>";
                            var emailTemplate = EmailTemplateServices.Instance.GetEmailTemplateWRTBusiness(appointment.Business, EmailTemplate);
                            if (emailTemplate != null)
                            {
                                emailBody += "<h2 style='font-family: Arial, sans-serif;'>" + EmailTemplate + "</h2>";
                                emailBody += emailTemplate.TemplateCode;
                                emailBody = emailBody.Replace("{{Customer_first_name}}", customer.FirstName);
                                emailBody = emailBody.Replace("{{Customer_last_name}}", customer.LastName);
                                emailBody = emailBody.Replace("{{Customer_initial}}", "Dear");
                                emailBody = emailBody.Replace("{{date}}", appointment.Date.ToString("yyyy-MM-dd"));
                                emailBody = emailBody.Replace("{{time}}", appointment.Time.ToString("H:mm:ss"));
                                emailBody = emailBody.Replace("{{end_time}}", appointment.EndTime.ToString("H:mm:ss"));
                                emailBody = emailBody.Replace("{{employee}}", employee.Name);
                                emailBody = emailBody.Replace("{{employee_specialization}}", employee.Specialization);
                                emailBody = emailBody.Replace("{{employee_picture}}", $"<img class='text-center' style='height:50px;width:auto;' src='{"http://app.yourbookingplatform.com" + employee.Photo}'>");

                                emailBody = emailBody.Replace("{{services}}", ConcatenatedServices);
                                emailBody = emailBody.Replace("{{company_name}}", company.Business);
                                emailBody = emailBody.Replace("{{company_email}}", company.NotificationEmail);
                                emailBody = emailBody.Replace("{{company_address}}", company.Address);
                                emailBody = emailBody.Replace("{{company_logo}}", $"<img class='text-center' style='height:50px;width:auto;' src='{"http://app.yourbookingplatform.com" + company.Logo}'>");
                                emailBody = emailBody.Replace("{{company_phone}}", company.PhoneNumber);
                                emailBody = emailBody.Replace("{{password}}", customer.Password);
                                string cancelLink = string.Format("http://app.yourbookingplatform.com/Appointment/CancelByEmail/?AppointmentID={0}", appointment.ID);
                                emailBody = emailBody.Replace("{{cancellink}}", $"<a href='{cancelLink}' class='btn btn-primary'>CANCEL/RESCHEDULE</a>");
                                emailBody += "</body></html>";
                                SendEmail(customer.Email, EmailTemplate, emailBody, company);
                            }
                            #endregion
                        }



                        var googleCalendar = GoogleCalendarServices.Instance.GetGoogleCalendarServicesWRTBusiness(appointment.Business);
                        if (googleCalendar != null && !googleCalendar.Disabled)
                        {
                            if (oldEmployeeID != appointment.EmployeeID)
                            {
                                GenerateonGoogleCalendar(appointment.ID, ConcatenatedServices, oldEmployeeID, "SAVING");

                            }
                            else
                            {
                                GenerateonGoogleCalendar(appointment.ID, ConcatenatedServices, 0, "SAVING");

                            }
                        }
                    }

                    if (oldTime != appointment.Time.ToString("H:mm") || oldDate != appointment.Date.ToString("yyyy-MM-dd"))
                    {
                        string ConcatenatedServices = "";
                        foreach (var item in appointment.Service.Split(',').ToList())
                        {
                            var Service = ServiceServices.Instance.GetService(int.Parse(item));
                            if (Service != null)
                            {
                                if (ConcatenatedServices == "")
                                {
                                    ConcatenatedServices = String.Join(",", Service.Name);
                                }
                                else
                                {
                                    ConcatenatedServices = String.Join(",", ConcatenatedServices, Service.Name);

                                }
                            }
                        }
                        var employee = EmployeeServices.Instance.GetEmployee(appointment.EmployeeID);
                        var emailDetails = EmailTemplateServices.Instance.GetEmailTemplateWRTBusiness(appointment.Business, "Appointment Moved");
                        if (emailDetails != null && emailDetails.IsActive == true)
                        {
                            string emailBody = "<html><body>";
                            emailBody += "<h2 style='font-family: Arial, sans-serif;'>Appointment Moved</h2>";
                            emailBody += emailDetails.TemplateCode;
                            emailBody = emailBody.Replace("{{Customer_first_name}}", customer.FirstName);
                            emailBody = emailBody.Replace("{{Customer_last_name}}", customer.LastName);
                            emailBody = emailBody.Replace("{{Customer_initial}}", customer.Gender == "Male" ? "Mr." : "Ms.");
                            emailBody = emailBody.Replace("{{date}}", appointment.Date.ToString("yyyy-MM-dd"));
                            emailBody = emailBody.Replace("{{time}}", appointment.Time.ToString("H:mm:ss"));
                            emailBody = emailBody.Replace("{{end_time}}", appointment.EndTime.ToString("H:mm"));
                            emailBody = emailBody.Replace("{{employee_picture}}", $"<img class='text-center' style='height:50px;width:auto;' src='{"http://app.yourbookingplatform.com" + employee.Photo}'>");
                            emailBody = emailBody.Replace("{{previous_date}}", oldDate);
                            emailBody = emailBody.Replace("{{previous_time}}", oldTime);
                            emailBody = emailBody.Replace("{{employee}}", employee.Name);
                            emailBody = emailBody.Replace("{{employee_specialization}}", employee.Specialization);
                            emailBody = emailBody.Replace("{{services}}", ConcatenatedServices);
                            emailBody = emailBody.Replace("{{company_name}}", company.Business);
                            emailBody = emailBody.Replace("{{company_email}}", company.NotificationEmail);
                            emailBody = emailBody.Replace("{{password}}", customer.Password);
                            emailBody = emailBody.Replace("{{company_address}}", company.Address);
                            emailBody = emailBody.Replace("{{company_logo}}", $"<img class='text-center' style='height:50px;width:auto;' src='{"http://app.yourbookingplatform.com" + company.Logo}'>");
                            emailBody = emailBody.Replace("{{company_phone}}", company.PhoneNumber);
                            string cancelLink = string.Format("http://app.yourbookingplatform.com/Appointment/CancelByEmail/?AppointmentID={0}", appointment.ID);
                            emailBody = emailBody.Replace("{{cancellink}}", $"<a href='{cancelLink}' class='btn btn-primary'>CANCEL/RESCHEDULE</a>");

                            emailBody += "</body></html>";
                            if (IsValidEmail(customer.Email))
                            {
                                SendEmail(customer.Email, "Appointment Moved", emailBody, company);
                            }
                        }
                    }

                    return Json(new { success = true }, JsonRequestBehavior.AllowGet);
                }


            }
            catch (Exception ex)
            {
                return Json(new { success = true }, JsonRequestBehavior.AllowGet);
            }


        }


        public string RefreshToken(string Company)
        {
            try
            {
          

                var googleCalendar = GoogleCalendarServices.Instance.GetGoogleCalendarServicesWRTBusiness(Company);
                if (googleCalendar == null)
                {
                    return "Failed: Google Calendar service not found for the company.";
                }

                var url = new System.Uri("https://oauth2.googleapis.com/token");
                var restClient = new RestClient(url);
                var request = new RestRequest();

                var ClientID = "201633868472-3sf5q4hbiqupcf0smo6auch9bku6bech.apps.googleusercontent.com";
                var ClientSecret = "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";
                request.AddQueryParameter("client_id", ClientID);
                request.AddQueryParameter("client_secret", ClientSecret);
                request.AddQueryParameter("grant_type", "refresh_token");
                request.AddQueryParameter("refresh_token", googleCalendar.RefreshToken);

                var response = restClient.Post(request);

                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    var tokenResponse = JsonConvert.DeserializeObject<TokenResponseForDB>(response.Content);
                    if (tokenResponse == null)
                    {
                        return "Failed: Unable to deserialize token response.";
                    }

                    googleCalendar.AccessToken = tokenResponse.AccessToken;
                    GoogleCalendarServices.Instance.UpdateGoogleCalendarIntegration(googleCalendar);

                    return "Refreshed";
                }
                else
                {
                    return "Failed: " + response.Content;
                }
            }
            catch (Exception ex)
            {
                // Log the exception details for further analysis
                // Consider using a logging framework like NLog, Serilog, or log4net
                return "Failed: " + ex.Message + " | StackTrace: " + ex.StackTrace;
            }
        }



        //public async Task<string> GenerateonGoogleCalendar(int ID, string Services, int oldEmployeeID = 0)
        //{
        //    // Retrieve appointment
        //    var appointment = AppointmentServices.Instance.GetAppointment(ID);
        //    if (appointment == null)
        //    {
        //        return "Error: Appointment not found.";
        //    }

        //    var Oldemployee = EmployeeServices.Instance.GetEmployee(oldEmployeeID);
        //    // Retrieve logged-in user
        //    var loggedInUser = UserManager.FindById(User.Identity.GetUserId());
        //    if (loggedInUser == null)
        //    {
        //        return "Error: Logged-in user not found.";
        //    }

        //    var googleCalendar = GoogleCalendarServices.Instance.GetGoogleCalendarServicesWRTBusiness(loggedInUser.Company);
        //    if (googleCalendar == null)
        //    {
        //        return "Error: Google Calendar service not found.";
        //    }
        //    var userCalendarID = "";
        //    if (oldEmployeeID != 0)
        //    {
        //        userCalendarID = Oldemployee.GoogleCalendarID;
        //    }
        //    else
        //    {
        //        var employee = EmployeeServices.Instance.GetEmployee(appointment.EmployeeID);
        //        userCalendarID = employee.GoogleCalendarID;

        //    }
        //    var Nurl = new System.Uri("https://www.googleapis.com/calendar/v3/calendars/" + userCalendarID + "/events/" + appointment.GoogleCalendarEventID);
        //    var NrestClient = new RestClient(Nurl);
        //    var Nrequest = new RestRequest();

        //    Nrequest.AddQueryParameter("key", "AIzaSyASKpY6I08IVKFMw3muX39uMzPc5sBDaSc");
        //    Nrequest.AddHeader("Authorization", "Bearer " + googleCalendar.AccessToken);
        //    Nrequest.AddHeader("Accept", "application/json");
        //    try
        //    {
        //        var Nresponse = NrestClient.Delete(Nrequest);

        //        if (oldEmployeeID != 0)
        //        {


        //            RefreshToken(appointment.Business);
        //            int year = appointment.Date.Year;
        //            int month = appointment.Date.Month;
        //            int day = appointment.Date.Day;
        //            int starthour = appointment.Time.Hour;
        //            int startminute = appointment.Time.Minute;
        //            int startseconds = appointment.Time.Second;

        //            int endhour = appointment.EndTime.Hour;
        //            int endminute = appointment.EndTime.Minute;
        //            int endseconds = appointment.EndTime.Second;

        //            DateTime startDateNew = new DateTime(year, month, day, starthour, startminute, startseconds);
        //            DateTime endDateNew = new DateTime(year, month, day, endhour, endminute, endseconds);

        //            DateTime currentDateTime = DateTime.Now;
        //            TimeSpan offset = TimeZoneInfo.Local.GetUtcOffset(currentDateTime);



        //            // Retrieve Google Calendar services


        //            // Retrieve company
        //            var company = CompanyServices.Instance.GetCompany().FirstOrDefault(x => x.Business == loggedInUser.Company);
        //            if (company == null)
        //            {
        //                return "Error: Company not found.";
        //            }

        //            // Retrieve employee
        //            var employee = EmployeeServices.Instance.GetEmployee(appointment.EmployeeID);
        //            if (employee == null)
        //            {
        //                return "Error: Employee not found.";
        //            }
        //            if (employee.GoogleCalendarID != null && employee.GoogleCalendarID != "")
        //            {
        //                var url = $"https://www.googleapis.com/calendar/v3/calendars/{employee.GoogleCalendarID}/events";
        //                var finalUrl = new Uri(url);

        //                RestClient restClient = new RestClient(finalUrl);
        //                RestRequest request = new RestRequest();
        //                var calendarEvent = new Event
        //                {
        //                    Summary = "Appointment at: " + loggedInUser.Company,
        //                    Description = Services,
        //                    Start = new EventDateTime() { DateTime = startDateNew.ToString("yyyy-MM-dd'T'HH:mm:ss.fffK"), TimeZone = company.TimeZone },
        //                    End = new EventDateTime() { DateTime = endDateNew.ToString("yyyy-MM-dd'T'HH:mm:ss.fffK"), TimeZone = company.TimeZone }
        //                };

        //                var model = JsonConvert.SerializeObject(calendarEvent, new JsonSerializerSettings
        //                {
        //                    ContractResolver = new CamelCasePropertyNamesContractResolver()
        //                });

        //                request.AddQueryParameter("key", "AIzaSyASKpY6I08IVKFMw3muX39uMzPc5sBDaSc");
        //                request.AddHeader("Authorization", "Bearer " + googleCalendar.AccessToken);
        //                request.AddHeader("Accept", "application/json");
        //                request.AddHeader("Content-Type", "application/json");
        //                request.AddParameter("application/json", model, ParameterType.RequestBody);

        //                var response = restClient.Post(request);

        //                if (response.StatusCode == System.Net.HttpStatusCode.OK)
        //                {
        //                    JObject jsonObj = JObject.Parse(response.Content);
        //                    appointment.GoogleCalendarEventID = jsonObj["id"]?.ToString();
        //                    AppointmentServices.Instance.UpdateAppointment(appointment);
        //                    return "Saved";
        //                }
        //                else
        //                {
        //                    var history = new History();
        //                    history.Date = DateTime.Now;
        //                    history.Note = response.Content;
        //                    history.Business = "Error";
        //                    history.Type = "Error";
        //                    HistoryServices.Instance.SaveHistory(history);
        //                    return "Error: " + response.Content;

        //                }
        //            }

        //        }
        //        else
        //        {



        //            int year = appointment.Date.Year;
        //            int month = appointment.Date.Month;
        //            int day = appointment.Date.Day;
        //            int starthour = appointment.Time.Hour;
        //            int startminute = appointment.Time.Minute;
        //            int startseconds = appointment.Time.Second;

        //            int endhour = appointment.EndTime.Hour;
        //            int endminute = appointment.EndTime.Minute;
        //            int endseconds = appointment.EndTime.Second;

        //            DateTime startDateNew = new DateTime(year, month, day, starthour, startminute, startseconds);
        //            DateTime endDateNew = new DateTime(year, month, day, endhour, endminute, endseconds);

        //            DateTime currentDateTime = DateTime.Now;
        //            TimeSpan offset = TimeZoneInfo.Local.GetUtcOffset(currentDateTime);



        //            // Retrieve Google Calendar services


        //            // Retrieve company
        //            var company = CompanyServices.Instance.GetCompany().FirstOrDefault(x => x.Business == loggedInUser.Company);
        //            if (company == null)
        //            {
        //                return "Error: Company not found.";
        //            }

        //            // Retrieve employee
        //            var employee = EmployeeServices.Instance.GetEmployee(appointment.EmployeeID);
        //            if (employee == null)
        //            {
        //                return "Error: Employee not found.";
        //            }

        //            var url = $"https://www.googleapis.com/calendar/v3/calendars/{employee.GoogleCalendarID}/events";
        //            var finalUrl = new Uri(url);

        //            RestClient restClient = new RestClient(finalUrl);
        //            RestRequest request = new RestRequest();
        //            var calendarEvent = new Event
        //            {
        //                Summary = "Appointment at: " + loggedInUser.Company,
        //                Description = Services,
        //                Start = new EventDateTime() { DateTime = startDateNew.ToString("yyyy-MM-dd'T'HH:mm:ss.fffK"), TimeZone = company.TimeZone },
        //                End = new EventDateTime() { DateTime = endDateNew.ToString("yyyy-MM-dd'T'HH:mm:ss.fffK"), TimeZone = company.TimeZone }
        //            };

        //            var model = JsonConvert.SerializeObject(calendarEvent, new JsonSerializerSettings
        //            {
        //                ContractResolver = new CamelCasePropertyNamesContractResolver()
        //            });

        //            request.AddQueryParameter("key", "AIzaSyASKpY6I08IVKFMw3muX39uMzPc5sBDaSc");
        //            request.AddHeader("Authorization", "Bearer " + googleCalendar.AccessToken);
        //            request.AddHeader("Accept", "application/json");
        //            request.AddHeader("Content-Type", "application/json");
        //            request.AddParameter("application/json", model, ParameterType.RequestBody);

        //            var response = restClient.Post(request);

        //            if (response.StatusCode == System.Net.HttpStatusCode.OK)
        //            {
        //                JObject jsonObj = JObject.Parse(response.Content);
        //                appointment.GoogleCalendarEventID = jsonObj["id"]?.ToString();
        //                AppointmentServices.Instance.UpdateAppointment(appointment);
        //                return "Saved";
        //            }
        //            else
        //            {
        //                var history = new History();
        //                history.Date = DateTime.Now;
        //                history.Note = response.Content;
        //                history.Business = "Error";
        //                history.Type = "Error";
        //                HistoryServices.Instance.SaveHistory(history);
        //                return "Error: " + response.Content;

        //            }

        //        }
        //        return "success";
        //    }
        //    catch (Exception ex)
        //    {
        //        if (oldEmployeeID != 0)
        //        {


        //            RefreshToken(appointment.Business);
        //            int year = appointment.Date.Year;
        //            int month = appointment.Date.Month;
        //            int day = appointment.Date.Day;
        //            int starthour = appointment.Time.Hour;
        //            int startminute = appointment.Time.Minute;
        //            int startseconds = appointment.Time.Second;

        //            int endhour = appointment.EndTime.Hour;
        //            int endminute = appointment.EndTime.Minute;
        //            int endseconds = appointment.EndTime.Second;

        //            DateTime startDateNew = new DateTime(year, month, day, starthour, startminute, startseconds);
        //            DateTime endDateNew = new DateTime(year, month, day, endhour, endminute, endseconds);

        //            DateTime currentDateTime = DateTime.Now;
        //            TimeSpan offset = TimeZoneInfo.Local.GetUtcOffset(currentDateTime);



        //            // Retrieve Google Calendar services


        //            // Retrieve company
        //            var company = CompanyServices.Instance.GetCompany().FirstOrDefault(x => x.Business == loggedInUser.Company);
        //            if (company == null)
        //            {
        //                return "Error: Company not found.";
        //            }

        //            // Retrieve employee
        //            var employee = EmployeeServices.Instance.GetEmployee(appointment.EmployeeID);
        //            if (employee == null)
        //            {
        //                return "Error: Employee not found.";
        //            }
        //            var url = $"https://www.googleapis.com/calendar/v3/calendars/{employee.GoogleCalendarID}/events";
        //            var finalUrl = new Uri(url);

        //            RestClient restClient = new RestClient(finalUrl);
        //            RestRequest request = new RestRequest();
        //            var calendarEvent = new Event
        //            {
        //                Summary = "Appointment at: " + loggedInUser.Company,
        //                Description = Services,
        //                Start = new EventDateTime() { DateTime = startDateNew.ToString("yyyy-MM-dd'T'HH:mm:ss.fffK"), TimeZone = company.TimeZone },
        //                End = new EventDateTime() { DateTime = endDateNew.ToString("yyyy-MM-dd'T'HH:mm:ss.fffK"), TimeZone = company.TimeZone }
        //            };

        //            var model = JsonConvert.SerializeObject(calendarEvent, new JsonSerializerSettings
        //            {
        //                ContractResolver = new CamelCasePropertyNamesContractResolver()
        //            });

        //            request.AddQueryParameter("key", "AIzaSyASKpY6I08IVKFMw3muX39uMzPc5sBDaSc");
        //            request.AddHeader("Authorization", "Bearer " + googleCalendar.AccessToken);
        //            request.AddHeader("Accept", "application/json");
        //            request.AddHeader("Content-Type", "application/json");
        //            request.AddParameter("application/json", model, ParameterType.RequestBody);

        //            var response = restClient.Post(request);

        //            if (response.StatusCode == System.Net.HttpStatusCode.OK)
        //            {
        //                JObject jsonObj = JObject.Parse(response.Content);
        //                appointment.GoogleCalendarEventID = jsonObj["id"]?.ToString();
        //                AppointmentServices.Instance.UpdateAppointment(appointment);
        //                return "Saved";
        //            }
        //            else
        //            {
        //                var history = new History();
        //                history.Date = DateTime.Now;
        //                history.Note = response.Content;
        //                history.Business = "Error";
        //                history.Type = "Error";
        //                HistoryServices.Instance.SaveHistory(history);
        //                return "Error: " + response.Content;

        //            }

        //        }
        //        else
        //        {



        //            int year = appointment.Date.Year;
        //            int month = appointment.Date.Month;
        //            int day = appointment.Date.Day;
        //            int starthour = appointment.Time.Hour;
        //            int startminute = appointment.Time.Minute;
        //            int startseconds = appointment.Time.Second;

        //            int endhour = appointment.EndTime.Hour;
        //            int endminute = appointment.EndTime.Minute;
        //            int endseconds = appointment.EndTime.Second;

        //            DateTime startDateNew = new DateTime(year, month, day, starthour, startminute, startseconds);
        //            DateTime endDateNew = new DateTime(year, month, day, endhour, endminute, endseconds);

        //            DateTime currentDateTime = DateTime.Now;
        //            TimeSpan offset = TimeZoneInfo.Local.GetUtcOffset(currentDateTime);



        //            // Retrieve Google Calendar services


        //            // Retrieve company
        //            var company = CompanyServices.Instance.GetCompany().FirstOrDefault(x => x.Business == loggedInUser.Company);
        //            if (company == null)
        //            {
        //                return "Error: Company not found.";
        //            }

        //            // Retrieve employee
        //            var employee = EmployeeServices.Instance.GetEmployee(appointment.EmployeeID);
        //            if (employee == null)
        //            {
        //                return "Error: Employee not found.";
        //            }

        //            var url = $"https://www.googleapis.com/calendar/v3/calendars/{employee.GoogleCalendarID}/events";
        //            var finalUrl = new Uri(url);

        //            RestClient restClient = new RestClient(finalUrl);
        //            RestRequest request = new RestRequest();
        //            var calendarEvent = new Event
        //            {
        //                Summary = "Appointment at: " + loggedInUser.Company,
        //                Description = Services,
        //                Start = new EventDateTime() { DateTime = startDateNew.ToString("yyyy-MM-dd'T'HH:mm:ss.fffK"), TimeZone = company.TimeZone },
        //                End = new EventDateTime() { DateTime = endDateNew.ToString("yyyy-MM-dd'T'HH:mm:ss.fffK"), TimeZone = company.TimeZone }
        //            };

        //            var model = JsonConvert.SerializeObject(calendarEvent, new JsonSerializerSettings
        //            {
        //                ContractResolver = new CamelCasePropertyNamesContractResolver()
        //            });

        //            request.AddQueryParameter("key", "AIzaSyASKpY6I08IVKFMw3muX39uMzPc5sBDaSc");
        //            request.AddHeader("Authorization", "Bearer " + googleCalendar.AccessToken);
        //            request.AddHeader("Accept", "application/json");
        //            request.AddHeader("Content-Type", "application/json");
        //            request.AddParameter("application/json", model, ParameterType.RequestBody);

        //            var response = restClient.Post(request);

        //            if (response.StatusCode == System.Net.HttpStatusCode.OK)
        //            {
        //                JObject jsonObj = JObject.Parse(response.Content);
        //                appointment.GoogleCalendarEventID = jsonObj["id"]?.ToString();
        //                AppointmentServices.Instance.UpdateAppointment(appointment);
        //                return "Saved";
        //            }
        //            else
        //            {
        //                var history = new History();
        //                history.Date = DateTime.Now;
        //                history.Note = response.Content;
        //                history.Business = "Error";
        //                history.Type = "Error";
        //                HistoryServices.Instance.SaveHistory(history);
        //                return "Error: " + response.Content;

        //            }

        //        }
        //        throw;
        //    }



        //}


        public string GenerateonGoogleCalendar(int ID, string Services, int oldEmployeeID = 0, string NOTIMEZONE = "")
        {
            // Retrieve appointment
            var appointment = AppointmentServices.Instance.GetAppointment(ID);
            if (appointment == null) return "Error: Appointment not found.";

            var loggedInUser = UserManager.FindById(User.Identity.GetUserId());
            if (loggedInUser == null) return "Error: Logged-in user not found.";

          

            var company = CompanyServices.Instance.GetCompanyByName(appointment.Business);
            // Get Calendar ID (either old employee or new employee)
            if (oldEmployeeID != 0)
            {
                var old = EmployeeServices.Instance.GetEmployee(oldEmployeeID);
                var ToBeInputtedIDs = new Dictionary<GoogleCalendarIntegration, string>();
                //delete previous one
                if (old.Business == appointment.Business)
                {
                    RefreshToken(appointment.Business);

                    //employee is in same business as appointment
                    var googleKey = GoogleCalendarServices.Instance.GetGoogleCalendarServicesWRTBusiness(appointment.Business);
                    ToBeInputtedIDs.Add(googleKey, old.GoogleCalendarID);
                    var employeeRequest = EmployeeRequestServices.Instance.GetEmployeeRequestsWRTEMPID(old.ID);
                    var requestedEmployee = RequestedEmployeeServices.Instance.GetRequestedEmployeeWRTEmployeeID(old.ID);
                    if (employeeRequest.Any(x => x.EmployeeID == old.ID))
                    {
                        if (requestedEmployee != null && requestedEmployee.GoogleCalendarID != null && requestedEmployee.GoogleCalendarID != "")
                        {
                            RefreshToken(requestedEmployee.Business);
                            googleKey = GoogleCalendarServices.Instance.GetGoogleCalendarServicesWRTBusiness(requestedEmployee.Business);
                            ToBeInputtedIDs.Add(googleKey, requestedEmployee.GoogleCalendarID);
                        }
                    }
                }
                else
                {
                    RefreshToken(old.Business);
                    var googleKey = GoogleCalendarServices.Instance.GetGoogleCalendarServicesWRTBusiness(old.Business);
                    ToBeInputtedIDs.Add(googleKey, old.GoogleCalendarID);
                    var employeeRequest = EmployeeRequestServices.Instance.GetEmployeeRequestsWRTEMPID(old.ID);
                    var requestedEmployee = RequestedEmployeeServices.Instance.GetRequestedEmployeeWRTEmployeeID(old.ID);
                    if (employeeRequest.Any(x => x.EmployeeID == old.ID))
                    {
                        if (requestedEmployee != null && requestedEmployee.GoogleCalendarID != null && requestedEmployee.GoogleCalendarID != "")
                        {
                            RefreshToken(appointment.Business);
                            googleKey = GoogleCalendarServices.Instance.GetGoogleCalendarServicesWRTBusiness(appointment.Business);
                            ToBeInputtedIDs.Add(googleKey, requestedEmployee.GoogleCalendarID);
                        }
                    }
                }

                foreach (var item in ToBeInputtedIDs)
                {

                    if (item.Key != null && !item.Key.Disabled)
                    {
                        foreach (var cc in appointment.GoogleCalendarEventID.Split(',').ToList())
                        {
                            var Nurl = new System.Uri("https://www.googleapis.com/calendar/v3/calendars/" + item.Value + "/events/" + cc);
                            var NrestClient = new RestClient(Nurl);
                            var Nrequest = new RestRequest();

                            Nrequest.AddQueryParameter("key", "AIzaSyASKpY6I08IVKFMw3muX39uMzPc5sBDaSc");
                            Nrequest.AddHeader("Authorization", "Bearer " + item.Key.AccessToken);
                            Nrequest.AddHeader("Accept", "application/json");
                            var Nresponse = NrestClient.Delete(Nrequest);

                            
                           
                        }
                      
                    }
                }
                var newemp = EmployeeServices.Instance.GetEmployee(appointment.EmployeeID);
                ToBeInputtedIDs = new Dictionary<GoogleCalendarIntegration, string>();
                //delete previous one
                if (newemp.Business == appointment.Business)
                {
                    //employee is in same business as appointment
                    RefreshToken(appointment.Business);
                    var googleKey = GoogleCalendarServices.Instance.GetGoogleCalendarServicesWRTBusiness(appointment.Business);
                    ToBeInputtedIDs.Add(googleKey, newemp.GoogleCalendarID);
                    var employeeRequest = EmployeeRequestServices.Instance.GetEmployeeRequestsWRTEMPID(newemp.ID);
                    var requestedEmployee = RequestedEmployeeServices.Instance.GetRequestedEmployeeWRTEmployeeID(newemp.ID);
                    if (employeeRequest.Any(x => x.EmployeeID == newemp.ID))
                    {
                        if (requestedEmployee != null && requestedEmployee.GoogleCalendarID != null && requestedEmployee.GoogleCalendarID != "")
                        {
                            RefreshToken(requestedEmployee.Business);
                            googleKey = GoogleCalendarServices.Instance.GetGoogleCalendarServicesWRTBusiness(requestedEmployee.Business);
                            ToBeInputtedIDs.Add(googleKey, requestedEmployee.GoogleCalendarID);
                        }
                    }
                }
                else
                {
                    RefreshToken(newemp.Business);
                    var googleKey = GoogleCalendarServices.Instance.GetGoogleCalendarServicesWRTBusiness(newemp.Business);
                    ToBeInputtedIDs.Add(googleKey, newemp.GoogleCalendarID);
                    var employeeRequest = EmployeeRequestServices.Instance.GetEmployeeRequestsWRTEMPID(newemp.ID);
                    var requestedEmployee = RequestedEmployeeServices.Instance.GetRequestedEmployeeWRTEmployeeID(newemp.ID);
                    if (employeeRequest.Any(x => x.EmployeeID == newemp.ID))
                    {
                        if (requestedEmployee != null && requestedEmployee.GoogleCalendarID != null && requestedEmployee.GoogleCalendarID != "")
                        {
                            RefreshToken(appointment.Business);
                            googleKey = GoogleCalendarServices.Instance.GetGoogleCalendarServicesWRTBusiness(appointment.Business);
                            ToBeInputtedIDs.Add(googleKey, requestedEmployee.GoogleCalendarID);
                        }
                    }
                }
                foreach (var item in ToBeInputtedIDs)
                {
                    var employee = EmployeeServices.Instance.GetEmployee(appointment.EmployeeID);
                    if (employee == null)
                    {
                        return "Error: Employee not found.";
                    }


                    var url = $"https://www.googleapis.com/calendar/v3/calendars/{item.Value}/events";
                    var finalUrl = new Uri(url);

                    RestClient restClient = new RestClient(finalUrl);
                    RestRequest request = new RestRequest();
                    DateTime startDate = appointment.Date.Add(appointment.Time.TimeOfDay);
                    DateTime endDate = appointment.Date.Add(appointment.EndTime.TimeOfDay);



                    var calendarEvent = new Event
                    {
                        Summary = "Appointment at: " + loggedInUser.Company,
                        Description = Services + " ID: " + appointment.ID,
                        Start = new EventDateTime()
                        {
                            DateTime = startDate.ToString("yyyy-MM-dd'T'HH:mm:ss"), // UTC with offset
                            TimeZone = company.TimeZone
                        },
                        End = new EventDateTime()
                        {
                            DateTime = endDate.ToString("yyyy-MM-dd'T'HH:mm:ss"), // UTC with offset
                            TimeZone = company.TimeZone
                        }
                    };



                    var model = JsonConvert.SerializeObject(calendarEvent, new JsonSerializerSettings
                    {
                        ContractResolver = new CamelCasePropertyNamesContractResolver()
                    });
                    request.AddQueryParameter("key", "AIzaSyASKpY6I08IVKFMw3muX39uMzPc5sBDaSc");
                    request.AddHeader("Authorization", "Bearer " + item.Key.AccessToken);
                    request.AddHeader("Accept", "application/json");
                    request.AddHeader("Content-Type", "application/json");
                    request.AddParameter("application/json", model, ParameterType.RequestBody);

                    var response = restClient.Post(request);
                    if (response.StatusCode == System.Net.HttpStatusCode.OK)
                    {
                        JObject jsonObj = JObject.Parse(response.Content);
                        if (appointment.GoogleCalendarEventID != null)
                        {
                            appointment.GoogleCalendarEventID = appointment.GoogleCalendarEventID.Replace(appointment.GoogleCalendarEventID, jsonObj["id"]?.ToString());
                        }
                        else
                        {
                            appointment.GoogleCalendarEventID = jsonObj["id"]?.ToString();

                        }
                        AppointmentServices.Instance.UpdateAppointment(appointment);
                        // Event successfully updated
                    }
                    else
                    {
                        // Log error response
                        SaveErrorHistory(response.Content);
                    }
                }
                return "Saved";
            }
            else
            {
                var employee = EmployeeServices.Instance.GetEmployee(appointment.EmployeeID);
                var ToBeInputtedIDs = new Dictionary<GoogleCalendarIntegration, string>();
                if (employee.Business == appointment.Business)
                {
                    //employee is in same business as appointment
                    RefreshToken(appointment.Business);

                    var googleKey = GoogleCalendarServices.Instance.GetGoogleCalendarServicesWRTBusiness(appointment.Business);
                    ToBeInputtedIDs.Add(googleKey, employee.GoogleCalendarID);
                    var employeeRequest = EmployeeRequestServices.Instance.GetEmployeeRequestsWRTEMPID(employee.ID);
                    var requestedEmployee = RequestedEmployeeServices.Instance.GetRequestedEmployeeWRTEmployeeID(employee.ID);
                    if (employeeRequest.Any(x => x.EmployeeID == employee.ID))
                    {
                        if (requestedEmployee != null && requestedEmployee.GoogleCalendarID != null && requestedEmployee.GoogleCalendarID != "")
                        {
                            RefreshToken(requestedEmployee.Business);
                            googleKey = GoogleCalendarServices.Instance.GetGoogleCalendarServicesWRTBusiness(requestedEmployee.Business);

                            ToBeInputtedIDs.Add(googleKey, requestedEmployee.GoogleCalendarID);
                        }
                    }
                }
                else
                {
                    RefreshToken(employee.Business);

                    var googleKey = GoogleCalendarServices.Instance.GetGoogleCalendarServicesWRTBusiness(employee.Business);
                    ToBeInputtedIDs.Add(googleKey, employee.GoogleCalendarID);
                    var employeeRequest = EmployeeRequestServices.Instance.GetEmployeeRequestsWRTEMPID(employee.ID);
                    var requestedEmployee = RequestedEmployeeServices.Instance.GetRequestedEmployeeWRTEmployeeID(employee.ID);
                    if (employeeRequest.Any(x => x.EmployeeID == employee.ID))
                    {
                        if (requestedEmployee != null && requestedEmployee.GoogleCalendarID != null && requestedEmployee.GoogleCalendarID != "")
                        {
                            RefreshToken(appointment.Business);
                            googleKey = GoogleCalendarServices.Instance.GetGoogleCalendarServicesWRTBusiness(appointment.Business);
                            ToBeInputtedIDs.Add(googleKey, requestedEmployee.GoogleCalendarID);
                        }
                    }
                }
                //delete previous one
                

                try
                {
                    // Prepare event details
                    DateTime startDate = appointment.Date.Add(appointment.Time.TimeOfDay);
                    DateTime endDate = appointment.Date.Add(appointment.EndTime.TimeOfDay);

                    var calendarEvent = new Event
                    {
                        Summary = "Appointment at: " + loggedInUser.Company,
                        Description = Services + " ID: " + appointment.ID,
                        Start = new EventDateTime()
                        {
                            DateTime = startDate.ToString("yyyy-MM-dd'T'HH:mm:ss"), // UTC with offset
                            TimeZone = company.TimeZone
                        },
                        End = new EventDateTime()
                        {
                            DateTime = endDate.ToString("yyyy-MM-dd'T'HH:mm:ss"), // UTC with offset
                            TimeZone = company.TimeZone
                        }
                    };

                    var model = JsonConvert.SerializeObject(calendarEvent, new JsonSerializerSettings
                    {
                        ContractResolver = new CamelCasePropertyNamesContractResolver()
                    });

                    foreach (var item in ToBeInputtedIDs)
                    {

                        if (appointment.GoogleCalendarEventID != null && appointment.GoogleCalendarEventID.Split(',').Count() == ToBeInputtedIDs.Count())
                        {
                            var calendareventIds = appointment.GoogleCalendarEventID.Split(',').ToList();
                            foreach (var cc in calendareventIds)
                            {
                                var apiUrl = $"https://www.googleapis.com/calendar/v3/calendars/{item.Value}/events/{cc}";
                                var restClient = new RestClient(apiUrl);
                                var request = new RestRequest();


                                request.AddQueryParameter("key", "AIzaSyASKpY6I08IVKFMw3muX39uMzPc5sBDaSc");
                                request.AddHeader("Authorization", "Bearer " + item.Key.AccessToken);
                                request.AddHeader("Accept", "application/json");
                                request.AddHeader("Content-Type", "application/json");
                                request.AddParameter("application/json", model, ParameterType.RequestBody);
                                // Execute API request to update the event
                                var response = restClient.Execute(request, Method.Patch);
                                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                                {
                                    JObject jsonObj = JObject.Parse(response.Content);
                                    if (appointment.GoogleCalendarEventID != null)
                                    {
                                        appointment.GoogleCalendarEventID = appointment.GoogleCalendarEventID.Replace(cc, jsonObj["id"]?.ToString());
                                    }
                                    else
                                    {
                                        appointment.GoogleCalendarEventID = jsonObj["id"]?.ToString();

                                    }
                                    AppointmentServices.Instance.UpdateAppointment(appointment);

                                    // Event successfully updated
                                }
                                else
                                {
                                    // Log error response
                                    SaveErrorHistory(response.Content);
                                }
                            }
                            
                        }
                        else
                        {
                        

                            var url = $"https://www.googleapis.com/calendar/v3/calendars/{item.Value}/events";
                            var finalUrl = new Uri(url);

                            RestClient restClient = new RestClient(finalUrl);
                            RestRequest request = new RestRequest();




                            request.AddQueryParameter("key", "AIzaSyASKpY6I08IVKFMw3muX39uMzPc5sBDaSc");
                            request.AddHeader("Authorization", "Bearer " + item.Key.AccessToken);
                            request.AddHeader("Accept", "application/json");
                            request.AddHeader("Content-Type", "application/json");
                            request.AddParameter("application/json", model, ParameterType.RequestBody);

                            var response = restClient.Post(request);
                            if (response.StatusCode == System.Net.HttpStatusCode.OK)
                            {
                                JObject jsonObj = JObject.Parse(response.Content);
                                if(appointment.GoogleCalendarEventID != null)
                                {
                                    appointment.GoogleCalendarEventID = string.Join(",", appointment.GoogleCalendarEventID, jsonObj["id"]?.ToString());
                                }
                                else
                                {
                                    appointment.GoogleCalendarEventID = jsonObj["id"]?.ToString();

                                }
                                AppointmentServices.Instance.UpdateAppointment(appointment);
                                // Event successfully updated
                            }
                            else
                            {
                                // Log error response
                                SaveErrorHistory(response.Content);
                            }
                        }
                    }
                    return "Saved";


                }
                catch (Exception ex)
                {
                    // Log exception
                    SaveErrorHistory(ex.Message);
                    return "Error: " + ex.Message;
                }
            }
        }

        // Helper method to log error history
        private void SaveErrorHistory(string message)
        {
            var history = new History
            {
                Date = DateTime.Now,
                Note = message,
                Business = "Error",
                Type = "Error"
            };
            HistoryServices.Instance.SaveHistory(history);
        }




        public class Event
        {
            public Event()
            {
                TimeZoneInfo localTimeZone = TimeZoneInfo.Local;

                this.Start = new EventDateTime()
                {
                    TimeZone = localTimeZone.Id
                };
                this.End = new EventDateTime()
                {
                    TimeZone = localTimeZone.Id
                };
            }

            public string Id { get; set; }

            public string Summary { get; set; }

            public string Description { get; set; }

            public EventDateTime Start { get; set; }

            public EventDateTime End { get; set; }
        }

        public class EventDateTime
        {
            public string DateTime { get; set; }

            public string TimeZone { get; set; }
        }



        [HttpPost]
        public ActionResult SaveNewCustomer(string FirstName, string LastName, string Email, string MobileNumber)
        {

            var customer = new Entities.Customer();
            var user = UserManager.FindById(User.Identity.GetUserId());
            customer.FirstName = FirstName.Replace("'", "");
            customer.LastName = LastName.Replace("'", "");
            customer.Email = Email;
            customer.MobileNumber = MobileNumber;
            customer.Business = user.Company;

            var AlreadyCustomer = CustomerServices.Instance.GetCustomerWRTBusiness(user.Company, Email);
            if (AlreadyCustomer == null)
            {
                customer.DateAdded = DateTime.Now;
                CustomerServices.Instance.SaveCustomer(customer);
                var history = new History();
                history.Business = user.Company;
                history.CustomerName = customer.FirstName + " " + customer.LastName;
                history.Date = DateTime.Now;
                history.Note = "New customer saved: " + history.CustomerName;
                history.EmployeeName = "";
                HistoryServices.Instance.SaveHistory(history);
            }
            else
            {
                customer = AlreadyCustomer;
            }



            return Json(new { success = true, customer = customer }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult Delete(int ID)
        {
            AppointmentActionViewModel model = new AppointmentActionViewModel();
            var appointment = AppointmentServices.Instance.GetAppointment(ID);
            model.ID = appointment.ID;
            model.Date = appointment.Date;
            return PartialView("_Delete", model);
        }

        [HttpGet]
        public ActionResult DeleteNew(int ID)
        {
            AppointmentDetailsViewModel model = new AppointmentDetailsViewModel();
            var appointment = AppointmentServices.Instance.GetAppointment(ID);
            model.Appointment = appointment;
            model.Date = appointment.Date;
            return PartialView("_DeleteNew", model);
        }

        [HttpGet]
        public JsonResult DeleteFinally(int ID, string deleteOption = "")
        {
            try
            {

                var appointment = AppointmentServices.Instance.GetAppointment(ID);
                var LoggedInUser = UserManager.FindById(User.Identity.GetUserId());

                if (appointment.IsRepeat)
                {
                    if (deleteOption == "Delete all recurring absences")
                    {
                        var IDTobeUsed = 0;
                        if (appointment.FirstRepeatedID == 0)
                        {
                            IDTobeUsed = appointment.ID;
                        }
                        else
                        {
                            IDTobeUsed = appointment.FirstRepeatedID;
                        }
                        var appointmentsoccurency = AppointmentServices.Instance.OtherRecurrencesAppointments(IDTobeUsed);
                        var employee = EmployeeServices.Instance.GetEmployee(appointment.EmployeeID);
                        var ToBeInputtedIDs = new Dictionary<GoogleCalendarIntegration,string>();
                        
                        var googleKey = GoogleCalendarServices.Instance.GetGoogleCalendarServicesWRTBusiness(appointment.Business);
                        
                        ToBeInputtedIDs.Add(googleKey, employee.GoogleCalendarID);
                        
                        var employeeRequest = EmployeeRequestServices.Instance.GetEmployeeRequestsWRTBusiness(appointment.Business);
                        
                        var requestedEmployee = RequestedEmployeeServices.Instance.GetRequestedEmployeeWRTEmployeeID(employee.ID);
                        
                        
                        if (employeeRequest.Any(x => x.EmployeeID == employee.ID))
                        {
                            if (requestedEmployee != null && requestedEmployee.GoogleCalendarID != null && requestedEmployee.GoogleCalendarID != "")
                            {
                                googleKey = GoogleCalendarServices.Instance.GetGoogleCalendarServicesWRTBusiness(employee.Business);
                                ToBeInputtedIDs.Add(googleKey, requestedEmployee.GoogleCalendarID);
                            }
                        }
                        foreach (var item in appointmentsoccurency)
                        {

                            try
                            {
                                foreach (var gcalID in ToBeInputtedIDs)
                                {
                                    if (gcalID.Key != null && !gcalID.Key.Disabled)
                                    {
                                        var url = new System.Uri("https://www.googleapis.com/calendar/v3/calendars/" + gcalID.Value + "/events/" + item.GoogleCalendarEventID);
                                        RestClient restClient = new RestClient(url);
                                        RestRequest request = new RestRequest();

                                        request.AddQueryParameter("key", "AIzaSyASKpY6I08IVKFMw3muX39uMzPc5sBDaSc");
                                        request.AddHeader("Authorization", "Bearer " + gcalID.Key.AccessToken);
                                        request.AddHeader("Accept", "application/json");

                                        var response = restClient.Delete(request);
                                        if (response.StatusCode == System.Net.HttpStatusCode.NoContent)
                                        {
                                            item.DELETED = true;
                                            item.DeletedTime = DateTime.Now.ToString();
                                            AppointmentServices.Instance.UpdateAppointment(item);
                                        }
                                        else
                                        {
                                            item.DELETED = true;
                                            item.DeletedTime = DateTime.Now.ToString();
                                            AppointmentServices.Instance.UpdateAppointment(item);


                                            var history = new History();
                                            history.Date = DateTime.Now;
                                            history.Note = response.Content;
                                            history.Business = "Error";
                                            history.Type = "Error";
                                            HistoryServices.Instance.SaveHistory(history);
                                        }
                                    }
                                    else
                                    {
                                        item.DELETED = true;
                                        item.DeletedTime = DateTime.Now.ToString();
                                        AppointmentServices.Instance.UpdateAppointment(item);
                                    }
                                }
                                
                            }
                            catch (Exception ex)
                            {
                                var history = new History();
                                history.Date = DateTime.Now;
                                history.Note = ex.Message;
                                history.Business = "Error";
                                history.Type = "Error";
                                HistoryServices.Instance.SaveHistory(history);
                            }

                        }
                       

                    }
                    else
                    {
                        appointment.DELETED = true;
                        appointment.DeletedTime = DateTime.Now.ToString();
                        AppointmentServices.Instance.UpdateAppointment(appointment);
                    }
                }
                else
                {
                    appointment.DELETED = true;
                    appointment.DeletedTime = DateTime.Now.ToString();
                    AppointmentServices.Instance.UpdateAppointment(appointment);

                    var customer = CustomerServices.Instance.GetCustomer(appointment.ID);
                    var employee = EmployeeServices.Instance.GetEmployee(appointment.EmployeeID);
                    var ToBeInputtedIDs = new Dictionary<GoogleCalendarIntegration,string>();
                    //delete previous one
                    var googleKey = GoogleCalendarServices.Instance.GetGoogleCalendarServicesWRTBusiness(appointment.Business);
                    ToBeInputtedIDs.Add(googleKey, employee.GoogleCalendarID);
                    var employeeRequest = EmployeeRequestServices.Instance.GetEmployeeRequestsWRTBusiness(appointment.Business);
                    var requestedEmployee = RequestedEmployeeServices.Instance.GetRequestedEmployeeWRTEmployeeID(employee.ID);
                    if (employeeRequest.Any(x => x.EmployeeID == employee.ID))
                    {
                        if (requestedEmployee != null && requestedEmployee.GoogleCalendarID != null && requestedEmployee.GoogleCalendarID != "")
                        {
                            googleKey = GoogleCalendarServices.Instance.GetGoogleCalendarServicesWRTBusiness(employee.Business);
                            ToBeInputtedIDs.Add(googleKey,requestedEmployee.GoogleCalendarID);
                        }
                    }
                    if (customer != null)
                    {
                        var historyNew = new History();
                        historyNew.Business = appointment.Business;
                        historyNew.CustomerName = customer.FirstName + " " + customer.LastName;
                        historyNew.Date = DateTime.Now;
                        historyNew.AppointmentID = appointment.ID;
                        historyNew.Note = "Appointment was deleted:" + historyNew.CustomerName + "By :" + LoggedInUser.Name;
                        historyNew.EmployeeName = employee.Name;
                        historyNew.Name = "Appointment Deleted";
                        HistoryServices.Instance.SaveHistory(historyNew);

                    }
                    else
                    {
                        var historyNew = new History();
                        historyNew.Business = appointment.Business;
                        historyNew.CustomerName = "";
                        historyNew.Date = DateTime.Now;
                        historyNew.AppointmentID = appointment.ID;
                        historyNew.Note = "Appointment was deleted:" + historyNew.CustomerName + "By :" + LoggedInUser.Name;
                        historyNew.EmployeeName = employee.Name;
                        historyNew.Name = "Appointment Deleted";
                        HistoryServices.Instance.SaveHistory(historyNew);
                    }
                    foreach (var item in ToBeInputtedIDs)
                    {
                        if (item.Key != null && !item.Key.Disabled)
                        {
                            var url = new System.Uri("https://www.googleapis.com/calendar/v3/calendars/" + item.Value + "/events/" + appointment.GoogleCalendarEventID);
                            RestClient restClient = new RestClient(url);
                            RestRequest request = new RestRequest();

                            request.AddQueryParameter("key", "AIzaSyASKpY6I08IVKFMw3muX39uMzPc5sBDaSc");
                            request.AddHeader("Authorization", "Bearer " + item.Key.AccessToken);
                            request.AddHeader("Accept", "application/json");

                            var response = restClient.Delete(request);
                            
                        }
                        else
                        {

                        }
                    }
                    
                }
                return Json(new { success = true }, JsonRequestBehavior.AllowGet);


            }
            catch (Exception ex)
            {

                return Json(new { success = false, Message = ex }, JsonRequestBehavior.AllowGet);
            }

        }

        [HttpPost]
        public JsonResult MakeAsNoShow(int ID)
        {
            var appointment = AppointmentServices.Instance.GetAppointment(ID);
            appointment.Status = "No Show";
            appointment.Color = "#E94234";
            AppointmentServices.Instance.UpdateAppointment(appointment);

            var customer = CustomerServices.Instance.GetCustomer(appointment.CustomerID);
            var LoggedInUser = UserManager.FindById(User.Identity.GetUserId());
            var employee = EmployeeServices.Instance.GetEmployee(appointment.EmployeeID);
            var company = CompanyServices.Instance.GetCompany(LoggedInUser.Company).FirstOrDefault();
            if (customer != null)
            {
                #region MailingRegion
                string ConcatenatedServices = "";
                foreach (var item in appointment.Service.Split(',').ToList())
                {
                    var Service = ServiceServices.Instance.GetService(int.Parse(item));
                    if (Service != null)
                    {
                        if (ConcatenatedServices == "")
                        {
                            ConcatenatedServices = String.Join(",", Service.Name);
                        }
                        else
                        {
                            ConcatenatedServices = String.Join(",", ConcatenatedServices, Service.Name);

                        }
                    }
                }
                var emailDetails = EmailTemplateServices.Instance.GetEmailTemplateWRTBusiness(LoggedInUser.Company, "No-Show");
                if (emailDetails != null && emailDetails.IsActive == true)
                {
                    string emailBody = "<html><body>";
                    emailBody += "<h2 style='font-family: Arial, sans-serif;'>Appointment Marked as No Show</h2>";
                    emailBody += emailDetails.TemplateCode;
                    emailBody = emailBody.Replace("{{Customer_first_name}}", customer.FirstName);
                    emailBody = emailBody.Replace("{{Customer_last_name}}", customer.LastName);
                    emailBody = emailBody.Replace("{{Customer_initial}}", customer.Gender == "Male" ? "Mr." : "Ms.");
                    emailBody = emailBody.Replace("{{date}}", appointment.Date.ToString("yyyy-MM-dd"));
                    emailBody = emailBody.Replace("{{time}}", appointment.Time.ToString("H:mm:ss"));
                    emailBody = emailBody.Replace("{{end_time}}", appointment.EndTime.ToString("H:mm:ss"));
                    emailBody = emailBody.Replace("{{employee}}", employee.Name);
                    emailBody = emailBody.Replace("{{employee_specialization}}", employee.Specialization);
                    emailBody = emailBody.Replace("{{employee_picture}}", $"<img class='text-center' style='height:50px;width:auto;' src='{"http://app.yourbookingplatform.com" + employee.Photo}'>");
                    emailBody = emailBody.Replace("{{services}}", ConcatenatedServices);
                    emailBody = emailBody.Replace("{{company_name}}", LoggedInUser.Company);
                    emailBody = emailBody.Replace("{{company_email}}", company.NotificationEmail);
                    emailBody = emailBody.Replace("{{company_address}}", company.Address);
                    emailBody = emailBody.Replace("{{company_logo}}", $"<img class='text-center' style='height:50px;width:auto;' src='{"http://app.yourbookingplatform.com" + company.Logo}'>");
                    emailBody = emailBody.Replace("{{company_phone}}", company.PhoneNumber);

                    emailBody += "</body></html>";
                    if (IsValidEmail(customer.Email))
                    {
                        SendEmail(customer.Email, "No Show", emailBody, company);
                    }
                }

                var history = new History();
                history.CustomerName = customer.FirstName + " " + customer.LastName;
                history.EmployeeName = employee.Name;
                history.Business = company.Business;
                history.AppointmentID = appointment.ID;
                history.Date = DateTime.Now;
                history.Name = "No Show";

                history.Note = "Appointment was marked as no show by the employee:" + LoggedInUser.Name;
                HistoryServices.Instance.SaveHistory(history);
                #endregion
            }
            return Json(new { success = true }, JsonRequestBehavior.AllowGet);
        }


        [HttpPost]
        public JsonResult UnMakeAsNoShow(int ID)
        {
            var appointment = AppointmentServices.Instance.GetAppointment(ID);
            appointment.Status = null;
            appointment.Color = "#5DAF4D";
            AppointmentServices.Instance.UpdateAppointment(appointment);

            return Json(new { success = true }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult DeletedAppointments(string StartDate = "", string EndDate = "")
        {
            AppointmentListingViewModel model = new AppointmentListingViewModel();
            if (StartDate != "" && EndDate != "")
            {
                model.StartDate = DateTime.Parse(StartDate);
                model.EndDate = DateTime.Parse(EndDate);
            }
            else
            {
                model.StartDate = DateTime.Now;
                model.EndDate = DateTime.Now;
            }
            var LoggedInUser = UserManager.FindById(User.Identity.GetUserId());
            var appointments = AppointmentServices.Instance.GetAllAppointmentWRTBusiness(LoggedInUser.Company, true).Where(x => x.Date >= model.StartDate && x.Date <= model.EndDate).ToList();
            var AppointmentModel = new List<AppointmentListModel>();
            foreach (var item in appointments)
            {
                var customer = CustomerServices.Instance.GetCustomer(item.CustomerID);
                if (item.Service != null)
                {
                    var ServiceListCommand = item.Service.Split(',').ToList();
                    var ServiceDuration = item.ServiceDuration.Split(',').ToList();


                    var serviceList = new List<ServiceModelForCustomerProfile>();
                    for (int i = 0; i < ServiceListCommand.Count && i < ServiceDuration.Count; i++)
                    {
                        var serivce = ServiceServices.Instance.GetService(int.Parse(ServiceListCommand[i]));
                        var serviceViewModel = new ServiceModelForCustomerProfile
                        {
                            Name = serivce.Name,
                            Duration = ServiceDuration[i],
                            ID = serivce.ID,
                        };

                        serviceList.Add(serviceViewModel);
                    }
                    if (customer == null)
                    {

                        AppointmentModel.Add(new AppointmentListModel { DeletedTime = item.DeletedTime, Color = item.Color, StartDate = item.Date, ID = item.ID, CustomerLastName = " ", EndTime = item.EndTime, StartTime = item.Time, CustomerFirstName = "Walk In", Services = serviceList });
                    }
                    else
                    {

                        AppointmentModel.Add(new AppointmentListModel { DeletedTime = item.DeletedTime, Color = item.Color, StartDate = item.Date, ID = item.ID, CustomerLastName = customer.LastName, EndTime = item.EndTime, StartTime = item.Time, CustomerFirstName = customer.FirstName, Services = serviceList });

                    }
                }
                else
                {

                    if (customer == null)
                    {

                        AppointmentModel.Add(new AppointmentListModel { DeletedTime = item.DeletedTime, Color = item.Color, StartDate = item.Date, ID = item.ID, CustomerLastName = " ", EndTime = item.EndTime, StartTime = item.Time, CustomerFirstName = "Walk In" });
                    }
                    else
                    {

                        AppointmentModel.Add(new AppointmentListModel { DeletedTime = item.DeletedTime, Color = item.Color, StartDate = item.Date, ID = item.ID, CustomerLastName = customer.LastName, EndTime = item.EndTime, StartTime = item.Time, CustomerFirstName = customer.FirstName });

                    }
                }

            }
            model.Appointments = AppointmentModel;
            return View(model);

        }

        [HttpGet]
        public ActionResult DeletePermanently(int ID)
        {
            var appointment = AppointmentServices.Instance.GetAppointment(ID);

            var LoggedInUser = UserManager.FindById(User.Identity.GetUserId());

            var customer = CustomerServices.Instance.GetCustomer(appointment.ID);
            var employee = EmployeeServices.Instance.GetEmployee(appointment.EmployeeID);
            var historyNew = new History();
            historyNew.Business = appointment.Business;
            historyNew.CustomerName = customer.FirstName + " " + customer.LastName;
            historyNew.Date = DateTime.Now;
            historyNew.AppointmentID = appointment.ID;
            historyNew.Note = "Appointment was permanently deleted:" + historyNew.CustomerName + "By :" + LoggedInUser.Name;
            historyNew.EmployeeName = employee.Name;
            HistoryServices.Instance.SaveHistory(historyNew);
            AppointmentServices.Instance.DeleteAppointment(ID);
            return RedirectToAction("DeletedAppointments", "Appointment");
        }


        [HttpGet]
        public ActionResult Restore(int ID)
        {
            var appointment = AppointmentServices.Instance.GetAppointment(ID);
            appointment.DELETED = false;
            AppointmentServices.Instance.UpdateAppointment(appointment);
            var LoggedInUser = UserManager.FindById(User.Identity.GetUserId());

            var customer = CustomerServices.Instance.GetCustomer(appointment.CustomerID);
            var employee = EmployeeServices.Instance.GetEmployee(appointment.EmployeeID);
            var history = new History();
            history.Note = "Deleted Appointment was Restored by: " + LoggedInUser.Name;
            history.Date = DateTime.Now;
            history.AppointmentID = appointment.ID;
            history.Business = LoggedInUser.Company;
            if (customer != null)
            {
                history.CustomerName = customer.FirstName + " " + customer.LastName;
            }
            else
            {
                history.CustomerName = "Walk In";

            }
            history.EmployeeName = employee.Name;
            history.Name = "Restored";
            HistoryServices.Instance.SaveHistory(history);

            return RedirectToAction("DeletedAppointments", "Appointment");
        }


        [HttpPost]
        public ActionResult Delete(AppointmentActionViewModel model)
        {
            var Appointment = AppointmentServices.Instance.GetAppointment(model.ID);
            Appointment.DELETED = true;
            Appointment.DeletedTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            AppointmentServices.Instance.UpdateAppointment(Appointment);
            var customer = CustomerServices.Instance.GetCustomer(Appointment.CustomerID);
            var employee = EmployeeServices.Instance.GetEmployee(Appointment.EmployeeID);

            var LoggedInUser = UserManager.FindById(User.Identity.GetUserId());
            var history = new History();
            history.CustomerName = customer.FirstName + " " + customer.LastName;
            history.EmployeeName = employee.Name;
            history.Business = LoggedInUser.Company;
            history.AppointmentID = Appointment.ID;
            history.Note = "Appointment was deleted by the employee: " + LoggedInUser.Name;
            history.Name = "Appointment Deleted";
            HistoryServices.Instance.SaveHistory(history);
            return Json(new { success = true }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult DeleteNewPost(int ID)
        {
            var Appointment = AppointmentServices.Instance.GetAppointment(ID);
            Appointment.DELETED = true;
            Appointment.DeletedTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

            AppointmentServices.Instance.UpdateAppointment(Appointment);

            var LoggedInUser = UserManager.FindById(User.Identity.GetUserId());

            var customer = CustomerServices.Instance.GetCustomer(Appointment.CustomerID);
            var employee = EmployeeServices.Instance.GetEmployee(Appointment.EmployeeID);
            var ToBeInputtedIDs = new Dictionary<GoogleCalendarIntegration, string>();
            //delete previous one
            RefreshToken(Appointment.Business);
            var googleKey = GoogleCalendarServices.Instance.GetGoogleCalendarServicesWRTBusiness(Appointment.Business);
            ToBeInputtedIDs.Add(googleKey, employee.GoogleCalendarID);
            var employeeRequest = EmployeeRequestServices.Instance.GetEmployeeRequestsWRTBusiness(Appointment.Business);
            var requestedEmployee = RequestedEmployeeServices.Instance.GetRequestedEmployeeWRTEmployeeID(employee.ID);
            if (employeeRequest.Any(x => x.EmployeeID == employee.ID))
            {
                if (requestedEmployee != null && requestedEmployee.GoogleCalendarID != null && requestedEmployee.GoogleCalendarID != "")
                {
                    RefreshToken(employee.Business);
                    googleKey = GoogleCalendarServices.Instance.GetGoogleCalendarServicesWRTBusiness(employee.Business);
                    ToBeInputtedIDs.Add(googleKey, requestedEmployee.GoogleCalendarID);
                }
            }
            var history = new History();
            if (customer != null)
            {

                history.Note = "Deleted Appointment was Deleted by: " + LoggedInUser.Name + " for Customer: " + customer.FirstName + " " + customer.LastName;
                history.Date = DateTime.Now;
                history.Business = LoggedInUser.Company;
                history.CustomerName = customer.FirstName + " " + customer.LastName;
                history.EmployeeName = employee.Name;
                history.AppointmentID = Appointment.ID;
                history.Name = "Appointment Deleted";
                HistoryServices.Instance.SaveHistory(history);

                foreach (var item in ToBeInputtedIDs)
                {
                    if (item.Key != null && !item.Key.Disabled)
                    {
                        var url = new System.Uri("https://www.googleapis.com/calendar/v3/calendars/" + item.Value + "/events/" + Appointment.GoogleCalendarEventID);
                        RestClient restClient = new RestClient(url);
                        RestRequest request = new RestRequest();

                        request.AddQueryParameter("key", "AIzaSyASKpY6I08IVKFMw3muX39uMzPc5sBDaSc");
                        request.AddHeader("Authorization", "Bearer " + item.Key.AccessToken);
                        request.AddHeader("Accept", "application/json");

                        var response = restClient.Delete(request);

                    }
                    
                }
                return Json(new { success = true }, JsonRequestBehavior.AllowGet);

            }
            else
            {
                history.Note = "Deleted Appointment was Deleted by: " + LoggedInUser.Name + " for Customer: Walk In";
                history.Date = DateTime.Now;
                history.Business = LoggedInUser.Company;
                history.CustomerName = "Walk In";
                history.EmployeeName = employee.Name;
                history.AppointmentID = Appointment.ID;
                HistoryServices.Instance.SaveHistory(history);


                foreach (var item in ToBeInputtedIDs)
                {
                    if (item.Key != null && !item.Key.Disabled)
                    {
                        var url = new System.Uri("https://www.googleapis.com/calendar/v3/calendars/" + item.Value + "/events/" + Appointment.GoogleCalendarEventID);
                        RestClient restClient = new RestClient(url);
                        RestRequest request = new RestRequest();

                        request.AddQueryParameter("key", "AIzaSyASKpY6I08IVKFMw3muX39uMzPc5sBDaSc");
                        request.AddHeader("Authorization", "Bearer " + item.Key.AccessToken);
                        request.AddHeader("Accept", "application/json");

                        var response = restClient.Delete(request);
                       
                    }
               
                }
                return Json(new { success = true }, JsonRequestBehavior.AllowGet);


            }




        }

        public ActionResult ActionPartial(string date, DateTime time, int employeeID = 0)
        {
            try
            {
                AppointmentListingViewModel model = new AppointmentListingViewModel();
                var ServicesList = new List<ServiceModel>();
                var LoggedInUser = UserManager.FindById(User.Identity.GetUserId());

                if (LoggedInUser == null) { return RedirectToAction("Login", "Account"); }
                model.Company = CompanyServices.Instance.GetCompany().Where(x => x.Business == LoggedInUser.Company).FirstOrDefault();

                if (LoggedInUser.Role != "Super Admin")
                {
                    //var categories = ServicesCategoriesServices.Instance.GetServiceCategoriesWRTBusiness(LoggedInUser.Company, "");
                    //foreach (var item in categories)
                    //{
                    //    var ServicesWRTCategory = ServiceServices.Instance.GetServiceWRTCategory(LoggedInUser.Company, item.Name).Where(x => x.IsActive).OrderBy(x => x.DisplayOrder).ToList();
                    //    ServicesList.Add(new ServiceModel { ServiceCategory = item, Services = ServicesWRTCategory, Company = model.Company });
                    //}
                    model.AbsenseServices = ServiceServices.Instance.GetServiceWRTCategory(LoggedInUser.Company, "ABSENSE").Where(x => x.IsActive).OrderBy(x => x.DisplayOrder).ToList();
                    var employees = EmployeeServices.Instance.GetEmployeeWRTBusiness(LoggedInUser.Company, true).OrderBy(x => x.DisplayOrder).ToList();

                    var employeerequests = EmployeeRequestServices.Instance.GetEmployeeRequestByBusiness(model.Company.ID);
                    foreach (var item in employeerequests)
                    {
                        if (item.Accepted)
                        {
                            var employeeCompany = EmployeeServices.Instance.GetEmployee(item.EmployeeID);
                            if (!employees.Select(x => x.ID).Contains(employeeCompany.ID))
                            {
                                employees.Add(employeeCompany);
                            }
                        }
                    }


                    model.EmployeesForAction = employees;


                }
                else
                {
                    //var categories = ServicesCategoriesServices.Instance.GetServiceCategories().OrderBy(x => x.DisplayOrder).ToList();
                    //foreach (var item in categories)
                    //{
                    //    var ServicesWRTCategory = ServiceServices.Instance.GetService().Where(x => x.Category == item.Name && x.IsActive).OrderBy(x => x.DisplayOrder).ToList();
                    //    ServicesList.Add(new ServiceModel { ServiceCategory = item, Services = ServicesWRTCategory, Company = model.Company });
                    //}
                    //model.AbsenseServices = ServiceServices.Instance.GetService().Where(x => x.Category == "ABSENSE" && x.IsActive).OrderBy(x => x.DisplayOrder).ToList();
                    model.EmployeesForAction = EmployeeServices.Instance.GetEmployee().Where(x => x.IsActive == true).OrderBy(x => x.DisplayOrder).ToList();
                }
                model.EmployeeID = employeeID;
                //model.Date = DateTime.Parse(date);

                model.Date = DateTime.Parse(date.Substring(4, 11));
                model.Time = time;
                //model.Services = ServicesList.OrderBy(X => X.ServiceCategory.DisplayOrder).ToList();
                return PartialView("_ActionPartial", model);
            }
            catch (Exception ex)
            {

                throw;
            }

        }
        public bool SendEmail(string toEmail, string subject, string emailBody, Company company)
        {
            try
            {
                string senderEmail = "support@yourbookingplatform.com";
                string senderPassword = "ttpa fcbl mpbn fxdl";

                int Port = int.Parse(ConfigurationManager.AppSettings["portforSmtp"]);
                string Host = ConfigurationManager.AppSettings["hostForSmtp"];
                System.Net.Mail.MailMessage mail = new System.Net.Mail.MailMessage();
                mail.To.Add(toEmail);
                MailAddress ccAddress = new MailAddress(company.NotificationEmail, company.Business);

                mail.CC.Add(ccAddress);
                mail.From = new MailAddress(company.NotificationEmail, company.Business, System.Text.Encoding.UTF8);
                mail.Subject = subject;
                mail.SubjectEncoding = System.Text.Encoding.UTF8;
                mail.Body = emailBody;
                mail.BodyEncoding = System.Text.Encoding.UTF8;
                mail.IsBodyHtml = true;
                mail.ReplyTo = new MailAddress(company.NotificationEmail); // Set the ReplyTo address

                mail.Priority = MailPriority.High;
                SmtpClient client = new SmtpClient();
                client.Credentials = new System.Net.NetworkCredential(senderEmail, senderPassword);
                client.Port = Port;
                client.Host = Host;
                client.EnableSsl = true;
                client.Send(mail);
                return true;
            }
            catch (Exception ex)
            {
                Session["EmailStatus"] = ex.ToString();
                return false;
            }

        }
        public ActionResult Export(string StartDate, string EndDate)
        {
            var LoggedInUser = UserManager.FindById(User.Identity.GetUserId()); if (LoggedInUser == null) { return RedirectToAction("Login", "Account"); }
            var Appointments = new List<Appointment>();
            var StartDat = DateTime.Parse(StartDate);
            var EndDat = DateTime.Parse(EndDate);
            if (LoggedInUser.Role != "Super Admin")
            {
                Appointments = AppointmentServices.Instance.GetAllAppointmentWRTBusiness(LoggedInUser.Company, false, StartDat, EndDat);

            }
            else
            {

                Appointments = AppointmentServices.Instance.GetAllAppointmentWRTBusiness(false)
                     .Where(X => X.Date >= StartDat && X.Date <= EndDat).ToList();
            }

            // Create a DataTable and populate it with the site data
            System.Data.DataTable tableData = new System.Data.DataTable();

            tableData.Columns.Add("ID", typeof(string));
            tableData.Columns.Add("Employee", typeof(string));
            tableData.Columns.Add("Employee Specialization", typeof(string));
            tableData.Columns.Add("Client Name", typeof(string));
            tableData.Columns.Add("Client Phone", typeof(string));
            tableData.Columns.Add("Client Email", typeof(string));
            tableData.Columns.Add("Visit Date", typeof(string));
            tableData.Columns.Add("Created Date", typeof(string));
            tableData.Columns.Add("Status", typeof(string));
            tableData.Columns.Add("Service Category", typeof(string));
            tableData.Columns.Add("Service Names", typeof(string));
            tableData.Columns.Add("Service Cost", typeof(string));
            tableData.Columns.Add("Service Discount", typeof(string));
            tableData.Columns.Add("Service Price After Discount", typeof(string));
            tableData.Columns.Add("Deposit Amount", typeof(string));
            tableData.Columns.Add("Deposit Method", typeof(string));
            tableData.Columns.Add("Notes", typeof(string));
            tableData.Columns.Add("ServiceDuration", typeof(string));
            tableData.Columns.Add("Online Price Change", typeof(string));
            tableData.Columns.Add("Amount Change", typeof(string));
            tableData.Columns.Add("Total After Price Change", typeof(string));
            tableData.Columns.Add("Cancelled", typeof(string));


            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            foreach (var Appointment in Appointments)
            {
                if (Appointment.Service != null)
                {
                    var appointmentServiceIds = Appointment.Service.Split(',').ToList();

                    if (appointmentServiceIds.Count() > 0)
                    {
                        if (Appointment.Service != null)
                        {
                            var ServiceListCommand = Appointment.Service.Split(',').ToList();
                            var ServiceDuration = Appointment.ServiceDuration.Split(',').ToList();
                            var ServiceDiscount = Appointment.ServiceDiscount.Split(',').ToList();

                            for (int i = 0; i < ServiceListCommand.Count && i < ServiceDuration.Count && i < ServiceDiscount.Count; i++)
                            {
                                if (i == 0)
                                {
                                    DataRow row = tableData.NewRow();
                                    var customer = CustomerServices.Instance.GetCustomer(Appointment.CustomerID);
                                    row["ID"] = Appointment.ID;
                                    var employee = EmployeeServices.Instance.GetEmployee(Appointment.EmployeeID);
                                    if (employee != null)
                                    {
                                        row["Employee"] = employee.Name;
                                        row["Employee Specialization"] = employee.Specialization;
                                    }
                                    else
                                    {
                                        row["Employee"] = "--";
                                        row["Employee Specialization"] = "--";
                                    }

                                    if (customer == null)
                                    {
                                        row["Client Name"] = "Walk In";
                                        row["Client Phone"] = "";
                                        row["Client Email"] = "";
                                    }
                                    else
                                    {
                                        row["Client Name"] = customer.FirstName + " " + customer.LastName;
                                        row["Client Phone"] = customer.MobileNumber;
                                        row["Client Email"] = customer.Email;
                                    }

                                    row["Visit Date"] = Appointment.Date.ToString("yyyy-MM-dd") + " -" + Appointment.Time.ToString("H:mm:ss");
                                    row["Created Date"] = Appointment.BookingDate.ToString("yyyy-MM-dd");
                                    row["Status"] = Appointment.Status;

                                    var service = ServiceServices.Instance.GetService(int.Parse(ServiceListCommand[i]));
                                    row["Service Category"] = service.Category;
                                    row["Service Names"] = service.Name;
                                    row["Service Cost"] = service.Price.ToString();
                                    row["Service Discount"] = float.Parse(ServiceDiscount[i]).ToString();
                                    row["Service Price After Discount"] = Convert.ToString(service.Price - (service.Price * float.Parse(ServiceDiscount[i]) / 100));
                                    row["Deposit Amount"] = Appointment.Deposit;
                                    row["Deposit Method"] = Appointment.DepositMethod;
                                    row["Notes"] = Appointment.Notes;
                                    row["ServiceDuration"] = ServiceDuration[i].ToString();

                                    var priceChange = PriceChangeServices.Instance.GetPriceChange(Appointment.OnlinePriceChange);
                                    if (priceChange != null)
                                    {
                                        row["Online Price Change"] = priceChange.TypeOfChange + " " + priceChange.Percentage + " %";
                                        if (priceChange.TypeOfChange == "Discount")
                                        {
                                            row["Amount Change"] = service.Price * (priceChange.Percentage / 100);
                                        }
                                        else
                                        {
                                            row["Amount Change"] = service.Price * (priceChange.Percentage / 100);
                                        }

                                        if (priceChange.TypeOfChange == "Discount")
                                        {
                                            var serviceminusdeposit = service.Price/* - Appointment.Deposit*/;
                                            var FinalTotal = serviceminusdeposit - (serviceminusdeposit * (priceChange.Percentage / 100));
                                            row["Total After Price Change"] = FinalTotal;

                                        }
                                        else
                                        {
                                            var serviceminusdeposit = service.Price/* - Appointment.Deposit*/;
                                            var FinalTotal = serviceminusdeposit + (serviceminusdeposit * (priceChange.Percentage / 100));
                                            row["Total After Price Change"] = FinalTotal;
                                        }

                                    }
                                    else
                                    {
                                        row["Online Price Change"] = "";
                                        row["Amount Change"] = 0;
                                        row["Total After Price Change"] = Convert.ToString(service.Price - (service.Price * float.Parse(ServiceDiscount[i]) / 100));
                                    }
                                    row["Cancelled"] = Appointment.IsCancelled.ToString();
                                    tableData.Rows.Add(row);
                                }
                                else
                                {


                                    // Create a row with no service information
                                    DataRow row = tableData.NewRow();

                                    row["ID"] = Appointment.ID;
                                    var employee = EmployeeServices.Instance.GetEmployee(Appointment.EmployeeID);
                                    if (employee != null)
                                    {
                                        row["Employee"] = employee.Name;
                                        row["Employee Specialization"] = employee.Specialization;
                                    }
                                    else
                                    {
                                        row["Employee"] = "--";
                                        row["Employee Specialization"] = "--";
                                    }
                                    var customer = CustomerServices.Instance.GetCustomer(Appointment.CustomerID);
                                    if (customer == null)
                                    {
                                        row["Client Name"] = "Walk In";
                                        row["Client Phone"] = "";
                                        row["Client Email"] = "";
                                    }
                                    else
                                    {
                                        row["Client Name"] = customer.FirstName + " " + customer.LastName;
                                        row["Client Phone"] = customer.MobileNumber;
                                        row["Client Email"] = customer.Email;
                                    }

                                    row["Visit Date"] = Appointment.Date.ToString("yyyy-MM-dd") + " -" + Appointment.Time.ToString("H:mm:ss");
                                    row["Created Date"] = Appointment.BookingDate.ToString("yyyy-MM-dd");
                                    row["Status"] = Appointment.Status;

                                    var service = ServiceServices.Instance.GetService(int.Parse(ServiceListCommand[i]));
                                    row["Service Category"] = service.Category;
                                    row["Service Names"] = service.Name;
                                    row["Service Cost"] = service.Price.ToString();
                                    row["Service Discount"] = "";
                                    row["Service Price After Discount"] = Convert.ToString(service.Price - (service.Price * float.Parse(ServiceDiscount[i]) / 100));

                                    row["ServiceDuration"] = ServiceDuration[i].ToString();

                                    row["Deposit Amount"] = "";
                                    row["Deposit Method"] = "";
                                    row["Notes"] = Appointment.Notes;
                                    var priceChange = PriceChangeServices.Instance.GetPriceChange(Appointment.OnlinePriceChange);
                                    if (priceChange != null)
                                    {
                                        row["Online Price Change"] = priceChange.TypeOfChange + " " + priceChange.Percentage + " %";
                                        if (priceChange.TypeOfChange == "Discount")
                                        {
                                            row["Amount Change"] = service.Price * (priceChange.Percentage / 100);
                                        }
                                        else
                                        {
                                            row["Amount Change"] = service.Price * (priceChange.Percentage / 100);
                                        }


                                        if (priceChange.TypeOfChange == "Discount")
                                        {
                                            var serviceminusdeposit = service.Price;
                                            var FinalTotal = serviceminusdeposit - (serviceminusdeposit * (priceChange.Percentage / 100));
                                            row["Total After Price Change"] = FinalTotal;
                                        }
                                        else
                                        {
                                            var serviceminusdeposit = service.Price;
                                            var FinalTotal = serviceminusdeposit + (serviceminusdeposit * (priceChange.Percentage / 100));
                                            row["Total After Price Change"] = FinalTotal;
                                        }

                                    }
                                    else
                                    {
                                        row["Online Price Change"] = "";
                                        row["Amount Change"] = 0;
                                        row["Total After Price Change"] = Convert.ToString(service.Price - (service.Price * float.Parse(ServiceDiscount[i]) / 100));
                                    }
                                    row["Cancelled"] = Appointment.IsCancelled.ToString();
                                    tableData.Rows.Add(row);

                                }
                            }

                        }
                    }
                }
                else
                {
                    DataRow row = tableData.NewRow();
                    var customer = CustomerServices.Instance.GetCustomer(Appointment.CustomerID);
                    row["ID"] = Appointment.ID;
                    var employee = EmployeeServices.Instance.GetEmployee(Appointment.EmployeeID);
                    if (employee != null)
                    {
                        row["Employee"] = employee.Name;
                        row["Employee Specialization"] = employee.Specialization;
                    }
                    else
                    {
                        row["Employee"] = "--";
                        row["Employee Specialization"] = "--";
                    }

                    if (customer == null)
                    {
                        row["Client Name"] = "Walk In";
                        row["Client Phone"] = "";
                        row["Client Email"] = "";
                    }
                    else
                    {
                        row["Client Name"] = customer.FirstName + " " + customer.LastName;
                        row["Client Phone"] = customer.MobileNumber;
                        row["Client Email"] = customer.Email;
                    }

                    row["Visit Date"] = Appointment.Date.ToString("yyyy-MM-dd") + " -" + Appointment.Time.ToString("H:mm:ss");
                    row["Created Date"] = Appointment.BookingDate.ToString("yyyy-MM-dd");
                    row["Status"] = Appointment.Status;


                    row["Service Category"] = "";
                    row["Service Names"] = "";
                    row["Service Cost"] = "";
                    row["Service Discount"] = Appointment.ServiceDiscount;
                    row["Service Price After Discount"] = "";
                    row["Deposit Amount"] = Appointment.Deposit;
                    row["Deposit Method"] = Appointment.DepositMethod;
                    row["Notes"] = Appointment.Notes;
                    row["ServiceDuration"] = Appointment.ServiceDuration;

                    var priceChange = PriceChangeServices.Instance.GetPriceChange(Appointment.OnlinePriceChange);
                    if (priceChange != null)
                    {
                        row["Online Price Change"] = priceChange.TypeOfChange + " " + priceChange.Percentage + " %";
                        if (priceChange.TypeOfChange == "Discount")
                        {
                            row["Amount Change"] = "";
                        }
                        else
                        {
                            row["Amount Change"] = "";
                        }

                        if (priceChange.TypeOfChange == "Discount")
                        {
                            var serviceminusdeposit = 0;
                            var FinalTotal = serviceminusdeposit - (serviceminusdeposit * (priceChange.Percentage / 100));
                            row["Total After Price Change"] = FinalTotal;

                        }
                        else
                        {
                            var serviceminusdeposit = 0;
                            var FinalTotal = serviceminusdeposit + (serviceminusdeposit * (priceChange.Percentage / 100));
                            row["Total After Price Change"] = FinalTotal;
                        }

                    }
                    else
                    {
                        row["Online Price Change"] = "";
                        row["Amount Change"] = 0;
                        row["Total After Price Change"] = 0;
                    }
                    row["Cancelled"] = Appointment.IsCancelled.ToString();
                    tableData.Rows.Add(row);
                }
            }

            using (ExcelPackage package = new ExcelPackage())
            {
                // Create a new worksheet
                ExcelWorksheet worksheet = package.Workbook.Worksheets.Add("Appointments");

                // Set the column names
                for (int i = 0; i < tableData.Columns.Count; i++)
                {
                    worksheet.Cells[1, i + 1].Value = tableData.Columns[i].ColumnName;
                }

                // Set the row data
                for (int row = 0; row < tableData.Rows.Count; row++)
                {
                    for (int col = 0; col < tableData.Columns.Count; col++)
                    {
                        worksheet.Cells[row + 2, col + 1].Value = tableData.Rows[row][col];
                    }
                }

                // Auto-fit columns for better readability
                worksheet.Cells.AutoFitColumns();

                // Convert the Excel package to a byte array
                byte[] excelBytes = package.GetAsByteArray();

                // Return the Excel file as a downloadable file
                return File(excelBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Appointments.xlsx");
            }
        }


        public ActionResult Unsubscribe(int CustomerID)
        {
            var customer = CustomerServices.Instance.GetCustomer(CustomerID);
            customer.HaveNewsLetter = false;
            CustomerServices.Instance.UpdateCustomer(customer);
            return View();
        }



        [HttpPost]
        public ActionResult ActionEvent(DateTime Date, string Time, int Service, int EmployeeID, bool AllEmployees, string Notes, string Color,
                                bool IsRepeat, string Frequency, string Every, string NumberOfTimes, string EndWeek, List<string> On,
                                string EndDate, string EndDay, string EndMonth, string OnThe)
        {
            var LoggedInUser = UserManager.FindById(User.Identity.GetUserId());
            if (LoggedInUser == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var service = ServiceServices.Instance.GetService(Service);
            int ID = 0;

            if (AllEmployees)
            {
                var allEmployeesUsage = EmployeeServices.Instance.GetEmployeeWRTBusiness(LoggedInUser.Company, true)
                                                                .OrderBy(x => x.DisplayOrder)
                                                                .ToList();
                foreach (var emp in allEmployeesUsage)
                {
                    CreateAppointment(emp.ID, Date, Time, Service, Notes, Color, LoggedInUser, service, false);
                }

                return Json(new { success = true }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                if (IsRepeat)
                {
                    var EveryData = Every.Replace("Weeks", "").Replace("Week", "").Replace("Days", "").Replace("Day", "").Replace("Months", "").Replace("Month", "").Trim();
                    int everyWeeks = int.Parse(EveryData);
                    int? numberOfTimes = !string.IsNullOrEmpty(NumberOfTimes) ? int.Parse(NumberOfTimes) : (int?)null;
                    DateTime? specificEndDate = !string.IsNullOrEmpty(EndDate) ? DateTime.Parse(EndDate) : (DateTime?)null;

                    ScheduleEvent(Frequency, everyWeeks, On, EndWeek, numberOfTimes, specificEndDate, Date, Time, Service, EmployeeID, Notes, Color, LoggedInUser, service, OnThe);
                }
                else
                {
                    CreateAppointment(EmployeeID, Date, Time, Service, Notes, Color, LoggedInUser, service, false);
                }

                return Json(new { success = true }, JsonRequestBehavior.AllowGet);
            }
        }
        private async Task<int> CreateAppointment(int employeeId, DateTime date, string time, int serviceId, string notes, string color, User loggedInUser, Entities.Service service, bool IsRepeat, string frequency = "", int every = 0, string ends = "", int? numberofTimes = 0, string SpecificEndDate = "", int RepeatedAppoitnmentID = 0)
        {
            var appointment = new Appointment
            {
                Business = loggedInUser.Company,
                BookingDate = DateTime.Now,
                Date = date,
                Deposit = 0,
                DepositMethod = string.Empty,
                Notes = notes,
                IsRepeat = IsRepeat,
                IsPaid = true,
                Color = color,
                EmployeeID = employeeId,
                FirstRepeatedID = RepeatedAppoitnmentID != 0 ? RepeatedAppoitnmentID : 0,
                Every = every.ToString(),
                Frequency = frequency,
                Ends = "Ends:" + ends + " || Number of Times:" + numberofTimes + " || End Date" + SpecificEndDate,
                FromGCAL = false,

                Service = serviceId.ToString()
            };

            string startTimeStr = time.Split('_')[0];
            string endTimeStr = time.Split('_')[1];

            DateTime startTime = DateTime.Parse(startTimeStr);
            DateTime endTime = DateTime.Parse(endTimeStr);

            TimeSpan duration = endTime - startTime;
            appointment.Time = startTime;
            appointment.ServiceDuration = duration.TotalMinutes.ToString();
            appointment.EndTime = endTime;
            appointment.ServiceDiscount = "0";
            appointment.TotalCost = 0;
            appointment.Color = "darkgray";
            appointment.Status = "Pending";

            AppointmentServices.Instance.SaveAppointment(appointment);

            var history = new History();
            var employee = EmployeeServices.Instance.GetEmployee(employeeId);

            history.Business = loggedInUser.Company;
            history.CustomerName = "Walk In";
            history.Date = DateTime.Now;
            history.Type = "Absense";
            history.AppointmentID = appointment.ID;
            history.EmployeeName = employee.Name;
            history.Note = $"Event Saved for: {history.EmployeeName} by {loggedInUser.Name}";
            history.Name = "Appointment Created";
            HistoryServices.Instance.SaveHistory(history);


            var googleCalendar = GoogleCalendarServices.Instance.GetGoogleCalendarServicesWRTBusiness(loggedInUser.Company);
            if (googleCalendar != null && !googleCalendar.Disabled)
            {
                string googleMessage = GenerateonGoogleCalendar(appointment.ID, service.Name);
                // Handle the Google Calendar integration messages as needed
            }
            return appointment.ID;
        }

        static List<DayOfWeek> CreateDayOfWeekList(List<string> daysString)
        {
            // Create a list of DayOfWeek enums by parsing the strings
            List<DayOfWeek> daysOfWeek = new List<DayOfWeek>();

            foreach (string day in daysString)
            {
                // Parse each day and add to the list
                daysOfWeek.Add((DayOfWeek)Enum.Parse(typeof(DayOfWeek), day, true));
            }

            return daysOfWeek;
        }


        DateTime GetNextOccurrenceDate(DateTime startDate, List<DayOfWeek> targetDays)
        {
            // Sort the target days of the week based on their position relative to today
            var sortedDays = targetDays
                .Select(day => new
                {
                    Day = day,
                    DaysToAdd = ((int)day - (int)startDate.DayOfWeek + 7) % 7
                })
                .OrderBy(d => d.DaysToAdd)
                .ToList();

            // Find the first day after today
            var nextDay = sortedDays.FirstOrDefault(d => d.DaysToAdd > 0);

            if (nextDay != null)
            {
                // Return the next occurrence in the current week
                return startDate.AddDays(nextDay.DaysToAdd);
            }
            else
            {
                // All target days are either today or before today, so return the first day from the next week
                return startDate.AddDays(sortedDays.First().DaysToAdd + 7);
            }
        }



        private async Task<string> ScheduleEvent(string frequency, int every, List<string> daysOfWeek, string ends, int? numberOfTimes, DateTime? specificEndDate,
                                   DateTime startDate, string time, int serviceId, int employeeId, string notes, string color,
                                   User loggedInUser, Entities.Service service, string OnThe = "")
        {
            var nextEventDate = startDate;
            int FirstAppointmentID = 0;
            int Occurences = 1;
            var EventSwitchID = 0;
            int TimesCount = 1;
            if (ends == "Never")
            {
                specificEndDate = startDate.AddMonths(12);
            }
            if (frequency == "Every Week")
            {
                while (true)
                {

                    if (TimesCount > every)
                    {
                        break;
                    }
                    if (/*ends == "NumberOfTimes" && */Occurences >= numberOfTimes)
                    {
                        break;
                    }
                    else if (/*ends == "Specific Date" &&*/ nextEventDate >= specificEndDate)
                    {
                        break;
                    }
                    else if (/*ends == "Never" &&*/ nextEventDate >= specificEndDate)
                    {
                        break;
                    }
                    else
                    {
                        if (EventSwitchID != 0)
                        {
                            var eventSwitchDate = new EventSwitchDate();
                            eventSwitchDate.Business = loggedInUser.Company;
                            eventSwitchDate.Date = nextEventDate.Date;
                            eventSwitchDate.EventID = EventSwitchID;
                            EventSwitchServices.Instance.SaveEventSwitchDate(eventSwitchDate);
                        }
                        if (Occurences == 1)
                        {
                            var ID = await CreateAppointment(employeeId, nextEventDate, time, serviceId, notes, color, loggedInUser, service, true, frequency, every, ends, numberOfTimes, specificEndDate.ToString(), FirstAppointmentID);
                            FirstAppointmentID = ID;
                            var eventSwitch = new EventSwitch();
                            eventSwitch.AppointmentID = ID;
                            eventSwitch.SwitchStatus = true;
                            eventSwitch.Business = loggedInUser.Company;
                            EventSwitchServices.Instance.SaveEventSwitch(eventSwitch);
                            EventSwitchID = eventSwitch.ID;
                        }
                        if (Occurences % daysOfWeek.Count() == 0)
                        {
                            TimesCount++;
                        }
                        Occurences++;
                        List<DayOfWeek> daysOfWeeks = CreateDayOfWeekList(daysOfWeek);
                        nextEventDate = GetNextOccurrenceDate(nextEventDate, daysOfWeeks);


                    }

                }
            }
            else if (frequency == "Every Day")
            {
                while (true)
                {
                    if (TimesCount > every)
                    {
                        break;
                    }
                    if (Occurences >= numberOfTimes)
                    {
                        break;
                    }
                    else if (nextEventDate >= specificEndDate)
                    {
                        break;
                    }
                    else if (nextEventDate >= specificEndDate)
                    {
                        break;
                    }
                    else
                    {
                        if (EventSwitchID != 0)
                        {
                            var eventSwitchDate = new EventSwitchDate();
                            eventSwitchDate.Business = loggedInUser.Company;
                            eventSwitchDate.Date = nextEventDate.Date;
                            eventSwitchDate.EventID = EventSwitchID;
                            EventSwitchServices.Instance.SaveEventSwitchDate(eventSwitchDate);
                        }
                        if (Occurences == 1)
                        {
                            var ID = await CreateAppointment(employeeId, nextEventDate, time, serviceId, notes, color, loggedInUser, service, true, frequency, every, ends, numberOfTimes, specificEndDate.ToString(), FirstAppointmentID);
                            FirstAppointmentID = ID;
                            var eventSwitch = new EventSwitch();
                            eventSwitch.AppointmentID = ID;
                            eventSwitch.SwitchStatus = true;
                            eventSwitch.Business = loggedInUser.Company;
                            EventSwitchServices.Instance.SaveEventSwitch(eventSwitch);
                        }
                        if (Occurences % daysOfWeek.Count() == 0)
                        {
                            TimesCount++;
                        }
                        Occurences++;
                        nextEventDate = nextEventDate.AddDays(1);

                    }
                }
            }
            else if (frequency == "Every Month")
            {
                while (true)
                {
                    if (TimesCount > every)
                    {
                        break;
                    }
                    if (Occurences >= numberOfTimes)
                    {
                        break;
                    }
                    else if (nextEventDate >= specificEndDate)
                    {
                        break;
                    }
                    else if (nextEventDate >= specificEndDate)
                    {
                        break;
                    }
                    else
                    {
                        if (EventSwitchID != 0)
                        {
                            var eventSwitchDate = new EventSwitchDate();
                            eventSwitchDate.Business = loggedInUser.Company;
                            eventSwitchDate.Date = nextEventDate;
                            eventSwitchDate.EventID = EventSwitchID;
                            EventSwitchServices.Instance.SaveEventSwitchDate(eventSwitchDate);
                        }
                        if (Occurences == 1)
                        {
                            var ID = await CreateAppointment(employeeId, nextEventDate, time, serviceId, notes, color, loggedInUser, service, true, frequency, every, ends, numberOfTimes, specificEndDate.ToString(), FirstAppointmentID);
                            FirstAppointmentID = ID;
                            var eventSwitch = new EventSwitch();
                            eventSwitch.AppointmentID = ID;
                            eventSwitch.SwitchStatus = true;
                            eventSwitch.Business = loggedInUser.Company;
                            EventSwitchServices.Instance.SaveEventSwitch(eventSwitch);
                        }
                        if (Occurences % daysOfWeek.Count() == 0)
                        {
                            TimesCount++;
                        }
                        Occurences++;
                        if (OnThe == "23rd")
                        {
                            DateTime nextMonth = nextEventDate.AddMonths(1);
                            nextEventDate = new DateTime(nextMonth.Year, nextMonth.Month, 23);

                        }
                        else if (OnThe == "4th Saturday")
                        {
                            DateTime firstDayNextMonth = new DateTime(nextEventDate.Year, nextEventDate.Month, 1).AddMonths(1);
                            int daysToAdd = ((DayOfWeek.Saturday - firstDayNextMonth.DayOfWeek + 7) % 7) + (3 * 7); // 3 weeks after the first Saturday
                            nextEventDate = firstDayNextMonth.AddDays(daysToAdd);
                        }
                        else
                        {
                            break;
                        }
                    }
                }
            }

            return "Its Done";


        }



        //[HttpPost]
        //public ActionResult ActionEvent(DateTime Date, string Time, int Service, int EmployeeID, bool AllEmployees, string Notes, string Color,
        //bool IsRepeat,      
        //string Frequency,   
        //string Every,
        //string NumberOfTimes,
        //string EndWeek,
        //List<string> On,
        //string EndDate,
        //string EndDay,
        //string EndMonth,
        //string OnThe)
        //{
        //    var LoggedInUser = UserManager.FindById(User.Identity.GetUserId()); if (LoggedInUser == null) { return RedirectToAction("Login", "Account"); }
        //    var service = ServiceServices.Instance.GetService(Service);
        //    int ID = 0;
        //    if (AllEmployees == true)
        //    {
        //        var AllEmployeesUsage = EmployeeServices.Instance.GetEmployeeWRTBusiness(LoggedInUser.Company, true).OrderBy(x => x.DisplayOrder).ToList();
        //        foreach (var emp in AllEmployeesUsage)
        //        {
        //            var appointment = new Appointment();
        //            appointment.Business = LoggedInUser.Company;
        //            appointment.BookingDate = DateTime.Now;
        //            appointment.Date = Date;



        //            //appointment.Time = Time;
        //            appointment.Deposit = 0;
        //            appointment.DepositMethod = "";
        //            appointment.Notes = Notes;
        //            appointment.Color = Color;
        //            appointment.EmployeeID = emp.ID;
        //            appointment.Every = "";
        //            appointment.Ends = "Never";
        //            appointment.Service = Service.ToString();
        //            string startTimeStr = Time.Split('_')[0];
        //            string endTimeStr = Time.Split('_')[1];

        //            // Parsing start and end time strings to DateTime
        //            DateTime startTime = DateTime.Parse(startTimeStr);
        //            DateTime endTime = DateTime.Parse(endTimeStr);

        //            // Calculating time duration
        //            TimeSpan duration = endTime - startTime;
        //            appointment.Time = startTime;

        //            appointment.ServiceDuration = duration.TotalMinutes.ToString();
        //            int MinsToBeAddedForEndTime = duration.Minutes;
        //            appointment.EndTime = endTime;
        //            appointment.ServiceDiscount = "0";
        //            appointment.TotalCost = 0;
        //            appointment.Color = "darkgray";
        //            AppointmentServices.Instance.SaveAppointment(appointment);
        //            ID = appointment.ID;
        //            var history = new History();
        //            history.Business = LoggedInUser.Company;
        //            history.CustomerName = "Walk In";
        //            history.Date = DateTime.Now;
        //            history.Type = "Absense";
        //            history.EmployeeName = EmployeeServices.Instance.GetEmployee(emp.ID).Name;
        //            history.Note = "Event Saved for:" + history.EmployeeName + " by " + LoggedInUser.Name;
        //            HistoryServices.Instance.SaveHistory(history);

        //            var googleCalendar = GoogleCalendarServices.Instance.GetGoogleCalendarServicesWRTBusiness(LoggedInUser.Company);
        //            var mesage = "";
        //            var Message = "";
        //            if (googleCalendar != null && !googleCalendar.Disabled)
        //            {
        //                Message = RefreshToken(LoggedInUser.Company);
        //                mesage = GenerateonGoogleCalendar(appointment.ID, service.Name);
        //            }
        //        }

        //        return Json(new { success = true }, JsonRequestBehavior.AllowGet);
        //    }
        //    else
        //    {
        //        if (IsRepeat)
        //        {

        //        }
        //        else
        //        {
        //            #region CreateEventRegion
        //            var appointment = new Appointment();
        //            appointment.Business = LoggedInUser.Company;
        //            appointment.BookingDate = DateTime.Now;
        //            appointment.Date = Date;
        //            //appointment.Time = Time;
        //            appointment.Deposit = 0;
        //            appointment.DepositMethod = "";
        //            appointment.EmployeeID = EmployeeID;
        //            appointment.Every = "";
        //            appointment.Ends = "Never";
        //            appointment.Notes = Notes;
        //            appointment.Color = Color;
        //            appointment.Service = Service.ToString();

        //            string startTimeStr = Time.Split('_')[0];
        //            string endTimeStr = Time.Split('_')[1];

        //            // Parsing start and end time strings to DateTime
        //            DateTime startTime = DateTime.Parse(startTimeStr);
        //            DateTime endTime = DateTime.Parse(endTimeStr);

        //            // Calculating time duration
        //            TimeSpan duration = endTime - startTime;
        //            appointment.Time = startTime;
        //            appointment.ServiceDuration = duration.TotalMinutes.ToString();
        //            double MinsToBeAddedForEndTime = duration.TotalMinutes;
        //            appointment.EndTime = endTime;
        //            appointment.ServiceDiscount = "0";
        //            appointment.TotalCost = 0;
        //            appointment.Color = "darkgray";
        //            AppointmentServices.Instance.SaveAppointment(appointment);

        //            var history = new History();
        //            history.Business = LoggedInUser.Company;
        //            history.CustomerName = "Walk In";
        //            history.Date = DateTime.Now;
        //            history.Type = "Absense";
        //            history.EmployeeName = EmployeeServices.Instance.GetEmployee(EmployeeID).Name;
        //            history.Note = "Event Saved for:" + history.EmployeeName + " by " + LoggedInUser.Name;
        //            HistoryServices.Instance.SaveHistory(history);

        //            var googleCalendar = GoogleCalendarServices.Instance.GetGoogleCalendarServicesWRTBusiness(LoggedInUser.Company);
        //            var mesage = "";
        //            var Message = "";
        //            if (googleCalendar != null && !googleCalendar.Disabled)
        //            {
        //                Message = RefreshToken(LoggedInUser.Company);
        //                mesage = GenerateonGoogleCalendar(appointment.ID, service.Name);
        //            }
        //            return Json(new { success = true, Mesage = mesage + " " + Message }, JsonRequestBehavior.AllowGet);
        //            #endregion

        //        }
        //        return Json(new { success = true }, JsonRequestBehavior.AllowGet);






        //    }



        //}



        [HttpPost]
        public ActionResult ActionEventUpdate(int ID, DateTime Date, string Time, int Service, int EmployeeID, bool AllEmployees, string Notes, string Color)
        {
            var LoggedInUser = UserManager.FindById(User.Identity.GetUserId()); if (LoggedInUser == null) { return RedirectToAction("Login", "Account"); }

            var appointment = AppointmentServices.Instance.GetAppointment(ID);
            appointment.Business = LoggedInUser.Company;
            appointment.BookingDate = DateTime.Now;
            appointment.Date = Date;
            appointment.Deposit = 0;
            appointment.DepositMethod = "";
            appointment.EmployeeID = EmployeeID;
            appointment.Every = "";
            appointment.Ends = "Never";
            appointment.Notes = Notes;
            appointment.Color = Color;
            appointment.Service = Service.ToString();


            string startTimeStr = Time.Split('_')[0];
            string endTimeStr = Time.Split('_')[1];

            // Parsing start and end time strings to DateTime
            DateTime startTime = DateTime.Parse(startTimeStr);
            DateTime endTime = DateTime.Parse(endTimeStr);

            // Calculating time duration
            TimeSpan duration = endTime - startTime;
            appointment.Time = startTime;
            appointment.ServiceDuration = duration.TotalMinutes.ToString();
            int MinsToBeAddedForEndTime = duration.Minutes;
            appointment.EndTime = endTime;



            appointment.ServiceDiscount = "0";
            appointment.TotalCost = 0;
            appointment.Color = "darkgray";
            AppointmentServices.Instance.UpdateAppointment(appointment);

            var history = new History();
            history.Business = LoggedInUser.Company;
            history.CustomerName = "Walk In";
            history.Type = "Absense";
            history.AppointmentID = appointment.ID;
            history.Date = DateTime.Now;
            history.EmployeeName = EmployeeServices.Instance.GetEmployee(EmployeeID).Name;
            history.Note = "Event Update for:" + history.EmployeeName + " by " + LoggedInUser.Name;
            history.Name = "Appoitnement Updated";
            HistoryServices.Instance.SaveHistory(history);


            var calendarEvent = new Event();
            int year = appointment.Date.Year;
            int month = appointment.Date.Month;
            int day = appointment.Date.Day;
            int starthour = appointment.Time.Hour;
            int startminute = appointment.Time.Minute;
            int startseconds = appointment.Time.Second;

            int endhour = appointment.EndTime.Hour;
            int endminute = appointment.EndTime.Minute;
            int endseconds = appointment.EndTime.Second;
            var employee = EmployeeServices.Instance.GetEmployee(appointment.EmployeeID);
            var ToBeInputtedIDs = new Dictionary<GoogleCalendarIntegration, string>();
            //delete previous one
            RefreshToken(appointment.Business);
            var googleKey = GoogleCalendarServices.Instance.GetGoogleCalendarServicesWRTBusiness(appointment.Business);
            ToBeInputtedIDs.Add(googleKey, employee.GoogleCalendarID);
            var employeeRequest = EmployeeRequestServices.Instance.GetEmployeeRequestsWRTBusiness(appointment.Business);
            var requestedEmployee = RequestedEmployeeServices.Instance.GetRequestedEmployeeWRTEmployeeID(employee.ID);
            if (employeeRequest.Any(x => x.EmployeeID == employee.ID))
            {
                if (requestedEmployee != null && requestedEmployee.GoogleCalendarID != null && requestedEmployee.GoogleCalendarID != "")
                {
                    RefreshToken(employee.Business);
                    googleKey = GoogleCalendarServices.Instance.GetGoogleCalendarServicesWRTBusiness(employee.Business);
                    ToBeInputtedIDs.Add(googleKey, requestedEmployee.GoogleCalendarID);
                }
            }
            var company = CompanyServices.Instance.GetCompany().Where(x => x.Business == LoggedInUser.Company).FirstOrDefault();
            var service = ServiceServices.Instance.GetService(int.Parse(appointment.Service));
            foreach (var item in ToBeInputtedIDs)
            {
                if (item.Key != null && !item.Key.Disabled)
                {
                    DateTime startDateNew = new DateTime(year, month, day, starthour, startminute, startseconds);
                    DateTime EndDateNew = new DateTime(year, month, day, endhour, endminute, endseconds);
                    var url = new System.Uri("https://www.googleapis.com/calendar/v3/calendars/" + item.Value + "/events/" + appointment.GoogleCalendarEventID);
                    RestClient restClient = new RestClient(url);
                    RestRequest request = new RestRequest();

                    calendarEvent.Start = new EventDateTime() { DateTime = startDateNew.ToString("yyyy-MM-dd'T'HH:mm:ss"), TimeZone = company.TimeZone };
                    calendarEvent.End = new EventDateTime() { DateTime = EndDateNew.ToString("yyyy-MM-dd'T'HH:mm:ss"), TimeZone = company.TimeZone };
                    calendarEvent.Description = service.Name;
                    calendarEvent.Summary = "Appointment at: " + LoggedInUser.Company;


                    var model = JsonConvert.SerializeObject(calendarEvent, new JsonSerializerSettings
                    {
                        ContractResolver = new CamelCasePropertyNamesContractResolver()
                    });

                    request.AddQueryParameter("key", "AIzaSyASKpY6I08IVKFMw3muX39uMzPc5sBDaSc");
                    request.AddHeader("Authorization", "Bearer " + item.Key.AccessToken);
                    request.AddHeader("Accept", "application/json");
                    request.AddHeader("Content-Type", "application/json");
                    request.AddParameter("application/json", model, ParameterType.RequestBody);

                    var resultant = restClient.Patch(request);

                    if (resultant.StatusCode == System.Net.HttpStatusCode.OK)
                    {

                    }
                }

            }
            return Json(new { success = true }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult ActionEvent(int ID)
        {
            AppointmentActionViewModel model = new AppointmentActionViewModel();
            var ServicesList = new List<ServiceModel>();
            var LoggedInUser = UserManager.FindById(User.Identity.GetUserId());
            if (LoggedInUser == null) { return RedirectToAction("Login", "Account"); }
            var company = CompanyServices.Instance.GetCompany().Where(x => x.Business == LoggedInUser.Company).FirstOrDefault();
            if (LoggedInUser.Role != "Super Admin")
            {

                model.AbsenseServices = ServiceServices.Instance.GetServiceWRTCategory(LoggedInUser.Company, "ABSENSE").Where(x => x.IsActive).OrderBy(x => x.DisplayOrder).ToList();
                model.Employees = EmployeeServices.Instance.GetEmployeeWRTBusiness(LoggedInUser.Company, true).OrderBy(x => x.DisplayOrder).ToList();
            }
            else
            {
                model.AbsenseServices = ServiceServices.Instance.GetServiceWRTCategory("ABSENSE").OrderBy(x => x.DisplayOrder).Where(x => x.IsActive).ToList();
                model.Employees = EmployeeServices.Instance.GetEmployeeWRTBusiness(true).OrderBy(x => x.DisplayOrder).ToList();
            }


            model.Company = company;
            model.Services = ServicesList.OrderBy(X => X.ServiceCategory.DisplayOrder).ToList();
            if (ID != 0)
            {
                var appointment = AppointmentServices.Instance.GetAppointment(ID);
                model.ID = ID;
                model.Service = appointment.Service;
                model.ServiceDuration = appointment.ServiceDuration;
                model.Date = appointment.Date;
                model.Time = appointment.Time;
                model.Color = appointment.Color;
                model.IsRepeat = appointment.IsRepeat;
                model.EndTime = appointment.EndTime;
                model.ID = appointment.ID;
                model.EmployeeID = appointment.EmployeeID;
                model.Frequency = appointment.Frequency;
                model.Deposit = appointment.Deposit;
                model.Notes = appointment.Notes;
                model.TotalCost = appointment.TotalCost;
                model.BookingDate = appointment.BookingDate;
                model.Label = appointment.Label;
                model.Status = appointment.Status;
            }
            return View(model);
        }

    }
}
