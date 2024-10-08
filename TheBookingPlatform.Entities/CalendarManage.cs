using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheBookingPlatform.Entities
{
    public class CalendarManage:BaseEntity
    {
        public string UserID { get; set; }
        public string ManageOf { get; set; }

    }
}
