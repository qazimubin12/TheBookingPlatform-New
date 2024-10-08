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
using System.Net.Mail;
using System.Configuration;
using System.Net;
using System.Text;

namespace TheBookingPlatform.Controllers
{
    public class LoyaltyCardController : Controller
    {
        // GET: LoyaltyCard
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

        public LoyaltyCardController()
        {
        }

        public LoyaltyCardController(AMUserManager userManager, AMSignInManager signInManager)
        {
            UserManager = userManager;
            SignInManager = signInManager;
        }

        #endregion
        public ActionResult Index()
        {
            LoyaltyCardListingViewModel model = new LoyaltyCardListingViewModel();
            var LoggedInUser = UserManager.FindById(User.Identity.GetUserId());
            if (LoggedInUser != null)
            {
                if (LoggedInUser.Role != "Super Admin")
                {
                    var LoyaltyCardFinalList = new List<LoyaltyCardModel>();
                    var loyaltyCards = LoyaltyCardServices.Instance.GetLoyaltyCard().Where(x => x.Business == LoggedInUser.Company).ToList();
                    foreach (var item in loyaltyCards)
                    {
                        var loyaltyCardPromotions = LoyaltyCardServices.Instance.GetLoyaltyCardPromotions(item.ID);

                        LoyaltyCardFinalList.Add(new LoyaltyCardModel { LoyaltyCard = item, LoyaltyCardPromotionsIndex = loyaltyCardPromotions });
                    }
                    model.LoyaltyCards = LoyaltyCardFinalList;

                }
                else
                {
                    var LoyaltyCardFinalList = new List<LoyaltyCardModel>();
                    var loyaltyCards = LoyaltyCardServices.Instance.GetLoyaltyCard();
                    foreach (var item in loyaltyCards)
                    {
                        var loyaltyCardPromotions = LoyaltyCardServices.Instance.GetLoyaltyCardPromotions(item.ID);

                        LoyaltyCardFinalList.Add(new LoyaltyCardModel { LoyaltyCard = item, LoyaltyCardPromotionsIndex = loyaltyCardPromotions });
                    }
                    model.LoyaltyCards = LoyaltyCardFinalList;
                }
                return View(model);
            }
            else
            {
                return RedirectToAction("Login", "Account");
            }
        }



        [HttpGet]
        public JsonResult CheckCustomerWithLoyaltyCard(int CustomerID, int LoyaltyCardID)
        {
            bool AlreadyAssigned = LoyaltyCardServices.Instance.GetLoyaltyCardAssignments().Where(x => x.CustomerID == CustomerID && x.LoyaltyCardID == LoyaltyCardID).Any();
            return Json(new { AlreadyAssigned = AlreadyAssigned }, JsonRequestBehavior.AllowGet);
        }



        [HttpGet]
        public ActionResult UpdatePromotion(int PromotionID)
        {
            LoyaltyCardPromotionViewModel model = new LoyaltyCardPromotionViewModel();
            model.LoyaltyCardPromotion = LoyaltyCardServices.Instance.GetLoyaltyCardPromotion(PromotionID);
            if (model.LoyaltyCardPromotion != null)
            {
                string[] servicesArray = model.LoyaltyCardPromotion.Services.Split(',');

                // Use TryParse to convert each string to an int, skipping non-parsable values
                List<int> alreadyHaveServices = servicesArray.Select(s =>
                {
                    int serviceId;
                    if (int.TryParse(s, out serviceId))
                    {
                        return serviceId;
                    }
                    else
                    {
                        return 0;  // Or any other value you want for non-parsable items
                    }
                })
                .Where(id => id != 0)  // Filter out 0 (or the chosen value for non-parsable items)
                .ToList();

                // Assuming model.AlreadyHaveServices is a List<int>
                model.AlreadyHaveServices = alreadyHaveServices;
                model.LoyaltyCard = LoyaltyCardServices.Instance.GetLoyaltyCard(model.LoyaltyCardPromotion.LoyaltyCardID);
                model.Services = ServiceServices.Instance.GetService().Where(x => x.IsActive && x.Business == model.LoyaltyCardPromotion.Business).ToList();
                model.ID = model.LoyaltyCardPromotion.ID;
                model.Percentage = model.LoyaltyCardPromotion.Percentage;
            }

            return View(model);

        }

