using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
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
    public class SaleController : Controller
    {
        // GET: Sale
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
        public SaleController()
        {
        }



        public SaleController(AMUserManager userManager, AMSignInManager signInManager)
        {
            UserManager = userManager;
            SignInManager = signInManager;
        }

        public ActionResult CreateSale(int SaleID = 0,int AppointmentID = 0)
        {
            SaleActionViewModel model = new SaleActionViewModel();
            var LoggedInUser = UserManager.FindById(User.Identity.GetUserId());
            var sale = SaleServices.Instance.GetSale(SaleID);
            model.Company = CompanyServices.Instance.GetCompany(LoggedInUser.Company).FirstOrDefault();
            model.Products = ProductServices.Instance.GetProductWRTBusiness(LoggedInUser.Company);
            model.Sale = sale;
            if(AppointmentID == 0)
            {
                model.Type = "Via Sale";
                model.Customers = CustomerServices.Instance.GetCustomersWRTBusiness(LoggedInUser.Company,"");
            }
            else
            {
                model.Type = "Via Appointment";
                model.Appointment = AppointmentServices.Instance.GetAppointment(AppointmentID);
                model.Customer = CustomerServices.Instance.GetCustomer(model.Appointment.CustomerID);
            }
            if (model.Sale != null)
            {
                model.ID = SaleID;
                var saleproductModel = new List<SaleProductModel>();
                var saleProducts = SaleProductServices.Instance.GetSaleProductWRTBusiness(LoggedInUser.Company, model.Sale.ID);
                foreach (var product in saleProducts)
                {
                    var item = ProductServices.Instance.GetProduct(product.ProductID);
                    if (item != null)
                    {
                        saleproductModel.Add(new SaleProductModel { Product = item, Qty = product.Qty, Total = product.Total });
                    }
                }
                model.SaleProducts = saleproductModel;

            }
            return View(model);
        }


        [HttpPost]
        public JsonResult SaveSale(SaleRequest saleRequest)
        {
            var LoggedInUser = UserManager.FindById(User.Identity.GetUserId());

            if (saleRequest.ID != 0)
            {
                var sale = SaleServices.Instance.GetSale(saleRequest.ID);
                if(sale != null)
                {
                    SaleServices.Instance.DeleteSale(saleRequest.ID);
                }
                if (saleRequest.SaleProducts != null)
                {
                    sale = new Sale();
                    if (saleRequest.AppointmentID != 0)
                    {
                        sale.Type = "Via Appointment";
                    }
                    else
                    {
                        sale.Type = "Via Sale";
                    }
                    sale.Remarks = saleRequest.Remarks;
                    sale.CustomerID = saleRequest.CustomerID;
                    sale.AppointmentID = saleRequest.AppointmentID;
                    sale.Business = LoggedInUser.Company;
                    sale.Date = DateTime.Now;
                    SaleServices.Instance.SaveSale(sale);

                    foreach (var item in saleRequest.SaleProducts)
                    {
                        var saleProduct = new SaleProduct();
                        saleProduct.ProductID = item.ProductID;
                        saleProduct.Qty = item.Qty;
                        saleProduct.Total = item.Total;
                        saleProduct.Business = LoggedInUser.Company;
                        saleProduct.ReferenceID = sale.ID;
                        SaleProductServices.Instance.SaveSaleProduct(saleProduct);

                    }
                }
            }
            else
            {
                var sale = new Sale();
                if(saleRequest.AppointmentID != 0)
                {
                    sale.Type = "Via Appointment";
                }
                else
                {
                    sale.Type = "Via Sale";
                }
                sale.Remarks = saleRequest.Remarks;
                sale.CustomerID = saleRequest.CustomerID;
                sale.AppointmentID = saleRequest.AppointmentID;
                sale.Business = LoggedInUser.Company;
                sale.Date = DateTime.Now;   
                SaleServices.Instance.SaveSale(sale);

                foreach (var item in saleRequest.SaleProducts)
                {
                    var saleProduct = new SaleProduct();
                    saleProduct.ProductID = item.ProductID;
                    saleProduct.Qty = item.Qty;
                    saleProduct.Total = item.Total;
                    saleProduct.Business = LoggedInUser.Company;
                    saleProduct.ReferenceID = sale.ID;
                    SaleProductServices.Instance.SaveSaleProduct(saleProduct);  

                }
            }
            return Json(new { success = true }, JsonRequestBehavior.AllowGet);
        }
    }
}