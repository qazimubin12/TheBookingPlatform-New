using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using TheBookingPlatform.Entities;

namespace TheBookingPlatform.ViewModels
{
    public class AppointmentListingViewModel
    {
        public bool TodayOff { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime  EndDate { get; set; }
        public DateTime SelectedDate { get; set; }
        public List<Holiday> Holidays { get; set; }
        public List<WaitingListDetailedModel> MainWaitingLists { get; set; }
        public List<WaitingListModel> WaitingLists { get; set; }
        public List<Service> AbsenseServices { get; set; }
        public List<OpeningHour> OpeningHours { get; set; }
        public List<AppointmentListModel> Appointments { get; set; }
        public string GoToDate { get; set; }
        public List<ShiftOfEmployeeModel> Employees { get; set; }
        public List<Employee> EmployeesForAction { get; set; }
        public User LoggedInUser { get; set; }
        public string SearchTerm { get; set; }

        public Employee SelectedEmployee { get; set; }
        public string TodayOpeningHours { get; set; }

        public int EmployeeID { get; set; }
        public DateTime Date { get; set; }
        public DateTime Time { get; set; }
        public DateTime DeletedTime { get; set; }

        public bool AllEmployees { get; set; }

        //For Appointment Model
        public List<ServiceModel> Services { get; set; }
        public Company Company { get; set; }
    }

    public class CancelByEmailViewModel
    {
        public Appointment Appointment { get; set; }
        public List<ServiceAppViewModel> Services { get; set; }
        public Company Company { get; set; }
    }


    public class EmployeeTimeTableModel
    {
        public Employee Employee { get; set; }
        public int WeekBetween { get; set; }
        public List<FullTimeTableModel> TimeTables { get; set; }

        public int DisplayOrder { get; set; }
    }


    public class SaleOnCheckout
    {
        public Sale Sale { get; set; }
        public List<SaleProductModel> SaleProducts { get; set; }
    }
    public class AppointmentDetailsViewModel
    {
        public string Selected { get; set; }
        public Company Company { get; set; }
        public Appointment Appointment { get; set; }
        public PriceChange PriceChange { get; set; }
        public Customer Customer { get; set; }
        public Employee Employee { get; set; }
        public List<ServiceAppViewModel> Services { get; set; }
        public List<AppointmentModel> TotalAppointmentsCustomer { get; set; }//Customer Appointment

        public List<History> Histories { get; set; }
        public List<File> Files { get; set; }
        public List<Message> Messages { get; set; }

        public List<LoyaltyCardAssignmentModel> LoyaltyCardAssignments { get; set; }


        //For Sending Message
        public DateTime Date { get; set; }
        public string Type { get; set; }
        public string Status { get; set; }
        public string Channel { get; set; }
        public string Subject { get; set; }
        public string SentBy { get; set; }


        //For File
        public int FileID { get; set; }
        public string Name { get; set; }
        public string URL { get; set; }
        public string Size { get; set; }
        public string UploadedBy { get; set; }
        public DateTime DateTime { get; set; }
        public int CustomerID { get; set; }
        public float Deposit { get; set; }
        public string DepositMethod { get; set; }

        public List<History> LoyaltyCardHistories { get; set; }
        public EmployeePriceChange EmployeePriceChange { get;  set; }
        public Coupon Coupon { get;  set; }
        public SaleOnCheckout SaleOnCheckOut { get;  set; }
    }



    public class RebookReminderAppointmentViewModel
    {
        public List<RebookReminder> RebookReminderAppointments { get; set; }
    }






    public class CustomerAppointmentModel
    {
        public string FirstName { get; set; }
        public bool IsBlocked { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string MobileNumber { get; set; }
        public int ID { get; set; }

    }



    public class ServiceAppInvoiceViewModel
    {
        public int ID { get; set; }
        public string Service { get; set; }
        public string Duration { get; set; }
        public float Discount { get; set; }
        public float Price { get; set; }
        public float VatAmount { get; set; }
    }
    public class ServiceAppViewModel
    {
        public int ID { get; set; }
        public string Service { get; set; }
        public string Duration { get; set; }
        public string Category { get; set; }
        public float Discount { get; set; }
        public float Price { get; set; }
    }

    public class AppointmentListModel
    {
        public string DeletedTime { get; set; }
        public string Color { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime StartTime { get; set; }
        public int ID { get; set; }
        public DateTime EndTime { get; set; }
        public List<ServiceModelForCustomerProfile> Services { get; set; }

        public string CustomerFirstName { get; set; }
        public string Business { get; set; }
        public string CustomerLastName { get; set; }
    }
    public class AppointmentModel
    {
        public int NoOfAppointments { get; set; }
        public int NoOfNoShows { get; set; }
        public bool NewCustomer { get; set; }
        public string Business { get; set; }
        public bool AnyEmployeeSelected { get; set; }
        public string CustomerFirstName { get; set; }
        public string CustomerLastName { get; set; }
        public string MobileNumber { get; set; }
        public DateTime Date { get; set; }
        public DateTime Time { get; set; }
        public DateTime AppointmentEndTime { get; set; }

