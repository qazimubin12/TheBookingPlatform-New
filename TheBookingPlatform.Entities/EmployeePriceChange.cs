using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheBookingPlatform.Entities
{
    public class EmployeePriceChange:BaseEntity
    {
        public int EmployeeID { get; set; }
        public string TypeOfChange { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public float Percentage { get; set; }
        public bool Repeat { get; set; }
        public string Frequency { get; set; }
        public string Every { get; set; }
        public string Days { get; set; }
        public string Ends { get; set; }
    }
}
