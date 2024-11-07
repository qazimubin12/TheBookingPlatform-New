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

        
        public JsonResult GetLostClietCustomers(string FilterDuration)
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

                var currentDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
                var CurrentDatePrev = currentDate.AddDays(-numberOfDays);
                List<Customer> LostClientsList = new List<Customer>();
                if (user != null)
                {
                    var customers = CustomerServices.Instance.GetCustomerWRTBusiness(user.Company);
                    var lostClients = new List<int>();

                    var lostClientIds = AppointmentServices.Instance.GetLostClients(user.Company, false, false, CurrentDatePrev, 30, customers.Select(x => x.ID).ToList());
                    LostClientsList = customers.Where(x => lostClientIds.Contains(x.ID)).ToList();                   
                }
                return Json(new { success = true, LostCustomers = LostClientsList }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(new { success =false }, JsonRequestBehavior.AllowGet);

            }
        }

        public ActionResult Dashboard(string StartDate = "", string EndDate = "", string FilterDuration = "")
        {
            AdminViewModel model = new AdminViewModel();
            var user = UserManager.FindById(User.Identity.GetUserId()); 
            model.SignedInUser = user;
            if (model.SignedInUser != null)
            {
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

                        var lostClientIds = AppointmentServices.Instance.GetLostClients(user.Company, false, false, model.StartDate, numberOfDays, customers.Select(x => x.ID).ToList());
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
            var selectedEmployees = SelectedEmployeeIDs.Select(x => int.Parse(x)).ToList();
            var AbsenseServiceIds = ServiceServices.Instance.GetAbsenseServiceIDs(loggedInUser.Company);

            var AllAppointmentsWithabsenceIDsFilters = AppointmentServices.Instance.GetAllAppointmentWRTBusiness(StartDate, EndDate, loggedInUser.Company, IsCancelled, selectedEmployees);
            if(SelectedStatuses != null && SelectedStatuses.Count() > 0)
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
                        return Json(new { success = true, Email = gotoLogin.Email, Password = gotoLogin.Password }, JsonRequestBehavior.AllowGet);

                    }
                    else
                    {

                        var gotoLogin = UserManager.FindById(franchise.UserID);
                        var model = new LoginViewModel();
                        model.RememberMe = false;
                        model.Email = gotoLogin.Email;
                        model.Password = gotoLogin.Password;
                        return Json(new { success = true, Email = gotoLogin.Email, Password = gotoLogin.Password }, JsonRequestBehavior.AllowGet);
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

       


    }
}