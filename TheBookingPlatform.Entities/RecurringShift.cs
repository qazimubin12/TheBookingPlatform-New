using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheBookingPlatform.Entities
{
    public class RecurringShift:BaseEntity
    {
        public int ShiftID { get; set; }
        public string Frequency { get; set; } //Weekly //Bi-Weekly
        public string RecurEnd { get; set; } //Never //Custom Date
        public string RecurEndDate { get; set; }
        public bool IsDeleted { get; set; }
    }
}
