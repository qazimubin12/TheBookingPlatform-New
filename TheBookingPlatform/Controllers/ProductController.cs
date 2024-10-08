using Microsoft.AspNet.Identity.Owin;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TheBookingPlatform.Entities;
using TheBookingPlatform.Services;
using TheBookingPlatform.ViewModels;
using OfficeOpenXml;

using System.IO;
using Newtonsoft.Json;
using Category = TheBookingPlatform.Entities.Category;

namespace TheBookingPlatform.Controllers
{
    public class ProductController : Controller
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

        public ProductController()
        {
        }

        public ProductController(AMUserManager userManager, AMSignInManager signInManager)
        {
            UserManager = userManager;
            SignInManager = signInManager;
        }

        #endregion
        // GET: Product
        [NoCache]
        public ActionResult Index(string SearchTerm = "", string Selected = "")
        {
            ProductListingViewModel model = new ProductListingViewModel();
            model.SearchTerm = SearchTerm;
            model.Selected = Selected;
            return View(model);
        }


        [HttpPost]
        public ActionResult Import(HttpPostedFileBase excelfile)
        {
            if (excelfile == null || excelfile.ContentLength == 0)
            {
                ViewBag.Error = "Please Select Excel File";
                return View();
            }
            else
            {
                bool isPresent = false;
                var ProductAddedList = new List<Product>();
                ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.NonCommercial; // or LicenseContext.Commercial

                if (excelfile != null && excelfile.ContentLength > 0)
                {
                    using (var package = new ExcelPackage(excelfile.InputStream))
                    {
                        var worksheet = package.Workbook.Worksheets[0];
                        var rowCount = worksheet.Dimension.Rows;

                        for (int row = 2; row <= rowCount; row++) // Assuming the first row is header
                        {
                            var product = new Product();
                            product.Name = worksheet.Cells[row, 1].Value.ToString(); //ProductName
                            var CategoryName = worksheet.Cells[row, 2].Value.ToString();//Category Name
                            var Category = CategoryServices.Instance.GetCategory(CategoryName).FirstOrDefault();
                            product.CategoryID = Category.ID;
                            product.Business = UserManager.FindById(User.Identity.GetUserId()).Company;
                            product.SalesPrice = float.Parse(worksheet.Cells[row, 3].Value.ToString());
                            product.CostPrice = float.Parse(worksheet.Cells[row, 4].Value.ToString());
                            product.EANCode = worksheet.Cells[row, 5].Value.ToString();
                            product.PartNumber = worksheet.Cells[row, 6].Value.ToString();
                            product.VAT = worksheet.Cells[row, 7].Value.ToString();
                            product.MinStock = int.Parse(worksheet.Cells[row, 8].Value.ToString());
                            product.MaxStock = int.Parse(worksheet.Cells[row, 9].Value.ToString());
                            product.CurrentStock = int.Parse(worksheet.Cells[row, 10].Value.ToString());
                            if (product.CurrentStock != 0 || product.MaxStock != 0 || product.CurrentStock != 0)
                            {
                                product.ManageStockOrder = true;
                            }
                            else
                            {
                                product.ManageStockOrder = false;

                            }
                            var Supplier = SupplierServices.Instance.GetSupplier(worksheet.Cells[row, 11].Value.ToString()).FirstOrDefault();
                            product.SupplierID = Supplier.ID;

                            var LoggedInUser = UserManager.FindById(User.Identity.GetUserId());
                            var List = new List<Product>();
                            if(LoggedInUser.Role == "Super Admin")
                            {
                                List = ProductServices.Instance.GetProducts();
                            }
                            else
                            {
                                List = ProductServices.Instance.GetProducts(LoggedInUser.Company);
                            }



                            if (List.Count != 0)
                            {
                                foreach (var item in List)
                                {
                                    if (item.Name == product.Name && item.CategoryID == product.CategoryID)
                                    {
                                        isPresent = true;
                                        break;
                                    }
                                    else
                                    {
                                        isPresent = false;
                                    }




                                }
                                if (isPresent == false)
                                {
                                    ProductAddedList.Add(product);
                                    ProductServices.Instance.SaveProduct(product);
                                }
                            }
                            else
                            {
                                ProductAddedList.Add(product);

                                ProductServices.Instance.SaveProduct(product);

                            }
                        }

                    }
                    ViewBag.Products = ProductAddedList;
                    return RedirectToAction("Import", "Product");

                }



                else
                {
                    ViewBag.Error = "Incorrect File";
                    return RedirectToAction("Import", "Product");
                }
            }

            //var Prcoess = Process.GetProcessesByName("EXCEL.EXE").FirstOrDefault();
            //Prcoess.Kill();

        }



