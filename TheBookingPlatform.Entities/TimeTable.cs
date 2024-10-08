using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheBookingPlatform.Entities
{
    public class TimeTable:BaseEntity
    {
        public int EmployeeID { get; set; }
        public DateTime StartDate { get; set; }

        //Each Day 
        public string Type { get; set; } //Weekly or biWeekly // According to Start Date
        public string Day { get; set; }
        public string TimeStart { get; set; }
        public string TimeEnd { get; set; }
        public DateTime Date { get; set; }


        public bool Repeat { get; set; }
        public string RepeatEnd { get; set; } //Never //Custom Date
        public string DateOfRepeatEnd { get; set; }









    }
}
