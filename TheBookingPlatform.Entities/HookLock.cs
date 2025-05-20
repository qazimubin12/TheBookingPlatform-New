using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheBookingPlatform.Entities
{
    public class HookLock:BaseEntity
    {
        public bool IsLocked { get; set; }
        public int EmployeeID { get; set; }
    }
}
