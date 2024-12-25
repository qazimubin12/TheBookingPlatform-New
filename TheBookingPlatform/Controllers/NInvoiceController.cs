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
using TheBookingPlatformV3.Services;

namespace TheBookingPlatform.Controllers
{
    public class NInvoiceController : Controller
    {
        // GET: NInvoice

        #region Usermanageregion
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
        public NInvoiceController()
        {
        }



        public NInvoiceController(AMUserManager userManager, AMSignInManager signInManager)
        {
            UserManager = userManager;
            SignInManager = signInManager;
        }

        #endregion





        public ActionResult Index()
        {
            NInvoiceListingViewModel model = new NInvoiceListingViewModel();
            var listofNInvocies = new List<NInvoiceModel>();
            var ninvoices = new List<NInvoice>();
            var loggedInUser = UserManager.FindById(User.Identity.GetUserId());

            if (User.IsInRole("Super Admin"))
            {
                ninvoices = NInvoiceServices.Instance.GetNInvoice();
            }
            else
            {
                ninvoices = NInvoiceServices.Instance.GetNInvoiceWRTBusiness(loggedInUser.Company);
            }
            foreach (var item in ninvoices)
            {
                var vat = VatServices.Instance.GetVat(item.VAT);
                listofNInvocies.Add(new NInvoiceModel { NInvoice = item, Vat = vat });
            }
            model.NInvoices = listofNInvocies;
            return View(model);
        }

        public static string GenerateCode()
        {
            string prefix = "INV";
            string currentYear = DateTime.Now.Year.ToString();

            // Calculate the number of random digits needed
            int numRandomDigits = 12 - prefix.Length - currentYear.Length;

            if (numRandomDigits <= 0)
            {
                throw new Exception("The code cannot be 8 digits long with the given prefix and year.");
            }

            Random random = new Random();
            string randomDigits = random.Next(0, (int)Math.Pow(10, numRandomDigits))
                                        .ToString($"D{numRandomDigits}");

            return $"{prefix}{randomDigits}{currentYear}";
        }
        [HttpGet]
        public ActionResult Action(int ID = 0)
        {
            NInvoiceActionViewModel model = new NInvoiceActionViewModel();
            var vats = new List<Vat>();
            if (User.IsInRole("Super Admin"))
            {
                vats = VatServices.Instance.GetVat();
            }
            else
            {
                var loggedInUser = UserManager.FindById(User.Identity.GetUserId());
                vats = VatServices.Instance.GetVatWRTBusiness(loggedInUser.Company);
            }
            model.Vats = vats;
            if (ID != 0)
            {
                var ninvoice = NInvoiceServices.Instance.GetNInvoice(ID);
                model.ID = ninvoice.ID;
                model.IssueDate = ninvoice.IssueDate;
                model.VAT = ninvoice.VAT;
                model.InvoiceNo = ninvoice.InvoiceNo;
                model.DueDate = ninvoice.DueDate;
                model.PaymentMethod = ninvoice.PaymentMethod;
                model.CompanyLogo = ninvoice.CompanyLogo;
                model.ItemDetails = ninvoice.ItemDetails;
                model.Remarks = ninvoice.Remarks;
                model.CustomerName = ninvoice.CustomerName;
                model.CustomerEmail = ninvoice.CustomerEmail;
                model.CustomerPhone = ninvoice.CustomerPhone;
                model.CustomerAddress = ninvoice.CustomerAddress;
                model.CompanyName = ninvoice.CompanyName;
                model.CompanyEmail = ninvoice.CompanyEmail;
                model.CompanyPhone = ninvoice.CompanyPhone;
                model.CompanyAddress = ninvoice.CompanyAddress;


                if (ninvoice.ItemDetails != null)
                {
                    var NInvoiceItemModelList = new List<NInvoiceItemModel>();
                    if (ninvoice.ItemDetails != null)
                    {
                        var items = ninvoice.ItemDetails.Split('~').ToList();
                        foreach (var item in items)
                        {
                            var DataofItem = item.Split('_');
                            var Service = DataofItem[0];
                            var Duration = DataofItem[1];
                            var price = DataofItem[2];
                            var Amount = DataofItem[3];
                            NInvoiceItemModelList.Add(new NInvoiceItemModel { Service = Service, Duration = Duration, Price = price, Amount = Amount });
                        }
                        model.Items = NInvoiceItemModelList;
                    }
                }


            }
            else
            {
                model.InvoiceNo = GenerateCode();
            }
            return View(model);
        }

