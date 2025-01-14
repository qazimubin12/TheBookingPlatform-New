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
        public Company Company { get; set; }
    }

    public class ReferralFurherListViewModel
    {
        public List<ReferralListModel> Referrals { get; set; }
        public Customer Customer { get; set; }
    }

    

    public class ReferralListModel
    {
        public Referral Referral { get; set; }
        public Customer Customer { get; set; }
        

    }
}