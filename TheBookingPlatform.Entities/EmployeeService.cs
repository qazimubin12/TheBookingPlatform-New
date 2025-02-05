using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheBookingPlatform.Entities
{
    public class EmployeeService:BaseEntity
    {
        public int EmployeeID { get; set; }
        public int ServiceID { get; set; }
        public bool BufferEnabled { get; set; }
        public string BufferTime { get; set; }
    }
}
