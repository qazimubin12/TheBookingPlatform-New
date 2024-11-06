using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheBookingPlatform.Entities
{
    public class SaleProduct:BaseEntity
    {
        public int ReferenceID { get; set; }
        public int ProductID { get; set; }
        public float Qty { get; set; }
        public float Total { get; set; }
        
    }
}
