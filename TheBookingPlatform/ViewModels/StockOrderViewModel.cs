using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using TheBookingPlatform.Entities;

namespace TheBookingPlatform.ViewModels
{
    public class StockOrderListingViewModel
    {
        public List<StockOrderModel> StockOrders { get; set; }
        public string SearchTerm { get; set; }
    }

    public class StockOrderModel
    {
        public StockOrder StockOrder { get; set; }
        public int Quantity { get; set; }
    }


    public class MainStockOrderProductModel 
    {
        public int ProductID { get; set; }
        public string ProductName { get; set; }
        public string PartNumber { get; set; }
        public float Price { get; set; }
        public int OrderedQty { get; set; }
        public int Recieved { get; set; }
        public int Difference { get; set; }
        public float Total { get; set; }
    }


    public class MainStockOrderProductModelStringVersion
    {
        public string ProductID { get; set; }
        public string ProductName { get; set; }
        public string PartNumber { get; set; }
        public string Price { get; set; }
        public string OrderedQty { get; set; }
        public string Recieved { get; set; }
        public string Difference { get; set; }
        public string Total { get; set; }
    }

    //public class StockOrderNewProductModel  //for Drafting
    //{

    //}


    //public class StockOrderProductModel
    //{
    //    public string ProductName { get; set; }
    //    public string PartNumber { get; set; }
    //    public float Price { get; set; }
    //    public int Quantity { get; set; }
    //    public int Recieved { get; set; }
    //    public int Difference { get; set; }
    //    public float Total { get; set; }
    //}

    public class StockOrderActionViewModel
    {
        public int ID { get; set; }
        public int SupplierID { get; set; }
        public List<SupplierModel> Suppliers { get; set; }
        public string SupplierName { get; set; }
        public List<Product> Products { get; set; }
        public List<Category> Categories { get; set; }
        public string ProductDetails { get; set; } //ProductName,PartNumber,Amount,Quantity,Received,Total     JSON STRINGIFY
        public List<MainStockOrderProductModel> ProductInCart { get; set; }
        public float GrandTotal { get; set; }
        public string Status { get; set; } //Open For Just Save /// Ordered For Placing Order //// Complete for Getting all Products Requested 
                                           // BackOrder for Getting Less as Requested
        public bool IsDraft { get; set; }
        public DateTime CreatedDate { get; set; } // If Saving Draft 
        public DateTime OrderedDate { get; set; } // For Placing Order
    }


    public class StockOrderDetailViewModel
    {
        public int ID { get; set; }
        public StockOrder StockOrder { get; set; }
        public List<MainStockOrderProductModel> ProductDetails { get; set; }
        public string ProductDetailsProcessing { get; set; } //ProductName,PartNumber,Amount,Quantity,Received,Total     JSON STRINGIFY

    }
}