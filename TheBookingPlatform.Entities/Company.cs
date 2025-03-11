using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheBookingPlatform.Entities
{
    public class Company:BaseEntity
    {
        public string TimeZone { get; set; }

        public string Address { get; set; }
        public string PostalCode { get; set; }
        public string City { get; set; }
        public string PhoneNumber { get; set; }
        public string Logo{ get; set; }
        public string NotificationEmail { get; set; }
        public string ContactEmail { get; set; }
        public string BillingEmail { get; set; }
        public string EmployeesLinked { get; set; }
        public string CreatedBy { get; set; }
        public string CountryName { get; set; }
        public string Currency { get; set; }
        public string InvoiceLine { get; set; }
        public string BookingLink { get; set; }
        public float Deposit { get; set; } = 0;

        public string CancellationTime { get; set; }

        public bool PaymentMethodIntegration { get; set; }
        public string APIKEY { get; set; }
        public string PUBLISHEDKEY { get; set; }
        public int NewsLetterWeekInterval { get; set; } = 0;
        public string BookingLinkInfo { get; set; }
        public float ReferralPercentage { get; set; }
        public string StatusForPayroll { get; set; }
        public string CompanyCode { get; set; }
        public int Package { get; set; }

        public string SubscriptionID { get; set; }
        public string SubscriptionStatus { get; set; }
        public bool OwnerCompany { get; set; }

    }
}
