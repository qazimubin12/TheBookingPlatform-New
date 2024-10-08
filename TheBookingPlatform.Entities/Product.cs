using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheBookingPlatform.Entities
{
    public class Product:BaseEntity
    {
        public string Name { get; set; }
        public float SalesPrice { get; set; }
        public float CostPrice { get; set; }
        public string EANCode { get; set; }
        public string PartNumber { get; set; }
        public string VAT { get; set; }
        public bool ManageStockOrder { get; set; }
        public int MinStock { get; set; }
        public int MaxStock { get; set; }
        public int CurrentStock { get; set; }
        public int CategoryID { get; set; }
        public int SupplierID { get; set; }
    }
}
