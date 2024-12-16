using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using TheBookingPlatform.Services;
using TheBookingPlatform.ViewModels;
using TheBookingPlatform.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TheBookingPlatform.Models;
using System.Web.Routing;
using System.Globalization;
using Newtonsoft.Json;
using static TheBookingPlatform.Controllers.EmployeeRequestController;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Notification = TheBookingPlatform.Entities.Notification;
using System.ComponentModel.Design;

namespace TheBookingPlatform.Controllers
{
    public class AdminController : Controller
    {
        private AMSignInManager _signInManager;
        private AMRolesManager _rolesManager;
        private AMUserManager _userManager;
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
        // GET: Admin
        public ActionResult Index()
        {
            AdminViewModel model = new AdminViewModel();
            var user = UserManager.FindById(User.Identity.GetUserId());
            model.Name = user.Name;
            return View(model);
        }

        public JsonResult CheckEmployee()
        {
            if (User.IsInRole("Calendar"))
            {
                var loggedInUser = UserManager.FindById(User.Identity.GetUserId());
                var employee = EmployeeServices.Instance.GetEmployeeWithLinkedUserID(loggedInUser.Id);
                if (employee != null)
                {
                    if (employee.Type == "Do not Display Payroll")
                    {
                        return Json(new { success = false }, JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        return Json(new { success = true }, JsonRequestBehavior.AllowGet);

                    }
                }
                else
                {
                    var company = CompanyServices.Instance.GetCompany().Where(x => x.Business == loggedInUser.Company).FirstOrDefault();
                    var employeeRequest = EmployeeRequestServices.Instance.GetEmployeeRequestByBusiness(company.ID);
                    foreach (var item in employeeRequest)
                    {
                        if (item.Accepted)
                        {
                            var empRequestedEmployee = EmployeeServices.Instance.GetEmployee(item.EmployeeID);
                            var employee1 = EmployeeServices.Instance.GetEmployeeWithLinkedUserID(empRequestedEmployee.LinkedEmployee);
                            if (empRequestedEmployee.ID == employee1.ID)
                            {
                                if (empRequestedEmployee.Type == "Do not Display Payroll")
                                {
                                    return Json(new { success = false }, JsonRequestBehavior.AllowGet);
                                }
                                else
                                {
                                    return Json(new { success = true }, JsonRequestBehavior.AllowGet);

                                }
                            }

                        }
                    }

                    return Json(new { success = true }, JsonRequestBehavior.AllowGet);
                }
            }
            else
            {
                return Json(new { success = true }, JsonRequestBehavior.AllowGet);

            }
        }

        
        public JsonResult GetLostClietCustomers(string FilterDuration,DateTime StartDate, DateTime EndDate)
        {
            var numberOfDays = 30;
            var user = UserManager.FindById(User.Identity.GetUserId());

            if (FilterDuration != "")
            {
                switch (FilterDuration)
                {
                    case "30 days":
                        numberOfDays = 30;
                        break;
                    case "60 days":
                        numberOfDays = 60;
                        break;
                    case "3 months":
                        numberOfDays = 90; // Approximate for 3 months
                        break;
                    case "6 months":
                        numberOfDays = 180; // Approximate for 6 months
                        break;
                    case "1 year":
                        numberOfDays = 365;
                        break;
                    case "2 years":
                        numberOfDays = 730; // 2 * 365
                        break;
                    default:
                        // Handle unexpected value or keep numberOfDays as 0
                        break;
                }

                var currentDate = StartDate;
                var CurrentDatePrev = currentDate.AddDays(-numberOfDays);
                List<Customer> LostClientsList = new List<Customer>();
                if (user != null)
                {
                    var customers = CustomerServices.Instance.GetCustomerWRTBusiness(user.Company);
                    var lostClients = new List<int>();

                    var lostClientIds = AppointmentServices.Instance.GetLostClients(user.Company, false, false, StartDate, numberOfDays);
                    LostClientsList = customers.Where(x => lostClientIds.Contains(x.ID)).ToList();                   
                }
                var customerExport = new List<CustomerExport>();
                foreach (var item in LostClientsList)
                {
                    customerExport.Add(new CustomerExport
                    {
                        FirstName = item.FirstName,
                        LastName = item.LastName,
                        AdditionalInformation = item.AdditionalInformation,
                        AdditionalInvoiceInformation = item.AdditionalInvoiceInformation,
                        Address = item.Address,
                        City = item.City,
                        DateAdded = item.DateAdded.ToString("yyyy-MM-dd"),
                        DateOfBirth = item.DateOfBirth?.ToString("yyyy-MM-dd"),
                        Email = item.Email,
                        Gender = item.Gender,
                        HaveNewsLetter = item.HaveNewsLetter,
                        IsBlocked = item.IsBlocked,
                        MobileNumber = item.MobileNumber,
                        PostalCode = item.PostalCode,
                        ReferralBalance = item.ReferralBalance,
                        ReferralCode = item.ReferralCode,
                        WarningInformation = item.WarningInformation
                    });
                }
                return Json(new { success = true, LostCustomers = customerExport }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(new { success =false }, JsonRequestBehavior.AllowGet);

            }
        }

        public class CustomerExport
        {
            public string DateAdded { get; set; }
            public string FirstName { get; set; }
            public string LastName { get; set; }
            public string Gender { get; set; }
            public string DateOfBirth { get; set; }
            public string Email { get; set; }
            public string MobileNumber { get; set; }
            public string Address { get; set; }
            public int PostalCode { get; set; }
            public string City { get; set; }
            public bool HaveNewsLetter { get; set; } = true;
            public string AdditionalInformation { get; set; }
            public string AdditionalInvoiceInformation { get; set; }
            public string WarningInformation { get; set; }
            public bool IsBlocked { get; set; }
            public string ReferralCode { get; set; }
            public float ReferralBalance { get; set; }
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


        public async Task<string> CreateWatch(Employee employee,string Company)
        {
            var googleCalendarnew = GoogleCalendarServices.Instance.GetGoogleCalendarServicesWRTBusiness(Company);
            using (var newclient = new HttpClient())
            {
                newclient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", googleCalendarnew.AccessToken);

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

                var newcontent = new StringContent(JsonConvert.SerializeObject(watchRequestBody), Encoding.UTF8, "application/json");
                var newrequestUrl = $"https://www.googleapis.com/calendar/v3/calendars/{employee.GoogleCalendarID}/events/watch";
                var newresponse =  await newclient.PostAsync(newrequestUrl, newcontent);
                var responseBody = await newresponse.Content.ReadAsStringAsync();

                if (newresponse.IsSuccessStatusCode)
                {
                    var history = new History
                    {
                        Note = "Setup Successfully watched",
                        Business = googleCalendarnew.Business,
                        Date = DateTime.Now
                    };
                    Channel channel = JsonConvert.DeserializeObject<Channel>(responseBody);

                    employee.WatchChannelID = channel.Id;
                    EmployeeServices.Instance.UpdateEmployee(employee);
                    HistoryServices.Instance.SaveHistory(history);

                    var employeeWatch = EmployeeWatchServices.Instance.GetEmployeeWatchByEmployeeID(employee.ID);
                    if (employeeWatch != null)
                    {
                        //employeeWatch.ExpirationDate = DateTime.Parse(channel.Expiration);
                        long timestampSeconds = channel.Expiration / 1000;

                        // Convert to DateTime
                        DateTime dateTime = DateTimeOffset.FromUnixTimeSeconds(timestampSeconds).DateTime;
                        employeeWatch.ExpirationDate = dateTime;
                        employeeWatch.EmployeeID = employee.ID;
                        employeeWatch.Business = employee.Business;
                        EmployeeWatchServices.Instance.UpdateEmployeeWatch(employeeWatch);
                    }
                    else
                    {
                        employeeWatch = new EmployeeWatch();
                        //employeeWatch.ExpirationDate = DateTime.Parse(channel.Expiration);
                        long timestampSeconds = channel.Expiration / 1000;

                        // Convert to DateTime
                        DateTime dateTime = DateTimeOffset.FromUnixTimeSeconds(timestampSeconds).DateTime;
                        employeeWatch.ExpirationDate = dateTime;
                        employeeWatch.EmployeeID = employee.ID;
                        employeeWatch.Business = employee.Business;
                        EmployeeWatchServices.Instance.SaveEmployeeWatch(employeeWatch);
                    }
                    return "Success";
                }
                else
                {
                    var history = new History
                    {
                        Note = "Failed to set up watch " + responseBody,
                        Business = googleCalendarnew.Business,
                        Date = DateTime.Now
                    };
                    HistoryServices.Instance.SaveHistory(history);
                    return "Failed";
                }

            }

        }

        public ActionResult Dashboard(string StartDate = "", string EndDate = "", string FilterDuration = "")
        {
            AdminViewModel model = new AdminViewModel();
            var user = UserManager.FindById(User.Identity.GetUserId()); 
            model.SignedInUser = user;
            if (model.SignedInUser != null)
            {



                var employees2 = EmployeeServices.Instance.GetEmployeeWRTBusiness(user.Company, true);
                foreach (var item in employees2)
                {
                    var employeeWatch = EmployeeWatchServices.Instance.GetEmployeeWatchByEmployeeID(item.ID);
                    if (employeeWatch != null)
                    {
                        if (employeeWatch.ExpirationDate <= DateTime.Now)
                        {
                            CreateWatch(item,user.Company);
                        }
                    }
                    else
                    {
                        CreateWatch(item,user.Company);
                    }
                }

                if (StartDate != "" || EndDate != "")
                {
                    model.StartDate = DateTime.Parse(StartDate);
                    model.EndDate = DateTime.Parse(EndDate);
                    var numberOfDays = 30;
                    if (FilterDuration != "")
                    {
                        switch (FilterDuration)
                        {
                            case "30 days":
                                numberOfDays = 30;
                                break;
                            case "60 days":
                                numberOfDays = 60;
                                break;
                            case "3 months":
                                numberOfDays = 90; // Approximate for 3 months
                                break;
                            case "6 months":
                                numberOfDays = 180; // Approximate for 6 months
                                break;
                            case "1 year":
                                numberOfDays = 365;
                                break;
                            case "2 years":
                                numberOfDays = 730; // 2 * 365
                                break;
                            default:
                                // Handle unexpected value or keep numberOfDays as 0
                                break;
                        }
                    }
                    int LostClients = 0;
                    int ReturnedClients = 0;
                    int TotalNewClients = 0;
                    //var currentDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
                    //var CurrentDatePrev = currentDate.AddDays(-numberOfDays);
                    if (user != null)
                    {
                        var customers = CustomerServices.Instance.GetCustomerWRTBusiness(user.Company);
                        var lostClients = new List<int>();

                        var lostClientIds = AppointmentServices.Instance.GetLostClients(user.Company, false, false, model.StartDate, numberOfDays);
                        //LostClientsList = customers.Where(x=> lostClientIds.Contains(x.ID)).ToList();


                        LostClients = lostClientIds.Count;
                        foreach (var item in customers)
                        {
                            if (item.DateAdded.Date >= model.StartDate.Date && item.DateAdded.Date <= model.EndDate.Date)
                            {
                                TotalNewClients++;
                            }
                            if (AppointmentServices.Instance.GetAppointmentBookingWRTBusiness(user.Company, false, false, model.StartDate, model.EndDate, numberOfDays, item.ID))
                            {
                                ReturnedClients++;
                            }

                        }

                        /////////////////////
                        /// Occupancy
                        /// 

                        var employeeOccupancyList = new List<EmployeeOccupancy>();

                        var employees = new List<Employee>();
                        employees.AddRange(EmployeeServices.Instance.GetEmployeeWRTBusiness(user.Company, true).OrderBy(x => x.DisplayOrder).ToList());
                        var company = CompanyServices.Instance.GetCompany(user.Company).FirstOrDefault();

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



                        foreach (var item in employees)
                        {
                            float SumForAverage = 0;
                            float WorkedHours = 0;

                            var empShifts = new List<ShiftModel>();
                            var EnabledDates = new List<DateTime>();
                            var roster = TimeTableRosterServices.Instance.GetTimeTableRosterByEmpID(item.ID);
                            List<float> DailyOccupancy = new List<float>();
                            for (DateTime FirtIT = model.StartDate; FirtIT <= model.EndDate; FirtIT = FirtIT.AddDays(1))
                            {
                                var shifts = ShiftServices.Instance.GetShiftWRTBusiness(item.Business, item.ID).Where(x => x.Day == FirtIT.DayOfWeek.ToString()).ToList();
                                if (shifts.Count() > 0)
                                {
                                    foreach (var shift in shifts)
                                    {
                                        var recurringShifts = RecurringShiftServices.Instance.GetRecurringShiftWRTBusiness(item.Business, shift.ID);
                                        if (recurringShifts != null)
                                        {
                                            if (recurringShifts.RecurEnd == "Custom Date" && recurringShifts.RecurEndDate != "")
                                            {
                                                if (IsDateInRangeNew(DateTime.Parse(recurringShifts.RecurEndDate), FirtIT))
                                                {
                                                    if (recurringShifts.Frequency == "Bi-Weekly")
                                                    {

                                                        if (GetNextDayStatus(FirtIT, shift.Date, shift.Day.ToString()) == "YES")
                                                        {
                                                            var exceptionShift = ExceptionShiftServices.Instance.GetExceptionShiftWRTBusiness(item.Business, shift.ID);
                                                            empShifts.Add(new ShiftModel { Shift = shift, RecurShift = recurringShifts, ExceptionShift = exceptionShift });
                                                            EnabledDates.Add(FirtIT);
                                                        }


                                                    }
                                                    else
                                                    {
                                                        var exceptionShift = ExceptionShiftServices.Instance.GetExceptionShiftWRTBusiness(item.Business, shift.ID);
                                                        empShifts.Add(new ShiftModel { Shift = shift, RecurShift = recurringShifts, ExceptionShift = exceptionShift });
                                                        EnabledDates.Add(FirtIT);
                                                    }
                                                }
                                            }
                                            else
                                            {
                                                if (recurringShifts.Frequency == "Bi-Weekly")
                                                {
                                                    if (roster != null)
                                                    {
                                                        if (GetNextDayStatus(FirtIT, shift.Date, shift.Day.ToString()) == "YES")
                                                        {
                                                            var exceptionShift = ExceptionShiftServices.Instance.GetExceptionShiftWRTBusiness(item.Business, shift.ID);
                                                            empShifts.Add(new ShiftModel { Shift = shift, RecurShift = recurringShifts, ExceptionShift = exceptionShift });
                                                            EnabledDates.Add(FirtIT);
                                                        }
                                                    }

                                                }
                                                else
                                                {
                                                    if (shift.Date <= FirtIT)
                                                    {
                                                        var exceptionShift = ExceptionShiftServices.Instance.GetExceptionShiftWRTBusiness(item.Business, shift.ID);
                                                        empShifts.Add(new ShiftModel { Shift = shift, RecurShift = recurringShifts, ExceptionShift = exceptionShift });
                                                        EnabledDates.Add(FirtIT);
                                                    }
                                                }
                                            }
                                        }
                                        else
                                        {
                                            var exceptionShiftByShiftID = ExceptionShiftServices.Instance.GetExceptionShiftWRTBusinessAndShift(item.Business, shift.ID);
                                            if (exceptionShiftByShiftID != null)
                                            {
                                                if (exceptionShiftByShiftID.LastOrDefault() != null)
                                                {
                                                    if (exceptionShiftByShiftID.LastOrDefault().ExceptionDate.ToString("yyyy-MM-dd") == FirtIT.ToString("yyyy-MM-dd"))
                                                    {
                                                        empShifts.Add(new ShiftModel { Shift = shift, RecurShift = recurringShifts, ExceptionShift = exceptionShiftByShiftID });
                                                        EnabledDates.Add(FirtIT);
                                                    }

                                                }
                                            }
                                            else
                                            {
                                                empShifts.Add(new ShiftModel { Shift = shift, RecurShift = recurringShifts });
                                                EnabledDates.Add(FirtIT);
                                            }

                                        }
                                    }
                                }

                            }

                            foreach (var date in EnabledDates)
                            {
                                var usethisShift = empShifts.Where(x => x.RecurShift != null && x.Shift.IsRecurring && x.Shift.Day == date.DayOfWeek.ToString() && x.Shift.Date.Date <= date.Date).Select(X => X.Shift).FirstOrDefault();
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
                                                foreach (var dd in firstEmpShift.ExceptionShift.ToList())
                                                {
                                                    if (dd.ShiftID == firstEmpShift.Shift.ID && dd.ExceptionDate.ToString("yyyy-MM-dd") == date.ToString("yyyy-MM-dd") && dd.IsNotWorking == false)
                                                    {
                                                        FoundExceptionToo = true;
                                                        FoundedExceptionShift = dd;
                                                        break;
                                                    }
                                                    if (dd.ShiftID == firstEmpShift.Shift.ID && dd.ExceptionDate.ToString("yyyy-MM-dd") == date.ToString("yyyy-MM-dd") && dd.IsNotWorking == true)
                                                    {
                                                        FoundExceptionToo = true;
                                                        FoundedExceptionShift = dd;
                                                        break;
                                                    }
                                                }


                                            }

                                        }
                                        if (FoundExceptionToo)
                                        {

                                            if (FoundedExceptionShift.ID != 0 && !FoundedExceptionShift.IsNotWorking)
                                            {
                                                DateTime startTime = date.Date
                                               .AddHours(DateTime.ParseExact(FoundedExceptionShift.StartTime, "H:mm", CultureInfo.InvariantCulture).Hour)
                                               .AddMinutes(DateTime.ParseExact(FoundedExceptionShift.StartTime, "H:mm", CultureInfo.InvariantCulture).Minute);

                                                DateTime endTime = date.Date
                                                    .AddHours(DateTime.ParseExact(FoundedExceptionShift.EndTime, "H:mm", CultureInfo.InvariantCulture).Hour)
                                                    .AddMinutes(DateTime.ParseExact(FoundedExceptionShift.EndTime, "H:mm", CultureInfo.InvariantCulture).Minute);

                                                SumForAverage += (float)(endTime - startTime).TotalMinutes;
                                            }



                                        }
                                        else
                                        {
                                            DateTime startTime = date.Date
                                                       .AddHours(DateTime.ParseExact(usethisShift.StartTime, "H:mm", CultureInfo.InvariantCulture).Hour)
                                                       .AddMinutes(DateTime.ParseExact(usethisShift.StartTime, "H:mm", CultureInfo.InvariantCulture).Minute);

                                            DateTime endTime = date.Date
                                                .AddHours(DateTime.ParseExact(usethisShift.EndTime, "H:mm", CultureInfo.InvariantCulture).Hour)
                                                .AddMinutes(DateTime.ParseExact(usethisShift.EndTime, "H:mm", CultureInfo.InvariantCulture).Minute);


                                            SumForAverage += (float)(endTime - startTime).TotalMinutes;




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
                                                    foreach (var datee in firstEmpShift.ExceptionShift.ToList())
                                                    {
                                                        if (datee.ExceptionDate.ToString("yyyy-MM-dd") == date.ToString("yyyy-MM-dd") && datee.IsNotWorking == false)
                                                        {
                                                            FoundExceptionToo = true;
                                                            FoundedExceptionShift = datee;
                                                            break;
                                                        }
                                                        if (datee.ExceptionDate.ToString("yyyy-MM-dd") == date.ToString("yyyy-MM-dd") && datee.IsNotWorking == true)
                                                        {
                                                            FoundExceptionToo = true;
                                                            FoundedExceptionShift = datee;
                                                            break;
                                                        }
                                                    }


                                                }

                                            }
                                            if (FoundExceptionToo)
                                            {

                                                if (FoundedExceptionShift.ID != 0 && !FoundedExceptionShift.IsNotWorking)
                                                {
                                                    DateTime startTime = date.Date
                                                   .AddHours(DateTime.ParseExact(FoundedExceptionShift.StartTime, "H:mm", CultureInfo.InvariantCulture).Hour)
                                                   .AddMinutes(DateTime.ParseExact(FoundedExceptionShift.StartTime, "H:mm", CultureInfo.InvariantCulture).Minute);

                                                    DateTime endTime = date.Date
                                                        .AddHours(DateTime.ParseExact(FoundedExceptionShift.EndTime, "H:mm", CultureInfo.InvariantCulture).Hour)
                                                        .AddMinutes(DateTime.ParseExact(FoundedExceptionShift.EndTime, "H:mm", CultureInfo.InvariantCulture).Minute);
                                                    SumForAverage += (float)(endTime - startTime).TotalMinutes;


                                                }



                                            }
                                        }

                                    }
                                }



                                var appointmentsforthatday = AppointmentServices.Instance.GetAppointmentBookingWRTBusiness(company.Business, false, false, item.ID,date).Where(x=>x.Color != "darkgray").ToList();
                                foreach (var app in appointmentsforthatday)
                                {
                                    var DurationWorked = (float)(app.EndTime - app.Time).TotalHours;
                                    WorkedHours += DurationWorked;

                                }
                                var finalSum = SumForAverage / 60;
                                if (finalSum == 0)
                                {
                                    DailyOccupancy.Add(0);
                                }
                                else
                                {
                                    float occupancyPercentage = (WorkedHours / finalSum) * 100;
                                    DailyOccupancy.Add(occupancyPercentage);
                                }
                            }



                            if(DailyOccupancy.Count()> 0)
                            {
                                employeeOccupancyList.Add(new EmployeeOccupancy { Employee = item, Percentage = (float)Math.Round(DailyOccupancy.Average(),1),TotalTime = SumForAverage/60,WorkedHours = WorkedHours });

                            }
                            else
                            {
                                employeeOccupancyList.Add(new EmployeeOccupancy { Employee = item, Percentage = 0,TotalTime = SumForAverage,WorkedHours = WorkedHours });

                            }
                        }




                        model.EmployeeOccupancies = employeeOccupancyList;

                        model.ReturnedClients = ReturnedClients;
                        model.NewClients = TotalNewClients;
                        model.FilterDuration = FilterDuration;
                        model.LostClients = LostClients;

                        if (!User.IsInRole("Super Admin"))
                        {
                            model.Company = CompanyServices.Instance.GetCompany(model.SignedInUser.Company).FirstOrDefault();
                            if (model.Company != null)
                            {
                                model.OpeningHours = OpeningHourServices.Instance.GetOpeningHour().Where(x => x.Business == model.Company.Business).ToList();
                                model.Shifts = ShiftServices.Instance.GetShiftWRTBusiness(user.Company);

                            }
                        }


                        if (user.Role == "Owner")
                        {
                            var package = PackageServices.Instance.GetPackage(user.Package);
                            if (package == null && user.IsInTrialPeriod == false)
                            {
                                return RedirectToAction("Pay", "User", new { UserID = user.Id });
                            }
                        }
                    }
                    else
                    {
                        return RedirectToAction("Login", "Account");
                    }
                    return View(model);
                }
                else
                {
                    DateTime firstDay = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
                    DateTime lastDayOfCurrentMonth = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.DaysInMonth(DateTime.Now.Year, DateTime.Now.Month));
                    model.StartDate = firstDay;
                    model.EndDate = lastDayOfCurrentMonth;
                    return View(model);
                }
            }
            else
            {
                return RedirectToAction("Login", "Account");

            }

        }

        [HttpGet]
        public ActionResult Analysis()
        {
            AnalysisViewModel model = new AnalysisViewModel();
            var loggedInUser = UserManager.FindById(User.Identity.GetUserId());
            model.Employees = EmployeeServices.Instance.GetEmployeeWRTBusiness(loggedInUser.Company, "");
            var Company = CompanyServices.Instance.GetCompanyByName(loggedInUser.Company);
            var employeeRequest = EmployeeRequestServices.Instance.GetEmployeeRequestByBusiness(Company.ID).ToList();
            foreach (var item in employeeRequest)
            {
                if (item.Accepted)
                {
                    if (Company.ID == item.CompanyIDFor || Company.ID == item.CompanyIDFrom)
                    {
                        var employee = EmployeeServices.Instance.GetEmployee(item.EmployeeID);
                        if (!model.Employees.Select(x=>x.ID).Contains(employee.ID))
                        {
                            model.Employees.Add(employee);
                        }
                    }
                  

                }
            }



            var listOfStrings = new List<string>
            {
                "No Show",
                "Paid",
                "Pending"
            };
            model.Statuses = listOfStrings;
            model.StartDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            model.EndDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.DaysInMonth(DateTime.Now.Year, DateTime.Now.Month));
            Dictionary<string, int> serviceBookingCounts = new Dictionary<string, int>();
            Dictionary<string, int> serviceCategoryBookingCounts = new Dictionary<string, int>();
            model.serviceBookingCounts = serviceBookingCounts;
            model.serviceCategoryBookingCount = serviceCategoryBookingCounts;
            var dayWiseSales = new List<DayWiseSale>();
            var dayWiseClientVisitation = new List<ClientVisitation>();
            model.DayWiseSales = dayWiseSales;
            model.NumberOfClientsOnEachDays = dayWiseClientVisitation;
            model.NoOfRebookReminderSent = 0;
            model.NoOfRebookReminderClicked = 0;
            model.NoOfRebookReminderOpened = 0;
            model.AppointmentsByRebook = 0;


            return View(model);
        }


        public class EmployeePriceChangeModel
        {
            public string Type { get; set; }
            public float Amount { get; set; }
        }
        public class OnlinePriceChangeModel
        {
            public string Type { get; set; }
            public float Amount { get; set; }
        }

        [HttpPost]
        public ActionResult Analysis(DateTime StartDate, DateTime EndDate, List<string> SelectedEmployeeIDs, List<string> SelectedStatuses, bool IsCancelled)
        {
            AnalysisViewModel model = new AnalysisViewModel();
            var loggedInUser = UserManager.FindById(User.Identity.GetUserId());

            model.Employees = EmployeeServices.Instance.GetEmployeeWRTBusiness(loggedInUser.Company,true);
            var listOfStrings = new List<string>
            {
                "",
                "No Show",
                "Paid",
                "Pending"
            };
            model.Statuses = listOfStrings;
            model.SelectedEmployeeIDs = String.Join(",", SelectedEmployeeIDs);
            model.Status = String.Join(",", SelectedStatuses);
            model.IsCancelled = IsCancelled;
            model.StartDate = StartDate;
            model.EndDate = EndDate;
            var Company = CompanyServices.Instance.GetCompanyByName(loggedInUser.Company);

            var selectedEmployees = SelectedEmployeeIDs.Select(x => int.Parse(x)).ToList();
            var AbsenseServiceIds = ServiceServices.Instance.GetAbsenseServiceIDs(loggedInUser.Company);

            var AllAppointmentsWithabsenceIDsFilters = AppointmentServices.Instance.GetAllAppointmentWRTBusiness(StartDate, EndDate, loggedInUser.Company, IsCancelled, selectedEmployees);
            var employeeRequest = EmployeeRequestServices.Instance.GetEmployeeRequestByBusiness(Company.ID).ToList();
            foreach (var item in employeeRequest)
            {
                if (item.Accepted)
                {
                    var companyFrom = CompanyServices.Instance.GetCompany(item.CompanyIDFrom);
                    if (Company.ID != item.CompanyIDFrom)
                    {
                        var appointment = AppointmentServices.Instance.GetAppointmentBookingWRTBusiness(companyFrom.Business, false, IsCancelled, item.EmployeeID, StartDate, EndDate);
                        AllAppointmentsWithabsenceIDsFilters.AddRange(appointment);
                    }
                   

                }
            }

            if (SelectedStatuses != null && SelectedStatuses.Count() > 0)
            {
                AllAppointmentsWithabsenceIDsFilters = AllAppointmentsWithabsenceIDsFilters.Where(x => SelectedStatuses.Contains(x.Status?.Trim())).ToList();
            }
            model.SumOfOnlineDeposit = AllAppointmentsWithabsenceIDsFilters.Where(x => x.DepositMethod == "Online").Sum(x => x.Deposit);
            model.SumOfCashDeposit = AllAppointmentsWithabsenceIDsFilters.Where(x => x.DepositMethod == "Cash").Sum(x => x.Deposit);
            model.SumOfPinDeposit = AllAppointmentsWithabsenceIDsFilters.Where(x => x.DepositMethod == "Pin").Sum(x => x.Deposit);

            var servicesAll = AllAppointmentsWithabsenceIDsFilters;











            float ServiceCost = 0;
            float TotalOnlinePriceChange = 0;
            float TotalEmployeePriceChange = 0;

            float TotalFinalCost = 0;
            var dayWiseSales = new List<DayWiseSale>();
            var dayWiseClientVisitation = new List<ClientVisitation>();
            var result = from appointment in servicesAll
                         group appointment by appointment.Date.ToString("yyyy-MM-dd") into grouped
                         select new
                         {
                             Date = grouped.Key,
                             DistinctCustomerCount = grouped.Select(appointment => appointment.CustomerID).Distinct().Count()
                         };

            foreach (var item in result)
            {
                var clientVisitation = new ClientVisitation
                {
                    Date = item.Date,
                    NoOfClients = item.DistinctCustomerCount
                };

                dayWiseClientVisitation.Add(clientVisitation);
            }
            model.NumberOfClientsOnEachDays = dayWiseClientVisitation;
            var ListOfOnlinePriceChange = new List<OnlinePriceChangeModel>();
            var ListOfEmployeePriceChange = new List<EmployeePriceChangeModel>();
            var AppointmentIDs = servicesAll.Select(x => x.ID).ToList();
            var sales = SaleServices.Instance.GetSaleWRTBusiness(loggedInUser.Company, AppointmentIDs).Select(x=>x.ID).ToList();
            var saleProducts = SaleProductServices.Instance.GetSaleProductWRTBusiness(loggedInUser.Company, sales);
            float saleProductsAmount = 0;
            if(saleProducts != null && saleProducts.Count() > 0)
            {
                saleProductsAmount = saleProducts.Sum(x => x.Total);
            }
            try
            {
                foreach (var service in servicesAll)
                {
                    if (service.FromGCAL)
                    {
                        continue;
                    }
                    var servicesplit = service.Service.Split(',').Select(x => int.Parse(x)).ToList();
                    var PriceChange = PriceChangeServices.Instance.GetPriceChange(service.OnlinePriceChange);
                    var EmployeePriceChange = EmployeePriceChangeServices.Instance.GetEmployeePriceChange(service.EmployeePriceChange);

                    foreach (var item in servicesplit)
                    {
                        var ServicePrice = ServiceServices.Instance.GetService(item).Price;
                        float dailySale = 0;
                        TotalOnlinePriceChange = 0;
                        TotalEmployeePriceChange = 0;
                        if (PriceChange != null)
                        {
                            if (PriceChange.TypeOfChange == "Price Increase")
                            {
                                var PriceChangeServiced = ServicePrice * (PriceChange.Percentage / 100);
                                TotalOnlinePriceChange += PriceChangeServiced;
                                ListOfOnlinePriceChange.Add(new OnlinePriceChangeModel { Amount = TotalOnlinePriceChange, Type = PriceChange.TypeOfChange });
                                TotalFinalCost += ServicePrice + PriceChangeServiced;
                                dailySale = ServicePrice + PriceChangeServiced;

                            }
                            else
                            {
                                var PriceChangeServiced = ServicePrice * (PriceChange.Percentage / 100);
                                TotalOnlinePriceChange += PriceChangeServiced;
                                ListOfOnlinePriceChange.Add(new OnlinePriceChangeModel { Amount = TotalOnlinePriceChange, Type = PriceChange.TypeOfChange });
                                TotalFinalCost += ServicePrice - PriceChangeServiced;
                                dailySale = ServicePrice - PriceChangeServiced;
                            }
                        }
                        else
                        {
                            dailySale = ServicePrice;

                        }

                        if (EmployeePriceChange != null)
                        {
                            if (EmployeePriceChange.TypeOfChange == "Price Increase")
                            {
                                var PriceChangeServiced = ServicePrice * (EmployeePriceChange.Percentage / 100);
                                TotalEmployeePriceChange += PriceChangeServiced;
                                ListOfEmployeePriceChange.Add(new EmployeePriceChangeModel { Amount = TotalEmployeePriceChange, Type = EmployeePriceChange.TypeOfChange });

                                dailySale = ServicePrice + PriceChangeServiced;


                            }
                            else
                            {
                                var PriceChangeServiced = ServicePrice * (EmployeePriceChange.Percentage / 100);
                                TotalEmployeePriceChange += PriceChangeServiced;
                                ListOfEmployeePriceChange.Add(new EmployeePriceChangeModel { Amount = TotalEmployeePriceChange, Type = EmployeePriceChange.TypeOfChange });
                                dailySale = ServicePrice + PriceChangeServiced;


                            }
                        }
                        else
                        {
                            dailySale = ServicePrice;

                        }

                        ServiceCost += ServicePrice;
                        var existingDaySale = dayWiseSales.FirstOrDefault(x => x.Date.Day == service.Date.Day && x.Date.Month == service.Date.Month && x.Date.Year == service.Date.Year);
                        if (existingDaySale != null)
                        {
                            existingDaySale.DaySale += dailySale;
                        }
                        else
                        {
                            dayWiseSales.Add(new DayWiseSale { Date = service.Date.Date, DaySale = dailySale });
                        }

                    }
                }

            }
            catch (Exception ex)
            {

                throw;
            }
            model.NoShowAppointmentsCount = servicesAll.Where(X => X.Status == "No Show" && X.FromGCAL == false).Count();

            model.PendingAppointmentsCount = servicesAll.Where(X => X.Status == "Pending" && X.FromGCAL == false).Count();
            model.CancelledAppointmentCount = servicesAll.Where(X => X.IsCancelled && X.FromGCAL == false).Count();
            model.CheckoutAppointmentsCount = servicesAll.Where(X => X.Status == "Paid" && X.FromGCAL == false).Count();
            model.TotalNoOfAppointments = servicesAll.Count();
            for (DateTime date = StartDate; date <= EndDate; date = date.AddDays(1))
            {
                var existingDaySale = dayWiseSales.FirstOrDefault(x => x.Date.Day == date.Day && x.Date.Month == date.Month && x.Date.Year == date.Year);

                if (existingDaySale == null)
                {
                    // Date not found in the list, add a new DayWiseSale with day sale of 0
                    dayWiseSales.Add(new DayWiseSale { Date = date.Date, DaySale = 0 });
                }
            }

            model.DayWiseSales = dayWiseSales.OrderBy(x => x.Date.Day).ToList();
            model.TotalServicePrices = ServiceCost;
            float OfflineDiscountCost = 0;
            Dictionary<string, int> serviceBookingCounts = new Dictionary<string, int>();
            Dictionary<string, int> serviceCategoryBookingCounts = new Dictionary<string, int>();
            foreach (var item in servicesAll)
            {
                if (item.FromGCAL)
                {
                    continue;
                }
                var serviceIds = item.Service.Split(',').Select(x => int.Parse(x)).ToList();

                var servicesplit = item.ServiceDiscount.Split(',').Select(x => float.Parse(x)).ToList();
                for (int i = 0; i < serviceIds.Count; i++)
                {
                    var service = ServiceServices.Instance.GetService(serviceIds[i]);
                    var serviceName = service.Name;
                    var serviceCategory = service.Category;
                    if (serviceBookingCounts.ContainsKey(serviceName))
                    {
                        serviceBookingCounts[serviceName]++;
                    }
                    else
                    {
                        serviceBookingCounts[serviceName] = 1;
                    }

                    if (serviceCategoryBookingCounts.ContainsKey(serviceCategory))
                    {
                        serviceCategoryBookingCounts[serviceCategory]++;
                    }
                    else
                    {
                        serviceCategoryBookingCounts[serviceCategory] = 1;
                    }
                    int serviceId = serviceIds[i];
                    float discount = servicesplit[i];
                    var finalDiscount = discount / 100;
                    // Your calculations based on serviceId and discount here
                    var offlineDiscountValue = service.Price * finalDiscount;
                    OfflineDiscountCost += offlineDiscountValue;
                }

            }
            model.TotalOfflineDiscount = OfflineDiscountCost;
            model.TotalPriceAfterOfflineDiscount = ServiceCost - OfflineDiscountCost;
            model.saleProductsAmount = saleProductsAmount;
            var OnlinePriceChange = 0.0;
            foreach (var item in ListOfOnlinePriceChange)
            {
                if (item.Type == "Discount")
                {
                    OnlinePriceChange -= item.Amount;
                }
                else
                {
                    OnlinePriceChange += item.Amount;

                }
            }

            model.TotalOnlinePriceChange = float.Parse(OnlinePriceChange.ToString());


            var TotalAfterPriceChange = ServiceCost - OfflineDiscountCost;
            foreach (var item in ListOfOnlinePriceChange)
            {
                if (item.Type == "Discount")
                {
                    TotalAfterPriceChange -= item.Amount;
                }
                else
                {
                    TotalAfterPriceChange += item.Amount;
                }
            }
            foreach (var item in ListOfEmployeePriceChange)
            {
                if (item.Type == "Discount")
                {
                    TotalAfterPriceChange -= item.Amount;
                }
                else
                {
                    TotalAfterPriceChange += item.Amount;
                }
            }

            model.TotalAfterPriceChange = TotalAfterPriceChange;
            model.serviceBookingCounts = serviceBookingCounts;
            model.serviceCategoryBookingCount = serviceCategoryBookingCounts;
            var EmployeePriceChanges = 0.0;
            foreach (var item in ListOfEmployeePriceChange)
            {
                if (item.Type == "Discount")
                {
                    EmployeePriceChanges -= item.Amount;
                }
                else
                {
                    EmployeePriceChanges += item.Amount;

                }
            }
            model.TotalEmployeePriceChange = float.Parse(EmployeePriceChanges.ToString());

            var filteredAppointments = servicesAll.Where(x => x.FromGCAL == false).Select(x => x.CustomerID).Distinct().ToList();
            int TotalNewClients = 0;
            int ReturnedClients = 0;
            var LostClientsList = new List<Customer>();
            int LostClients = 0;
            DateTime thirtyDaysAgo = StartDate.AddDays(-30);

        

           
            float CheckOutCashCount = 0;
            float CheckOutCardCount = 0;
            float CheckOutPinCount = 0;
            float CheckOutGiftCardCount = 0;
            float CheckOutLoyaltyCardCount = 0;
            var Invoices = InvoiceServices.Instance.GetInvoices(loggedInUser.Company, AppointmentIDs);
            foreach (var item in Invoices)
            {

                if (item.PaymentMethod.Trim() == "Cash")
                {
                    CheckOutCashCount += item.GrandTotal;
                }
                else if (item.PaymentMethod.Trim() == "Card")
                {
                    CheckOutCardCount += item.GrandTotal;
                }
                else if (item.PaymentMethod.Trim() == "Gift Card")
                {
                    CheckOutGiftCardCount += item.GrandTotal;
                }
           
                else if (item.PaymentMethod.Trim() == "Loyalty Card")
                {
                    CheckOutLoyaltyCardCount += item.GrandTotal;
                }
            }
            

            int NoOfRebookReminderSent = 0;
            int NoOfRebookReminderClicked = 0;
            int NoOfRebookReminderOpened = 0;
            int AppointmentsByRebook = 0;
            var RebookReminders = RebookReminderServices.Instance.GetRebookReminderWRTBusiness(loggedInUser.Company, StartDate, EndDate);
            foreach (var item in RebookReminders)
            {
                NoOfRebookReminderSent++;
                if (item.Opened)
                {
                    NoOfRebookReminderOpened++;
                }
                if (item.Clicked)
                {
                    NoOfRebookReminderClicked++;
                }
            }
            AppointmentsByRebook = servicesAll.Where(X => X.BookedFromRebook && X.FromGCAL == false).Count();



            model.NoOfRebookReminderClicked = NoOfRebookReminderClicked;
            model.NoOfRebookReminderOpened = NoOfRebookReminderOpened;
            model.NoOfRebookReminderSent = NoOfRebookReminderSent;
            model.AppointmentsByRebook = AppointmentsByRebook;
            model.CheckedOutCardCount = CheckOutCardCount;
            model.CheckedOutCashCount = CheckOutCashCount;
            model.CheckedOutGiftCardCount = CheckOutGiftCardCount;
            model.CheckedOutLoyaltyCardCount = CheckOutGiftCardCount;
            model.CheckedOutPinCount = CheckOutPinCount;



            return View(model);
        }
        public ActionResult GetOpeningHoursCount()
        {
            var user = UserManager.FindById(User.Identity.GetUserId());
            if (user != null)
            {
                var companyList = new List<Company>();
                int openingHoursCount = OpeningHourServices.Instance.GetOpeningHour().Where(x => x.Business == user.Company).ToList().Count();
                var loggedInUserCompany = CompanyServices.Instance.GetCompany().Where(X => X.Business == user.Company).FirstOrDefault();
                if (loggedInUserCompany != null)
                {
                    var franchises = FranchiseRequestServices.Instance.GetFranchiseRequestByBusiness(loggedInUserCompany.ID);
                    foreach (var item in franchises)
                    {

                        if (item.Accepted && item.UserID == user.Id || item.MappedToUserID == user.Id)
                        {
                            var company = CompanyServices.Instance.GetCompany(item.CompanyIDFrom);
                            if (!companyList.Select(X => X.ID).Contains(company.ID))
                            {
                                companyList.Add(company);
                            }
                            company = CompanyServices.Instance.GetCompany(item.CompanyIDFor);
                            if (!companyList.Select(X => X.ID).Contains(company.ID))
                            {
                                companyList.Add(company);
                            }
                        }
                    }
                }
                // Return the opening hours count as a JSON response
                return Json(new { count = openingHoursCount, Company = user.Company, Companies = companyList }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return RedirectToAction("Login", "Account");
            }
        }



        [HttpPost]
        public ActionResult SwitchOrganisation(int Business)
        {
            var company = CompanyServices.Instance.GetCompany(Business);
            var final = UserManager.FindById(User.Identity.GetUserId());
            var companyLoggedIn = CompanyServices.Instance.GetCompany().Where(x => x.Business == final.Company).FirstOrDefault();
            var franchise = FranchiseRequestServices.Instance.GetFranchiseRequestByUserID(final.Id, Business, companyLoggedIn.ID);

            if (franchise != null)
            {
                if (franchise.Accepted)
                {
                    HttpContext.GetOwinContext().Authentication.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
                    if (franchise.UserID == final.Id)
                    {
                        var gotoLogin = UserManager.FindById(franchise.MappedToUserID);
                        return Json(new { success = true, Email = gotoLogin.Email, KIKO = gotoLogin.Password }, JsonRequestBehavior.AllowGet);

                    }
                    else
                    {

                        var gotoLogin = UserManager.FindById(franchise.UserID);
                        var model = new LoginViewModel();
                        model.RememberMe = false;
                        model.Email = gotoLogin.Email;
                        model.Password = gotoLogin.Password;
                        return Json(new { success = true, Email = gotoLogin.Email, KIKO = gotoLogin.Password }, JsonRequestBehavior.AllowGet);
                    }

                }
                else
                {
                    return Json(new { success = false, Message = "Request not Accepted" }, JsonRequestBehavior.AllowGet);

                }
            }
            else
            {
                return Json(new { success = false, Message = "No Request found" }, JsonRequestBehavior.AllowGet);

            }
        }


        public class NotificationModel
        {
            public int ID { get; set; }
            public string Link { get; set; }
            public string Title { get; set; }
            public string Date { get; set; }

        }

        [HttpGet]
        public JsonResult GetNotifications()
        {
            var LoggedInUser= UserManager.FindById(User.Identity.GetUserId());
            var notifications = NotificationServices.Instance.GetNotificationWRTBusiness(LoggedInUser.Company);
            var notificationList = new List<NotificationModel>();
            foreach (var item in notifications)
            {
                if (LoggedInUser.ReadNotifications != null && LoggedInUser.ReadNotifications.Split(',').ToList().Contains(item.ID.ToString()))
                {

                    notificationList.Add(new NotificationModel {ID=item.ID, Date = item.Date.ToString("yyyy-MM-dd"),Link = item.Link,Title = item.Title});
                }
            }
            return Json(new { success = true,Notifications =notifications }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult UpdateUserRead(int ID)
        {
            var LoggedInUser = UserManager.FindById(User.Identity.GetUserId());
            var notification = NotificationServices.Instance.GetNotification(ID);
            if (notification != null)
            {
                if (LoggedInUser.ReadNotifications == null)
                {
                    LoggedInUser.ReadNotifications = String.Join(",", notification.ID);
                }
                else
                {
                    LoggedInUser.ReadNotifications = String.Join(",", LoggedInUser.ReadNotifications, notification.ID);
                }
            }
            UserManager.Update(LoggedInUser);
            return Json(new { success = true }, JsonRequestBehavior.AllowGet);
        }





    }
}