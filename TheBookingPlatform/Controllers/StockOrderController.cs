using Microsoft.Ajax.Utilities;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;

using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TheBookingPlatform.Entities;
using TheBookingPlatform.Services;
using TheBookingPlatform.ViewModels;

namespace TheBookingPlatform.Controllers
{
    public class StockOrderController : Controller
    {

        #region UserManagerRegion
        private AMSignInManager _signInManager;
        private AMUserManager _userManager;
        public AMSignInManager SignInManager
        {
            get
            {
                return _signInManager ?? HttpContext.GetOwinContext().Get<AMSignInManager>();
            }
            private set
            {
                _signInManager = value;
            }
        }
        public AMUserManager UserManager
        {
            get
            {
                return _userManager ?? HttpContext.GetOwinContext().GetUserManager<AMUserManager>();
            }
            private set
            {
                _userManager = value;
            }
        }
        private AMRolesManager _rolesManager;
        public AMRolesManager RolesManager
        {
            get
            {
                return _rolesManager ?? HttpContext.GetOwinContext().GetUserManager<AMRolesManager>();
            }
            private set
            {
                _rolesManager = value;
            }
        }

        public StockOrderController()
        {
        }

        public StockOrderController(AMUserManager userManager, AMSignInManager signInManager)
        {
            UserManager = userManager;
            SignInManager = signInManager;
        }

        #endregion
        // GET: StockOrder

        [NoCache]
        public ActionResult Index(string SearchTerm = "")
        {
            StockOrderListingViewModel model = new StockOrderListingViewModel();
                        var LoggedInUser = UserManager.FindById(User.Identity.GetUserId()); if(LoggedInUser == null) { return RedirectToAction("Login", "Account"); }
            model.SearchTerm = SearchTerm;
            var ListOfStockOrder = new List<StockOrderModel>();
            if(LoggedInUser.Role != "Super Admin")
            {
                var StockOrders = StockOrderServices.Instance.GetStockOrder(SearchTerm).Where(x => x.Business == LoggedInUser.Company).ToList();
                foreach (var item in StockOrders)
                {
                    int Quantity = JsonConvert.DeserializeObject<List<MainStockOrderProductModel>>(item.ProductDetails).Sum(x => x.OrderedQty);
                    ListOfStockOrder.Add(new StockOrderModel { Quantity = Quantity,StockOrder = item});
                }

            }
            else
            {
               var StockOrders = StockOrderServices.Instance.GetStockOrder(SearchTerm);
                foreach (var item in StockOrders)
                {
                    int Quantity = JsonConvert.DeserializeObject<List<MainStockOrderProductModel>>(item.ProductDetails).Sum(x => x.OrderedQty);
                    ListOfStockOrder.Add(new StockOrderModel { Quantity = Quantity, StockOrder = item });
                }
            }
            model.StockOrders = ListOfStockOrder;
            return View(model);
        }

