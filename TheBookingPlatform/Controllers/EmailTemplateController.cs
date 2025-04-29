using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
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
    public class EmailTemplateController : Controller
    {
        // GET: EmailTemplate
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
        public EmailTemplateController()
        {
        }



        public EmailTemplateController(AMUserManager userManager, AMSignInManager signInManager)
        {
            UserManager = userManager;
            SignInManager = signInManager;
        }
        public ActionResult Index(string SearchTerm = "")
        {
            var LoggedInUser = UserManager.FindById(User.Identity.GetUserId());

            SettingsViewModel model = new SettingsViewModel();
            model.SearchTerm = SearchTerm;
            if (LoggedInUser.Role == "Super Admin")
            {
                model.EmailTemplates = EmailTemplateServices.Instance.GetEmailTemplate(SearchTerm);
            }
            else
            {
                model.EmailTemplates = EmailTemplateServices.Instance.GetEmailTemplateWRTBusiness(LoggedInUser.Company);

            }
            return PartialView(model);
        }

        [HttpGet]
        public ActionResult Action(int ID = 0)
        {
            EmailTemplateActionViewModel model = new EmailTemplateActionViewModel();
            var durationList = new List<string>
            {
                "Immediately",
                "15 Mins",
                "30 Mins",
                "1 Hr",
                "12 Hr",
                "24 Hr"
            };
            model.Durations = durationList;

            var DurationForFeedBack = new List<string>
            {
                "Always","3 Appointments","6 Appointments","12 Appointments"
            };
            model.DurationsForFeedback = DurationForFeedBack;
            var variableList = new List<VariableModel>
            {
                new VariableModel { VariableCode = "{{Customer_first_name}}", VariableDescription = "Customer First Name" },
                new VariableModel { VariableCode = "{{Customer_last_name}}", VariableDescription = "Customer Last Name" },
                new VariableModel { VariableCode = "{{Customer_initial}}", VariableDescription = "Customer Initials Mr or Ms" },
                new VariableModel { VariableCode = "{{date}}", VariableDescription = "Appointment Date" },
                new VariableModel { VariableCode = "{{time}}", VariableDescription = "Appointment Time" },
                new VariableModel { VariableCode = "{{end_time}}", VariableDescription = "Appointment End Time" },
                new VariableModel { VariableCode = "{{employee}}", VariableDescription = "Employee Name" },
                new VariableModel { VariableCode = "{{employee_specialization}}", VariableDescription = "Employee Specialization" },
                new VariableModel { VariableCode = "{{services}}", VariableDescription = "Services Name (separated by comma)" },
                new VariableModel { VariableCode = "{{company_name}}", VariableDescription = "Company Name" },
                new VariableModel { VariableCode = "{{company_email}}", VariableDescription = "Company Email"},
                new VariableModel { VariableCode = "{{company_address}}", VariableDescription = "Company Address" },
                new VariableModel { VariableCode = "{{company_logo}}", VariableDescription = "Company Logo" },
                new VariableModel { VariableCode = "{{company_phone}}", VariableDescription = "Company Phone" },
                new VariableModel { VariableCode = "{{password}}", VariableDescription = "Password" },
                new VariableModel { VariableCode = "{{booking_button}}", VariableDescription = "Booking Button" },
                new VariableModel { VariableCode = "{{employee_picture}}", VariableDescription = "Employee Picture" },
                new VariableModel { VariableCode = "{{previous_date}}", VariableDescription = "Previous Date" },
                new VariableModel { VariableCode = "{{previous_time}}", VariableDescription = "Previous Time" },
                new VariableModel { VariableCode = "{{cancellink}}", VariableDescription = "Cancel Link" },
                new VariableModel { VariableCode = "{{loyalty_card}}", VariableDescription = "Loyalty Card Name" },
                new VariableModel { VariableCode = "{{loyalty_card_Number}}", VariableDescription = "Loyalty Card Number" },
                new VariableModel { VariableCode = "{{loyalty_card_bonus}}", VariableDescription = "Loyalty Card Cashback" },
                new VariableModel { VariableCode = "{{loyalty_card_balance}}", VariableDescription = "Loyalty Card Balance" },
                new VariableModel { VariableCode = "{{giftcard_code}}", VariableDescription = "Gift Card Code" },
                new VariableModel { VariableCode = "{{giftcard_name}}", VariableDescription = "Gift Card Name" },
                new VariableModel { VariableCode = "{{giftcard_expiry_days}}", VariableDescription = "Gift Card Expiry Days" },
            };
            model.Variables = variableList;
            if (ID != 0)
            {
                var emailtemplate = EmailTemplateServices.Instance.GetEmailTemplate(ID);
                model.ID = emailtemplate.ID;
                model.TemplateCode = emailtemplate.TemplateCode;
                model.Name = emailtemplate.Name;
                model.Duration = emailtemplate.Duration;
            }
            return View(model);
        }


        [HttpPost]
        public JsonResult UpdateEmailTemplate(int ID, bool IsActive)
        {
            var emailTemplate = EmailTemplateServices.Instance.GetEmailTemplate(ID);
            emailTemplate.IsActive = IsActive;
            EmailTemplateServices.Instance.UpdateEmailTemplate(emailTemplate);
            return Json(new { success = true }, JsonRequestBehavior.AllowGet);
        }



        [HttpPost]
        public ActionResult Action(int ID, string Name, string TemplateCode, string duration)
        {
            var LoggedInUser = UserManager.FindById(User.Identity.GetUserId());
            var DecodedTemplateCode = HttpUtility.UrlDecode(TemplateCode);
            if (ID != 0)
            {
                var emailTemplate = EmailTemplateServices.Instance.GetEmailTemplate(ID);
                emailTemplate.Name = Name;
                emailTemplate.TemplateCode = DecodedTemplateCode;
                emailTemplate.Duration = duration;
                EmailTemplateServices.Instance.UpdateEmailTemplate(emailTemplate);
            }
            else
            {
                if (User.IsInRole("Super Admin"))
                {
                    var companies = CompanyServices.Instance.GetCompany().Select(x => x.Business).ToList();
                    foreach (var item in companies)
                    {
                        var emailTemplate = new EmailTemplate();
                        emailTemplate.Name = Name;
                        emailTemplate.Business = item;
                        emailTemplate.TemplateCode = DecodedTemplateCode;
                        emailTemplate.Duration = duration;
                        EmailTemplateServices.Instance.SaveEmailTemplate(emailTemplate);
                    }

                }
            }

            return Json(new { success = true }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult Delete(int ID)
        {
            EmailTemplateActionViewModel model = new EmailTemplateActionViewModel();
            var EmailTemplate = EmailTemplateServices.Instance.GetEmailTemplate(ID);
            model.ID = EmailTemplate.ID;
            return PartialView("_Delete", model);
        }


        [HttpPost]
        public ActionResult Delete(EmailTemplateActionViewModel model)
        {
            var EmailTemplate = EmailTemplateServices.Instance.GetEmailTemplate(model.ID);

            EmailTemplateServices.Instance.DeleteEmailTemplate(EmailTemplate.ID);

            return Json(new { success = true }, JsonRequestBehavior.AllowGet);
        }
    }

}