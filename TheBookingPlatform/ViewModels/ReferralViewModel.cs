using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using TheBookingPlatform.Entities;

namespace TheBookingPlatform.ViewModels
{
    public class ReferralListingViewModel
    {
        public List<Customer> ReferralCustomers { get; set; }
    }

    

    public class ReferralListModel
    {
        public Referral Referral { get; set; }
        public Customer ReferredCustomer { get; set; }
        public Customer Customer { get; set; }
        

    }
}