using System;
using System.Globalization;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using TheBookingPlatform.Models;
using TheBookingPlatform.Services;
using TheBookingPlatform.ViewModels;
using TheBookingPlatform.Entities;

namespace TheBookingPlatform.Controllers
{
    [Authorize]
    public class AccountController : Controller
    {
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
        public AccountController()
        {
        }



        public AccountController(AMUserManager userManager, AMSignInManager signInManager)
        {
            UserManager = userManager;
            SignInManager = signInManager;
        }









        [HttpGet]
        public ActionResult NotAllowed()
        {
            HomeViewModel model = new HomeViewModel();
            model.SignedInUser = UserManager.FindById(User.Identity.GetUserId());
            return View(model);
        }

        //
        // GET: /Account/Login
        [AllowAnonymous]
        public ActionResult Login(string returnUrl)
        {
            if (returnUrl != null)
            {
                RedirectToLocal(returnUrl);
            }
            var userLoggedIn = UserManager.FindById(User.Identity.GetUserId());
            if (userLoggedIn != null)
            {
                if (userLoggedIn.Role != "Super Admin")
                {
                    if (userLoggedIn.Company != null)
                    {
                        return RedirectToAction("Dashboard", "Admin");
                    }
                    else
                    {
                        Session["RegisteredEmail"] = userLoggedIn.Email;
                        Session["RegisteredPAKKITA"] = userLoggedIn.Password;
                        return RedirectToAction("RegisterCompany", "User");
                    }
                }
                else
                {
                    return RedirectToAction("Dashboard", "User");

                }
            }
            else
            {
                ViewBag.ReturnUrl = returnUrl;
                return View();
            }
        }

        //
        // POST: /Account/Login[
        [AllowAnonymous]
        public async Task<ActionResult> AutoLogin(string Email, string PasswordKAK, string returnUrl = "")
        {
            var model = new LoginViewModel();
            model.Email = Email;
            model.RememberMe = false;

            // This doesn't count login failures towards account lockout
            // To enable password failures to trigger account lockout, change to shouldLockout: true
            var user = await UserManager.FindByEmailAsync(model.Email);
            if (user != null && user.Role != "Super Admin")
            {
                if (user.IsActive == false)
                {
                    return RedirectToAction("Login", "Account");
                }
                if (user.IsInTrialPeriod && user.IsPaid == false)
                {
                    if ((DateTime.Now - user.RegisteredDate).Days >= 30)
                    {
                        return RedirectToAction("Pay", "User", new { UserID = user.Id });
                    }
                }
                else if (user.IsPaid)
                {
                    if (user.LastPaymentDate != null)
                    {
                        var remainderDays = (DateTime.Parse(user.LastPaymentDate).AddMonths(1).Date - DateTime.Now.Date).Days;

                        if (remainderDays < 1)
                        {
                            return RedirectToAction("Pay", "User", new { UserID = user.Id });
                        }
                    }
                    else
                    {
                        return RedirectToAction("Pay", "User", new { UserID = user.Id });
                    }
                }


                if (user != null)
                {
                    Session["User"] = user.Name;
                }
            }
            if (user != null)
            {
                var result = await SignInManager.PasswordSignInAsync(user.UserName, user.Password, model.RememberMe, shouldLockout: false);
                switch (result)
                {
                    case SignInStatus.Success:
                        Session["RegisteredEmail"] = user.Email;
                        Session["RegisteredPAKKITA"] = user.Package;
                        var package = PackageServices.Instance.GetPackage(user.Package);
                        if (package != null)
                        {
                            var claims = await UserManager.GetClaimsAsync(user.Id);
                            foreach (var item in claims)
                            {
                                var resultnew = await UserManager.RemoveClaimAsync(user.Id, item);
                            }
                            claims = await UserManager.GetClaimsAsync(user.Id);
                            if (!claims.Any(c => c.Type == "Package" && c.Value != ""))
                            {
                                await UserManager.AddClaimAsync(user.Id, new Claim("Package", package.Features ?? ""));
                            }
                            if (!claims.Any(c => c.Type == "InTrial" && c.Value != ""))
                            {
                                if (user.IsInTrialPeriod)
                                {
                                    await UserManager.AddClaimAsync(user.Id, new Claim("InTrial", "Yes"));

                                }
                                else
                                {
                                    await UserManager.AddClaimAsync(user.Id, new Claim("InTrial", "No"));

                                }
                            }
                        }
                        else
                        {
                            var claims = await UserManager.GetClaimsAsync(user.Id);
                            if (!claims.Any(c => c.Type == "InTrial" && c.Value != ""))
                            {
                                if (user.IsInTrialPeriod)
                                {
                                    await UserManager.AddClaimAsync(user.Id, new Claim("InTrial", "Yes"));

                                }
                                else
                                {
                                    await UserManager.AddClaimAsync(user.Id, new Claim("InTrial", "No"));

                                }
                            }
                            claims = await UserManager.GetClaimsAsync(user.Id);
                        }
                        Session["ID"] = user.Id;
                        var log = new History();
                        log.Business = user.Company;
                        var employee = EmployeeServices.Instance.GetEmployee().Where(x => x.LinkedEmployee == user.Id).FirstOrDefault();
                        if (employee != null)
                        {
                            log.EmployeeName = employee.Name;
                            log.Date = DateTime.Now;
                            log.Note = log.EmployeeName + " Logged In";
                            HistoryServices.Instance.SaveHistory(log);
                        }

                        return RedirectToLocal(returnUrl);
                    case SignInStatus.LockedOut:
                        return View("Lockout");
                    case SignInStatus.RequiresVerification:
                        return RedirectToAction("SendCode", new { ReturnUrl = returnUrl, RememberMe = model.RememberMe });
                    case SignInStatus.Failure:
                    default:
                        ModelState.AddModelError("", "Invalid login attempt.");
                        return View(model);
                }
            }
            else
            {
                ModelState.AddModelError("", "Invalid login attempt.");
                return View(model);
            }
        }



