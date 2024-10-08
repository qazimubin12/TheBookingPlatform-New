using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheBookingPlatform.Entities
{
    public class EmployeeRequest:BaseEntity
    {
        public int EmployeeID { get; set; }
        public bool Accepted { get; set; }
        public string Status { get; set; } //Requested //Declined //Accepted
        public int CompanyIDFrom { get; set; }
        public int  CompanyIDFor { get; set; }
    }
}
