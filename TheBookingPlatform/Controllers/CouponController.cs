using Microsoft.AspNet.Identity.Owin;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Mail;
using System.Web;
using System.Web.Mvc;
using TheBookingPlatform.Entities;
using TheBookingPlatform.Services;
using TheBookingPlatform.ViewModels;

namespace TheBookingPlatform.Controllers
{
    public class CouponController : Controller
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
        public CouponController()
        {
        }



        public CouponController(AMUserManager userManager, AMSignInManager signInManager)
        {

            UserManager = userManager;
            SignInManager = signInManager;
        }
        // GET: Coupon
        [Authorize(Roles = "Accountant,Manager,Admin,Owner")]
        public ActionResult Index()
        {
            CouponListingViewModel model = new CouponListingViewModel();
            var LoggedInUser = UserManager.FindById(User.Identity.GetUserId());
            if (LoggedInUser == null)
            {
                return RedirectToAction("Login", "Account");
            }
            if (LoggedInUser.Role != "Super Admin")
            {
                model.Coupons = CouponServices.Instance.GetCouponWRTBusiness(LoggedInUser.Company);
            }
            else
            {
                model.Coupons = CouponServices.Instance.GetCoupon();
            }

            return View(model);
        }

        public bool SendEmail(string toEmail, string subject, string emailBody)
        {
            try
            {
                string senderEmail = "support@yourbookingplatform.com";
                string senderPassword = "ttpa fcbl mpbn fxdl";

                var loggedInUser = UserManager.FindById(User.Identity.GetUserId());
                var company = CompanyServices.Instance.GetCompany().Where(x => x.Business == loggedInUser.Company).FirstOrDefault();
                int Port = int.Parse(ConfigurationManager.AppSettings["portforSmtp"]);
                string Host = ConfigurationManager.AppSettings["hostForSmtp"];
                MailMessage mail = new MailMessage();
                mail.To.Add(toEmail);
                MailAddress ccAddress = new MailAddress(company.NotificationEmail, loggedInUser.Company);

                mail.CC.Add(ccAddress);
                mail.From = new MailAddress(company.NotificationEmail, loggedInUser.Company, System.Text.Encoding.UTF8);
                mail.Subject = subject;
                mail.SubjectEncoding = System.Text.Encoding.UTF8;
                mail.Body = emailBody;
                mail.BodyEncoding = System.Text.Encoding.UTF8;
                mail.IsBodyHtml = true;
                mail.ReplyTo = new MailAddress(company.NotificationEmail); // Set the ReplyTo address

                mail.Priority = MailPriority.High;
                SmtpClient client = new SmtpClient();
                client.Credentials = new System.Net.NetworkCredential(senderEmail, senderPassword);
                client.Port = Port;
                client.Host = Host;
                client.EnableSsl = true;
                client.Send(mail);
                return true;
            }
            catch (Exception ex)
            {
                Session["EmailStatus"] = ex.ToString();
                return false;
            }

        }
        bool IsValidEmail(string email)
        {
            var trimmedEmail = email.Trim();

            if (trimmedEmail.EndsWith("."))
            {
                return false; // suggested by @TK-421
            }
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == trimmedEmail;
            }
            catch
            {
                return false;
            }
        }



        [HttpPost]
        public JsonResult SendEmailToClients(int ID)
        {
            var coupon = CouponServices.Instance.GetCoupon(ID);
            if (coupon != null && !coupon.IsDisabled)
            {
                var couponSwitch = CouponSwitchServices.Instance.GetCouponSwitch().Where(x => x.CouponID == ID).FirstOrDefault();
                if(couponSwitch != null)
                {
                    couponSwitch.BlastingStatus = true;
                    CouponSwitchServices.Instance.UpdateCouponSwitch(couponSwitch);
                }
                else
                {
                    couponSwitch = new CouponSwitch();
                    couponSwitch.BlastingStatus = true;
                    couponSwitch.CouponID = ID;
                    couponSwitch.Business = coupon.Business;
                    CouponSwitchServices.Instance.SaveCouponSwitch(couponSwitch);
                }
               
            }
            return Json(new { success = true }, JsonRequestBehavior.AllowGet);
        }


        [HttpGet]
        public JsonResult CheckCouponCode(int CustomerID, string Code)
        {
            var LoggedInUser = UserManager.FindById(User.Identity.GetUserId());

            var couponAssignments = CouponServices.Instance.GetCouponAssignmentsWRTBusiness(LoggedInUser.Company, CustomerID);
            var coupon = new Coupon();
            var couponAssignment = new CouponAssignment();
            string Message = "";
            if (couponAssignment != null)
            {
                if (couponAssignments.Count() > 0)
                {
                    foreach (var item in couponAssignments)
                    {
                        coupon = CouponServices.Instance.GetCoupon(item.CouponID);
                        if (coupon.CouponCode == Code && !coupon.IsDisabled && item.Used < coupon.UsageCount)
                        {
                            couponAssignment = item;
                            break;
                        }
                        else if (coupon.IsDisabled)
                        {
                            Message = "This coupon is disabled.";

                        }
                        else if (coupon.CouponCode != Code)
                        {
                            Message = "Coupon Code is incorrect.";
                        }
                        else
                        {
                            Message = "No Coupon Found.";
                        }
                    }
                }
                else
                {
                    Message = "No Coupon Found.";
                }
                if (Message == "")
                {
                    return Json(new { success = true, Coupon = coupon, CouponAssignment = couponAssignment }, JsonRequestBehavior.AllowGet);

                }
                else
                {
                    return Json(new { success = false, Message = Message }, JsonRequestBehavior.AllowGet);

                }
            }
            else
            {
                return Json(new { success = false, Message = "No Coupon Found" }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet]
        public ActionResult Action(int ID = 0)
        {
            CouponActionViewModel model = new CouponActionViewModel();
            if (ID != 0)
            {
                var coupon = CouponServices.Instance.GetCoupon(ID);
                if (coupon != null)
                {
                    model.ID = coupon.ID;
                    model.ExpiryDate = coupon.ExpiryDate;
                    model.UsageCount = coupon.UsageCount;
                    model.CouponCode = coupon.CouponCode;
                    model.CouponName = coupon.CouponName;
                    model.Discount = coupon.Discount;
                    model.CouponDescription = coupon.CouponDescription;
                }
            }
            return View(model);
        }

        [HttpPost]
        public JsonResult EnableCoupon(int ID)
        {
            var coupon = CouponServices.Instance.GetCoupon(ID);
            coupon.IsDisabled = false;
            CouponServices.Instance.UpdateCoupon(coupon);
            return Json(new { success = true }, JsonRequestBehavior.AllowGet);
        }


        [HttpPost]
        public JsonResult DisableCoupon(int ID)
        {
            var coupon = CouponServices.Instance.GetCoupon(ID);
            coupon.IsDisabled = true;
            CouponServices.Instance.UpdateCoupon(coupon);
            return Json(new { success = true }, JsonRequestBehavior.AllowGet);
        }


        [HttpPost]
        public JsonResult Action(CouponActionViewModel model)
        {
            var LoggedInUser = UserManager.FindById(User.Identity.GetUserId());

            if (model.ID != 0)
            {
                var coupon = CouponServices.Instance.GetCoupon(model.ID);
                coupon.ID = model.ID;
                coupon.ExpiryDate = model.ExpiryDate;
                coupon.UsageCount = model.UsageCount;
                coupon.CouponCode = model.CouponCode;
                coupon.Business = LoggedInUser.Company;
                coupon.Discount = model.Discount;
                coupon.CouponName = model.CouponName;
                coupon.CouponDescription = model.CouponDescription;
                CouponServices.Instance.UpdateCoupon(coupon);
                return Json(new { success = true }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                var coupon = new Entities.Coupon();
                coupon.DateCreated = model.DateCreated;
                coupon.Business = LoggedInUser.Company;
                coupon.ExpiryDate = model.ExpiryDate;
                coupon.UsageCount = model.UsageCount;
                coupon.DateCreated = DateTime.Now;
                coupon.CouponCode = model.CouponCode;
                coupon.Discount = model.Discount;
                coupon.CouponName = model.CouponName;
                coupon.CouponDescription = model.CouponDescription;
                CouponServices.Instance.SaveCoupon(coupon);
                return Json(new { success = true }, JsonRequestBehavior.AllowGet);
            }



        }


        [HttpGet]
        public ActionResult Delete(int ID)
        {
            CouponActionViewModel model = new CouponActionViewModel();
            var Coupon = CouponServices.Instance.GetCoupon(ID);
            model.ID = Coupon.ID;
            return PartialView("_Delete", model);
        }

        [HttpPost]
        public ActionResult Delete(CouponActionViewModel model)
        {
            var Coupon = CouponServices.Instance.GetCoupon(model.ID);

            CouponServices.Instance.DeleteCoupon(Coupon.ID);

            return Json(new { success = true }, JsonRequestBehavior.AllowGet);
        }
    }

}