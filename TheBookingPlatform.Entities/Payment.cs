using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheBookingPlatform.Entities
{
    public class Payment : BaseEntity
    {
        public string UserID { get; set; }
        public int PackageID { get; set; }
        public DateTime LastPaidDate { get; set; }

    }
}
