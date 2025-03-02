using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Stripe;
using Stripe.Climate;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using TheBookingPlatform.Entities;
using TheBookingPlatform.Services;
using TheBookingPlatform.ViewModels;

namespace TheBookingPlatform.Controllers
{
    public class PackageController : Controller
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
        public PackageController()
        {
        }



        public PackageController(AMUserManager userManager, AMSignInManager signInManager)
        {
            UserManager = userManager;
            SignInManager = signInManager;
        }


        // GET: Package
        public ActionResult Index(string SearchTerm = "")
        {
            PackageListingViewModel model = new PackageListingViewModel();
            model.SearchTerm = SearchTerm;
            model.Packages = PackageServices.Instance.GetPackage(SearchTerm);
            return View(model);
        }





        [HttpGet]
        [NoCache]
        public ActionResult Action(int ID = 0)
        {
            PackageActionViewModel model = new PackageActionViewModel();
            var featuresList = new List<string>
            {
                "Users",
                "Company",
                "Opening Hours",
                "Services",
                "Customers",
                "Time Table",
                "Reviews",
                "Logs",
                "Rebook Reminders",
                "Referrals",
                "Email Template",
                "Employee Price Changes",
                "Employee Requests",
                "Google Calendar Integration",
                "Integrations",
                "Franchise Requests",
                "Price Changes",
                "Report",
                "PayRoll",
                "Invoice Maker",
                "Analysis",
                "Appointments",
                "Services Categories",
                "Stripe",
                "Resources",
                "Holidays",
                "VAT",
                "Products",
                "Gift Cards",
                "Coupons",
                "Loyalty Cards",
                "Employees",
                "Company Other Settings",
                "Timetable",
                "Company Switch"
            };
            model.FeaturesList = featuresList;
            if (ID != 0)
            {
                var Package = PackageServices.Instance.GetPackage(ID);
                model.ID = Package.ID;
                model.Name = Package.Name;
                model.Description = Package.Description;
                model.APIKEY = Package.APIKEY;
                model.VAT = Package.VAT;
                model.Price = Package.Price;
                model.NoOfUsers = Package.NoOfUsers;
                if (Package.Features != null)
                {
                    model.Features = Package.Features.Split(',').ToList();
                }
            }
            return View(model);
        }


        [HttpPost]
        public async Task<ActionResult> Action(PackageActionViewModel model)
        {
            if (model.ID != 0)
            {
                var priceChanged = false;
                var Package = PackageServices.Instance.GetPackage(model.ID);
                if(Package.Price != model.Price)
                {
                    priceChanged = true;
                }
                Package.ID = model.ID;
                Package.Name = model.Name;
                Package.Price = model.Price;
                Package.APIKEY = model.APIKEY;
                Package.VAT = model.VAT;
                Package.NoOfUsers = model.NoOfUsers;
                Package.Description = model.Description;
                if (model.Features != null)
                {
                    Package.Features = String.Join(",", model.Features);
                }
                var users = UserManager.Users.Where(x => x.Package == Package.ID).ToList();
                foreach (var item in users)
                {
                    var claimsall = UserManager.GetClaims(item.Id);
                    foreach (var claim in claimsall)
                    {
                        var resultnew =  UserManager.RemoveClaim(item.Id, claim);
                    }
                    claimsall =  UserManager.GetClaims(item.Id);
                    if (!claimsall.Any(c => c.Type == "Package" && c.Value != ""))
                    {
                        UserManager.AddClaim(item.Id, new Claim("Package", Package.Features ?? ""));
                    }
                }
                PackageServices.Instance.UpdatePackage(Package);
                var payments = PaymentServices.Instance.GetPayment();
                decimal amountInDollars = Package.Price; // Base price
                decimal vatInDollars = Package.Price * (Package.VAT / 100); // VAT

                long amountInCents = Convert.ToInt64(amountInDollars * 100);
                long vatInCents = Convert.ToInt64(vatInDollars * 100);
                if (priceChanged)
                {

                   
                    foreach (var item in payments)
                    {
                        var priceService = new PriceService();
                        var priceOptions = new PriceCreateOptions
                        {
                            UnitAmount = amountInCents + vatInCents, // Total price including VAT
                            Currency = "eur",
                            Recurring = new PriceRecurringOptions
                            {
                                Interval = "month",
                                IntervalCount = 1
                            },
                            Product = item.ProductID
                        };
                        var price = priceService.Create(priceOptions); // ✅ This returns a new price

                        UpdateSubscriptionPrice(item.SubcriptionID, price.Id);
                    }
                }
            }
            else
            {
                var package = new Package();
                package.Name = model.Name;
                package.Price = model.Price;
                package.Description = model.Description;
                package.NoOfUsers = model.NoOfUsers;
                package.APIKEY = model.APIKEY;
                package.VAT = model.VAT;
                if (model.Features != null)
                {
                    package.Features = String.Join(",", model.Features);
                }
                var users = UserManager.Users.Where(x => x.Package == package.ID).ToList();
                foreach (var item in users)
                {
                    var claimsall = UserManager.GetClaims(item.Id);
                    foreach (var claim in claimsall)
                    {
                        var resultnew = UserManager.RemoveClaim(item.Id, claim);
                    }
                    claimsall = UserManager.GetClaims(item.Id);
                    if (!claimsall.Any(c => c.Type == "Package" && c.Value != ""))
                    {
                        UserManager.AddClaim(item.Id, new Claim("Package", package.Features ?? ""));
                    }
                }
                PackageServices.Instance.SavePackage(package);
            }
            return Json(new { success = true }, JsonRequestBehavior.AllowGet);
        }


        public void UpdateSubscriptionPrice(string subscriptionId, string newPriceId)
        {
            var subscriptionService = new SubscriptionService();

            // Retrieve the existing subscription
            var subscription = subscriptionService.Get(subscriptionId);

            // Update the subscription with the new price
            var updateOptions = new SubscriptionUpdateOptions
            {
                Items = new List<SubscriptionItemOptions>
        {
            new SubscriptionItemOptions
            {
                Id = subscription.Items.Data[0].Id, // Keep the same subscription item ID
                Price = newPriceId // Set the new price ID
            }
        },
                ProrationBehavior = "create_prorations" // Handles price change prorations
            };

            subscriptionService.Update(subscriptionId, updateOptions);
        }



        [HttpGet]
        [NoCache]
        public ActionResult Delete(int ID)
        {
            PackageActionViewModel model = new PackageActionViewModel();
            var Package = PackageServices.Instance.GetPackage(ID);
            model.ID = Package.ID;
            return PartialView("_Delete", model);
        }


        [HttpPost]
        public ActionResult Delete(PackageActionViewModel model)
        {
            var message = PackageServices.Instance.DeletePackage(model.ID);
            return Json(new { success = true, Message = message }, JsonRequestBehavior.AllowGet);
        }
    }
}