        [HttpPost]
        public ActionResult UpdatePromotion(int ID, string ServiceListAllReady, float Percentage)
        {
            var LCPromotion = LoyaltyCardServices.Instance.GetLoyaltyCardPromotion(ID);
            LCPromotion.Percentage = Percentage;
            LCPromotion.Services = ServiceListAllReady;
            LCPromotion.Percentage = Percentage;
            LoyaltyCardServices.Instance.UpdateLoyaltyCardPromotion(LCPromotion);
            return Json(new { success = true, JsonRequestBehavior.AllowGet });
        }


        [HttpGet]
        public ActionResult Action(int ID = 0)
        {
            LoyaltyCardActionViewModel model = new LoyaltyCardActionViewModel();
            var LoggedInUser = UserManager.FindById(User.Identity.GetUserId());
            if (LoggedInUser.Role == "Super Admin")
            {
                model.Services = ServiceServices.Instance.GetService().Where(x => x.IsActive).ToList();
                model.Percentages = LoyaltyCardServices.Instance.GetLoyaltyCardPromotions().Select(x => x.Percentage).ToList();

            }
            else
            {
                model.Percentages = LoyaltyCardServices.Instance.GetLoyaltyCardPromotions().Where(x => x.Business == LoggedInUser.Company).Select(x => x.Percentage).ToList();
                model.Services = ServiceServices.Instance.GetService().Where(x => x.Business == LoggedInUser.Company && x.IsActive).ToList();

            }
            if (ID != 0)
            {
                var loyaltyCard = LoyaltyCardServices.Instance.GetLoyaltyCard(ID);
                model.ID = loyaltyCard.ID;
                model.Name = loyaltyCard.Name;
                model.Days = loyaltyCard.Days;
                model.IsActive = loyaltyCard.IsActive;
                var AlreadyIncludedServices = new List<int>();
                var listofLoyaltyCardPromotionsFinal = new List<LoyaltyCardPromotionActionModel>();
                var loyaltyCardPromotions = LoyaltyCardServices.Instance.GetLoyaltyCardPromotions(loyaltyCard.ID);
                foreach (var item in loyaltyCardPromotions)
                {
                    if (item.Services != "")
                    {
                        var ListOfServices = new List<Service>();
                        foreach (var servicetobeadd in item.Services.Split(',').ToList())
                        {
                            if (int.TryParse(servicetobeadd, out var parsedValue))
                            {
                                var service = ServiceServices.Instance.GetService(parsedValue);
                                ListOfServices.Add(service);
                                AlreadyIncludedServices.Add(service.ID);
                            }
                        }
                        listofLoyaltyCardPromotionsFinal.Add(new LoyaltyCardPromotionActionModel
                        {
                            LoyaltyCardPromotionID = item.ID,
                            Percentage = item.Percentage,
                            Services = ListOfServices
                            
                        });
                    }
                    else
                    {
                        listofLoyaltyCardPromotionsFinal.Add(new LoyaltyCardPromotionActionModel
                        {
                            LoyaltyCardPromotionID = item.ID,
                            Percentage = item.Percentage,
                            Services = null
                        });
                    }
                }
                model.AlreadyIncludedServices = AlreadyIncludedServices;
                model.LoyaltyCardPromotions = listofLoyaltyCardPromotionsFinal;
            }
            return View(model);
        }


