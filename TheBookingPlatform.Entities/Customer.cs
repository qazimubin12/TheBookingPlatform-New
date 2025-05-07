using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheBookingPlatform.Entities
{
    public class Customer:BaseEntity
    {
        public DateTime DateAdded { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Gender { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string Email { get; set; }
        public string MobileNumber { get; set; }
        public string Address { get; set; }
        public int PostalCode { get; set; }
        public string City { get; set; }
        public bool HaveNewsLetter { get; set; } = true;
        public string AdditionalInformation { get; set; }
        public string AdditionalInvoiceInformation { get; set; }
        public string WarningInformation { get; set; }
        public string ProfilePicture { get; set; }  
        public string Password { get; set; }

        public int PriceChangeIDNotification { get; set; }
        public bool IsBlocked { get; set; }
        public string ReferralCode { get; set; }
        public float ReferralBalance { get; set; }
    }
}
