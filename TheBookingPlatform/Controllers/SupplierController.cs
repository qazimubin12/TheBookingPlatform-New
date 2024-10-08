using Microsoft.AspNet.Identity.Owin;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TheBookingPlatform.Services;
using TheBookingPlatform.ViewModels;
using Microsoft.Owin.BuilderProperties;
using System.Xml.Linq;
using TheBookingPlatform.Entities;

namespace TheBookingPlatform.Controllers
{
    public class SupplierController : Controller
    {
        // GET: Supplier
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

        public SupplierController()
        {
        }

        public SupplierController(AMUserManager userManager, AMSignInManager signInManager)
        {
            UserManager = userManager;
            SignInManager = signInManager;
        }

        #endregion


        [NoCache]
        public ActionResult Index(string SearchTerm = "")
        {
            SupplierListingViewModel model = new SupplierListingViewModel();
                        var LoggedInUser = UserManager.FindById(User.Identity.GetUserId()); if(LoggedInUser == null) { return RedirectToAction("Login", "Account"); }
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
                var Suppliers =  SupplierServices.Instance.GetSupplier(SearchTerm).Where(x => x.Business == LoggedInUser.Company).ToList();
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
            model.Suppliers = ListOfSuppliers;
            return View(model);
        }




        public ActionResult ShowSuppliers(string SearchTerm = "")
        {
            SupplierListingViewModel model = new SupplierListingViewModel();
            var ListOfSuppliers = new List<SupplierModel>();
            var LoggedInUser = UserManager.FindById(User.Identity.GetUserId());
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
                var Suppliers = SupplierServices.Instance.GetSupplierWRTBusiness(LoggedInUser.Company, SearchTerm);
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
            model.Suppliers = ListOfSuppliers;
            return PartialView("Index", model);
        }


        [HttpGet]
        public ActionResult Action(int ID = 0)
        {
            SupplierActionViewModel model = new SupplierActionViewModel();
            if (ID != 0)
            {
                var Supplier = SupplierServices.Instance.GetSupplier(ID);
                model.ID = Supplier.ID;
                model.Name = Supplier.Name;
                model.Email = Supplier.Email;
                model.Address = Supplier.Address;
                model.PostalCode = Supplier.PostalCode;
                model.City = Supplier.City;
                model.TotalInventory = Supplier.TotalInventory;

            }
            return PartialView("_Action", model);
        }


        [HttpPost]
        public ActionResult Action(SupplierActionViewModel model)
        {
            if (model.ID != 0)
            {
                var Supplier = SupplierServices.Instance.GetSupplier(model.ID);
                Supplier.ID = model.ID;
                Supplier.Name = model.Name;
                Supplier.Email = model.Email;
                Supplier.Address = model.Address;
                Supplier.PostalCode = model.PostalCode;
                Supplier.City = model.City;
                Supplier.TotalInventory = model.TotalInventory;

                SupplierServices.Instance.UpdateSupplier(Supplier);
            }
            else
            {
                var supplier = new Supplier();
                            var LoggedInUser = UserManager.FindById(User.Identity.GetUserId()); if(LoggedInUser == null) { return RedirectToAction("Login", "Account"); }
                if (LoggedInUser.Role != "Super Admin")
                {
                    supplier.Business = LoggedInUser.Company;
                }
                supplier.Name = model.Name;
                supplier.Email = model.Email;
                supplier.Address = model.Address;
                supplier.PostalCode = model.PostalCode;
                supplier.City = model.City;
                supplier.TotalInventory = model.TotalInventory;
                SupplierServices.Instance.SaveSupplier(supplier);
            }
            return Json(new { success = true }, JsonRequestBehavior.AllowGet);
        }





        [HttpGet]
        public ActionResult Delete(int ID)
        {
            SupplierActionViewModel model = new SupplierActionViewModel();
            var Supplier = SupplierServices.Instance.GetSupplier(ID);
            model.ID = Supplier.ID;
            return PartialView("_Delete", model);
        }


        [HttpPost]
        public ActionResult Delete(SupplierActionViewModel model)
        {
            var message = SupplierServices.Instance.DeleteSupplier(model.ID);

            return Json(new { success = true,Message=message }, JsonRequestBehavior.AllowGet);
        }
    }
}