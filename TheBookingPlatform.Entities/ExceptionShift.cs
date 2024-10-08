using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheBookingPlatform.Entities
{
    public class ExceptionShift:BaseEntity
    {
        public DateTime ExceptionDate { get; set; }
        public string StartTime { get; set; }
        public string EndTime { get; set; }
        public int ShiftID { get; set; }
        public bool IsNotWorking { get; set; }
    }
}
