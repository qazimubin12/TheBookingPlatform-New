using TheBookingPlatform.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TheBookingPlatform.ViewModels
{
    public class AdminViewModel
    {
        public User SignedInUser { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int IntervalCalendar { get; set; }
        public Company Company { get; set; }
        public List<OpeningHour> OpeningHours { get; set; }
        public string Name { get; set; }
        public string Business { get; set; }

        public string ID { get; set; }
        public string Email { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public string Contact { get; set; }
        public string SearchTerm { get; set; }
        public List<Appointment> AllAppointments { get; set; }
        public List<Shift> Shifts { get;  set; }
        public int NewClients { get;  set; }
        public List<Customer> LostClientsList { get;  set; }
        public int ReturnedClients { get;  set; }
        public int LostClients { get;  set; }
        public string FilterDuration { get;  set; }
        public List<EmployeeOccupancy> EmployeeOccupancies { get; set; }
        public string Role { get;  set; }
    }


    public class  EmployeeOccupancy
    {
        public Employee Employee { get; set; }
        public float Percentage { get; set; }
        public float WorkedHours { get; set; }
        public float TotalTime { get; set; }

    }
    public class DayWiseSale
    {
        public DateTime Date { get; set; }
        public float DaySale { get; set; }
    }

    public class ClientVisitation
    {
        public string Date { get; set; }
        public int NoOfClients { get; set; }
    }
    public class AnalysisViewModel
    {
        public float saleProductsAmount { get; set; }

        public List<DayWiseSale> DayWiseSales { get; set; }
        public List<Employee> Employees { get; set; }
        public List<string> Statuses { get; set; }


        



        //Filtered Given
        public string SelectedEmployeeIDs { get; set; }
        public string Status { get; set; }
        public DateTime StartDate { get; set; }
        public bool IsCancelled { get; set; }
        public DateTime EndDate { get; set; }


        //Date Returning
        public float SumOfCashDeposit { get; set; }
        public float SumOfOnlineDeposit { get; set; }
        public float SumOfPinDeposit { get; set; }

        public float TotalServicePrices { get; set; }
        public float TotalOfflineDiscount { get; set; }
        public float TotalPriceAfterOfflineDiscount { get; set; }
        public float TotalOnlinePriceChange { get; set; }
        public float TotalAfterPriceChange { get; set; }
        //public List<EarningOverTimePeriod> EarningOverTimePeriods { get; set; }
        public List<ClientVisitation> NumberOfClientsOnEachDays { get; set; }

        public int CheckoutAppointmentsCount { get; set; }
        public int NoShowAppointmentsCount { get; set; }
        public int PendingAppointmentsCount { get; set; }
        public int CancelledAppointmentCount { get; set; }
        public int TotalNoOfAppointments { get; set; }




        public float CheckedOutCashCount { get; set; }
        public float CheckedOutPinCount { get; set; }
        public float CheckedOutCardCount { get; set; }
        public float CheckedOutGiftCardCount { get; set; }
        public float CheckedOutLoyaltyCardCount { get; set; }
        public Dictionary<string, int> serviceBookingCounts { get; set; }
        public int NewClients { get; set; }
        public int ReturnedClients { get; set; }
        public int LostClients { get; set; }
        public Dictionary<string, int> serviceCategoryBookingCount { get;  set; }


        public int NoOfRebookReminderSent    { get; set; }
        public int NoOfRebookReminderClicked { get; set; }
        public int NoOfRebookReminderOpened   { get; set; }
        public int AppointmentsByRebook         { get; set; }
        public float TotalEmployeePriceChange { get;  set; }
        public List<Customer> LostClientsList { get;  set; }
    }


    public class PopularServices
    {
        public int ServiceCount { get; set; }
        public string ServiceName { get; set; }
    }

    public class PopularServiceCategory
    {
        public int CategoryCount { get; set; }
        public string CategoryName { get; set; }
    }


    public class EarningOverTimePeriod
    {
        public DateTime Date { get; set; }
        public float TotalAfterPriceChange { get; set; }
    }
    public class HistoryModel
    {
  
        public History History { get; set; }
    }

  
    public class HomeViewModel
    {
        public string Error { get; set; }
        public User SignedInUser { get; set; }
        public List<History> Histories { get; set; }
        public List<PriceChangeModel> PriceChanges { get; set; }
        public string SearchTerm { get; set; }
        public Company Company { get; set; }
    }

    public class PriceChangeModel
    {
        public PriceChange PriceChange { get; set; }
        public PriceChangeSwitch PriceChangeSwitch { get; set; }
    }


}

