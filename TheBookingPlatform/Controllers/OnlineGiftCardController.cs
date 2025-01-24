using Stripe.Checkout;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Mail;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;
using TheBookingPlatform.Entities;
using TheBookingPlatform.Services;
using TheBookingPlatform.ViewModels;
using Microsoft.AspNet.Identity;
using Stripe;

namespace TheBookingPlatform.Controllers
{
    public class OnlineGiftCardController : Controller
    {
        // GET: OnlineGiftCard
        public ActionResult Index(string businessName)
        {
            OnlineGiftCardViewModel model = new OnlineGiftCardViewModel();
            model.Company = CompanyServices.Instance.GetCompany().Where(x => x.Business == businessName).FirstOrDefault();
            if (model.Company.PaymentMethodIntegration)
            {
                model.GiftCard = GiftCardServices.Instance.GetGiftCard().Where(x => x.Business == businessName).FirstOrDefault();
                return View("Index", "_GiftCardLayout", model);
            }
            else
            {
                return RedirectToAction("NotFound", "OnlineGiftCard");
            }
        }

        public ActionResult NotFound()
        {
            return View();
        }

        public float GetNumericPartFromString(string input)
        {
            // Regular expression to match numbers in the string
            Regex regex = new Regex(@"\d+");

            // Match the regular expression with the input string
            Match match = regex.Match(input);

            if (match.Success)
            {
                if (float.TryParse(match.Value, out float result))
                {
                    return result;
                }
            }

            return 0.0f; // If no number found or parsing fails, return 0.0
        }


        public ActionResult NotPurchased(string businessName)
        {
            OnlineGiftCardViewModel model = new OnlineGiftCardViewModel();
            model.businessName = businessName;
            return View("NotPurchased", "_GiftCardLayout", model);
        }

        public ActionResult Purchased(string businessName, int CustomerID,string CardName,float Amount)
        {
            var customer = CustomerServices.Instance.GetCustomer(CustomerID);
            var giftCard = GiftCardServices.Instance.GetGiftCard().Where(x=>x.Name == CardName.Trim() && x.Business == businessName).FirstOrDefault(); 
            var newAssignment = new GiftCardAssignment();
            newAssignment.Business = businessName;
            newAssignment.CustomerID = customer.ID;
            newAssignment.AssignedDate = DateTime.Now;
            newAssignment.AssignedAmount = Amount;
            newAssignment.AssignedCode = giftCard.Code + "-" + customer.ID + "-" + DateTime.Now.ToString("dd") + DateTime.Now.ToString("MM")+DateTime.Now.ToString("HH")+DateTime.Now.ToString("mm") + DateTime.Now.ToString("ss") + DateTime.Now.ToString("yyyy");
            newAssignment.Balance = Amount;
            newAssignment.GiftCardID = giftCard.ID;
            newAssignment.Days = giftCard.Days;
            GiftCardServices.Instance.SaveGiftCardAssignment(newAssignment);

            var history = new History();
            history.Name = "Gift Card Purchased";
            history.Note = "Gift card was purchased of ID: " + giftCard.ID + " by Customer:" + customer.FirstName + " " + customer.LastName;
            history.Business = businessName;
            history.Date  = DateTime.Now;
            history.CustomerName = customer.FirstName + " " + customer.LastName;
            history.Type = giftCard.ID + "GiftCard";
            HistoryServices.Instance.SaveHistory(history);

            var company = CompanyServices.Instance.GetCompany().Where(x=>x.Business == businessName).FirstOrDefault();
            var emailDetails = EmailTemplateServices.Instance.GetEmailTemplate().Where(x => x.Name == "Gift Card Issue Notification" && x.Business == businessName).FirstOrDefault();
            if (emailDetails != null && emailDetails.IsActive == true)
            {
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
                emailBody = emailBody.Replace("{{giftcard_code}}", newAssignment.AssignedCode);
                emailBody = emailBody.Replace("{{giftcard_name}}", giftCard.Name);
                emailBody = emailBody.Replace("{{giftcard_expiry_days}}", (giftCard.Date.AddDays(giftCard.Days) - DateTime.Now).Days + " Days");
                emailBody = emailBody.Replace("{{company_logo}}", $"<img class='text-center' style='height:50px;width:auto;' src='{"http://app.yourbookingplatform.com" + company.Logo}'>");
                emailBody = emailBody.Replace("{{company_phone}}", company.PhoneNumber);
                emailBody = emailBody.Replace("{{password}}", customer.Password);

                emailBody += "</body></html>";


                if (IsValidEmail(customer.Email))
                {
                    SendEmail(customer.Email, "Gift Card Assignment", emailBody, company);
                }
            }
            ViewBag.GiftCardCode = newAssignment.AssignedCode;
            return View("Purchased", "_GiftCardLayout");
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


        public bool SendEmail(string toEmail, string subject, string emailBody, Company company)
        {
            try
            {
                string senderEmail = "support@yourbookingplatform.com";
                string senderPassword = "ttpa fcbl mpbn fxdl";
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
         
        }

        [HttpPost]
        public ActionResult BuyGiftCard(string CardName,string Amount,string Email,string MobileNumber,string FirstName, string LastName,string Company)
        {
            var company = CompanyServices.Instance.GetCompany().Where(x => x.Business == Company).FirstOrDefault();
            var secretKey = company.APIKEY;
            StripeConfiguration.ApiKey = secretKey;

            var customer = CustomerServices.Instance.GetCustomerWRTBusiness(Company, Email);
        
            Random random = new Random();
            customer.Password = new string(Enumerable.Repeat("abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789", 8).Select(s => s[random.Next(s.Length)]).ToArray());
            
            if (customer == null)
            {
                customer.DateAdded = DateTime.Now;
                customer.Business = Company;
                customer.FirstName = FirstName;
                customer.LastName = LastName;
                customer.Email = Email;
                customer.MobileNumber = MobileNumber;
                CustomerServices.Instance.SaveCustomer(customer);
            }
          

            float totalAmount = GetNumericPartFromString(Amount);
            var options = new SessionCreateOptions
            {
                PaymentMethodTypes = new List<string> { "card", "ideal" },
                LineItems = new List<SessionLineItemOptions>(),
                Mode = "payment", // You can use "subscription" for subscriptions.
                // You can set the success and cancel URLs for redirection after payment.
                SuccessUrl = "http://app.yourbookingplatform.com" + Url.Action("Purchased", "OnlineGiftCard", new {businessName = Company,CustomerID = customer.ID,CardName=CardName,Amount= totalAmount }),
                CancelUrl = "http://app.yourbookingplatform.com" + Url.Action("NotPurchased", "OnlineGiftCard", new { businessName = Company }),
            };
            var lineItems = new List<SessionLineItemOptions>
            {
                // Add a separate line item for the total amount.
                new SessionLineItemOptions
                {
                    Quantity = 1,
                    PriceData = new SessionLineItemPriceDataOptions
                    {
                        Currency = "eur",
                        UnitAmount = long.Parse(ConvertEuroToCents(totalAmount).ToString()),
                        ProductData = new SessionLineItemPriceDataProductDataOptions
                        {
                            Name = CardName,
                            Description = "Gift Card for: "+Company
                        },
                    }
                }
            };

            options.LineItems = lineItems;
            var serviceSession = new SessionService();
            Session session = serviceSession.Create(options);


         


            
            return Json(new {success=true, session = session.Url }, JsonRequestBehavior.AllowGet);
        }

        public int ConvertEuroToCents(float euroAmount)
        {
            // Convert euros to cents
            int centsAmount = (int)(euroAmount * 100);
            return centsAmount;
        }
    }
}