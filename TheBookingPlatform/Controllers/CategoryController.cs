using Microsoft.AspNet.Identity.Owin;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TheBookingPlatform.Services;
using TheBookingPlatform.ViewModels;
using TheBookingPlatform.Entities;

namespace TheBookingPlatform.Controllers
{
    public class CategoryController : Controller
    {
        // GET: Category
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

        public CategoryController()
        {
        }

        public CategoryController(AMUserManager userManager, AMSignInManager signInManager)
        {
            UserManager = userManager;
            SignInManager = signInManager;
        }

        #endregion

        [NoCache]
        public ActionResult Index(string SearchTerm = "")
        {
            CategoriesListingViewModel model = new CategoriesListingViewModel();
            var LoggedInUser = UserManager.FindById(User.Identity.GetUserId()); if (LoggedInUser == null) { return RedirectToAction("Login", "Account"); }
            if (LoggedInUser.Role == "Super Admin")
            {
                model.Categories = CategoryServices.Instance.GetCategory(SearchTerm);
            }
            else
            {
                model.Categories = CategoryServices.Instance.GetCategoryWRTBusiness(LoggedInUser.Company, SearchTerm);

            }
            return View(model);
        }


        public ActionResult ShowCategories(string SearchTerm = "")
        {
            CategoriesListingViewModel model = new CategoriesListingViewModel();
            var LoggedInUser = UserManager.FindById(User.Identity.GetUserId()); if (LoggedInUser == null) { return RedirectToAction("Login", "Account"); }
            if (LoggedInUser.Role == "Super Admin")
            {
                model.Categories = CategoryServices.Instance.GetCategory(SearchTerm);
            }
            else
            {
                model.Categories = CategoryServices.Instance.GetCategoryWRTBusiness(LoggedInUser.Company, SearchTerm);

            }
            return PartialView("Index", model);
        }



        [HttpGet]
        [NoCache]
        public ActionResult Action(int ID = 0)
        {
            CategoriesActionViewModel model = new CategoriesActionViewModel();
            if (ID != 0)
            {
                var Category = CategoryServices.Instance.GetCategory(ID);
                model.ID = Category.ID;
                model.Name = Category.Name;
            }
            return PartialView("_Action", model);
        }


        [HttpPost]
        public ActionResult Action(CategoriesActionViewModel model)
        {
            if (model.ID != 0)
            {
                var Category = CategoryServices.Instance.GetCategory(model.ID);
                Category.ID = model.ID;
                Category.Name = model.Name;
             

                CategoryServices.Instance.UpdateCategory(Category);
            }
            else
            {
                var category = new Category();
                            var LoggedInUser = UserManager.FindById(User.Identity.GetUserId()); if(LoggedInUser == null) { return RedirectToAction("Login", "Account"); }
                if (LoggedInUser.Role != "Super Admin")
                {
                    category.Business = LoggedInUser.Company;
                }
                category.Name = model.Name;
              
                CategoryServices.Instance.SaveCategory(category);
            }
            return Json(new { success = true }, JsonRequestBehavior.AllowGet);
        }





        [HttpGet]
        [NoCache]
        public ActionResult Delete(int ID)
        {
            CategoriesActionViewModel model = new CategoriesActionViewModel();
            var Category = CategoryServices.Instance.GetCategory(ID);
            model.ID = Category.ID;
            return PartialView("_Delete", model);
        }


        [HttpPost]
        public ActionResult Delete(CategoriesActionViewModel model)
        {
            var message = CategoryServices.Instance.DeleteCategory(model.ID);
            return Json(new { success = true,Message= message }, JsonRequestBehavior.AllowGet);
        }
    }
}