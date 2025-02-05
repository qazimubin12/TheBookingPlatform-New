using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheBookingPlatform.Entities
{
    public class Invoice:BaseEntity
    {
        public int AppointmentID { get; set; }
        public string PaymentMethod { get; set; }
        public int CustomerID { get; set; }
        public int EmployeeID { get; set; }
        public float GrandTotal { get; set; }
        public float TipAmount { get; set; }
        public string TipType { get; set; }
        public bool Tip { get; set; }
        public DateTime OrderDate { get; set; }
        public string OrderNo { get; set; }
        public string Remarks { get; set; }
    }
}
