using Microsoft.AspNet.Identity;
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
using Newtonsoft.Json;
using System.IO;
using System.Net.Http.Headers;
using System.Net;
using System.Text;
using Microsoft.Owin.Security.Twitter.Messages;
using System.Text.Json.Nodes;
using System.Net.NetworkInformation;

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


        public ActionResult SavePayment(string session_id = "")
        {
            var sessionService = new SessionService();
            var session = sessionService.Get(session_id); // Retrieve session from Stripe
            string subscriptionId = session.SubscriptionId; // Get the Subscription ID


            var eventService = new EventService();

            // Fetch all events (You can also filter by type if needed)
            var eventListOptions = new EventListOptions
            {
                Limit = 50 // Adjust as needed
            };
            var events = eventService.List(eventListOptions);

            string sessionId = session.Id; // Replace with your actual Session ID
            Stripe.Event completedEvent = null;

            // Find the specific event for the session
            foreach (var stripeEvent in events)
            {
                if (stripeEvent.Type == "checkout.session.completed")
                {
                    var sessionObject = stripeEvent.Data.Object as Stripe.Checkout.Session;
                    if (sessionObject != null && sessionObject.Id == sessionId)
                    {
                        completedEvent = stripeEvent;
                        break;
                    }
                }
            }
            string ProductId = "";
            if (!string.IsNullOrEmpty(subscriptionId))
            {
                var subscriptionService = new SubscriptionService();
                var subscription = subscriptionService.Get(subscriptionId);

                // Step 2: Get Price ID from Subscription
                string priceId = subscription.Items.Data[0].Price.Id;

                if (!string.IsNullOrEmpty(priceId))
                {
                    var priceService = new PriceService();
                    var price = priceService.Get(priceId);

                    // Step 3: Get Product ID from Price
                    ProductId = price.ProductId;
                }
                else
                {
                    Console.WriteLine("No Price ID found for this subscription.");
                }
            }
            else
            {
                Console.WriteLine("No Subscription ID found in the session.");
            }

            // If event found, get metadata
            if (completedEvent != null)
            {
                var metadata = ((Stripe.Checkout.Session)completedEvent.Data.Object).Metadata;

                string userId = metadata.ContainsKey("UserID") ? metadata["UserID"] : null;
                string packageId = metadata.ContainsKey("PackageID") ? metadata["PackageID"] : null;
                var package = PackageServices.Instance.GetPackage(int.Parse(packageId));

                var price = float.Parse(package.Price.ToString());
                var vatpercentage = float.Parse(package.VAT.ToString());


                var vatAmount = (vatpercentage / 100) * price;
                var payment = new Payment();
                var user = UserManager.FindById(userId);
                var company = CompanyServices.Instance.GetCompanyByName(user.Company);

                company.Package = package.ID;
                company.SubscriptionID = subscriptionId;
                company.SubscriptionStatus = "Active";
                CompanyServices.Instance.UpdateCompany(company);
                user.IsPaid = true;
                UserManager.Update(user);


                payment.Business = user.Company;
                payment.LastPaidDate = DateTime.Now;
                payment.PackageID = package.ID;
                payment.UserID = user.Id;
                payment.Total = vatAmount;
                payment.ProductID = ProductId;
                
                payment.SubcriptionID = subscriptionId;
                PaymentServices.Instance.SavePayment(payment);


                var currentUsers = UserManager.Users.Where(x => x.Company == user.Company && x.Id != user.Id).ToList();
                foreach (var item in currentUsers)
                {
                    item.IsInTrialPeriod = user.IsInTrialPeriod;
                    item.IsPaid = user.IsPaid;
                    UserManager.Update(item);
                }
            }
            else
            {
                Console.WriteLine("No completed event found for this session.");
            }

           


          

            return RedirectToAction("Login", "Account");

        }

        //[HttpPost]
        //public JsonResult PayPackage(int PackageID, string UserID)
        //{
        //    var package = PackageServices.Instance.GetPackage(PackageID);
        //    var user = UserManager.FindById(UserID);
        //    var apikey = package.APIKEY;
        //    StripeConfiguration.SetApiKey(apikey);


        //    var options = new SessionCreateOptions
        //    {
        //        PaymentMethodTypes = new List<string> { "card", "ideal" },
        //        LineItems = new List<SessionLineItemOptions>(),
        //        Mode = "payment", // You can use "subscription" for subscriptions.
        //        // You can set the success and cancel URLs for redirection after payment.
        //        SuccessUrl = "http://app.yourbookingplatform.com" + Url.Action("SavePayment", "User", new { UserID = UserID, PackageID = PackageID }),
        //        CancelUrl = "http://app.yourbookingplatform.com" + Url.Action("Login", "Account"),
        //    };

        //    decimal amountInDollars = package.Price;
        //    decimal vatinCents = package.Price * (package.VAT / 100);

        //    // Convert the amount to cents
        //    long amountInCents = Convert.ToInt64(amountInDollars * 100);
        //    long vatincenters = Convert.ToInt64(vatinCents * 100);
        //    var lineItems = new List<SessionLineItemOptions>
        //    {
        //        // Add a separate line item for the total amount.
        //        new SessionLineItemOptions
        //        {
        //            Quantity = 1,
        //            PriceData = new SessionLineItemPriceDataOptions
        //            {
        //                Currency = "eur",
        //                UnitAmount = amountInCents,
        //                ProductData = new SessionLineItemPriceDataProductDataOptions
        //                {
        //                    Name = package.Name,
        //                    Description = package.Description
        //                },
        //            }
        //        },

        //         new SessionLineItemOptions
        //         {
        //            Quantity = 1,
        //            PriceData = new SessionLineItemPriceDataOptions
        //            {
        //                Currency = "eur",
        //                UnitAmount = vatincenters,
        //                ProductData = new SessionLineItemPriceDataProductDataOptions
        //                {
        //                    Name = "VAT",
        //                    Description = package.VAT.ToString()+"%"
        //                },
        //            }
        //         }
        //    };

        //    options.LineItems = lineItems;
        //    var serviceSession = new SessionService();
        //    Session session = serviceSession.Create(options);






        //    return Json(new { success = true, URL = session.Url }, JsonRequestBehavior.AllowGet);
        //}

        [HttpPost]
        public ActionResult PayPackage(int PackageID, string UserID)
        {
            var package = PackageServices.Instance.GetPackage(PackageID);
            var user = UserManager.FindById(UserID);
            StripeConfiguration.ApiKey = package.APIKEY;

            // Step 1: Retrieve existing Stripe Customer
            var customerService = new CustomerService();
            var customerList = customerService.List(new CustomerListOptions { Email = user.Email });
            var customer = customerList.Data.FirstOrDefault();
            var company = CompanyServices.Instance.GetCompanyByName(user.Company);
            if (!company.OwnerCompany) { 
            if (customer == null)
            {
                // Create new Stripe Customer if doesn't exist
                var customerOptions = new CustomerCreateOptions
                {
                    Email = user.Email,
                    Name = user.Name
                };
                customer = customerService.Create(customerOptions);
            }

            // Step 2: Fetch the user's current subscription (if any)
            var subscriptionService = new SubscriptionService();
            var existingSubscriptions = subscriptionService.List(new SubscriptionListOptions { Customer = customer.Id });

            var existingSubscription = existingSubscriptions.Data.FirstOrDefault();

            // Step 3: Create Product & Price
            var productService = new ProductService();
            var productOptions = new ProductCreateOptions
            {
                Name = package.Name,
                Description = package.Description
            };
            var product = productService.Create(productOptions);

            var priceService = new PriceService();
            var priceOptions = new PriceCreateOptions
            {
                UnitAmount = Convert.ToInt64(package.Price * 100),
                Currency = "eur",
                Recurring = new PriceRecurringOptions { Interval = "month", IntervalCount = 1 },
                Product = product.Id
            };
            var price = priceService.Create(priceOptions);
            var taxRateService = new TaxRateService();
            var taxRateOptions = new TaxRateCreateOptions
            {
                DisplayName = "VAT",
                Inclusive = false, // Set to true if VAT should be included in the price
                Percentage = package.VAT, // VAT percentage from package
                Country = "NL" // Adjust based on your tax region
            };
            var taxRate = taxRateService.Create(taxRateOptions);

                if (existingSubscription != null)
                {
                    // Step 4: Upgrade Subscription (Replacing old plan)
                    var subscriptionUpdateOptions = new SubscriptionUpdateOptions
                    {
                        Items = new List<SubscriptionItemOptions>
    {
        new SubscriptionItemOptions
        {
            Id = existingSubscription.Items.Data[0].Id, // Existing Subscription Item ID
            Price = price.Id, // New Price ID
            TaxRates = new List<string> { taxRate.Id }  // Attach VAT Tax Rate here
        }
    },
                        ProrationBehavior = "create_prorations" // Ensures fair charge adjustment
                    };

                    // Apply tax rates at the subscription level
                    subscriptionUpdateOptions.DefaultTaxRates = new List<string> { taxRate.Id };

                    var updatedSubscription = subscriptionService.Update(existingSubscription.Id, subscriptionUpdateOptions);
                    company.SubscriptionID = updatedSubscription.Id;
                    company.SubscriptionStatus = "Active";
                    CompanyServices.Instance.UpdateCompany(company);
                }
                return RedirectToAction("Login", "Account");
            }
            else
            {
                // Step 5: Create new subscription if user has none
                StripeConfiguration.ApiKey = package.APIKEY; // Use your Stripe API Key

                // Step 2: Create a Stripe Customer
                var customerService2 = new CustomerService();
                var customerOptions2 = new CustomerCreateOptions
                {
                    Email = user.Email,
                    Name = user.Name
                };
                var customer2 = customerService.Create(customerOptions2);

                // Step 3: Calculate package price and VAT
                decimal amountInDollars2 = package.Price;
                long amountInCents2 = Convert.ToInt64(amountInDollars2 * 100);

                // Step 4: Create a Product in Stripe
                var productService2 = new ProductService();
                var productOptions2 = new ProductCreateOptions
                {
                    Name = package.Name,
                    Description = package.Description
                };
                var product2 = productService2.Create(productOptions2);

                // Step 5: Create a Price for the Product (Excluding VAT)
                var priceService2 = new PriceService();
                var priceOptions2 = new PriceCreateOptions
                {
                    UnitAmount = amountInCents2, // Base price without VAT
                    Currency = "eur",
                    Recurring = new PriceRecurringOptions
                    {
                        Interval = "month", // Subscription interval
                        IntervalCount = 1    // Every 1 month
                    },
                    Product = product2.Id
                };
                var price2 = priceService2.Create(priceOptions2);

                // Step 6: Create a VAT Tax Rate in Stripe
                var taxRateService2 = new TaxRateService();
                var taxRateOptions2 = new TaxRateCreateOptions
                {
                    DisplayName = "VAT",
                    Inclusive = false, // Set to true if VAT should be included in the price
                    Percentage = package.VAT, // VAT percentage from package
                    Country = "NL" // Adjust based on your tax region
                };
                var taxRate2 = taxRateService2.Create(taxRateOptions2);

                // Step 7: Create a Subscription instead of Checkout Session
                var subscriptionService2 = new SubscriptionService();
                var subscriptionOptions2 = new SubscriptionCreateOptions
                {
                    Customer = customer2.Id,
                    Items = new List<SubscriptionItemOptions>
        {
            new SubscriptionItemOptions
            {
                Price = price2.Id,
                Quantity = 1,
                TaxRates = new List<string> { taxRate2.Id } // Attach VAT Tax Rate here
            }
        },
                    Metadata = new Dictionary<string, string>
        {
            { "UserID", UserID },
            { "PackageID", PackageID.ToString() }
        },
                    PaymentBehavior = "default_incomplete", // Allows payment to be completed later
                    Expand = new List<string> { "latest_invoice.payment_intent" }
                };
                var subscription2 = subscriptionService2.Create(subscriptionOptions2);

                // Step 8: Return Subscription ID and Payment Intent Client Secret for frontend
                var clientSecret2 = subscription2.LatestInvoice.PaymentIntent.ClientSecret;
                return Json(new { success = true, SubscriptionID = subscription2.Id, ClientSecret = clientSecret2 }, JsonRequestBehavior.AllowGet);
            }

        }

        //[HttpPost]
        //public JsonResult PayPackage(int PackageID, string UserID)
        //{
        //    // Step 1: Fetch package and user details (assume these services are implemented)
        //    var package = PackageServices.Instance.GetPackage(PackageID);
        //    var user = UserManager.FindById(UserID);
        //    StripeConfiguration.ApiKey = package.APIKEY; // Use your Stripe API Key

        //    // Step 2: Create a Stripe Customer
        //    var customerService = new CustomerService();
        //    var customerOptions = new CustomerCreateOptions
        //    {
        //        Email = user.Email,
        //        Name = user.Name
        //    };
        //    var customer = customerService.Create(customerOptions);

        //    // Step 3: Calculate package price and VAT
        //    decimal amountInDollars = package.Price;
        //    long amountInCents = Convert.ToInt64(amountInDollars * 100);

        //    // Step 4: Create a Product in Stripe
        //    var productService = new ProductService();
        //    var productOptions = new ProductCreateOptions
        //    {
        //        Name = package.Name,
        //        Description = package.Description
        //    };
        //    var product = productService.Create(productOptions);

        //    // Step 5: Create a Price for the Product (Excluding VAT)
        //    var priceService = new PriceService();
        //    var priceOptions = new PriceCreateOptions
        //    {
        //        UnitAmount = amountInCents, // Base price without VAT
        //        Currency = "eur",
        //        Recurring = new PriceRecurringOptions
        //        {
        //            Interval = "month", // Subscription interval
        //            IntervalCount = 1    // Every 1 month
        //        },
        //        Product = product.Id
        //    };
        //    var price = priceService.Create(priceOptions);

        //    // Step 6: Create a VAT Tax Rate in Stripe
        //    var taxRateService = new TaxRateService();
        //    var taxRateOptions = new TaxRateCreateOptions
        //    {
        //        DisplayName = "VAT",
        //        Inclusive = false, // Set to true if VAT should be included in the price
        //        Percentage = package.VAT, // VAT percentage from package
        //        Country = "NL" // Adjust based on your tax region
        //    };
        //    var taxRate = taxRateService.Create(taxRateOptions);

        //    // Step 7: Create a Checkout Session with the VAT Tax Rate
        //    var sessionService = new SessionService();
        //    var sessionOptions = new SessionCreateOptions
        //    {
        //        Customer = customer.Id,
        //        PaymentMethodTypes = new List<string> { "card", "ideal" },
        //        LineItems = new List<SessionLineItemOptions>
        //{
        //    new SessionLineItemOptions
        //    {
        //        Price = price.Id,
        //        Quantity = 1,
        //        TaxRates = new List<string> { taxRate.Id } // Attach VAT Tax Rate here
        //    }
        //},
        //        Mode = "subscription",
        //        SuccessUrl = "http://app.yourbookingplatform.com" + Url.Action("SavePayment", "User") + "?session_id={CHECKOUT_SESSION_ID}",
        //        CancelUrl = "http://app.yourbookingplatform.com" + Url.Action("Login", "Account"),
        //        Metadata = new Dictionary<string, string>
        //{
        //    { "UserID", UserID },
        //    { "PackageID", PackageID.ToString() }
        //}
        //    };
        //    var session = sessionService.Create(sessionOptions);

        //    // Step 8: Return Checkout Session URL
        //    return Json(new { success = true, URL = session.Url }, JsonRequestBehavior.AllowGet);
        //}



        [HttpPost]
        public ActionResult UserRegistrationWebHook()
        {
            try
            {
                // Read the raw body from the request
                Request.InputStream.Seek(0, System.IO.SeekOrigin.Begin);
                var jsonBody = new StreamReader(Request.InputStream).ReadToEnd();

                // Save the raw body to history for debugging
             
                // Verify the Stripe event
          
                var history = new History
                {
                    Business = "STRIPE_WEBHOOK",
                    Note = jsonBody,
                    Date = DateTime.Now
                };
                HistoryServices.Instance.SaveHistory(history);
                JObject parsedJson = JObject.Parse(jsonBody);
                bool isPaid = (bool)parsedJson["data"]["object"]["paid"];
                string type = (string)parsedJson["type"];
                string subscriptionId = (string)parsedJson["data"]["object"]["subscription"];
                string userId = (string)parsedJson["data"]["object"]["lines"]["data"][0]["metadata"]["UserID"];
                int packageId = (int)parsedJson["data"]["object"]["lines"]["data"][0]["metadata"]["PackageID"];
                float total = (float)parsedJson["data"]["object"]["amount_paid"];
                // Handle different types of Stripe events
                if (type == "invoice.payment_succeeded")
                {

                    var user = UserManager.FindById(userId);
                    if (user != null)
                    {
                        user.IsPaid = true;
                        UserManager.Update(user);

                        // Save payment details
                        var payment = new Payment
                        {
                            Business = user.Company,
                            LastPaidDate = DateTime.Now,
                            PackageID = packageId,
                            UserID = userId,
                            Total = total,
                            SubcriptionID= subscriptionId
                        };
                        var company = CompanyServices.Instance.GetCompanyByName(user.Company);
                        company.SubscriptionID = subscriptionId;
                        company.SubscriptionStatus = "Active";
                        CompanyServices.Instance.UpdateCompany(company);
                        PaymentServices.Instance.SavePayment(payment);

                        // Update other users in the same company
                        var currentUsers = UserManager.Users
                            .Where(x => x.Company == user.Company && x.Id != user.Id)
                            .ToList();

                        foreach (var item in currentUsers)
                        {
                            item.IsPaid = user.IsPaid;
                            item.IsInTrialPeriod = user.IsInTrialPeriod;
                            UserManager.Update(item);
                        }
                    }


                }
                else if (type == "invoice.payment_failed")
                {
                    System.Diagnostics.Debug.WriteLine($"Payment failed for UserID: {userId}, PackageID: {packageId}");

                }

                return new HttpStatusCodeResult(200); // Acknowledge receipt of the webhook
            }
            catch (StripeException ex)
            {
                // Handle Stripe exceptions
                System.Diagnostics.Debug.WriteLine($"StripeException: {ex.Message}");
                return new HttpStatusCodeResult(400); // Bad Request
            }
            catch (Exception ex)
            {
                // Handle general exceptions
                System.Diagnostics.Debug.WriteLine($"Exception: {ex.Message}");
                return new HttpStatusCodeResult(400); // Bad Request
            }
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
            var LoggedInUser = UserManager.FindById(User.Identity.GetUserId());
            if (LoggedInUser.Company == null)
            {


                CompanyActionViewModel model = new CompanyActionViewModel();
                model.Email = LoggedInUser.Email;
                model.PAKKIDA = LoggedInUser.Password;
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

        public ActionResult Pay(string UserID, string IsUpgrading = "")
        {
            PayViewModel model = new PayViewModel();
            model.User = UserManager.FindById(UserID);
            var company = CompanyServices.Instance.GetCompanyByName(model.User.Company);
            if (company.Package != 0)
            {
                var currentPackage = PackageServices.Instance.GetPackage(company.Package);
                model.Packages = PackageServices.Instance.GetPackage()
                                   .Where(x => x.Price > currentPackage.Price) // Show only higher packages
                                   .ToList();
                if(model.Packages.Count ==  0)
                {
                    model.Packages = PackageServices.Instance.GetPackage()
                                   .Where(x => x.Price ==  currentPackage.Price) // Show only higher packages
                                   .ToList();
                }


            }
            else
            {
                model.Packages = PackageServices.Instance.GetPackage();
            }
            model.IsUpgrading = IsUpgrading;
            HttpContext.GetOwinContext().Authentication.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
            return View(model);
        }
        public ActionResult Register(RegisterViewModel model)
        {
            model.Roles = RolesManager.Roles.ToList();
            var user = UserManager.FindById(User.Identity.GetUserId());
            if (user != null)
            {
                var company = CompanyServices.Instance.GetCompanyByName(user.Company);
                var package = PackageServices.Instance.GetPackage(company.Package);
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
            model.LoggedInUser = owner;
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
                model.Country = user.Country;
                model.Contact = user.PhoneNumber;
                model.Email = user.Email;
                model.IntervalCalendar = user.IntervalCalendar;
                model.Role = user.Role;
                model.passkaka = user.Password;
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
                try
                {
                    var user = await UserManager.FindByIdAsync(model.ID);
                    var role = RolesManager.FindById(model.Role);
                    if (role == null)
                    {
                        role = RolesManager.FindByName(model.Role);
                    }
                    var roles = RolesManager.Roles.ToList();
                    foreach (var item in roles)
                    {

                        await UserManager.RemoveFromRoleAsync(user.Id, item.Name);
                    }
                    user.Id = model.ID;
                    user.Name = model.Name;
                    user.PhoneNumber = model.Contact;
                    user.Email = model.Email;
                    user.UserName = model.Email;
                    user.IntervalCalendar = model.IntervalCalendar;
                    user.Role = role.Name;
                    user.Country = model.Country;
                    user.Password = model.passkaka;
                    var token = await UserManager.GeneratePasswordResetTokenAsync(model.ID);
                    var result2 = await UserManager.ResetPasswordAsync(model.ID, token, model.passkaka);

                    if (User.IsInRole("Super Admin"))
                    {
                        user.Company = model.Company;
                    }

                    await UserManager.AddToRoleAsync(user.Id, user.Role);
                    result = await UserManager.UpdateAsync(user);
                }
                catch (Exception ex)
                {

                    throw;
                }

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
                    user.Country = model.Country;
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