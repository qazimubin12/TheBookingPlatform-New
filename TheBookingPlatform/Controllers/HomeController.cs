using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using TheBookingPlatform.Services;
using TheBookingPlatform.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TheBookingPlatform.Entities;
using Microsoft.AspNetCore.Http;
using TheBookingPlatform.Database.Migrations;

namespace TheBookingPlatform.Controllers
{
    public class HomeController : Controller
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


        public ActionResult Index()
        {
            AdminViewModel model = new AdminViewModel();

            return View(model);
        }



        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }

        [HttpGet]
        public ActionResult ShowRebookReminders()
        {
            RebookReminderAppointmentViewModel model = new RebookReminderAppointmentViewModel();
            var LoggedInUser = UserManager.FindById(User.Identity.GetUserId());
            model.RebookReminderAppointments = RebookReminderServices.Instance.GetRebookRemindersWRTBusiness(LoggedInUser.Company);
            return PartialView("_ShowRebookReminders", model);
        }

        public ActionResult ShowServices()
        {
            ServiceListingViewModel model = new ServiceListingViewModel();
            var ServicesList = new List<ServiceModel>();
            var LoggedInUser = UserManager.FindById(User.Identity.GetUserId()); if (LoggedInUser == null) { return RedirectToAction("Login", "Account"); }
            var company = CompanyServices.Instance.GetCompany().Where(x => x.Business == LoggedInUser.Company).FirstOrDefault();
            if (LoggedInUser.Role != "Super Admin")
            {
                var categories = ServicesCategoriesServices.Instance.GetServiceCategories("").Where(x => x.Business == LoggedInUser.Company).OrderBy(x => x.DisplayOrder).ToList();
                foreach (var item in categories)
                {
                    var ServicesWRTCategory = ServiceServices.Instance.GetService().Where(x => x.Category == item.Name && x.Business == LoggedInUser.Company).OrderBy(x => x.DisplayOrder).ToList();
                    ServicesList.Add(new ServiceModel { ServiceCategory = item, Services = ServicesWRTCategory, Company = company });
                }
            }
            else
            {
                var categories = ServicesCategoriesServices.Instance.GetServiceCategories("").OrderBy(x => x.DisplayOrder).ToList();
                foreach (var item in categories)
                {
                    var ServicesWRTCategory = ServiceServices.Instance.GetService().Where(x => x.Category == item.Name).OrderBy(x => x.DisplayOrder).ToList();
                    ServicesList.Add(new ServiceModel { ServiceCategory = item, Services = ServicesWRTCategory, Company = company });
                }
            }
            var DeletedServices = new ServiceModelInServices();

            model.Services = ServicesList.OrderBy(X => X.ServiceCategory.DisplayOrder).ToList();
            var DeletedServicesList = ServiceServices.Instance.GetService().Where(x => x.IsActive == false && x.Business == LoggedInUser.Company).OrderBy(x => x.DisplayOrder).ToList();
            if (DeletedServicesList.Count() > 0)
            {
                DeletedServices.Services = DeletedServicesList;
                DeletedServices.Company = company;
                DeletedServices.ServiceCategory = "DELETED";
                model.DeletedServices = DeletedServices;
            }
            return PartialView("_ServiceListing", model);
        }


        public ActionResult ShowOpeningHours()
        {
            var LoggedInUser = UserManager.FindById(User.Identity.GetUserId()); if (LoggedInUser == null) { return RedirectToAction("Login", "Account"); }

            OpeningHourListingViewModel model = new OpeningHourListingViewModel();
            model.OpeningHours = OpeningHourServices.Instance.GetOpeningHoursWRTBusiness(LoggedInUser.Company, "");
            return PartialView("_OpeningHoursListing", model);
        }


        [HttpGet]
        public ActionResult AssignService(int ID)
        {
            SettingsViewModel model = new SettingsViewModel();

            var ServicesList = new List<ServiceModel>();
            var LoggedInUser = UserManager.FindById(User.Identity.GetUserId()); if (LoggedInUser == null) { return RedirectToAction("Login", "Account"); }
            var company = CompanyServices.Instance.GetCompany().Where(x => x.Business == LoggedInUser.Company).FirstOrDefault();
            if (LoggedInUser.Role != "Super Admin")
            {
                var categories = ServicesCategoriesServices.Instance.GetServiceCategoriesWRTBusiness(LoggedInUser.Company);
                foreach (var item in categories)
                {
                    var ServicesWRTCategory = ServiceServices.Instance.GetServiceWRTCategory(LoggedInUser.Company, item.Name);
                    ServicesList.Add(new ServiceModel { ServiceCategory = item, Services = ServicesWRTCategory, Company = company });
                }
            }
            else
            {
                var categories = ServicesCategoriesServices.Instance.GetServiceCategories().OrderBy(x => x.DisplayOrder).ToList();
                foreach (var item in categories)
                {
                    var ServicesWRTCategory = ServiceServices.Instance.GetServiceWRTCategory(item.Name);
                    ServicesList.Add(new ServiceModel { ServiceCategory = item, Services = ServicesWRTCategory, Company = company });
                }

            }
            var employee = EmployeeServices.Instance.GetEmployee(ID);
            model.EmployeeFull = employee;
            model.EmployeeID = employee.ID;
            var ServiceEmployeeData = "";

            var ServiceEmployees = EmployeeServiceServices.Instance.GetEmployeeService(employee.Business, employee.ID);
            foreach (var item in ServiceEmployees)
            {
                if (ServiceEmployeeData == "")
                {
                    ServiceEmployeeData = String.Join("_", item.ServiceID);
                }
                else
                {
                    ServiceEmployeeData = String.Join("_", ServiceEmployeeData, item.ServiceID);
                }
            }
            model.ServiceData = ServiceEmployeeData;
            model.Services = ServicesList;
            return PartialView("_AssignService", model);
        }

        [HttpPost]
        public ActionResult AssignService(SettingsViewModel model)
        {
            var ServiceEmployees = EmployeeServiceServices.Instance.GetEmployeeServiceWRTEmployeeID(model.EmployeeID);
            foreach (var item in ServiceEmployees)
            {
                EmployeeServiceServices.Instance.DeleteEmployeeService(item.ID);
            }
            if (model.ServiceData != null)
            {
                var ServiceList = model.ServiceData.Split('_').ToList();

                foreach (var item in ServiceList)
                {

                    var LoggedInUser = UserManager.FindById(User.Identity.GetUserId()); if (LoggedInUser == null) { return RedirectToAction("Login", "Account"); }
                    var service = ServiceServices.Instance.GetService(int.Parse(item));
                    var MainEmployee_Service = new EmployeeService();
                    MainEmployee_Service.Business = service.Business;
                    MainEmployee_Service.ServiceID = service.ID;
                    MainEmployee_Service.EmployeeID = model.EmployeeID;
                    EmployeeServiceServices.Instance.SaveEmployeeService(MainEmployee_Service);
                }
            }

            return Json(new { success = true }, JsonRequestBehavior.AllowGet);
        }
        public ActionResult ShowEmployees()
        {
            EmployeeListingViewModel model = new EmployeeListingViewModel();
            var LoggedInUser = UserManager.FindById(User.Identity.GetUserId()); if (LoggedInUser == null) { return RedirectToAction("Login", "Account"); }
            if (LoggedInUser.Role == "Super Admin")
            {
                var Employees = EmployeeServices.Instance.GetEmployee().OrderBy(x => x.DisplayOrder).ToList(); 
              
                var ListOfEmployeeModel = new List<EmployeeModel>();
                foreach (var item in Employees)
                {
                    var ServiceForEmployee = EmployeeServiceServices.Instance.GetEmployeeServiceWRTEmployeeID(item.ID).Select(x => x.ServiceID).ToList();
                    var ServiceList = new List<Service>();
                    foreach (var service in ServiceForEmployee)
                    {
                        var Service = ServiceServices.Instance.GetService(service);
                        ServiceList.Add(Service);
                    }

                    ListOfEmployeeModel.Add(new EmployeeModel { Employee = item, Services = ServiceList });
                }
                model.Employees = ListOfEmployeeModel;

            }
            else
            {
                var Employees = EmployeeServices.Instance.GetEmployeeWRTBusiness(LoggedInUser.Company, true).OrderBy(x => x.DisplayOrder).ToList();
                var ListOfEmployeeModel = new List<EmployeeModel>(); 
                var company = CompanyServices.Instance.GetCompany().Where(x => x.Business == LoggedInUser.Company).FirstOrDefault();
                var employeeRequest = EmployeeRequestServices.Instance.GetEmployeeRequestByBusiness(company.ID);
                foreach (var item in employeeRequest)
                {
                    if (item.Accepted)
                    {
                        var employee = EmployeeServices.Instance.GetEmployee(item.EmployeeID);
                        if (!Employees.Select(x => x.ID).ToList().Contains(employee.ID))
                        {
                            Employees.Add(employee);
                        }
                    }
                }
                foreach (var item in Employees)
                {
                    var ServiceForEmployee = EmployeeServiceServices.Instance.GetEmployeeServiceWRTEmployeeID(item.ID).Select(x => x.ServiceID).ToList();
                    var ServiceList = new List<Service>();
                    foreach (var service in ServiceForEmployee)
                    {
                        var Service = ServiceServices.Instance.GetService(service);
                        ServiceList.Add(Service);
                    }

                    ListOfEmployeeModel.Add(new EmployeeModel { Employee = item, Services = ServiceList });
                }
                model.Employees = ListOfEmployeeModel;

            }
            var listofCalendarAccess = new List<string>
            {
                "Do not limit",
                "Do not show previous days",
                "1 day before",
                "3 days before",
                "7 days before",
                "1 month before",
                "3 months before",
                "6 months before"
            };
            model.CalendarHistoryAccessList = listofCalendarAccess;
            model.Types = new List<string>
            {
                "Percentage",
                "Worked Hours",
                "Time to Time",
                "Do not Display Payroll"
            };
            return PartialView("_EmployeeListing", model);
        }

        public class HistoryModel
        {
            public History History { get; set; }
            public string Date { get; set; }
        }
        public ActionResult ShowHistories(int skip = 0, int take = 1000)
        {
            var LoggedInUser = UserManager.FindById(User.Identity.GetUserId());
            var hisotrymodel = new List<HistoryModel>();

            if (LoggedInUser.Role == "Super Admin")
            {
                var histories = HistoryServices.Instance
                    .GetHistories()
                    .OrderByDescending(h => h.Date)
                    .Skip(skip)
                    .Take(take)
                    .ToList();

                foreach (var item in histories)
                {
                    hisotrymodel.Add(new HistoryModel { History = item,Date = item.Date.ToString("yyyy-MM-dd") });
                }
            }
            else
            {
                var histories = HistoryServices.Instance
                    .GetHistoriesWRTBusiness(LoggedInUser.Company)
                    .OrderByDescending(h => h.Date)
                    .Skip(skip)
                    .Take(take)
                    .ToList();


                foreach (var item in histories)
                {
                    hisotrymodel.Add(new HistoryModel { History = item, Date = item.Date.ToString("yyyy-MM-dd") });
                }
            }

            return Json(hisotrymodel, JsonRequestBehavior.AllowGet); // for Load More AJAX
        }


        public ActionResult ShowHistory()
        {
            var LoggedInUser = UserManager.FindById(User.Identity.GetUserId());

            HomeViewModel model = new HomeViewModel();
            
            return PartialView("_HistoryListing", model);
        }


        public ActionResult PriceChange()
        {
            HomeViewModel model = new HomeViewModel();
            var user = UserManager.FindById(User.Identity.GetUserId());
            var pricechange = new List<PriceChangeModel>();
            if (user.Role != "Super Admin")
            {

                var priceChanges = PriceChangeServices.Instance.GetPriceChangeWRTBusiness(user.Company);
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
            return PartialView("_PriceChangeListing", model);
        }

        public ActionResult ShowCompanySettings()
        {
            var user = UserManager.FindById(User.Identity.GetUserId());

            if (user.Role != "Super Admin")
            {
                SettingsViewModel model = new SettingsViewModel();
                model.Company = CompanyServices.Instance.GetCompany().Where(x => x.CreatedBy == user.Id).FirstOrDefault();
                model.TimeZones = CompanyServices.Instance.GetTimeZones();
                var statusList = new List<string> { "Paid", "Pending", "No-Show" };
                model.StatusList = statusList;
                if (model.Company == null)
                {
                    model.Company = CompanyServices.Instance.GetCompany().Where(x => x.Business == user.Company).FirstOrDefault();

                }
                else
                {
                    //var baseUrl = $"{request.Scheme}://{request.Host}{request.PathBase}";
                    //model.BookingLink = Url.Action("Welcome", "Booking", new {Param= HomeServices.Instance.Encode(model.Company.Business) });

                }

                if (model.Company.CompanyCode == "" || model.Company.CompanyCode == null)
                {
                    model.Company.CompanyCode = Guid.NewGuid().ToString();
                    CompanyServices.Instance.UpdateCompany(model.Company);
                }
                model.GiftCard = GiftCardServices.Instance.GetGiftCard().Where(x => x.Business == model.Company.Business).FirstOrDefault();
                List<string> countries = new List<string>
{
    "Russia", "Ukraine", "France", "Spain", "Sweden", "Norway", "Germany", "Finland",
    "Poland", "Italy", "United Kingdom", "Romania", "Belarus", "Kazakhstan", "Greece",
    "Bulgaria", "Iceland", "Hungary", "Portugal", "Serbia", "Austria", "Czechia", "Ireland",
    "Lithuania", "Latvia", "Croatia", "Bosnia and Herzegovina", "Slovakia", "Estonia",
    "Denmark", "Switzerland", "Netherlands", "Moldova", "Belgium", "Albania",
    "North Macedonia", "Turkey", "Slovenia", "Montenegro", "Kosovo", "Azerbaijan", "Georgia",
    "Luxembourg", "Andorra", "Malta", "Liechtenstein", "San Marino", "Monaco", "Vatican City",
    "Armenia", "Cyprus", "Afghanistan", "Algeria", "Angola", "Antigua and Barbuda",
    "Argentina", "Australia", "Bahamas", "Bahrain", "Bangladesh",
    "Barbados", "Belize", "Benin", "Bhutan", "Bolivia",
    "Botswana", "Brazil", "Brunei", "Burkina Faso", "Burundi", "Cabo Verde", "Cambodia",
    "Cameroon", "Canada", "Central African Republic", "Chad", "Chile", "China", "Colombia", "Comoros",
    "Congo (Congo-Brazzaville)", "Congo (Congo-Kinshasa)", "Costa Rica", "Cuba",
    "Czech Republic", "Djibouti", "Dominica", "Dominican Republic", "Ecuador", "Egypt", "El Salvador",
    "Equatorial Guinea", "Eritrea", "Eswatini", "Ethiopia", "Fiji", "Gabon",
    "Gambia", "Ghana", "Grenada", "Guatemala", "Guinea", "Guinea-Bissau",
    "Guyana", "Haiti", "Honduras", "India", "Indonesia", "Iran", "Iraq",
    "Israel", "Jamaica", "Japan", "Jordan", "Kenya", "Kiribati", "Korea North",
    "Korea South", "Kuwait", "Kyrgyzstan", "Laos", "Lebanon", "Lesotho", "Liberia", "Libya",
    "Madagascar", "Malawi", "Malaysia", "Maldives", "Mali",
    "Marshall Islands", "Mauritania", "Mauritius", "Mexico", "Micronesia",
    "Mongolia", "Morocco", "Mozambique", "Myanmar", "Namibia", "Nauru", "Nepal",
    "New Zealand", "Nicaragua", "Niger", "Nigeria", "Oman", "Pakistan", "Palau",
    "Panama", "Papua New Guinea", "Paraguay", "Peru", "Philippines", "Qatar",
    "Rwanda", "Saint Kitts and Nevis", "Saint Lucia", "Saint Vincent and the Grenadines", "Samoa",
    "Sao Tome and Principe", "Saudi Arabia", "Senegal", "Seychelles", "Sierra Leone",
    "Singapore", "Solomon Islands", "Somalia", "South Africa", "South Sudan",
    "Sri Lanka", "Sudan", "Suriname", "Syria", "Taiwan", "Tajikistan", "Tanzania",
    "Thailand", "Timor-Leste", "Togo", "Tonga", "Trinidad and Tobago", "Tunisia", "Turkmenistan",
    "Tuvalu", "Uganda", "United Arab Emirates", "United States of America",
    "Uruguay", "Uzbekistan", "Vanuatu", "Venezuela", "Vietnam", "Yemen", "Zambia", "Zimbabwe"
};

                model.Countries = countries;


                var AvailableCurrencies = new List<Currency>
        {
            new Currency { Symbol = "$", Name = "United States Dollar", Abbreviation = "USD" },
            new Currency { Symbol = "€", Name = "Euro", Abbreviation = "EUR" },
            new Currency { Symbol = "£", Name = "British Pound Sterling", Abbreviation = "GBP" },
            new Currency { Symbol = "¥", Name = "Japanese Yen", Abbreviation = "JPY" },
            new Currency { Symbol = "AU$", Name = "Australian Dollar", Abbreviation = "AUD" },
            new Currency { Symbol = "CA$", Name = "Canadian Dollar", Abbreviation = "CAD" },
            new Currency { Symbol = "CHF", Name = "Swiss Franc", Abbreviation = "CHF" },
            new Currency { Symbol = "¥", Name = "Chinese Yuan", Abbreviation = "CNY" },
            new Currency { Symbol = "₹", Name = "Indian Rupee", Abbreviation = "INR" },
            new Currency { Symbol = "R$", Name = "Brazilian Real", Abbreviation = "BRL" },
            new Currency { Symbol = "Mex$", Name = "Mexican Peso", Abbreviation = "MXN" },
            new Currency { Symbol = "R", Name = "South African Rand", Abbreviation = "ZAR" },
            new Currency { Symbol = "S$", Name = "Singapore Dollar", Abbreviation = "SGD" },
            new Currency { Symbol = "kr", Name = "Swedish Krona", Abbreviation = "SEK" },
            new Currency { Symbol = "NZ$", Name = "New Zealand Dollar", Abbreviation = "NZD" },

        };
                model.Currencies = AvailableCurrencies;
                return PartialView("_CompanySettings", model);
            }
            else
            {
                CompanyListingViewModel model = new CompanyListingViewModel();
                var companies = CompanyServices.Instance.GetCompany();
                var CompaniesList = new List<CompanyModel>();
                model.TimeZones = CompanyServices.Instance.GetTimeZones();

                foreach (var item in companies)
                {
                    List<string> UsersList = new List<string>();
                    var ListOfUsersIds = item.EmployeesLinked.Split(',').ToList();
                    foreach (var users in ListOfUsersIds)
                    {
                        UsersList.Add(users);
                    }
                    CompaniesList.Add(new CompanyModel { Company = item, EmployeesLinked = UsersList });
                }
                model.Companies = CompaniesList;
                return PartialView("_CompanyListing", model);
            }

        }




        public ActionResult Login(string Email, string Password)
        {
            if (!string.IsNullOrEmpty(Email) && !string.IsNullOrEmpty(Password))
            {
                return RedirectToAction("Dashboard", "Admin");
            }
            else
            {
                return View();
            }
        }



        //[HttpPost]
        //public ActionResult Login(string Email, string Password)
        //{
        //    var user = DBNull;/* AMUserManager(username, password);*/
        //    if (user != null)
        //    {
        //        Session["ID"] = user.ID.ToString();
        //        Session["UserName"] = user.UserName.ToString();
        //        Session["Email"] = user.Email.ToString();
        //        Session["Role"] = user.Role.ToString();
        //        if (user.Role == "Admin")
        //        {
        //            return RedirectToAction("AdminDashboard", "Home");
        //        }
        //        else if (user.Role == "Kitchen Staff")
        //        {
        //            return RedirectToAction("KitchenDashboard", "Home");
        //        }
        //        else if (user.Role == "Cashier")
        //        {
        //            return RedirectToAction("BillingDashboard", "Home");
        //        }

        //        else if (user.Role == "Waiter")
        //        {
        //            return RedirectToAction("WaiterApp", "Home");
        //        }
        //        else if (user.Role == "Kitchen Master")
        //        {
        //            return RedirectToAction("KitchenDashboard", "Home");
        //        }
        //    }
        //    else
        //    {

        //    }
        //    {
        //        string msg = "Invalid Password or UserName";
        //        TempData["ErrorMessage"] = msg;
        //    }

        //    Session["ID"] = null;
        //    Session["UserName"] = null;
        //    Session["Email"] = null;
        //    Session["Role"] = null;
        //    return RedirectToAction("Index", "Login");

        //}


    }
}