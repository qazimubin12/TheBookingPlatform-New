using Microsoft.AspNet.Identity.EntityFramework;
using TheBookingPlatform.Entities;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheBookingPlatform.Database
{
    public class DSContext :IdentityDbContext<User>,IDisposable
    {
        public DSContext() : base("TheBookingPlatformConnectionStrings")
        {

        }
       
        public static DSContext Create()
        {
            return new DSContext();
        }
        public DbSet<Product> Products { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Supplier> Suppliers { get; set; }
        public DbSet<StockOrder> StockOrders { get; set; }
        public DbSet<StockHistory> StockHistories { get; set; }
        public DbSet<Company> Companies { get; set; }
        public DbSet<Customer> Customers { get; set; }
        public DbSet<Vat> Vats { get; set; }
        public DbSet<Resource> Resources{ get; set; }
        public DbSet<Service> Services { get; set; }
        public DbSet<ServiceCategory> ServiceCategories { get; set; }
        public DbSet<Employee> Employees { get; set; }
        public DbSet<EmployeeService> EmployeeServices { get; set; }
        public DbSet<Appointment> Appointments { get; set; }
        public DbSet<File> Files { get; set; }
        public DbSet<History> Histories { get; set; }
        public DbSet<Message> Messages { get; set; }
        public DbSet<Invoice> Invoices { get; set; }
        public DbSet<OpeningHour> OpeningHours { get; set; }
        public DbSet<TimeTable> TimeTables { get; set; }
        public DbSet<WaitingList> WaitingLists { get; set; }
        public DbSet<LoyaltyCard> LoyaltyCards { get; set; }
        public DbSet<LoyaltyCardPromotion> LoyaltyCardPromotions { get; set; }
        public DbSet<LoyaltyCardAssignment> LoyaltyCardAssignments { get; set; }
        public DbSet<EmailTemplate> EmailTemplates { get; set; }
        public DbSet<PriceChange> PriceChanges { get; set; }
        public DbSet<GiftCard> GiftCards { get; set; }
        public DbSet<GiftCardAssignment> GiftCardAssignments { get; set; }

        public DbSet<CalendarManage> CalendarManages { get; set; }
        public DbSet<RebookReminder> RebookReminders { get; set; }
        public DbSet<Review> Reviews { get; set; }






        /////////V3//////////////
        public DbSet<EmployeeRequest> EmployeeRequests { get; set; }
        public DbSet<FranchiseRequest> FranchiseRequests { get; set; }
        public DbSet<TimeTableRoster> TimeTableRosters { get; set; }
        public DbSet<Shift> Shifts { get; set; }
        public DbSet<RecurringShift> RecurringShifts { get; set; }
        public DbSet<ExceptionShift> ExceptionShifts { get; set; }
        public DbSet<Holiday> Holidays { get; set; }
        public DbSet<GoogleCalendarIntegration> GoogleCalendarIntegrations { get; set; }
        public DbSet<EmployeePriceChange> EmployeePriceChanges { get; set; }

        public DbSet<Coupon> Coupons { get; set; }
        public DbSet<CouponAssignment> CouponAssignments { get; set; }

        public DbSet<Referral> Referrals { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<Package> Packages { get; set; }
        public DbSet<CouponSwitch> CouponSwitches { get; set; }
        public DbSet<NInvoice> NInvoices { get; set; }
        public DbSet<Reminder> Reminders { get; set; }
        public DbSet<PriceChangeSwitch> PriceChangeSwitches { get; set; }
        public DbSet<EventSwitch> EventSwitches { get; set; }
        public DbSet<EventSwitchDate> EventSwitchDates { get; set; }
        public DbSet<GEventSwitch> GEventSwitches { get; set; }
        public DbSet<SaleProduct> SaleProducts { get; set; }
        public DbSet<Sale> Sales { get; set; }
        public DbSet<SumUpToken> SumUpTokens { get; set; }
        public DbSet<FailedAppointment> FailedAppointments { get; set; }
        public DbSet<EmployeeWatch> EmployeeWatches { get; set; }
        public DbSet<Notification> Notifications { get; set; }
    }
}
