using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Permissions;
using System.Web;
using TheBookingPlatform.Entities;

namespace TheBookingPlatform.ViewModels
{
    public class ShiftListingViewModel
    {
        
        public List<Employee> Employee { get; set; }
        public List<DateTime> weekDates { get; set; }
        public List<ShiftOfEmployeeModel> Shifts { get; set; }
        public List<ShiftDetail> ShiftDetails { get; set; }
    }


    public class ShiftDetail
    {
        public DateTime Date { get; set; }
        public int EmployeeID { get; set; }
    }

    public class EmployeeShiftHoursCountModel
    {
        public int ID { get; set; }
        public float Hours { get; set; }
    }

    public class ShiftActionViewModel
    {
        public List<Employee> Employees { get; set; }
        public Employee Employee { get; set; }
        public DateTime Date { get; set; }

        public int ID { get; set; }
        public int EmployeeID { get; set; }
        public string Day { get; set; }
        public string StartTime { get; set; }
        public string EndTime { get; set; }
        public bool IsRecurring { get; set; }


        //Recurring Shift
        public string Frequency { get; set; }
        public string RecurEnd { get; set; }
        public string RecurEndDate { get; set; }


        //For Exception Shift Check
        public bool OnlyThis { get; set; }
        public bool IsNotWorking { get; set; }

    }

    public class ShiftOfEmployeeModel
    {
        public List<ShiftModel> Shifts { get; set; }
        public Employee Employee { get; set; }
        public EmployeePriceChange PriceChange { get; set; }
        public List<ExceptionShift> ExceptionShifts { get; set; }
        public int DisplayOrder { get; set; }

    }


    public class ShiftModel
    {
        public Shift Shift { get; set; }
        public RecurringShift RecurShift { get; set; }
        public List<ExceptionShift> ExceptionShift { get; set; }
    }
}