using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using TheBookingPlatform.Entities;

namespace TheBookingPlatform.ViewModels
{
    public class TimeTableListingViewModel
    {
        public List<OpeningHour> OpeningHours { get; set; }
        public List<TimeTableModel> TimeTables { get; set; }
        public List<Employee> Employees { get; set; }
        public DateTime CurrentWeekStart { get; set; }
        public DateTime CurrentWeekEnd { get; set; }
    }

    public class EmployeeModelNew
    {
        public Employee Employee { get; set; }
        public float HoursWorked { get; set; }
    }


    public class TimeTableModel
    {
        public Employee Employee { get; set; }
        public List<FullTimeTableModel> TimeTable { get; set; }
        public float HoursWorked { get; set; }
    }

    public class FullTimeTableModel
    {
        public TimeTable TimeTable { get; set; }
        public bool IsWorking { get; set; }
    }
    public class TimeTableActionViewModel
    {
        //public TimeTable TimeTable { get; set; }
        public List<TimeTable> TimeTable { get; set; }
        public Employee Employee { get; set; }
        public int ID { get; set; }
        public DateTime RosterStartDate { get; set; }
        public int EmployeeID { get; set; }
        public DateTime StartDate { get; set; }


        public bool Monday { get; set; }
        public string MondayTime { get; set; }
        public string MondayType { get; set; }

        ///weekly or biweekly
        public bool MondayRepeatCB { get; set; }
        public string MondayRepeatEnd { get; set; }
        public string MondayDateOfRepeatEnd { get; set; }

        public bool Tuesday { get; set; }
        public string TuesdayTime { get; set; }
        public string TuesdayType { get; set; }
        public bool TuesdayRepeatCB { get; set; }
        public string TuesdayRepeatEnd { get; set; }
        public string TuesdayDateOfRepeatEnd { get; set; }

        ///weekly or biweekly

        public bool Wednesday { get; set; }
        public string WednesdayTime { get; set; }
        public string WednesdayType { get; set; } ///weekly or biweekly
        public bool WednesdayRepeatCB { get; set; }
        public string WednesdayRepeatEnd { get; set; }
        public string WednesdayDateOfRepeatEnd { get; set; }

        public bool Thursday { get; set; }
        public string ThursdayTime { get; set; }
        public string ThursdayType { get; set; } ///weekly or biweekly
        public bool ThursdayRepeatCB { get; set; }
        public string ThursdayRepeatEnd { get; set; }
        public string ThursdayDateOfRepeatEnd { get; set; }


        public bool Friday { get; set; }
        public string FridayTime { get; set; }
        public string FridayType { get; set; } ///weekly or biweekly
        public bool FridayRepeatCB { get; set; }
        public string FridayRepeatEnd { get; set; }
        public string FridayDateOfRepeatEnd { get; set; }


        public bool Saturday { get; set; }
        public string SaturdayTime { get; set; }
        public string SaturdayType { get; set; } ///weekly or biweekly
        public bool SaturdayRepeatCB { get; set; }
        public string SaturdayRepeatEnd { get; set; }
        public string SaturdayDateOfRepeatEnd { get; set; }


        public bool Sunday { get; set; }
        public string SundayTime { get; set; }
        public string SundayType { get; set; } ///weekly or biweekly
        public bool SundayRepeatCB { get; set; }
        public string SundayRepeatEnd { get; set; }
        public string SundayDateOfRepeatEnd { get; set; }
    }

}