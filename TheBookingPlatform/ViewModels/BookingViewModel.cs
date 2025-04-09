using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using TheBookingPlatform.Entities;
using TheBookingPlatform.ViewModels;

namespace TheBookingPlatform.ViewModels
{
    public class ReferralModel
    {
        public Customer Customer { get; set; }
        public Customer ReferredCustomer { get; set; }
        public Appointment Appointment { get; set; }
        public Referral Referral { get; set; }
        public string Services { get; set; }

    }


    public class BookingViewModel
    {
        public int ID { get; set; }
        public EmployeePriceChange EmployeePriceChangeFull { get; set; }
        public int CustomersCount { get; set; }
        public string SentBy { get; set; }
        public string By { get; set; }
        public bool AnyAvailableEmployeeSelected { get; set; }
        public List<ServiceModelForBooking> Services { get; set; }
        public float DepositText_ { get; set; }

        public List<ServicePriceListModel> ServicesPriceList { get; set; }
        public List<ServiceAppViewModel> ServicesForCancellation { get; set; }
        public List<ReferralModel> Referrals { get; set; }
        public string Param { get; set; }
        public Employee Employee { get; set; }
        public string CompanyName { get; set; }
        public Company Company { get; set; }
        public List<ServiceFormModel> ServicesOnly { get; set; }
        public List<EmployeeNewModel> Employees { get; set; }


        //Customer Form
        public string FirstName { get; set; }
        public string ReferralCode { get; set; }
        public int CustomerID { get; set; }
        public string ErrorNote { get; set; }
        public string LastName { get; set; }
        public string Gender { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string Email { get; set; }
        public string MobileNumber { get; set; }
        public string PhoneNumber { get; set; }
        public string Address { get; set; }
        public int PostalCode { get; set; }
        public string City { get; set; }
        public string Comment { get; set; }
        public string Password { get; set; }
        public int CompanyID { get; set; }
        public string ServiceIDs { get; set; }
        public int OnlinePriceChange { get; set; }
        public int EmployeePriceChange { get; set; }

        public string Time { get; set; }
        public List<OpeningHour> OpeningHours { get; set; }
        public List<ReviewModel> Reviews { get; set; }
        public DateTime Date { get; set; }
        public string ReminderIn { get; set; }
        public int SelectedEmployeeID { get; set; }

        public Customer Customer { get; set; }

        public Appointment Appointment { get; set; }
        public float Deposit { get; set; }



        //Appointments of Customer
        public List<AppointmentModel> Appointments { get; set; }

        //History of Customer
        public List<History> Histories { get; set; }
        public List<GiftCardModelInBooking> GiftCardAssignments { get; set; }


        //Loyalty Card 
        public List<LoyaltyCardAssignmentModel> LoyaltyCardAssignments { get; set; }
        public int AppointmentID { get; set; }
        public int EmployeeID { get; set; }
        public int ReviewID { get; set; }


        public int CouponID { get; set; }
        public int CouponAssignmentID { get; set; }
        public string SuccessURL { get;  set; }
        public string PaymentSecret { get;  set; }
        public string CancelURL { get;  set; }
    }

    public class EmployeeNewModel
    {
        public EmployeePriceChange EmployeePriceChange { get; set; }
        public Employee Employee { get; set; }
        public List<ReviewModel> Reviews { get; set; }

        public float Rating { get; set; }
        public int Count { get; set; }
        public bool HaveEmpPriceChange { get; set; }
    }

    public class ResourceBookedAppointments
    {
        public Appointment Appointment { get; set; }
        public Resource Resource { get; set; }
    }
    public class ServiceFormModel
    {
        public Resource Resource { get; set; }
        public float OnlyDuration { get; set; }
        public string Name { get; set; }
        public int ID { get; set; }
        public string Duration { get; set; }
        public float Price { get; set; }
    }

    public class GiftCardModelInBooking
    {
        public GiftCardAssignment GiftCardAssignment { get; set; }
        public string GiftCardName { get; set; }
    }

    public class SlotsWithEmployeeIDModel
    {
        public int NoOfSlots { get; set; }
        public int EmployeeID { get; set; }
        public List<DateTime> DisabledDates { get; set; }
    }


    public class SlotsListWithEmployeeIDModel
    {
        public List<TimeSlotModel> NoOfSlots { get; set; }
        public int EmployeeID { get; set; }
    }

    public class TimeSlotModel
    {
        public string TimeSlot { get; set; }
        public bool HaveDiscount { get; set; }
        public float Percentage { get; set; }
        public string TypeOfChange { get; set; }
        public int PriceChangeID { get; set; }
        public int EmployeeID { get; set; }
        public bool EmpHaveDiscount { get; set; }
        public float EmpPercentage { get; set; }
        public string EmpTypeOfChange { get; set; }
        public int EmpPriceChangeID { get; set; }
    }

}