﻿using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using TheBookingPlatform.Models;
using TheBookingPlatform.Services;
using TheBookingPlatform.ViewModels;
using TheBookingPlatform.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Microsoft.Owin.BuilderProperties;
using Stripe.Checkout;
using Stripe;
using Newtonsoft.Json.Linq;
using System.Net.Http;

namespace TheBookingPlatform.Controllers
{
    public class UserController : Controller
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

        public UserController()
        {
        }

        public UserController(AMUserManager userManager, AMSignInManager signInManager)
        {
            UserManager = userManager;
            SignInManager = signInManager;
        }

        #endregion
        // GET: User
        public ActionResult Index(string searchterm)
        {
            UsersListingViewModel model = new UsersListingViewModel();
            if (User.Identity.IsAuthenticated)
            {
                var LoggedInUser = UserManager.FindById(User.Identity.GetUserId()); if (LoggedInUser == null) { return RedirectToAction("Login", "Account"); }
                if (LoggedInUser.Role == "Super Admin")
                {
                    model.Users = SearchUsers(searchterm);
                    model.Roles = RolesManager.Roles.ToList();
                }
                else
                {
                    model.Users = SearchUsers(searchterm).Where(x => x.Company == LoggedInUser.Company);
                    model.Roles = RolesManager.Roles.ToList().Where(x => x.Name != "Super Admin");
                }
            }
            else
            {
                if (Session["SuperAdminAccess"] == null)
                {
                    return RedirectToAction("PassKey", "User");
                }
                else
                {
                    if (Session["SuperAdminAccess"].ToString() == "12144")
                    {
                        model.Users = SearchUsers(searchterm);
                        model.Roles = RolesManager.Roles.ToList();
                    }
                    else
                    {
                        return RedirectToAction("PassKey", "User");
                    }
                }
            }
            model.LoggedInUser = UserManager.FindById(User.Identity.GetUserId());

            return View(model);
        }

        [HttpGet]
        public ActionResult Settings(string Selected = "")
        {
            SettingsViewModel model = new SettingsViewModel();
            model.Selected = Selected;
            return View(model);
        }


        [HttpGet]
        [AllowAnonymous]
        public ActionResult PassKey()
        {

            return View();
        }


        


        [HttpPost]
        public ActionResult PassKey(string PassKey)
        {
            Session["SuperAdminAccess"] = PassKey;
            return RedirectToAction("Index", "User");
        }

        public int ConvertEuroToCents(int euroAmount)
        {
            // Convert euros to cents
            int centsAmount = euroAmount * 100;
            return centsAmount;
        }


        public ActionResult SavePayment(string UserID, int PackageID)
        {
            var user = UserManager.FindById(UserID);
            user.Package = PackageID;
            user.IsPaid = true;
            user.LastPaymentDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm");
            UserManager.Update(user);

            var payment = new Payment();
            payment.Business = user.Company;
            payment.LastPaidDate = DateTime.Now;
            payment.PackageID = PackageID;
            payment.UserID = UserID;
            PaymentServices.Instance.SavePayment(payment);


            var currentUsers = UserManager.Users.Where(x => x.Company == user.Company && x.Id != user.Id).ToList();
            foreach (var item in currentUsers)
            {
                item.Package = user.Package;
                item.LastPaymentDate = user.LastPaymentDate;
                item.IsPaid = user.IsPaid;
                item.IsInTrialPeriod = user.IsInTrialPeriod;
                UserManager.Update(item);
            }

            return RedirectToAction("Login", "Account");

        }

