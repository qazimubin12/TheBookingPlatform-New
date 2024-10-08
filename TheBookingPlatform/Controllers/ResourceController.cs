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
    public class ResourceController : Controller
    {
        // GET: Resource
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

        public ResourceController()
        {
        }

        public ResourceController(AMUserManager userManager, AMSignInManager signInManager)
        {
            UserManager = userManager;
            SignInManager = signInManager;
        }

        #endregion
        // GET: Resource
        [NoCache]
        public ActionResult Index(string SearchTerm = "")
        {
            ResourceListingViewModel model = new ResourceListingViewModel();
            var LoggedInUser = UserManager.FindById(User.Identity.GetUserId()); if (LoggedInUser == null) { return RedirectToAction("Login", "Account"); }
            if (LoggedInUser.Role == "Super Admin")
            {
                model.Resources = ResourceServices.Instance.GetResource(SearchTerm);
            }
            else
            {
                model.Resources = ResourceServices.Instance.GetResource(SearchTerm).Where(x=>x.Business == LoggedInUser.Company).ToList();

            }
            return View(model);
        }


        [NoCache]
        [HttpGet]
        public ActionResult Action(int ID = 0)
        {
            ResourceActionViewModel model = new ResourceActionViewModel();
            var LoggedInUser = UserManager.FindById(User.Identity.GetUserId()); if (LoggedInUser == null) { return RedirectToAction("Login", "Account"); }
          
            model.ServicesList = ServiceServices.Instance.GetService(LoggedInUser.Company,"");
            if (ID != 0)
            {
                var Resource = ResourceServices.Instance.GetResource(ID);
                model.ID = Resource.ID;
                model.Business = Resource.Business;
                model.Name = Resource.Name;
                model.Availability = Resource.Availability;
                model.Type = Resource.Type;
                model.ServicesINTS = Resource.Services.Split(',').Select(x => int.Parse(x)).ToList();
            }
            return View("Action", model);
        }


        [HttpPost]
        public ActionResult Action(ResourceActionViewModel model)
        {
            if (model.ID != 0)
            {
                var Resource = ResourceServices.Instance.GetResource(model.ID);
                Resource.ID = model.ID;
                Resource.Name = model.Name;
                Resource.Availability = model.Availability;
                Resource.Type = model.Type;
                var service = String.Join(",", model.ServicesINTS);
                Resource.Services = service;
                ResourceServices.Instance.UpdateResource(Resource);
            }
            else
            {
                var resource = new Resource();
                            var LoggedInUser = UserManager.FindById(User.Identity.GetUserId()); if(LoggedInUser == null) { return RedirectToAction("Login", "Account"); }
                if (LoggedInUser.Role != "Super Admin")
                {
                    resource.Business = LoggedInUser.Company;
                }
                resource.Name = model.Name;
                resource.Availability = model.Availability;
                resource.Type = model.Type;
                var service = String.Join(",", model.ServicesINTS);
                resource.Services = service;
                ResourceServices.Instance.SaveResource(resource);
            }
            return Json(new { success = true }, JsonRequestBehavior.AllowGet);
        }



        [HttpGet]
        public ActionResult Delete(int ID)
        {
            ResourceActionViewModel model = new ResourceActionViewModel();
            var Resource = ResourceServices.Instance.GetResource(ID);
            model.ID = Resource.ID;
            return PartialView("_Delete", model);
        }


        [HttpPost]
        public ActionResult Delete(ResourceActionViewModel model)
        {
            var Resource = ResourceServices.Instance.GetResource(model.ID);
            ResourceServices.Instance.DeleteResource(Resource.ID);

            return Json(new { success = true }, JsonRequestBehavior.AllowGet);
        }
    }
}