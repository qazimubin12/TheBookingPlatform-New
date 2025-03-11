using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Stripe;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TheBookingPlatform.Entities;
using TheBookingPlatform.Services;
using TheBookingPlatform.ViewModels;

namespace TheBookingPlatform.Controllers
{
    public class StripeInvoicesController : Controller
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

        public StripeInvoicesController()
        {
        }

        public StripeInvoicesController(AMUserManager userManager, AMSignInManager signInManager)
        {
            UserManager = userManager;
            SignInManager = signInManager;
        }

        #endregion
        // GET: StripeInvoices
        public ActionResult Index(bool DontLetEm= false)
        {
            var LoggedInUser= UserManager.FindById(User.Identity.GetUserId());
            StripeInvoicesListingViewModel model = new StripeInvoicesListingViewModel();
            var payments = PaymentServices.Instance.GetPaymentWRTBusiness(LoggedInUser.Company).LastOrDefault();
            var company = CompanyServices.Instance.GetCompanyByName(LoggedInUser.Company);
            model.Company = company;
            var remainderDays = (payments.LastPaidDate.AddMonths(1).Date - DateTime.Now.Date).Days;
            model.LoggedInUser = LoggedInUser;
            if (!company.OwnerCompany) { 
            var package = PackageServices.Instance.GetPackage(company.Package);

                ViewData["ReminderDays"] = remainderDays;
            
            StripeConfiguration.ApiKey = package.APIKEY;
            var invoiceService = new InvoiceService();
            var invoices = invoiceService.List(new InvoiceListOptions
            {
                Subscription = payments.SubcriptionID, // Replace with your Subscription ID
                Limit = 10 // Set appropriate limit
            });
            var pendingInvoices = invoices.Where(i => i.Status == "draft" || i.Status == "open").ToList();
            var pastInvoices = invoices.Where(i => i.Status == "paid" || i.Status == "void").ToList();
            // Step 2: Fetch the user's current subscription (if any)
            var subscriptionService = new SubscriptionService();


            invoiceService = new InvoiceService();
            foreach (var item in pendingInvoices)
            {
                var existingSubscriptions = subscriptionService.List(new SubscriptionListOptions { Customer = item.CustomerId });
                var existingSubscription = existingSubscriptions.Data.FirstOrDefault();
                Stripe.Invoice finalizedinvoice = null;
                if (item.Status != "open")
                {
                    finalizedinvoice = invoiceService.FinalizeInvoice(item.Id);
                }
                if (finalizedinvoice != null)
                {
                    
                    var paidInvoice = invoiceService.Pay(finalizedinvoice.Id);
                    if(paidInvoice.Status == "paid")
                    {
                        var payment = new Payment();
                        payment.PackageID = package.ID;
                        payment.SubcriptionID = company.SubscriptionID;
                        payment.ProductID = existingSubscription.Items.Data[0].Id;
                        payment.LastPaidDate = DateTime.Now;
                        payment.Business = LoggedInUser.Company;
                        payment.Total = item.AmountPaid;
                        payment.UserID = User.Identity.GetUserId();
                        PaymentServices.Instance.SavePayment(payment);  
                    }
                }
                else
                {
                    var paidInvoice = invoiceService.Pay(item.Id);
                    if (paidInvoice.Status == "paid")
                    {
                        var payment = new Payment();
                        payment.PackageID = package.ID;
                        payment.SubcriptionID = company.SubscriptionID;
                        payment.ProductID = existingSubscription.Items.Data[0].Id;
                        payment.LastPaidDate = DateTime.Now;
                        payment.Business = LoggedInUser.Company;
                        payment.Total = item.AmountPaid;
                        payment.UserID = User.Identity.GetUserId();
                        PaymentServices.Instance.SavePayment(payment);
                    }
                }

            }
                if (LoggedInUser.IsActive)
                {
                    var customerID = pastInvoices.LastOrDefault().CustomerId;
                    var existingSubscriptions = subscriptionService.List(new SubscriptionListOptions { Customer = customerID });
                    var existingSubscription = existingSubscriptions.Data.FirstOrDefault();
                    Stripe.Invoice upcomingInvoice = null;
                    if (existingSubscription != null)
                    {
                        upcomingInvoice = invoiceService.Upcoming(new UpcomingInvoiceOptions
                        {
                            Subscription = payments.SubcriptionID // Replace with your Subscription ID
                        });
                    }
                    else
                    {
                        company.SubscriptionStatus = "Cancelled";
                        CompanyServices.Instance.UpdateCompany(company);
                    }
                    ViewData["ReminderDays"] = remainderDays;
                    StripeConfiguration.ApiKey = package.APIKEY;
                    invoices = invoiceService.List(new InvoiceListOptions
                    {
                        Subscription = payments.SubcriptionID, // Replace with your Subscription ID
                        Limit = 10 // Set appropriate limit
                    });
                    pendingInvoices = invoices.Where(i => i.Status == "draft" || i.Status == "open").ToList();
                    pastInvoices = invoices.Where(i => i.Status == "paid" || i.Status == "void").ToList();
                    model.PastInvoices = pastInvoices;
                    model.PendingInvoices = pendingInvoices;
                    model.UpComingInvoice = upcomingInvoice;
                    model.Package = package;
                    model.SubscriptionID = payments.SubcriptionID;
                    model.DontLetEm = DontLetEm;
                    if (DontLetEm && pendingInvoices.Count() > 0)
                    {
                        //return View(model);
                        return View("Index", "_StripeLayout", model);
                    }
                    else
                    {
                        return View(model);
                    }
                }
                else
                {
                    ViewData["ReminderDays"] = remainderDays;

                    return View(model);
                }
            }
            else
            {
                ViewData["ReminderDays"] = remainderDays;
                return View(model);
            }
        }


        [HttpPost]
        public JsonResult CancelSubscription(string subscriptionId, string UserID)
        {
            try
            {
                // Step 1: Fetch the subscription from Stripe
                var subscriptionService = new SubscriptionService();
                var subscription = subscriptionService.Get(subscriptionId);

                if (subscription == null || subscription.Status == "canceled")
                {
                    return Json(new { success = false, message = "Subscription not found or already canceled." });
                }

                // Step 2: Cancel the subscription immediately
                var cancelOptions = new SubscriptionCancelOptions();
                subscriptionService.Cancel(subscriptionId, cancelOptions);

                // Optional: Update user's subscription status in the database
                var user = UserManager.FindById(UserID);
                var company = CompanyServices.Instance.GetCompanyByName(user.Company);
                if (user != null)
                {
                    user.IsActive = false;
                    company.SubscriptionStatus = "Cancelled";
                    CompanyServices.Instance.UpdateCompany(company);
                    UserManager.Update(user);
                }

                // Step 3: Return success response
                return Json(new { success = true, message = "Subscription canceled successfully." });
            }
            catch (StripeException ex)
            {
                return Json(new { success = false, message = "Error: " + ex.Message });
            }
        }

    }
}