        [HttpPost]
        public JsonResult DeletePromotion(int LoyaltyPromotionID)
        {
            LoyaltyCardServices.Instance.DeleteLoyaltyCardPromotion(LoyaltyPromotionID);
            return Json(new {success=true},JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult Action(LoyaltyCardActionViewModel model)
        {
            var LoggedInUser = UserManager.FindById(User.Identity.GetUserId());
            
            if (model.ID != 0)
            {
                var LoyaltyCard = LoyaltyCardServices.Instance.GetLoyaltyCard(model.ID);
       
                LoyaltyCard.ID = model.ID;
                LoyaltyCard.IsActive = model.IsActive;
                LoyaltyCard.Name = model.Name;
                LoyaltyCard.Days = model.Days;
                LoyaltyCard.Date = DateTime.Now;
                LoyaltyCardServices.Instance.UpdateLoyaltyCard(LoyaltyCard);

                var history = new History();
                history.Date = DateTime.Now;
                history.Note = "Loyalty Card was updated "+ LoyaltyCard.Name + " by: " + LoggedInUser.Name;
                history.Business = LoggedInUser.Company;
                history.CustomerName = "";
                history.EmployeeName = "";
                HistoryServices.Instance.SaveHistory(history);


                var loyaltycardPromotions = LoyaltyCardServices.Instance.GetLoyaltyCardPromotions(LoyaltyCard.ID);
                foreach (var item in loyaltycardPromotions)
                {
                    LoyaltyCardServices.Instance.DeleteLoyaltyCardPromotion(item.ID);
                }

                var LoyaltyCardPromotion = model.ServiceListAlready.Split('|').ToList();
                foreach (var item in LoyaltyCardPromotion)
                {

                    var percentage = item.Split('_')[1].Replace("%", "").Trim();
                    var serviceInts = item.Split('_')[0];
                    var LCPromotion = new LoyaltyCardPromotion();
                    LCPromotion.Business = LoggedInUser.Company;
                    LCPromotion.Date = DateTime.Now;
                    LCPromotion.LoyaltyCardID = LoyaltyCard.ID;
                    LCPromotion.Percentage = float.Parse(percentage);
                    LCPromotion.Services = serviceInts;

                    LoyaltyCardServices.Instance.SaveLoyaltyCardPromotion(LCPromotion);
                }




            }
            else
            {
                var LoyaltyCard = new LoyaltyCard();
                LoyaltyCard.Business = LoggedInUser.Company;
                LoyaltyCard.IsActive = model.IsActive;
                LoyaltyCard.Name = model.Name;
                LoyaltyCard.Date = DateTime.Now;

                LoyaltyCard.Days = model.Days;
                LoyaltyCardServices.Instance.SaveLoyaltyCard(LoyaltyCard);

                var history = new History();
                history.Date = DateTime.Now;
                history.Note = "Loyalty Card was saved: " + LoyaltyCard.Name + " by: " + LoggedInUser.Name;
                history.Business = LoggedInUser.Company;
                history.CustomerName = "";
                history.EmployeeName = "";
                HistoryServices.Instance.SaveHistory(history);

                var LoyaltyCardPromotion = model.ServiceListAlready.Split('|').ToList();
                foreach (var item in LoyaltyCardPromotion)
                {

                    var percentage = item.Split('_')[1].Replace("%","").Trim();
                    var serviceInts = item.Split('_')[0];
                    var LCPromotion = new LoyaltyCardPromotion();
                    LCPromotion.Business = LoggedInUser.Company;
                    LCPromotion.LoyaltyCardID = LoyaltyCard.ID;
                    LCPromotion.Percentage = float.Parse(percentage);
                    LCPromotion.Services = serviceInts;
                    LCPromotion.Date = DateTime.Now;
                    LoyaltyCardServices.Instance.SaveLoyaltyCardPromotion(LCPromotion);

                    var newhistory = new History();
                    newhistory.Date = DateTime.Now;
                    newhistory.Note = "Loyalty Card Promotions was saved for: " + LoyaltyCard.Name + " by: " + LoggedInUser.Name;
                    newhistory.Business = LoggedInUser.Company;
                    newhistory.CustomerName = "";
                    newhistory.EmployeeName = "";
                    HistoryServices.Instance.SaveHistory(newhistory);
                }




            }
            return Json(new { success = true }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetLoyaltyCards(string SearchTerm = "")
        {
            var LoggedInUser = UserManager.FindById(User.Identity.GetUserId());
            if (LoggedInUser.Role != "Super Admin")
            {
                var LoyaltyCardFinalList = new List<LoyaltyCardModel>();
                var loyaltyCards = LoyaltyCardServices.Instance.GetLoyaltyCard(SearchTerm).Where(x => x.Business == LoggedInUser.Company).ToList();
                foreach (var item in loyaltyCards)
                {
                    var dataByFloatValue = new Dictionary<float, (int, List<Service>)>();
                    var loyaltyCardPromotions = LoyaltyCardServices.Instance.GetLoyaltyCardPromotions(item.ID);
                    foreach (var promotion in loyaltyCardPromotions)
                    {
                        int loyalPromotionId = promotion.ID;
                        var servicesInts = promotion.Services.Split(',').ToList();
                        float percentage = promotion.Percentage;
                        var FinalServiceList = new List<Service>();
                        foreach (var service in servicesInts)
                        {
                            FinalServiceList.Add(ServiceServices.Instance.GetService(int.Parse(service)));
                        }   
                        List<Service> services = FinalServiceList;

                        // Check if the percentage is already in the dictionary
                        if (dataByFloatValue.ContainsKey(percentage))
                        {
                            var existingData = dataByFloatValue[percentage];
                            // Concatenate the services to the existing percentage group
                            var updatedData = (existingData.Item1 + loyalPromotionId, existingData.Item2.Concat(services).ToList());
                            dataByFloatValue[percentage] = updatedData;
                        }
                        else
                        {
                            // Create a new percentage group and add the services
                            var newData = (loyalPromotionId, new List<Service>(services));
                            dataByFloatValue[percentage] = newData;
                        }
                    }


                    LoyaltyCardFinalList.Add(new LoyaltyCardModel { LoyaltyCard = item, LoyaltyCardPromotions = dataByFloatValue });
                }
                return Json(LoyaltyCardFinalList, JsonRequestBehavior.AllowGet);
            }
            else
            {
                var LoyaltyCardFinalList = new List<LoyaltyCardModel>();
                var loyaltyCards = LoyaltyCardServices.Instance.GetLoyaltyCard(SearchTerm);
                foreach (var item in loyaltyCards)
                {
                    var dataByFloatValue = new Dictionary<float, (int, List<Service>)>();
                    var loyaltyCardPromotions = LoyaltyCardServices.Instance.GetLoyaltyCardPromotions(item.ID);
                    foreach (var promotion in loyaltyCardPromotions)
                    {
                        int loyalPromotionId = promotion.ID;
                        var servicesInts = promotion.Services.Split(',').ToList();
                        float percentage = promotion.Percentage;
                        var FinalServiceList = new List<Service>();
                        foreach (var service in servicesInts)
                        {
                            FinalServiceList.Add(ServiceServices.Instance.GetService(int.Parse(service)));
                        }
                        List<Service> services = FinalServiceList;

                        // Check if the percentage is already in the dictionary
                        if (dataByFloatValue.ContainsKey(percentage))
                        {
                            var existingData = dataByFloatValue[percentage];
                            // Concatenate the services to the existing percentage group
                            var updatedData = (existingData.Item1 + loyalPromotionId, existingData.Item2.Concat(services).ToList());
                            dataByFloatValue[percentage] = updatedData;
                        }
                        else
                        {
                            // Create a new percentage group and add the services
                            var newData = (loyalPromotionId, new List<Service>(services));
                            dataByFloatValue[percentage] = newData;
                        }
                    }


                    LoyaltyCardFinalList.Add(new LoyaltyCardModel { LoyaltyCard = item, LoyaltyCardPromotions = dataByFloatValue });
                }
                return Json(LoyaltyCardFinalList, JsonRequestBehavior.AllowGet);
            }

        }


        [HttpGet]
        public ActionResult Issue(int CustomerID = 0)
        {
            LoyaltyCardIssueViewModel model = new LoyaltyCardIssueViewModel();
            var LoggedInUser = UserManager.FindById(User.Identity.GetUserId());
            if (LoggedInUser.Role == "Super Admin")
            {
                model.Customers = CustomerServices.Instance.GetCustomer();
                model.LoyaltyCards = LoyaltyCardServices.Instance.GetLoyaltyCard();
            }
            else
            {
                model.Customers = CustomerServices.Instance.GetCustomer().Where(x => x.Business == LoggedInUser.Company).ToList();
                model.LoyaltyCards = LoyaltyCardServices.Instance.GetLoyaltyCard().Where(x => x.Business == LoggedInUser.Company).ToList(); 


            }
            model.Customer = CustomerID;
            return View(model);
        }


        [HttpPost]
        public ActionResult CheckLoyaltyCardData(int CustomerID, string LoyaltyCardNumber,string ServicesData)
        {
            var customer = CustomerServices.Instance.GetCustomer(CustomerID);
            var loyaltyCardAssignment = LoyaltyCardServices.Instance.GetLoyaltyCardAssignments()
                  .Where(x => x.CustomerID == customer.ID && x.CardNumber == LoyaltyCardNumber
                  && x.Business == customer.Business).FirstOrDefault();
            if (loyaltyCardAssignment != null)
            {
                var loyaltycardpromotions = LoyaltyCardServices.Instance.GetLoyaltyCardPromotions().Where(x => x.LoyaltyCardID == loyaltyCardAssignment.LoyaltyCardID && x.Business == loyaltyCardAssignment.Business).ToList();
                var ListOfCashBackPercentages = new List<float>();
                foreach (var item in loyaltycardpromotions)
                {
                    int[] array1 = item.Services.Split(',').Select(int.Parse).ToArray();
                    int[] array2 = ServicesData.Split(',').Select(int.Parse).ToArray();
                    if(array1.Intersect(array2).ToList().Count() > 0)
                    {
                        ListOfCashBackPercentages.Add(item.Percentage);
                    }


                }
                return Json(new { success = true, LoyaltyCardAssignment = loyaltyCardAssignment,ListOfCashBackPercentages= ListOfCashBackPercentages.FirstOrDefault() }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(new { success = false, LoyaltyCardAssignment = loyaltyCardAssignment }, JsonRequestBehavior.AllowGet);

            }
        }


        [HttpPost]
        public ActionResult Issue(int Customer,string CardNumber,int LoyaltyCardID,float CashBack)
        {
            var loyaltyCard = LoyaltyCardServices.Instance.GetLoyaltyCard(LoyaltyCardID);
            var customer = CustomerServices.Instance.GetCustomer(Customer);
            var LCAssignment = new LoyaltyCardAssignment();
            LCAssignment.Business = customer.Business;
            LCAssignment.CardNumber = CardNumber;
            LCAssignment.CustomerID = Customer;
            LCAssignment.CashBack = CashBack;
            LCAssignment.Days = loyaltyCard.Days;
            LCAssignment.LoyaltyCardID = LoyaltyCardID;
            LCAssignment.Date = DateTime.Now;
            LoyaltyCardServices.Instance.SaveLoyaltyCardAssignment(LCAssignment);


            var log = new History();
            log.CustomerName = customer.FirstName +" "+customer.LastName;
            log.Business = customer.Business;
            log.Date = DateTime.Now;
            log.Note = "Loyalty Card Assigned to "+customer.FirstName +" "+customer.LastName + " having card number: "+CardNumber;
            HistoryServices.Instance.SaveHistory(log);


            #region MailingRegion
            var LoggedInUser = UserManager.FindById(User.Identity.GetUserId());
            var company = CompanyServices.Instance.GetCompany(LoggedInUser.Company).FirstOrDefault();
           
            var emailDetails = EmailTemplateServices.Instance.GetEmailTemplate().Where(x=>x.Name == "Loyalty Card Issue Notification" && x.Business == LoggedInUser.Company).FirstOrDefault();
            if (emailDetails != null && emailDetails.IsActive == true)
            {
                string emailBody = "<html><body>";
                emailBody += "<h2 style='font-family: Arial, sans-serif;'>Loyalty Card Issue</h2>";
                emailBody += emailDetails.TemplateCode;
                emailBody = emailBody.Replace("{{Customer_first_name}}", customer.FirstName);
                emailBody = emailBody.Replace("{{Customer_last_name}}", customer.LastName);
                emailBody = emailBody.Replace("{{Customer_initial}}", customer.Gender == "Male" ? "Mr." : "Ms.");
                emailBody = emailBody.Replace("{{Loyalty_Card_Number}}", CardNumber);
                emailBody = emailBody.Replace("{{date}}", DateTime.Now.ToString("yyyy-MM-dd"));
                emailBody = emailBody.Replace("{{time}}", DateTime.Now.ToString("H:mm:ss"));
                emailBody = emailBody.Replace("{{company_name}}", LoggedInUser.Company);
                emailBody = emailBody.Replace("{{company_email}}", company.NotificationEmail);
                emailBody = emailBody.Replace("{{company_address}}", company.Address);
                emailBody = emailBody.Replace("{{loyaltycard_expiry_day}}", (LCAssignment.Date.AddDays(loyaltyCard.Days) - DateTime.Now).Days + " Days");
                emailBody = emailBody.Replace("{{company_logo}}", $"<img class='text-center' style='height:50px;width:auto;' src='{"http://app.yourbookingplatform.com" + company.Logo}'>");
                emailBody = emailBody.Replace("{{company_phone}}", company.PhoneNumber);

                emailBody += "</body></html>";
                SendEmail(customer.Email, "Loyalty Card Issue", emailBody);
            }
            #endregion

            return Json(new {success=true},JsonRequestBehavior.AllowGet);
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
                mail.ReplyTo = new MailAddress(company.NotificationEmail); // Set the ReplyTo address

                mail.BodyEncoding = System.Text.Encoding.UTF8;
                mail.IsBodyHtml = true;
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
            //try
            //{
            //    string senderEmail = "Support@yourbookingplatform.com";
            //    string senderPassword = "Yourbookingplatform_001";


            //    //string senderEmail = "hello@ezoncs.com";
            //    //string senderPassword = "Ezoncs_001";
            //    int Port = int.Parse(ConfigurationManager.AppSettings["portforSmtp"]);
            //    string Host = ConfigurationManager.AppSettings["hostForSmtp"];

            //    SmtpClient client = new SmtpClient();
            //    client.Host = Host;
            //    client.Port = Port;
            //    client.EnableSsl = false;
            //    client.DeliveryMethod = SmtpDeliveryMethod.Network;
            //    client.UseDefaultCredentials = false;
            //    client.Credentials = new NetworkCredential(senderEmail, senderPassword, "yourbookingplatform.com");

            //    var loggedInUser = UserManager.FindById(User.Identity.GetUserId());
            //    var company = CompanyServices.Instance.GetCompany().Where(x => x.Business == loggedInUser.Company).FirstOrDefault();
            //    MailAddress ccAddress = new MailAddress(company.NotificationEmail, loggedInUser.Company);

            //    MailMessage mailMessage = new MailMessage(senderEmail, toEmail, subject, emailBody);
            //    mailMessage.IsBodyHtml = true;

            //    mailMessage.To.Add(toEmail);
            //    //mailMessage.CC.Add(ccAddress);
            //    mailMessage.ReplyToList.Add(company.NotificationEmail);
            //    mailMessage.BodyEncoding = UTF8Encoding.UTF8;

            //    client.Send(mailMessage);



            //    return true;
            //}
            //catch (Exception ex)
            //{
            //    Session["EmailStatus"] = ex.ToString();
            //    return false;
            //}

        }


        [HttpGet]
        public ActionResult Delete(int ID)
        {
            LoyaltyCardActionViewModel model = new LoyaltyCardActionViewModel();
            var LoyaltyCard = LoyaltyCardServices.Instance.GetLoyaltyCard(ID);
            model.ID = LoyaltyCard.ID;
            
            

            return PartialView("_Delete", model);
        }


        [HttpGet]
        public ActionResult DeleteAssignment(int ID)
        {
            LoyaltyCardActionViewModel model = new LoyaltyCardActionViewModel();
            model.AssignmentID = ID;
            return PartialView("_DeleteAssignment", model);
        }



        [HttpPost]
        public ActionResult Delete(LoyaltyCardActionViewModel model)
        {
            var LoyaltyCard = LoyaltyCardServices.Instance.GetLoyaltyCard(model.ID);
            var loyaltyCardPromotions = LoyaltyCardServices.Instance.GetLoyaltyCardPromotions(LoyaltyCard.ID);
            foreach (var item in loyaltyCardPromotions)
            {
                LoyaltyCardServices.Instance.DeleteLoyaltyCardPromotion(item.ID);
            }
            var message = LoyaltyCardServices.Instance.DeleteLoyaltyCard(LoyaltyCard.ID);
            if (message == "Deleted Successfully")
            {
                var log = new History();
                log.Business = LoyaltyCard.Business;
                log.Date = DateTime.Now;
                log.Note = "Loyalty Card was deleted along with the promotions";
                HistoryServices.Instance.SaveHistory(log);
            }
            return Json(new { success = true, Message = message }, JsonRequestBehavior.AllowGet);



        }



        [HttpPost]
        public ActionResult DeleteAssignment(LoyaltyCardActionViewModel model)
        {
          
            LoyaltyCardServices.Instance.DeleteLoyaltyCardAssignment(model.AssignmentID);

            return Json(new { success = true }, JsonRequestBehavior.AllowGet);
        }


        [HttpGet]
        public ActionResult CardAssignmentAction(int ID)
        {
            LoyaltyCardAssignmentActionViewModel model = new LoyaltyCardAssignmentActionViewModel();
            var assignment = LoyaltyCardServices.Instance.GetLoyaltyCardAssignment(ID);
            var loyaltycard = LoyaltyCardServices.Instance.GetLoyaltyCard(assignment.LoyaltyCardID);
            model.Customer = CustomerServices.Instance.GetCustomersWRTBusiness(loyaltycard.Business, "");
            model.LoyaltyCard = loyaltycard;
            model.ID = assignment.ID;
            model.CustomerID = assignment.CustomerID;
            model.LoyaltyCardID = assignment.LoyaltyCardID;
            model.CardNumber = assignment.CardNumber;
            model.Days = assignment.Days;
            model.CashBack = assignment.CashBack;
            model.Date = assignment.Date;
            return View(model);

        }

        [HttpPost]
        public JsonResult LoyaltyCardAssignmentAction(LoyaltyCardAssignmentActionViewModel model)
        {
            var assignment = LoyaltyCardServices.Instance.GetLoyaltyCardAssignment(model.ID);
            assignment.CashBack = model.CashBack;
            assignment.Days = model.Days;
            assignment.CustomerID = model.CustomerID;
            LoyaltyCardServices.Instance.UpdateLoyaltyCardAssignment(assignment);
            return Json(new { success = true }, JsonRequestBehavior.AllowGet);
        }


        [HttpGet]
        public ActionResult CardAssignmentIndex()
        {
            LoyaltyCardAssignmentViewModel model = new LoyaltyCardAssignmentViewModel();
            var LoggedInUser = UserManager.FindById(User.Identity.GetUserId());
            var ListOfFinalCardAssignments = new List<LoyaltyCardAssignmentModel>();
            if (LoggedInUser.Role == "Super Admin")
            {
                var LCAs = LoyaltyCardServices.Instance.GetLoyaltyCardAssignments();
                foreach (var item in LCAs)
                {
                    var loyaltyCard = LoyaltyCardServices.Instance.GetLoyaltyCard(item.LoyaltyCardID);
                    var customer = CustomerServices.Instance.GetCustomer(item.CustomerID);
                    var loyaltyCardAssignment = LCAs.Where(x=>x.CustomerID == item.CustomerID).FirstOrDefault();
                    float CashBack = 0;
                    if(loyaltyCardAssignment != null)
                    {
                        CashBack = loyaltyCardAssignment.CashBack;
                    }
                    ListOfFinalCardAssignments.Add(new LoyaltyCardAssignmentModel { LoyaltyCardDays = loyaltyCard.Days, LoyaltyCardName = loyaltyCard.Name, Customer = customer, LoyaltyCardAssignment = item,LoyaltyCardUsage = CashBack });
                }
            }
            else
            {
                var LCAs = LoyaltyCardServices.Instance.GetLoyaltyCardAssignments().Where(x => x.Business == LoggedInUser.Company).ToList();
                foreach (var item in LCAs)
                {
                    var loyaltyCard = LoyaltyCardServices.Instance.GetLoyaltyCard(item.LoyaltyCardID);
                    var customer = CustomerServices.Instance.GetCustomer(item.CustomerID);
                    float CashBack = 0;
                    var loyaltyCardAssignment = LCAs.Where(x => x.CustomerID == item.CustomerID).FirstOrDefault();
                    if (loyaltyCardAssignment != null)
                    {
                        CashBack = loyaltyCardAssignment.CashBack;
                    }
                    ListOfFinalCardAssignments.Add(new LoyaltyCardAssignmentModel {LoyaltyCardDays = loyaltyCard.Days, LoyaltyCardName = loyaltyCard.Name, Customer = customer, LoyaltyCardAssignment = item, LoyaltyCardUsage = CashBack });
                }

            }
            model.LoyaltyCardAssignments = ListOfFinalCardAssignments;
            return View(model);
        }
    }
}