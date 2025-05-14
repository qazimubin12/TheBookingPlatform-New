using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using OfficeOpenXml;
using Stripe;
using Stripe.FinancialConnections;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Mail;
using System.Net;
using System.Text;
using System.Web;
using System.Web.Mvc;
using TheBookingPlatform.Entities;
using TheBookingPlatform.Services;
using TheBookingPlatform.ViewModels;

namespace TheBookingPlatform.Controllers
{
    public class GiftCardController : Controller
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

        public GiftCardController()
        {
        }

        public GiftCardController(AMUserManager userManager, AMSignInManager signInManager)
        {
            UserManager = userManager;
            SignInManager = signInManager;
        }

        #endregion
        // GET: GiftCard
        public ActionResult Index(string SearchTerm = "")
        {

            GiftCardListingViewModel model = new GiftCardListingViewModel();
            model.SearchTerm = SearchTerm;
            var loggedInUser = UserManager.FindById(User.Identity.GetUserId());
            if (loggedInUser == null)
            {
                return RedirectToAction("Login", "Account");
            }
            else
            {
                if(loggedInUser.Role == "Super Admin")
                {
                    model.GiftCards = GiftCardServices.Instance.GetGiftCard(SearchTerm);
                }
                else
                {
                    model.GiftCards = GiftCardServices.Instance.GetGiftCard(SearchTerm).Where(x=>x.Business == loggedInUser.Company).ToList();
                }
                model.Company = CompanyServices.Instance.GetCompanyByName(loggedInUser.Company);
            }
            return View(model);
        }


        public ActionResult CardAssignmentIndex()
        {
            GiftCardAssignmentListingViewModel model = new GiftCardAssignmentListingViewModel();
            var GiftCardAssignmentModel = new List<GiftCardAssignmentModel>();
            var loggedInUser = UserManager.FindById(User.Identity.GetUserId());
            if (loggedInUser.Role == "Super Admin")
            {
                var GiftCardsAssignments = GiftCardServices.Instance.GetGiftCardAssignment();
                foreach (var item in GiftCardsAssignments)
                {
                    var GiftCard = GiftCardServices.Instance.GetGiftCard(item.GiftCardID);
                    var customer = CustomerServices.Instance.GetCustomer(item.CustomerID);

                    GiftCardAssignmentModel.Add(new GiftCardAssignmentModel { GiftCard = GiftCard, GiftCardAssignment = item, Customer = customer });
                }

            }
            else
            {
                var GiftCardsAssignments = GiftCardServices.Instance.GetGiftCardAssignment().Where(x => x.Business == loggedInUser.Company).ToList();
                foreach (var item in GiftCardsAssignments)
                {
                    var GiftCard = GiftCardServices.Instance.GetGiftCard(item.GiftCardID);
                    var customer = CustomerServices.Instance.GetCustomer(item.CustomerID);
                    GiftCardAssignmentModel.Add(new GiftCardAssignmentModel { GiftCard = GiftCard, GiftCardAssignment = item,Customer =customer });
                }
            }
            model.GiftCardAssignments = GiftCardAssignmentModel;
            return View(model);
        }



        [HttpGet]
        public ActionResult AssignmentAction(int ID = 0)
        {
            GiftCardAssignmentActionViewModel model = new GiftCardAssignmentActionViewModel();
            var loggedInUser = UserManager.FindById(User.Identity.GetUserId());
            if (loggedInUser == null)
            {
                return RedirectToAction("Login", "Account");
            }
            else
            {
                if (loggedInUser.Role == "Super Admin")
                {
                    model.GiftCards = GiftCardServices.Instance.GetGiftCard();
                    model.Customers = CustomerServices.Instance.GetCustomer();
                }
                else
                {
                    model.GiftCards = GiftCardServices.Instance.GetGiftCard().Where(x => x.Business == loggedInUser.Company).ToList();
                    model.Customers = CustomerServices.Instance.GetCustomer().Where(x => x.Business == loggedInUser.Company).ToList(); 
                }

                if (ID != 0)
                {
                    var giftCardAssignment = GiftCardServices.Instance.GetGiftCardAssignment(ID);
                    if (giftCardAssignment != null)
                    {
                        model.ID = giftCardAssignment.ID;
                        model.CustomerID = giftCardAssignment.CustomerID;
                        model.AssignedDate = giftCardAssignment.AssignedDate;
                        model.Balance = giftCardAssignment.Balance;
                        model.AssignedAmount = giftCardAssignment.AssignedAmount;
                        model.Customer = CustomerServices.Instance.GetCustomer(model.CustomerID);

                       
                    }
                }
                return View(model);
            }
        }


