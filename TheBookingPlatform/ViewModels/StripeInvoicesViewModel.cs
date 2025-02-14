using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using TheBookingPlatform.Entities;

namespace TheBookingPlatform.ViewModels
{
    public class StripeInvoicesListingViewModel
    {
        public List<StripeInvoiceModel> Payments { get; set; }
        public Company Company { get; set; }
    }

    public class StripeInvoiceModel
    {
        public Payment Payment { get; set; }
        public User User { get; set; }
        public Package Package { get; set; }
    }
}