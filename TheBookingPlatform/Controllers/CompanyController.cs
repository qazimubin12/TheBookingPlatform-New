using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;

using Stripe;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Configuration;
using System.Web.Mvc;
using TheBookingPlatform.Database.Migrations;
using TheBookingPlatform.Entities;
using TheBookingPlatform.Services;
using TheBookingPlatform.ViewModels;

namespace TheBookingPlatform.Controllers
{
    public class CompanyController : Controller
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

        public CompanyController()
        {
        }

        public CompanyController(AMUserManager userManager, AMSignInManager signInManager)
        {
            UserManager = userManager;
            SignInManager = signInManager;
        }

        #endregion
        // GET: Company
        [Authorize(Roles = "Super Admin")]
        [NoCache]
        public ActionResult Index(string SearchTerm = "")
        {
            //Check and fix for issues:
            var NewUsersList = UserManager.Users.ToList().Where(x => x.Role != "Super Admin").ToList();
            foreach (var item in NewUsersList)
            {
                var CompanyCheck = CompanyServices.Instance.GetCompany(item.Company).FirstOrDefault();
                if (CompanyCheck != null)
                {
                    if (CompanyCheck.EmployeesLinked != null)
                    {
                        var ListOfCopanyLinkedEmployees = CompanyCheck.EmployeesLinked.Split(',');
                        if (!ListOfCopanyLinkedEmployees.Contains(item.Name))
                        {
                            CompanyCheck.EmployeesLinked = String.Join(",", CompanyCheck.EmployeesLinked, item.Name);
                            CompanyServices.Instance.UpdateCompany(CompanyCheck);

                        }
                    }
                    else
                    {
                        CompanyCheck.EmployeesLinked = String.Join(",", item.Name);
                        CompanyServices.Instance.UpdateCompany(CompanyCheck);

                    }
                }

            }



            CompanyListingViewModel model = new CompanyListingViewModel();
            var companies = CompanyServices.Instance.GetCompany(SearchTerm);
            var CompaniesList = new List<CompanyModel>();
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
            return View(model);
        }



