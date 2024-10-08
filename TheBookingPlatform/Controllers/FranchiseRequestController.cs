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

namespace TheBookingPlatform.Controllers
{
    public class FranchiseRequestController : Controller
    {
        // GET: FranchiseRequest
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
        public FranchiseRequestController()
        {
        }
        public FranchiseRequestController(AMUserManager userManager, AMSignInManager signInManager)
        {
            UserManager = userManager;
            SignInManager = signInManager;
        }




        public void AcceptRequest(int ID)
        {
            var FranchiseRequest = FranchiseRequestServices.Instance.GetFranchiseRequest(ID);
            FranchiseRequest.Status = "Accepted";
            FranchiseRequest.Accepted = true;
            FranchiseRequestServices.Instance.UpdateFranchiseRequest(FranchiseRequest);
        }
        public void DeclineRequest(int ID)
        {
            var FranchiseRequest = FranchiseRequestServices.Instance.GetFranchiseRequest(ID);
            FranchiseRequest.Status = "Declined";
            FranchiseRequest.Accepted = false;
            FranchiseRequestServices.Instance.UpdateFranchiseRequest(FranchiseRequest);
        }


        // GET: FranchiseRequest
        public ActionResult Index()
        {
            FranchiseRequestListingViewModel model = new FranchiseRequestListingViewModel();
            var loggedInUser = UserManager.FindById(User.Identity.GetUserId());
            var company = CompanyServices.Instance.GetCompany().Where(x => x.Business == loggedInUser.Company).FirstOrDefault();
            model.LoggedInCompany = company.Business;
            var listofFranchiseRequestModel = new List<FranchiseRequestModel>();
            if (loggedInUser.Role != "Super Admin")
            {
                var FranchiseRequest = FranchiseRequestServices.Instance.GetFranchiseRequestByBusiness(company.ID);
                foreach (var item in FranchiseRequest)
                {

                    var companyFrom = CompanyServices.Instance.GetCompany(item.CompanyIDFrom);
                    var companyFor = CompanyServices.Instance.GetCompany(item.CompanyIDFor);
                    var user = UserManager.FindById(item.UserID);
                    listofFranchiseRequestModel.Add(new FranchiseRequestModel
                    {
                        FranchiseRequest = item,
                        CompanyFrom = companyFrom != null ? companyFrom.Business : "-",
                        CompanyFor = companyFor != null ? companyFor.Business : "-",
                        User = user != null ? user.Name : "-"
                    });
                }


            }
            else
            {
                var FranchiseRequest = FranchiseRequestServices.Instance.GetFranchiseRequest();
                foreach (var item in FranchiseRequest)
                {
                    var companyFrom = CompanyServices.Instance.GetCompany(item.CompanyIDFrom);
                    var user = UserManager.FindById(item.UserID);
                    listofFranchiseRequestModel.Add(new FranchiseRequestModel
                    {
                        FranchiseRequest = item,
                        CompanyFrom = companyFrom != null ? companyFrom.Business : "-",
                        User = user != null ? user.Name : "-"
                    });


                }
            }
            model.FranchiseRequests = listofFranchiseRequestModel;
            return View(model);
        }


        [HttpGet]
        public ActionResult Action(int ID = 0)
        {
            FranchiseRequestActionViewModel model = new FranchiseRequestActionViewModel();
            var loggedInUser = UserManager.FindById(User.Identity.GetUserId());
            var company = CompanyServices.Instance.GetCompany().Where(x => x.Business == loggedInUser.Company).FirstOrDefault();
            model.Users = UserManager.Users.Where(x => x.Company == company.Business).ToList();
            var listofUsers = new List<User>();

            if (ID != 0)
            {
                var FranchiseRequest = FranchiseRequestServices.Instance.GetFranchiseRequest(ID);
                model.ID = FranchiseRequest.ID;
                model.UserID = FranchiseRequest.UserID;
                model.Accepted = FranchiseRequest.Accepted;
                model.Status = FranchiseRequest.Status;
                model.CompanyIDFrom = FranchiseRequest.CompanyIDFrom;
                model.CompanyIDFor = FranchiseRequest.CompanyIDFor;
                model.MappedToUserID = FranchiseRequest.MappedToUserID;
            }
            return View(model);
        }

        [HttpPost]
        public JsonResult Action(string CompanyCode, string UserID, int ID,string MappedToUserID)
        {
            var loggedInUser = UserManager.FindById(User.Identity.GetUserId());
            var companyFor = CompanyServices.Instance.GetCompany().Where(x => x.Business == loggedInUser.Company).FirstOrDefault();
            var companyFrom = CompanyServices.Instance.GetCompany().Where(x => x.CompanyCode == CompanyCode).FirstOrDefault();

            if (ID == 0)
            {
                var FranchiseRequest = new FranchiseRequest();
                FranchiseRequest.Business = loggedInUser.Company;
                FranchiseRequest.CompanyIDFor = companyFor.ID;
                FranchiseRequest.CompanyIDFrom = companyFrom.ID;
                FranchiseRequest.Status = "Requested";
                FranchiseRequest.MappedToUserID = 
                FranchiseRequest.UserID = UserID;
                FranchiseRequest.MappedToUserID = MappedToUserID;
                FranchiseRequest.Accepted = false;
                FranchiseRequestServices.Instance.SaveFranchiseRequest(FranchiseRequest);

                var history = new History();
                history.Date = DateTime.Now;
                history.Business = loggedInUser.Company;
                history.EmployeeName = loggedInUser.Name;
                history.Note = "New User Requested, UserID: " + UserID + "";
                HistoryServices.Instance.SaveHistory(history);
                return Json(new { success = true }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                var FranchiseRequest = FranchiseRequestServices.Instance.GetFranchiseRequest(ID);
                FranchiseRequest.Business = loggedInUser.Company;
                FranchiseRequest.CompanyIDFor = companyFor.ID;
                FranchiseRequest.CompanyIDFrom = companyFrom.ID;
                FranchiseRequest.Status = "Requested";
                FranchiseRequest.UserID = UserID;
                FranchiseRequest.Accepted = false;
                FranchiseRequest.MappedToUserID = MappedToUserID;
                var history = new History();
                history.Date = DateTime.Now;
                history.Business = loggedInUser.Company;
                history.EmployeeName = loggedInUser.Name;
                history.Note = "User Request Updated, UserID: " + UserID + "";
                HistoryServices.Instance.SaveHistory(history);

                FranchiseRequestServices.Instance.UpdateFranchiseRequest(FranchiseRequest);
                return Json(new { success = true }, JsonRequestBehavior.AllowGet);
            }


        }


        [HttpGet]
        public ActionResult Delete(int ID)
        {
            FranchiseRequestActionViewModel model = new FranchiseRequestActionViewModel();
            var FranchiseRequest = FranchiseRequestServices.Instance.GetFranchiseRequest(ID);
            model.ID = FranchiseRequest.ID;
            return PartialView("_Delete", model);
        }


        [HttpPost]
        public ActionResult Delete(FranchiseRequestActionViewModel model)
        {
            var FranchiseRequest = FranchiseRequestServices.Instance.GetFranchiseRequest(model.ID);

            FranchiseRequestServices.Instance.DeleteFranchiseRequest(FranchiseRequest.ID);

            return Json(new { success = true }, JsonRequestBehavior.AllowGet);
        }
    }
}