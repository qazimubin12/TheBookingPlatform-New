using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;
using TheBookingPlatform.Models;
using TheBookingPlatform.Services;
using TheBookingPlatform.ViewModels;
using TheBookingPlatform.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace TheBookingPlatform.Controllers
{
    public class RoleController : Controller
    {
        // GET: Role
        private AMSignInManager _signInManager;
        private AMUserManager _userManager;
        private AMRolesManager _rolesManager;
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

        public RoleController()
        {
        }
        public RoleController(AMUserManager userManager, AMSignInManager signInManager,AMRolesManager roleManager)
        {
            UserManager = userManager;
            SignInManager = signInManager;
            RolesManager = roleManager;
        }



        // GET: User
        //[Authorize(Roles = "Admin")]

        public ActionResult Index(string searchterm)
        {
            RoleListingViewModel model = new RoleListingViewModel();
            model.Roles = SearchRoles(searchterm);
            return View(model);
        }

        public IEnumerable<IdentityRole> SearchRoles(string searchTerm)
        {

            var roles = RolesManager.Roles.AsQueryable();
            if (!string.IsNullOrEmpty(searchTerm))
            {
                roles = roles.Where(a => a.Name.ToLower().Contains(searchTerm.ToLower()));
            }

      
            return roles;
        }

        public ActionResult Register(RegisterViewModel model)
        {
            return View(model);
        }

        [HttpGet]
        public async Task<ActionResult> Action(string ID)
        {
            RoleActionViewModel model = new RoleActionViewModel();
            if (!string.IsNullOrEmpty(ID))
            {
                var role = await RolesManager.FindByIdAsync(ID);
                model.ID = role.Id;
                model.Name = role.Name;
              
            }
            return PartialView("_Action", model);
        }


        [HttpPost]
        public async Task<JsonResult> Action(RoleActionViewModel model)
        {
            JsonResult json = new JsonResult();
            IdentityResult result = null;
            if (!string.IsNullOrEmpty(model.ID)) //update record
            {
                var role = await RolesManager.FindByIdAsync(model.ID);

                role.Id = model.ID;
                role.Name = model.Name;
               
                result = await RolesManager.UpdateAsync(role);

            }
            else
            {
                var role = new IdentityRole();
                role.Name = model.Name;
                result = await RolesManager.CreateAsync(role);

            }

            json.Data = new { Success = result.Succeeded, Message = string.Join(", ", result.Errors) };

            return json;
        }

        [HttpGet]
        public async Task<ActionResult> Delete(string ID)
        {
            RoleActionViewModel model = new RoleActionViewModel();
            var role = await RolesManager.FindByIdAsync(ID);
            model.ID = role.Id;
           return PartialView("_Delete", model);
        }

        [HttpPost]
        public async Task<JsonResult> Delete(RoleActionViewModel model)
        {
            JsonResult json = new JsonResult();

            IdentityResult result = null;

            if (!string.IsNullOrEmpty(model.ID)) //we are trying to delete a record
            {
                var role = await RolesManager.FindByIdAsync(model.ID);

                result = await RolesManager.DeleteAsync(role);

                json.Data = new { Success = result.Succeeded, Message = string.Join(", ", result.Errors) };
            }
            else
            {
                json.Data = new { Success = false, Message = "Invalid Role." };
            }

            return json;
        }
    }
}