        [HttpGet]
        public ActionResult Action(int ID = 0)
        {
            GiftCardActionViewModel model = new GiftCardActionViewModel();
            var loggedInUser = UserManager.FindById(User.Identity.GetUserId());
            if (loggedInUser == null)
            {
                return RedirectToAction("Login", "Account");
            }
            else
            {
                if (ID != 0)
                {
                    var GiftCard = GiftCardServices.Instance.GetGiftCard(ID);
                    if (GiftCard != null)
                    {
                        model.ID = GiftCard.ID;
                        model.Name = GiftCard.Name;
                        model.Code = GiftCard.Code;
                        model.IsActive = GiftCard.IsActive;
                        model.Date = GiftCard.Date;
                        model.Days = GiftCard.Days;
                      
                        model.GiftCardAmount = GiftCard.GiftCardAmount;
                        model.GiftCardImage = GiftCard.GiftCardImage;
                    }
                }
                return View(model);
            }
        }


        [HttpPost]
        public ActionResult Action(GiftCardActionViewModel model)
        {
            var loggedInUser = UserManager.FindById(User.Identity.GetUserId());
            if (model.ID != 0)
            {
                var giftCard = GiftCardServices.Instance.GetGiftCard(model.ID);
                giftCard.ID = model.ID;
                giftCard.Name = model.Name;
                giftCard.Code = model.Code;
                giftCard.IsActive = model.IsActive;
                giftCard.Days = model.Days;
                giftCard.GiftCardAmount = model.GiftCardAmount;
                giftCard.GiftCardImage = model.GiftCardImage;
                GiftCardServices.Instance.UpdateGiftCard(giftCard);
                return Json(new { success = true }, JsonRequestBehavior.AllowGet);

            }
            else
            {
                var giftCard = new GiftCard();
                giftCard.Name = model.Name;
                giftCard.Code = model.Code;
                giftCard.IsActive = model.IsActive;
                giftCard.Days = model.Days;
                giftCard.Business = loggedInUser.Company;
                giftCard.GiftCardAmount = model.GiftCardAmount;
                giftCard.GiftCardImage = model.GiftCardImage;
                giftCard.Business = loggedInUser.Company;
                giftCard.Date = DateTime.Now;
                GiftCardServices.Instance.SaveGiftCard(giftCard);
                return Json(new { success = true }, JsonRequestBehavior.AllowGet);
            }
        }


        [HttpGet]
        public ActionResult Delete(int ID)
        {
            GiftCardActionViewModel model = new GiftCardActionViewModel();
            model.ID = ID;
            return PartialView("_Delete", model);
        }



        [HttpGet]
        public ActionResult HistoryOfGiftCard(int GiftCardID,int CustomerID)
        {
            GiftCardHistoryViewModel model = new GiftCardHistoryViewModel();
            var loggedInUser = UserManager.FindById(User.Identity.GetUserId());
            var customer = CustomerServices.Instance.GetCustomer(CustomerID);
            var giftCardHistory = HistoryServices.Instance.GetHistories().Where(X => X.Type == GiftCardID + "GiftCard" && X.Business == loggedInUser.Company && X.CustomerName == customer.FirstName +" "+customer.LastName).ToList();
            model.Histories = giftCardHistory;
            return View(model);

        }