        [HttpGet]
        public JsonResult GetEmployees(string CompanyCode)
        {
            var company = CompanyServices.Instance.GetCompany().Where(X => X.CompanyCode == CompanyCode).FirstOrDefault();
            if (company != null)
            {
                var employeeRequestIds = EmployeeRequestServices.Instance.GetEmployeeRequest()
                .Select(x => x.EmployeeID)
                .ToList();

                var employees = EmployeeServices.Instance.GetEmployeeWRTBusiness(company.Business, true);

                var finalEmployees = employees.Where(x => !employeeRequestIds.Contains(x.ID)).ToList();



                return Json(new { success = true, Employees = finalEmployees }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(new { success = false }, JsonRequestBehavior.AllowGet);
            }
        }


        [HttpGet]
        public JsonResult GetUsers(string CompanyCode)
        {
            var company = CompanyServices.Instance.GetCompany().Where(X => X.CompanyCode == CompanyCode).FirstOrDefault();
            if (company != null)
            {
                var franchiseRequestIDs = FranchiseRequestServices.Instance.GetFranchiseRequest()
                .Select(x => x.UserID)
                .ToList();

                var users = UserManager.Users.Where(x => x.Company == company.Business).ToList();

                var Users = users.Where(x => !franchiseRequestIDs.Contains(x.Id)).ToList();



                return Json(new { success = true, Users = Users }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(new { success = false }, JsonRequestBehavior.AllowGet);
            }
        }
        [HttpGet]
        [NoCache]
        public ActionResult Action(int ID = 0)
        {
            CompanyActionViewModel model = new CompanyActionViewModel();
            model.Users = UserManager.Users.ToList();
            List<string> countries = new List<string>
{
    "Afghanistan", "Albania", "Algeria", "Andorra", "Angola", "Antigua and Barbuda", "Argentina",
    "Armenia", "Australia", "Austria", "Azerbaijan", "Bahamas", "Bahrain", "Bangladesh",
    "Barbados", "Belarus", "Belgium", "Belize", "Benin", "Bhutan", "Bolivia",
    "Bosnia and Herzegovina", "Botswana", "Brazil", "Brunei", "Bulgaria", "Burkina Faso",
    "Burundi", "Cambodia", "Cameroon", "Canada", "Cape Verde", "Central African Republic",
    "Chad", "Chile", "China", "Colombia", "Comoros", "Congo (Brazzaville)",
    "Congo (Kinshasa)", "Costa Rica", "Croatia", "Cuba", "Cyprus", "Czech Republic",
    "Denmark", "Djibouti", "Dominica", "Dominican Republic", "East Timor", "Ecuador",
    "Egypt", "El Salvador", "Equatorial Guinea", "Eritrea", "Estonia", "Eswatini",
    "Ethiopia", "Fiji", "Finland", "France", "Gabon", "Gambia", "Georgia", "Germany",
    "Ghana", "Greece", "Grenada", "Guatemala", "Guinea", "Guinea-Bissau", "Guyana",
    "Haiti", "Honduras", "Hungary", "Iceland", "India", "Indonesia", "Iran", "Iraq",
    "Ireland", "Israel", "Italy", "Ivory Coast", "Jamaica", "Japan", "Jordan",
    "Kazakhstan", "Kenya", "Kiribati", "Korea, North", "Korea, South", "Kosovo", "Kuwait",
    "Kyrgyzstan", "Laos", "Latvia", "Lebanon", "Lesotho", "Liberia", "Libya",
    "Liechtenstein", "Lithuania", "Luxembourg", "Madagascar", "Malawi", "Malaysia",
    "Maldives", "Mali", "Malta", "Marshall Islands", "Mauritania", "Mauritius", "Mexico",
    "Micronesia", "Moldova", "Monaco", "Mongolia", "Montenegro", "Morocco", "Mozambique",
    "Myanmar", "Namibia", "Nauru", "Nepal", "Netherlands", "New Zealand", "Nicaragua",
    "Niger", "Nigeria", "North Macedonia", "Norway", "Oman", "Pakistan", "Palau",
    "Panama", "Papua New Guinea", "Paraguay", "Peru", "Philippines", "Poland",
    "Portugal", "Qatar", "Romania", "Russia", "Rwanda", "Saint Kitts and Nevis",
    "Saint Lucia", "Saint Vincent and the Grenadines", "Samoa", "San Marino",
    "Sao Tome and Principe", "Saudi Arabia", "Senegal", "Serbia", "Seychelles",
    "Sierra Leone", "Singapore", "Slovakia", "Slovenia", "Solomon Islands", "Somalia",
    "South Africa", "South Sudan", "Spain", "Sri Lanka", "Sudan", "Suriname",
    "Sweden", "Switzerland", "Syria", "Taiwan", "Tajikistan", "Tanzania", "Thailand",
    "Togo", "Tonga", "Trinidad and Tobago", "Tunisia", "Turkey", "Turkmenistan",
    "Tuvalu", "Uganda", "Ukraine", "United Arab Emirates", "United Kingdom",
    "United States", "Uruguay", "Uzbekistan", "Vanuatu", "Vatican City", "Venezuela",
    "Vietnam", "Yemen", "Zambia", "Zimbabwe"
};

            model.Countries = countries;
            var statusList = new List<string> { "Paid", "Pending", "No-Show" };
            model.StatusList = statusList;
            if (ID != 0)
            {

                var company = CompanyServices.Instance.GetCompany(ID);
                model.ID = company.ID;
                model.Business = company.Business;
                model.Address = company.Address;
                model.PostalCode = company.PostalCode;
                model.City = company.City;
                model.PhoneNumber = company.PhoneNumber;
                model.Logo = company.Logo;
                model.NotificationEmail = company.NotificationEmail;
                model.ContactEmail = company.ContactEmail;
                model.BillingEmail = company.BillingEmail;
                model.EmployeesLinked = company.EmployeesLinked;
                model.InvoiceLine = company.InvoiceLine;
                model.Currency = company.Currency;
                model.TimeZone = company.TimeZone;
                model.Deposit = company.Deposit;
                model.CountryName = company.CountryName;
                model.CancellationTime = company.CancellationTime;
                model.BookingLinkInfo = company.BookingLinkInfo;
                model.BookingLink = HomeServices.Instance.Encode(company.Business);
                model.StatusForPayroll = company.StatusForPayroll;
                model.ReferralPercentage = company.ReferralPercentage;

            }
            return View(model);
        }


        [HttpPost]
        public ActionResult Action(CompanyActionViewModel model)
        {
            if (model.ID != 0)
            {
                var company = CompanyServices.Instance.GetCompany(model.ID);
                company.ID = model.ID;
                var OldName = company.Business;
                company.Business = model.Business;
                company.Address = model.Address;
                company.PostalCode = model.PostalCode;
                company.City = model.City;
                company.PhoneNumber = model.PhoneNumber;
                company.Logo = model.Logo;
                company.NotificationEmail = model.NotificationEmail;
                company.ContactEmail = model.ContactEmail;
                company.CancellationTime = model.CancellationTime;
                company.BillingEmail = model.BillingEmail;
                company.Deposit = model.Deposit;
                company.EmployeesLinked = model.EmployeesLinked;
                company.InvoiceLine = model.InvoiceLine;
                company.Currency = model.Currency;
                company.BookingLink = model.BookingLink;
                company.CountryName = model.CountryName;
                company.TimeZone = model.TimeZone;
                company.NewsLetterWeekInterval = model.NewsLetterWeekInterval;
                company.APIKEY = model.APIKEY;
                company.ReferralPercentage = model.ReferralPercentage;
                company.PUBLISHEDKEY = model.PUBLISHEDKEY;
                company.BookingLinkInfo = model.BookingLinkInfo;
                company.PaymentMethodIntegration = model.PaymentMethodIntegration;
                company.StatusForPayroll = model.StatusForPayroll;
                if (CompanyServices.Instance.GetCompany().Where(x => x.Business.Trim().ToLower() == model.Business.Trim().ToLower()).Any()
                    && OldName != model.Business)
                {
                    return Json(new { success = false, Message = "Company Name already Registered" }, JsonRequestBehavior.AllowGet);

                }
                else
                {
                    CompanyServices.Instance.UpdateCompany(company);
                    Configuration config = WebConfigurationManager.OpenWebConfiguration("~");
                    var appSettings = config.AppSettings.Settings;
                    if (appSettings["StripeSecretKey"] != null)
                    {
                        appSettings["StripeSecretKey"].Value = company.APIKEY;
                        appSettings["StripePublishableKey"].Value = company.PUBLISHEDKEY;
                        config.Save(); // Save the changes
                        ViewBag.Message = "Configuration updated successfully.";
                    }
                    else
                    {
                        ViewBag.Message = "Key not found.";
                    }
                    if (OldName != model.Business)
                    {
                        CompanyServices.Instance.UpdateBusinessFromTheTables(company.Business, OldName);
                        CompanyServices.Instance.UpdateUsersCompany(company.Business, OldName);
                    }
                    return Json(new { success = true }, JsonRequestBehavior.AllowGet);


                }
            }
            else
            {
                var user = UserManager.FindById(User.Identity.GetUserId());
                var company = new Company();
                company.Business = model.Business;
                company.Address = model.Address;
                company.PostalCode = model.PostalCode;
                company.City = model.City;
                company.PhoneNumber = model.PhoneNumber;
                company.Logo = model.Logo;
                company.NotificationEmail = model.NotificationEmail;
                company.ReferralPercentage = model.ReferralPercentage;
                company.ContactEmail = model.ContactEmail;
                company.StatusForPayroll = model.StatusForPayroll;
                company.Deposit = model.Deposit;
                company.BillingEmail = model.BillingEmail;
                company.EmployeesLinked = user.Name;
                company.TimeZone = model.TimeZone;
                company.CancellationTime = model.CancellationTime;
                company.InvoiceLine = model.InvoiceLine;
                company.Currency = model.Currency;
                company.NewsLetterWeekInterval = model.NewsLetterWeekInterval;
                company.CountryName = model.CountryName;
                company.APIKEY = model.APIKEY;
                company.PUBLISHEDKEY = model.PUBLISHEDKEY;
                company.BookingLinkInfo = model.BookingLinkInfo;
                company.PaymentMethodIntegration = model.PaymentMethodIntegration;
                company.CreatedBy = User.Identity.GetUserId();
                if (CompanyServices.Instance.GetCompany().Where(x => x.Business.Trim().ToLower() == model.Business.Trim().ToLower()).Any())
                {
                    return Json(new { success = false, Message = "Company Name already Registered" }, JsonRequestBehavior.AllowGet);

                }
                else
                {
                    CompanyServices.Instance.SaveCompany(company);
                    return Json(new { success = true }, JsonRequestBehavior.AllowGet);
                }

            }
        }





        [HttpGet]
        public ActionResult Delete(int ID)
        {
            CompanyActionViewModel model = new CompanyActionViewModel();
            var company = CompanyServices.Instance.GetCompany(ID);
            model.ID = company.ID;
            return PartialView("_Delete", model);
        }


        [HttpPost]
        public ActionResult Delete(CompanyActionViewModel model)
        {
            var company = CompanyServices.Instance.GetCompany(model.ID);
            var Users = UserManager.Users.ToList().Where(x => x.Company == company.Business).ToList();
           
            CompanyServices.Instance.DeleteCompany(company.ID);

            foreach (var item in Users)
            {
                item.Company = "";
                UserManager.Update(item);

            }
            return Json(new { success = true }, JsonRequestBehavior.AllowGet);
        }
    }
}