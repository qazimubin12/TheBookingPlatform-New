using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using TheBookingPlatform.Entities;

namespace TheBookingPlatform.ViewModels
{
    public class NInvoiceListingViewModel
    {
        public List<NInvoiceModel> NInvoices { get; set; }

    }
    public class NInvoiceModel
    {
        public NInvoice NInvoice { get; set; }
        public Vat Vat { get; set; }
    }
    public class NInvoiceItemModel
    {
        public string Service { get; set; }
        public string Amount { get; set; }
        public string Duration { get; set; }
        public string Price { get; set; }

    }

    public class NInvoiceActionViewModel
    {
        public int ID { get; set; }
        public DateTime IssueDate { get; set; }
        public int VAT { get; set; }
        public Vat VATFULL { get; set; }
        public List<Vat> Vats { get; set; }
        public string InvoiceNo { get; set; }
        public string CustomerName { get; set; }
        public string CustomerEmail { get; set; }
        public string CompanyLogo { get; set; }

        public string CustomerPhone { get; set; }
        public string CustomerAddress { get; set; }



        public string CompanyName { get; set; }
        public string CompanyEmail { get; set; }
        public string CompanyPhone { get; set; }
        public string CompanyAddress { get; set; }

        public DateTime DueDate { get; set; }
        public string PaymentMethod { get; set; }
        public List<NInvoiceItemModel> Items { get; set; }
        public string ItemDetails { get; set; } //Service ID and Qty and Total _ separated.
        public string Remarks { get; set; }
        public string Currency { get;  set; }
    }
}