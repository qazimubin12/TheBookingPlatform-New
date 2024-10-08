using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using TheBookingPlatform.Entities;

namespace TheBookingPlatform.ViewModels
{
    public class InvoiceViewModel
    {
        
        public List<ServiceAppInvoiceViewModel> Services { get; set; }
        public Appointment Appointment { get; set; }
        public Customer Customer { get; set; }
        public User User { get; set; }
        public Company Company { get; set; }

        public Invoice Invoice { get; set; }
        public float VatAmount { get; set; }
    }
}