        [HttpPost]
        public JsonResult PayPackage(int PackageID, string UserID)
        {
            var package = PackageServices.Instance.GetPackage(PackageID);
            var user = UserManager.FindById(UserID);
            var apikey = package.APIKEY;
            StripeConfiguration.SetApiKey(apikey);


            var options = new SessionCreateOptions
            {
                PaymentMethodTypes = new List<string> { "card", "ideal" },
                LineItems = new List<SessionLineItemOptions>(),
                Mode = "payment", // You can use "subscription" for subscriptions.
                // You can set the success and cancel URLs for redirection after payment.
                SuccessUrl = "http://app.yourbookingplatform.com" + Url.Action("SavePayment", "User", new { UserID = UserID, PackageID = PackageID }),
                CancelUrl = "http://app.yourbookingplatform.com" + Url.Action("Login", "Account"),
            };

            decimal amountInDollars = package.Price;
            decimal vatinCents = package.Price * (package.VAT / 100);

            // Convert the amount to cents
            long amountInCents = Convert.ToInt64(amountInDollars * 100);
            long vatincenters = Convert.ToInt64(vatinCents * 100);
            var lineItems = new List<SessionLineItemOptions>
            {
                // Add a separate line item for the total amount.
                new SessionLineItemOptions
                {
                    Quantity = 1,
                    PriceData = new SessionLineItemPriceDataOptions
                    {
                        Currency = "eur",
                        UnitAmount = amountInCents,
                        ProductData = new SessionLineItemPriceDataProductDataOptions
                        {
                            Name = package.Name,
                            Description = package.Description
                        },
                    }
                },

                 new SessionLineItemOptions
                 {
                    Quantity = 1,
                    PriceData = new SessionLineItemPriceDataOptions
                    {
                        Currency = "eur",
                        UnitAmount = vatincenters,
                        ProductData = new SessionLineItemPriceDataProductDataOptions
                        {
                            Name = "VAT",
                            Description = package.VAT.ToString()+"%"
                        },
                    }
                 }
            };

            options.LineItems = lineItems;
            var serviceSession = new SessionService();
            Session session = serviceSession.Create(options);






            return Json(new { success = true, URL = session.Url }, JsonRequestBehavior.AllowGet);
        }


        [HttpPost]
        public JsonResult CheckEmail(string email)
        {
            // Example: Replace with your actual data access code to check if the email exists
            bool emailExists = UserManager.Users.Any(u => u.Email == email);

            return Json(new { exists = emailExists });
        }

        [HttpGet]
        public ActionResult RegisterCompany()
        {
            var Email = Convert.ToString(Session["RegisteredEmail"]);
            var Password = Convert.ToString(Session["RegisteredPAKKITA"]);
            if (Email != "" && Password != "")
            {

                CompanyActionViewModel model = new CompanyActionViewModel();
                model.Email = Email;
                model.PAKKIDA = Password;
                return View("RegisterCompany", "_InitialLayout", model);
            }
            else
            {
                return RedirectToAction("Register", "Account");
            }
        }

        [HttpPost]
        public ActionResult UpdateIsActiveStatus(string ID)
        {
            var User = UserManager.FindById(ID);
            if (User.IsActive == false)
            {
                User.IsActive = true;
                UserManager.Update(User);
                return Json(new { success = true });
            }
            else
            {
                User.IsActive = false;
                UserManager.Update(User);
                return Json(new { success = true });

            }

        }
        static async Task<string> GetCountryByIp()
        {
            string ipInfoUrl = "http://ipinfo.io/json"; // This URL might require an API key for production use.
            using (HttpClient client = new HttpClient())
            {
                var response = await client.GetStringAsync(ipInfoUrl);
                var json = JObject.Parse(response);
                return json["country"].ToString();
            }
        }

        static async Task<string> GetTimeZoneByIP()
        {
            string ipInfoUrl = "http://ipinfo.io/json"; // This URL might require an API key for production use.
            using (HttpClient client = new HttpClient())
            {
                var response = await client.GetStringAsync(ipInfoUrl);
                var json = JObject.Parse(response);
                return json["timezone"].ToString();
            }
        }

