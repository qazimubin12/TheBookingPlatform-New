using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheBookingPlatform.Entities
{
    public class StockHistory:BaseEntity
    {
        public int ProductID { get; set; }
        public DateTime Date { get; set; }
        public int Amount { get; set; }
        public string Reason { get; set; }

    }
}
