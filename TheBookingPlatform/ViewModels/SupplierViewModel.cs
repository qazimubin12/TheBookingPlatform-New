using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using TheBookingPlatform.Entities;

namespace TheBookingPlatform.ViewModels
{
    public class SupplierListingViewModel
    {
        public List<SupplierModel> Suppliers { get; set; }
        public string SearchTerm { get; set; }
    }

    public class SupplierModel
    {
        public Supplier Supplier { get; set; }
        public float TotalInventory { get; set; }

    }
    public class SupplierActionViewModel
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Address { get; set; }
        public int PostalCode { get; set; }
        public string City { get; set; }
        public float TotalInventory { get; set; }   //Cost Price * Quantity of all the Products Linked with this Supplier
    }
}