        [HttpGet]
        public ActionResult View(int ID)
        {
            NInvoiceActionViewModel model = new NInvoiceActionViewModel();

            var ninvoice = NInvoiceServices.Instance.GetNInvoice(ID);
            model.ID = ninvoice.ID;
            model.IssueDate = ninvoice.IssueDate;
            model.VAT = ninvoice.VAT;
            model.InvoiceNo = ninvoice.InvoiceNo;
            model.DueDate = ninvoice.DueDate;
            model.CompanyLogo = ninvoice.CompanyLogo;
            model.PaymentMethod = ninvoice.PaymentMethod;
            model.ItemDetails = ninvoice.ItemDetails;
            model.Remarks = ninvoice.Remarks;
            model.CustomerName = ninvoice.CustomerName;
            model.CustomerEmail = ninvoice.CustomerEmail;
            model.CustomerPhone = ninvoice.CustomerPhone;
            model.CustomerAddress = ninvoice.CustomerAddress;
            model.CompanyName = ninvoice.CompanyName;
            model.CompanyEmail = ninvoice.CompanyEmail;
            model.CompanyPhone = ninvoice.CompanyPhone;
            model.CompanyAddress = ninvoice.CompanyAddress;
            var company = CompanyServices.Instance.GetCompany(ninvoice.Business).FirstOrDefault();
            model.Currency = company.Currency;
            model.VATFULL = VatServices.Instance.GetVat(ninvoice.VAT);
            var NInvoiceItemModelList = new List<NInvoiceItemModel>();
            if (ninvoice.ItemDetails != null)
            {
                var items = ninvoice.ItemDetails.Split('~').ToList();
                foreach (var item in items)
                {
                    var DataofItem = item.Split('_');
                    var Service = DataofItem[0];
                    var Duration = DataofItem[1];
                    var price = DataofItem[2];
                    var Amount = DataofItem[3];
                    NInvoiceItemModelList.Add(new NInvoiceItemModel { Service = Service, Duration = Duration, Price = price, Amount = Amount });
                }
                model.Items = NInvoiceItemModelList;
            }
            return View(model);
        }



        [HttpPost]
        public JsonResult Action(NInvoiceActionViewModel model)
        {
            var LoggedInUser = UserManager.FindById(User.Identity.GetUserId());
            if (model.ID != 0)
            {
                var ninvoice = NInvoiceServices.Instance.GetNInvoice(model.ID);
                ninvoice.ID = model.ID;
                ninvoice.IssueDate = model.IssueDate;
                ninvoice.VAT = model.VAT;
                ninvoice.InvoiceNo = model.InvoiceNo;
                ninvoice.DueDate = model.DueDate;
                ninvoice.PaymentMethod = model.PaymentMethod;
                ninvoice.ItemDetails = model.ItemDetails;
                ninvoice.Remarks = model.Remarks;
                ninvoice.CustomerName = model.CustomerName;
                ninvoice.CustomerEmail = model.CustomerEmail;
                ninvoice.CompanyLogo = model.CompanyLogo;
                ninvoice.CustomerPhone = model.CustomerPhone;
                ninvoice.CustomerAddress = model.CustomerAddress;
                ninvoice.CompanyName = model.CompanyName;
                ninvoice.CompanyEmail = model.CompanyEmail;
                ninvoice.CompanyPhone = model.CompanyPhone;
                ninvoice.CompanyAddress = model.CompanyAddress;
                ninvoice.Business = LoggedInUser.Company;


                NInvoiceServices.Instance.UpdateNInvoice(ninvoice);
                return Json(new { success = true }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                var ninvoice = new NInvoice();
                ninvoice.IssueDate = model.IssueDate;
                ninvoice.VAT = model.VAT;
                ninvoice.InvoiceNo = model.InvoiceNo;
                ninvoice.DueDate = model.DueDate;
                ninvoice.PaymentMethod = model.PaymentMethod;
                ninvoice.ItemDetails = model.ItemDetails;
                ninvoice.Remarks = model.Remarks;
                ninvoice.CustomerName = model.CustomerName;
                ninvoice.CustomerEmail = model.CustomerEmail;
                ninvoice.CompanyLogo = model.CompanyLogo;
                ninvoice.CustomerPhone = model.CustomerPhone;
                ninvoice.CustomerAddress = model.CustomerAddress;
                ninvoice.CompanyName = model.CompanyName;
                ninvoice.CompanyEmail = model.CompanyEmail;
                ninvoice.CompanyPhone = model.CompanyPhone;
                ninvoice.CompanyAddress = model.CompanyAddress;
                ninvoice.Business = LoggedInUser.Company;

                NInvoiceServices.Instance.SaveNInvoice(ninvoice);
            }
            return Json(new { success = true }, JsonRequestBehavior.AllowGet);
        }



        [HttpGet]
        public ActionResult Delete(int ID)
        {
            NInvoiceActionViewModel model = new NInvoiceActionViewModel();
            var NInvoice = NInvoiceServices.Instance.GetNInvoice(ID);
            model.ID = NInvoice.ID;
            model.InvoiceNo = NInvoice.InvoiceNo;
            return PartialView("_Delete", model);
        }


        [HttpPost]
        public JsonResult Delete(NInvoiceActionViewModel model)
        {
            var NInvoice = NInvoiceServices.Instance.GetNInvoice(model.ID);
            if (NInvoice != null)
            {
                NInvoiceServices.Instance.DeleteNInvoice(NInvoice.ID);
                return Json(new { success = true, Message = "Deleted Successfully" }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(new { success = false, Message = "No NInvoice Found" }, JsonRequestBehavior.AllowGet);
            }
        }
    }
}