        [HttpPost]
        [AllowAnonymous]
        //[ValidateAntiForgeryToken]
        public async Task<ActionResult> Login(LoginViewModel model, string returnUrl = "")
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // This doesn't count login failures towards account lockout
            // To enable password failures to trigger account lockout, change to shouldLockout: true
            var user = await UserManager.FindByEmailAsync(model.Email);
            if (user != null && user.Role != "Super Admin")
            {
                if (user.IsActive == false)
                {
                    return RedirectToAction("Login", "Account");
                }
                if (user.IsInTrialPeriod && user.IsPaid == false)
                {
                    if ((DateTime.Now - user.RegisteredDate).Days >= 30)
                    {
                        return RedirectToAction("Pay", "User", new { UserID = user.Id });
                    }
                }
                else if (user.IsPaid)
                {
                    if (user.LastPaymentDate != null)
                    {
                        var remainderDays = (DateTime.Parse(user.LastPaymentDate).AddMonths(1).Date - DateTime.Now.Date).Days;

                        if (remainderDays < 1)
                        {
                            return RedirectToAction("Pay", "User", new { UserID = user.Id });
                        }
                    }
                    else
                    {
                        return RedirectToAction("Pay", "User", new { UserID = user.Id });
                    }
                }
              
                
               
                if (user != null)
                {
                    Session["User"] = user.Name;
                }
            }
            if (user != null)
            {
                var result = await SignInManager.PasswordSignInAsync(user.UserName, model.Password, model.RememberMe, shouldLockout: false);
                switch (result)
                {
                    case SignInStatus.Success:
                        Session["RegisteredEmail"] = user.Email;
                        Session["RegisteredPAKKITA"] = user.Package;
                        var package = PackageServices.Instance.GetPackage(user.Package);
                        if (package != null)
                        {
                            var claims = await UserManager.GetClaimsAsync(user.Id);
                            foreach (var item in claims)
                            {
                                var resultnew = await UserManager.RemoveClaimAsync(user.Id, item);
                            }
                            claims = await UserManager.GetClaimsAsync(user.Id);
                            if (!claims.Any(c => c.Type == "Package" && c.Value != ""))
                            {
                                await UserManager.AddClaimAsync(user.Id, new Claim("Package", package.Features ?? ""));
                            }
                            if (!claims.Any(c => c.Type == "InTrial" && c.Value != ""))
                            {
                                if (user.IsInTrialPeriod)
                                {
                                    await UserManager.AddClaimAsync(user.Id, new Claim("InTrial", "Yes"));

                                }
                                else
                                {
                                    await UserManager.AddClaimAsync(user.Id, new Claim("InTrial", "No"));

                                }
                            }
                            claims = await UserManager.GetClaimsAsync(user.Id);

                        }
                        else
                        {
                            var claims = await UserManager.GetClaimsAsync(user.Id);
                            if (!claims.Any(c => c.Type == "InTrial" && c.Value != ""))
                            {
                                if (user.IsInTrialPeriod)
                                {
                                    await UserManager.AddClaimAsync(user.Id, new Claim("InTrial", "Yes"));

                                }
                                else
                                {
                                    await UserManager.AddClaimAsync(user.Id, new Claim("InTrial", "No"));

                                }
                            }
                            claims = await UserManager.GetClaimsAsync(user.Id);
                        }
                        Session["ID"] = user.Id;
                        var log = new History();
                        log.Business = user.Company;
                        var employee = EmployeeServices.Instance.GetEmployee().Where(x => x.LinkedEmployee == user.Id).FirstOrDefault();
                        if (employee != null)
                        {
                            log.EmployeeName = employee.Name;
                            log.Date = DateTime.Now;
                            log.Note = log.EmployeeName + " Logged In";
                            HistoryServices.Instance.SaveHistory(log);
                        }

                        return RedirectToLocal(returnUrl);
                    case SignInStatus.LockedOut:
                        return View("Lockout");
                    case SignInStatus.RequiresVerification:
                        return RedirectToAction("SendCode", new { ReturnUrl = returnUrl, RememberMe = model.RememberMe });
                    case SignInStatus.Failure:
                    default:
                        ModelState.AddModelError("", "Invalid login attempt.");
                        return View(model);
                }
            }
            else
            {
                ModelState.AddModelError("", "Invalid login attempt.");
                return View(model);
            }
        }




