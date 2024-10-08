using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheBookingPlatform.Entities
{
    public class StockOrder:BaseEntity
    {
        public int SupplierID { get; set; }
        public string SupplierName { get; set; }
        public string ProductDetails { get; set; } //ProductName,PartNumber,Amount,Quantity,Received,Total     JSON STRINGIFY
        public float GrandTotal { get; set; }
        public string Status { get; set; } //Open For Just Save /// Ordered For Placing Order //// Complete for Getting all Products Requested 
                                           // BackOrder for Getting Less as Requested
        public bool IsDraft { get; set; }
        public string Notes { get; set; }
        public DateTime CreatedDate { get; set; } // If Saving Draft 
        public DateTime OrderedDate { get; set; } // For Placing Order
    }
}
