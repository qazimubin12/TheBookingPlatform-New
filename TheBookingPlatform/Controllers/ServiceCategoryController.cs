using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Services.Description;
using TheBookingPlatform.Entities;
using TheBookingPlatform.Services;
using TheBookingPlatform.ViewModels;

namespace TheBookingPlatform.Controllers
{
    public class ServiceCategoryController : Controller
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

        public ServiceCategoryController()
        {
        }

        public ServiceCategoryController(AMUserManager userManager, AMSignInManager signInManager)
        {
            UserManager = userManager;
            SignInManager = signInManager;
        }

        #endregion
        
        [NoCache]
        public ActionResult Index(string SearchTerm = "")
        {
            ServiceCategoriesListingViewModel model = new ServiceCategoriesListingViewModel();
                        var LoggedInUser = UserManager.FindById(User.Identity.GetUserId()); if(LoggedInUser == null) { return RedirectToAction("Login", "Account"); }
            if (LoggedInUser.Role == "Super Admin")
            {
                model.ServiceCategories = ServicesCategoriesServices.Instance.GetServiceCategories(SearchTerm);
            }
            else
            {
                model.ServiceCategories = ServicesCategoriesServices.Instance.GetServiceCategories(SearchTerm).Where(x=>x.Business == LoggedInUser.Company).ToList();

            }
            return View(model);
        }



        [HttpGet]
        [NoCache]
        public ActionResult Action(int ID = 0)
        {
            ServiceCategoriesActionViewModel model = new ServiceCategoriesActionViewModel();
            if (ID != 0)
            {
                var ServiceCategories = ServicesCategoriesServices.Instance.GetServiceCategory(ID);
                model.ID = ServiceCategories.ID;
                model.Business = ServiceCategories.Business;
                model.Name = ServiceCategories.Name;
                model.Type = ServiceCategories.Type;
          
            }
            return PartialView("_Action", model);
        }



        [HttpPost]
        public ActionResult UpdateDisplayOrder(int categoryId, int newOrder)
        {
            var ServiceCategory = ServicesCategoriesServices.Instance.GetServiceCategory(categoryId);
            ServiceCategory.DisplayOrder = newOrder;
            ServicesCategoriesServices.Instance.UpdateServiceCategory(ServiceCategory);

            return Json(new { success = true });
        }


        [HttpPost]
        public ActionResult Action(ServiceCategoriesActionViewModel model)
        {
            if (model.ID != 0)
            {
                var ServiceCategories = ServicesCategoriesServices.Instance.GetServiceCategory(model.ID);
                ServiceCategories.ID = model.ID;
                var oldName = ServiceCategories.Name;
                ServiceCategories.Name = model.Name;
                ServiceCategories.Type = model.Type;
                ServicesCategoriesServices.Instance.UpdateServiceCategory(ServiceCategories);

                var serviceList = ServiceServices.Instance.GetService().Where(x=> x.IsActive && x.Business ==  ServiceCategories.Business && x.Category == oldName).ToList();
                foreach (var item in serviceList)
                {
                    item.Category = model.Name;
                    ServiceServices.Instance.UpdateService(item);
                }


                
            }
            else
            {
                var serviceCategories = new ServiceCategory();
                            var LoggedInUser = UserManager.FindById(User.Identity.GetUserId()); if(LoggedInUser == null) { return RedirectToAction("Login", "Account"); }
                if (LoggedInUser.Role != "Super Admin")
                {
                    serviceCategories.Business = LoggedInUser.Company;
                }
                serviceCategories.Name = model.Name;
               serviceCategories.Type = model.Type;
                ServicesCategoriesServices.Instance.SaveServiceCategory(serviceCategories);
            }
            return Json(new { success = true }, JsonRequestBehavior.AllowGet);
        }



        [HttpGet]
        public ActionResult Delete(int ID)
        {
            ServiceCategoriesActionViewModel model = new ServiceCategoriesActionViewModel();
            var ServiceCategories = ServicesCategoriesServices.Instance.GetServiceCategory(ID);
            model.ID = ServiceCategories.ID;
            model.Name = ServiceCategories.Name;
            return PartialView("_Delete", model);
        }


        [HttpPost]
        public JsonResult Delete(ServiceCategoriesActionViewModel model)
        {
            var LoggedInUser = UserManager.FindById(User.Identity.GetUserId());
            var message = ServicesCategoriesServices.Instance.DeleteServiceCategory(model.ID,model.Name,LoggedInUser.Company);
            return Json(new { Message = message }, JsonRequestBehavior.AllowGet);
        }
    }
}