        [HttpGet]
        public ActionResult Import()
        {
            return View();
        }

        public JsonResult GetCategories(string SearchTerm = "")
        {
            // Retrieve data from the database
            var LoggedInUser = UserManager.FindById(User.Identity.GetUserId());
            var categories = new List<Category>();
            if(LoggedInUser.Role == "Super Admin")
            {
                categories = CategoryServices.Instance.GetCategory(SearchTerm);

            }
            else
            {
                categories = CategoryServices.Instance.GetCategoryWRTBusiness(LoggedInUser.Company,SearchTerm);
            }

            // Return data as JSON
            return Json(categories, JsonRequestBehavior.AllowGet);
        }


        public JsonResult GetStockOrders(string SearchTerm = "")
        {
            var LoggedInUser = UserManager.FindById(User.Identity.GetUserId());
            var ListOfStockOrder = new List<StockOrderModel>();
            if (LoggedInUser.Role != "Super Admin")
            {
                var StockOrders = StockOrderServices.Instance.GetStockOrder(SearchTerm).Where(x => x.Business == LoggedInUser.Company).ToList();
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
            return Json(ListOfStockOrder, JsonRequestBehavior.AllowGet);

        }

        public JsonResult GetProducts(string SearchTerm = "")
        {
            // Retrieve data from the database
            var LoggedInUser = UserManager.FindById(User.Identity.GetUserId());
            var ProductListModel = new List<ProductModel>();
            var Products = new List<Product>();
            if (LoggedInUser.Role != "Super Admin")
            {
                Products = ProductServices.Instance.GetProductWRTBusiness(LoggedInUser.Company,SearchTerm);
            }
            else
            {
                Products = ProductServices.Instance.GetProducts(SearchTerm);
            }
            foreach (var item in Products)
            {
                var Category = CategoryServices.Instance.GetCategory(item.CategoryID);
                ProductListModel.Add(new ProductModel {ID= item.ID,Category = Category.Name,CurrentStock = item.CurrentStock,ManageStockOrder = item.ManageStockOrder,
                Name=item.Name,Price = item.SalesPrice,VAT = item.VAT});

            }

            // Return data as JSON
            return Json(ProductListModel, JsonRequestBehavior.AllowGet);
        }


        public JsonResult GetSuppliers(string SearchTerm = "")
        {
            // Retrieve data from the database
            var LoggedInUser = UserManager.FindById(User.Identity.GetUserId());
            var ListOfSuppliers = new List<SupplierModel>();
            if (LoggedInUser.Role == "Super Admin")
            {
                var Suppliers = SupplierServices.Instance.GetSupplier(SearchTerm);
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
            }
            else
            {
                var Suppliers = SupplierServices.Instance.GetSupplier(SearchTerm).Where(x => x.Business == LoggedInUser.Company).ToList();
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
            }
            // Return data as JSON
            return Json(ListOfSuppliers, JsonRequestBehavior.AllowGet);
        }




        public ActionResult ShowProducts(string SearchTerm = "")
        {
            ProductListingViewModel model = new ProductListingViewModel();
            var LoggedInUser = UserManager.FindById(User.Identity.GetUserId()); if (LoggedInUser == null) { return RedirectToAction("Login", "Account"); }
            var ProductListModel = new List<ProductModel>();
            var Products = new List<Product>();
            if (LoggedInUser.Role != "Super Admin")
            {
                Products = ProductServices.Instance.GetProductWRTBusiness(LoggedInUser.Company, SearchTerm);
            }
            else
            {
                Products = ProductServices.Instance.GetProducts(SearchTerm);
            }
            foreach (var item in Products)
            {
                var Category = CategoryServices.Instance.GetCategory(item.CategoryID);
                ProductListModel.Add(new ProductModel
                {
                    ID = item.ID,
                    Category = Category.Name,
                    CurrentStock = item.CurrentStock,
                    ManageStockOrder = item.ManageStockOrder,
                    Name = item.Name,
                    Price = item.SalesPrice,
                    VAT = item.VAT
                });
            }
            model.Products = ProductListModel;
            return PartialView("_Listing", model);
        }







        [NoCache]
        [HttpGet]
        public ActionResult Action(int ID = 0)
        {
            ProductActionViewModel model = new ProductActionViewModel();
            var LoggedInUser = UserManager.FindById(User.Identity.GetUserId()); if (LoggedInUser == null) { return RedirectToAction("Login", "Account"); }
            if (LoggedInUser.Role != "Super Admin")
            {
                model.Categories = CategoryServices.Instance.GetCategoryWRTBusiness(LoggedInUser.Company);
                model.Suppliers = SupplierServices.Instance.GetSupplierWRTBusiness(LoggedInUser.Company);
                model.Vats = VatServices.Instance.GetVatWRTBusiness(LoggedInUser.Company);
            }
            else
            {
                model.Categories = CategoryServices.Instance.GetCategory();
                model.Suppliers = SupplierServices.Instance.GetSupplier();
                model.Vats = VatServices.Instance.GetVat();
            }
            if (ID != 0)
            {
                var Product = ProductServices.Instance.GetProduct(ID);
                model.ID = Product.ID;
                model.Name = Product.Name;
                model.SalesPrice = Product.SalesPrice;
                model.CostPrice = Product.CostPrice;
                model.EANCode = Product.EANCode;
                model.VAT = Product.VAT;
                model.ManageStockOrder = Product.ManageStockOrder;
                model.MinStock = Product.MinStock;
                model.MaxStock = Product.MaxStock;
                model.PartNumber = Product.PartNumber;
                model.CurrentStock = Product.CurrentStock;
                model.CategoryID = Product.CategoryID;
                model.SupplierID = Product.SupplierID;

            }
            return View(model);
        }


        public FileResult DownloadFile(string path)
        {
            if (System.IO.File.Exists(path))
            {
                // Determine the content type (MIME type) of the file
                string contentType = MimeMapping.GetMimeMapping(path);

                // Read the file data into a byte array
                byte[] fileBytes = System.IO.File.ReadAllBytes(path);

                // Create a FileContentResult to send the file to the client
                return File(fileBytes, contentType, Path.GetFileName(path));
            }
            else
            {
                // Return a not found response if the file doesn't exist
                return null;
            }
        }

        [HttpGet]
        public ActionResult ProductImport()
        {
            ProductImportViewModel model = new ProductImportViewModel();
            model.Vats = VatServices.Instance.GetVat();
            return PartialView("_ProductImport", model);
        }


        [HttpPost]
        public ActionResult Action(ProductActionViewModel model)
        {
            if (model.ID != 0)
            {
                var Product = ProductServices.Instance.GetProduct(model.ID);
                Product.ID = model.ID;
                Product.Name = model.Name;
                Product.SalesPrice = model.SalesPrice;
                Product.CostPrice = model.CostPrice;
                Product.EANCode = model.EANCode;
                Product.VAT = model.VAT;
                Product.ManageStockOrder = model.ManageStockOrder;
                Product.PartNumber = model.PartNumber;
                Product.MinStock = model.MinStock;
                Product.MaxStock = model.MaxStock;
                Product.CurrentStock = model.CurrentStock;
                Product.CategoryID = model.CategoryID;
                Product.SupplierID = model.SupplierID;
                ProductServices.Instance.UpdateProduct(Product);
            }
            else
            {
                var Product = new Product();
                var LoggedInUser = UserManager.FindById(User.Identity.GetUserId()); if (LoggedInUser == null) { return RedirectToAction("Login", "Account"); }
                if (LoggedInUser.Role != "Super Admin")
                {
                    Product.Business = LoggedInUser.Company;
                }
                Product.Name = model.Name;
                Product.SalesPrice = model.SalesPrice;
                Product.CostPrice = model.CostPrice;
                Product.EANCode = model.EANCode;
                Product.VAT = model.VAT;
                Product.PartNumber = model.PartNumber;
                Product.ManageStockOrder = model.ManageStockOrder;
                Product.MinStock = model.MinStock;
                Product.MaxStock = model.MaxStock;
                Product.CurrentStock = model.CurrentStock;
                Product.CategoryID = model.CategoryID;
                Product.SupplierID = model.SupplierID;

                ProductServices.Instance.SaveProduct(Product);
            }
            return Json(new { success = true }, JsonRequestBehavior.AllowGet);
        }



        [HttpGet]
        public ActionResult Delete(int ID)
        {
            ProductActionViewModel model = new ProductActionViewModel();
            var Product = ProductServices.Instance.GetProduct(ID);
            model.ID = Product.ID;
            return PartialView("_Delete", model);
        }


        [HttpPost]
        public ActionResult Delete(ProductActionViewModel model)
        {
            var Product = ProductServices.Instance.GetProduct(model.ID);

            ProductServices.Instance.DeleteProduct(Product.ID);

            return Json(new { success = true }, JsonRequestBehavior.AllowGet);
        }
    }
}