        [HttpPost]
        public async Task<ActionResult> RegisterCompany(CompanyActionViewModel model)
        {
            var user = UserManager.Users.Where(x => x.Email == model.Email && x.Password == model.PAKKIDA).FirstOrDefault();
            var company = new Company();
            company.Business = model.Business;
            company.Address = model.Address;
            company.PostalCode = model.PostalCode;
            company.City = model.City;
            company.PhoneNumber = model.PhoneNumber;
            company.Logo = model.Logo;
            company.NotificationEmail = model.NotificationEmail;
            company.ContactEmail = model.ContactEmail;
            company.BillingEmail = model.BillingEmail;
            company.EmployeesLinked = model.EmployeesLinked;
            company.CreatedBy = user.Id;
            company.Currency = "USD";
            company.NewsLetterWeekInterval = 24;
            company.NewsLetterWeekInterval = model.NewsLetterWeekInterval;
            company.CountryName = await GetCountryByIp();
            company.EmployeesLinked = String.Join(",", user.Name);
            company.TimeZone = await GetTimeZoneByIP();
            company.CancellationTime = "24 Hours";

            if (CompanyServices.Instance.GetCompany().Where(x => x.Business.Trim().ToLower() == model.Business.Trim().ToLower()).Any())
            {
                return Json(new { success = false, Message = "Company Name already Registered" }, JsonRequestBehavior.AllowGet);

            }
            else
            {
                CompanyServices.Instance.SaveCompany(company);
                user.Company = company.Business;
                UserManager.Update(user);

                var emailTemplates = EmailTemplateServices.Instance.GetEmailTemplate().Where(x => x.Business == "ProductionServer").ToList();

                foreach (var item in emailTemplates)
                {
                    var emailTemplate = new EmailTemplate();
                    emailTemplate.Name = item.Name;
                    emailTemplate.TemplateCode = item.TemplateCode;
                    emailTemplate.Business = company.Business;
                    emailTemplate.Duration = "Immediately";

                    EmailTemplateServices.Instance.SaveEmailTemplate(emailTemplate);
                }

                var serviceCategory = new ServiceCategory();
                serviceCategory.Name = "ABSENSE";
                serviceCategory.Business = company.Business;
                ServicesCategoriesServices.Instance.SaveServiceCategory(serviceCategory);


                var listOfStrings = new List<string>
            {
                "Monday",
                "Tuesday",
                "Wednesday",
                "Thursday",
                "Friday",
                "Saturday",
                "Sunday"
            };

                foreach (var item in listOfStrings)
                {
                    var openingHour = new OpeningHour();
                    openingHour.Business = company.Business;
                    openingHour.Time = "09:00 - 22:00";
                    openingHour.Day = item;
                    OpeningHourServices.Instance.SaveOpeningHour(openingHour);
                }

                var employee = new Employee();
                employee.Business = company.Business;
                employee.Name = user.Name;
                employee.LinkedEmployee = user.Id;
                EmployeeServices.Instance.SaveEmployee(employee);

                var calendarManages = new CalendarManage();
                calendarManages.Business = company.Business;
                calendarManages.ManageOf = employee.ID.ToString();
                calendarManages.UserID = user.Id;
                CalendarManageServices.Instance.SaveCalendarManage(calendarManages);

                return Json(new { success = true, Email = user.Email, Password = user.Password }, JsonRequestBehavior.AllowGet);

            }



        }

        public IEnumerable<User> SearchUsers(string searchTerm)
        {
            var users = UserManager.Users.AsQueryable();

            if (!string.IsNullOrEmpty(searchTerm))
            {
                users = users.Where(a => a.Email.ToLower().Contains(searchTerm.ToLower()));
            }


            return users;
        }



