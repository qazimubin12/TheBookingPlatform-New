using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using TheBookingPlatform.Entities;

namespace TheBookingPlatform.ViewModels
{
    public class ProductListingViewModel
    {
        public List<ProductModel> Products { get; set; }
        public string Selected { get; set; }
        public string SearchTerm { get; set; }
    }

    public class ProductModel
    {
        public string Name { get; set; }
        public string Category { get; set; }
        public float Price { get; set; }
        public string VAT { get; set; }
        public int ID { get; set; }
        public bool ManageStockOrder { get; set; }
        public int CurrentStock { get; set; }
        //public Product Product { get; set; }
        //public Supplier Supplier { get; set; }
    }


    public class ProductImportViewModel
    {
        public List<Vat> Vats { get; set; }
    }
    public class ProductActionViewModel
    {
        public string Business { get; set; }

        public int ID { get; set; }
        public string Name { get; set; }
        public float SalesPrice { get; set; }
        public string PartNumber { get; set; }

        public float CostPrice { get; set; }
        public string EANCode { get; set; }
        public string VAT { get; set; }

        public List<Vat> Vats { get; set; }
        public bool ManageStockOrder { get; set; }
        public int MinStock { get; set; }
        public int MaxStock { get; set; }
        public int CurrentStock { get; set; }
        public int CategoryID { get; set; }
        public List<Category> Categories { get; set; }
        public int SupplierID { get; set; }
        public List<Supplier> Suppliers { get; set; }
    }
}