        [HttpGet]
        public JsonResult GetProduct(int ID)
        {
            var Product = ProductServices.Instance.GetProduct(ID);
            return Json(Product,JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult GetProductsAccordingToSupplier(int SupplierID)
        {
            var LoggedInUser = UserManager.FindById(User.Identity.GetUserId());
            var Products = new List<Product>();
            if(LoggedInUser.Role == "Super Admin")
            {
                Products = ProductServices.Instance.GetProductsWRTSupplierID(SupplierID);
            }
            else
            {
                Products = ProductServices.Instance.GetProductsWRTSupplierID(LoggedInUser.Company,SupplierID);

            }

            return Json(Products, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult PayOrder(int StockOrderID)
        {
            var StockOrder = StockOrderServices.Instance.GetStockOrder(StockOrderID);
            StockOrder.Status = "Paid";
            StockOrderServices.Instance.UpdateStockOrder(StockOrder);
            return Json(new { success = true }, JsonRequestBehavior.AllowGet);

        }


        [HttpPost]
        public JsonResult ProcessStockOrder(StockOrderDetailViewModel model)
        {
            var StockOrder = StockOrderServices.Instance.GetStockOrder(model.ID);
            StockOrder.Status = "Complete";


            var ProductsData = JsonConvert.DeserializeObject<List<MainStockOrderProductModelStringVersion>>(model.ProductDetailsProcessing);
            foreach (var item in ProductsData)
            {
                var product = ProductServices.Instance.GetProduct(item.ProductName);
                product.CurrentStock += int.Parse(item.Recieved);
                ProductServices.Instance.UpdateProduct(product);
            }


            var StockOrderNewProductModel = new List<MainStockOrderProductModel>();
            foreach (var item in ProductsData)
            {
                var product = ProductServices.Instance.GetProduct(item.ProductName);
                StockOrderNewProductModel.Add(new MainStockOrderProductModel
                {
                    ProductName = product.Name,
                    Price = float.Parse(item.Price),
                    ProductID = product.ID,
                    OrderedQty = int.Parse(item.OrderedQty),
                    PartNumber = product.PartNumber,
                    Difference = int.Parse(item.Difference),
                    Recieved = int.Parse(item.Recieved),
                    Total = float.Parse(item.Total)
                    
                }) ;

            }

            //Check this agian
            StockOrder.ProductDetails = JsonConvert.SerializeObject(StockOrderNewProductModel);
            StockOrderServices.Instance.UpdateStockOrder(StockOrder);
            return Json(new {success=true,StockOrderID =model.ID},JsonRequestBehavior.AllowGet);    
        }

        [HttpGet]
        public ActionResult ProcessOrderView(int ID)
        {
            StockOrderDetailViewModel model = new StockOrderDetailViewModel();
            model.StockOrder = StockOrderServices.Instance.GetStockOrder(ID);
            if(model.StockOrder.Status == "Ordered")
            {
                var ListOfStockOrderProductDetails = new List<MainStockOrderProductModel>();
                var ProductsData = JsonConvert.DeserializeObject<List<MainStockOrderProductModel>>(model.StockOrder.ProductDetails);
                foreach (var item in ProductsData)
                {
                    var Product = ProductServices.Instance.GetProduct(item.ProductID);
                    ListOfStockOrderProductDetails.Add(new MainStockOrderProductModel
                    {
                        ProductID = Product.ID,
                        ProductName = Product.Name,
                        PartNumber = Product.PartNumber,
                        Price = Product.CostPrice,
                        OrderedQty = item.OrderedQty,
                        Recieved = item.OrderedQty,
                        Total = item.Total,
                        Difference = 0
                    });
                }
                model.ProductDetails = ListOfStockOrderProductDetails;
            }
            else
            {
                var ListOfStockOrderProductDetails = new List<MainStockOrderProductModel>();
                var ProductsData = JsonConvert.DeserializeObject<List<MainStockOrderProductModel>>(model.StockOrder.ProductDetails);
                foreach (var item in ProductsData)
                {
                    var Product = ProductServices.Instance.GetProduct(item.ProductID);
                    ListOfStockOrderProductDetails.Add(new MainStockOrderProductModel
                    {
                        ProductName = Product.Name,

                        PartNumber = Product.PartNumber,
                        Price = Product.CostPrice,
                        OrderedQty = item.OrderedQty,
                        Recieved = item.Recieved,
                        Total = item.Total,
                        Difference = item.Difference
                    });
                }
                model.ProductDetails = ListOfStockOrderProductDetails;
            }
           
            return PartialView("_ProcessOrderView",model);
        }


        [HttpPost]
        public JsonResult SaveNotes(int StockOrderID, string Notes)
        {
            var StockOrder = StockOrderServices.Instance.GetStockOrder(StockOrderID);
            StockOrder.Notes = Notes;
            StockOrderServices.Instance.UpdateStockOrder(StockOrder);
            return Json(new {success=true},JsonRequestBehavior.AllowGet);
        }

        [NoCache]
        public ActionResult ShowStockOrders(string SearchTerm = "")
        {
            StockOrderListingViewModel model = new StockOrderListingViewModel();
                        var LoggedInUser = UserManager.FindById(User.Identity.GetUserId()); if(LoggedInUser == null) { return RedirectToAction("Login", "Account"); }
            model.SearchTerm = SearchTerm;
            var ListOfStockOrder = new List<StockOrderModel>();
            if (LoggedInUser.Role != "Super Admin")
            {
                var StockOrders = StockOrderServices.Instance.GetStockOrderWRTBusiness(LoggedInUser.Company, SearchTerm);
                foreach (var item in StockOrders)
                {
                    int Quantity = JsonConvert.DeserializeObject<List<MainStockOrderProductModel>>(item.ProductDetails).Sum(x => x.OrderedQty);
                    ListOfStockOrder.Add(new StockOrderModel { Quantity = Quantity, StockOrder = item });
                }

            }
            else
            {
                var StockOrders = StockOrderServices.Instance.GetStockOrder(SearchTerm);
                foreach (var item in StockOrders)
                {
                    int Quantity = JsonConvert.DeserializeObject<List<MainStockOrderProductModel>>(item.ProductDetails).Sum(x => x.OrderedQty);
                    ListOfStockOrder.Add(new StockOrderModel { Quantity = Quantity, StockOrder = item });
                }
            }
            model.StockOrders = ListOfStockOrder;
            return PartialView("Index", model);
        }

        [HttpGet]
        [NoCache]
        public ActionResult Action(int ID=0)
        {
            StockOrderActionViewModel model = new StockOrderActionViewModel();
                        var LoggedInUser = UserManager.FindById(User.Identity.GetUserId()); if(LoggedInUser == null) { return RedirectToAction("Login", "Account"); }
            var ListOfSuppliers = new List<SupplierModel>();
            if (LoggedInUser.Role != "Super Admin")
            {
                var Suppliers = SupplierServices.Instance.GetSupplierWRTBusiness(LoggedInUser.Company);
                foreach (var item in Suppliers)
                {
                    float TotalInventory = 0;
                    var products = ProductServices.Instance.GetProductsWRTSupplierID(LoggedInUser.Company, item.ID);
                    foreach (var prod in products)
                    {
                        TotalInventory += prod.CostPrice * prod.CurrentStock;
                    }
                    ListOfSuppliers.Add(new SupplierModel { Supplier = item, TotalInventory = TotalInventory });

                }
                model.Suppliers = ListOfSuppliers;
                model.Products = ProductServices.Instance.GetProducts(LoggedInUser.Company).Where(x =>x.ManageStockOrder == true).ToList();

            }
            else
            {
                var Suppliers = SupplierServices.Instance.GetSupplier();
                foreach (var item in Suppliers)
                {
                    float TotalInventory = 0;
                    var products = ProductServices.Instance.GetProductsWRTSupplierID(item.ID);
                    foreach (var prod in products)
                    {
                        TotalInventory += prod.CostPrice * prod.CurrentStock;
                    }
                    ListOfSuppliers.Add(new SupplierModel { Supplier = item, TotalInventory = TotalInventory });

                }
                model.Suppliers = ListOfSuppliers;
                model.Products = ProductServices.Instance.GetProducts().Where( x=>x.ManageStockOrder == true).ToList();
            }
            if (ID != 0)
            {
                var StockOrder = StockOrderServices.Instance.GetStockOrder(ID);
                model.ID = ID;
                model.SupplierID=StockOrder.SupplierID ;
                model.SupplierName=StockOrder.SupplierName;
                model.ProductInCart = JsonConvert.DeserializeObject<List<MainStockOrderProductModel>>(StockOrder.ProductDetails);
                model.GrandTotal=StockOrder.GrandTotal;
                model.Status=StockOrder.Status;
                model.IsDraft=StockOrder.IsDraft ;
                model.CreatedDate=StockOrder.CreatedDate;
                model.OrderedDate = StockOrder.OrderedDate;
            }
            return View(model);
        }


        [HttpPost]
        public ActionResult ActionStartOrder(StockOrderActionViewModel model)
        {
                        var LoggedInUser = UserManager.FindById(User.Identity.GetUserId()); if(LoggedInUser == null) { return RedirectToAction("Login", "Account"); }
            if (model.ID == 0)
            {
                var supplier = SupplierServices.Instance.GetSupplier(model.SupplierID);
                var stockOrder = new StockOrder();
                stockOrder.Status = "Ordered";
                stockOrder.Business = LoggedInUser.Company;
                stockOrder.SupplierName = supplier.Name;
                stockOrder.SupplierID = supplier.ID;
                stockOrder.GrandTotal = model.GrandTotal;
                stockOrder.CreatedDate = DateTime.Now;
                stockOrder.OrderedDate = DateTime.Now;
                stockOrder.IsDraft = false;

                stockOrder.ProductDetails = model.ProductDetails;

                StockOrderServices.Instance.SaveStockOrder(stockOrder);
                return Json(new { success = true, StockOrderID = stockOrder.ID }, JsonRequestBehavior.AllowGet);

            }
            else
            {
                var supplier = SupplierServices.Instance.GetSupplier(model.SupplierID);
                var stockOrder = StockOrderServices.Instance.GetStockOrder(model.ID);
                stockOrder.Status = "Ordered";
                stockOrder.Business = LoggedInUser.Company;
                stockOrder.SupplierName = supplier.Name;
                stockOrder.SupplierID = supplier.ID;
                stockOrder.GrandTotal = model.GrandTotal;
                stockOrder.CreatedDate = stockOrder.CreatedDate;
                stockOrder.OrderedDate = DateTime.Now;
                stockOrder.IsDraft = false;
                stockOrder.ProductDetails = model.ProductDetails;
                var Deserializer = JsonConvert.DeserializeObject<List<MainStockOrderProductModel>>(model.ProductDetails);

                StockOrderServices.Instance.UpdateStockOrder(stockOrder);
                return Json(new { success = true, StockOrderID = stockOrder.ID }, JsonRequestBehavior.AllowGet);

            }

        }


        [HttpGet]
        [NoCache]
        public ActionResult ShowStockOrderDetail(int StockOrderID)
        {
            StockOrderDetailViewModel model = new StockOrderDetailViewModel();
            model.StockOrder = StockOrderServices.Instance.GetStockOrder(StockOrderID);
            var ListOfStockOrderProductDetails = new List<MainStockOrderProductModel>();
            var ProductsData = JsonConvert.DeserializeObject<List<MainStockOrderProductModel>>(model.StockOrder.ProductDetails);
            foreach (var item in ProductsData)
            {
                var Product = ProductServices.Instance.GetProduct(item.ProductID);
                ListOfStockOrderProductDetails.Add(new MainStockOrderProductModel { ProductName = Product.Name, PartNumber = Product.PartNumber, Price = Product.CostPrice, OrderedQty = item.OrderedQty, Recieved = item.Recieved, Total = item.Total });
            }
            model.ProductDetails = ListOfStockOrderProductDetails;
            return View(model);
        }

        [HttpPost]
        public ActionResult Action(StockOrderActionViewModel model)
        {
                        var LoggedInUser = UserManager.FindById(User.Identity.GetUserId()); if(LoggedInUser == null) { return RedirectToAction("Login", "Account"); }
            if (model.ID == 0)
            {
                var supplier = SupplierServices.Instance.GetSupplier(model.SupplierID);
                var stockOrder = new StockOrder();
                stockOrder.Status = "Open";
                stockOrder.Business = LoggedInUser.Company;
                stockOrder.SupplierName = supplier.Name;
                stockOrder.SupplierID = supplier.ID;
                stockOrder.GrandTotal = model.GrandTotal;
                stockOrder.CreatedDate = DateTime.Now;
                stockOrder.OrderedDate = DateTime.Now;
                stockOrder.IsDraft = true;
                var Deserializer = JsonConvert.DeserializeObject<List<MainStockOrderProductModel>>(model.ProductDetails);
                stockOrder.ProductDetails = model.ProductDetails;


                StockOrderServices.Instance.SaveStockOrder(stockOrder);
            }
            else
            {
                var supplier = SupplierServices.Instance.GetSupplier(model.SupplierID);
                var stockOrder = StockOrderServices.Instance.GetStockOrder(model.ID);
                stockOrder.Status = "Open";
                stockOrder.Business = LoggedInUser.Company;
                stockOrder.SupplierName = supplier.Name;
                stockOrder.SupplierID = supplier.ID;
                stockOrder.GrandTotal = model.GrandTotal;
                stockOrder.CreatedDate = DateTime.Now;
                stockOrder.OrderedDate = DateTime.Now;
                stockOrder.IsDraft = true;
                stockOrder.ProductDetails = model.ProductDetails;
                StockOrderServices.Instance.UpdateStockOrder(stockOrder);
            }
            return Json(new { success = true }, JsonRequestBehavior.AllowGet);
        }


        [HttpGet]
        public ActionResult Delete(int ID)
        {
            StockOrderActionViewModel model = new StockOrderActionViewModel();
            var StockOrder = StockOrderServices.Instance.GetStockOrder(ID);
            model.ID = StockOrder.ID;
            return PartialView("_Delete", model);
        }


        [HttpPost]
        public ActionResult Delete(StockOrderActionViewModel model)
        {
            var StockOrder = StockOrderServices.Instance.GetStockOrder(model.ID);
            StockOrderServices.Instance.DeleteStockOrder(StockOrder.ID);

            return Json(new { success = true }, JsonRequestBehavior.AllowGet);
        }
    }
}