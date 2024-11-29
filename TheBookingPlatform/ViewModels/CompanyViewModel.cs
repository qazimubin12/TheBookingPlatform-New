using System.Collections.Generic;
using TheBookingPlatform.Entities;

namespace TheBookingPlatform.ViewModels
{
    public class CompanyListingViewModel
    {
        public List<CompanyModel> Companies { get; set; }
        public string SearchTerm { get; set; }
        public List<string> TimeZones { get;  set; }
    }


    public class CompanyModel
    {
        public Company Company { get; set; }
        public List<string> EmployeesLinked { get; set; }
    }
    public class CompanyActionViewModel
    {
        public List<Company> Companies { get; set; }
        public int ID { get; set; }
        public string Business { get; set; }
        public int NewsLetterWeekInterval { get; set; }
        public string Address { get; set; }
        public string PostalCode { get; set; }
        public string City { get; set; }
        public string PhoneNumber { get; set; }
        public string Logo { get; set; }
        public float Deposit { get; set; }
        public string NotificationEmail { get; set; }
        public string ContactEmail { get; set; }
        public string BillingEmail { get; set; }
        public int ParentCompanyID { get; set; }
        public string EmployeesLinked { get; set; }
        public List<User> Users { get; set; }
        public string CreatedBy { get; set; }
        public string CountryName { get; set; }
        public string Currency { get; set; }
        public string InvoiceLine { get; set; }
        public List<string> Countries { get; set; }
        public string BookingLink { get;  set; }
        public string CancellationTime { get;  set; }
        public string APIKEY { get; set; }
        public bool PaymentMethodIntegration { get; set; }
        public string PUBLISHEDKEY { get; set; }
        public string BookingLinkInfo { get;  set; }
        public string StatusForPayroll { get;  set; }
        public List<string> StatusList { get;  set; }
        public List<string> TimeZones { get; set; }
        public string TimeZone { get;  set; }
        public string Email { get;  set; }
        public string PAKKIDA { get;  set; }
        public float ReferralPercentage { get;  set; }
        public string SigningSecret { get;  set; }
    }
}