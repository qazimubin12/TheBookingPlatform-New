using Stripe;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using TheBookingPlatform.Entities;

namespace TheBookingPlatform.ViewModels
{
    public class StripeInvoicesListingViewModel
    {
        public Stripe.Invoice UpComingInvoice { get; set; }
        public List<Stripe.Invoice> PendingInvoices { get; set; }
        public List<Stripe.Invoice> PastInvoices { get; set; }
        public Company Company { get; set; }
        public User LoggedInUser { get;  set; }
        public Package Package { get;  set; }
        public string SubscriptionID { get;  set; }
        public bool DontLetEm { get;  set; }
    }

    public class StripeInvoiceModel
    {
        public Payment Payment { get; set; }
        public User User { get; set; }
        public Package Package { get; set; }
    }
}