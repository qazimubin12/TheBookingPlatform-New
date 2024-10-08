using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheBookingPlatform.Entities
{
    public class GiftCardAssignment:BaseEntity
    {
        public int GiftCardID { get; set; }
        public int CustomerID { get; set; }
        public string AssignedCode { get; set; }
        public float AssignedAmount { get; set; }
        public DateTime AssignedDate { get; set; }
        public int Days { get; set; }
        public float Balance { get; set; }
        //public string FirstName { get; set; }
        //public string LastName { get; set; }
        //public string Email { get; set; }
        //public string MobileNumber { get; set; }

    }
}