        public bool SendEmail(string toEmail, string subject, string emailBody, Company company)
        {
            try
            {
                string senderEmail = "support@yourbookingplatform.com";
                string senderPassword = "ttpa fcbl mpbn fxdl";

                var loggedInUser = UserManager.FindById(User.Identity.GetUserId());
                int Port = int.Parse(ConfigurationManager.AppSettings["portforSmtp"]);
                string Host = ConfigurationManager.AppSettings["hostForSmtp"];
                MailMessage mail = new MailMessage();
                mail.To.Add(toEmail);
                MailAddress ccAddress = new MailAddress(company.NotificationEmail, company.Business);

                mail.CC.Add(ccAddress);
                mail.From = new MailAddress(company.NotificationEmail, company.Business, System.Text.Encoding.UTF8);
                mail.Subject = subject;
                mail.SubjectEncoding = System.Text.Encoding.UTF8;
                mail.Body = emailBody;
                mail.BodyEncoding = System.Text.Encoding.UTF8;
                mail.IsBodyHtml = true;
                mail.Priority = MailPriority.High;
                mail.ReplyTo = new MailAddress(company.NotificationEmail); // Set the ReplyTo address

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



        [HttpPost]
        public ActionResult Issue(GiftCardAssignmentActionViewModel model)
        {
            var giftCard = GiftCardServices.Instance.GetGiftCard(model.GiftCardID);
            if (model.CustomerID != 0)
            {
                if (model.ID != 0)
                {
                    var giftCardAssignment = GiftCardServices.Instance.GetGiftCardAssignment(model.ID);
                    giftCardAssignment.ID = model.ID;
                    giftCardAssignment.CustomerID = model.CustomerID;
                    giftCardAssignment.GiftCardID = giftCard.ID;
                    giftCardAssignment.AssignedDate = model.AssignedDate;
                    giftCardAssignment.AssignedAmount = model.AssignedAmount;
                    giftCardAssignment.Balance = model.Balance;
                    giftCardAssignment.AssignedCode = giftCardAssignment.AssignedCode;
                    GiftCardServices.Instance.UpdateGiftCardAssignment(giftCardAssignment);


                    var customer = CustomerServices.Instance.GetCustomer(model.CustomerID);
                    var history = new History();
                    history.Business = customer.Business;
                    history.CustomerName = customer.FirstName + " " + customer.LastName;
                    history.Date = DateTime.Now;
                    history.Note = "Gift Card was updated, Card Name:" + giftCard.Name + "Assigned amount: " + model.AssignedAmount;
                    history.Type = giftCard.ID + "GiftCard";

                    return Json(new { success = true }, JsonRequestBehavior.AllowGet);

                }
                else
                {
                    var LoggedInUser = UserManager.FindById(User.Identity.GetUserId());
                    var giftCardAssignment = new GiftCardAssignment();
                    giftCardAssignment.ID = model.ID;
                    giftCardAssignment.CustomerID = model.CustomerID;
                    giftCardAssignment.AssignedDate = DateTime.Now;
                    giftCardAssignment.AssignedAmount = model.AssignedAmount;
                    giftCardAssignment.Days = giftCard.Days;
                    giftCardAssignment.Balance = model.Balance;
                    giftCardAssignment.GiftCardID = giftCard.ID;
                    giftCardAssignment.AssignedCode = giftCard.Code + "-" + model.CustomerID + "-" + DateTime.Now.ToString("dd") + DateTime.Now.ToString("MM") + DateTime.Now.ToString("HH") + DateTime.Now.ToString("mm") + DateTime.Now.ToString("ss") + DateTime.Now.ToString("yyyy");
                    giftCardAssignment.Business = LoggedInUser.Company;
                    GiftCardServices.Instance.SaveGiftCardAssignment(giftCardAssignment);

                    var customer = CustomerServices.Instance.GetCustomer(model.CustomerID);
                    var history = new History();
                    history.Business = customer.Business;
                    history.CustomerName = customer.FirstName + " " + customer.LastName;
                    history.Date = DateTime.Now;
                    history.Note = "Gift Card was assigned, Card Name:" + giftCard.Name + "Assigned amount: " + model.AssignedAmount;
                    history.Type = giftCard.ID + "GiftCard";
                    HistoryServices.Instance.SaveHistory(history);





                    var company = CompanyServices.Instance.GetCompany().Where(x => x.Business == LoggedInUser.Company).FirstOrDefault();
                    var emailDetails = EmailTemplateServices.Instance.GetEmailTemplate().Where(x => x.Name == "Gift Card Issue Notification" && x.Business == LoggedInUser.Company).FirstOrDefault();
                    string emailBody = "<html><body>";
                    emailBody += "<h2 style='font-family: Arial, sans-serif;'>Gift Card Assignment</h2>";
                    emailBody += emailDetails.TemplateCode;
                    emailBody = emailBody.Replace("{{Customer_first_name}}", customer.FirstName);
                    emailBody = emailBody.Replace("{{Customer_last_name}}", customer.LastName);
                    emailBody = emailBody.Replace("{{Customer_initial}}", "Dear");
                    emailBody = emailBody.Replace("{{date}}", DateTime.Now.ToString("yyyy-MM-dd"));
                    emailBody = emailBody.Replace("{{company_name}}", company.Business);
                    emailBody = emailBody.Replace("{{company_email}}", company.NotificationEmail);
                    emailBody = emailBody.Replace("{{company_address}}", company.Address);
                    emailBody = emailBody.Replace("{{giftcard_code}}", giftCardAssignment.AssignedCode);
                    emailBody = emailBody.Replace("{{giftcard_name}}", giftCard.Name);
                    emailBody = emailBody.Replace("{{giftcard_expiry_days}}", (giftCard.Date.AddDays(giftCardAssignment.Days) - DateTime.Now).Days + " Days");
                    emailBody = emailBody.Replace("{{company_logo}}", $"<img class='text-center' style='height:50px;width:auto;' src='{"http://app.yourbookingplatform.com" + company.Logo}'>");
                    emailBody = emailBody.Replace("{{company_phone}}", company.PhoneNumber);
                    emailBody = emailBody.Replace("{{password}}", customer.Password);

                    emailBody += "</body></html>";


                    if (IsValidEmail(customer.Email))
                    {
                        SendEmail(customer.Email, "Gift Card Assignment", emailBody, company);
                    }
                    return Json(new { success = true }, JsonRequestBehavior.AllowGet);
                }
            }
            else
            {
                return Json(new { success = false, Message = "Please Select Any Customer" });
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
        public ActionResult CheckGiftCardData(int CustomerID,string GiftCardCode)
        {
            var customer = CustomerServices.Instance.GetCustomer(CustomerID);
            var giftCardAssignment = GiftCardServices.Instance.GetGiftCardAssignment()
                  .Where(x => x.CustomerID == customer.ID && x.AssignedCode == GiftCardCode
                  && x.Business == customer.Business).FirstOrDefault();
            if (giftCardAssignment != null)
            {
                return Json(new { success = true, GiftCardAssignment = giftCardAssignment }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(new { success = false, GiftCardAssignment = giftCardAssignment }, JsonRequestBehavior.AllowGet);

            }
        }

        [HttpGet]
        public ActionResult DeleteAssignment(int ID)
        {
            GiftCardAssignmentActionViewModel model = new GiftCardAssignmentActionViewModel();
            model.ID = ID;
            return PartialView("_DeleteAssignment", model);
        }



        [HttpPost]
        public ActionResult DeleteAssignment(GiftCardAssignmentActionViewModel model)
        {
            var giftCard = GiftCardServices.Instance.GetGiftCardAssignment(model.ID);

            GiftCardServices.Instance.DeleteGiftCardAssignment(giftCard.ID);

            return Json(new { success = true }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult Delete(GiftCardActionViewModel model)
        {
            var giftCard = GiftCardServices.Instance.GetGiftCard(model.ID);
            
            var message =   GiftCardServices.Instance.DeleteGiftCard(giftCard.ID);
            if (message == "Deleted Successfully")
            {
                var log = new History();
                log.Business = giftCard.Business;
                log.Date = DateTime.Now;
                log.Note = "Loyalty Card was deleted along with the promotions";
                HistoryServices.Instance.SaveHistory(log);
            }
            return Json(new { success = true, Message = message }, JsonRequestBehavior.AllowGet);
        }
    }
}