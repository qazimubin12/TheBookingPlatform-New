using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheBookingPlatform.Entities
{
    public class NInvoice:BaseEntity
    {
        public string CustomerName { get; set; }
        public string CustomerEmail { get; set; }
        public string CustomerPhone { get; set; }
        public string CustomerAddress { get; set; }
        public string CompanyLogo { get; set; }
        public string CompanyName { get; set; }
        public string CompanyEmail { get; set; }
        public string CompanyPhone { get; set; }
        public string CompanyAddress { get; set; }


        public DateTime IssueDate { get; set; }
        public int VAT { get; set; }
        public string InvoiceNo { get; set; }
        public DateTime DueDate { get; set; }
        public string PaymentMethod { get; set; }
        public string ItemDetails { get; set; } //Service IDS comma separated.
        public string Remarks { get; set; }
    }
}