        private void ProcessSuccessfulLogin(User user)
        {
            Session["User"] = user.Name;
            Session["ID"] = user.Id;

            var log = new History
            {
                Business = user.Company
            };

            var employee = EmployeeServices.Instance.GetEmployee().FirstOrDefault(x => x.LinkedEmployee == user.Id);
            if (employee != null)
            {
                log.EmployeeName = employee.Name;
                log.Date = DateTime.Now;
                log.Note = $"{log.EmployeeName} Logged In";
                HistoryServices.Instance.SaveHistory(log);
            }
        }



        //
        // GET: /Account/VerifyCode
        [AllowAnonymous]
        public async Task<ActionResult> VerifyCode(string provider, string returnUrl, bool rememberMe)
        {
            // Require that the user has already logged in via username/password or external login
            if (!await SignInManager.HasBeenVerifiedAsync())
            {
                return View("Error");
            }
            return View(new VerifyCodeViewModel { Provider = provider, ReturnUrl = returnUrl, RememberMe = rememberMe });
        }

        //
        // POST: /Account/VerifyCode
        [HttpPost]
        [AllowAnonymous]
        public async Task<ActionResult> VerifyCode(VerifyCodeViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);

            }

            // The following code protects for brute force attacks against the two factor codes. 
            // If a user enters incorrect codes for a specified amount of time then the user account 
            // will be locked out for a specified amount of time. 
            // You can configure the account lockout settings in IdentityConfig
            //signedUser = UserManager.FindByEmail(model.Email);
            var result = await SignInManager.TwoFactorSignInAsync(model.Provider, model.Code, isPersistent: model.RememberMe, rememberBrowser: model.RememberBrowser);
            switch (result)
            {
                case SignInStatus.Success:
                    return RedirectToLocal(model.ReturnUrl);
                case SignInStatus.LockedOut:
                    return View("Lockout");
                case SignInStatus.Failure:
                default:
                    ModelState.AddModelError("", "Invalid code.");
                    return View(model);
            }
        }

        //
        // GET: /Account/Register
        [AllowAnonymous]
        public ActionResult Register(string Package = "")
        {
            RegisterViewModel model = new RegisterViewModel();
            model.Roles = RolesManager.Roles.ToList();
            model.Package = Package;
            return View(model);
        }







        //
        // POST: /Account/Register
        [HttpPost]
        [AllowAnonymous]
        public async Task<ActionResult> Register(RegisterViewModel model)
        {
            if (UserManager.Users.ToList().Where(x => x.Email.ToLower() == model.Email.ToLower()).Count() == 0)
            {
                if (ModelState.IsValid)
                {
                    var role = await RolesManager.FindByIdAsync(model.RoleID);
                    if (!User.Identity.IsAuthenticated)
                    {
                        var user = new User { UserName = model.Email, Email = model.Email, PhoneNumber = model.Contact, Name = model.Name, Role = role.Name, Password = model.Password, RegisteredDate = DateTime.Now, IsInTrialPeriod = true, IsPaid = false, LastPaymentDate = DateTime.Now.ToString("yyyy-MM-dd") };

                        var result = await UserManager.CreateAsync(user, model.Password);
                        if (result.Succeeded)
                        {
                            await UserManager.AddToRoleAsync(user.Id, role.Name);

                            //await SignInManager.SignInAsync(user, isPersistent:false, rememberBrowser:false);

                            // For more information on how to enable account confirmation and password reset please visit https://go.microsoft.com/fwlink/?LinkID=320771
                            // Send an email with this link
                            // string code = await UserManager.GenerateEmailConfirmationTokenAsync(user.Id);
                            // var callbackUrl = Url.Action("ConfirmEmail", "Account", new { userId = user.Id, code = code }, protocol: Request.Url.Scheme);
                            // await UserManager.SendEmailAsync(user.Id, "Confirm your account", "Please confirm your account by clicking <a href=\"" + callbackUrl + "\">here</a>");
                            if (!User.Identity.IsAuthenticated)
                            {
                                Session["RegisteredEmail"] = user.Email;
                                Session["RegisteredPAKKITA"] = user.Package;
                                return RedirectToAction("RegisterCompany", "User");
                            }
                            else
                            {
                                if (user.Role == "Admin")
                                {
                                    return RedirectToAction("Admin", "Dashboard");
                                }
                                else
                                {
                                    return RedirectToAction("Index", "User");

                                }
                            }
                        }

                    }
                    else
                    {
                        var LoggedInUserCompany = UserManager.FindById(User.Identity.GetUserId());
                        var Company = CompanyServices.Instance.GetCompany().Where(x => x.Business == LoggedInUserCompany.Company).FirstOrDefault();
                    

                        var user = new User { UserName = model.Email, Email = model.Email, PhoneNumber = model.Contact, Name = model.Name, Role = role.Name, Password = model.Password, 
                                            Package = LoggedInUserCompany.Package,LastPaymentDate = LoggedInUserCompany.LastPaymentDate, Company = LoggedInUserCompany.Company, RegisteredDate = DateTime.Now, IsInTrialPeriod = false, IsPaid = true };
                        var result = await UserManager.CreateAsync(user, model.Password);
                        if (result.Succeeded)
                        {
                            await UserManager.AddToRoleAsync(user.Id, role.Name);
                            var newEmployeeLinking = String.Join(",", Company.EmployeesLinked, user.Name);
                            Company.EmployeesLinked = newEmployeeLinking;
                            CompanyServices.Instance.UpdateCompany(Company);
                            //await SignInManager.SignInAsync(user, isPersistent:false, rememberBrowser:false);

                            // For more information on how to enable account confirmation and password reset please visit https://go.microsoft.com/fwlink/?LinkID=320771
                            // Send an email with this link
                            // string code = await UserManager.GenerateEmailConfirmationTokenAsync(user.Id);
                            // var callbackUrl = Url.Action("ConfirmEmail", "Account", new { userId = user.Id, code = code }, protocol: Request.Url.Scheme);
                            // await UserManager.SendEmailAsync(user.Id, "Confirm your account", "Please confirm your account by clicking <a href=\"" + callbackUrl + "\">here</a>");
                            if (!User.Identity.IsAuthenticated)
                            {

                                return RedirectToAction("", "");
                            }
                            else
                            {
                                return RedirectToAction("Index", "User");
                            }
                            AddErrors(result);
                        }
                    }



                }
                return RedirectToAction("", "");

            }
            else
            {
                Session["Message"] = "User Already Registered";
                return RedirectToAction("Register", "Account");

            }

            // If we got this far, something failed, redisplay form
        }

        //
        // GET: /Account/ConfirmEmail
        [AllowAnonymous]
        public async Task<ActionResult> ConfirmEmail(string userId, string code)
        {
            if (userId == null || code == null)
            {
                return View("Error");
            }
            var result = await UserManager.ConfirmEmailAsync(userId, code);
            return View(result.Succeeded ? "ConfirmEmail" : "Error");
        }

        //
        // GET: /Account/ForgotPassword
        [AllowAnonymous]
        public ActionResult ForgotPassword()
        {
            return View();
        }

        //
        // POST: /Account/ForgotPassword
        [HttpPost]
        [AllowAnonymous]
        public async Task<ActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await UserManager.FindByNameAsync(model.Email);
                if (user == null || !(await UserManager.IsEmailConfirmedAsync(user.Id)))
                {
                    // Don't reveal that the user does not exist or is not confirmed
                    return View("ForgotPasswordConfirmation");
                }

                // For more information on how to enable account confirmation and password reset please visit https://go.microsoft.com/fwlink/?LinkID=320771
                // Send an email with this link
                // string code = await UserManager.GeneratePasswordResetTokenAsync(user.Id);
                // var callbackUrl = Url.Action("ResetPassword", "Account", new { userId = user.Id, code = code }, protocol: Request.Url.Scheme);		
                // await UserManager.SendEmailAsync(user.Id, "Reset Password", "Please reset your password by clicking <a href=\"" + callbackUrl + "\">here</a>");
                // return RedirectToAction("ForgotPasswordConfirmation", "Account");
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        //
        // GET: /Account/ForgotPasswordConfirmation
        [AllowAnonymous]
        public ActionResult ForgotPasswordConfirmation()
        {
            return View();
        }

        [Authorize]
        [HttpGet]
        public ActionResult UserProfile()
        {
            AdminViewModel model = new AdminViewModel();
            var user = UserManager.FindById(User.Identity.GetUserId());
            model.Company = CompanyServices.Instance.GetCompany().Where(x => x.Business == user.Company).FirstOrDefault();
            model.Contact = user.PhoneNumber;
            model.Email = user.Email;
            model.ID = user.Id;
            model.UserName = user.UserName;
            model.Name = user.Name;
            return View(model);
        }


        [HttpPost]
        public async Task<ActionResult> UserProfile(AdminViewModel model)
        {
            var user = UserManager.FindById(User.Identity.GetUserId());
            if (model.Password != null && model.Password != "")
            {
                var token = await UserManager.GeneratePasswordResetTokenAsync(model.ID);
                var result = await UserManager.ResetPasswordAsync(model.ID, token, model.Password);
            }



            user.Id = model.ID;
            user.Name = model.Name;
            user.PhoneNumber = model.Contact;
            user.UserName = model.Email;
            user.Email = model.Email;
            await UserManager.UpdateAsync(user);

            return View(model);
        }

        //
        // GET: /Account/ResetPassword
        [AllowAnonymous]
        public ActionResult ResetPassword(string code)
        {
            return code == null ? View("Error") : View();
        }

        //
        // POST: /Account/ResetPassword
        [HttpPost]
        [AllowAnonymous]
        public async Task<ActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            var user = await UserManager.FindByNameAsync(model.Email);
            if (user == null)
            {
                // Don't reveal that the user does not exist
                return RedirectToAction("ResetPasswordConfirmation", "Account");
            }
            var result = await UserManager.ResetPasswordAsync(user.Id, model.Code, model.Password);
            if (result.Succeeded)
            {
                return RedirectToAction("ResetPasswordConfirmation", "Account");
            }
            AddErrors(result);
            return View();
        }

        //
        // GET: /Account/ResetPasswordConfirmation
        [AllowAnonymous]
        public ActionResult ResetPasswordConfirmation()
        {
            return View();
        }


        [HttpGet]
        public JsonResult CheckUser(string email, string password)
        {
            var user = UserManager.Users.Where(x => x.Email == email && x.Password == password).FirstOrDefault();
            if (user != null)
            {
                return Json(new { success = true }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(new { success = false }, JsonRequestBehavior.AllowGet);

            }
        }
        //
        // POST: /Account/ExternalLogin
        [HttpPost]
        [AllowAnonymous]
        public ActionResult ExternalLogin(string provider, string returnUrl)
        {
            // Request a redirect to the external login provider
            return new ChallengeResult(provider, Url.Action("ExternalLoginCallback", "Account", new { ReturnUrl = returnUrl }));
        }

        //
        // GET: /Account/SendCode
        [AllowAnonymous]
        public async Task<ActionResult> SendCode(string returnUrl, bool rememberMe)
        {
            var userId = await SignInManager.GetVerifiedUserIdAsync();
            if (userId == null)
            {
                return View("Error");
            }
            var userFactors = await UserManager.GetValidTwoFactorProvidersAsync(userId);
            var factorOptions = userFactors.Select(purpose => new SelectListItem { Text = purpose, Value = purpose }).ToList();
            return View(new SendCodeViewModel { Providers = factorOptions, ReturnUrl = returnUrl, RememberMe = rememberMe });
        }

        //
        // POST: /Account/SendCode
        [HttpPost]
        [AllowAnonymous]
        public async Task<ActionResult> SendCode(SendCodeViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View();
            }

            // Generate the token and send it
            if (!await SignInManager.SendTwoFactorCodeAsync(model.SelectedProvider))
            {
                return View("Error");
            }
            return RedirectToAction("VerifyCode", new { Provider = model.SelectedProvider, ReturnUrl = model.ReturnUrl, RememberMe = model.RememberMe });
        }

        //
        // GET: /Account/ExternalLoginCallback
        [AllowAnonymous]
        public async Task<ActionResult> ExternalLoginCallback(string returnUrl)
        {
            var loginInfo = await AuthenticationManager.GetExternalLoginInfoAsync();
            if (loginInfo == null)
            {
                return RedirectToAction("Login");
            }

            // Sign in the user with this external login provider if the user already has a login
            var result = await SignInManager.ExternalSignInAsync(loginInfo, isPersistent: false);
            switch (result)
            {
                case SignInStatus.Success:
                    return RedirectToLocal(returnUrl);
                case SignInStatus.LockedOut:
                    return View("Lockout");
                case SignInStatus.RequiresVerification:
                    return RedirectToAction("SendCode", new { ReturnUrl = returnUrl, RememberMe = false });
                case SignInStatus.Failure:
                default:
                    // If the user does not have an account, then prompt the user to create an account
                    ViewBag.ReturnUrl = returnUrl;
                    ViewBag.LoginProvider = loginInfo.Login.LoginProvider;
                    return View("ExternalLoginConfirmation", new ExternalLoginConfirmationViewModel { Email = loginInfo.Email });
            }
        }

        //
        // POST: /Account/ExternalLoginConfirmation
        [HttpPost]
        [AllowAnonymous]
        public async Task<ActionResult> ExternalLoginConfirmation(ExternalLoginConfirmationViewModel model, string returnUrl)
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Manage");
            }

            if (ModelState.IsValid)
            {
                // Get the information about the user from the external login provider
                var info = await AuthenticationManager.GetExternalLoginInfoAsync();
                if (info == null)
                {
                    return View("ExternalLoginFailure");
                }
                var user = new User { UserName = model.Email, Email = model.Email };
                var result = await UserManager.CreateAsync(user);
                if (result.Succeeded)
                {
                    result = await UserManager.AddLoginAsync(user.Id, info.Login);
                    if (result.Succeeded)
                    {
                        await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
                        return RedirectToLocal(returnUrl);
                    }
                }
                AddErrors(result);
            }

            ViewBag.ReturnUrl = returnUrl;
            return View(model);
        }

        //
        // POST: /Account/LogOff
        [HttpPost]
        public ActionResult LogOff()
        {
            //AuthenticationManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
            HttpContext.GetOwinContext().Authentication.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
            return RedirectToAction("Login", "Account");
        }



        //
        // GET: /Account/ExternalLoginFailure
        [AllowAnonymous]
        public ActionResult ExternalLoginFailure()
        {
            return View();
        }


        [HttpGet]
        public ActionResult LogOff(string search = "")
        {
            AuthenticationManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
            return RedirectToAction("Login", "Account");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_userManager != null)
                {
                    _userManager.Dispose();
                    _userManager = null;
                }

                if (_signInManager != null)
                {
                    _signInManager.Dispose();
                    _signInManager = null;
                }
            }

            base.Dispose(disposing);
        }

        #region Helpers
        // Used for XSRF protection when adding external logins
        private const string XsrfKey = "XsrfId";

        private IAuthenticationManager AuthenticationManager
        {
            get
            {
                return HttpContext.GetOwinContext().Authentication;
            }
        }

        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error);
            }
        }
        private ActionResult RedirectToLocal(string returnUrl)
        {
            var userID = "";
            if (Convert.ToString(Session["ID"]) != "")
            {
                userID = Session["ID"].ToString();
            }
            if (Url.IsLocalUrl(returnUrl))
            {
                return RedirectToAction(returnUrl);
            }
            var user = UserManager.FindById(userID);
            if (user.Role == "Super Admin")
            {
                return RedirectToAction("Dashboard", "Admin");

            }
            else if (user.Role == "Admin")
            {
                var UserLogginIn = UserManager.FindById(userID);
                if (UserLogginIn.Company == null)
                {
                    var Company = CompanyServices.Instance.GetCompany().Where(x => x.CreatedBy == userID).FirstOrDefault();
                    if (Company != null)
                    {
                        return RedirectToAction("Dashboard", "Admin");
                    }
                    else
                    {
                        return RedirectToAction("RegisterCompany", "User");

                    }
                }
                else
                {
                    return RedirectToAction("Dashboard", "Admin");

                }
            }
            else if (user.Role == "Calendar")
            {
                return RedirectToAction("Dashboard", "Admin");
            }
            else if (user.Role == "Accountant")
            {
                return RedirectToAction("Dashboard", "Admin");
            }
            else if (user.Role == "Owner")
            {
                var UserLogginIn = UserManager.FindById(userID);
                if (UserLogginIn.Company == null)
                {
                    var Company = CompanyServices.Instance.GetCompany().Where(x => x.CreatedBy == userID).FirstOrDefault();
                    if (Company != null)
                    {
                        return RedirectToAction("Dashboard", "Admin");
                    }
                    else
                    {
                        return RedirectToAction("RegisterCompany", "User");

                    }
                }
                else
                {
                    return RedirectToAction("Dashboard", "Admin");

                }
            }
            else if (user.Role == "Manager")
            {
                return RedirectToAction("Dashboard", "Admin");
            }
            else
            {
                return RedirectToAction("", "");

            }
        }


        internal class ChallengeResult : HttpUnauthorizedResult
        {
            public ChallengeResult(string provider, string redirectUri)
                : this(provider, redirectUri, null)
            {
            }

            public ChallengeResult(string provider, string redirectUri, string userId)
            {
                LoginProvider = provider;
                RedirectUri = redirectUri;
                UserId = userId;
            }

            public string LoginProvider { get; set; }
            public string RedirectUri { get; set; }
            public string UserId { get; set; }

            public override void ExecuteResult(ControllerContext context)
            {
                var properties = new AuthenticationProperties { RedirectUri = RedirectUri };
                if (UserId != null)
                {
                    properties.Dictionary[XsrfKey] = UserId;
                }
                context.HttpContext.GetOwinContext().Authentication.Challenge(properties, LoginProvider);
            }
        }
        #endregion
    }
}