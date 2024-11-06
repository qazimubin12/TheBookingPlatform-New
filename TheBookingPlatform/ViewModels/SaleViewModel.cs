using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using TheBookingPlatform.Entities;

namespace TheBookingPlatform.ViewModels
{
    public class SaleActionViewModel
    {
        public int ID { get; set; }
        public Company Company { get; set; }
        public int AppointmentID { get; set; }
        public Appointment Appointment { get; set; }
        public Customer Customer { get; set; }
        public int CustomerID { get; set; }
        public List<Product> Products { get; set; }
        public string Type { get; set; }
        public string Remarks { get; set; }
        public DateTime Date { get; set; }
        public List<SaleProductModel> SaleProducts { get; set; }
        public Sale Sale { get;  set; }
        public List<Customer> Customers { get;  set; }
    }

    public class SaleProductModel
    {
        public Product Product { get; set; }
        public float Qty { get; set; }
        public float Total { get; set; }
    }

    public class SaleProductJS
    {
        public int ProductID { get; set; }
        public int Qty { get; set; }
        public float Total { get; set; }
    }

    public class SaleRequest
    {
        public int ID { get; set; }
        public int AppointmentID { get; set; }

        public int CustomerID { get; set; }
        public string Remarks { get; set; }
        public List<SaleProductJS> SaleProducts { get; set; }
    }
}