        public float TotalCost { get; set; }
        public DateTime BookingDate { get; set; }
        public bool IsCancelled { get; set; }
        public bool IsPaid { get; set; }
        public string Status { get; set; }
        public int ID { get; set; }

        public string Color { get; set; }
        public string EmployeeName { get; set; }
        public string EmployeeSpecialization { get; set; }

        public string Notes { get; set; }
        public DateTime DeletedTime { get; set; }
        public int EmployeeID { get; set; }

        /// <summary>
        /// ////DTO
        /// </summary>


        public bool IsWalkIn { get; set; }
        public string DateString { get; set; }
        public string StartTime { get; set; }
        public string EndTime { get; set; }
        //public Appointment Appointment { get; set; }
        public float TotalDuration { get; set; }
        //public Employee Employee { get; set; }
        //public List<ServiceAppViewModel> Services { get; set; }
        public Customer Customer { get; set; }
        public List<ServiceAppViewModel> ServicesNew { get; set; }
        public List<ServiceModelForCustomerProfile> Services { get; set; }
        public string CustomerEmail { get; set; }
        public bool ReminderSent { get;  set; }
        public bool IsRepeat { get;  set; }
        public bool FromGCAL { get;  set; }
        public List<Entities.Buffer> Buffers { get;  set; }
    }

    public class ServiceModelForCustomerProfile
    {

        public string Name { get; set; }
        public string Duration { get; set; }
        public float Price { get; set; }
        public float Discount { get; set; }
        public string Category { get; set; }
        public int ID { get;  set; }
    }
    public class WaitingListModel
    {
        public DateTime Date { get; set; }
        public int EmployeeID { get; set; }
        public int Count { get; set; }
    }

    public class WaitingListDetailedModel
    {
        public string DateString { get; set; }
        public string StartTime { get; set; }
        public string EndTime { get; set; }
        public WaitingList WaitingList { get; set; }
        public Customer Customer { get; set; }
        public float TotalDuration { get; set; }
        public Employee Employee { get; set; }
        public List<ServiceAppViewModel> Services { get; set; }
    }

    public class AppointmentActionViewModel
    {

        public List<Employee> Employees { get; set; }
        public List<Service> AbsenseServices { get; set; }
        public int WaitingListID { get; set; }

        public string Start { get; set; }
        public string End { get; set; }
        public int ID { get; set; }
        public bool AllEmployees { get; set; }

        public string Business { get; set; }
        public string Service { get; set; }
        public float Discount { get; set; }
        public string ServiceDuration { get; set; }
        public string ServiceDiscount { get; set; }
        public List<ServiceAppViewModel> ServiceAlotted { get; set; }
        public List<ServiceModel> Services { get; set; }
        public DateTime Date { get; set; }
        public DateTime Time { get; set; }   //Start Time - All Service Duration Mins ++
        //If Repeat Appointment
        public bool IsRepeat { get; set; }
        public string Color { get; set; }
        public int EmployeeID { get; set; }
        public string Frequency { get; set; }
        public string Every { get; set; }
        public string Days { get; set; }
        public string EndsDay { get; set; }
        public string EndsMonth { get; set; }
        public string EndsWeek { get; set; }

        public string EveryWeek { get; set; }

        public DateTime EndsDateWeek { get; set; }
        public int EndsNumberOfTimesWeek { get; set; }

        public string EveryDay { get; set; }

        public DateTime EndsDateDay { get; set; }
        public int EndsNumberOfTimesDay { get; set; }

        public string EveryMonth { get; set; }

        public DateTime EndsDateMonth { get; set; }
        public int EndsNumberOfTimesMonth { get; set; }


        //Other
        public string Notes { get; set; }
        public float TotalCost { get; set; }

        public bool IsWalkIn { get; set; }       //If Walk In  is true //No Customer Selected If False Customer Selected
        public int CustomerID { get; set; }
        public List<Customer> Customers { get; set; }
        public Customer Customer { get; set; }
        public DateTime BookingDate { get; set; }
        public string Label { get; set; }

        public string Status { get; set; }
        public float Deposit { get; set; }
        public string DepositMethod { get; set; }
        public int OnlinePriceChange { get; set; }
        public PriceChange PriceChangeOnline { get; set; }
        public Company Company { get; set; }
        public bool IsWaitingList { get; set; }
        public DateTime EndTime { get; set; }
    }
}