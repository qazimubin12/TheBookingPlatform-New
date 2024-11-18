using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheBookingPlatform.Entities
{
    public class SumUpToken:BaseEntity
    {
        public string Token { get; set; }
        public string PaymentMessage { get; set; }
    }
}
