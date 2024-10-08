using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheBookingPlatform.Entities
{
    public class PriceChangeSwitch:BaseEntity
    {
        public int PriceChangeID { get; set; }
        public bool SwitchStatus { get; set; }
    }
}
