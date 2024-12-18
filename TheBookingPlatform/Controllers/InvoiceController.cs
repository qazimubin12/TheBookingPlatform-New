using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using OfficeOpenXml.FormulaParsing.Excel.Functions.Finance.Implementations;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Mail;
using System.Net;
using System.Text;
using System.Web;
using System.Web.Mvc;
using TheBookingPlatform.Database.Migrations;
using TheBookingPlatform.Entities;
using TheBookingPlatform.Services;

namespace TheBookingPlatform.Controllers
{
    public class InvoiceController : Controller
    {
        // GET: Invoice

        private AMSignInManager _signInManager;
        private AMRolesManager _rolesManager;
        private AMUserManager _userManager;
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
        [HttpPost]
        public JsonResult SaveInvoice(string CustomerID, string EmployeeID, string AppointmentID, string PaymentMethod, string GrandTotal, string FinalGrandTotal, string Notes, string Referral = "", string Code = "")
        {
            var Customer = CustomerServices.Instance.GetCustomer(int.Parse(CustomerID));
            if (PaymentMethod == "Referral")
            {
                Customer.ReferralBalance = float.Parse(Referral);
                CustomerServices.Instance.UpdateCustomer(Customer);
            }
            var random = new Random();
            var LoggedInUser = UserManager.FindById(User.Identity.GetUserId());
            var invoice = new Entities.Invoice();
            invoice.CustomerID = int.Parse(CustomerID);
            invoice.AppointmentID = int.Parse(AppointmentID);
            invoice.PaymentMethod = PaymentMethod;
            invoice.EmployeeID = int.Parse(EmployeeID);
            invoice.GrandTotal = float.Parse(GrandTotal);
            invoice.Business = LoggedInUser.Company;
            invoice.OrderDate = DateTime.Now;
            invoice.Remarks = Notes;
            invoice.OrderNo = "OR" + DateTime.Now.ToString("dd-MM-yyyy") + random.Next(100, 1000) + "DER";
            InvoiceServices.Instance.SaveInvoice(invoice);
            var appointment = AppointmentServices.Instance.GetAppointment(invoice.AppointmentID);
            appointment.Status = "Paid";
            appointment.Color = "#5DAF4D";

            AppointmentServices.Instance.UpdateAppointment(appointment);

            if (PaymentMethod == "Referral")
            {

            }



            var review = new Entities.Review();
            review.Business = LoggedInUser.Company;
            review.AppointmentID = appointment.ID;
            review.CustomerID = int.Parse(CustomerID);
            review.Date = DateTime.Now;
            review.EmployeeID = int.Parse(EmployeeID);
            review.Rating = 0;
            ReviewServices.Instance.SaveReview(review);


            float RecievedCashBack = 0;

            if (Customer != null)
            {
                var lcAssignment = LoyaltyCardServices.Instance.GetLoyaltyCardAssignmentByCustomerID(Customer.ID);

                if (lcAssignment != null)
                {

                    float CashBack = lcAssignment.CashBack;
                    var LoyaltyCard = LoyaltyCardServices.Instance.GetLoyaltyCard(lcAssignment.LoyaltyCardID);
                    if (LoyaltyCard.IsActive)
                    {
                        var loyaltycardpromotions = LoyaltyCardServices.Instance.GetLoyaltyCardPromotions(LoyaltyCard.ID);
                        foreach (var promotion in loyaltycardpromotions)
                        {
                            var ServicesAppliedFor = promotion.Services.Split(',').ToList();
                            foreach (var service in ServicesAppliedFor)
                            {
                                if (appointment.Service.Split(',').ToList().Contains(service))
                                {
                                    float price = ServiceServices.Instance.GetService(int.Parse(service)).Price;
                                    float percentage = promotion.Percentage;
                                    CashBack += price * (percentage / 100);
                                    RecievedCashBack += price * (percentage / 100);
                                    lcAssignment.CashBack = CashBack;
                                    LoyaltyCardServices.Instance.UpdateLoyaltyCardAssignment(lcAssignment);
                                }
                            }
                        }

                        if (lcAssignment.CashBack > 0)
                        {
                            var company = CompanyServices.Instance.GetCompany().Where(x => x.Business == Customer.Business).FirstOrDefault();
                            var emailDetails = EmailTemplateServices.Instance.GetEmailTemplate().Where(x => x.Name == "Loyalty Card Increase Notification" && x.Business == company.Business).FirstOrDefault();
                            if (emailDetails != null && emailDetails.IsActive == true)
                            {
                                string emailBody = "<html><body>";
                                emailBody += "<h2 style='font-family: Arial, sans-serif;'>Loyalty Card Increase Notification</h2>";
                                emailBody += emailDetails.TemplateCode;
                                emailBody = emailBody.Replace("{{Customer_first_name}}", Customer.FirstName);
                                emailBody = emailBody.Replace("{{Customer_last_name}}", Customer.LastName);
                                emailBody = emailBody.Replace("{{loyalty_card}}", LoyaltyCard.Name);
                                emailBody = emailBody.Replace("{{loyalty_card_Number}}", lcAssignment.CardNumber);
                                emailBody = emailBody.Replace("{{loyalty_card_bonus}}", RecievedCashBack.ToString());
                                emailBody = emailBody.Replace("{{loyalty_card_balance}}", lcAssignment.CashBack.ToString());
                                emailBody = emailBody.Replace("{{Customer_initial}}", "Dear");
                                emailBody = emailBody.Replace("{{date}}", appointment.Date.ToString("yyyy-MM-dd"));
                                emailBody = emailBody.Replace("{{time}}", appointment.Time.ToString("H:mm:ss"));
                                emailBody = emailBody.Replace("{{end_time}}", appointment.EndTime.ToString("H:mm:ss"));
                                emailBody = emailBody.Replace("{{company_name}}", company.Business);
                                emailBody = emailBody.Replace("{{company_email}}", company.NotificationEmail);
                                emailBody = emailBody.Replace("{{company_address}}", company.Address);
                                emailBody = emailBody.Replace("{{company_logo}}", $"<img class='text-center' style='height:50px;width:auto;' src='{"http://app.yourbookingplatform.com" + company.Logo}'>");
                                emailBody = emailBody.Replace("{{company_phone}}", company.PhoneNumber);
                                emailBody += "</body></html>";
                                if (IsValidEmail(Customer.Email))
                                {
                                    SendEmail(Customer.Email, "Loyalty Card Increase Notification", emailBody, company);
                                }
                            }
                            var history = new History();
                            history.Business = Customer.Business;
                            history.CustomerName = Customer.FirstName + " " + Customer.LastName;
                            history.Date = DateTime.Now;
                            history.AppointmentID = invoice.AppointmentID;
                            history.Name = "Loyalty Card Cash Back";
                            history.Note = "Received Loyalty Card Cash Back  " + RecievedCashBack + " Total Cash Back is:  " + lcAssignment.CashBack + " Received having Loyalty Card:" + LoyaltyCard.Name;
                            history.Type = lcAssignment.LoyaltyCardID + "LoyaltyCard";
                            HistoryServices.Instance.SaveHistory(history);
                        }


                    }
                }
            }

            if (Code != "" && PaymentMethod == "Gift Card")
            {
                string ConcatenatedServices = "";
                foreach (var item in appointment.Service.Split(',').ToList())
                {
                    var ServiceNew = ServiceServices.Instance.GetService(int.Parse(item));
                    if (ServiceNew != null)
                    {

                        if (ConcatenatedServices == "")
                        {
                            ConcatenatedServices = String.Join(",", ServiceNew.Name);
                        }
                        else
                        {
                            ConcatenatedServices = String.Join(",", ConcatenatedServices, ServiceNew.Name);
                        }
                    }
                }

                var giftCardSupposedtoBeCode = Code.Split('-')[0];
                var giftCard = GiftCardServices.Instance.GetGiftCard().Where(x => x.Code == giftCardSupposedtoBeCode).FirstOrDefault();
                if (giftCard != null)
                {
                    var giftCardAssignment = GiftCardServices.Instance.GetGiftCardAssignmentByGiftCardID(giftCard.ID).Where(x => x.CustomerID == Customer.ID).FirstOrDefault();

                    if (giftCardAssignment != null)
                    {
                        float Amountused = 0;
                        if ((giftCardAssignment.AssignedDate.AddDays(giftCardAssignment.Days) - DateTime.Now).Days <= 0)
                        {

                        }
                        else
                        {
                            if (giftCardAssignment.Balance > 0)
                            {

                                if (giftCardAssignment.Balance > float.Parse(FinalGrandTotal))
                                {
                                    giftCardAssignment.Balance -= float.Parse(FinalGrandTotal);
                                    Amountused = float.Parse(FinalGrandTotal);
                                    GiftCardServices.Instance.UpdateGiftCardAssignment(giftCardAssignment);
                                }
                                else
                                {
                                    Amountused = giftCardAssignment.Balance;
                                    giftCardAssignment.Balance = 0;
                                    GiftCardServices.Instance.UpdateGiftCardAssignment(giftCardAssignment);
                                }


                                var history = new History();
                                history.Business = Customer.Business;
                                history.CustomerName = Customer.FirstName + " " + Customer.LastName;
                                history.Date = DateTime.Now;
                                history.AppointmentID = invoice.AppointmentID;
                                history.Name = "Gift Card Used";
                                history.Note = "Gift Card was used, Card Name:" + giftCard.Name + "Amount used: " + Amountused + " Services: " + ConcatenatedServices;
                                history.Type = giftCard.ID + "GiftCard";
                                HistoryServices.Instance.SaveHistory(history);
                            }
                        }
                    }
                }
            }


            if (PaymentMethod == "Loyalty Card")
            {
                string ConcatenatedServices = "";
                foreach (var item in appointment.Service.Split(',').ToList())
                {
                    var ServiceNew = ServiceServices.Instance.GetService(int.Parse(item));
                    if (ServiceNew != null)
                    {

                        if (ConcatenatedServices == "")
                        {
                            ConcatenatedServices = String.Join(",", ServiceNew.Name);
                        }
                        else
                        {
                            ConcatenatedServices = String.Join(",", ConcatenatedServices, ServiceNew.Name);
                        }
                    }
                }


                var LcAssignment = LoyaltyCardServices.Instance.GetLoyaltyCardAssignmentByCustomerID(Customer.ID);
                if (LcAssignment != null)
                {
                    float Amountused = 0;
                    if (LcAssignment.CashBack > 0)
                    {

                        if (LcAssignment.CashBack > float.Parse(FinalGrandTotal))
                        {
                            LcAssignment.CashBack -= float.Parse(FinalGrandTotal);
                            Amountused = float.Parse(FinalGrandTotal);
                            LoyaltyCardServices.Instance.UpdateLoyaltyCardAssignment(LcAssignment);
                        }
                        else
                        {
                            Amountused = LcAssignment.CashBack;
                            LcAssignment.CashBack = 0;
                            LoyaltyCardServices.Instance.UpdateLoyaltyCardAssignment(LcAssignment);
                        }

                        var loyaltyCard = LoyaltyCardServices.Instance.GetLoyaltyCard().Where(x => x.ID == LcAssignment.LoyaltyCardID).FirstOrDefault();
                        var company = CompanyServices.Instance.GetCompany().Where(x => x.Business == Customer.Business).FirstOrDefault();
                        var emailDetails = EmailTemplateServices.Instance.GetEmailTemplate().Where(x => x.Name == "Loyalty Card Decrease Notification" && x.Business == company.Business).FirstOrDefault();
                        if (emailDetails != null && emailDetails.IsActive == true)
                        {
                            string emailBody = "<html><body>";
                            emailBody += "<h2 style='font-family: Arial, sans-serif;'>Loyalty Card Decrease Notification</h2>";
                            emailBody += emailDetails.TemplateCode;
                            emailBody = emailBody.Replace("{{Customer_first_name}}", Customer.FirstName);
                            emailBody = emailBody.Replace("{{Customer_last_name}}", Customer.LastName);
                            emailBody = emailBody.Replace("{{loyalty_card}}", loyaltyCard.Name);
                            emailBody = emailBody.Replace("{{loyalty_card_Number}}", LcAssignment.CardNumber);
                            emailBody = emailBody.Replace("{{loyalty_card_bonus}}", Amountused.ToString());
                            emailBody = emailBody.Replace("{{loyalty_card_balance}}", LcAssignment.CashBack.ToString());
                            emailBody = emailBody.Replace("{{Customer_initial}}", "Dear");
                            emailBody = emailBody.Replace("{{date}}", appointment.Date.ToString("yyyy-MM-dd"));
                            emailBody = emailBody.Replace("{{time}}", appointment.Time.ToString("H:mm:ss"));
                            emailBody = emailBody.Replace("{{end_time}}", appointment.EndTime.ToString("H:mm:ss"));
                            emailBody = emailBody.Replace("{{company_name}}", company.Business);
                            emailBody = emailBody.Replace("{{company_email}}", company.NotificationEmail);
                            emailBody = emailBody.Replace("{{company_address}}", company.Address);
                            emailBody = emailBody.Replace("{{company_logo}}", $"<img class='text-center' style='height:50px;width:auto;' src='{"http://app.yourbookingplatform.com" + company.Logo}'>");
                            emailBody = emailBody.Replace("{{company_phone}}", company.PhoneNumber);
                            emailBody += "</body></html>";
                            if (IsValidEmail(Customer.Email))
                            {
                                SendEmail(Customer.Email, "Loyalty Card Decrease Notification", emailBody, company);
                            }
                        }
                        var history = new History();
                        history.Business = Customer.Business;
                        history.CustomerName = Customer.FirstName + " " + Customer.LastName;
                        history.Date = DateTime.Now;
                        history.Note = "Loyalty Card was used, Card Number:" + LcAssignment.CardNumber + "Amount used: " + Amountused + " Services: " + ConcatenatedServices;
                        history.AppointmentID = invoice.AppointmentID;
                        history.Name = "Loyalty Card Cash Back";
                        history.Type = LcAssignment.ID + "LoyaltyCard";
                        HistoryServices.Instance.SaveHistory(history);
                    }
                }





            }
            return Json(new { success = true, InvoiceID = invoice.ID }, JsonRequestBehavior.AllowGet);

        }

        [HttpGet]
        public JsonResult CheckGiftCard(int CustomerID, string GiftCardCode)
        {

            //var giftCardSupposedtoBeCode = GiftCardCode.Split('-')[0];
            var giftCard = GiftCardServices.Instance.GetGiftCardAssignment().Where(x => x.AssignedCode == GiftCardCode).FirstOrDefault();
            if (giftCard != null)
            {
                var customer = CustomerServices.Instance.GetCustomer(CustomerID);
                var giftCardAssignment = GiftCardServices.Instance.GetGiftCardAssignmentByGiftCardID(giftCard.GiftCardID).Where(x=>x.CustomerID == customer.ID).FirstOrDefault();

                if(giftCardAssignment != null)
                {
                    if ((giftCardAssignment.AssignedDate.AddDays(giftCardAssignment.Days) - DateTime.Now).Days <= 0)
                    {

                        return Json(new { success = false, Message = "Gift Card has been expired" }, JsonRequestBehavior.AllowGet);

                    }
                    else
                    {
                        if (giftCardAssignment.Balance > 0)
                        {
                            return Json(new { success = true, Amount = giftCardAssignment.Balance }, JsonRequestBehavior.AllowGet);

                        }
                        else
                        {
                            return Json(new { success = false, Message = "Insufficient Balance in Gift Card" }, JsonRequestBehavior.AllowGet);

                        }
                    }
                }
                else
                {
                    return Json(new { success = false, Message = "Gift Card Is not assigned to this customer" }, JsonRequestBehavior.AllowGet);
                }
            }
            else
            {
                return Json(new {success=false,Message="No Gift Card Found"},JsonRequestBehavior.AllowGet);
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



        [HttpGet]
        public JsonResult CheckLoyaltyCard(int CustomerID, string LoyaltyCardCode)
        {
          
            var customer = CustomerServices.Instance.GetCustomer(CustomerID);
            var lcAssignment = LoyaltyCardServices.Instance.GetLoyaltyCardAssignments().Where(x => x.CustomerID == customer.ID && x.CardNumber == LoyaltyCardCode).FirstOrDefault();
            if (lcAssignment != null)
            {
                if (lcAssignment.CashBack > 0)
                {
                    if ((lcAssignment.Date.AddDays(lcAssignment.Days) - DateTime.Now).Days <= 0)
                    {
                        return Json(new { success = false, Message = "Loyalty Card has been expired" }, JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        return Json(new { success = true, Amount = lcAssignment.CashBack }, JsonRequestBehavior.AllowGet);
                    }

                }
                else
                {
                    return Json(new { success = false, Message = "Insufficient Balance in Loyalty Card" }, JsonRequestBehavior.AllowGet);

                }
            }
            else
            {
                return Json(new { success = false, Message = "Loyalty Card Is not assigned to this customer" }, JsonRequestBehavior.AllowGet);
            }
            
         
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
                MailAddress ccAddress = new MailAddress(company.NotificationEmail, loggedInUser.Company);

                mail.CC.Add(ccAddress);
                mail.From = new MailAddress(company.NotificationEmail, loggedInUser.Company, System.Text.Encoding.UTF8);
                mail.Subject = subject;
                mail.SubjectEncoding = System.Text.Encoding.UTF8;
                mail.Body = emailBody;
                mail.BodyEncoding = System.Text.Encoding.UTF8;
                mail.ReplyTo = new MailAddress(company.NotificationEmail); // Set the ReplyTo address

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

            //    MailAddress ccAddress = new MailAddress(company.NotificationEmail, company.Business);

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


        [HttpPost]
        public ActionResult SendInvoiceToCustomer(int ID)
        {
            var appointment = AppointmentServices.Instance.GetAppointment(ID);
            var customer = CustomerServices.Instance.GetCustomer(appointment.CustomerID);

            if (customer.Password == null)
            {
                Random random = new Random();
                customer.Password = new string(Enumerable.Repeat("abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789", 8).Select(s => s[random.Next(s.Length)]).ToArray());
                CustomerServices.Instance.UpdateCustomer(customer);
            }
            var employee = EmployeeServices.Instance.GetEmployee(appointment.EmployeeID);
            var company = CompanyServices.Instance.GetCompany().Where(x=>x.Business ==  appointment.Business).FirstOrDefault();
            string ConcatenatedServices = "";
            foreach (var item in appointment.Service.Split(',').ToList())
            {
                var ServiceNew = ServiceServices.Instance.GetService(int.Parse(item));
                if (ServiceNew != null)
                {

                    if (ConcatenatedServices == "")
                    {
                        ConcatenatedServices = String.Join(",", ServiceNew.Name);
                    }
                    else
                    {
                        ConcatenatedServices = String.Join(",", ConcatenatedServices, ServiceNew.Name);
                    }
                }
            }
            var emailDetails = EmailTemplateServices.Instance.GetEmailTemplate().Where(x => x.Name == "Payment invoice" && x.Business == company.Business).FirstOrDefault();
            if (emailDetails != null && emailDetails.IsActive == true)
            {
                string emailBody = "<html><body>";
                emailBody += "<h2 style='font-family: Arial, sans-serif;'>Payment Invoice</h2>";
                emailBody += emailDetails.TemplateCode;
                emailBody = emailBody.Replace("{{Customer_first_name}}", customer.FirstName);
                emailBody = emailBody.Replace("{{Customer_last_name}}", customer.LastName);
                emailBody = emailBody.Replace("{{Customer_initial}}", "Dear");
                emailBody = emailBody.Replace("{{date}}", appointment.Date.ToString("yyyy-MM-dd"));
                emailBody = emailBody.Replace("{{time}}", appointment.Time.ToString("H:mm:ss"));
                emailBody = emailBody.Replace("{{end_time}}", appointment.EndTime.ToString("H:mm:ss"));
                emailBody = emailBody.Replace("{{employee}}", employee.Name);
                emailBody = emailBody.Replace("{{employee_picture}}", $"<img class='text-center' style='height:50px;width:auto;' src='{"https://app.yourbookingplatform.com" + employee.Photo}'>");
                emailBody = emailBody.Replace("{{services}}", ConcatenatedServices);
                emailBody = emailBody.Replace("{{company_name}}", company.Business);
                emailBody = emailBody.Replace("{{company_email}}", company.NotificationEmail);
                emailBody = emailBody.Replace("{{company_address}}", company.Address);
                emailBody = emailBody.Replace("{{company_logo}}", $"<img class='text-center' style='height:50px;width:auto;' src='{"https://app.yourbookingplatform.com" + company.Logo}'>");
                emailBody = emailBody.Replace("{{company_phone}}", company.PhoneNumber);
                emailBody = emailBody.Replace("{{password}}", customer.Password);
                string Link = "https://app.yourbookingplatform.com" + Url.Action("Invoice", "Appointment", new { ID = ID });
                emailBody = emailBody.Replace("{{Invoice_link}}", Link);
                emailBody += "</body></html>";

                if (IsValidEmail(customer.Email))
                {
                    SendEmail(customer.Email, "Payment Invoice", emailBody, company);
                }
            }
            return Json(new { success = true }, JsonRequestBehavior.AllowGet);

        }
    }
}