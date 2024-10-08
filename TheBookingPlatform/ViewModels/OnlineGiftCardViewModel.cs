using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using TheBookingPlatform.Entities;

namespace TheBookingPlatform.ViewModels
{
    public class OnlineGiftCardViewModel
    {
        public Company Company { get; set; }
        public GiftCard GiftCard { get; set; }
        public float AssignedAmount { get; set; }
        public string businessName { get; set; }
        public DateTime AssignedDate { get; set; }
        public float Balance { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string MobileNumber { get; set; }
    }
}