        [HttpPost]
        public ActionResult UpdateIsClosed(int ID)
        {
            var openinghours = OpeningHourServices.Instance.GetOpeningHour(ID);
            if (openinghours.isClosed == true)
            {
                openinghours.isClosed = false;
            }
            else
            {
                openinghours.isClosed = true;

            }
            OpeningHourServices.Instance.UpdateOpeningHour(openinghours);
            return Json(new { success = true }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Pay(string UserID)
        {
            PayViewModel model = new PayViewModel();
            model.User = UserManager.FindById(UserID);
            model.Packages = PackageServices.Instance.GetPackage();
            HttpContext.GetOwinContext().Authentication.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
            return View(model);
        }
        public ActionResult Register(RegisterViewModel model)
        {
            model.Roles = RolesManager.Roles.ToList();
            var user = UserManager.FindById(User.Identity.GetUserId());
            if (user != null)
            {
                var package = PackageServices.Instance.GetPackage(user.Package);
                if (package != null)
                {
                    model.NoOfUserAllowed = package.NoOfUsers;
                    model.NoOfUsers = UserManager.Users.Where(x => x.Company == user.Company && x.Id != user.Id).Count();
                }
            }
            return PartialView("_Register", model);
        }

        [HttpGet]
        public async Task<ActionResult> Action(string ID)
        {
            UserActionModel model = new UserActionModel();
            model.Roles = RolesManager.Roles.ToList();
            if (User.IsInRole("Super Admin"))
            {
                model.Companies = CompanyServices.Instance.GetCompany();
            }
            var owner = UserManager.FindById(User.Identity.GetUserId());
            if (owner != null)
            {
                if (owner.Role == "Owner" || owner.Role == "Super Admin")
                {
                    model.LoggedInOwnerID = owner.Id;
                }
                else
                {
                    model.LoggedInOwnerID = "";
                }
            }
            if (!string.IsNullOrEmpty(ID))
            {
                var user = await UserManager.FindByIdAsync(ID);
                model.ID = user.Id;
                model.Company = user.Company;
                model.Name = user.Name;
                model.Contact = user.PhoneNumber;
                model.Email = user.Email;
                model.IntervalCalendar = user.IntervalCalendar;
                model.Role = user.Role;
                model.Password = user.Password;
            }
            return PartialView("_Action", model);
        }

        [HttpGet]
        public ActionResult Dashboard()
        {
            return View();
        }

        [HttpPost]
        public async Task<JsonResult> Action(UserActionModel model)
        {
            JsonResult json = new JsonResult();
            IdentityResult result = null;
            if (!string.IsNullOrEmpty(model.ID)) //update record
            {
                var user = await UserManager.FindByIdAsync(model.ID);
                await UserManager.RemoveFromRoleAsync(user.Id, model.Role);
                var role = RolesManager.FindById(model.Role);
                user.Id = model.ID;
                user.Name = model.Name;
                user.PhoneNumber = model.Contact;
                user.Email = model.Email;
                user.UserName = model.Email;
                user.IntervalCalendar = model.IntervalCalendar;
                user.Role = role.Name;
                user.Password = model.Password;
                var token = await UserManager.GeneratePasswordResetTokenAsync(model.ID);
                var result2 = await UserManager.ResetPasswordAsync(model.ID, token, model.Password);

                if (User.IsInRole("Super Admin"))
                {
                    user.Company = model.Company;
                }

                await UserManager.AddToRoleAsync(user.Id, user.Role);
                result = await UserManager.UpdateAsync(user);
                json.Data = new { Success = result.Succeeded, Message = string.Join(", ", result.Errors) };



            }
            else
            {

                var LoggedInUser = UserManager.FindById(User.Identity.GetUserId());
                if (UserManager.Users.ToList().Where(x => x.Email.ToLower() == model.Email.ToLower() && x.Company == LoggedInUser.Company).Count() == 0)
                {
                    var user = new User();
                    user.Name = model.Name;
                    user.PhoneNumber = model.Contact;
                    user.Email = model.Email;
                    user.Company = LoggedInUser.Company;
                    user.Password = model.Password;
                    user.Role = model.Role;
                    user.IntervalCalendar = model.IntervalCalendar;
                    user.UserName = model.Email;
                    result = await UserManager.CreateAsync(user);
                    await UserManager.AddToRoleAsync(user.Id, user.Role);
                    json.Data = new { Success = result.Succeeded, Message = string.Join(", ", result.Errors) };

                }
                else
                {
                    json.Data = new { Success = false, Message = "This email already exists in the system" };

                }

            }


            return json;
        }

        [HttpGet]
        public async Task<ActionResult> Delete(string ID)
        {
            UserActionModel model = new UserActionModel();

            var user = await UserManager.FindByIdAsync(ID);

            model.ID = user.Id;

            return PartialView("_Delete", model);
        }

        [HttpGet]
        public async Task<ActionResult> Activate(string ID)
        {
            UserActionModel model = new UserActionModel();

            var user = await UserManager.FindByIdAsync(ID);

            model.ID = user.Id;

            return PartialView("_Activate", model);
        }

        [HttpPost]
        public async Task<JsonResult> Activate(UserActionModel model)
        {
            JsonResult json = new JsonResult();
            var user = UserManager.FindById(model.ID);
            user.IsActive = true;
            var result = await UserManager.UpdateAsync(user);
            json.Data = new { Success = result.Succeeded, Message = string.Join(", ", result.Errors) };
            return json;
        }

        [HttpPost]
        public async Task<JsonResult> Delete(UserActionModel model)
        {
            JsonResult json = new JsonResult();
            var user = UserManager.FindById(model.ID);
            user.IsActive = false;
            var result = await UserManager.UpdateAsync(user);
            json.Data = new { Success = result.Succeeded, Message = string.Join(", ", result.Errors) };


            //var loggedInUser = UserManager.FindById(User.Identity.GetUserId());
            //if (loggedInUser.Id != model.ID)
            //{
            //    var employee = EmployeeServices.Instance.GetEmployee().Where(x=>x.LinkedEmployee ==model.ID).FirstOrDefault();
            //    if (employee == null)
            //    {

            //        IdentityResult result = null;

            //        if (!string.IsNullOrEmpty(model.ID)) //we are trying to delete a record
            //        {
            //            var user = await UserManager.FindByIdAsync(model.ID);

            //            result = await UserManager.DeleteAsync(user);

            //            json.Data = new { Success = result.Succeeded, Message = string.Join(", ", result.Errors) };
            //        }
            //        else
            //        {
            //            json.Data = new { Success = false, Message = "Invalid user." };
            //        }
            //    }
            //    else
            //    {
            //        json.Data = new { Success = false, Message = "User is linked to an employee. Please Delete that Employee" };

            //    }
            //    var files = FileServices.Instance.GetFile().Where(x => x.Business == loggedInUser.Company && x.UploadedBy == model.ID).ToList();
            //    if(files.Count== 0)
            //    {

            //        IdentityResult result = null;

            //        if (!string.IsNullOrEmpty(model.ID)) //we are trying to delete a record
            //        {
            //            var user = await UserManager.FindByIdAsync(model.ID);

            //            result = await UserManager.DeleteAsync(user);

            //            json.Data = new { Success = result.Succeeded, Message = string.Join(", ", result.Errors) };
            //        }
            //        else
            //        {
            //            json.Data = new { Success = false, Message = "Invalid user." };
            //        }
            //    }
            //    else
            //    {
            //        json.Data = new { Success = false, Message = "User is linked to appointments files, Delete that Files first" };
            //    }
            //}
            //else
            //{
            //    json.Data = new { Success = false, Message = "Invalid Operation." };
            //}

            return json;
        }



        [HttpGet]
        public async Task<ActionResult> UserRoles(string ID)
        {
            UserRoleModel model = new UserRoleModel();
            model.UserID = ID;
            var user = await UserManager.FindByIdAsync(ID);
            var userRoleIDs = user.Roles.Select(x => x.RoleId).ToList();
            model.LoggedInUser = UserManager.FindById(User.Identity.GetUserId());

            model.UserRoles = RolesManager.Roles.Where(x => userRoleIDs.Contains(x.Id)).ToList();
            model.Roles = RolesManager.Roles.Where(x => !userRoleIDs.Contains(x.Id)).ToList();
            return PartialView("_UserRoles", model);
        }



        [HttpPost]
        public async Task<JsonResult> UserRoles(UserActionModel model)
        {
            JsonResult json = new JsonResult();
            IdentityResult result = null;
            if (!string.IsNullOrEmpty(model.ID)) //update record
            {
                var user = await UserManager.FindByIdAsync(model.ID);

                user.Id = model.ID;
                user.Name = model.Name;
                user.PhoneNumber = model.Contact;
                user.Email = model.Email;
                user.IntervalCalendar = model.IntervalCalendar;
                user.Password = model.Password;
                user.Role = model.Role;
                result = await UserManager.UpdateAsync(user);

            }
            else
            {
                var User = new User();
                User.Name = model.Name;
                User.PhoneNumber = model.Contact;
                User.Email = model.Email;
                User.Password = model.Password;
                User.IntervalCalendar = model.IntervalCalendar;
                User.UserName = model.Email;
                User.Role = model.Role;
                result = await UserManager.CreateAsync(User);

            }

            json.Data = new { Success = result.Succeeded, Message = string.Join(", ", result.Errors) };

            return json;
        }




        [HttpPost]
        public async Task<JsonResult> UserRoleOperation(string userID, string roleID, bool isDelete = false)
        {
            JsonResult json = new JsonResult();

            var user = await UserManager.FindByIdAsync(userID);
            var role = await RolesManager.FindByIdAsync(roleID);

            if (user != null && role != null)
            {
                IdentityResult result = null;
                if (!isDelete)
                {
                    result = await UserManager.AddToRoleAsync(userID, role.Name);
                    user.Role = role.Name;

                    UserManager.Update(user);
                }
                else
                {
                    result = await UserManager.RemoveFromRoleAsync(userID, role.Name);
                }
                json.Data = new { Success = result.Succeeded, Message = string.Join(", ", result.Errors) };

            }
            else
            {
                json.Data = new { Success = false, Message = "Invalid Operation" };

            }


            return json;
        }
    }
}