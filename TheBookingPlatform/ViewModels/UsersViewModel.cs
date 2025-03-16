using Microsoft.AspNet.Identity.EntityFramework;
using TheBookingPlatform.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TheBookingPlatform.ViewModels
{
    public class UsersListingViewModel
    {
        public IEnumerable<User> Users { get; set; }
        public string RoleID { get; set; }
        public IEnumerable<IdentityRole> Roles { get; set; }
        public string SearchTerm { get; set; }
        public User LoggedInUser { get; set; }
    }

    public class ReviewModel
    {
        public string Type { get; set; }
        public Review Review { get; set; }
        public string CustomerName { get; set; }
        public string EmployeeName { get; set; }
    }
    public class Currency
    {
        public string Name { get; set; }
        public string Symbol { get; set; }
        public string Abbreviation { get; set; }
    }
    public class SettingsViewModel
    {
        public List<Currency> Currencies { get; set; }
        public List<ReviewModel> Reviews { get; set; }
        public Company Company { get; set; }
        public string Selected { get; set; }
        public List<string> Countries { get; set; }
        //
        public int ID { get; set; }
        public string BusinessName { get; set; }
        public string Address { get; set; }
        public string PostalCode { get; set; }
        public string City { get; set; }
        public string PhoneNumber { get; set; }
        public string Logo { get; set; }
        public string NotificationEmail { get; set; }
        public string StatusForPayroll { get; set; }
        public List<string> StatusList { get; set; }
        public string ContactEmail { get; set; }
        public string BillingEmail { get; set; }
        public string EmployeesLinked { get; set; }
        public string CreatedBy { get; set; }
        public bool PaymentMethodIntegration { get; set; }
        public string CountryName { get; set; }
        public string Currency { get; set; }
        public string InvoiceLine { get; set; }
        public int NewsLetterWeekInterval { get; set; }
        public float Deposit { get; set; }




















        public List<EmailTemplate> EmailTemplates { get; set; }
        public string SearchTerm { get; set; }
        public string BookingLink { get;  set; }
        public GiftCard GiftCard { get;  set; }
        public Employee EmployeeFull { get;  set; }
        public int EmployeeID { get;  set; }
        public List<ServiceModel> Services { get;  set; }
        public string ServiceData { get;  set; }
        public List<string> TimeZones { get;  set; }
    }

    public class UserActionModel
    {
        public string passkaka { get; set; }

        public List<Company> Companies { get; set; }
        public string LoggedInOwnerID { get; set; }
        public string Role { get; set; }
        public string ID { get; set; }
        public string Country { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public int IntervalCalendar { get; set; }
        public string UserName { get; set; }
        public string Contact { get; set; }
        public string Password { get; set; }
        public User LoggedInUser { get; set; }
        public List<IdentityRole> Roles { get; set; }
        public string Company { get;  set; }
    }

    public class UserRoleModel
    {
        public string UserID { get; set; }
        public User LoggedInUser { get; set; }
        public IEnumerable<IdentityRole> UserRoles { get; set; }
        public IEnumerable<IdentityRole> Roles { get; set; }

    }
}