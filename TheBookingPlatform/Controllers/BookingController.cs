using Microsoft.AspNet.Identity;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json;
using RestSharp;
using Stripe;
using Stripe.Checkout;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity.Core.Common.CommandTrees.ExpressionBuilder;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;

using System.Web.Mvc;
using System.Web.Routing;

using TheBookingPlatform.Entities;
using TheBookingPlatform.Services;
using TheBookingPlatform.ViewModels;
using static TheBookingPlatform.Controllers.AppointmentController;
using Customer = TheBookingPlatform.Entities.Customer;
using MailMessage = System.Net.Mail.MailMessage;
using Service = TheBookingPlatform.Entities.Service;
using Event = TheBookingPlatform.Controllers.AppointmentController.Event;
using System.Threading.Tasks;
using static Microsoft.IO.RecyclableMemoryStreamManager;
using System.Web.UI.WebControls;
using Microsoft.Owin.Security.Twitter.Messages;
using System.Net.Http;
using static TheBookingPlatform.Controllers.BookingController;
using System.Net.Http.Headers;
using System.Text;
using System.Security.Cryptography;
using NodaTime;
using TheBookingPlatform.Models;

namespace TheBookingPlatform.Controllers
{
    public class BookingController : Controller
    {

        [HttpGet]
        public JsonResult CheckCouponCode(string Business, string CustomerEmail, string CouponCode,string FirstName,string LastName,string MobileNumber)
        {
            var customer = CustomerServices.Instance.GetCustomerWRTBusiness(Business, CustomerEmail);
            if(customer == null)
            {
                customer = new Customer();
                Random random = new Random();
                customer.Password = new string(Enumerable.Repeat("abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789", 8).Select(s => s[random.Next(s.Length)]).ToArray());
                customer.FirstName = FirstName;
                customer.LastName = LastName;
                customer.MobileNumber = MobileNumber;
                customer.DateOfBirth = DateTime.Now;
                customer.DateAdded = DateTime.Now;
                customer.Business = Business;
                customer.Email = CustomerEmail;
                CustomerServices.Instance.SaveCustomer(customer);
            }
            if (customer != null)
            {
                if (CouponCode != "")
                {
                    var coupon = CouponServices.Instance.GetCoupon().Where(x => x.CouponCode.Trim() == CouponCode).FirstOrDefault();
                    if (coupon.IsDisabled)
                    {
                        return Json(new { success = false, Message = "Coupon has been expired." }, JsonRequestBehavior.AllowGet);

                    }
                    else
                    {
                        var couponassignment = CouponServices.Instance.GetCouponAssignmentsWRTBusiness(Business).Where(x => x.CustomerID == customer.ID && x.CouponID == coupon.ID).FirstOrDefault();
                        if (couponassignment != null)
                        {
                            if (coupon.UsageCount <= couponassignment.Used)
                            {
                                return Json(new { success = false, Message = "The coupon has been used to its maximum limit.." }, JsonRequestBehavior.AllowGet);
                            }
                            else if (coupon.ExpiryDate.Date <= DateTime.Now.Date)
                            {
                                return Json(new { success = false, Message = "Coupon has been expired." }, JsonRequestBehavior.AllowGet);
                            }
                            else
                            {
                                return Json(new { success = true, Message = "Coupon Applied", CouponID = coupon.ID, CouponAssignmentID = couponassignment.ID, Percentage = coupon.Discount }, JsonRequestBehavior.AllowGet);

                            }
                        }
                        else
                        {
                            couponassignment = new CouponAssignment();
                            couponassignment.AssignedDate = DateTime.Now;
                            couponassignment.Business = Business;
                            couponassignment.IsSentToClient = true;
                            couponassignment.Used = 0;
                            couponassignment.CouponID = coupon.ID;
                            couponassignment.CustomerID = customer.ID;
                            CouponServices.Instance.SaveCouponAssignment(couponassignment);

                            return Json(new { success = true, Message = "Coupon Applied", CouponID = coupon.ID, CouponAssignmentID = couponassignment.ID, Percentage = coupon.Discount }, JsonRequestBehavior.AllowGet);
                        }
                    }
                }
                else
                {
                    return Json(new { success = false, Message = "Failed to retrieve the coupon." }, JsonRequestBehavior.AllowGet);

                }

            }
            else
            {
                return Json(new { success = false, Message = "Failed to retrieve the customer." }, JsonRequestBehavior.AllowGet);
            }
        }

        public class EventNew
        {
            public string Id { get; set; }
            public string Summary { get; set; }
            public DateTime Start { get; set; }
            public DateTime End { get; set; }
            // Add other relevant event properties as needed
        }

        public class GoogleEvent
        {
            public string Kind { get; set; }
            public string Etag { get; set; }
            public string Id { get; set; }
            public string Status { get; set; }
            public string HtmlLink { get; set; }
            public DateTime? StartDate { get; set; }
            public DateTime? EndDate { get; set; }
        }

        public class NEvent
        {
            public string Kind { get; set; }
            public string Etag { get; set; }
            public string Id { get; set; }
            public string Status { get; set; }
            public string HtmlLink { get; set; }
            public DateTime Created { get; set; }
            public DateTime Updated { get; set; }
            public string Summary { get; set; }
            public string Description { get; set; }
            public string Location { get; set; }
            public string ColorId { get; set; }
            public Creator Creator { get; set; }
            public Organizer Organizer { get; set; }
            public NEventDateTime Start { get; set; }
            public NEventDateTime End { get; set; }
            public bool EndTimeUnspecified { get; set; }
            public List<string> Recurrence { get; set; }
            public string RecurringEventId { get; set; }
            public NEventDateTime OriginalStartTime { get; set; }
            public string Transparency { get; set; }
            public string Visibility { get; set; }
            public string ICalUID { get; set; }
            public int Sequence { get; set; }
            public List<Attendee> Attendees { get; set; }
            public bool AttendeesOmitted { get; set; }
            public ExtendedProperties ExtendedProperties { get; set; }
            public string HangoutLink { get; set; }
            public ConferenceData ConferenceData { get; set; }
            public Gadget Gadget { get; set; }
            public bool AnyoneCanAddSelf { get; set; }
            public bool GuestsCanInviteOthers { get; set; }
            public bool GuestsCanModify { get; set; }
            public bool GuestsCanSeeOtherGuests { get; set; }
            public bool PrivateCopy { get; set; }
            public bool Locked { get; set; }
            public Reminders Reminders { get; set; }
            public Source Source { get; set; }
            public WorkingLocationProperties WorkingLocationProperties { get; set; }
            public OutOfOfficeProperties OutOfOfficeProperties { get; set; }
            public FocusTimeProperties FocusTimeProperties { get; set; }
            public List<Attachment> Attachments { get; set; }
            public string EventType { get; set; }
        }

        public class Creator
        {
            public string Id { get; set; }
            public string Email { get; set; }
            public string DisplayName { get; set; }
            public bool Self { get; set; }
        }

        public class Organizer
        {
            public string Id { get; set; }
            public string Email { get; set; }
            public string DisplayName { get; set; }
            public bool Self { get; set; }
        }

        public class NEventDateTime
        {
            public string Date { get; set; }
            public string DateTime { get; set; }
            public string TimeZone { get; set; }
        }

        public class Attendee
        {
            public string Id { get; set; }
            public string Email { get; set; }
            public string DisplayName { get; set; }
            public bool Organizer { get; set; }
            public bool Self { get; set; }
            public bool Resource { get; set; }
            public bool Optional { get; set; }
            public string ResponseStatus { get; set; }
            public string Comment { get; set; }
            public int AdditionalGuests { get; set; }
        }

        public class ExtendedProperties
        {
            public Dictionary<string, string> Private { get; set; }
            public Dictionary<string, string> Shared { get; set; }
        }

        public class ConferenceData
        {
            public CreateRequest CreateRequest { get; set; }
            public List<EntryPoint> EntryPoints { get; set; }
            public ConferenceSolution ConferenceSolution { get; set; }
            public string ConferenceId { get; set; }
            public string Signature { get; set; }
            public string Notes { get; set; }
        }

        public class CreateRequest
        {
            public string RequestId { get; set; }
            public ConferenceSolutionKey ConferenceSolutionKey { get; set; }
            public Status Status { get; set; }
        }

        public class ConferenceSolutionKey
        {
            public string Type { get; set; }
        }

        public class Status
        {
            public string StatusCode { get; set; }
        }

        public class EntryPoint
        {
            public string EntryPointType { get; set; }
            public string Uri { get; set; }
            public string Label { get; set; }
            public string Pin { get; set; }
            public string AccessCode { get; set; }
            public string MeetingCode { get; set; }
            public string Passcode { get; set; }
            public string Password { get; set; }
        }

        public class ConferenceSolution
        {
            public ConferenceSolutionKey Key { get; set; }
            public string Name { get; set; }
            public string IconUri { get; set; }
        }

        public class Gadget
        {
            public string Type { get; set; }
            public string Title { get; set; }
            public string Link { get; set; }
            public string IconLink { get; set; }
            public int Width { get; set; }
            public int Height { get; set; }
            public string Display { get; set; }
            public Dictionary<string, string> Preferences { get; set; }
        }

        public class Reminders
        {
            public bool UseDefault { get; set; }
            public List<ReminderOverride> Overrides { get; set; }
        }

        public class ReminderOverride
        {
            public string Method { get; set; }
            public int Minutes { get; set; }
        }

        public class Source
        {
            public string Url { get; set; }
            public string Title { get; set; }
        }

        public class WorkingLocationProperties
        {
            public string Type { get; set; }
            public object HomeOffice { get; set; }
            public CustomLocation CustomLocation { get; set; }
            public OfficeLocation OfficeLocation { get; set; }
        }

        public class CustomLocation
        {
            public string Label { get; set; }
        }

        public class OfficeLocation
        {
            public string BuildingId { get; set; }
            public string FloorId { get; set; }
            public string FloorSectionId { get; set; }
            public string DeskId { get; set; }
            public string Label { get; set; }
        }

        public class OutOfOfficeProperties
        {
            public string AutoDeclineMode { get; set; }
            public string DeclineMessage { get; set; }
        }

        public class FocusTimeProperties
        {
            public string AutoDeclineMode { get; set; }
            public string DeclineMessage { get; set; }
            public string ChatStatus { get; set; }
        }

        public class Attachment
        {
            public string FileUrl { get; set; }
            public string Title { get; set; }
            public string MimeType { get; set; }
            public string IconLink { get; set; }
            public string FileId { get; set; }
        }



        public static NEvent GetEvent(string apiKey, string calendarId, string eventId)
        {
            var baseUrl = "https://www.googleapis.com/calendar/v3/calendars/";
            var url = $"{baseUrl}{calendarId}/events/{eventId}";

            var client = new RestClient(baseUrl);
            var request = new RestRequest(url, Method.Get);

            request.AddParameter("key", apiKey, ParameterType.QueryString);

            var response = client.Execute(request);

            if (response.StatusCode == HttpStatusCode.OK)
            {
                NEvent calendarEvent = JsonConvert.DeserializeObject<NEvent>(response.Content);
                return calendarEvent;
            }
            else
            {
                var history = new History
                {
                    Date = DateTime.Now,
                    Note = $"Error retrieving event: {response.StatusCode} - {response.Content}",
                    Type = "Error",
                    Business = "Error"
                };
                HistoryServices.Instance.SaveHistory(history);
                throw new Exception($"Error retrieving event: {response.StatusCode} - {response.Content}");
            }
        }
        public static GCalendarEventsRoot GetEvents(string apiKey, string calendarId, DateTime updatedMin)
        {
            var baseUrl = "https://www.googleapis.com/calendar/v3/calendars/";
            var url = $"{baseUrl}{calendarId}/events";

            var client = new RestClient(baseUrl);
            var request = new RestRequest(url, Method.Get);

            request.AddParameter("key", apiKey, ParameterType.QueryString);
            request.AddParameter("updatedMin", updatedMin.ToString("o"), ParameterType.QueryString);  // Use 'o' format for DateTime

            var response = client.Execute<List<EventNew>>(request);

            if (response.StatusCode == HttpStatusCode.OK)
            {

                GCalendarEventsRoot calendar = JsonConvert.DeserializeObject<GCalendarEventsRoot>(response.Content);

                return calendar;
            }
            else
            {
                //var history = new History();
                //history.Date = DateTime.Now;
                //history.Note = "Error retrieving events:" + response.StatusCode + "-" + response.Content;
                //history.Business = "QASIM";
                //HistoryServices.Instance.SaveHistory(history);
                throw new Exception($"Error retrieving events: {response.StatusCode} - {response.Content}");
            }
        }

        public class GCalendarEvent
        {
            public string Kind { get; set; }
            public string Etag { get; set; }
            public string Id { get; set; }
            public string Status { get; set; }
            public string HtmlLink { get; set; }
            public DateTime? Created { get; set; }
            public DateTime? Updated { get; set; }
            public string Summary { get; set; }
            public GEventCreator Creator { get; set; }
            public GEventOrganizer Organizer { get; set; }
            public GEventDateTime Start { get; set; }
            public GEventDateTime End { get; set; }
            public string ICalUID { get; set; }
            public int Sequence { get; set; }
            public string EventType { get; set; }
        }

        public class GEventCreator
        {
            public string Email { get; set; }
        }

        public class GEventOrganizer
        {
            public string Email { get; set; }
            public string DisplayName { get; set; }
            public bool? Self { get; set; }
        }

        public class GEventDateTime
        {
            public DateTime? DateTime { get; set; }
            public string TimeZone { get; set; }
        }

        public class GCalendarEventsRoot
        {
            public string Kind { get; set; }
            public string Etag { get; set; }
            public string Summary { get; set; }
            public string Description { get; set; }
            public string Updated { get; set; }
            public string TimeZone { get; set; }
            public string AccessRole { get; set; }
            public List<GCalendarEvent> Items { get; set; }
        }



        static string GetCalendarId(string input)
        {
            // Split the string by ';'
            string[] parts = input.Split(';');

            // Loop through each part to find the X-Goog-Resource-URI
            foreach (string part in parts)
            {
                if (part.Contains("X-Goog-Resource-URI"))
                {
                    // Split the part by '=' to extract the URL
                    string[] keyValue = part.Split('=');
                    if (keyValue.Length == 3)
                    {
                        string url = keyValue[1];

                        // Now extract the calendar ID from the URL
                        string[] urlParts = url.Split('/');
                        if (urlParts.Length > 6)
                        {
                            string calendarId = urlParts[6]; // calendar ID will be at index 6
                            return calendarId;
                        }
                    }
                    else if (keyValue.Length == 2)
                    {
                        string url = keyValue[1];

                        // Now extract the calendar ID from the URL
                        string[] urlParts = url.Split('/');
                        if (urlParts.Length > 6)
                        {
                            string calendarId = urlParts[6]; // calendar ID will be at index 6
                            return calendarId;
                        }
                    }
                }
            }

            // Return null if not found
            return null;
        }

        private void SaveHistoryError(string note, Exception ex, Appointment appointment = null)
        {
            var nhistory = new History();
            nhistory.Date = DateTime.Now;
            nhistory.Note = note + " - " + ex.Message;
            nhistory.Type = "Error";
            if (appointment != null)
            {
                nhistory.Type = JsonConvert.SerializeObject(appointment);
            }
            nhistory.Business = "Error";
            HistoryServices.Instance.SaveHistory(nhistory);
        }


        [Route("stripe/webhook")]
        [HttpPost]
        public async Task<string> Webhook()
        {
            var json = new StreamReader(Request.InputStream).ReadToEnd();

            // Parse the JSON string into a JObject
            JObject jsonObject = JObject.Parse(json);
            var AppointmentID = 0;
            string Status = "";
            // Safely extract and parse AppointmentID
            int appointmentId;
            if (int.TryParse(jsonObject["data"]?["object"]?["metadata"]?["AppointmentID"]?.ToString(), out appointmentId))
            {
                AppointmentID = appointmentId;
            }
            
            // Extract status
            string status = jsonObject["data"]?["object"]?["status"]?.ToString();
            if (!string.IsNullOrEmpty(status))
            {
                Status = status;
            }
            var findAppointment = AppointmentServices.Instance.GetAppointment(AppointmentID);
            var service = new PaymentIntentService();
            var appointment = AppointmentServices.Instance.GetAppointment(findAppointment.ID);
            var company = CompanyServices.Instance.GetCompany(appointment.Business).FirstOrDefault();

            var customer = CustomerServices.Instance.GetCustomer(appointment.CustomerID);
            var employee = EmployeeServices.Instance.GetEmployee(appointment.EmployeeID);
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

            if (Status == "succeeded")
            {
                // Payment was successful
                appointment.IsPaid = true;
                appointment.IsCancelled = false;
                AppointmentServices.Instance.UpdateAppointment(appointment);

                var reminder = ReminderServices.Instance.GetReminderWRTAppID(appointment.ID);
                reminder.IsCancelled = false;
                reminder.Paid = true;
                ReminderServices.Instance.UpdateReminder(reminder);

                var history = new History();
                history.EmployeeName = employee.Name;
                history.CustomerName = customer.FirstName + " " + customer.LastName;
                history.Note = "Online Appointment was paid for " + history.CustomerName + "";
                history.Date = DateTime.Now;
                history.Business = appointment.Business;
                history.Type = "General";
                HistoryServices.Instance.SaveHistory(history);


                #region ConfirmationEmailSender
                var EmailTemplate = "";
                if (!AppointmentServices.Instance.IsNewCustomer(company.Business, appointment.CustomerID))
                {
                    EmailTemplate = "First Registration Email";
                }
                else
                {
                    EmailTemplate = "Appointment Confirmation";
                }





                string emailBody = "<html><body>";
                var emailTemplate = EmailTemplateServices.Instance.GetEmailTemplateWRTBusiness(appointment.Business, EmailTemplate);
                if (emailTemplate != null)
                {
                    emailBody += "<h2 style='font-family: Arial, sans-serif;'>" + EmailTemplate + "</h2>";
                    emailBody += emailTemplate.TemplateCode;
                    emailBody = emailBody.Replace("{{Customer_first_name}}", customer.FirstName);
                    emailBody = emailBody.Replace("{{Customer_last_name}}", customer.LastName);
                    emailBody = emailBody.Replace("{{Customer_initial}}", "Dear");
                    emailBody = emailBody.Replace("{{date}}", appointment.Date.ToString("yyyy-MM-dd"));
                    emailBody = emailBody.Replace("{{time}}", appointment.Time.ToString("H:mm:ss"));
                    emailBody = emailBody.Replace("{{end_time}}", appointment.EndTime.ToString("H:mm:ss"));
                    emailBody = emailBody.Replace("{{employee}}", employee.Name);
                    emailBody = emailBody.Replace("{{employee_specialization}}", employee.Specialization);
                    emailBody = emailBody.Replace("{{employee_picture}}", $"<img class='text-center' style='height:50px;width:auto;' src='{"http://app.yourbookingplatform.com" + employee.Photo}'>");

                    emailBody = emailBody.Replace("{{services}}", ConcatenatedServices);
                    emailBody = emailBody.Replace("{{company_name}}", company.Business);
                    emailBody = emailBody.Replace("{{company_email}}", company.NotificationEmail);
                    emailBody = emailBody.Replace("{{company_address}}", company.Address);
                    emailBody = emailBody.Replace("{{company_logo}}", $"<img class='text-center' style='height:50px;width:auto;' src='{"http://app.yourbookingplatform.com" + company.Logo}'>");
                    emailBody = emailBody.Replace("{{company_phone}}", company.PhoneNumber);
                    emailBody = emailBody.Replace("{{password}}", customer.Password);
                    string cancelLink = string.Format("http://app.yourbookingplatform.com/Appointment/CancelByEmail/?AppointmentID={0}", appointment.ID);
                    emailBody = emailBody.Replace("{{cancellink}}", $"<a href='{cancelLink}' class='btn btn-primary'>CANCEL/RESCHEDULE</a>");
                    emailBody += "</body></html>";
                    SendEmail(customer.Email, EmailTemplate, emailBody, company);
                }
                #endregion


            }
            else
            {
                // Payment was not successful
                appointment.IsPaid = false;
                appointment.IsCancelled = true;
                AppointmentServices.Instance.UpdateAppointment(appointment);

                var failedAppointment = new FailedAppointment();
                failedAppointment.AppointmentID = appointment.ID;
                failedAppointment.Failed = true;
                FailedAppointmentServices.Instance.SaveFailedAppointment(failedAppointment);


                if (appointment.CouponID != 0)
                {
                    var couponAssignment = CouponServices.Instance.GetCouponAssignmentsWRTBusinessAndCouponID(appointment.Business, appointment.CouponID, appointment.CustomerID);
                    if (couponAssignment != null)
                    {
                        couponAssignment.Used -= 1;
                        CouponServices.Instance.UpdateCouponAssignment(couponAssignment);
                    }
                }

                var reminder = ReminderServices.Instance.GetReminderWRTAppID(appointment.ID);
                reminder.IsCancelled = true;
                ReminderServices.Instance.UpdateReminder(reminder);

                var googleCalendar = GoogleCalendarServices.Instance.GetGoogleCalendarServicesWRTBusiness(company.Business);
                if (googleCalendar != null && !googleCalendar.Disabled)
                {
                    try
                    {
                        var url = new System.Uri("https://www.googleapis.com/calendar/v3/calendars/" + employee.GoogleCalendarID + "/events/" + appointment.GoogleCalendarEventID);
                        var restClient = new RestClient(url);
                        var request = new RestRequest();

                        request.AddQueryParameter("key", "AIzaSyASKpY6I08IVKFMw3muX39uMzPc5sBDaSc");
                        request.AddHeader("Authorization", "Bearer " + googleCalendar.AccessToken);
                        request.AddHeader("Accept", "application/json");
                        var response = restClient.Delete(request);
                        if (response.StatusCode == System.Net.HttpStatusCode.NoContent)
                        {
                            var history = new History();
                            history.Date = DateTime.Now;
                            history.Note = "Appointment got deleted from GCalendar";
                            history.Business = appointment.Business;
                            HistoryServices.Instance.SaveHistory(history);
                        }
                        else
                        {

                            var history = new History();
                            history.Date = DateTime.Now;
                            history.Note = response.Content;
                            history.Business = "Error";
                            HistoryServices.Instance.SaveHistory(history);
                        }
                    }
                    catch (Exception ex)
                    {

                        var history = new History();
                        history.Date = DateTime.Now;
                        history.Note = ex.Message;
                        history.Business = "Error";
                        HistoryServices.Instance.SaveHistory(history);
                    }



                }
              

                #region ReminderMailingRegion

                //var employee = EmployeeServices.Instance.GetEmployee(appointment.EmployeeID);

                if (customer.Password == null)
                {
                    Random random = new Random();
                    customer.Password = new string(Enumerable.Repeat("abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789", 8).Select(s => s[random.Next(s.Length)]).ToArray());
                    CustomerServices.Instance.UpdateCustomer(customer);
                }

                var emailDetails = EmailTemplateServices.Instance.GetEmailTemplateWRTBusiness(company.Business, "Appointment Payment Reminder");
                if (emailDetails != null && emailDetails.IsActive == true)
                {
                    string emailBody = "<html><body>";
                    emailBody += "<h2 style='font-family: Arial, sans-serif;'>Appointment Payment Reminder</h2>";
                    emailBody += emailDetails.TemplateCode;
                    emailBody = emailBody.Replace("{{Customer_first_name}}", customer.FirstName);
                    //emailBody = emailBody.Replace("{{Customer_last_name}}", customer.LastName);
                    //emailBody = emailBody.Replace("{{Customer_initial}}", "Dear");
                    emailBody = emailBody.Replace("{{date}}", appointment.Date.ToString("yyyy-MM-dd"));
                    emailBody = emailBody.Replace("{{time}}", appointment.Time.ToString("H:mm:ss"));
                    //emailBody = emailBody.Replace("{{end_time}}", appointment.EndTime.ToString("H:mm:ss"));
                    //emailBody = emailBody.Replace("{{employee}}", employee.Name);
                    //emailBody = emailBody.Replace("{{employee_picture}}", $"<img class='text-center' style='height:50px;width:auto;' src='{"http://app.yourbookingplatform.com" + employee.Photo}'>");
                    emailBody = emailBody.Replace("{{services}}", ConcatenatedServices);
                    emailBody = emailBody.Replace("{{company_name}}", company.Business);
                    emailBody = emailBody.Replace("{{company_email}}", company.NotificationEmail);
                    emailBody = emailBody.Replace("{{company_address}}", company.Address);
                    emailBody = emailBody.Replace("{{company_logo}}", $"<img class='text-center' style='height:50px;width:auto;' src='{"http://app.yourbookingplatform.com" + company.Logo}'>");
                    emailBody = emailBody.Replace("{{company_phone}}", company.PhoneNumber);
                    emailBody = emailBody.Replace("{{password}}", customer.Password);

                    emailBody += "</body></html>";


                    if (IsValidEmail(customer.Email))
                    {
                        SendEmail(customer.Email, "Appointment Payment Awaiting", emailBody, company);
                    }

                }
                #endregion

            }
            return "True";

        }



        [Route("google/webhook")]
        [HttpPost]
        public async Task<ActionResult> TestingGoogleToYBP()
        {

            


            string jsonContent = string.Empty;
            string contentLength = Request.ContentLength.ToString();
            string contentType = Request.ContentType;
            string headers = string.Empty;
            try
            {
                headers = string.Join("; ", Request.Headers.AllKeys.Select(key => key + "=" + Request.Headers[key]));
            }
            catch (Exception ex)
            {
                SaveHistoryError("Error extracting headers", ex);
            }

            var input = $"Headers: {headers}\nContent Length: {contentLength}\nContent Type: {contentType}\nJSON Content: {jsonContent}";
            var history = new History();
            history.Date = DateTime.Now;
            history.Note = jsonContent + contentLength + contentType + headers;
            string uri = string.Empty;
            try
            {
                uri = GetCalendarId(input); //this is me getting CalendarID from response
            }
            catch (Exception ex)
            {
                SaveHistoryError("Error extracting Calendar ID", ex);
            }



            Employee employee = null;
            try
            {
                employee = EmployeeServices.Instance.GetEmployeeWithLinkedGoogleCalendarID(uri);

                

            }
            catch (Exception ex)
            {
                SaveHistoryError("Error fetching employee with Calendar ID", ex);
            }


            Company company = null;
            try
            {
                company = CompanyServices.Instance.GetCompanyByName(employee?.Business);
            }
            catch (Exception ex)
            {
                SaveHistoryError("Error fetching company by employee business", ex);
            }


            GoogleCalendarIntegration googleCalendar = null;
            try
            {
                googleCalendar = GoogleCalendarServices.Instance.GetGoogleCalendarServicesWRTBusiness(employee?.Business);
            }
            catch (Exception ex)
            {
                SaveHistoryError("Error fetching Google Calendar service", ex);
            }
            GCalendarEventsRoot Events = null;
            try
            {
                Events = GetEvents(googleCalendar?.ApiKEY, uri, DateTime.Now.Date);
            }
            catch (Exception ex)
            {
                SaveHistoryError("Error fetching events from Google Calendar", ex);
            }


            foreach (var item in Events.Items)
            {

                try
                {
                    if (item.Status == "confirmed")
                    {
                        if (item.Id != null && item.End != null && item.End.DateTime != null)
                        {

                            var fullEvent = GetEvent(googleCalendar.ApiKEY, uri, item.Id);


                            string format = "MM/dd/yyyy hh:mm:ss tt";




                            string Json = "";

                            var appointment = AppointmentServices.Instance.GetAppointmentWithGCalEventID(item.Id);

                            try
                            {
                                if (appointment != null)
                                {
                                    try
                                    {
                                        var requestUrl = $"https://www.googleapis.com/calendar/v3/calendars/{employee.GoogleCalendarID}/events/{item.Id}";
                                        var client = new RestClient(requestUrl);

                                        // Create a new request
                                        var request = new RestRequest();

                                        // Add the Bearer token to the Authorization header
                                        request.AddHeader("Authorization", $"Bearer {googleCalendar.AccessToken}");

                                        // Make the REST request asynchronously
                                        var response = await client.ExecuteAsync(request, Method.Get);

                                        // Check if the response is successful
                                        if (response.IsSuccessful)
                                        {
                                            try
                                            {
                                                NEvent calendarEvent = JsonConvert.DeserializeObject<NEvent>(response.Content);

                                                // Extract the organizer's email (which can be used as calendarId)
                                                if (calendarEvent.Organizer == null)
                                                {

                                                    appointment.Date = DateTime.Parse(fullEvent.Start.DateTime);
                                                    appointment.Time = DateTime.Parse(fullEvent.Start.DateTime);
                                                    appointment.EndTime = DateTime.Parse(fullEvent.End.DateTime);
                                                    appointment.GoogleCalendarEventID = item.Id;
                                                    AppointmentServices.Instance.UpdateAppointment(appointment);
                                                }
                                                else
                                                {
                                                    string organizerEmail = calendarEvent.Organizer.Email;
                                                    var newemployee = EmployeeServices.Instance.GetEmployeeWithLinkedGoogleCalendarID(organizerEmail);

                                                    appointment.Date = DateTime.Parse(fullEvent.Start.DateTime);
                                                    appointment.Time = DateTime.Parse(fullEvent.Start.DateTime);
                                                    appointment.EndTime = DateTime.Parse(fullEvent.End.DateTime);
                                                    appointment.EmployeeID = newemployee.ID;
                                                    appointment.GoogleCalendarEventID = item.Id;
                                                    AppointmentServices.Instance.UpdateAppointment(appointment);
                                                }


                                            }
                                            catch (Exception ex)
                                            {

                                                string appointmentJson = JsonConvert.SerializeObject(appointment);
                                                var nhistory = new History();
                                                nhistory.Date = DateTime.Now;
                                                nhistory.Note = ex.Message;
                                                nhistory.Type = appointmentJson;
                                                nhistory.Type = "Error by Qmubin";
                                                HistoryServices.Instance.SaveHistory(nhistory);
                                            }
                                            // Deserialize the response content


                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        string appointmentJson = JsonConvert.SerializeObject(appointment);
                                        var nhistory = new History();
                                        nhistory.Date = DateTime.Now;
                                        nhistory.Note = ex.Message;
                                        nhistory.Type = appointmentJson;
                                        nhistory.Type = "Error by Qmubin";
                                        HistoryServices.Instance.SaveHistory(nhistory);
                                    }




                                }
                                else
                                {

                                    appointment = new Appointment();
                                    appointment.Date = DateTime.Parse(fullEvent.Start.DateTime);
                                    appointment.Time = DateTime.Parse(fullEvent.Start.DateTime);
                                    appointment.EndTime = DateTime.Parse(fullEvent.End.DateTime);
                                    appointment.DepositMethod = "Pin";
                                    appointment.Notes = fullEvent.Summary;
                                    TimeSpan duration = appointment.EndTime - appointment.Time;
                                    var service = ServiceServices.Instance.GetService(employee.Business, "ABSENSE","").FirstOrDefault();
                                    if (service != null)
                                    {
                                        appointment.Service = service.ID.ToString();
                                    }
                                    appointment.Status = "Pending";
                                    appointment.Color = "#dff0e3";
                                    appointment.Business = employee.Business;
                                    appointment.EmployeeID = employee.ID;
                                    appointment.FromGCAL = true;
                                    appointment.IsPaid = true;
                                    appointment.ServiceDuration = duration.TotalMinutes + "mins";
                                    appointment.BookingDate = DateTime.Now;
                                    appointment.GoogleCalendarEventID = item.Id;
                                    AppointmentServices.Instance.SaveAppointment(appointment);


                                }
                            }
                            catch (Exception ex)
                            {
                                string appointmentJson = JsonConvert.SerializeObject(appointment);
                                var nhistory = new History();
                                nhistory.Date = DateTime.Now;
                                nhistory.Note = ex.Message;
                                nhistory.Type = appointmentJson;
                                nhistory.Type = "Error by Qmubin";
                                HistoryServices.Instance.SaveHistory(nhistory);
                            }

                        }

                    }
                    if (item.Status == "cancelled")
                    {

                        try
                        {

                            var appointment = AppointmentServices.Instance.GetAppointmentWithGCalEventID(item.Id);
                            if (appointment != null && appointment.FromGCAL) //From GCal is because it's deleting the appoiintment which was created on YBP
                            {
                                appointment.DELETED = true;
                                appointment.GoogleCalendarEventID = "CANCELLED";
                                appointment.DeletedTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm");
                                AppointmentServices.Instance.UpdateAppointment(appointment);


                                var listOfGoogleCalendarIds = EmployeeServices.Instance.GetEmployeeWRTBusiness(appointment.Business, true).Select(x => x.GoogleCalendarID).ToList();
                                bool Found = false;
                                var EmployeeID = 0;
                                var StartDate = DateTime.Now;
                                var EndDate = DateTime.Now;
                                foreach (var gog in listOfGoogleCalendarIds)
                                {
                                    var client = new RestClient("https://www.googleapis.com/calendar/v3/calendars");
                                    string url = $"{gog}/events/{item.Id}";

                                    // Create a request object
                                    var request = new RestRequest(url, Method.Get);
                                    request.AddHeader("Authorization", $"Bearer {googleCalendar.AccessToken}");
                                    try
                                    {
                                        var response = await client.ExecuteAsync(request);

                                        if (response.IsSuccessful)
                                        {
                                            // Parse and display JSON response
                                            JObject eventObject = JObject.Parse(response.Content);
                                            string Status = eventObject["status"]?.ToString();

                                            if (Status == "confirmed")
                                            {
                                                Found = true;
                                                string Summary = eventObject["summary"]?.ToString();
                                                string OrganizerEmail = eventObject["organizer"]?["email"]?.ToString();
                                                StartDate = DateTime.Parse(eventObject["start"]["dateTime"].ToString());
                                                EndDate = DateTime.Parse(eventObject["end"]["dateTime"].ToString());
                                                EmployeeID = EmployeeServices.Instance.GetEmployeeWithLinkedGoogleCalendarID(gog).ID;

                                                appointment = new Appointment
                                                {
                                                    Date = StartDate,
                                                    Time = StartDate,
                                                    EndTime = EndDate,
                                                    DepositMethod = "Pin",
                                                    Notes = Summary,
                                                    Status = "Pending",
                                                    Color = "#dff0e3",
                                                    Business = employee.Business,
                                                    EmployeeID = EmployeeID,
                                                    FromGCAL = true,
                                                    IsPaid = true,
                                                    BookingDate = DateTime.Now,
                                                    GoogleCalendarEventID = item.Id
                                                };

                                                // Calculate duration
                                                TimeSpan duration = appointment.EndTime - appointment.Time;
                                                appointment.ServiceDuration = duration.TotalMinutes + " mins";

                                                // Get service and assign it
                                                var service = ServiceServices.Instance.GetService(employee.Business, "ABSENSE").FirstOrDefault();
                                                if (service != null)
                                                {
                                                    appointment.Service = service.ID.ToString();
                                                }

                                                // Save appointment
                                                AppointmentServices.Instance.SaveAppointment(appointment);
                                            }
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        history = new History();
                                        history.Date = DateTime.Now;
                                        history.Note = ex.Message + " " + JsonConvert.SerializeObject(item);
                                        history.Type = "NewStatus EX";
                                        HistoryServices.Instance.SaveHistory(history);
                                    }



                                }


                            }

                        }
                        catch (Exception ex)
                        {

                            var appointment = AppointmentServices.Instance.GetAppointmentWithGCalEventID(item.Id);
                            history = new History();
                            history.Date = DateTime.Now;
                            history.Note = ex.Message + " " + JsonConvert.SerializeObject(item);
                            history.Type = "NewStatus EX";
                            HistoryServices.Instance.SaveHistory(history);
                        }

                    }
                   
                }
                catch (Exception ex)
                {

                    var nhistory = new History();
                    nhistory.Date = DateTime.Now;
                    nhistory.Note = ex.Message;
                    nhistory.Business = JsonConvert.SerializeObject(item);
                    nhistory.Type = "Error";
                    HistoryServices.Instance.SaveHistory(nhistory);
                }




            }




            return Json(new { success = true, Message = "Its Done" }, JsonRequestBehavior.AllowGet);
        }




        [HttpGet]
        public ActionResult ForgotPassword(string businessName)
        {
            BookingViewModel model = new BookingViewModel();
            model.Company = CompanyServices.Instance.GetCompany().Where(x => x.Business == businessName).FirstOrDefault();
            return View("ForgotPassword", "_BookingLayout", model);
        }


        [AllowAnonymous]
        [NoCache]
        public ActionResult Index(string businessName, int CustomerID = 0, string By = "")
        {
            BookingViewModel model = new BookingViewModel();
            var name = businessName;
            var company = CompanyServices.Instance.GetCompany().Where(x => x.Business == name).FirstOrDefault();
            model.Company = company;
            model.By = By;
            var service = new List<ServiceModelForBooking>();
            var BestSellerServices = ServiceServices.Instance.GetBestSellerServices(businessName);
            var categories = ServicesCategoriesServices.Instance.GetServiceCategoriesWRTBusinessAndCategory(name, "ABSENSE").OrderBy(x => x.DisplayOrder).ToList();
            foreach (var item in categories)
            {

                var serviceModel = new List<ServiceModelFORBK>();
                var ServicesWRTCategory = ServiceServices.Instance.GetServiceWRTCategory(name, item.Name, true).Where(x => x.IsActive).OrderBy(x => x.DisplayOrder).ToList();
                foreach (var ser in ServicesWRTCategory)
                {
                    if (BestSellerServices.Contains(ser.ID.ToString()))
                    {
                        serviceModel.Add(new ServiceModelFORBK { Service = ser, BestSeller = true });
                    }
                    else
                    {
                        serviceModel.Add(new ServiceModelFORBK { Service = ser, BestSeller = false });

                    }
                }
                service.Add(new ServiceModelForBooking { ServiceCategory = item, Services = serviceModel, Company = model.Company });
            }

            model.Services = service;

            model.CustomerID = CustomerID;
            return View("Index", "_BookingLayout", model);
        }



        bool IsValidEmail(string email)
        {
            var trimmedEmail = email.Trim();

            if (trimmedEmail.EndsWith("."))
            {
                return false;
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
        public JsonResult IsValidEmailAjx(string email)
        {
            var trimmedEmail = email.Trim();

            if (trimmedEmail.EndsWith("."))
            {
                return Json(new { success = false }, JsonRequestBehavior.AllowGet);
            }
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                if (addr.Address == trimmedEmail)
                {
                    return Json(new { success = true }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(new { success = false }, JsonRequestBehavior.AllowGet);
                }
            }
            catch
            {
                return Json(new { success = false }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet]
        public ActionResult GoToPayment()
        {
            // Retrieve and process FormData from the URL
            BookingViewModel model = new BookingViewModel();
            model.FirstName = Request.QueryString["FirstName"];
            model.LastName = Request.QueryString["LastName"];
            model.Email = Request.QueryString["Email"];
            model.MobileNumber = Request.QueryString["MobileNumber"];
            model.Comment = Request.QueryString["Comment"];
            model.Password = Request.QueryString["Password"];
            model.ServiceIDs = Request.QueryString["ServiceIDs"];
            model.CompanyID = int.Parse(Request.QueryString["CompanyID"]);
            var serviceslist = new List<ServiceFormModel>();
            // Your logic here to process the form data
            foreach (var item in model.ServiceIDs.Split(',').ToList())
            {
                var service = ServiceServices.Instance.GetService(int.Parse(item));
                serviceslist.Add(new ServiceFormModel { Name = service.Name, Duration = service.Duration, Price = service.Price });
            }
            model.ServicesOnly = serviceslist;


            return View("GoToPayment", "_BookingLayout", model);
        }

        public int ConvertEuroToCents(float euroAmount)
        {
            // Convert euros to cents
            int centsAmount = (int)(euroAmount * 100);
            return centsAmount;
        }

        [HttpPost]
        public JsonResult ForgotPassword(string Email, string Company)
        {
            var company = CompanyServices.Instance.GetCompany(Company).FirstOrDefault();
            var customer = CustomerServices.Instance.GetCustomerWRTBusiness(company.Business, Email);

            if (customer.Password == null)
            {
                Random random = new Random();
                customer.Password = new string(Enumerable.Repeat("abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789", 8).Select(s => s[random.Next(s.Length)]).ToArray());
                CustomerServices.Instance.UpdateCustomer(customer);
            }
            var emailDetails = EmailTemplateServices.Instance.GetEmailTemplateWRTBusiness(company.Business, "Forgot Password");
            if (emailDetails != null && emailDetails.IsActive == true)
            {
                string emailBody = "<html><body>";
                emailBody += "<h2 style='font-family: Arial, sans-serif;'>Forgot Password</h2>";
                emailBody += emailDetails.TemplateCode;
                emailBody = emailBody.Replace("{{Customer_first_name}}", customer.FirstName);
                emailBody = emailBody.Replace("{{company_name}}", company.Business);
                emailBody = emailBody.Replace("{{company_email}}", company.NotificationEmail);
                emailBody = emailBody.Replace("{{company_address}}", company.Address);
                emailBody = emailBody.Replace("{{company_logo}}", $"<img class='text-center' style='height:50px;width:auto;' src='{"http://app.yourbookingplatform.com" + company.Logo}'>");
                emailBody = emailBody.Replace("{{company_phone}}", company.PhoneNumber);
                emailBody = emailBody.Replace("{{password}}", customer.Password);
                emailBody += "</body></html>";
                if (IsValidEmail(customer.Email))
                {
                    var message = SendEmail(customer.Email, "Reset Credentials", emailBody, company);
                    return Json(new { success = true, Message = message }, JsonRequestBehavior.AllowGet);

                }
                else
                {
                    return Json(new { success = true, Message = "Incorrect Email" }, JsonRequestBehavior.AllowGet);
                }
            }
            else
            {
                return Json(new { success = true, Message = "Not Enabled" }, JsonRequestBehavior.AllowGet);

            }

        }



        [HttpGet]
        public JsonResult CheckPayment(int AppointmentID)
        {
            var appointment = AppointmentServices.Instance.GetAppointment(AppointmentID);
            if ((DateTime.Now - appointment.BookingDate).TotalMinutes < 15 && appointment.IsPaid == false)
            {
                return Json(new { success = true }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                appointment.IsCancelled = true;
                AppointmentServices.Instance.UpdateAppointment(appointment);


                var reminder = ReminderServices.Instance.GetReminderWRTAppID(appointment.ID);
                reminder.IsCancelled = true;
                ReminderServices.Instance.UpdateReminder(reminder);


                return Json(new { success = false }, JsonRequestBehavior.AllowGet);
            }

        }



        [HttpGet]
        public ActionResult Login(string businessName)
        {
            BookingViewModel model = new BookingViewModel();
            model.Company = CompanyServices.Instance.GetCompany().Where(x => x.Business.Trim() == businessName.Trim()).FirstOrDefault();
            return View("Login", "_BookingLayout", model);
        }

        [HttpGet]
        public ActionResult AboutCompany(string businessName, int CustomerID = 0)
        {
            BookingViewModel model = new BookingViewModel();
            var reviewModel = new List<ReviewModel>();
            model.CustomerID = CustomerID;
            model.Company = CompanyServices.Instance.GetCompany().Where(x => x.Business.Trim() == businessName.Trim()).FirstOrDefault();
            model.OpeningHours = OpeningHourServices.Instance.GetOpeningHoursWRTBusiness(model.Company.Business, "");
            var reviews = ReviewServices.Instance.GetReviewWRTBusiness(model.Company.Business, "").Where(X => X.Rating != 0).ToList();
            foreach (var item in reviews)
            {
                var customer = CustomerServices.Instance.GetCustomer(item.CustomerID);
                var employee = EmployeeServices.Instance.GetEmployee(item.EmployeeID);
                reviewModel.Add(new ReviewModel { Review = item, EmployeeName = employee.Name, CustomerName = customer.FirstName + " " + customer.LastName });
            }
            model.Reviews = reviewModel;
            return View("AboutCompany", "_BookingLayout", model);
        }

        [HttpPost]
        public ActionResult Login(string Email, string Password, string businessName)
        {
            var LoggedInCustomer = CustomerServices.Instance.GetCustomerWRTBusiness(businessName, Email, Password);
            if (LoggedInCustomer != null)
            {
                if (LoggedInCustomer.IsBlocked)
                {
                    return Json(new { success = false, Message = "You are blocked from the system" }, JsonRequestBehavior.AllowGet);

                }
                else
                {
                    return Json(new { success = true, CustomerID = LoggedInCustomer.ID, Name = LoggedInCustomer.FirstName + " " + LoggedInCustomer.LastName }, JsonRequestBehavior.AllowGet);
                }
            }
            else
            {
                return Json(new { success = false, Message = "Invalid Credentials" }, JsonRequestBehavior.AllowGet);

            }
        }


        [HttpPost]
        public JsonResult CancelAppointment(int ID)
        {

            var appointment = AppointmentServices.Instance.GetAppointment(ID);
            appointment.IsCancelled = true;
            AppointmentServices.Instance.UpdateAppointment(appointment);


            var reminder = ReminderServices.Instance.GetReminderWRTAppID(appointment.ID);
            reminder.IsCancelled = true;
            ReminderServices.Instance.UpdateReminder(reminder);


            var customer = CustomerServices.Instance.GetCustomer(appointment.CustomerID);
            var employee = EmployeeServices.Instance.GetEmployee(appointment.EmployeeID);

            var historyNew = new History();
            historyNew.Business = appointment.Business;
            historyNew.CustomerName = customer.FirstName + " " + customer.LastName;
            historyNew.Date = DateTime.Now;
            historyNew.Note = "Appointment was cancelled on Customer Profile for :" + historyNew.CustomerName;
            historyNew.EmployeeName = employee.Name;
            HistoryServices.Instance.SaveHistory(historyNew);
            var googleCalendar = GoogleCalendarServices.Instance.GetGoogleCalendarServicesWRTBusiness(appointment.Business);
            if (googleCalendar != null && !googleCalendar.Disabled)
            {
                if (appointment.GoogleCalendarEventID != null)
                {
                    RefreshToken(appointment.Business);
                    var url = new System.Uri("https://www.googleapis.com/calendar/v3/calendars/" + employee.GoogleCalendarID + "/events/" + appointment.GoogleCalendarEventID);
                    RestClient restClient = new RestClient(url);
                    RestRequest request = new RestRequest();

                    request.AddQueryParameter("key", "AIzaSyASKpY6I08IVKFMw3muX39uMzPc5sBDaSc");
                    request.AddHeader("Authorization", "Bearer " + googleCalendar.AccessToken);
                    request.AddHeader("Accept", "application/json");

                    var response = restClient.Delete(request);

                }
            }


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

            if (customer.Password == null)
            {
                Random random = new Random();
                customer.Password = new string(Enumerable.Repeat("abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789", 8).Select(s => s[random.Next(s.Length)]).ToArray());
                CustomerServices.Instance.UpdateCustomer(customer);
            }

            var company = CompanyServices.Instance.GetCompany(customer.Business).FirstOrDefault();
            var emailDetails = EmailTemplateServices.Instance.GetEmailTemplateWRTBusiness(company.Business, "Appointment Cancelled");
            if (emailDetails != null && emailDetails.IsActive == true)
            {
                string emailBody = "<html><body>";
                emailBody += "<h2 style='font-family: Arial, sans-serif;'>Appointment Cancelled</h2>";
                emailBody += emailDetails.TemplateCode;
                emailBody = emailBody.Replace("{{Customer_first_name}}", customer.FirstName);
                //emailBody = emailBody.Replace("{{Customer_last_name}}", customer.LastName);
                //emailBody = emailBody.Replace("{{Customer_initial}}", "Dear");
                emailBody = emailBody.Replace("{{date}}", appointment.Date.ToString("yyyy-MM-dd"));
                //emailBody = emailBody.Replace("{{employee_picture}}", $"<img class='text-center' style='height:50px;width:auto;' src='{"http://app.yourbookingplatform.com" + employee.Photo}'>");

                emailBody = emailBody.Replace("{{time}}", appointment.Time.ToString("H:mm:ss"));
                //emailBody = emailBody.Replace("{{end_time}}", appointment.EndTime.ToString("H:mm:ss"));
                //emailBody = emailBody.Replace("{{employee}}", employee.Name);
                //emailBody = emailBody.Replace("{{employee_specialization}}", employee.Specialization);
                emailBody = emailBody.Replace("{{services}}", ConcatenatedServices);
                emailBody = emailBody.Replace("{{company_name}}", company.Business);
                emailBody = emailBody.Replace("{{company_email}}", company.NotificationEmail);
                emailBody = emailBody.Replace("{{company_address}}", company.Address);
                emailBody = emailBody.Replace("{{company_logo}}", $"<img class='text-center' style='height:50px;width:auto;' src='{"http://app.yourbookingplatform.com" + company.Logo}'>");
                emailBody = emailBody.Replace("{{company_phone}}", company.PhoneNumber);
                emailBody = emailBody.Replace("{{password}}", customer.Password);

                emailBody += "</body></html>";
                if (IsValidEmail(customer.Email))
                {
                    SendEmail(customer.Email, "Appointment Cancelled", emailBody, company);
                }
            }
            return Json(new { success = true }, JsonRequestBehavior.AllowGet);





        }
        static string GenerateRandomCode()
        {
            Random random = new Random();
            string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            string digits = "0123456789";

            char[] code = new char[5];
            code[0] = chars[random.Next(chars.Length)];   // First character is a letter
            code[1] = digits[random.Next(digits.Length)]; // Second character is a digit
            code[2] = digits[random.Next(digits.Length)]; // Third character is a digit
            code[3] = chars[random.Next(chars.Length)];   // Fourth character is a letter
            code[4] = chars[random.Next(chars.Length)]; // Fifth character is a letter

            return new string(code);
        }
        [HttpGet]
        public ActionResult Referrals(int CustomerID, string businessName)
        {
            BookingViewModel model = new BookingViewModel();
            var customer = CustomerServices.Instance.GetCustomer(CustomerID);
            if (customer.ReferralCode == null || customer.ReferralCode == "")
            {
                customer.ReferralCode = GenerateRandomCode();
            }
            model.Company = CompanyServices.Instance.GetCompany().Where(X => X.Business == businessName).FirstOrDefault();
            model.Customer = customer;
            CustomerServices.Instance.UpdateCustomer(customer);
            var referrals = ReferralServices.Instance.GetReferralWRTBusinessREF(businessName, CustomerID);
            var referlsit = new List<ReferralModel>();
            foreach (var item in referrals)
            {
                var appointment = AppointmentServices.Instance.GetAppointment(item.AppointmentID);
                string ConcatenatedServices = "";
                var referredCustomer = CustomerServices.Instance.GetCustomer(item.CustomerID);
                foreach (var serv in appointment.Service.Split(',').ToList())
                {
                    var ServiceNew = ServiceServices.Instance.GetService(int.Parse(serv));
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
                referlsit.Add(new ReferralModel { ReferredCustomer = referredCustomer, Referral = item, Appointment = appointment, Services = ConcatenatedServices, Customer = customer });
            }
            model.Referrals = referlsit;
            return View("Referrals", "_BookingLayout", model);
        }


        [HttpGet]
        public ActionResult CustomerAppointments(int CustomerID, string businessName)
        {
            BookingViewModel model = new BookingViewModel();
            model.Customer = CustomerServices.Instance.GetCustomer(CustomerID);
            model.Company = CompanyServices.Instance.GetCompany().Where(x => x.Business == businessName).FirstOrDefault();

            var Appointments = AppointmentServices.Instance.GetAppointmentsByCustomerProfile(model.Company.Business, CustomerID, false).OrderByDescending(x => x.ID).ToList();

            var listofAppointments = new List<AppointmentModel>();
            foreach (var item in Appointments)
            {
                int TotalDuration = 0;
                var appointment = AppointmentServices.Instance.GetAppointment(item.ID);
                var employee = EmployeeServices.Instance.GetEmployee(item.EmployeeID);
                var ServiceListCommand = item.Service.Split(',').ToList();
                var ServiceDuration = item.ServiceDuration.Split(',').ToList();
                var serviceList = new List<ServiceModelForCustomerProfile>();
                for (int i = 0; i < ServiceListCommand.Count && i < ServiceDuration.Count; i++)
                {
                    var Service = ServiceServices.Instance.GetService(int.Parse(ServiceListCommand[i]));
                    if (Service != null)
                    {
                        var serviceViewModel = new ServiceModelForCustomerProfile
                        {
                            Name = Service.Name,
                            Duration = ServiceDuration[i]
                        };
                        TotalDuration += int.Parse(ServiceDuration[i].Replace("mins", "").Replace("Mins", "").Trim());
                        serviceList.Add(serviceViewModel);

                    }
                }
                listofAppointments.Add(new AppointmentModel
                {
                    Date = item.Date,
                    Time = item.Time,
                    AppointmentEndTime = item.EndTime,
                    TotalCost = item.TotalCost,
                    BookingDate = item.BookingDate,
                    Status = item.Status,
                    IsCancelled = item.IsCancelled,
                    IsPaid = item.IsPaid,
                    ID = item.ID,
                    EmployeeName = employee.Name,
                    EmployeeSpecialization = employee.Specialization,
                    Customer = model.Customer,
                    Services = serviceList,
                    TotalDuration = TotalDuration
                });

            }
            model.Appointments = listofAppointments;
            return View("CustomerAppointments", "_BookingLayout", model);
        }


        public ActionResult PrivacyPolicy(string businessName)
        {
            BookingViewModel model = new BookingViewModel();
            model.Company = CompanyServices.Instance.GetCompany().Where(x => x.Business == businessName).FirstOrDefault();

            return View("PrivacyPolicy", "_BookingLayout", model);

        }

        [HttpGet]
        public ActionResult CustomerGiftCardAssignment(int CustomerID, string businessName)
        {
            BookingViewModel model = new BookingViewModel();
            model.Customer = CustomerServices.Instance.GetCustomer(CustomerID);
            model.Company = CompanyServices.Instance.GetCompany().Where(x => x.Business == businessName).FirstOrDefault();
            var giftCardAssignments = new List<GiftCardModelInBooking>();
            var CustomergiftCardAssignments = GiftCardServices.Instance.GetGiftCardAssignmentsWRTBusiness(businessName, CustomerID);
            foreach (var item in CustomergiftCardAssignments)
            {
                var giftCard = GiftCardServices.Instance.GetGiftCard(item.GiftCardID);
                giftCardAssignments.Add(new GiftCardModelInBooking { GiftCardName = giftCard.Name, GiftCardAssignment = item });
            }
            model.GiftCardAssignments = giftCardAssignments;

            return View("CustomerGiftCardAssignment", "_BookingLayout", model);

        }


        [HttpGet]
        public ActionResult CustomerLoyaltyCardAssignments(int CustomerID, string businessName)
        {
            BookingViewModel model = new BookingViewModel();
            var ListOfFinalCardAssignments = new List<LoyaltyCardAssignmentModel>();
            model.Customer = CustomerServices.Instance.GetCustomer(CustomerID);
            model.Company = CompanyServices.Instance.GetCompany().Where(x => x.Business == businessName).FirstOrDefault();
            var LCAs = LoyaltyCardServices.Instance.GetLoyaltyCardAssignmentsWRTbBusinessAndCustomer(model.Company.Business, CustomerID);
            foreach (var item in LCAs)
            {
                var loyaltyCard = LoyaltyCardServices.Instance.GetLoyaltyCard(item.LoyaltyCardID);
                float CashBack = 0;

                ListOfFinalCardAssignments.Add(new LoyaltyCardAssignmentModel { LoyaltyCardDays = loyaltyCard.Days, LoyaltyCardName = loyaltyCard.Name, LoyaltyCardAssignment = item, LoyaltyCardUsage = CashBack });
            }


            model.LoyaltyCardAssignments = ListOfFinalCardAssignments;
            return View("CustomerLoyaltyCardAssignments", "_BookingLayout", model);

        }

        [HttpGet]
        public ActionResult CustomerHistory(int CustomerID, string businessName)
        {
            BookingViewModel model = new BookingViewModel();
            model.Customer = CustomerServices.Instance.GetCustomer(CustomerID);
            model.Company = CompanyServices.Instance.GetCompany().Where(x => x.Business == businessName).FirstOrDefault();
            model.Histories = HistoryServices.Instance.GetHistoriesWRTBusinessandCustomer(businessName, model.Customer.FirstName + " " + model.Customer.LastName);
            return View("CustomerHistory", "_BookingLayout", model);


        }

        [HttpGet]
        public ActionResult SubscribedToWaitingList(string businessName)
        {
            return View();
        }

        [HttpGet]
        [NoCache]
        public ActionResult CustomerProfile(string businessName, int CustomerID = 0, string WaitingList = "")
        {
            BookingViewModel model = new BookingViewModel();
            model.Company = CompanyServices.Instance.GetCompany().Where(x => x.Business.Trim() == businessName).FirstOrDefault();
            model.Customer = CustomerServices.Instance.GetCustomer(CustomerID);

            if (WaitingList == "Yes")
            {
                if (CustomerID != 0)
                {
                    var customer = CustomerServices.Instance.GetCustomer(CustomerID);
                    model.Customer = customer;
                    model.CompanyName = businessName;
                    model.CustomerID = CustomerID;
                    return RedirectToAction("SubscribedToWaitingList", "Booking", new { businessName = businessName });
                }
                else
                {
                    model.CompanyName = businessName;
                    model.CustomerID = 0;
                    return RedirectToAction("SubscribedToWaitingList", "Booking", new { businessName = businessName });
                }
            }
            else
            {

                if (CustomerID != 0)
                {
                    var customer = CustomerServices.Instance.GetCustomer(CustomerID);
                    model.Customer = customer;
                    model.CompanyName = businessName;
                    model.CustomerID = CustomerID;
                    return View("CustomerProfile", "_BookingLayout", model);
                }
                else
                {
                    model.CompanyName = businessName;
                    model.CustomerID = 0;
                    return View("Welcome", "_BookingLayout", model);
                }

            }
        }


        [HttpPost]
        public JsonResult CustomerProfile(BookingViewModel model)
        {
            var customer = CustomerServices.Instance.GetCustomer(model.ID);
            customer.ID = model.ID;
            customer.City = model.City;
            customer.PostalCode = model.PostalCode;
            customer.DateOfBirth = model.DateOfBirth;
            customer.FirstName = model.FirstName;
            customer.LastName = model.LastName;
            customer.Address = model.Address;
            customer.Email = model.Email;
            customer.MobileNumber = model.MobileNumber;
            customer.Gender = model.Gender;
            customer.Password = model.Password;
            customer.MobileNumber = model.MobileNumber;
            CustomerServices.Instance.UpdateCustomer(customer);
            return Json(new { success = true }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult PriceList(string businessName)
        {
            BookingViewModel model = new BookingViewModel();
            var name = businessName;
            var company = CompanyServices.Instance.GetCompany().Where(x => x.Business == name).FirstOrDefault();
            model.Company = company;
            var service = new List<ServicePriceListModel>();
            var categories = ServicesCategoriesServices.Instance.GetServiceCategoriesWRTBusinessAndCategory(name, "ABSENSE").OrderBy(x => x.DisplayOrder);
            foreach (var item in categories)
            {
                var ServicesWRTCategory = ServiceServices.Instance.GetServiceWRTCategory(name, item.Name, true).Where(x => x.IsActive).OrderBy(x => x.DisplayOrder).ToList();
                service.Add(new ServicePriceListModel { ServiceCategory = item, Services = ServicesWRTCategory, Currency = model.Company.Currency });
            }
            model.ServicesPriceList = service;
            return View("PriceList", "_BookingLayout", model);
        }

        [HttpGet]
        public ActionResult Welcome(string businessName, string By = "", int AppointmentID = 0)
        {
            BookingViewModel model = new BookingViewModel();

            model.By = By;
            if (model.By == "FromRebook")
            {
                if (AppointmentID != 0)
                {
                    var RebookReminder = RebookReminderServices.Instance.GetRebookReminderWRTBusiness(AppointmentID);
                    if (RebookReminder != null)
                    {
                        RebookReminder.Clicked = true;
                        RebookReminderServices.Instance.UpdateRebookReminder(RebookReminder);
                    }
                    else
                    {
                        var appointment = AppointmentServices.Instance.GetAppointment(AppointmentID);
                        var customer = CustomerServices.Instance.GetCustomer(appointment.CustomerID);
                        var rebookreminder = new RebookReminder();
                        rebookreminder.CustomerName = customer.FirstName + " " + customer.LastName;
                        rebookreminder.Business = appointment.Business;
                        rebookreminder.Sent = true;
                        rebookreminder.Clicked = true;
                        rebookreminder.Opened = true;
                        rebookreminder.Date = DateTime.Now;
                        RebookReminderServices.Instance.SaveRebookReminder(rebookreminder);

                    }
                }
            }
            model.Company = CompanyServices.Instance.GetCompany().Where(x => x.Business.Trim() == businessName.Trim()).FirstOrDefault();
            return View("Welcome", "_BookingLayout", model);
        }

        public bool IsCurrentDateInRange(DateTime startDate, DateTime endDate)
        {
            DateTime currentDate = DateTime.Now;

            if (currentDate >= startDate && currentDate <= endDate)
            {
                return true;
            }
            else
            {
                return false;
            }
        }


        [HttpGet]
        public ActionResult CustomerSettings(string businessName, int CustomerID)
        {
            BookingViewModel model = new BookingViewModel();
            var customer = CustomerServices.Instance.GetCustomer(CustomerID);
            model.Customer = customer;
            model.Company = CompanyServices.Instance.GetCompany().Where(x => x.Business.Trim() == businessName.Trim()).FirstOrDefault();
            return View("CustomerSettings", "_BookingLayout", model);
        }

        [HttpGet]
        [NoCache]
        public ActionResult Form(string businessName, string ids, int CustomerID = 0, string SentBy = "", int AppointmentID = 0, string By = "")
        {
            BookingViewModel model = new BookingViewModel();
            model.SentBy = SentBy;
            model.Company = CompanyServices.Instance.GetCompany().Where(x => x.Business.Trim() == businessName.Trim()).FirstOrDefault();
            var servicelist = new List<ServiceFormModel>();
            var FinalIDs = ids.Replace("_", ",").Split(',').Where(x => x != "").ToList();
            model.By = By;
            if (SentBy == "Cancellation")
            {
                model.Appointment = AppointmentServices.Instance.GetAppointment(AppointmentID);
                if (model.Appointment.IsCancelled)
                {
                    return RedirectToAction("CannotReschedule", "Appointment", new { ID = AppointmentID });
                }
                model.ServiceIDs = model.Appointment.Service;
            }
            else
            {
                model.ServiceIDs = String.Join(",", FinalIDs);
            }
            model.CompanyID = model.Company.ID;
            foreach (var item in FinalIDs)
            {
                var serviceItem = ServiceServices.Instance.GetService(int.Parse(item));

                servicelist.Add(new ServiceFormModel { OnlyDuration = float.Parse(serviceItem.Duration.Replace("mins", "").Replace("Mins", "")), Name = serviceItem.Name, Duration = serviceItem.Duration, Price = serviceItem.Price });

            }
            var finalIDsInt = FinalIDs.Select(int.Parse).ToList();

            var employeeservices = EmployeeServiceServices.Instance.GetEmployeeServiceWRTBusiness(model.Company.Business);

            var employeeRequest = EmployeeRequestServices.Instance.GetEmployeeRequestByBusiness(model.Company.ID);
            foreach (var item in employeeRequest)
            {
                if (item.Accepted)
                {
                    employeeservices.AddRange(EmployeeServiceServices.Instance.GetEmployeeServiceWRTEmployeeID(item.EmployeeID));
                }
            }

            var AvailableEmployeeList = new List<int>();


            var employeeIDs = employeeservices
            .Where(es => finalIDsInt.Contains(es.ServiceID))
            .GroupBy(es => es.EmployeeID)
            .Where(grp => grp.Select(g => g.ServiceID).Distinct().Count() == finalIDsInt.Count)
            .Select(grp => grp.Key)
            .ToList();

            var listofemployee = new List<EmployeeNewModel>();
            var employees = EmployeeServices.Instance.GetEmployeeWRTBusiness(true, true, employeeIDs);
            foreach (var item in employees)
            {
                var AllSpecificRating = ReviewServices.Instance
                    .GetReviewWRTBusiness(model.Company.Business)
                    .Where(x => x.EmployeeID == item.ID
                    && x.Feedback != null)
                    .Select(x => x.Rating)
                    .DefaultIfEmpty(0);

                var CountOfTheRatigs = AllSpecificRating.Count();
                var RatingService = AllSpecificRating.Average();

                var listofReivew = new List<ReviewModel>();
                var reviews = ReviewServices.Instance
                    .GetReviewWRTBusiness(model.Company.Business)
                    .Where(x => x.EmployeeID == item.ID
                    && x.Feedback != null).ToList();
                foreach (var rev in reviews)
                {
                    var customer = CustomerServices.Instance.GetCustomer(rev.CustomerID);
                    listofReivew.Add(new ReviewModel { Review = rev, CustomerName = customer.FirstName });
                }
                bool HavePricChange = false;
                var priceChange = new EmployeePriceChange();
                var employeePriceChange = EmployeePriceChangeServices.Instance.GetEmployeePriceChangeWRTBusiness(item.ID, businessName);
                foreach (var empchange in employeePriceChange)
                {
                    if (IsCurrentDateInRange(empchange.StartDate, empchange.EndDate))
                    {
                        HavePricChange = true;
                        priceChange = empchange;
                        break;
                    }
                }
                listofemployee.Add(new EmployeeNewModel { Reviews = listofReivew, Employee = item, EmployeePriceChange = priceChange, HaveEmpPriceChange = HavePricChange, Rating = RatingService, Count = CountOfTheRatigs });
            }
            model.Employees = listofemployee;
            if (model.Employees.Count() == 0)
            {
                model.ErrorNote = "Yes";
            }
            else
            {
                model.ErrorNote = "No";
            }

            if (servicelist.Count() == 0 || model.ErrorNote != "No")
            {
                model.ErrorNote = "Yes";
            }
            else
            {
                model.ErrorNote = "No";

            }
            model.ServicesOnly = servicelist;
            float Deposit = 0;
            foreach (var item in model.ServicesOnly)
            {
                Deposit += item.Price;
            }
            model.Deposit = float.Parse(Convert.ToString(Deposit * (model.Company.Deposit / 100)));
            model.CustomerID = CustomerID;
            if (SentBy == "Cancellation")
            {
                var customer = CustomerServices.Instance.GetCustomer(model.CustomerID);
                if (customer != null)
                {
                    model.FirstName = customer.FirstName;
                    model.LastName = customer.LastName;
                    model.MobileNumber = customer.MobileNumber;
                    model.Email = customer.Email;
                }
                DateTime combinedDateTime = new DateTime(model.Appointment.Date.Year, model.Appointment.Date.Month, model.Appointment.Date.Day, model.Appointment.Time.Hour, model.Appointment.Time.Minute, model.Appointment.Time.Second);
                TimeSpan difference = combinedDateTime - DateTime.Now;
                if (difference.TotalHours >= (int.Parse(model.Company.CancellationTime.Replace("Hours", "").Replace("Hour", ""))) || difference.TotalHours < 0)
                {
                    return View("Form", "_BookingLayout", model);
                }
                else
                {
                    return RedirectToAction("CannotReschedule", "Appointment", new { ID = AppointmentID });
                }
            }
            else
            {
                return View("Form", "_BookingLayout", model);
            }

        }

        public string SendEmail(string toEmail, string subject, string emailBody, Company company)
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
                mail.ReplyTo = new MailAddress(company.NotificationEmail); // Set the ReplyTo address

                mail.Body = emailBody;
                mail.BodyEncoding = System.Text.Encoding.UTF8;
                mail.IsBodyHtml = true;

                mail.Priority = MailPriority.High;
                SmtpClient client = new SmtpClient();
                client.Credentials = new System.Net.NetworkCredential(senderEmail, senderPassword);
                client.Port = Port;
                client.Host = Host;
                client.EnableSsl = true;
                client.Send(mail);
                return "Done";
            }
            catch (Exception ex)
            {
                Session["EmailStatus"] = ex.ToString();
                return "Failed";
            }
            //try
            //{
            //    string senderEmail = "Support@yourbookingplatform.com";
            //    string senderPassword = "Yourbookingplatform_001";

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



            //    return "Done";
            //}
            //catch (Exception ex)
            //{
            //    Session["EmailStatus"] = ex.ToString();
            //    return ex.ToString();
            //}

        }
        public string RefreshToken(string Company)
        {
            var url = new System.Uri("https://oauth2.googleapis.com/token");
            RestClient restClient = new RestClient(url);
            RestRequest request = new RestRequest();
            var googleCalendar = GoogleCalendarServices.Instance.GetGoogleCalendarServicesWRTBusiness(Company);
            if (googleCalendar != null)
            {
                var ClientID = "201633868472-3sf5q4hbiqupcf0smo6auch9bku6bech.apps.googleusercontent.com";
                var ClientSecret = "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";
                request.AddQueryParameter("client_id", ClientID);
                request.AddQueryParameter("client_secret", ClientSecret);
                request.AddQueryParameter("grant_type", "refresh_token");
                request.AddQueryParameter("refresh_token", googleCalendar.RefreshToken);

                var response = restClient.Post(request);

                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    var googleCalendarnew = GoogleCalendarServices.Instance.GetGoogleCalendarServicesWRTBusiness(Company);
                    var tokenResponse = JsonConvert.DeserializeObject<TokenResponseForDB>(response.Content);
                    googleCalendarnew.AccessToken = tokenResponse.AccessToken;
                    GoogleCalendarServices.Instance.UpdateGoogleCalendarIntegration(googleCalendarnew);
                    return "Refreshed";
                }
                else
                {
                    return "Failed";

                }
            }
            else
            {
                return "non configured";

            }
        }

        [NoCache]
        [HttpGet]
        public ActionResult ProcessPayment(string businessName, string SuccessURL, string CancelURL, string PaymentSecret, int AppointmentID)
        {
            BookingViewModel model = new BookingViewModel();
            var company = CompanyServices.Instance.GetCompany(businessName).FirstOrDefault();
            model.Company = company;
            model.SuccessURL = SuccessURL;
            model.PaymentSecret = PaymentSecret;
            model.CancelURL = CancelURL;

            var appointment = AppointmentServices.Instance.GetAppointment(AppointmentID);
            model.Appointment = appointment;
            if (appointment.IsCancelled)
            {
                return RedirectToAction("AppointmentCancelled", "Appointment", new { ID = AppointmentID });
            }
            return View("ProcessPayment", "_BookingLayout", model);
        }


        [HttpGet]
        public ActionResult Summary(int AppointmentID)
        {
            BookingViewModel model = new BookingViewModel();
            var appointment = AppointmentServices.Instance.GetAppointment(AppointmentID);
            model.Appointment = appointment;
            return View("Summary", "_BookingLayout", model);

        }

        static bool IsDateInRange(DateTime dateToCheck, DateTime startDate, DateTime endDate)
        {
            // Extracting only the date part without the time part
            DateTime dateOnlyToCheck = dateToCheck.Date;
            DateTime startDateOnly = startDate.Date;
            DateTime endDateOnly = endDate.Date;

            if (dateOnlyToCheck >= endDateOnly)
            {
                return true; // If it's greater, it's definitely not in range
            }

            // Checking if the date falls within the range
            return dateOnlyToCheck >= startDateOnly && dateOnlyToCheck <= endDateOnly;

            // Checking if the date falls within the range
        }

        public ActionResult PaymentSuccess(string paymentIntentId, int AppointmentID)
        {
            var service = new PaymentIntentService();
            var appointment = AppointmentServices.Instance.GetAppointment(AppointmentID);
            var company = CompanyServices.Instance.GetCompany(appointment.Business).FirstOrDefault();

            var customer = CustomerServices.Instance.GetCustomer(appointment.CustomerID);
            var employee = EmployeeServices.Instance.GetEmployee(appointment.EmployeeID);

            StripeConfiguration.ApiKey = company.APIKEY;
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
            var paymentIntent = service.Get(paymentIntentId);

            if (paymentIntent.Status == "succeeded")
            {
                return RedirectToAction("CustomerProfile", "Booking", new { CustomerID = paymentIntent.Metadata["CustomerID"], businessName = paymentIntent.Metadata["BusinessName"] });
            }
            else
            {
                // Payment was not successful
                appointment.IsPaid = false;
                appointment.IsCancelled = true;
                AppointmentServices.Instance.UpdateAppointment(appointment);

                var failedAppointment = new FailedAppointment();
                failedAppointment.AppointmentID = appointment.ID;
                failedAppointment.Failed = true;
                FailedAppointmentServices.Instance.SaveFailedAppointment(failedAppointment);


                if (appointment.CouponID != 0)
                {
                    var couponAssignment = CouponServices.Instance.GetCouponAssignmentsWRTBusinessAndCouponID(appointment.Business, appointment.CouponID, appointment.CustomerID);
                    if (couponAssignment != null)
                    {
                        couponAssignment.Used -= 1;
                        CouponServices.Instance.UpdateCouponAssignment(couponAssignment);
                    }
                }

                var reminder = ReminderServices.Instance.GetReminderWRTAppID(appointment.ID);
                reminder.IsCancelled = true;
                ReminderServices.Instance.UpdateReminder(reminder);

                var googleCalendar = GoogleCalendarServices.Instance.GetGoogleCalendarServicesWRTBusiness(company.Business);
                if (googleCalendar != null && !googleCalendar.Disabled)
                {
                    try
                    {
                        var url = new System.Uri("https://www.googleapis.com/calendar/v3/calendars/" + employee.GoogleCalendarID + "/events/" + appointment.GoogleCalendarEventID);
                        var restClient = new RestClient(url);
                        var request = new RestRequest();

                        request.AddQueryParameter("key", "AIzaSyASKpY6I08IVKFMw3muX39uMzPc5sBDaSc");
                        request.AddHeader("Authorization", "Bearer " + googleCalendar.AccessToken);
                        request.AddHeader("Accept", "application/json");
                        var response = restClient.Delete(request);
                        if (response.StatusCode == System.Net.HttpStatusCode.NoContent)
                        {
                            var history = new History();
                            history.Date = DateTime.Now;
                            history.Note = "Appointment got deleted from GCalendar";
                            history.Business = appointment.Business;
                            HistoryServices.Instance.SaveHistory(history);
                        }
                        else
                        {

                            var history = new History();
                            history.Date = DateTime.Now;
                            history.Note = response.Content;
                            history.Business = "Error";
                            HistoryServices.Instance.SaveHistory(history);
                        }
                    }
                    catch (Exception ex)
                    {

                        var history = new History();
                        history.Date = DateTime.Now;
                        history.Note = ex.Message;
                        history.Business = "Error";
                        HistoryServices.Instance.SaveHistory(history);
                    }



                }
                //var reminder = new Entities.Reminder();
                //reminder.Service = appointment.Service;
                //reminder.Business = appointment.Business;
                //reminder.AppointmentID = appointment.ID;
                //reminder.Date = combinedDateTime;
                //reminder.CustomerID = appointment.CustomerID;
                //reminder.EmployeeID = appointment.EmployeeID;
                //ReminderServices.Instance.SaveReminder(reminder);

                #region ReminderMailingRegion

                //var employee = EmployeeServices.Instance.GetEmployee(appointment.EmployeeID);

                if (customer.Password == null)
                {
                    Random random = new Random();
                    customer.Password = new string(Enumerable.Repeat("abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789", 8).Select(s => s[random.Next(s.Length)]).ToArray());
                    CustomerServices.Instance.UpdateCustomer(customer);
                }

                var emailDetails = EmailTemplateServices.Instance.GetEmailTemplateWRTBusiness(company.Business, "Appointment Payment Reminder");
                if (emailDetails != null && emailDetails.IsActive == true)
                {
                    string emailBody = "<html><body>";
                    emailBody += "<h2 style='font-family: Arial, sans-serif;'>Appointment Payment Reminder</h2>";
                    emailBody += emailDetails.TemplateCode;
                    emailBody = emailBody.Replace("{{Customer_first_name}}", customer.FirstName);
                    //emailBody = emailBody.Replace("{{Customer_last_name}}", customer.LastName);
                    //emailBody = emailBody.Replace("{{Customer_initial}}", "Dear");
                    emailBody = emailBody.Replace("{{date}}", appointment.Date.ToString("yyyy-MM-dd"));
                    emailBody = emailBody.Replace("{{time}}", appointment.Time.ToString("H:mm:ss"));
                    //emailBody = emailBody.Replace("{{end_time}}", appointment.EndTime.ToString("H:mm:ss"));
                    //emailBody = emailBody.Replace("{{employee}}", employee.Name);
                    //emailBody = emailBody.Replace("{{employee_picture}}", $"<img class='text-center' style='height:50px;width:auto;' src='{"http://app.yourbookingplatform.com" + employee.Photo}'>");
                    emailBody = emailBody.Replace("{{services}}", ConcatenatedServices);
                    emailBody = emailBody.Replace("{{company_name}}", company.Business);
                    emailBody = emailBody.Replace("{{company_email}}", company.NotificationEmail);
                    emailBody = emailBody.Replace("{{company_address}}", company.Address);
                    emailBody = emailBody.Replace("{{company_logo}}", $"<img class='text-center' style='height:50px;width:auto;' src='{"http://app.yourbookingplatform.com" + company.Logo}'>");
                    emailBody = emailBody.Replace("{{company_phone}}", company.PhoneNumber);
                    emailBody = emailBody.Replace("{{password}}", customer.Password);

                    emailBody += "</body></html>";


                    if (IsValidEmail(customer.Email))
                    {
                        SendEmail(customer.Email, "Appointment Payment Awaiting", emailBody, company);
                    }
                }
                #endregion

                return RedirectToAction("Index", "Booking", new { businessName = paymentIntent.Metadata["BusinessName"] });
            }
        }

        // Action for handling canceled payments
        public ActionResult PaymentCancel(string paymentIntentId, int AppointmentID)
        {
            // Handle cancellation
            var service = new PaymentIntentService();
            var appointment = AppointmentServices.Instance.GetAppointment(AppointmentID);
            var company = CompanyServices.Instance.GetCompany(appointment.Business).FirstOrDefault();
            StripeConfiguration.ApiKey = company.APIKEY;
            var paymentIntent = service.Get(paymentIntentId);

            appointment.IsPaid = false;
            appointment.IsCancelled = true;
            AppointmentServices.Instance.UpdateAppointmentNew(appointment);
            // Redirect to a cancellation page or back to the booking page
            if (appointment.CouponID != 0)
            {
                var couponAssignment = CouponServices.Instance.GetCouponAssignmentsWRTBusinessAndCouponID(appointment.Business, appointment.CouponID, appointment.CustomerID);
                if (couponAssignment != null)
                {
                    couponAssignment.Used -= 1;
                    CouponServices.Instance.UpdateCouponAssignment(couponAssignment);
                }
            }

            var reminder = ReminderServices.Instance.GetReminderWRTAppID(appointment.ID);
            reminder.IsCancelled = true;
            ReminderServices.Instance.UpdateReminder(reminder);


            var employee = EmployeeServices.Instance.GetEmployee(appointment.EmployeeID);
            var googleCalendar = GoogleCalendarServices.Instance.GetGoogleCalendarServicesWRTBusiness(company.Business);
            if (googleCalendar != null && !googleCalendar.Disabled)
            {
                try
                {
                    var url = new System.Uri("https://www.googleapis.com/calendar/v3/calendars/" + employee.GoogleCalendarID + "/events/" + appointment.GoogleCalendarEventID);
                    var restClient = new RestClient(url);
                    var request = new RestRequest();

                    request.AddQueryParameter("key", "AIzaSyASKpY6I08IVKFMw3muX39uMzPc5sBDaSc");
                    request.AddHeader("Authorization", "Bearer " + googleCalendar.AccessToken);
                    request.AddHeader("Accept", "application/json");
                    var response = restClient.Delete(request);
                    if (response.StatusCode == System.Net.HttpStatusCode.NoContent)
                    {
                        var history = new History();
                        history.Date = DateTime.Now;
                        history.Note = "Appointment got deleted from GCalendar";
                        history.Business = appointment.Business;
                        HistoryServices.Instance.SaveHistory(history);
                    }
                    else
                    {
                        var history = new History();
                        history.Date = DateTime.Now;
                        history.Note = response.Content;
                        history.Business = "Error";
                        HistoryServices.Instance.SaveHistory(history);
                    }
                }
                catch (Exception ex)
                {

                    var history = new History();
                    history.Date = DateTime.Now;
                    history.Note = ex.Message;
                    history.Business = "Error";
                    HistoryServices.Instance.SaveHistory(history);
                }



            }
            #region ReminderMailingRegion

            //var employee = EmployeeServices.Instance.GetEmployee(appointment.EmployeeID);
            var customer = CustomerServices.Instance.GetCustomer(appointment.CustomerID);

            if (customer.Password == null)
            {
                Random random = new Random();
                customer.Password = new string(Enumerable.Repeat("abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789", 8).Select(s => s[random.Next(s.Length)]).ToArray());
                CustomerServices.Instance.UpdateCustomer(customer);
            }
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
            var emailDetails = EmailTemplateServices.Instance.GetEmailTemplateWRTBusiness(company.Business, "Appointment Payment Reminder");
            if (emailDetails != null && emailDetails.IsActive == true)
            {
                string emailBody = "<html><body>";
                emailBody += "<h2 style='font-family: Arial, sans-serif;'>Appointment Payment Reminder</h2>";
                emailBody += emailDetails.TemplateCode;
                emailBody = emailBody.Replace("{{Customer_first_name}}", customer.FirstName);
                //emailBody = emailBody.Replace("{{Customer_last_name}}", customer.LastName);
                //emailBody = emailBody.Replace("{{Customer_initial}}", "Dear");
                emailBody = emailBody.Replace("{{date}}", appointment.Date.ToString("yyyy-MM-dd"));
                emailBody = emailBody.Replace("{{time}}", appointment.Time.ToString("H:mm:ss"));
                //emailBody = emailBody.Replace("{{end_time}}", appointment.EndTime.ToString("H:mm:ss"));
                //emailBody = emailBody.Replace("{{employee}}", employee.Name);
                //emailBody = emailBody.Replace("{{employee_picture}}", $"<img class='text-center' style='height:50px;width:auto;' src='{"http://app.yourbookingplatform.com" + employee.Photo}'>");
                emailBody = emailBody.Replace("{{services}}", ConcatenatedServices);
                emailBody = emailBody.Replace("{{company_name}}", company.Business);
                emailBody = emailBody.Replace("{{company_email}}", company.NotificationEmail);
                emailBody = emailBody.Replace("{{company_address}}", company.Address);
                emailBody = emailBody.Replace("{{company_logo}}", $"<img class='text-center' style='height:50px;width:auto;' src='{"http://app.yourbookingplatform.com" + company.Logo}'>");
                emailBody = emailBody.Replace("{{company_phone}}", company.PhoneNumber);
                emailBody = emailBody.Replace("{{password}}", customer.Password);

                emailBody += "</body></html>";


                if (IsValidEmail(customer.Email))
                {
                    SendEmail(customer.Email, "Appointment Payment Awaiting", emailBody, company);
                }
            }
            #endregion

            return RedirectToAction("Index", "Booking", new { businessName = paymentIntent.Metadata["BusinessName"] });
        }

        [HttpPost]
        public ActionResult RescheduleAppointment(int AppointmentID, string Time, DateTime Date, int SelectedEmployeeID, string Notes,
            int OnlinePriceChange = 0)
        {
            var appointment = AppointmentServices.Instance.GetAppointment(AppointmentID);


            var oldDate = appointment.Date.ToString("yyyy-MM-dd");
            var oldTime = appointment.Time.ToString("HH:mm");
            var oldEmployee = appointment.EmployeeID;

            var company = CompanyServices.Instance.GetCompany().Where(x => x.Business == appointment.Business).FirstOrDefault();
            var customer = CustomerServices.Instance.GetCustomer(appointment.CustomerID);

            var StartTime = DateTime.Parse(Time.Split('-')[0].Trim());
            var EndTime = DateTime.Parse(Time.Split('-')[1].Trim());
            appointment.Date = Date;
            appointment.Time = StartTime;
            appointment.EmployeeID = SelectedEmployeeID;
            appointment.Every = "";
            appointment.Ends = "Never";
            appointment.Notes = Notes;
            appointment.CustomerID = customer.ID;
            appointment.OnlinePriceChange = OnlinePriceChange;
            appointment.IsWalkIn = false;
            string ConcatenatedServices = "";
            int MinsToBeAddedForEndTime = 0;
            float TotalCost = 0;
            string Durations = "";
            if (appointment.Service != null)
            {
                foreach (var item in appointment.Service.Split(',').ToList())
                {
                    var ServiceNew = ServiceServices.Instance.GetService(int.Parse(item));
                    if (ServiceNew != null)
                    {
                        TotalCost += ServiceNew.Price;
                        MinsToBeAddedForEndTime += int.Parse(ServiceNew.Duration.Replace("mins", "").Replace("Mins", ""));
                        if (Durations == "")
                        {
                            Durations = String.Join(",", ServiceNew.Duration);
                        }
                        else
                        {
                            Durations = String.Join(",", Durations, ServiceNew.Duration);
                        }
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

            }
            appointment.EndTime = appointment.Time.AddMinutes(MinsToBeAddedForEndTime);
            appointment.ServiceDuration = Durations;
            appointment.TotalCost = TotalCost - appointment.Deposit;
            var employee = EmployeeServices.Instance.GetEmployee(SelectedEmployeeID);
            AppointmentServices.Instance.UpdateAppointment(appointment);

            var reminder = ReminderServices.Instance.GetReminderWRTAppID(appointment.ID);
            if (reminder != null)
            {
                DateTime combinedDateTime = new DateTime(appointment.Date.Year, appointment.Date.Month, appointment.Date.Day, appointment.Time.Hour, appointment.Time.Minute, appointment.Time.Second);
                reminder.Date = combinedDateTime;
                ReminderServices.Instance.UpdateReminder(reminder);
            }
            else
            {
                reminder = new TheBookingPlatform.Entities.Reminder();
                reminder.Service = appointment.Service;
                reminder.Business = appointment.Business;
                DateTime combinedDateTime = new DateTime(appointment.Date.Year, appointment.Date.Month, appointment.Date.Day, appointment.Time.Hour, appointment.Time.Minute, appointment.Time.Second);
                reminder.Date = combinedDateTime;
                reminder.CustomerID = appointment.CustomerID;
                reminder.AppointmentID = appointment.ID;
                reminder.EmployeeID = appointment.EmployeeID;
                reminder.IsCancelled = appointment.IsCancelled;
                reminder.Paid = appointment.IsPaid;
                ReminderServices.Instance.SaveReminder(reminder);
            }



            var googleCalendar = GoogleCalendarServices.Instance.GetGoogleCalendarServicesWRTBusiness(appointment.Business);
            if (googleCalendar != null && !googleCalendar.Disabled)
            {
                var Oldemployee = EmployeeServices.Instance.GetEmployee(oldEmployee);
                var userCalendarID = "";
                if (oldEmployee != 0)
                {

                    userCalendarID = Oldemployee.GoogleCalendarID;
                }
                else
                {
                    userCalendarID = employee.GoogleCalendarID;

                }

                var Nurl = new System.Uri("https://www.googleapis.com/calendar/v3/calendars/" + userCalendarID + "/events/" + appointment.GoogleCalendarEventID);
                var NrestClient = new RestClient(Nurl);
                var Nrequest = new RestRequest();

                Nrequest.AddQueryParameter("key", "AIzaSyASKpY6I08IVKFMw3muX39uMzPc5sBDaSc");
                Nrequest.AddHeader("Authorization", "Bearer " + googleCalendar.AccessToken);
                Nrequest.AddHeader("Accept", "application/json");
                var Nresponse = NrestClient.Delete(Nrequest);


                int year = appointment.Date.Year;
                int month = appointment.Date.Month;
                int day = appointment.Date.Day;
                int starthour = appointment.Time.Hour;
                int startminute = appointment.Time.Minute;
                int startseconds = appointment.Time.Second;

                int endhour = appointment.EndTime.Hour;
                int endminute = appointment.EndTime.Minute;
                int endseconds = appointment.EndTime.Second;

                DateTime startDateNew = new DateTime(year, month, day, starthour, startminute, startseconds);
                DateTime endDateNew = new DateTime(year, month, day, endhour, endminute, endseconds);

                DateTime currentDateTime = DateTime.Now;
                TimeSpan offset = TimeZoneInfo.Local.GetUtcOffset(currentDateTime);



                // Retrieve Google Calendar services




                var url = $"https://www.googleapis.com/calendar/v3/calendars/{employee.GoogleCalendarID}/events";
                var finalUrl = new Uri(url);

                RestClient restClient = new RestClient(finalUrl);
                RestRequest request = new RestRequest();
                var calendarEvent = new Event
                {
                    Summary = "Appointment at: " + appointment.Business,
                    Description = ConcatenatedServices,
                    Start = new EventDateTime() { DateTime = startDateNew.ToString("yyyy-MM-dd'T'HH:mm:ss.fffK"), TimeZone = company.TimeZone },
                    End = new EventDateTime() { DateTime = endDateNew.ToString("yyyy-MM-dd'T'HH:mm:ss.fffK"), TimeZone = company.TimeZone }
                };

                var model = JsonConvert.SerializeObject(calendarEvent, new JsonSerializerSettings
                {
                    ContractResolver = new CamelCasePropertyNamesContractResolver()
                });

                request.AddQueryParameter("key", "AIzaSyASKpY6I08IVKFMw3muX39uMzPc5sBDaSc");
                request.AddHeader("Authorization", "Bearer " + googleCalendar.AccessToken);
                request.AddHeader("Accept", "application/json");
                request.AddHeader("Content-Type", "application/json");
                request.AddParameter("application/json", model, ParameterType.RequestBody);

                var response = restClient.Post(request);

                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    JObject jsonObj = JObject.Parse(response.Content);
                    appointment.GoogleCalendarEventID = jsonObj["id"]?.ToString();
                    AppointmentServices.Instance.UpdateAppointment(appointment);
                }
                else
                {
                    var history = new History();
                    history.Date = DateTime.Now;
                    history.Note = response.Content;
                    history.Business = appointment.Business;
                    history.Type = "Error";
                    HistoryServices.Instance.SaveHistory(history);

                }
                //var calendarEvent = new Event();
                //int year = appointment.Date.Year;
                //int month = appointment.Date.Month;
                //int day = appointment.Date.Day;
                //int starthour = appointment.Time.Hour;
                //int startminute = appointment.Time.Minute;
                //int startseconds = appointment.Time.Second;

                //int endhour = appointment.EndTime.Hour;
                //int endminute = appointment.EndTime.Minute;
                //int endseconds = appointment.EndTime.Second;
                //var service = ServiceServices.Instance.GetService(int.Parse(appointment.Service));
                //DateTime startDateNew = new DateTime(year, month, day, starthour, startminute, startseconds);
                //DateTime EndDateNew = new DateTime(year, month, day, endhour, endminute, endseconds);
                //var url = new System.Uri("https://www.googleapis.com/calendar/v3/calendars/" + employee.GoogleCalendarID + "/events/" + appointment.GoogleCalendarEventID);
                //RestClient restClient = new RestClient(url);
                //RestRequest request = new RestRequest();

                //calendarEvent.Start = new EventDateTime() { DateTime = startDateNew.ToString("yyyy-MM-dd'T'HH:mm:ss.fffK"), TimeZone = company.TimeZone };
                //calendarEvent.End = new EventDateTime() { DateTime = EndDateNew.ToString("yyyy-MM-dd'T'HH:mm:ss.fffK"), TimeZone = company.TimeZone };
                //calendarEvent.Description = service.Name;
                //calendarEvent.Summary = "Appointment at: " + appointment.Business;


                //var model = JsonConvert.SerializeObject(calendarEvent, new JsonSerializerSettings
                //{
                //    ContractResolver = new CamelCasePropertyNamesContractResolver()
                //});

                //request.AddQueryParameter("key", "AIzaSyASKpY6I08IVKFMw3muX39uMzPc5sBDaSc");
                //request.AddHeader("Authorization", "Bearer " + googleCalendar.AccessToken);
                //request.AddHeader("Accept", "application/json");
                //request.AddHeader("Content-Type", "application/json");
                //request.AddParameter("application/json", model, ParameterType.RequestBody);

                //var resultant = restClient.Patch(request);

            }
            Session["CustomerLoggedIn"] = customer.ID;
            #region MailingRegion
            if (customer != null)
            {
                if (customer.Password == null)
                {
                    Random random = new Random();
                    customer.Password = new string(Enumerable.Repeat("abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789", 8).Select(s => s[random.Next(s.Length)]).ToArray());
                    CustomerServices.Instance.UpdateCustomer(customer);
                }

                var historyNew = new History();
                historyNew.Business = appointment.Business;
                historyNew.CustomerName = customer.FirstName + " " + customer.LastName;
                historyNew.Date = DateTime.Now;
                historyNew.Note = "Appointment was reschedule by " + historyNew.CustomerName + " Previous Date:" + oldDate + "Time was " + oldTime + "to new Date:" + appointment.Date.ToString("yyyy-MM-dd") + "and New Time is: " + appointment.Time.ToString("HH:mm");
                historyNew.EmployeeName = employee.Name;
                HistoryServices.Instance.SaveHistory(historyNew);

                var emailDetails = EmailTemplateServices.Instance.GetEmailTemplateWRTBusiness(appointment.Business, "Appointment Moved");
                if (emailDetails != null && emailDetails.IsActive == true)
                {
                    string emailBody = "<html><body>";
                    emailBody += "<h2 style='font-family: Arial, sans-serif;'>Appointment Moved</h2>";
                    emailBody += emailDetails.TemplateCode;
                    emailBody = emailBody.Replace("{{Customer_first_name}}", customer.FirstName);
                    emailBody = emailBody.Replace("{{Customer_last_name}}", customer.LastName);
                    emailBody = emailBody.Replace("{{Customer_initial}}", customer.Gender == "Male" ? "Mr." : "Ms.");
                    emailBody = emailBody.Replace("{{date}}", appointment.Date.ToString("yyyy-MM-dd"));
                    emailBody = emailBody.Replace("{{time}}", appointment.Time.ToString("H:mm:ss"));
                    emailBody = emailBody.Replace("{{end_time}}", appointment.EndTime.ToString("H:mm"));
                    emailBody = emailBody.Replace("{{employee_picture}}", $"<img class='text-center' style='height:50px;width:auto;' src='{"http://app.yourbookingplatform.com" + employee.Photo}'>");
                    emailBody = emailBody.Replace("{{previous_date}}", oldDate);
                    emailBody = emailBody.Replace("{{previous_time}}", oldTime);
                    emailBody = emailBody.Replace("{{employee}}", employee.Name);
                    emailBody = emailBody.Replace("{{employee_specialization}}", employee.Specialization);
                    emailBody = emailBody.Replace("{{services}}", ConcatenatedServices);
                    emailBody = emailBody.Replace("{{company_name}}", company.Business);
                    emailBody = emailBody.Replace("{{company_email}}", company.NotificationEmail);
                    emailBody = emailBody.Replace("{{password}}", customer.Password);
                    emailBody = emailBody.Replace("{{company_address}}", company.Address);
                    emailBody = emailBody.Replace("{{company_logo}}", $"<img class='text-center' style='height:50px;width:auto;' src='{"http://app.yourbookingplatform.com" + company.Logo}'>");
                    emailBody = emailBody.Replace("{{company_phone}}", company.PhoneNumber);
                    string cancelLink = string.Format("http://app.yourbookingplatform.com/Appointment/CancelByEmail/?AppointmentID={0}", appointment.ID);
                    emailBody = emailBody.Replace("{{cancellink}}", $"<a href='{cancelLink}' class='btn btn-primary'>CANCEL/RESCHEDULE</a>");

                    emailBody += "</body></html>";
                    if (IsValidEmail(customer.Email))
                    {
                        SendEmail(customer.Email, "Appointment Moved", emailBody, company);
                    }
                }
            }
            #endregion
            string scheme = Request.Url.Scheme;  // e.g., "http" or "https"
            string host = Request.Url.Host;      // e.g., "localhost"
            int port = Request.Url.Port;         // e.g., "44317"

            // Construct the main domain URL
            string mainDomain = $"{scheme}://{host}:{port}/";
            var link = mainDomain + Url.Action("CustomerProfile", "Booking", new { CustomerID = customer.ID, AppointmentID = appointment.ID, businessName = company.Business, SentBy = "Cancellation" });
            return Json(new { session = link });

        }


        [HttpGet]
        public JsonResult AfterPayment(int AppointmentID)
        {
            var appointment = AppointmentServices.Instance.GetAppointment(AppointmentID);
            var company = CompanyServices.Instance.GetCompany().Where(x => x.Business == appointment.Business).FirstOrDefault();
            var secretKey = company.APIKEY;
            StripeConfiguration.ApiKey = secretKey;




            var customer = CustomerServices.Instance.GetCustomer(appointment.CustomerID);
            string ConcatenatedServices = appointment.Service;
            int MinsToBeAddedForEndTime = 0;
            float TotalCost = 0;
            var ServicesOnly = new List<Service>();
            string Durations = "";
            if (appointment.Service != null)
            {
                foreach (var item in appointment.Service.Split(',').ToList())
                {
                    var ServiceNew = ServiceServices.Instance.GetService(int.Parse(item));
                    if (ServiceNew != null)
                    {
                        ServicesOnly.Add(ServiceNew);
                        TotalCost += ServiceNew.Price;
                        MinsToBeAddedForEndTime += int.Parse(ServiceNew.Duration.Replace("mins", "").Replace("Mins", ""));
                        if (Durations == "")
                        {
                            Durations = String.Join(",", ServiceNew.Duration);
                        }
                        else
                        {
                            Durations = String.Join(",", Durations, ServiceNew.Duration);
                        }
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

            }

            // Split the ServiceDuration string by commas
            string[] durations = appointment.Service.Split(',');

            // Get the number of values
            int numberOfServices = durations.Length;

            // Create a string with zeros for each service
            string zeros = string.Join(",", Enumerable.Repeat("0", numberOfServices));
            // Save the zeros string to ServiceDiscount







            long totalAmount = 0;
            //foreach (var item in model.ServicesOnly)
            //{
            //    totalAmount += Convert.ToInt64(item.Price * 100);
            //}

            totalAmount = Convert.ToInt64(appointment.Deposit * 100);

            var options = new PaymentIntentCreateOptions
            {
                Amount = totalAmount,
                Currency = company.Currency,
                PaymentMethodTypes = new List<string> { "card", "ideal" },
                Metadata = new Dictionary<string, string>
                {
                    { "CustomerID", customer.ID.ToString() },
                    { "AppointmentID", appointment.ID.ToString() },
                    { "BusinessName", company.Business }
                }
            };

            var service = new PaymentIntentService();
            var paymentIntent = service.Create(options);

            appointment.PaymentSession = paymentIntent.Id;
            AppointmentServices.Instance.UpdateAppointment(appointment);

            var scheme = Request.Url.Scheme;
            var successUrl = Url.Action("PaymentSuccess", "Booking", new { paymentIntentId = paymentIntent.Id, AppointmentID = appointment.ID }, scheme);
            var cancelUrl = Url.Action("PaymentCancel", "Booking", new { paymentIntentId = paymentIntent.Id, AppointmentID = appointment.ID }, scheme);

            // Return the URLs and client secret to the view
            ViewBag.ClientSecret = paymentIntent.ClientSecret;
            ViewBag.SuccessUrl = successUrl;
            ViewBag.CancelUrl = cancelUrl;
            // Generate the URLs for redirection


            AppointmentServices.Instance.UpdateAppointment(appointment);
            var profilelink = "http://app.yourbookingplatform.com" + Url.Action("CustomerProfile", "Booking", new { CustomerID = customer.ID, AppointmentID = appointment.ID, businessName = company.Business });
            var request = HttpContext.Request;
            var baseUrl = $"{request.Url.Scheme}://{request.Url.Authority}{Request.ApplicationPath.TrimEnd('/')}";
            var paymenturl = baseUrl + Url.Action("ProcessPayment", "Booking", new { businessName = company.Business, SuccessURL = successUrl, CancelURL = cancelUrl, PaymentSecret = paymentIntent.ClientSecret, AppointmentID = appointment.ID });

            return Json(new { session = paymenturl }, JsonRequestBehavior.AllowGet);
        }


        [HttpPost]
        public JsonResult Form(BookingViewModel model)
        {

            try
            {
                var serviceslist = new List<ServiceFormModel>();
                var company = CompanyServices.Instance.GetCompany(model.CompanyID);
                var secretKey = company.APIKEY;
                StripeConfiguration.ApiKey = secretKey;
                float Deposit = 0;
                string ConcatenatedServices = "";
                if (company.PaymentMethodIntegration)
                {
                    foreach (var item in model.ServiceIDs.Split(',').ToList())
                    {
                        var service = ServiceServices.Instance.GetService(int.Parse(item));
                        var Resource = ResourceServices.Instance.GetResource().Where(X => X.Services.Contains(item)).FirstOrDefault();
                        if (Resource != null)
                        {
                            serviceslist.Add(new ServiceFormModel { ID = service.ID, Name = service.Name, Duration = service.Duration, Price = service.Price, Resource = Resource });
                        }
                        else
                        {
                            serviceslist.Add(new ServiceFormModel { ID = service.ID, Name = service.Name, Duration = service.Duration, Price = service.Price });

                        }
                        Deposit += service.Price;
                    }
                    ConcatenatedServices = String.Join(",", serviceslist.Select(x => x.Name).ToList());
                    /////////////USE COUPON THING IF PRESENT


                }
                else
                {
                    foreach (var item in model.ServiceIDs.Split(',').ToList())
                    {
                        var service = ServiceServices.Instance.GetService(int.Parse(item));
                        serviceslist.Add(new ServiceFormModel { ID = service.ID, Name = service.Name, Duration = service.Duration, Price = service.Price });

                    }
                    ConcatenatedServices = String.Join(",",serviceslist.Select(x => x.Name).ToList());
                }
                if (model.CustomerID == 0)
                {

                    bool CustomerFound = false;
                    int FoundCustomerID = 0;

                    var customers = CustomerServices.Instance.GetCustomersWRTBusiness(company.Business, "");
                    var customer = new Customer();
                    customer.Business = company.Business;
                    customer.FirstName = model.FirstName;
                    customer.LastName = model.LastName;
                    customer.Email = model.Email;
                    customer.DateAdded = DateTime.Now;
                    customer.DateOfBirth = DateTime.Now;
                    customer.MobileNumber = model.MobileNumber;
                    foreach (var item in customers)
                    {
                        if (item.Email.Trim().ToLower() == customer.Email.Trim().ToLower())
                        {
                            CustomerFound = true;
                            FoundCustomerID = item.ID;
                            break;
                        }
                        else
                        {
                            CustomerFound = false;
                        }
                    }
                    if (!CustomerFound)
                    {

                        Random random = new Random();
                        customer.Password = new string(Enumerable.Repeat("abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789", 8).Select(s => s[random.Next(s.Length)]).ToArray());
                        CustomerServices.Instance.SaveCustomer(customer);
                    }
                    else
                    {
                        customer = CustomerServices.Instance.GetCustomer(FoundCustomerID);
                    }






                    var StartTime = DateTime.Parse(model.Time.Split('-')[0].Trim());
                    var EndTime = DateTime.Parse(model.Time.Split('-')[1].Trim());
                    model.ServicesOnly = serviceslist;
                    var appointment = new Appointment();
                    appointment.Business = company.Business;
                    appointment.BookingDate = DateTime.Now;
                    appointment.CouponID = model.CouponID;
                    appointment.CouponAssignmentID = model.CouponAssignmentID;


                    appointment.Date = model.Date;
                    appointment.Time = StartTime;
                    if (model.CouponID == 0)
                    {
                        if (company.Deposit != 0)
                        {
                            if (company.PaymentMethodIntegration)
                            {
                                appointment.Deposit = float.Parse(Convert.ToString(Deposit * (company.Deposit / 100)));
                            }
                        }
                        else
                        {
                            if (company.PaymentMethodIntegration)
                            {
                                appointment.Deposit = float.Parse(Convert.ToString(Deposit * 0));
                            }
                            else
                            {
                                appointment.Deposit = 0;

                            }
                        }
                    }
                    else
                    {
                        var coupon = CouponServices.Instance.GetCoupon(model.CouponID);
                        var couponAssignment = CouponServices.Instance.GetCouponAssignmentsWRTBusiness(company.Business, customer.ID).Where(x => x.CouponID == coupon.ID).FirstOrDefault();
                        couponAssignment.Used += 1;
                        CouponServices.Instance.UpdateCouponAssignment(couponAssignment);


                        if (company.Deposit != 0)
                        {
                            if (company.PaymentMethodIntegration)
                            {
                                appointment.Deposit = model.DepositText_;
                            }
                        }
                        else
                        {
                            if (company.PaymentMethodIntegration)
                            {
                                appointment.Deposit = model.DepositText_;
                            }
                            else
                            {
                                appointment.Deposit = 0;

                            }
                        }
                    }
                    appointment.DepositMethod = "Online";
                    appointment.EmployeeID = model.SelectedEmployeeID;
                    if (model.By != null && model.By != "")
                    {
                        appointment.BookedFromRebook = true;
                    }
                    else
                    {
                        appointment.BookedFromRebook = false;

                    }
                    appointment.Every = "";
                    appointment.Ends = "Never";

                    appointment.Notes = model.Comment;

                    appointment.CustomerID = customer.ID;
                    appointment.AnyAvailableEmployeeSelected = model.AnyAvailableEmployeeSelected;
                    appointment.IsWalkIn = false;
                    appointment.EmployeePriceChange = model.EmployeePriceChange;
                    appointment.OnlinePriceChange = model.OnlinePriceChange;
                    var ConcatenatedResource = "";
                    int MinsToBeAddedForEndTime = 0;
                    float TotalCost = 0;
                    string Durations = "";
                    if (model.ServiceIDs != null)
                    {
                        foreach (var item in model.ServicesOnly)
                        {

                            TotalCost += item.Price;
                            MinsToBeAddedForEndTime += int.Parse(item.Duration.Replace("mins", "").Replace("Mins", ""));
                            if (Durations == "")
                            {
                                Durations = String.Join(",", item.Duration);
                            }
                            else
                            {
                                Durations = String.Join(",", Durations, item.Duration);
                            }
                      


                        }

                    }
                    appointment.EndTime = appointment.Time.AddMinutes(MinsToBeAddedForEndTime);
                    appointment.Service = model.ServiceIDs;

                    // Split the ServiceDuration string by commas
                    var countofservices = appointment.Service.Split(',').ToList().Count();

                    List<string> zeroes = new List<string>();

                    for (int i = 0; i < countofservices; i++)
                    {
                        zeroes.Add("0");
                    }

                    string result = string.Join(",", zeroes);

                    // Save the zeros string to ServiceDiscount
                    appointment.ServiceDiscount = result;
                    appointment.ServiceDuration = Durations;
                    appointment.TotalCost = TotalCost - appointment.Deposit;
                    appointment.Color = "#952AB2";
                    if (company.PaymentMethodIntegration)
                    {
                        appointment.IsPaid = false;
                    }
                    else
                    {
                        appointment.IsPaid = true;
                    }
                    //var appointmentmightbeconflicting = AppointmentServices.Instance.GetAppointmentBookingWRTBusiness(company.Business, false, false, appointment.EmployeeID)
                    //    .Where(x => x.Date.ToString("yyyy-MM-dd") == appointment.Date.ToString("yyyy-MM-dd")
                    //    && x.Time.ToString("HH:mm") == appointment.Time.ToString("HH:mm")).ToList();
                    //if (appointmentmightbeconflicting.Count() > 0)
                    //{
                    //    return Json(new { success = false, Message = "The Time Slot was just booked by someone else, kindly change the timeslot selected" }, JsonRequestBehavior.AllowGet);
                    //}
                    //else
                    //{                    //var appointmentmightbeconflicting = AppointmentServices.Instance.GetAppointmentBookingWRTBusiness(company.Business, false, false, appointment.EmployeeID)
                    //    .Where(x => x.Date.ToString("yyyy-MM-dd") == appointment.Date.ToString("yyyy-MM-dd")
                    //    && x.Time.ToString("HH:mm") == appointment.Time.ToString("HH:mm")).ToList();
                    //if (appointmentmightbeconflicting.Count() > 0)
                    //{
                    //    return Json(new { success = false, Message = "The Time Slot was just booked by someone else, kindly change the timeslot selected" }, JsonRequestBehavior.AllowGet);
                    //}
                    //else
                    //{


                        AppointmentServices.Instance.SaveAppointment(appointment);


                        var reminder = new TheBookingPlatform.Entities.Reminder();
                        reminder.Service = appointment.Service;
                        reminder.Business = appointment.Business;
                        reminder.AppointmentID = appointment.ID;
                        DateTime combinedDateTime = new DateTime(appointment.Date.Year, appointment.Date.Month, appointment.Date.Day, appointment.Time.Hour, appointment.Time.Minute, appointment.Time.Second);
                        reminder.Date = combinedDateTime;
                        reminder.CustomerID = appointment.CustomerID;
                        reminder.EmployeeID = appointment.EmployeeID;
                        ReminderServices.Instance.SaveReminder(reminder);


                        var employee = EmployeeServices.Instance.GetEmployee(appointment.EmployeeID);
                        var history = new History();
                        if (employee != null)
                        {
                            history.EmployeeName = employee.Name;
                        }
                        else
                        {
                            history.EmployeeName = "Any Available Specialist";
                        }
                        history.CustomerName = customer.FirstName + " " + customer.LastName;
                        history.Note = "Appointment ID:" + appointment.ID + "   Online Appointment was created for " + history.CustomerName + "";
                        history.Date = DateTime.Now;
                        history.Business = appointment.Business;
                        history.Type = "General";
                        HistoryServices.Instance.SaveHistory(history);

                        long totalAmountDeposit = (long)appointment.Deposit;
                        var googleCalendar = GoogleCalendarServices.Instance.GetGoogleCalendarServicesWRTBusiness(appointment.Business);
                        if (googleCalendar != null && !googleCalendar.Disabled)
                        {
                            RefreshToken(appointment.Business);
                            GenerateonGoogleCalendar(appointment, ConcatenatedServices, appointment.Business);
                        }

                        if (company.PaymentMethodIntegration)
                        {
                            long totalAmount = 0;
                            //foreach (var item in model.ServicesOnly)
                            //{
                            //    totalAmount += Convert.ToInt64(item.Price * 100);
                            //}

                            totalAmount = Convert.ToInt64(appointment.Deposit * 100);
                            if (totalAmount > 50)
                            {
                                var options = new PaymentIntentCreateOptions
                                {
                                    Amount = totalAmount,
                                    Currency = company.Currency,
                                    PaymentMethodTypes = new List<string> { "card", "ideal" },
                                    Metadata = new Dictionary<string, string>
                                {
                                    { "CustomerID", customer.ID.ToString() },
                                    { "AppointmentID", appointment.ID.ToString() },
                                    { "BusinessName", company.Business }
                                }
                                };

                                var service = new PaymentIntentService();
                                var paymentIntent = service.Create(options);
                                appointment.PaymentSession = paymentIntent.Id;
                                AppointmentServices.Instance.UpdateAppointment(appointment);
                                var scheme = Request.Url.Scheme;
                                var successUrl = Url.Action("PaymentSuccess", "Booking", new { paymentIntentId = paymentIntent.Id, AppointmentID = appointment.ID }, scheme);
                                var cancelUrl = Url.Action("PaymentCancel", "Booking", new { paymentIntentId = paymentIntent.Id, AppointmentID = appointment.ID }, scheme);

                                // Return the URLs and client secret to the view
                                ViewBag.ClientSecret = paymentIntent.ClientSecret;
                                ViewBag.SuccessUrl = successUrl;
                                ViewBag.CancelUrl = cancelUrl;
                                // Generate the URLs for redirection


                                AppointmentServices.Instance.UpdateAppointment(appointment);
                                var profilelink = "http://app.yourbookingplatform.com" + Url.Action("CustomerProfile", "Booking", new { CustomerID = customer.ID, AppointmentID = appointment.ID, businessName = company.Business });
                                var request = HttpContext.Request;
                                var baseUrl = $"{request.Url.Scheme}://{request.Url.Authority}{Request.ApplicationPath.TrimEnd('/')}";
                                var paymenturl = baseUrl + Url.Action("ProcessPayment", "Booking", new { businessName = company.Business, SuccessURL = successUrl, CancelURL = cancelUrl, PaymentSecret = paymentIntent.ClientSecret, AppointmentID = appointment.ID });
                                return Json(new { session = paymenturl, ProfileLink = profilelink });
                            }
                            else
                            {
                                appointment.IsPaid = true;
                                AppointmentServices.Instance.UpdateAppointment(appointment);

                                var historyn = new History();
                                historyn.EmployeeName = employee.Name;
                                historyn.CustomerName = customer.FirstName + " " + customer.LastName;
                                historyn.Note = "Online Appointment was paid for " + historyn.CustomerName + "";
                                historyn.Date = DateTime.Now;
                                historyn.Business = appointment.Business;
                                historyn.Type = "General";
                                HistoryServices.Instance.SaveHistory(historyn);

                                #region ConfirmationEmailSender
                                var EmailTemplate = "";
                                if (!AppointmentServices.Instance.IsNewCustomer(company.Business, appointment.CustomerID))
                                {
                                    EmailTemplate = "First Registration Email";
                                }
                                else
                                {
                                    EmailTemplate = "Appointment Confirmation";
                                }





                                string emailBody = "<html><body>";
                                var emailTemplate = EmailTemplateServices.Instance.GetEmailTemplateWRTBusiness(appointment.Business, EmailTemplate);
                                if (emailTemplate != null)
                                {
                                    emailBody += "<h2 style='font-family: Arial, sans-serif;'>" + EmailTemplate + "</h2>";
                                    emailBody += emailTemplate.TemplateCode;
                                    emailBody = emailBody.Replace("{{Customer_first_name}}", customer.FirstName);
                                    emailBody = emailBody.Replace("{{Customer_last_name}}", customer.LastName);
                                    emailBody = emailBody.Replace("{{Customer_initial}}", "Dear");
                                    emailBody = emailBody.Replace("{{date}}", appointment.Date.ToString("yyyy-MM-dd"));
                                    emailBody = emailBody.Replace("{{time}}", appointment.Time.ToString("H:mm:ss"));
                                    emailBody = emailBody.Replace("{{end_time}}", appointment.EndTime.ToString("H:mm:ss"));
                                    emailBody = emailBody.Replace("{{employee}}", employee.Name);
                                    emailBody = emailBody.Replace("{{employee_specialization}}", employee.Specialization);
                                    emailBody = emailBody.Replace("{{employee_picture}}", $"<img class='text-center' style='height:50px;width:auto;' src='{"http://app.yourbookingplatform.com" + employee.Photo}'>");

                                    emailBody = emailBody.Replace("{{services}}", ConcatenatedServices);
                                    emailBody = emailBody.Replace("{{company_name}}", company.Business);
                                    emailBody = emailBody.Replace("{{company_email}}", company.NotificationEmail);
                                    emailBody = emailBody.Replace("{{company_address}}", company.Address);
                                    emailBody = emailBody.Replace("{{company_logo}}", $"<img class='text-center' style='height:50px;width:auto;' src='{"http://app.yourbookingplatform.com" + company.Logo}'>");
                                    emailBody = emailBody.Replace("{{company_phone}}", company.PhoneNumber);
                                    emailBody = emailBody.Replace("{{password}}", customer.Password);
                                    string cancelLink = string.Format("http://app.yourbookingplatform.com/Appointment/CancelByEmail/?AppointmentID={0}", appointment.ID);
                                    emailBody = emailBody.Replace("{{cancellink}}", $"<a href='{cancelLink}' class='btn btn-primary'>CANCEL/RESCHEDULE</a>");
                                    emailBody += "</body></html>";
                                    SendEmail(customer.Email, EmailTemplate, emailBody, company);
                                }
                                #endregion


                                var request = HttpContext.Request;
                                var baseUrl = $"{request.Url.Scheme}://{request.Url.Authority}{Request.ApplicationPath.TrimEnd('/')}";
                                var link = baseUrl + Url.Action("CustomerProfile", "Booking", new { CustomerID = customer.ID, AppointmentID = appointment.ID, businessName = company.Business });
                                return Json(new { session = link, });

                            }

                        }
                        else
                        {
                            var historyn = new History();
                            if (employee != null)
                            {
                                historyn.EmployeeName = employee.Name;
                            }
                            else
                            {
                                historyn.EmployeeName = "Any Available Specialist";
                            }
                            historyn.CustomerName = customer.FirstName + " " + customer.LastName;
                            historyn.Note = "Online Appointment was paid for " + historyn.CustomerName + "";
                            historyn.Date = DateTime.Now;
                            historyn.Business = appointment.Business;
                            historyn.Type = "General";
                            HistoryServices.Instance.SaveHistory(historyn);

                            #region ConfirmationEmailSender
                            var EmailTemplate = "";
                            if (!AppointmentServices.Instance.IsNewCustomer(company.Business, appointment.CustomerID))
                            {
                                EmailTemplate = "First Registration Email";
                            }
                            else
                            {
                                EmailTemplate = "Appointment Confirmation";
                            }





                            string emailBody = "<html><body>";
                            var emailTemplate = EmailTemplateServices.Instance.GetEmailTemplateWRTBusiness(appointment.Business, EmailTemplate);
                            if (emailTemplate != null)
                            {
                                emailBody += "<h2 style='font-family: Arial, sans-serif;'>" + EmailTemplate + "</h2>";
                                emailBody += emailTemplate.TemplateCode;
                                emailBody = emailBody.Replace("{{Customer_first_name}}", customer.FirstName);
                                emailBody = emailBody.Replace("{{Customer_last_name}}", customer.LastName);
                                emailBody = emailBody.Replace("{{Customer_initial}}", "Dear");
                                emailBody = emailBody.Replace("{{date}}", appointment.Date.ToString("yyyy-MM-dd"));
                                emailBody = emailBody.Replace("{{time}}", appointment.Time.ToString("H:mm:ss"));
                                emailBody = emailBody.Replace("{{end_time}}", appointment.EndTime.ToString("H:mm:ss"));
                                emailBody = emailBody.Replace("{{employee}}", employee.Name);
                                emailBody = emailBody.Replace("{{employee_specialization}}", employee.Specialization);
                                emailBody = emailBody.Replace("{{employee_picture}}", $"<img class='text-center' style='height:50px;width:auto;' src='{"http://app.yourbookingplatform.com" + employee.Photo}'>");

                                emailBody = emailBody.Replace("{{services}}", ConcatenatedServices);
                                emailBody = emailBody.Replace("{{company_name}}", company.Business);
                                emailBody = emailBody.Replace("{{company_email}}", company.NotificationEmail);
                                emailBody = emailBody.Replace("{{company_address}}", company.Address);
                                emailBody = emailBody.Replace("{{company_logo}}", $"<img class='text-center' style='height:50px;width:auto;' src='{"http://app.yourbookingplatform.com" + company.Logo}'>");
                                emailBody = emailBody.Replace("{{company_phone}}", company.PhoneNumber);
                                emailBody = emailBody.Replace("{{password}}", customer.Password);
                                string cancelLink = string.Format("http://app.yourbookingplatform.com/Appointment/CancelByEmail/?AppointmentID={0}", appointment.ID);
                                emailBody = emailBody.Replace("{{cancellink}}", $"<a href='{cancelLink}' class='btn btn-primary'>CANCEL/RESCHEDULE</a>");
                                emailBody += "</body></html>";
                                SendEmail(customer.Email, EmailTemplate, emailBody, company);
                            }
                            #endregion
                            if (CustomerFound)
                            {

                                var request = HttpContext.Request;
                                var baseUrl = $"{request.Url.Scheme}://{request.Url.Authority}{Request.ApplicationPath.TrimEnd('/')}";
                                var link = baseUrl + Url.Action("CustomerProfile", "Booking", new { CustomerID = customer.ID, AppointmentID = appointment.ID, businessName = company.Business });
                                return Json(new { session = link, });
                            }
                            else
                            {
                                var request = HttpContext.Request;
                                var baseUrl = $"{request.Url.Scheme}://{request.Url.Authority}{Request.ApplicationPath.TrimEnd('/')}";
                                var link = baseUrl + Url.Action("CustomerProfile", "Booking", new { CustomerID = customer.ID, AppointmentID = appointment.ID, businessName = company.Business });
                                return Json(new { session = link });

                            }

                        }
                    //}
                }
                else
                {
                    var customer = CustomerServices.Instance.GetCustomer(model.CustomerID);




                    var StartTime = DateTime.Parse(model.Time.Split('-')[0].Trim());
                    var EndTime = DateTime.Parse(model.Time.Split('-')[1].Trim());
                    model.ServicesOnly = serviceslist;
                    var appointment = new Appointment();
                    appointment.Business = company.Business;
                    appointment.CouponID = model.CouponID;
                    appointment.CouponAssignmentID = model.CouponAssignmentID;
                    appointment.BookingDate = DateTime.Now;
                    appointment.Date = model.Date;
                    appointment.Time = StartTime;


                    if (model.CouponID == 0)
                    {
                        if (company.Deposit != 0)
                        {
                            if (company.PaymentMethodIntegration)
                            {
                                appointment.Deposit = float.Parse(Convert.ToString(Deposit * (company.Deposit / 100)));
                            }
                        }
                        else
                        {
                            if (company.PaymentMethodIntegration)
                            {
                                appointment.Deposit = float.Parse(Convert.ToString(Deposit * 0));
                            }
                            else
                            {
                                appointment.Deposit = 0;

                            }
                        }
                    }
                    else
                    {
                        var coupon = CouponServices.Instance.GetCoupon(model.CouponID);
                        var couponAssignment = CouponServices.Instance.GetCouponAssignmentsWRTBusiness(company.Business, customer.ID).Where(x => x.CouponID == coupon.ID).FirstOrDefault();
                        couponAssignment.Used += 1;
                        CouponServices.Instance.UpdateCouponAssignment(couponAssignment);


                        if (company.Deposit != 0)
                        {
                            if (company.PaymentMethodIntegration)
                            {
                                appointment.Deposit = model.DepositText_;
                            }
                        }
                        else
                        {
                            if (company.PaymentMethodIntegration)
                            {
                                appointment.Deposit = model.DepositText_;
                            }
                            else
                            {
                                appointment.Deposit = 0;

                            }
                        }
                    }



                    appointment.DepositMethod = "Online";
                    appointment.EmployeeID = model.SelectedEmployeeID;

                    appointment.Every = "";
                    appointment.Ends = "Never";


                    appointment.Notes = model.Comment;

                    appointment.CustomerID = customer.ID;
                    appointment.EmployeePriceChange = model.EmployeePriceChange;
                    appointment.OnlinePriceChange = model.OnlinePriceChange;

                    appointment.IsWalkIn = false;
                    int MinsToBeAddedForEndTime = 0;
                    float TotalCost = 0;
                    string Durations = "";
                    var ConcatenatedResource = "";
                    if (model.ServiceIDs != null)
                    {
                        foreach (var item in model.ServicesOnly)
                        {
                            var ServiceNew = ServiceServices.Instance.GetService(item.ID);
                            if (ServiceNew != null)
                            {
                                TotalCost += ServiceNew.Price;
                                MinsToBeAddedForEndTime += int.Parse(ServiceNew.Duration.Replace("mins", "").Replace("Mins", ""));
                                if (Durations == "")
                                {
                                    Durations = String.Join(",", ServiceNew.Duration);
                                }
                                else
                                {
                                    Durations = String.Join(",", Durations, ServiceNew.Duration);
                                }
                               
                                if (item.Resource != null)
                                {
                                    if (ConcatenatedResource == "")
                                    {
                                        ConcatenatedResource = String.Join(",", item.ID + "_SER_RES_" + item.Resource.ID);
                                    }
                                    else
                                    {
                                        ConcatenatedResource = String.Join(",", ConcatenatedResource, item.ID + "_SER_RES_" + item.Resource.ID);

                                    }
                                }

                            }
                        }

                    }
                    appointment.EndTime = appointment.Time.AddMinutes(MinsToBeAddedForEndTime);
                    appointment.Service = model.ServiceIDs;

                    // Split the ServiceDuration string by commas
                    var countofservices = appointment.Service.Split(',').ToList().Count();

                    List<string> zeroes = new List<string>();

                    for (int i = 0; i < countofservices; i++)
                    {
                        zeroes.Add("0");
                    }

                    string result = string.Join(",", zeroes);

                    // Save the zeros string to ServiceDiscount
                    appointment.ServiceDiscount = result;

                    appointment.ServiceDuration = Durations;
                    appointment.TotalCost = TotalCost - appointment.Deposit;
                    appointment.Color = "#952AB2";
                    var employee = new Employee();
                    if (company.PaymentMethodIntegration)
                    {
                        appointment.IsPaid = false;

                    }
                    else
                    {
                        appointment.IsPaid = true;
                    }
                    //var appointmentmightbeconflicting = AppointmentServices.Instance.GetAppointmentBookingWRTBusiness(company.Business, false, false, appointment.EmployeeID)
                    //    .Where(x => x.Date.ToString("yyyy-MM-dd") == appointment.Date.ToString("yyyy-MM-dd")
                    //    && x.Time.ToString("HH:mm") == appointment.Time.ToString("HH:mm")).ToList();
                    //if (appointmentmightbeconflicting.Count() > 0)
                    //{
                    //    return Json(new { success = false, Message = "The Time Slot was just booked by someone else, kindly change the timeslot selected" }, JsonRequestBehavior.AllowGet);
                    //}
                    //else
                    //{

                        AppointmentServices.Instance.SaveAppointment(appointment);
                        
                        var reminder = new TheBookingPlatform.Entities.Reminder();
                        reminder.Service = appointment.Service;
                        reminder.Business = appointment.Business;
                        reminder.AppointmentID = appointment.ID;
                        DateTime combinedDateTime = new DateTime(appointment.Date.Year, appointment.Date.Month, appointment.Date.Day, appointment.Time.Hour, appointment.Time.Minute, appointment.Time.Second);
                        reminder.Date = combinedDateTime;
                        reminder.CustomerID = appointment.CustomerID;
                        reminder.EmployeeID = appointment.EmployeeID;
                        ReminderServices.Instance.SaveReminder(reminder);

                        var history = new History();
                        employee = EmployeeServices.Instance.GetEmployee(appointment.EmployeeID);
                        if (employee != null)
                        {
                            history.EmployeeName = employee.Name;

                        }
                        history.CustomerName = customer.FirstName + " " + customer.LastName;
                        history.Note = "Online Appointment was created for " + history.CustomerName + "";
                        history.Date = DateTime.Now;
                        history.Business = appointment.Business;
                        history.Type = "General";
                        HistoryServices.Instance.SaveHistory(history);
                        var googleCalendar = GoogleCalendarServices.Instance.GetGoogleCalendarServicesWRTBusiness(appointment.Business);
                        if (googleCalendar != null && !googleCalendar.Disabled)
                        {
                            RefreshToken(appointment.Business);
                            GenerateonGoogleCalendar(appointment, ConcatenatedServices, appointment.Business);
                        }


                        //float totalAmount = model.ServicesOnly.Sum(x => x.Price);
                        //long totalAmountDeposit = (long)appointment.Deposit;
                        if (company.PaymentMethodIntegration)
                        {
                            long totalAmount = 0;
                            //foreach (var item in model.ServicesOnly)
                            //{
                            //    totalAmount += Convert.ToInt64(item.Price * 100);
                            //}

                            totalAmount = Convert.ToInt64(appointment.Deposit * 100);
                            if (totalAmount > 50)
                            {
                                var options = new PaymentIntentCreateOptions
                                {
                                    Amount = totalAmount,
                                    Currency = company.Currency,
                                    PaymentMethodTypes = new List<string> { "card", "ideal" },
                                    Metadata = new Dictionary<string, string>
                                {
                                    { "CustomerID", customer.ID.ToString() },
                                    { "AppointmentID", appointment.ID.ToString() },
                                    { "BusinessName", company.Business }
                                }
                                };

                                var service = new PaymentIntentService();
                                var paymentIntent = service.Create(options);

                                appointment.PaymentSession = paymentIntent.Id;
                                AppointmentServices.Instance.UpdateAppointment(appointment);

                                var scheme = Request.Url.Scheme;
                                var successUrl = Url.Action("PaymentSuccess", "Booking", new { paymentIntentId = paymentIntent.Id, AppointmentID = appointment.ID }, scheme);
                                var cancelUrl = Url.Action("PaymentCancel", "Booking", new { paymentIntentId = paymentIntent.Id, AppointmentID = appointment.ID }, scheme);

                                // Return the URLs and client secret to the view
                                ViewBag.ClientSecret = paymentIntent.ClientSecret;
                                ViewBag.SuccessUrl = successUrl;
                                ViewBag.CancelUrl = cancelUrl;
                                // Generate the URLs for redirection


                                AppointmentServices.Instance.UpdateAppointment(appointment);
                                var profilelink = "http://app.yourbookingplatform.com" + Url.Action("CustomerProfile", "Booking", new { CustomerID = customer.ID, AppointmentID = appointment.ID, businessName = company.Business });

                                Session["CustomerLoggedIn"] = customer.ID;
                                var request = HttpContext.Request;
                                var baseUrl = $"{request.Url.Scheme}://{request.Url.Authority}{Request.ApplicationPath.TrimEnd('/')}";
                                profilelink = baseUrl + Url.Action("CustomerProfile", "Booking", new { CustomerID = customer.ID, AppointmentID = appointment.ID, businessName = company.Business });
                                appointment.PaymentSession = paymentIntent.Id;
                                AppointmentServices.Instance.UpdateAppointment(appointment);
                                //return Json(new { session = session.Url, ProfileLink = profilelink });
                                return Json(new { session = paymentIntent.NextAction.RedirectToUrl.Url, ProfileLink = profilelink });
                            }
                            else
                            {
                                appointment.IsPaid = true;
                                AppointmentServices.Instance.UpdateAppointment(appointment);

                                var historyn = new History();
                                historyn.EmployeeName = employee.Name;
                                historyn.CustomerName = customer.FirstName + " " + customer.LastName;
                                historyn.Note = "Online Appointment was paid for " + historyn.CustomerName + "";
                                historyn.Date = DateTime.Now;
                                historyn.Business = appointment.Business;
                                historyn.Type = "General";
                                HistoryServices.Instance.SaveHistory(historyn);

                                #region ConfirmationEmailSender
                                var EmailTemplate = "";
                                if (!AppointmentServices.Instance.IsNewCustomer(company.Business, appointment.CustomerID))
                                {
                                    EmailTemplate = "First Registration Email";
                                }
                                else
                                {
                                    EmailTemplate = "Appointment Confirmation";
                                }





                                string emailBody = "<html><body>";
                                var emailTemplate = EmailTemplateServices.Instance.GetEmailTemplateWRTBusiness(appointment.Business, EmailTemplate);
                                if (emailTemplate != null)
                                {
                                    emailBody += "<h2 style='font-family: Arial, sans-serif;'>" + EmailTemplate + "</h2>";
                                    emailBody += emailTemplate.TemplateCode;
                                    emailBody = emailBody.Replace("{{Customer_first_name}}", customer.FirstName);
                                    emailBody = emailBody.Replace("{{Customer_last_name}}", customer.LastName);
                                    emailBody = emailBody.Replace("{{Customer_initial}}", "Dear");
                                    emailBody = emailBody.Replace("{{date}}", appointment.Date.ToString("yyyy-MM-dd"));
                                    emailBody = emailBody.Replace("{{time}}", appointment.Time.ToString("H:mm:ss"));
                                    emailBody = emailBody.Replace("{{end_time}}", appointment.EndTime.ToString("H:mm:ss"));
                                    emailBody = emailBody.Replace("{{employee}}", employee.Name);
                                    emailBody = emailBody.Replace("{{employee_specialization}}", employee.Specialization);
                                    emailBody = emailBody.Replace("{{employee_picture}}", $"<img class='text-center' style='height:50px;width:auto;' src='{"http://app.yourbookingplatform.com" + employee.Photo}'>");

                                    emailBody = emailBody.Replace("{{services}}", ConcatenatedServices);
                                    emailBody = emailBody.Replace("{{company_name}}", company.Business);
                                    emailBody = emailBody.Replace("{{company_email}}", company.NotificationEmail);
                                    emailBody = emailBody.Replace("{{company_address}}", company.Address);
                                    emailBody = emailBody.Replace("{{company_logo}}", $"<img class='text-center' style='height:50px;width:auto;' src='{"http://app.yourbookingplatform.com" + company.Logo}'>");
                                    emailBody = emailBody.Replace("{{company_phone}}", company.PhoneNumber);
                                    emailBody = emailBody.Replace("{{password}}", customer.Password);
                                    string cancelLink = string.Format("http://app.yourbookingplatform.com/Appointment/CancelByEmail/?AppointmentID={0}", appointment.ID);
                                    emailBody = emailBody.Replace("{{cancellink}}", $"<a href='{cancelLink}' class='btn btn-primary'>CANCEL/RESCHEDULE</a>");
                                    emailBody += "</body></html>";
                                    SendEmail(customer.Email, EmailTemplate, emailBody, company);
                                }
                                #endregion

                                var request = HttpContext.Request;
                                var baseUrl = $"{request.Url.Scheme}://{request.Url.Authority}{Request.ApplicationPath.TrimEnd('/')}";
                                var link = baseUrl + Url.Action("CustomerProfile", "Booking", new { CustomerID = customer.ID, AppointmentID = appointment.ID, businessName = company.Business });
                                return Json(new { session = link });
                            }
                        }
                        else
                        {
                            var historyn = new History();
                            historyn.EmployeeName = employee.Name;
                            historyn.CustomerName = customer.FirstName + " " + customer.LastName;
                            historyn.Note = "Online Appointment was paid for " + historyn.CustomerName + "";
                            historyn.Date = DateTime.Now;
                            historyn.Business = appointment.Business;
                            historyn.Type = "General";
                            HistoryServices.Instance.SaveHistory(historyn);

                            #region ConfirmationEmailSender
                            var EmailTemplate = "";
                            if (!AppointmentServices.Instance.IsNewCustomer(company.Business, appointment.CustomerID))
                            {
                                EmailTemplate = "First Registration Email";
                            }
                            else
                            {
                                EmailTemplate = "Appointment Confirmation";
                            }





                            string emailBody = "<html><body>";
                            var emailTemplate = EmailTemplateServices.Instance.GetEmailTemplateWRTBusiness(appointment.Business, EmailTemplate);
                            if (emailTemplate != null)
                            {
                                emailBody += "<h2 style='font-family: Arial, sans-serif;'>" + EmailTemplate + "</h2>";
                                emailBody += emailTemplate.TemplateCode;
                                emailBody = emailBody.Replace("{{Customer_first_name}}", customer.FirstName);
                                emailBody = emailBody.Replace("{{Customer_last_name}}", customer.LastName);
                                emailBody = emailBody.Replace("{{Customer_initial}}", "Dear");
                                emailBody = emailBody.Replace("{{date}}", appointment.Date.ToString("yyyy-MM-dd"));
                                emailBody = emailBody.Replace("{{time}}", appointment.Time.ToString("H:mm:ss"));
                                emailBody = emailBody.Replace("{{end_time}}", appointment.EndTime.ToString("H:mm:ss"));
                                emailBody = emailBody.Replace("{{employee}}", employee.Name);
                                emailBody = emailBody.Replace("{{employee_specialization}}", employee.Specialization);
                                emailBody = emailBody.Replace("{{employee_picture}}", $"<img class='text-center' style='height:50px;width:auto;' src='{"http://app.yourbookingplatform.com" + employee.Photo}'>");

                                emailBody = emailBody.Replace("{{services}}", ConcatenatedServices);
                                emailBody = emailBody.Replace("{{company_name}}", company.Business);
                                emailBody = emailBody.Replace("{{company_email}}", company.NotificationEmail);
                                emailBody = emailBody.Replace("{{company_address}}", company.Address);
                                emailBody = emailBody.Replace("{{company_logo}}", $"<img class='text-center' style='height:50px;width:auto;' src='{"http://app.yourbookingplatform.com" + company.Logo}'>");
                                emailBody = emailBody.Replace("{{company_phone}}", company.PhoneNumber);
                                emailBody = emailBody.Replace("{{password}}", customer.Password);
                                string cancelLink = string.Format("http://app.yourbookingplatform.com/Appointment/CancelByEmail/?AppointmentID={0}", appointment.ID);
                                emailBody = emailBody.Replace("{{cancellink}}", $"<a href='{cancelLink}' class='btn btn-primary'>CANCEL/RESCHEDULE</a>");
                                emailBody += "</body></html>";
                                SendEmail(customer.Email, EmailTemplate, emailBody, company);
                            }
                            #endregion
                            var request = HttpContext.Request;
                            var baseUrl = $"{request.Url.Scheme}://{request.Url.Authority}{Request.ApplicationPath.TrimEnd('/')}";
                            var link = baseUrl + Url.Action("CustomerProfile", "Booking", new { CustomerID = customer.ID, AppointmentID = appointment.ID, businessName = company.Business });
                            return Json(new { session = link });
                        }
                    //}
                }

            }
            catch (Exception ex)
            {

                throw;
            }
        }


        private void UpdateAppointmentStatus(Session session)
        {
            // Retrieve AppointmentID from metadata
            if (session.Metadata.TryGetValue("order_id", out var appointmentIdString))
            {
                if (int.TryParse(appointmentIdString, out var appointmentId))
                {

                    var appointment = AppointmentServices.Instance.GetAppointment(appointmentId);
                    appointment.IsPaid = true;
                    AppointmentServices.Instance.UpdateAppointment(appointment);
                    // Now you have the AppointmentID, and you can update the appointment status
                    // Use appointmentId variable in your logic
                }
                else
                {
                    // Log or handle the case where parsing AppointmentID fails
                }
            }
            else
            {
                // Log or handle the case where AppointmentID is not found in metadata
            }

            // Your logic to update the appointment status as paid
            // You can retrieve relevant information from the session object
            // For example, session.CustomerId, session.Metadata, etc.
        }

        private void UpdateAppointmentStatus(int iD, string v)
        {
            var appointment = AppointmentServices.Instance.GetAppointment(iD);
            appointment.IsPaid = true;
            AppointmentServices.Instance.UpdateAppointment(appointment);
        }


        public string GetNextDayStatus(DateTime CurrentWeekStart, DateTime rosterStartDate, string targetDayString)
        {
            // Calculate the difference in days between the roster start date and today
            if (!Enum.TryParse(targetDayString, true, out DayOfWeek targetDay))
            {
                return "Invalid day of the week specified.";
            }
            var FinalToBeCheckedDate = CurrentWeekStart;


            TimeSpan difference = FinalToBeCheckedDate - rosterStartDate.Date;

            // Calculate the number of weeks passed since the roster start date
            int weeksPassed = (int)(difference.TotalDays / 7);

            // Determine if the current week is odd or even
            bool isEvenWeek = weeksPassed % 2 == 0;
            bool isNextTargetDayInCurrentWeek = false;
            if (isEvenWeek && difference.TotalDays >= 0)
            {
                isNextTargetDayInCurrentWeek = true;
            }




            return isNextTargetDayInCurrentWeek ? "YES" : "NO";
        }


        [HttpPost]
        public JsonResult AddToWaitingList(BookingViewModel model)
        {
            try
            {
                var serviceslist = new List<ServiceFormModel>();
                var company = CompanyServices.Instance.GetCompany(model.CompanyID);
                float Deposit = 0;
                if (model.CustomerID == 0)
                {

                    bool CustomerFound = false;
                    int FoundCustomerID = 0;

                    var customers = CustomerServices.Instance.GetCustomersWRTBusiness(company.Business, "");
                    var customer = new Customer();
                    customer.Business = company.Business;
                    customer.FirstName = model.FirstName;
                    customer.LastName = model.LastName;
                    customer.Email = model.Email;
                    customer.DateOfBirth = DateTime.Now;
                    customer.DateAdded = DateTime.Now;
                    customer.MobileNumber = model.MobileNumber;
                    foreach (var item in customers)
                    {
                        if (item.Email.Trim().ToLower() == customer.Email.Trim().ToLower())
                        {
                            CustomerFound = true;
                            FoundCustomerID = item.ID;
                            break;
                        }
                        else
                        {
                            CustomerFound = false;
                        }
                    }
                    if (!CustomerFound)
                    {

                        Random random = new Random();
                        customer.Password = new string(Enumerable.Repeat("abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789", 8).Select(s => s[random.Next(s.Length)]).ToArray());
                        CustomerServices.Instance.SaveCustomer(customer);
                    }
                    else
                    {
                        customer = CustomerServices.Instance.GetCustomer(FoundCustomerID);
                    }



                    model.ServicesOnly = serviceslist;
                    var waitingList = new WaitingList();
                    waitingList.Business = company.Business;
                    waitingList.BookingDate = DateTime.Now;
                    waitingList.Date = model.Date;
                    waitingList.EmployeeID = model.SelectedEmployeeID;
                    waitingList.Notes = model.Comment;
                    waitingList.CustomerID = customer.ID;
                    string ConcatenatedServices = "";
                    int MinsToBeAddedForEndTime = 0;
                    float TotalCost = 0;
                    string Durations = "";
                    if (model.ServiceIDs != null)
                    {
                        foreach (var item in model.ServiceIDs.Split(',').ToList())
                        {
                            var ServiceNew = ServiceServices.Instance.GetService(int.Parse(item));
                            if (ServiceNew != null)
                            {
                                TotalCost += ServiceNew.Price;
                                MinsToBeAddedForEndTime += int.Parse(ServiceNew.Duration.Replace("mins", "").Replace("Mins", ""));
                                if (Durations == "")
                                {
                                    Durations = String.Join(",", ServiceNew.Duration);
                                }
                                else
                                {
                                    Durations = String.Join(",", Durations, ServiceNew.Duration);
                                }
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

                    }
                    waitingList.Service = model.ServiceIDs;

                    // Split the ServiceDuration string by commas
                    string[] durations = model.ServiceIDs.Split(',');

                    // Get the number of values
                    int numberOfServices = durations.Length;

                    // Create a string with zeros for each service
                    string zeros = string.Join(",", Enumerable.Repeat("0", numberOfServices));

                    // Save the zeros string to ServiceDiscount
                    waitingList.ServiceDiscount = zeros;

                    waitingList.ServiceDuration = Durations;
                    waitingList.TotalCost = TotalCost;
                    waitingList.Color = "#952AB2";
                    waitingList.Time = DateTime.Now;
                    if (model.SelectedEmployeeID == 0)
                    {
                        var finalServicesID = model.ServiceIDs.Split(',').ToList().Select(x => int.Parse(x)).ToList();
                        var employeeservices = EmployeeServiceServices.Instance.GetEmployeeService().Where(x => x.Business == company.Business).ToList();
                        var employeeIDs = employeeservices
                        .Where(es => finalServicesID.Contains(es.ServiceID))
                        .GroupBy(es => es.EmployeeID)
                        .Where(grp => grp.Select(g => g.ServiceID).Distinct().Count() == finalServicesID.Count)
                        .Select(grp => grp.Key)
                        .ToList();

                        waitingList.EmployeeIDs = String.Join(",", employeeIDs);
                        waitingList.EmployeeID = model.SelectedEmployeeID;
                        waitingList.NonSelectedEmployee = true;
                        WaitingListServices.Instance.SaveWaitingList(waitingList);

                    }
                    else
                    {
                        waitingList.EmployeeID = model.SelectedEmployeeID;
                        waitingList.NonSelectedEmployee = false;
                        WaitingListServices.Instance.SaveWaitingList(waitingList);

                    }
                    string scheme = Request.Url.Scheme;  // e.g., "http" or "https"
                    string host = Request.Url.Host;      // e.g., "localhost"
                    int port = Request.Url.Port;         // e.g., "44317"

                    // Construct the main domain URL
                    string mainDomain = $"{scheme}://{host}:{port}/";
                    if (CustomerFound)
                    {

                        var link = mainDomain + Url.Action("CustomerProfile", "Booking", new { CustomerID = customer.ID, businessName = company.Business, WaitingList = "Yes" });
                        return Json(new { session = link, });
                    }
                    else
                    {
                        var link = mainDomain + Url.Action("CustomerProfile", "Booking", new { CustomerID = 0, businessName = company.Business, WaitingList = "Yes" });
                        return Json(new { session = link });

                    }

                }
                else
                {
                    var customer = CustomerServices.Instance.GetCustomer(model.CustomerID);
                    model.ServicesOnly = serviceslist;
                    var waitingList = new WaitingList();
                    waitingList.Business = company.Business;
                    waitingList.BookingDate = DateTime.Now;
                    waitingList.Date = model.Date;
                    waitingList.EmployeeID = model.SelectedEmployeeID;
                    waitingList.Notes = model.Comment;
                    waitingList.CustomerID = customer.ID;
                    string ConcatenatedServices = "";
                    int MinsToBeAddedForEndTime = 0;
                    float TotalCost = 0;
                    string Durations = "";
                    if (model.ServiceIDs != null)
                    {
                        foreach (var item in model.ServiceIDs.Split(',').ToList())
                        {
                            var ServiceNew = ServiceServices.Instance.GetService(int.Parse(item));
                            if (ServiceNew != null)
                            {
                                TotalCost += ServiceNew.Price;
                                MinsToBeAddedForEndTime += int.Parse(ServiceNew.Duration.Replace("mins", "").Replace("Mins", ""));
                                if (Durations == "")
                                {
                                    Durations = String.Join(",", ServiceNew.Duration);
                                }
                                else
                                {
                                    Durations = String.Join(",", Durations, ServiceNew.Duration);
                                }
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

                    }
                    waitingList.Service = model.ServiceIDs;

                    // Split the ServiceDuration string by commas
                    string[] durations = model.ServiceIDs.Split(',');

                    // Get the number of values
                    int numberOfServices = durations.Length;

                    // Create a string with zeros for each service
                    string zeros = string.Join(",", Enumerable.Repeat("0", numberOfServices));

                    // Save the zeros string to ServiceDiscount
                    waitingList.ServiceDiscount = zeros;
                    waitingList.TotalCost = TotalCost;
                    waitingList.Color = "#952AB2";
                    waitingList.Time = DateTime.Now;
                    waitingList.ServiceDuration = Durations;
                    if (model.SelectedEmployeeID == 0)
                    {
                        var finalServicesID = model.ServiceIDs.Split(',').ToList().Select(x => int.Parse(x)).ToList();
                        var employeeservices = EmployeeServiceServices.Instance.GetEmployeeService().Where(x => x.Business == company.Business).ToList();
                        var employeeIDs = employeeservices
                        .Where(es => finalServicesID.Contains(es.ServiceID))
                        .GroupBy(es => es.EmployeeID)
                        .Where(grp => grp.Select(g => g.ServiceID).Distinct().Count() == finalServicesID.Count)
                        .Select(grp => grp.Key)
                        .ToList();

                        waitingList.EmployeeIDs = String.Join(",", employeeIDs);
                        waitingList.EmployeeID = model.SelectedEmployeeID;
                        waitingList.NonSelectedEmployee = true;
                        WaitingListServices.Instance.SaveWaitingList(waitingList);

                    }
                    else
                    {
                        waitingList.EmployeeID = model.SelectedEmployeeID;
                        WaitingListServices.Instance.SaveWaitingList(waitingList);

                    }
                    string scheme = Request.Url.Scheme;  // e.g., "http" or "https"
                    string host = Request.Url.Host;      // e.g., "localhost"
                    int port = Request.Url.Port;         // e.g., "44317"

                    // Construct the main domain URL
                    string mainDomain = $"{scheme}://{host}:{port}/";
                    var link = mainDomain + Url.Action("CustomerProfile", "Booking", new { CustomerID = customer.ID, businessName = company.Business, WaitingList = "Yes" });
                    return Json(new { session = link, });
                }

            }
            catch (Exception ex)
            {

                throw;
            }
        }


        public bool IsSlotWithinEmployeeTimeRange(DateTime slotStartTime, DateTime slotEndTime, DateTime employeeStartTime, DateTime employeeEndTime)
        {
            // Ensure slot times are within the employee's available times
            return slotStartTime.TimeOfDay >= employeeStartTime.TimeOfDay && slotEndTime.TimeOfDay <= employeeEndTime.TimeOfDay;
        }

        [HttpGet]
        public JsonResult GetEndTime(DateTime StartTime, string Duration)
        {
            var duration = Duration.Replace("mins", "").Replace("Mins", "");
            var endtime = StartTime.AddMinutes(int.Parse(duration));
            return Json(new { success = true, EndTime = endtime.ToString("HH:mm") }, JsonRequestBehavior.AllowGet);
        }

        static bool IsDateInRangeNew(DateTime dateToCheck, DateTime startDate)
        {
            // Extracting only the date part without the time part
            DateTime dateOnlyToCheck = dateToCheck.Date;
            DateTime startDateOnly = startDate.Date;

            if (dateOnlyToCheck >= startDateOnly)
            {
                return true; // If it's greater, it's definitely not in range
            }
            else
            {
                return false;
            }

            // Checking if the date falls within the range

            // Checking if the date falls within the range
        }


        [HttpGet]
        public JsonResult GetDisabledDaysNEW(string businessName, int EmployeeID, string ServiceIds, string monthYear, string PrevOrNext = "", string date = "")
        {
            // Example list of disabled days (replace this with your actual data)
            //List<string> allDays = new List<string> { "Sunday", "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday" };
            var ServiceData = ServiceIds.Split(',').ToList();
            bool HaveMultiple = false;
            List<DateTime> disabledDays = new List<DateTime>();
            var ListOfEmployeeSlotsCount = new List<SlotsWithEmployeeIDModel>();
            if (EmployeeID == 0)
            {
                var holidays = HolidayServices.Instance.GetHolidayWRTBusiness(businessName, "");
                foreach (var holiday in holidays)
                {
                    disabledDays.Add(holiday.Date);
                }

                return Json(new { HaveMultiple = HaveMultiple, disabledDays = disabledDays, FinalSelectedDate = DateTime.Now, EmployeeID = 0 }, JsonRequestBehavior.AllowGet);
            }




            else
            {
                if (date != "")
                {

                    return Json(new { disabledDays = disabledDays, EmployeeID = EmployeeID }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    var currentDate = DateTime.Now;
                    var FinalSelectedDate = DateTime.Now;
                    if (monthYear != null)
                    {
                        if (PrevOrNext == "Next")
                        {
                            currentDate = DateTime.Parse(monthYear).AddMonths(1);
                        }
                        else if (PrevOrNext == "Prev")
                        {
                            currentDate = DateTime.Parse(monthYear).AddMonths(-1);

                        }
                        else
                        {
                            currentDate = DateTime.Parse(monthYear);
                        }
                    }
                    DateTime SelectedDate = DateTime.Now;
                    if (currentDate.Month != DateTime.Now.Month || currentDate.Year != DateTime.Now.Year)
                    {
                        SelectedDate = currentDate;
                    }
                    var Allshifts = new List<ShiftOfEmployeeModel>();
                    var empShifts = new List<ShiftModel>();

                    FinalSelectedDate = SelectedDate;

                    int TimeInMinutes = 0;
                    var ServicesIDs = ServiceIds.Trim().Split(',').ToList();
                    var employee = EmployeeServices.Instance.GetEmployee(EmployeeID);
                    foreach (var item in ServicesIDs)
                    {
                        var service = ServiceServices.Instance.GetService(int.Parse(item));
                        TimeInMinutes += int.Parse(service.Duration.Replace("mins", "").Replace("Mins", ""));
                    }
                    int Count = 0;


                    var DisabledDates = new List<DateTime>();
                    var EnabledDates = new List<DateTime>();
                    DateTime currentMonthStart = new DateTime(FinalSelectedDate.Year, FinalSelectedDate.Month, 1);
                    DateTime currentMonthEnd = currentMonthStart.AddMonths(1).AddDays(-1);
                    var roster = TimeTableRosterServices.Instance.GetTimeTableRosterByEmpID(EmployeeID);
                    var holidays = HolidayServices.Instance.GetHolidayWRTBusiness(businessName, "").Select(X => X.Date);

                    for (DateTime FirtIT = currentMonthStart; FirtIT <= currentMonthEnd; FirtIT = FirtIT.AddDays(1))
                    {
                        var shifts = ShiftServices.Instance.GetShiftWRTBusiness(businessName, EmployeeID).Where(x => x.Day == FirtIT.DayOfWeek.ToString()).ToList();
                        if (shifts.Count() > 0)
                        {
                            foreach (var shift in shifts)
                            {
                                var recurringShifts = RecurringShiftServices.Instance.GetRecurringShiftWRTBusiness(businessName, shift.ID);
                                if (recurringShifts != null)
                                {
                                    if (recurringShifts.RecurEnd == "Custom Date" && recurringShifts.RecurEndDate != "")
                                    {
                                        if (IsDateInRangeNew(DateTime.Parse(recurringShifts.RecurEndDate), FirtIT))
                                        {
                                            if (recurringShifts.Frequency == "Bi-Weekly")
                                            {

                                                if (GetNextDayStatus(FirtIT, shift.Date, shift.Day.ToString()) == "YES")
                                                {
                                                    var exceptionShift = ExceptionShiftServices.Instance.GetExceptionShiftWRTBusiness(businessName, shift.ID);
                                                    empShifts.Add(new ShiftModel { Shift = shift, RecurShift = recurringShifts, ExceptionShift = exceptionShift });
                                                    EnabledDates.Add(FirtIT);
                                                }


                                            }
                                            else
                                            {
                                                var exceptionShift = ExceptionShiftServices.Instance.GetExceptionShiftWRTBusiness(businessName, shift.ID);
                                                empShifts.Add(new ShiftModel { Shift = shift, RecurShift = recurringShifts, ExceptionShift = exceptionShift });
                                                EnabledDates.Add(FirtIT);
                                            }
                                        }
                                    }
                                    else
                                    {
                                        if (recurringShifts.Frequency == "Bi-Weekly")
                                        {
                                            if (roster != null)
                                            {
                                                if (GetNextDayStatus(FirtIT, shift.Date, shift.Day.ToString()) == "YES")
                                                {
                                                    var exceptionShift = ExceptionShiftServices.Instance.GetExceptionShiftWRTBusiness(businessName, shift.ID);
                                                    empShifts.Add(new ShiftModel { Shift = shift, RecurShift = recurringShifts, ExceptionShift = exceptionShift });
                                                    EnabledDates.Add(FirtIT);
                                                }
                                            }

                                        }
                                        else
                                        {
                                            if (shift.Date <= FirtIT)
                                            {
                                                var exceptionShift = ExceptionShiftServices.Instance.GetExceptionShiftWRTBusiness(businessName, shift.ID);
                                                empShifts.Add(new ShiftModel { Shift = shift, RecurShift = recurringShifts, ExceptionShift = exceptionShift });
                                                EnabledDates.Add(FirtIT);
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    var exceptionShiftByShiftID = ExceptionShiftServices.Instance.GetExceptionShiftWRTBusinessAndShift(businessName, shift.ID);
                                    if (exceptionShiftByShiftID != null)
                                    {
                                        if (exceptionShiftByShiftID.LastOrDefault() != null)
                                        {
                                            if (exceptionShiftByShiftID.LastOrDefault().ExceptionDate.ToString("yyyy-MM-dd") == FirtIT.ToString("yyyy-MM-dd"))
                                            {
                                                empShifts.Add(new ShiftModel { Shift = shift, RecurShift = recurringShifts, ExceptionShift = exceptionShiftByShiftID });
                                                EnabledDates.Add(FirtIT);
                                            }

                                        }
                                    }
                                    else
                                    {
                                        empShifts.Add(new ShiftModel { Shift = shift, RecurShift = recurringShifts });
                                        EnabledDates.Add(FirtIT);
                                    }

                                }
                            }
                        }

                    }
                    Allshifts.Add(new ShiftOfEmployeeModel { Employee = employee, Shifts = empShifts });
                    EnabledDates = EnabledDates.Except(holidays).ToList();

                    for (DateTime FirtIT = currentMonthStart; FirtIT <= currentMonthEnd; FirtIT = FirtIT.AddDays(1))
                    {
                        if (!EnabledDates.Contains(FirtIT))
                        {
                            disabledDays.Add(FirtIT);
                        }
                    }

                    return Json(new { disabledDays = disabledDays, FinalSelectedDate = FinalSelectedDate, EmployeeID = EmployeeID }, JsonRequestBehavior.AllowGet);
                }

            }



        }


        public EmployeePriceChange GetPriceChange(int EmployeeID, DateTime SelectedDate, TimeSpan slotStart, TimeSpan slotEnd)
        {
            bool ChangeFound = false;
            int ChangeID = 0;
            string TypeOfChange = "";
            var discountpercentage = 0.0;
            var priceChanges = EmployeePriceChangeServices.Instance.GetEmployeePriceChange(EmployeeID, "");
            if (priceChanges.Count() > 0)
            {
                foreach (var item in priceChanges)
                {

                    if (item.StartDate.Date <= SelectedDate.Date && item.EndDate.Date >= SelectedDate.Date
                        && item.StartDate.TimeOfDay <= slotStart && item.EndDate.TimeOfDay >= slotEnd)
                    {

                        discountpercentage = item.Percentage;
                        ChangeFound = true;
                        ChangeID = item.ID;
                        TypeOfChange = item.TypeOfChange;
                        //ListOfTimeSlotsWithDiscount.Add(new TimeSlotModel { TimeSlot = timeSlot, TypeOfChange = item.TypeOfChange, HaveDiscount = true, Percentage = discountpercentage,PriceChangeID = item.ID });
                        break;

                    }
                    else
                    {

                        //ListOfTimeSlotsWithDiscount.Add(new TimeSlotModel { TimeSlot = timeSlot, TypeOfChange = item.TypeOfChange, HaveDiscount = false, Percentage = discountpercentage, PriceChangeID = 0 });
                        ChangeFound = false;
                    }


                }
            }
            else
            {
                //ListOfTimeSlotsWithDiscount.Add(new TimeSlotModel { TimeSlot = timeSlot, TypeOfChange = item.TypeOfChange, HaveDiscount = false, Percentage = discountpercentage,PriceChangeID = 0 });
                ChangeFound = false;

            }

            if (ChangeFound)
            {
                var employeePriceChange = new EmployeePriceChange();
                employeePriceChange.EmployeeID = EmployeeID;
                employeePriceChange.ID = ChangeID;
                employeePriceChange.Percentage = float.Parse(discountpercentage.ToString());
                employeePriceChange.TypeOfChange = TypeOfChange;
                return employeePriceChange;
            }
            else
            {
                var employeePriceChange = new EmployeePriceChange();
                return employeePriceChange;

            }

        }

        static bool IsDateInRange(DateTime dateToCheck, DateTime startDate)
        {
            // Extracting only the date part without the time part
            DateTime dateOnlyToCheck = dateToCheck.Date;
            DateTime startDateOnly = startDate.Date;

            if (dateOnlyToCheck > startDateOnly)
            {
                return true; // If it's greater, it's definitely not in range
            }
            else
            {
                return false;
            }

            // Checking if the date falls within the range

            // Checking if the date falls within the range
        }


        public string GenerateonGoogleCalendar(Appointment appointment, string Services, string Business)
        {
            int year = appointment.Date.Year;
            int month = appointment.Date.Month;
            int day = appointment.Date.Day;
            int starthour = appointment.Time.Hour;
            int startminute = appointment.Time.Minute;
            int startseconds = appointment.Time.Second;

            int endhour = appointment.EndTime.Hour;
            int endminute = appointment.EndTime.Minute;
            int endseconds = appointment.EndTime.Second;

            DateTime startDateNew = new DateTime(year, month, day, starthour, startminute, startseconds);
            DateTime EndDateNew = new DateTime(year, month, day, endhour, endminute, endseconds);

            DateTime currentDateTime = DateTime.Now;
            TimeSpan offset = TimeZoneInfo.Local.GetUtcOffset(currentDateTime);

            var googleCalendar = GoogleCalendarServices.Instance.GetGoogleCalendarServicesWRTBusiness(Business);
            var company = CompanyServices.Instance.GetCompany().Where(x => x.Business == Business).FirstOrDefault();
            var employee = EmployeeServices.Instance.GetEmployee(appointment.EmployeeID);
            var url = "https://www.googleapis.com/calendar/v3/calendars/primary/events".Replace("primary", employee.GoogleCalendarID);
            var finalUrl = new System.Uri(url);
            RestClient restClient = new RestClient(finalUrl);
            RestRequest request = new RestRequest();
            var calendarEvent = new Event();
            calendarEvent.Summary = "Appointment at: " + Business;
            calendarEvent.Description = Services;
            var TimeZone = TimeZoneInfo.Local.Id;
            calendarEvent.Start = new EventDateTime() { DateTime = startDateNew.ToString("yyyy-MM-dd'T'HH:mm:ss.fffK"), TimeZone = company.TimeZone };
            calendarEvent.End = new EventDateTime() { DateTime = EndDateNew.ToString("yyyy-MM-dd'T'HH:mm:ss.fffK"), TimeZone = company.TimeZone };

            var model = JsonConvert.SerializeObject(calendarEvent, new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            });

            request.AddQueryParameter("key", "AIzaSyASKpY6I08IVKFMw3muX39uMzPc5sBDaSc");
            request.AddHeader("Authorization", "Bearer " + googleCalendar.AccessToken);
            request.AddHeader("Accept", "application/json");
            request.AddHeader("Content-Type", "application/json");
            request.AddParameter("application/json", model, ParameterType.RequestBody);

            var response = restClient.Post(request);

            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                JObject jsonObj = JObject.Parse(response.Content);
                appointment.GoogleCalendarEventID = jsonObj["id"].ToString();
                AppointmentServices.Instance.UpdateAppointment(appointment);
                return "Saved";
            }
            else
            {
                var history = new History();
                history.Note = "Error" + response.Content;
                history.Business = "Error";
                history.Date = DateTime.Now;
                HistoryServices.Instance.SaveHistory(history);
                return "Error" + response.Content;

            }
        }


        public List<string> FindAvailableSlots(DateTime employeeStartTime, DateTime employeeEndTime, List<Appointment> appointments, int durationInMinutes)
        {
            List<string> availableSlots = new List<string>();
            List<string> NotavailableSlots = new List<string>();
            DateTime currentTime = DateTime.Now;

            var company = appointments.FirstOrDefault().Business;
            var Company = CompanyServices.Instance.GetCompany().Where(x => x.Business == company).FirstOrDefault();

           
            currentTime = TheBookingPlatform.Models.TimeZoneConverter.ConvertToTimeZone(currentTime, Company.TimeZone).AddMinutes(5);
            var FinalAppointments = new List<Appointment>();
            foreach (var item in appointments)
            {
                if (item.IsPaid == false && item.DepositMethod == "Online" && item.IsCancelled == false && (DateTime.Now - item.BookingDate).TotalMinutes > 15)
                {
                    item.IsCancelled = true;
                }
                else
                {
                    FinalAppointments.Add(item);
                }
            }
            FinalAppointments.Sort((x, y) => x.Time.TimeOfDay.CompareTo(y.Time.TimeOfDay));

            DateTime lastEndTime = employeeStartTime;

            if (FinalAppointments.Count > 0)
            {
                var firstAppointment = FinalAppointments[0];
                // Consider slots before the first appointment
                // Step 1: Check if currentTime is greater than or equal to employeeStartTime
                if (currentTime >= employeeStartTime)
                {
                    // Step 2: Check if there is a gap between currentTime and the first appointment
                    if (firstAppointment.Time.TimeOfDay > currentTime.TimeOfDay)
                    {
                        TimeSpan timeGap = firstAppointment.Time.TimeOfDay - currentTime.TimeOfDay;

                        if (timeGap.TotalMinutes >= durationInMinutes)
                        {
                            int slots = (int)(timeGap.TotalMinutes / durationInMinutes);

                            for (int i = 0; i < slots; i++)
                            {
                                DateTime slotStart = currentTime.AddMinutes(i * durationInMinutes);
                                DateTime slotEnd = slotStart.AddMinutes(durationInMinutes);

                                if (IsSlotWithinEmployeeTimeRange(slotStart, slotEnd, employeeStartTime, employeeEndTime))
                                {
                                    if (employeeStartTime.Date.ToString("yyyy-MM-dd") == currentTime.ToString("yyyy-MM-dd"))
                                    {
                                        if (slotStart.TimeOfDay >= currentTime.TimeOfDay)
                                        {
                                            availableSlots.Add(slotStart.ToString("HH:mm") + " - " + slotEnd.ToString("HH:mm"));
                                        }
                                    }
                                    else
                                    {
                                        availableSlots.Add(slotStart.ToString("HH:mm") + " - " + slotEnd.ToString("HH:mm"));
                                    }
                                    lastEndTime = firstAppointment.EndTime;
                                }
                            }
                        }
                    }
                }
                else if(employeeStartTime.TimeOfDay <= firstAppointment.Time.TimeOfDay)
                {
                    TimeSpan timeGap = firstAppointment.Time.TimeOfDay - employeeStartTime.TimeOfDay;

                    if (timeGap.TotalMinutes >= durationInMinutes)
                    {
                        int slots = (int)(timeGap.TotalMinutes / durationInMinutes);

                        for (int i = 0; i < slots; i++)
                        {
                            DateTime slotStart = employeeStartTime.AddMinutes(i * durationInMinutes);
                            DateTime slotEnd = slotStart.AddMinutes(durationInMinutes);
                            if (IsSlotWithinEmployeeTimeRange(slotStart, slotEnd, employeeStartTime, employeeEndTime))
                            {
                                //if (slotEnd.TimeOfDay <= firstAppointment.Time.TimeOfDay)
                                //{\
                                if (employeeStartTime.Date.ToString("yyyy-MM-dd") == currentTime.ToString("yyyy-MM-dd"))
                                {
                                    if (slotStart.TimeOfDay >= currentTime.TimeOfDay)
                                    {
                                        availableSlots.Add(slotStart.ToString("HH:mm") + " - " + slotEnd.ToString("HH:mm"));

                                    }
                                }
                                else
                                {
                                    availableSlots.Add(slotStart.ToString("HH:mm") + " - " + slotEnd.ToString("HH:mm"));

                                }
                                lastEndTime = firstAppointment.EndTime;


                                //}
                                //else if (slotEnd.Hour == employeeEndTime.TimeOfDay.Hours)
                                //{
                                //    if (slotEnd.Minute < employeeEndTime.TimeOfDay.Minutes)
                                //    {
                                //        availableSlots.Add(slotStart.ToString("HH:mm") + " - " + slotEnd.ToString("HH:mm"));
                                //        lastEndTime = firstAppointment.EndTime;

                                //    }
                                //    else if (slotEnd.Minute == employeeEndTime.TimeOfDay.Minutes)
                                //    {
                                //        availableSlots.Add(slotStart.ToString("HH:mm") + " - " + slotEnd.ToString("HH:mm"));
                                //        lastEndTime = firstAppointment.EndTime;


                                //    }
                                //}
                            }
                        }
                    }

                }
            }

            foreach (var appointment in FinalAppointments)
            {
                if (lastEndTime.TimeOfDay <= appointment.Time.TimeOfDay)
                {
                    TimeSpan timeGap = appointment.Time.TimeOfDay - lastEndTime.TimeOfDay;

                    if (timeGap.TotalMinutes >= durationInMinutes)
                    {
                        int slots = (int)(timeGap.TotalMinutes / durationInMinutes);

                        for (int i = 0; i < slots; i++)
                        {
                            DateTime slotStart = lastEndTime.AddMinutes(i * durationInMinutes);
                            DateTime slotEnd = slotStart.AddMinutes(durationInMinutes);
                            if (IsSlotWithinEmployeeTimeRange(slotStart, slotEnd, employeeStartTime, employeeEndTime))
                            {
                                if (slotEnd.TimeOfDay <= appointment.Time.TimeOfDay)
                                {
                                    if (employeeStartTime.Date.ToString("yyyy-MM-dd") == currentTime.ToString("yyyy-MM-dd"))
                                    {
                                        if (slotStart.TimeOfDay >= currentTime.TimeOfDay)
                                        {
                                            availableSlots.Add(slotStart.ToString("HH:mm") + " - " + slotEnd.ToString("HH:mm"));

                                        }
                                    }
                                    else
                                    {
                                        availableSlots.Add(slotStart.ToString("HH:mm") + " - " + slotEnd.ToString("HH:mm"));

                                    }
                                }
                                //else if (slotEnd.Hour == employeeEndTime.TimeOfDay.Hours)
                                //{
                                //    if (slotEnd.Minute < employeeEndTime.TimeOfDay.Minutes)
                                //    {
                                //        availableSlots.Add(slotStart.ToString("HH:mm") + " - " + slotEnd.ToString("HH:mm"));
                                //    }
                                //    else if (slotEnd.Minute == employeeEndTime.TimeOfDay.Minutes)
                                //    {
                                //        availableSlots.Add(slotStart.ToString("HH:mm") + " - " + slotEnd.ToString("HH:mm"));

                                //    }
                                //}
                            }
                        }
                    }


                }
                if (employeeStartTime.TimeOfDay <= appointment.Time.TimeOfDay)
                {
                    if (lastEndTime.TimeOfDay < appointment.EndTime.TimeOfDay)
                    {
                        lastEndTime = appointment.EndTime;
                    }
                }
                else if (employeeEndTime.TimeOfDay >= appointment.Time.TimeOfDay)
                {
                    lastEndTime = appointment.EndTime;
                }

            }

            // Consider slots after the last appointment, if any
            if (lastEndTime.TimeOfDay < employeeEndTime.TimeOfDay)
            {
                if (lastEndTime.TimeOfDay > employeeStartTime.TimeOfDay)
                {
                    TimeSpan timeGap = employeeEndTime.TimeOfDay - lastEndTime.TimeOfDay;

                    if (timeGap.TotalMinutes >= durationInMinutes)
                    {
                        int slots = (int)(timeGap.TotalMinutes / durationInMinutes);

                        for (int i = 0; i < slots; i++)
                        {
                            DateTime slotStart = lastEndTime.AddMinutes(i * durationInMinutes);
                            DateTime slotEnd = slotStart.AddMinutes(durationInMinutes);
                            if (IsSlotWithinEmployeeTimeRange(slotStart, slotEnd, employeeStartTime, employeeEndTime))
                            {
                                if (slotEnd.Hour < employeeEndTime.TimeOfDay.Hours)
                                {
                                    if (employeeStartTime.Date.ToString("yyyy-MM-dd") == currentTime.ToString("yyyy-MM-dd"))
                                    {
                                        if (slotStart.TimeOfDay >= currentTime.TimeOfDay)
                                        {
                                            availableSlots.Add(slotStart.ToString("HH:mm") + " - " + slotEnd.ToString("HH:mm"));

                                        }
                                    }
                                    else
                                    {
                                        availableSlots.Add(slotStart.ToString("HH:mm") + " - " + slotEnd.ToString("HH:mm"));

                                    }
                                }
                                else if (slotEnd.Hour == employeeEndTime.TimeOfDay.Hours)
                                {
                                    if (slotEnd.Minute < employeeEndTime.TimeOfDay.Minutes)
                                    {
                                        if (employeeStartTime.Date.ToString("yyyy-MM-dd") == currentTime.ToString("yyyy-MM-dd"))
                                        {
                                            if (slotStart.TimeOfDay >= currentTime.TimeOfDay)
                                            {
                                                availableSlots.Add(slotStart.ToString("HH:mm") + " - " + slotEnd.ToString("HH:mm"));

                                            }
                                        }
                                        else
                                        {
                                            availableSlots.Add(slotStart.ToString("HH:mm") + " - " + slotEnd.ToString("HH:mm"));

                                        }
                                    }
                                    else if (slotEnd.Minute == employeeEndTime.TimeOfDay.Minutes)
                                    {
                                        if (employeeStartTime.Date.ToString("yyyy-MM-dd") == currentTime.ToString("yyyy-MM-dd"))
                                        {
                                            if (slotStart.TimeOfDay >= currentTime.TimeOfDay)
                                            {
                                                availableSlots.Add(slotStart.ToString("HH:mm") + " - " + slotEnd.ToString("HH:mm"));

                                            }
                                        }
                                        else
                                        {
                                            availableSlots.Add(slotStart.ToString("HH:mm") + " - " + slotEnd.ToString("HH:mm"));

                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                else
                {
                    TimeSpan timeGap = employeeEndTime.TimeOfDay - lastEndTime.TimeOfDay;

                    if (timeGap.TotalMinutes >= durationInMinutes)
                    {
                        int slots = (int)(timeGap.TotalMinutes / durationInMinutes);

                        for (int i = 0; i < slots; i++)
                        {
                            DateTime slotStart = lastEndTime.AddMinutes(i * durationInMinutes);
                            DateTime slotEnd = slotStart.AddMinutes(durationInMinutes);
                            if (IsSlotWithinEmployeeTimeRange(slotStart, slotEnd, employeeStartTime, employeeEndTime))
                            {
                                if (slotEnd.Hour < employeeEndTime.TimeOfDay.Hours)
                                {

                                    if (employeeStartTime.Date.ToString("yyyy-MM-dd") == currentTime.ToString("yyyy-MM-dd"))
                                    {
                                        if (slotStart.TimeOfDay >= currentTime.TimeOfDay)
                                        {
                                            availableSlots.Add(slotStart.ToString("HH:mm") + " - " + slotEnd.ToString("HH:mm"));

                                        }
                                    }
                                    else
                                    {
                                        availableSlots.Add(slotStart.ToString("HH:mm") + " - " + slotEnd.ToString("HH:mm"));

                                    }

                                }
                                else if (slotEnd.Hour == employeeEndTime.TimeOfDay.Hours)
                                {
                                    if (slotEnd.Minute < employeeEndTime.TimeOfDay.Minutes)
                                    {
                                        if (employeeStartTime.Date.ToString("yyyy-MM-dd") == currentTime.ToString("yyyy-MM-dd"))
                                        {
                                            if (slotStart.TimeOfDay >= currentTime.TimeOfDay)
                                            {
                                                availableSlots.Add(slotStart.ToString("HH:mm") + " - " + slotEnd.ToString("HH:mm"));

                                            }
                                        }
                                        else
                                        {
                                            availableSlots.Add(slotStart.ToString("HH:mm") + " - " + slotEnd.ToString("HH:mm"));

                                        }
                                    }
                                    else if (slotEnd.Minute == employeeEndTime.TimeOfDay.Minutes)
                                    {
                                        if (employeeStartTime.Date.ToString("yyyy-MM-dd") == currentTime.ToString("yyyy-MM-dd"))
                                        {
                                            if (slotStart.TimeOfDay >= currentTime.TimeOfDay)
                                            {
                                                availableSlots.Add(slotStart.ToString("HH:mm") + " - " + slotEnd.ToString("HH:mm"));

                                            }
                                        }
                                        else
                                        {
                                            availableSlots.Add(slotStart.ToString("HH:mm") + " - " + slotEnd.ToString("HH:mm"));

                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            return availableSlots;
        }



        [HttpGet]
        public JsonResult GetTimeSlots(int CompanyID, DateTime SelectedDate, string ServiceIDs, int EmployeeID, bool DoesSelected)
        {
            var Allshifts = new List<ShiftOfEmployeeModel>();



            var services = ServiceIDs.Split(',').ToList();
            int TimeInMinutes = 0;
            var maxSlotsItem = new SlotsListWithEmployeeIDModel();
            var Company = CompanyServices.Instance.GetCompany(CompanyID);

            foreach (var item in services)
            {
                var service = ServiceServices.Instance.GetService(int.Parse(item));
                TimeInMinutes += int.Parse(service.Duration.Replace("mins", "").Replace("Mins", ""));
            }

            var deduplicatedList = new List<TimeSlotModel>();

            var ProposedDate = SelectedDate;
            var DayOfWeek = SelectedDate.DayOfWeek.ToString();
            var openingHour = OpeningHourServices.Instance.GetOpeningHourWRTBusiness(Company.Business, DayOfWeek);
            var openingtime = openingHour.Time;
            var starTimeOpening = DateTime.Parse(openingtime.Split('-')[0].Trim());
            var endTimeOpening = DateTime.Parse(openingtime.Split('-')[1].Trim());
            var disabledEmployees = EmployeeServices.Instance.GetEmployeeWRTBusinessOnlyID(Company.Business, false);
            var Employee = EmployeeServices.Instance.GetEmployee(EmployeeID);

            if (Employee != null)
            {
                var empShifts = new List<ShiftModel>();

                
                var appointments = AppointmentServices.Instance.GetAppointmentBookingWRTBusiness(SelectedDate.Day, SelectedDate.Month, SelectedDate.Year, EmployeeID, false, false).ToList();


                var ListOfTimeSlotsWithDiscount = new List<TimeSlotModel>();
                var employeeRequest = EmployeeRequestServices.Instance.GetEmployeeRequestByBusiness(CompanyID).Where(x=>x.EmployeeID == Employee.ID).ToList();
                foreach (var item in employeeRequest)
                {
                    if (item.Accepted)
                    {
                        if (CompanyID == item.CompanyIDFrom)
                        {
                            var appointment = AppointmentServices.Instance.GetAppointmentBookingWRTBusiness(item.Business, false, false, item.EmployeeID).Where(x => x.Date.ToString("yyyy-MM-dd") == SelectedDate.ToString("yyyy-MM-dd")).ToList();
                            appointments.AddRange(appointment);
                        }
                        else
                        {
                            var companyFrom = CompanyServices.Instance.GetCompany(item.CompanyIDFrom);
                            var appointment = AppointmentServices.Instance.GetAppointmentBookingWRTBusiness(companyFrom.Business, false, false, item.EmployeeID).Where(x => x.Date.ToString("yyyy-MM-dd") == SelectedDate.ToString("yyyy-MM-dd")).ToList();
                            appointments.AddRange(appointment);
                        }

                    }
                }

                var roster = TimeTableRosterServices.Instance.GetTimeTableRosterByEmpID(EmployeeID);
                var ListOfEmployeeSlotsCount = new List<SlotsListWithEmployeeIDModel>();

                if (roster != null)
                {
                    var shifts = ShiftServices.Instance.GetShiftWRTBusiness(Company.Business, EmployeeID);
                    foreach (var shift in shifts)
                    {
                        var recurringShifts = RecurringShiftServices.Instance.GetRecurringShiftWRTBusiness(Company.Business, shift.ID);
                        if (recurringShifts != null)
                        {
                            if (recurringShifts.RecurEnd == "Custom Date" && recurringShifts.RecurEndDate != "")
                            {
                                if (IsDateInRangeNew(DateTime.Parse(recurringShifts.RecurEndDate), SelectedDate))
                                {

                                    if (recurringShifts.Frequency == "Bi-Weekly")
                                    {

                                        if (GetNextDayStatus(SelectedDate, shift.Date, shift.Day.ToString()) == "YES")
                                        {
                                            var exceptionShift = ExceptionShiftServices.Instance.GetExceptionShiftWRTBusiness(Company.Business, shift.ID);
                                            empShifts.Add(new ShiftModel { Shift = shift, RecurShift = recurringShifts, ExceptionShift = exceptionShift });
                                        }


                                    }
                                    else
                                    {
                                        var exceptionShift = ExceptionShiftServices.Instance.GetExceptionShiftWRTBusiness(Company.Business, shift.ID);
                                        empShifts.Add(new ShiftModel { Shift = shift, RecurShift = recurringShifts, ExceptionShift = exceptionShift });
                                    }
                                }
                            }
                            else
                            {
                                if (recurringShifts.Frequency == "Bi-Weekly")
                                {
                                    if (roster != null)
                                    {
                                        if (GetNextDayStatus(SelectedDate, shift.Date, shift.Day.ToString()) == "YES")
                                        {
                                            var exceptionShift = ExceptionShiftServices.Instance.GetExceptionShiftWRTBusiness(Company.Business, shift.ID);
                                            empShifts.Add(new ShiftModel { Shift = shift, RecurShift = recurringShifts, ExceptionShift = exceptionShift });
                                        }
                                    }

                                }
                                else
                                {
                                    var exceptionShift = ExceptionShiftServices.Instance.GetExceptionShiftWRTBusiness(Company.Business, shift.ID);
                                    empShifts.Add(new ShiftModel { Shift = shift, RecurShift = recurringShifts, ExceptionShift = exceptionShift });
                                }
                            }
                        }
                        else
                        {
                            var exceptionShiftByShiftID = ExceptionShiftServices.Instance.GetExceptionShiftWRTBusinessAndShift(Company.Business, shift.ID);
                            if (exceptionShiftByShiftID != null)
                            {
                                empShifts.Add(new ShiftModel { Shift = shift, RecurShift = recurringShifts, ExceptionShift = exceptionShiftByShiftID });
                            }
                            else
                            {
                                empShifts.Add(new ShiftModel { Shift = shift, RecurShift = recurringShifts });

                            }

                        }
                    }

                    var usethisShift = empShifts.Where(x => x.RecurShift != null && x.Shift.IsRecurring && x.Shift.Day == SelectedDate.DayOfWeek.ToString() && x.Shift.Date.Date <= SelectedDate.Date).Select(X => X.Shift).FirstOrDefault();
                    var useExceptionShifts = empShifts
                        .Where(x => x.ExceptionShift != null && x.ExceptionShift.Count() > 0 && !x.Shift.IsRecurring)
                        .SelectMany(x => x.ExceptionShift);

                    if (usethisShift == null && useExceptionShifts == null)
                    {
                        //okay bye
                    }
                    else
                    {
                        if (usethisShift != null)
                        {
                            bool FoundExceptionToo = false;
                            var FoundedExceptionShift = new ExceptionShift();
                            var empShiftsCount = empShifts.Count();
                            if (empShiftsCount > 0) // Ensure there's at least one element in empShifts
                            {
                                for (int i = 0; i < empShiftsCount; i++)
                                {
                                    var firstEmpShift = empShifts[i]; // Accessing empShifts at index i
                                    foreach (var item in firstEmpShift.ExceptionShift.ToList())
                                    {
                                        if (item.ShiftID == firstEmpShift.Shift.ID && item.ExceptionDate.ToString("yyyy-MM-dd") == SelectedDate.ToString("yyyy-MM-dd") && item.IsNotWorking == false)
                                        {
                                            FoundExceptionToo = true;
                                            FoundedExceptionShift = item;
                                            break;
                                        }
                                        if (item.ShiftID == firstEmpShift.Shift.ID && item.ExceptionDate.ToString("yyyy-MM-dd") == SelectedDate.ToString("yyyy-MM-dd") && item.IsNotWorking == true)
                                        {
                                            FoundExceptionToo = true;
                                            FoundedExceptionShift = item;
                                            break;
                                        }
                                    }


                                }

                            }
                            if (FoundExceptionToo)
                            {

                                if (FoundedExceptionShift.ID != 0 && !FoundedExceptionShift.IsNotWorking)
                                {
                                    DateTime startTime = SelectedDate.Date
                                   .AddHours(DateTime.ParseExact(FoundedExceptionShift.StartTime, "H:mm", CultureInfo.InvariantCulture).Hour)
                                   .AddMinutes(DateTime.ParseExact(FoundedExceptionShift.StartTime, "H:mm", CultureInfo.InvariantCulture).Minute);

                                    DateTime endTime = SelectedDate.Date
                                        .AddHours(DateTime.ParseExact(FoundedExceptionShift.EndTime, "H:mm", CultureInfo.InvariantCulture).Hour)
                                        .AddMinutes(DateTime.ParseExact(FoundedExceptionShift.EndTime, "H:mm", CultureInfo.InvariantCulture).Minute);


                                    var CheckSlots = new List<string>();



                                    CheckSlots = FindAvailableSlots(startTime, endTime, appointments, TimeInMinutes);



                                    if (CheckSlots.Count() != 0)
                                    {

                                        var priceChanges = PriceChangeServices.Instance.GetPriceChangeWRTBusiness(Company.Business, "");
                                        foreach (var timeslot in CheckSlots)
                                        {
                                            string timeSlot = timeslot;
                                            bool CheckPriceChange = false;
                                            var slotStart = TimeSpan.Parse(timeslot.Split('-')[0]);
                                            var slotEnd = TimeSpan.Parse(timeslot.Split('-')[1]);
                                            float discountpercentage = 0;
                                            bool ChangeFound = false;
                                            int ChangeID = 0;
                                            string TypeOfChange = "";

                                            var employeePriceChange = GetPriceChange(EmployeeID, SelectedDate, slotStart, slotEnd);


                                            if (priceChanges.Count() > 0)
                                            {
                                                foreach (var item in priceChanges)
                                                {

                                                    if (item.StartDate.Date <= SelectedDate.Date && item.EndDate.Date >= SelectedDate.Date)
                                                    {
                                                        if (slotStart >= item.StartDate.TimeOfDay && slotEnd <= item.EndDate.TimeOfDay)
                                                        {
                                                            discountpercentage = item.Percentage;
                                                            ChangeFound = true;
                                                            ChangeID = item.ID;
                                                            TypeOfChange = item.TypeOfChange;
                                                            //ListOfTimeSlotsWithDiscount.Add(new TimeSlotModel { TimeSlot = timeSlot, TypeOfChange = item.TypeOfChange, HaveDiscount = true, Percentage = discountpercentage,PriceChangeID = item.ID });
                                                            break;

                                                        }
                                                        else
                                                        {

                                                            //ListOfTimeSlotsWithDiscount.Add(new TimeSlotModel { TimeSlot = timeSlot, TypeOfChange = item.TypeOfChange, HaveDiscount = false, Percentage = discountpercentage, PriceChangeID = 0 });
                                                            ChangeFound = false;
                                                        }

                                                    }
                                                    else
                                                    {
                                                        //ListOfTimeSlotsWithDiscount.Add(new TimeSlotModel { TimeSlot = timeSlot, TypeOfChange = item.TypeOfChange, HaveDiscount = false, Percentage = discountpercentage,PriceChangeID = 0 });
                                                        ChangeFound = false;

                                                    }

                                                }

                                                if (ChangeFound)
                                                {
                                                    if (employeePriceChange.EmployeeID != 0)
                                                    {
                                                        ListOfTimeSlotsWithDiscount.Add(new TimeSlotModel
                                                        {
                                                            TimeSlot = timeSlot,
                                                            TypeOfChange = TypeOfChange,
                                                            HaveDiscount = true,
                                                            Percentage = discountpercentage,
                                                            EmployeeID = EmployeeID,
                                                            PriceChangeID = ChangeID,
                                                            EmpHaveDiscount = true,
                                                            EmpPercentage = employeePriceChange.Percentage,
                                                            EmpPriceChangeID = employeePriceChange.ID,
                                                            EmpTypeOfChange = employeePriceChange.TypeOfChange
                                                        });

                                                    }
                                                    else
                                                    {
                                                        ListOfTimeSlotsWithDiscount.Add(new TimeSlotModel { EmployeeID = EmployeeID, TimeSlot = timeSlot, TypeOfChange = TypeOfChange, HaveDiscount = true, Percentage = discountpercentage, PriceChangeID = ChangeID });
                                                    }

                                                }
                                                else
                                                {
                                                    if (employeePriceChange.EmployeeID != 0)
                                                    {
                                                        ListOfTimeSlotsWithDiscount.Add(new TimeSlotModel
                                                        {
                                                            TimeSlot = timeSlot,
                                                            TypeOfChange = TypeOfChange,
                                                            HaveDiscount = false,
                                                            Percentage = discountpercentage,
                                                            PriceChangeID = 0,
                                                            EmployeeID = EmployeeID,
                                                            EmpHaveDiscount = true,
                                                            EmpPercentage = employeePriceChange.Percentage,
                                                            EmpPriceChangeID = employeePriceChange.ID,
                                                            EmpTypeOfChange = employeePriceChange.TypeOfChange
                                                        });

                                                    }
                                                    else
                                                    {
                                                        ListOfTimeSlotsWithDiscount.Add(new TimeSlotModel { EmployeeID = EmployeeID, TimeSlot = timeSlot, TypeOfChange = TypeOfChange, HaveDiscount = false, Percentage = discountpercentage, PriceChangeID = 0 });
                                                    }

                                                }
                                            }
                                            else
                                            {
                                                ListOfTimeSlotsWithDiscount.Add(new TimeSlotModel { EmployeeID = EmployeeID, TimeSlot = timeSlot, TypeOfChange = TypeOfChange, HaveDiscount = false, Percentage = discountpercentage, PriceChangeID = 0 });

                                            }
                                        }
                                        //ListOfEmployeeSlotsCount.Add(new SlotsListWithEmployeeIDModel { NoOfSlots = ListOfTimeSlotsWithDiscount, EmployeeID = EmployeeID });

                                    }
                                }



                            }
                            else
                            {
                                DateTime startTime = SelectedDate.Date
                                           .AddHours(DateTime.ParseExact(usethisShift.StartTime, "H:mm", CultureInfo.InvariantCulture).Hour)
                                           .AddMinutes(DateTime.ParseExact(usethisShift.StartTime, "H:mm", CultureInfo.InvariantCulture).Minute);

                                DateTime endTime = SelectedDate.Date
                                    .AddHours(DateTime.ParseExact(usethisShift.EndTime, "H:mm", CultureInfo.InvariantCulture).Hour)
                                    .AddMinutes(DateTime.ParseExact(usethisShift.EndTime, "H:mm", CultureInfo.InvariantCulture).Minute);

                                var CheckSlots = new List<string>();

                                if (usethisShift.IsRecurring)
                                {
                                    var recurringShift = RecurringShiftServices.Instance.GetRecurringShiftWRTBusiness(Company.Business, usethisShift.ID);
                                    if (recurringShift != null)
                                    {
                                        if (recurringShift.RecurEnd == "Never")
                                        {
                                            if (recurringShift.Frequency == "Bi-Weekly")
                                            {
                                                if (GetNextDayStatus(SelectedDate, usethisShift.Date, SelectedDate.DayOfWeek.ToString()) == "YES")
                                                {
                                                    CheckSlots = FindAvailableSlots(startTime, endTime, appointments, TimeInMinutes);
                                                }
                                            }
                                            else
                                            {
                                                CheckSlots = FindAvailableSlots(startTime, endTime, appointments, TimeInMinutes);
                                            }
                                        }
                                        else
                                        {
                                            if (DateTime.Parse(recurringShift.RecurEndDate) >= SelectedDate)
                                            {
                                                if (recurringShift.Frequency == "Bi-Weekly")
                                                {
                                                    if (GetNextDayStatus(SelectedDate, usethisShift.Date, SelectedDate.DayOfWeek.ToString()) == "YES")
                                                    {
                                                        CheckSlots = FindAvailableSlots(startTime, endTime, appointments, TimeInMinutes);
                                                    }
                                                }
                                                else
                                                {
                                                    CheckSlots = FindAvailableSlots(startTime, endTime, appointments, TimeInMinutes);
                                                }

                                            }
                                        }




                                    }
                                    else
                                    {

                                    }
                                }
                                else
                                {
                                    CheckSlots = FindAvailableSlots(startTime, endTime, appointments, TimeInMinutes);

                                }

                                var priceChanges = PriceChangeServices.Instance.GetPriceChangeWRTBusiness(Company.Business, "");
                                foreach (var timeslot in CheckSlots)
                                {
                                    string timeSlot = timeslot;
                                    bool CheckPriceChange = false;
                                    var slotStart = TimeSpan.Parse(timeslot.Split('-')[0]);
                                    var slotEnd = TimeSpan.Parse(timeslot.Split('-')[1]);
                                    float discountpercentage = 0;
                                    bool ChangeFound = false;
                                    int ChangeID = 0;
                                    string TypeOfChange = "";
                                    var employeePriceChange = GetPriceChange(EmployeeID, SelectedDate, slotStart, slotEnd);
                                    if (priceChanges.Count() > 0)
                                    {
                                        foreach (var item in priceChanges)
                                        {

                                            if (item.StartDate.Date <= SelectedDate.Date && item.EndDate.Date >= SelectedDate.Date)
                                            {
                                                if (slotStart >= item.StartDate.TimeOfDay && slotEnd <= item.EndDate.TimeOfDay)
                                                {
                                                    discountpercentage = item.Percentage;
                                                    ChangeFound = true;
                                                    ChangeID = item.ID;
                                                    TypeOfChange = item.TypeOfChange;
                                                    //ListOfTimeSlotsWithDiscount.Add(new TimeSlotModel { TimeSlot = timeSlot, TypeOfChange = item.TypeOfChange, HaveDiscount = true, Percentage = discountpercentage,PriceChangeID = item.ID });
                                                    break;

                                                }
                                                else
                                                {

                                                    //ListOfTimeSlotsWithDiscount.Add(new TimeSlotModel { TimeSlot = timeSlot, TypeOfChange = item.TypeOfChange, HaveDiscount = false, Percentage = discountpercentage, PriceChangeID = 0 });
                                                    ChangeFound = false;
                                                }

                                            }
                                            else
                                            {
                                                //ListOfTimeSlotsWithDiscount.Add(new TimeSlotModel { TimeSlot = timeSlot, TypeOfChange = item.TypeOfChange, HaveDiscount = false, Percentage = discountpercentage,PriceChangeID = 0 });
                                                ChangeFound = false;

                                            }

                                        }

                                        if (ChangeFound)
                                        {
                                            if (employeePriceChange.EmployeeID != 0)
                                            {
                                                ListOfTimeSlotsWithDiscount.Add(new TimeSlotModel
                                                {
                                                    TimeSlot = timeSlot,
                                                    TypeOfChange = TypeOfChange,
                                                    HaveDiscount = true,
                                                    Percentage = discountpercentage,
                                                    PriceChangeID = ChangeID,
                                                    EmpHaveDiscount = true,
                                                    EmployeeID = EmployeeID,
                                                    EmpPercentage = employeePriceChange.Percentage,
                                                    EmpPriceChangeID = employeePriceChange.ID,
                                                    EmpTypeOfChange = employeePriceChange.TypeOfChange
                                                });

                                            }
                                            else
                                            {
                                                ListOfTimeSlotsWithDiscount.Add(new TimeSlotModel { EmployeeID = EmployeeID, TimeSlot = timeSlot, TypeOfChange = TypeOfChange, HaveDiscount = true, Percentage = discountpercentage, PriceChangeID = ChangeID });
                                            }
                                        }
                                        else
                                        {
                                            if (employeePriceChange.EmployeeID != 0)
                                            {
                                                ListOfTimeSlotsWithDiscount.Add(new TimeSlotModel
                                                {
                                                    TimeSlot = timeSlot,
                                                    TypeOfChange = TypeOfChange,
                                                    HaveDiscount = false,
                                                    Percentage = discountpercentage,
                                                    PriceChangeID = 0,
                                                    EmpHaveDiscount = true,
                                                    EmployeeID = EmployeeID,
                                                    EmpPercentage = employeePriceChange.Percentage,
                                                    EmpPriceChangeID = employeePriceChange.ID,
                                                    EmpTypeOfChange = employeePriceChange.TypeOfChange
                                                });

                                            }
                                            else
                                            {
                                                ListOfTimeSlotsWithDiscount.Add(new TimeSlotModel { EmployeeID = EmployeeID, TimeSlot = timeSlot, TypeOfChange = TypeOfChange, HaveDiscount = false, Percentage = discountpercentage, PriceChangeID = 0 });
                                            }

                                        }
                                    }
                                    else
                                    {

                                        if (employeePriceChange.EmployeeID != 0)
                                        {
                                            ListOfTimeSlotsWithDiscount.Add(new TimeSlotModel
                                            {
                                                TimeSlot = timeSlot,
                                                TypeOfChange = TypeOfChange,
                                                HaveDiscount = false,
                                                Percentage = discountpercentage,
                                                PriceChangeID = 0,
                                                EmployeeID = EmployeeID,
                                                EmpHaveDiscount = true,
                                                EmpPercentage = employeePriceChange.Percentage,
                                                EmpPriceChangeID = employeePriceChange.ID,
                                                EmpTypeOfChange = employeePriceChange.TypeOfChange
                                            });

                                        }
                                        else
                                        {
                                            ListOfTimeSlotsWithDiscount.Add(new TimeSlotModel { EmployeeID = EmployeeID, TimeSlot = timeSlot, TypeOfChange = TypeOfChange, HaveDiscount = false, Percentage = discountpercentage, PriceChangeID = 0 });
                                        }

                                    }
                                }

                            }

                        }
                        else
                        {

                            if (useExceptionShifts != null)
                            {
                                bool FoundExceptionToo = false;
                                var FoundedExceptionShift = new ExceptionShift();
                                var empShiftsCount = empShifts.Count();
                                if (empShiftsCount > 0) // Ensure there's at least one element in empShifts
                                {
                                    for (int i = 0; i < empShiftsCount; i++)
                                    {
                                        var firstEmpShift = empShifts[i]; // Accessing empShifts at index i
                                        foreach (var item in firstEmpShift.ExceptionShift.ToList())
                                        {
                                            if (item.ExceptionDate.ToString("yyyy-MM-dd") == SelectedDate.ToString("yyyy-MM-dd") && item.IsNotWorking == false)
                                            {
                                                FoundExceptionToo = true;
                                                FoundedExceptionShift = item;
                                                break;
                                            }
                                            if (item.ExceptionDate.ToString("yyyy-MM-dd") == SelectedDate.ToString("yyyy-MM-dd") && item.IsNotWorking == true)
                                            {
                                                FoundExceptionToo = true;
                                                FoundedExceptionShift = item;
                                                break;
                                            }
                                        }


                                    }

                                }


                                if (FoundExceptionToo)
                                {

                                    if (FoundedExceptionShift.ID != 0 && !FoundedExceptionShift.IsNotWorking)
                                    {
                                        DateTime startTime = SelectedDate.Date
                                       .AddHours(DateTime.ParseExact(FoundedExceptionShift.StartTime, "H:mm", CultureInfo.InvariantCulture).Hour)
                                       .AddMinutes(DateTime.ParseExact(FoundedExceptionShift.StartTime, "H:mm", CultureInfo.InvariantCulture).Minute);

                                        DateTime endTime = SelectedDate.Date
                                            .AddHours(DateTime.ParseExact(FoundedExceptionShift.EndTime, "H:mm", CultureInfo.InvariantCulture).Hour)
                                            .AddMinutes(DateTime.ParseExact(FoundedExceptionShift.EndTime, "H:mm", CultureInfo.InvariantCulture).Minute);


                                        var CheckSlots = new List<string>();



                                        CheckSlots = FindAvailableSlots(startTime, endTime, appointments, TimeInMinutes);



                                        if (CheckSlots.Count() != 0)
                                        {
                                            var priceChanges = PriceChangeServices.Instance.GetPriceChangeWRTBusiness(Company.Business, "");
                                            foreach (var timeslot in CheckSlots)
                                            {

                                                string timeSlot = timeslot;
                                                bool CheckPriceChange = false;
                                                var slotStart = TimeSpan.Parse(timeslot.Split('-')[0]);
                                                var slotEnd = TimeSpan.Parse(timeslot.Split('-')[1]);
                                                float discountpercentage = 0;
                                                bool ChangeFound = false;
                                                int ChangeID = 0;
                                                string TypeOfChange = "";
                                                var employeePriceChange = GetPriceChange(EmployeeID, SelectedDate, slotStart, slotEnd);

                                                if (priceChanges.Count() > 0)
                                                {
                                                    foreach (var item in priceChanges)
                                                    {

                                                        if (item.StartDate.Date <= SelectedDate.Date && item.EndDate.Date >= SelectedDate.Date)
                                                        {
                                                            if (slotStart >= item.StartDate.TimeOfDay && slotEnd <= item.EndDate.TimeOfDay)
                                                            {
                                                                discountpercentage = item.Percentage;
                                                                ChangeFound = true;
                                                                ChangeID = item.ID;
                                                                TypeOfChange = item.TypeOfChange;
                                                                //ListOfTimeSlotsWithDiscount.Add(new TimeSlotModel { TimeSlot = timeSlot, TypeOfChange = item.TypeOfChange, HaveDiscount = true, Percentage = discountpercentage,PriceChangeID = item.ID });
                                                                break;

                                                            }
                                                            else
                                                            {

                                                                //ListOfTimeSlotsWithDiscount.Add(new TimeSlotModel { TimeSlot = timeSlot, TypeOfChange = item.TypeOfChange, HaveDiscount = false, Percentage = discountpercentage, PriceChangeID = 0 });
                                                                ChangeFound = false;
                                                            }

                                                        }
                                                        else
                                                        {
                                                            //ListOfTimeSlotsWithDiscount.Add(new TimeSlotModel { TimeSlot = timeSlot, TypeOfChange = item.TypeOfChange, HaveDiscount = false, Percentage = discountpercentage,PriceChangeID = 0 });
                                                            ChangeFound = false;

                                                        }

                                                    }

                                                    if (ChangeFound)
                                                    {
                                                        if (employeePriceChange.EmployeeID != 0)
                                                        {
                                                            ListOfTimeSlotsWithDiscount.Add(new TimeSlotModel
                                                            {
                                                                TimeSlot = timeSlot,
                                                                TypeOfChange = TypeOfChange,
                                                                HaveDiscount = true,
                                                                Percentage = discountpercentage,
                                                                PriceChangeID = ChangeID,
                                                                EmployeeID = EmployeeID,
                                                                EmpHaveDiscount = true,
                                                                EmpPercentage = employeePriceChange.Percentage,
                                                                EmpPriceChangeID = employeePriceChange.ID,
                                                                EmpTypeOfChange = employeePriceChange.TypeOfChange
                                                            });

                                                        }
                                                        else
                                                        {
                                                            ListOfTimeSlotsWithDiscount.Add(new TimeSlotModel { EmployeeID = EmployeeID, TimeSlot = timeSlot, TypeOfChange = TypeOfChange, HaveDiscount = true, Percentage = discountpercentage, PriceChangeID = ChangeID });
                                                        }
                                                    }
                                                    else
                                                    {
                                                        if (employeePriceChange.EmployeeID != 0)
                                                        {
                                                            ListOfTimeSlotsWithDiscount.Add(new TimeSlotModel
                                                            {
                                                                TimeSlot = timeSlot,
                                                                TypeOfChange = TypeOfChange,
                                                                HaveDiscount = false,
                                                                Percentage = discountpercentage,
                                                                PriceChangeID = 0,
                                                                EmpHaveDiscount = true,
                                                                EmployeeID = EmployeeID,
                                                                EmpPercentage = employeePriceChange.Percentage,
                                                                EmpPriceChangeID = employeePriceChange.ID,
                                                                EmpTypeOfChange = employeePriceChange.TypeOfChange
                                                            });

                                                        }
                                                        else
                                                        {
                                                            ListOfTimeSlotsWithDiscount.Add(new TimeSlotModel { EmployeeID = EmployeeID, TimeSlot = timeSlot, TypeOfChange = TypeOfChange, HaveDiscount = false, Percentage = discountpercentage, PriceChangeID = 0 });
                                                        }

                                                    }
                                                }
                                                else
                                                {

                                                    if (employeePriceChange.EmployeeID != 0)
                                                    {
                                                        ListOfTimeSlotsWithDiscount.Add(new TimeSlotModel
                                                        {
                                                            TimeSlot = timeSlot,
                                                            TypeOfChange = TypeOfChange,
                                                            HaveDiscount = false,
                                                            Percentage = discountpercentage,
                                                            PriceChangeID = 0,
                                                            EmpHaveDiscount = true,
                                                            EmployeeID = EmployeeID,
                                                            EmpPercentage = employeePriceChange.Percentage,
                                                            EmpPriceChangeID = employeePriceChange.ID,
                                                            EmpTypeOfChange = employeePriceChange.TypeOfChange
                                                        });

                                                    }
                                                    else
                                                    {
                                                        ListOfTimeSlotsWithDiscount.Add(new TimeSlotModel { EmployeeID = EmployeeID, TimeSlot = timeSlot, TypeOfChange = TypeOfChange, HaveDiscount = false, Percentage = discountpercentage, PriceChangeID = 0 });
                                                    }

                                                }
                                            }


                                        }
                                    }



                                }
                                //ddddddddddd

                            }

                        }
                    }




                }

                if (ListOfTimeSlotsWithDiscount.Count() == 0 && EmployeeID == 0)
                {
                    var employeeservices = EmployeeServiceServices.Instance.GetEmployeeServiceWRTBusiness(Company.Business);
                    var AvailableEmployeeList = new List<int>();
                    var FinalServiceList = services.Select(int.Parse).ToList();

                    var employeeIDs = employeeservices
                            .Where(es => FinalServiceList.Contains(es.ServiceID))
                            .GroupBy(es => es.EmployeeID)
                            .Where(grp => grp.Select(g => g.ServiceID).Distinct().Count() == FinalServiceList.Count)
                            .Select(grp => grp.Key)
                            .ToList();

                    var Employees = EmployeeServices.Instance.GetEmployeeWRTBusiness(Company.Business, true, true, employeeIDs);

                    foreach (var emp in Employees)
                    {
                        var appointmentsnew = AppointmentServices.Instance.GetAppointmentBookingWRTBusiness(Company.Business, SelectedDate.Day, SelectedDate.Month, SelectedDate.Year, emp.ID, false, false);
                        var rosterEmp = TimeTableRosterServices.Instance.GetTimeTableRosterByEmpID(emp.ID);
                        if (rosterEmp != null)
                        {
                            var shifts = ShiftServices.Instance.GetShiftWRTBusiness(Company.Business, emp.ID);
                            foreach (var shift in shifts)
                            {
                                var recurringShifts = RecurringShiftServices.Instance.GetRecurringShiftWRTBusiness(Company.Business, shift.ID);
                                if (recurringShifts != null)
                                {
                                    if (recurringShifts.RecurEnd == "Custom Date" && recurringShifts.RecurEndDate != "")
                                    {
                                        if (IsDateInRangeNew(DateTime.Parse(recurringShifts.RecurEndDate), SelectedDate))
                                        {

                                            if (recurringShifts.Frequency == "Bi-Weekly")
                                            {

                                                if (GetNextDayStatus(SelectedDate, shift.Date, shift.Day.ToString()) == "YES")
                                                {
                                                    var exceptionShift = ExceptionShiftServices.Instance.GetExceptionShiftWRTBusiness(Company.Business, shift.ID);
                                                    empShifts.Add(new ShiftModel { Shift = shift, RecurShift = recurringShifts, ExceptionShift = exceptionShift });
                                                }


                                            }
                                            else
                                            {
                                                var exceptionShift = ExceptionShiftServices.Instance.GetExceptionShiftWRTBusiness(Company.Business, shift.ID);
                                                empShifts.Add(new ShiftModel { Shift = shift, RecurShift = recurringShifts, ExceptionShift = exceptionShift });
                                            }
                                        }
                                    }
                                    else
                                    {
                                        if (recurringShifts.Frequency == "Bi-Weekly")
                                        {
                                            if (rosterEmp != null)
                                            {
                                                if (GetNextDayStatus(SelectedDate, shift.Date, shift.Day.ToString()) == "YES")
                                                {
                                                    var exceptionShift = ExceptionShiftServices.Instance.GetExceptionShiftWRTBusiness(Company.Business, shift.ID);
                                                    empShifts.Add(new ShiftModel { Shift = shift, RecurShift = recurringShifts, ExceptionShift = exceptionShift });
                                                }
                                            }

                                        }
                                        else
                                        {
                                            var exceptionShift = ExceptionShiftServices.Instance.GetExceptionShiftWRTBusiness(Company.Business, shift.ID);
                                            empShifts.Add(new ShiftModel { Shift = shift, RecurShift = recurringShifts, ExceptionShift = exceptionShift });
                                        }
                                    }
                                }
                                else
                                {
                                    var exceptionShiftByShiftID = ExceptionShiftServices.Instance.GetExceptionShiftWRTBusinessAndShift(Company.Business, shift.ID);
                                    if (exceptionShiftByShiftID != null)
                                    {
                                        empShifts.Add(new ShiftModel { Shift = shift, RecurShift = recurringShifts, ExceptionShift = exceptionShiftByShiftID });
                                    }
                                    else
                                    {
                                        empShifts.Add(new ShiftModel { Shift = shift, RecurShift = recurringShifts });

                                    }

                                }
                            }
                            Allshifts.Add(new ShiftOfEmployeeModel { Employee = emp, Shifts = empShifts });



                            var usethisShift = Allshifts
                            .SelectMany(shifto => shifto.Shifts)
                            .FirstOrDefault(shift => shift.Shift.Day == SelectedDate.DayOfWeek.ToString())?.Shift;

                            if (usethisShift != null)
                            {

                                DateTime startTimeemp = SelectedDate.Date
                                           .AddHours(DateTime.ParseExact(usethisShift.StartTime, "H:mm", CultureInfo.InvariantCulture).Hour)
                                           .AddMinutes(DateTime.ParseExact(usethisShift.StartTime, "H:mm", CultureInfo.InvariantCulture).Minute);

                                DateTime endTimeemp = SelectedDate.Date
                                    .AddHours(DateTime.ParseExact(usethisShift.EndTime, "H:mm", CultureInfo.InvariantCulture).Hour)
                                    .AddMinutes(DateTime.ParseExact(usethisShift.EndTime, "H:mm", CultureInfo.InvariantCulture).Minute);

                                var CheckSlots = new List<string>();

                                if (usethisShift.IsRecurring)
                                {
                                    var recurringShift = RecurringShiftServices.Instance.GetRecurringShiftWRTBusiness(Company.Business, usethisShift.ID);
                                    if (recurringShift != null)
                                    {
                                        if (DateTime.Parse(recurringShift.RecurEndDate) >= SelectedDate)
                                        {
                                            if (recurringShift.Frequency == "Bi-Weekly")
                                            {
                                                if (GetNextDayStatus(SelectedDate, usethisShift.Date, SelectedDate.DayOfWeek.ToString()) == "YES")
                                                {
                                                    CheckSlots = FindAvailableSlots(startTimeemp, endTimeemp, appointments, TimeInMinutes);
                                                }
                                            }
                                            else
                                            {
                                                CheckSlots = FindAvailableSlots(startTimeemp, endTimeemp, appointments, TimeInMinutes);
                                            }

                                        }




                                    }
                                    else
                                    {

                                    }
                                }
                                else
                                {
                                    CheckSlots = FindAvailableSlots(startTimeemp, endTimeemp, appointments, TimeInMinutes);


                                }

                                var priceChanges = PriceChangeServices.Instance.GetPriceChangeWRTBusiness(Company.Business);

                                foreach (var timeslot in CheckSlots)
                                {
                                    string timeSlot = timeslot;
                                    bool CheckPriceChange = false;
                                    var slotStart = TimeSpan.Parse(timeslot.Split('-')[0]);
                                    var slotEnd = TimeSpan.Parse(timeslot.Split('-')[1]);
                                    float discountpercentage = 0;
                                    bool ChangeFound = false;
                                    int ChangeID = 0;
                                    string TypeOfChange = "";
                                    if (priceChanges.Count > 0)
                                    {
                                        foreach (var item in priceChanges)
                                        {

                                            if (item.StartDate.Date <= SelectedDate.Date && item.EndDate.Date >= SelectedDate.Date)
                                            {
                                                if (slotStart >= item.StartDate.TimeOfDay && slotEnd <= item.EndDate.TimeOfDay)
                                                {
                                                    discountpercentage = item.Percentage;
                                                    ChangeFound = true;
                                                    ChangeID = item.ID;
                                                    TypeOfChange = item.TypeOfChange;
                                                    //ListOfTimeSlotsWithDiscount.Add(new TimeSlotModel { TimeSlot = timeSlot, TypeOfChange = item.TypeOfChange, HaveDiscount = true, Percentage = discountpercentage,PriceChangeID = item.ID });
                                                    break;

                                                }
                                                else
                                                {

                                                    //ListOfTimeSlotsWithDiscount.Add(new TimeSlotModel { TimeSlot = timeSlot, TypeOfChange = item.TypeOfChange, HaveDiscount = false, Percentage = discountpercentage, PriceChangeID = 0 });
                                                    ChangeFound = false;
                                                }

                                            }
                                            else
                                            {
                                                //ListOfTimeSlotsWithDiscount.Add(new TimeSlotModel { TimeSlot = timeSlot, TypeOfChange = item.TypeOfChange, HaveDiscount = false, Percentage = discountpercentage,PriceChangeID = 0 });
                                                ChangeFound = false;

                                            }

                                        }

                                        if (ChangeFound)
                                        {
                                            ListOfTimeSlotsWithDiscount.Add(new TimeSlotModel { EmployeeID = emp.ID, TimeSlot = timeSlot, TypeOfChange = TypeOfChange, HaveDiscount = true, Percentage = discountpercentage, PriceChangeID = ChangeID });

                                        }
                                        else
                                        {
                                            ListOfTimeSlotsWithDiscount.Add(new TimeSlotModel { EmployeeID = emp.ID, TimeSlot = timeSlot, TypeOfChange = TypeOfChange, HaveDiscount = false, Percentage = discountpercentage, PriceChangeID = 0 });

                                        }
                                    }


                                }

                            }







                        }
                    }
                }

                deduplicatedList = ListOfTimeSlotsWithDiscount
         .GroupBy(x => x.TimeSlot) // Group by TimeSlot
         .Select(g =>
         {
             // Try to find an item where HaveDiscount or EmpHaveDiscount is true
             var prioritizedItem = g
                 .FirstOrDefault(x => x.HaveDiscount || x.EmpHaveDiscount);

             // If none of the items in the group have discounts, just take the first item
             return prioritizedItem ?? g.First();
         })
         .OrderBy(x => DateTime.Parse(x.TimeSlot.Split('-')[0].Trim())) // Extract and parse the start time for ordering
         .ToList();
                // Now combinedEmployeeSlots contains all employees' unique time slots without any duplicates

                //maxSlotsItem = combinedObject;
                return Json(new { ListOfTimeSlotsWithDiscount = deduplicatedList }, JsonRequestBehavior.AllowGet);

            }




            else
            {

                #region EmployeeGetting
                var employeeservices = EmployeeServiceServices.Instance.GetEmployeeServiceWRTBusiness(Company.Business);
                var employeeRequest = EmployeeRequestServices.Instance.GetEmployeeRequestByBusiness(Company.ID);
                foreach (var item in employeeRequest)
                {
                    if (item.Accepted)
                    {
                        employeeservices.AddRange(EmployeeServiceServices.Instance.GetEmployeeServiceWRTEmployeeID(item.EmployeeID));
                    }
                }


                var AvailableEmployeeList = new List<int>();
                var FinalServiceList = services.Select(int.Parse).ToList();

                var employeeIDs = employeeservices
                        .Where(es => FinalServiceList.Contains(es.ServiceID))
                        .GroupBy(es => es.EmployeeID)
                        .Where(grp => grp.Select(g => g.ServiceID).Distinct().Count() == FinalServiceList.Count)
                        .Select(grp => grp.Key)
                        .Except(disabledEmployees)
                        .ToList();

                #endregion
                var ListOfEmployeeSlotsCount = new List<SlotsListWithEmployeeIDModel>();
                var NewiteratedEmployeeList = new List<TimeSlotModel>();
                foreach (var empnew in employeeIDs)
                {
                    var empShifts = new List<ShiftModel>();
                    var emp = EmployeeServices.Instance.GetEmployee(empnew);
                    if (emp != null)
                    {
                       
                        if (emp.AllowOnlineBooking)
                        {
                            while (true)
                            {
                                var appointments = AppointmentServices.Instance.GetAppointmentBookingWRTBusiness(SelectedDate.Day, SelectedDate.Month, SelectedDate.Year, empnew, false, false).ToList();

                                var roster = TimeTableRosterServices.Instance.GetTimeTableRosterByEmpID(emp.ID);
                                if (roster != null)
                                {
                                    var shifts = ShiftServices.Instance.GetShiftWRTBusiness(Company.Business, emp.ID);
                                    if (shifts.Count > 0)
                                    {
                                        foreach (var shift in shifts)
                                        {
                                            var recurringShifts = RecurringShiftServices.Instance.GetRecurringShiftWRTBusiness(Company.Business, shift.ID);
                                            if (recurringShifts != null)
                                            {
                                                if (recurringShifts.RecurEnd == "Custom Date" && recurringShifts.RecurEndDate != "")
                                                {
                                                    if (IsDateInRangeNew(DateTime.Parse(recurringShifts.RecurEndDate), SelectedDate))
                                                    {

                                                        if (recurringShifts.Frequency == "Bi-Weekly")
                                                        {

                                                            if (GetNextDayStatus(SelectedDate, shift.Date, shift.Day.ToString()) == "YES")
                                                            {
                                                                var exceptionShift = ExceptionShiftServices.Instance.GetExceptionShiftWRTBusiness(Company.Business, shift.ID);
                                                                empShifts.Add(new ShiftModel { Shift = shift, RecurShift = recurringShifts, ExceptionShift = exceptionShift });
                                                            }


                                                        }
                                                        else
                                                        {
                                                            var exceptionShift = ExceptionShiftServices.Instance.GetExceptionShiftWRTBusiness(Company.Business, shift.ID);
                                                            empShifts.Add(new ShiftModel { Shift = shift, RecurShift = recurringShifts, ExceptionShift = exceptionShift });
                                                        }
                                                    }
                                                }
                                                else
                                                {
                                                    if (recurringShifts.Frequency == "Bi-Weekly")
                                                    {
                                                        if (roster != null)
                                                        {
                                                            if (GetNextDayStatus(SelectedDate, shift.Date, shift.Day.ToString()) == "YES")
                                                            {
                                                                var exceptionShift = ExceptionShiftServices.Instance.GetExceptionShiftWRTBusiness(Company.Business, shift.ID);
                                                                empShifts.Add(new ShiftModel { Shift = shift, RecurShift = recurringShifts, ExceptionShift = exceptionShift });
                                                            }
                                                        }

                                                    }
                                                    else
                                                    {
                                                        var exceptionShift = ExceptionShiftServices.Instance.GetExceptionShiftWRTBusiness(Company.Business, shift.ID);
                                                        empShifts.Add(new ShiftModel { Shift = shift, RecurShift = recurringShifts, ExceptionShift = exceptionShift });
                                                    }
                                                }
                                            }
                                            else
                                            {
                                                var exceptionShiftByShiftID = ExceptionShiftServices.Instance.GetExceptionShiftWRTBusinessAndShift(Company.Business, shift.ID);
                                                if (exceptionShiftByShiftID != null)
                                                {
                                                    empShifts.Add(new ShiftModel { Shift = shift, RecurShift = recurringShifts, ExceptionShift = exceptionShiftByShiftID });
                                                }
                                                else
                                                {
                                                    empShifts.Add(new ShiftModel { Shift = shift, RecurShift = recurringShifts });

                                                }

                                            }
                                        }

                                        var usethisShift = empShifts.Where(x => x.RecurShift != null && x.Shift.IsRecurring && x.Shift.Day == SelectedDate.DayOfWeek.ToString() && x.Shift.Date <= SelectedDate.Date).Select(X => X.Shift).FirstOrDefault();
                                        var useExceptionShifts = empShifts
                                            .Where(x => x.ExceptionShift != null && x.ExceptionShift.Count() > 0 && !x.Shift.IsRecurring)
                                            .SelectMany(x => x.ExceptionShift);

                                        if (usethisShift == null && useExceptionShifts == null)
                                        {
                                            //okay bye
                                        }
                                        else
                                        {
                                            if (usethisShift != null)
                                            {
                                                bool FoundExceptionToo = false;
                                                var FoundedExceptionShift = new ExceptionShift();
                                                var empShiftsCount = empShifts.Count();
                                                if (empShiftsCount > 0) // Ensure there's at least one element in empShifts
                                                {
                                                    for (int i = 0; i < empShiftsCount; i++)
                                                    {
                                                        var firstEmpShift = empShifts[i]; // Accessing empShifts at index i
                                                        foreach (var item in firstEmpShift.ExceptionShift.ToList())
                                                        {
                                                            if (item.ShiftID == firstEmpShift.Shift.ID && item.ExceptionDate.ToString("yyyy-MM-dd") == SelectedDate.ToString("yyyy-MM-dd") && item.IsNotWorking == false)
                                                            {
                                                                FoundExceptionToo = true;
                                                                FoundedExceptionShift = item;
                                                                break;
                                                            }
                                                            if (item.ShiftID == firstEmpShift.Shift.ID && item.ExceptionDate.ToString("yyyy-MM-dd") == SelectedDate.ToString("yyyy-MM-dd") && item.IsNotWorking == true)
                                                            {
                                                                FoundExceptionToo = true;
                                                                FoundedExceptionShift = item;
                                                                break;
                                                            }
                                                        }


                                                    }

                                                }
                                                if (FoundExceptionToo)
                                                {

                                                    if (FoundedExceptionShift.ID != 0 && !FoundedExceptionShift.IsNotWorking)
                                                    {
                                                        DateTime startTime = SelectedDate.Date
                                                       .AddHours(DateTime.ParseExact(FoundedExceptionShift.StartTime, "H:mm", CultureInfo.InvariantCulture).Hour)
                                                       .AddMinutes(DateTime.ParseExact(FoundedExceptionShift.StartTime, "H:mm", CultureInfo.InvariantCulture).Minute);

                                                        DateTime endTime = SelectedDate.Date
                                                            .AddHours(DateTime.ParseExact(FoundedExceptionShift.EndTime, "H:mm", CultureInfo.InvariantCulture).Hour)
                                                            .AddMinutes(DateTime.ParseExact(FoundedExceptionShift.EndTime, "H:mm", CultureInfo.InvariantCulture).Minute);


                                                        var CheckSlots = new List<string>();



                                                        CheckSlots = FindAvailableSlots(startTime, endTime, appointments, TimeInMinutes);



                                                        if (CheckSlots.Count() != 0)
                                                        {
                                                            var priceChanges = PriceChangeServices.Instance.GetPriceChangeWRTBusiness(Company.Business, "");
                                                            foreach (var timeslot in CheckSlots)
                                                            {
                                                                string timeSlot = timeslot;
                                                                bool CheckPriceChange = false;
                                                                var slotStart = TimeSpan.Parse(timeslot.Split('-')[0]);
                                                                var slotEnd = TimeSpan.Parse(timeslot.Split('-')[1]);
                                                                float discountpercentage = 0;
                                                                bool ChangeFound = false;
                                                                int ChangeID = 0;
                                                                string TypeOfChange = "";
                                                                var employeePriceChange = GetPriceChange(emp.ID, SelectedDate, slotStart, slotEnd);

                                                                if (priceChanges.Count() > 0)
                                                                {
                                                                    foreach (var item in priceChanges)
                                                                    {

                                                                        if (item.StartDate.Date <= SelectedDate.Date && item.EndDate.Date >= SelectedDate.Date)
                                                                        {
                                                                            if (slotStart >= item.StartDate.TimeOfDay && slotEnd <= item.EndDate.TimeOfDay)
                                                                            {
                                                                                discountpercentage = item.Percentage;
                                                                                ChangeFound = true;
                                                                                ChangeID = item.ID;
                                                                                TypeOfChange = item.TypeOfChange;
                                                                                //ListOfTimeSlotsWithDiscount.Add(new TimeSlotModel { TimeSlot = timeSlot, TypeOfChange = item.TypeOfChange, HaveDiscount = true, Percentage = discountpercentage,PriceChangeID = item.ID });
                                                                                break;

                                                                            }
                                                                            else
                                                                            {

                                                                                //ListOfTimeSlotsWithDiscount.Add(new TimeSlotModel { TimeSlot = timeSlot, TypeOfChange = item.TypeOfChange, HaveDiscount = false, Percentage = discountpercentage, PriceChangeID = 0 });
                                                                                ChangeFound = false;
                                                                            }

                                                                        }
                                                                        else
                                                                        {
                                                                            //ListOfTimeSlotsWithDiscount.Add(new TimeSlotModel { TimeSlot = timeSlot, TypeOfChange = item.TypeOfChange, HaveDiscount = false, Percentage = discountpercentage,PriceChangeID = 0 });
                                                                            ChangeFound = false;

                                                                        }

                                                                    }

                                                                    if (ChangeFound)
                                                                    {
                                                                        if (employeePriceChange.EmployeeID != 0)
                                                                        {
                                                                            NewiteratedEmployeeList.Add(new TimeSlotModel
                                                                            {
                                                                                TimeSlot = timeSlot,
                                                                                TypeOfChange = TypeOfChange,
                                                                                HaveDiscount = true,
                                                                                Percentage = discountpercentage,
                                                                                PriceChangeID = ChangeID,
                                                                                EmpHaveDiscount = true,
                                                                                EmployeeID = emp.ID,
                                                                                EmpPercentage = employeePriceChange.Percentage,
                                                                                EmpPriceChangeID = employeePriceChange.ID,
                                                                                EmpTypeOfChange = employeePriceChange.TypeOfChange
                                                                            });

                                                                        }
                                                                        else
                                                                        {
                                                                            NewiteratedEmployeeList.Add(new TimeSlotModel { TimeSlot = timeSlot, EmployeeID = emp.ID, TypeOfChange = TypeOfChange, HaveDiscount = true, Percentage = discountpercentage, PriceChangeID = ChangeID });
                                                                        }
                                                                    }
                                                                    else
                                                                    {
                                                                        if (employeePriceChange.EmployeeID != 0)
                                                                        {
                                                                            NewiteratedEmployeeList.Add(new TimeSlotModel
                                                                            {
                                                                                TimeSlot = timeSlot,
                                                                                TypeOfChange = TypeOfChange,
                                                                                HaveDiscount = false,
                                                                                Percentage = discountpercentage,
                                                                                PriceChangeID = 0,
                                                                                EmpHaveDiscount = true,
                                                                                EmpPercentage = employeePriceChange.Percentage,
                                                                                EmpPriceChangeID = employeePriceChange.ID,
                                                                                EmployeeID = emp.ID,
                                                                                EmpTypeOfChange = employeePriceChange.TypeOfChange
                                                                            });

                                                                        }
                                                                        else
                                                                        {
                                                                            NewiteratedEmployeeList.Add(new TimeSlotModel { TimeSlot = timeSlot, EmployeeID = emp.ID, TypeOfChange = TypeOfChange, HaveDiscount = false, Percentage = discountpercentage, PriceChangeID = 0 });
                                                                        }

                                                                    }
                                                                }
                                                                else
                                                                {
                                                                    if (employeePriceChange.EmployeeID != 0)
                                                                    {
                                                                        NewiteratedEmployeeList.Add(new TimeSlotModel
                                                                        {
                                                                            TimeSlot = timeSlot,
                                                                            TypeOfChange = TypeOfChange,
                                                                            HaveDiscount = false,
                                                                            Percentage = discountpercentage,
                                                                            PriceChangeID = 0,
                                                                            EmployeeID = emp.ID,
                                                                            EmpHaveDiscount = true,
                                                                            EmpPercentage = employeePriceChange.Percentage,
                                                                            EmpPriceChangeID = employeePriceChange.ID,
                                                                            EmpTypeOfChange = employeePriceChange.TypeOfChange
                                                                        });

                                                                    }
                                                                    else
                                                                    {
                                                                        NewiteratedEmployeeList.Add(new TimeSlotModel { EmployeeID = emp.ID, TimeSlot = timeSlot, TypeOfChange = TypeOfChange, HaveDiscount = false, Percentage = discountpercentage, PriceChangeID = 0 });
                                                                    }
                                                                }
                                                            }

                                                            //ListOfEmployeeSlotsCount.Add(new SlotsListWithEmployeeIDModel { EmployeeID=emp.ID, NoOfSlots = NewiteratedEmployeeList });
                                                            //Doubt Full Line
                                                        }
                                                    }



                                                }
                                                else
                                                {
                                                    DateTime startTime = SelectedDate.Date
                                                               .AddHours(DateTime.ParseExact(usethisShift.StartTime, "H:mm", CultureInfo.InvariantCulture).Hour)
                                                               .AddMinutes(DateTime.ParseExact(usethisShift.StartTime, "H:mm", CultureInfo.InvariantCulture).Minute);

                                                    DateTime endTime = SelectedDate.Date
                                                        .AddHours(DateTime.ParseExact(usethisShift.EndTime, "H:mm", CultureInfo.InvariantCulture).Hour)
                                                        .AddMinutes(DateTime.ParseExact(usethisShift.EndTime, "H:mm", CultureInfo.InvariantCulture).Minute);

                                                    var CheckSlots = new List<string>();

                                                    if (usethisShift.IsRecurring)
                                                    {
                                                        var recurringShift = RecurringShiftServices.Instance.GetRecurringShiftWRTBusiness(Company.Business, usethisShift.ID);
                                                        if (recurringShift != null)
                                                        {
                                                            if (recurringShift.RecurEnd != "Never")
                                                            {
                                                                if (DateTime.Parse(recurringShift.RecurEndDate) >= SelectedDate)
                                                                {
                                                                    if (recurringShift.Frequency == "Bi-Weekly")
                                                                    {
                                                                        if (GetNextDayStatus(SelectedDate, usethisShift.Date, SelectedDate.DayOfWeek.ToString()) == "YES")
                                                                        {
                                                                            CheckSlots = FindAvailableSlots(startTime, endTime, appointments, TimeInMinutes);
                                                                        }
                                                                    }
                                                                    else
                                                                    {
                                                                        CheckSlots = FindAvailableSlots(startTime, endTime, appointments, TimeInMinutes);
                                                                    }
                                                                    //if (CheckSlots.Count() != 0)
                                                                    //{
                                                                    //    ListOfEmployeeSlotsCount.Add(new SlotsListWithEmployeeIDModel { NoOfSlots = CheckSlots, EmployeeID = EmployeeID });
                                                                    //}
                                                                }
                                                            }
                                                            else
                                                            {
                                                                if (recurringShift.Frequency == "Bi-Weekly")
                                                                {
                                                                    if (GetNextDayStatus(SelectedDate, usethisShift.Date, SelectedDate.DayOfWeek.ToString()) == "YES")
                                                                    {
                                                                        CheckSlots = FindAvailableSlots(startTime, endTime, appointments, TimeInMinutes);
                                                                    }
                                                                }
                                                                else
                                                                {
                                                                    CheckSlots = FindAvailableSlots(startTime, endTime, appointments, TimeInMinutes);
                                                                }
                                                            }


                                                        }
                                                        else
                                                        {

                                                        }
                                                    }
                                                    else
                                                    {
                                                        CheckSlots = FindAvailableSlots(startTime, endTime, appointments, TimeInMinutes);

                                                    }

                                                    var priceChanges = PriceChangeServices.Instance.GetPriceChangeWRTBusiness(Company.Business, "");
                                                    foreach (var timeslot in CheckSlots)
                                                    {
                                                        string timeSlot = timeslot;
                                                        bool CheckPriceChange = false;
                                                        var slotStart = TimeSpan.Parse(timeslot.Split('-')[0]);
                                                        var slotEnd = TimeSpan.Parse(timeslot.Split('-')[1]);
                                                        float discountpercentage = 0;
                                                        bool ChangeFound = false;
                                                        int ChangeID = 0;
                                                        string TypeOfChange = "";
                                                        var employeePriceChange = GetPriceChange(emp.ID, SelectedDate, slotStart, slotEnd);

                                                        if (priceChanges.Count() > 0)
                                                        {
                                                            foreach (var item in priceChanges)
                                                            {

                                                                if (item.StartDate.Date <= SelectedDate.Date && item.EndDate.Date >= SelectedDate.Date)
                                                                {
                                                                    if (slotStart >= item.StartDate.TimeOfDay && slotEnd <= item.EndDate.TimeOfDay)
                                                                    {
                                                                        discountpercentage = item.Percentage;
                                                                        ChangeFound = true;
                                                                        ChangeID = item.ID;
                                                                        TypeOfChange = item.TypeOfChange;
                                                                        //ListOfTimeSlotsWithDiscount.Add(new TimeSlotModel { TimeSlot = timeSlot, TypeOfChange = item.TypeOfChange, HaveDiscount = true, Percentage = discountpercentage,PriceChangeID = item.ID });
                                                                        break;

                                                                    }
                                                                    else
                                                                    {

                                                                        //ListOfTimeSlotsWithDiscount.Add(new TimeSlotModel { TimeSlot = timeSlot, TypeOfChange = item.TypeOfChange, HaveDiscount = false, Percentage = discountpercentage, PriceChangeID = 0 });
                                                                        ChangeFound = false;
                                                                    }

                                                                }
                                                                else
                                                                {
                                                                    //ListOfTimeSlotsWithDiscount.Add(new TimeSlotModel { TimeSlot = timeSlot, TypeOfChange = item.TypeOfChange, HaveDiscount = false, Percentage = discountpercentage,PriceChangeID = 0 });
                                                                    ChangeFound = false;

                                                                }

                                                            }

                                                            if (ChangeFound)
                                                            {
                                                                if (employeePriceChange.EmployeeID != 0)
                                                                {
                                                                    NewiteratedEmployeeList.Add(new TimeSlotModel
                                                                    {
                                                                        TimeSlot = timeSlot,
                                                                        TypeOfChange = TypeOfChange,
                                                                        HaveDiscount = true,
                                                                        Percentage = discountpercentage,
                                                                        PriceChangeID = ChangeID,
                                                                        EmpHaveDiscount = true,
                                                                        EmpPercentage = employeePriceChange.Percentage,
                                                                        EmployeeID = emp.ID,
                                                                        EmpPriceChangeID = employeePriceChange.ID,
                                                                        EmpTypeOfChange = employeePriceChange.TypeOfChange
                                                                    });

                                                                }
                                                                else
                                                                {
                                                                    NewiteratedEmployeeList.Add(new TimeSlotModel { TimeSlot = timeSlot, EmployeeID = emp.ID, TypeOfChange = TypeOfChange, HaveDiscount = true, Percentage = discountpercentage, PriceChangeID = ChangeID });
                                                                }
                                                            }
                                                            else
                                                            {
                                                                if (employeePriceChange.EmployeeID != 0)
                                                                {
                                                                    NewiteratedEmployeeList.Add(new TimeSlotModel
                                                                    {
                                                                        TimeSlot = timeSlot,
                                                                        TypeOfChange = TypeOfChange,
                                                                        HaveDiscount = false,
                                                                        Percentage = discountpercentage,
                                                                        EmployeeID = emp.ID,
                                                                        PriceChangeID = 0,
                                                                        EmpHaveDiscount = true,
                                                                        EmpPercentage = employeePriceChange.Percentage,
                                                                        EmpPriceChangeID = employeePriceChange.ID,
                                                                        EmpTypeOfChange = employeePriceChange.TypeOfChange
                                                                    });

                                                                }
                                                                else
                                                                {
                                                                    NewiteratedEmployeeList.Add(new TimeSlotModel { TimeSlot = timeSlot, EmployeeID = emp.ID, TypeOfChange = TypeOfChange, HaveDiscount = false, Percentage = discountpercentage, PriceChangeID = 0 });
                                                                }

                                                            }
                                                        }
                                                        else
                                                        {
                                                            if (employeePriceChange.EmployeeID != 0)
                                                            {
                                                                NewiteratedEmployeeList.Add(new TimeSlotModel
                                                                {
                                                                    TimeSlot = timeSlot,
                                                                    TypeOfChange = TypeOfChange,
                                                                    HaveDiscount = false,
                                                                    Percentage = discountpercentage,
                                                                    PriceChangeID = 0,
                                                                    EmployeeID = emp.ID,
                                                                    EmpHaveDiscount = true,
                                                                    EmpPercentage = employeePriceChange.Percentage,
                                                                    EmpPriceChangeID = employeePriceChange.ID,
                                                                    EmpTypeOfChange = employeePriceChange.TypeOfChange
                                                                });

                                                            }
                                                            else
                                                            {
                                                                NewiteratedEmployeeList.Add(new TimeSlotModel { TimeSlot = timeSlot, EmployeeID = emp.ID, TypeOfChange = TypeOfChange, HaveDiscount = false, Percentage = discountpercentage, PriceChangeID = 0 });
                                                            }
                                                        }
                                                    }
                                                    //ListOfEmployeeSlotsCount.Add(new SlotsListWithEmployeeIDModel { NoOfSlots = NewiteratedEmployeeList, EmployeeID = emp.ID });


                                                }

                                            }
                                            else
                                            {

                                                if (useExceptionShifts != null)
                                                {
                                                    bool FoundExceptionToo = false;
                                                    var FoundedExceptionShift = new ExceptionShift();
                                                    var empShiftsCount = empShifts.Count();
                                                    if (empShiftsCount > 0) // Ensure there's at least one element in empShifts
                                                    {
                                                        for (int i = 0; i < empShiftsCount; i++)
                                                        {
                                                            var firstEmpShift = empShifts[i]; // Accessing empShifts at index i
                                                            foreach (var item in firstEmpShift.ExceptionShift.ToList())
                                                            {
                                                                if (item.ShiftID == firstEmpShift.Shift.ID && item.ExceptionDate.ToString("yyyy-MM-dd") == SelectedDate.ToString("yyyy-MM-dd") && item.IsNotWorking == false)
                                                                {
                                                                    FoundExceptionToo = true;
                                                                    FoundedExceptionShift = item;
                                                                    break;
                                                                }
                                                                if (item.ShiftID == firstEmpShift.Shift.ID && item.ExceptionDate.ToString("yyyy-MM-dd") == SelectedDate.ToString("yyyy-MM-dd") && item.IsNotWorking == true)
                                                                {
                                                                    FoundExceptionToo = true;
                                                                    FoundedExceptionShift = item;
                                                                    break;
                                                                }
                                                            }


                                                        }

                                                    }
                                                    if (FoundExceptionToo)
                                                    {

                                                        if (FoundedExceptionShift.ID != 0 && !FoundedExceptionShift.IsNotWorking)
                                                        {
                                                            DateTime startTime = SelectedDate.Date
                                                           .AddHours(DateTime.ParseExact(FoundedExceptionShift.StartTime, "H:mm", CultureInfo.InvariantCulture).Hour)
                                                           .AddMinutes(DateTime.ParseExact(FoundedExceptionShift.StartTime, "H:mm", CultureInfo.InvariantCulture).Minute);

                                                            DateTime endTime = SelectedDate.Date
                                                                .AddHours(DateTime.ParseExact(FoundedExceptionShift.EndTime, "H:mm", CultureInfo.InvariantCulture).Hour)
                                                                .AddMinutes(DateTime.ParseExact(FoundedExceptionShift.EndTime, "H:mm", CultureInfo.InvariantCulture).Minute);


                                                            var CheckSlots = new List<string>();



                                                            CheckSlots = FindAvailableSlots(startTime, endTime, appointments, TimeInMinutes);



                                                            if (CheckSlots.Count() != 0)
                                                            {
                                                                var priceChanges = PriceChangeServices.Instance.GetPriceChangeWRTBusiness(Company.Business, "");
                                                                foreach (var timeslot in CheckSlots)
                                                                {
                                                                    string timeSlot = timeslot;
                                                                    bool CheckPriceChange = false;
                                                                    var slotStart = TimeSpan.Parse(timeslot.Split('-')[0]);
                                                                    var slotEnd = TimeSpan.Parse(timeslot.Split('-')[1]);
                                                                    float discountpercentage = 0;
                                                                    bool ChangeFound = false;
                                                                    int ChangeID = 0;
                                                                    string TypeOfChange = "";
                                                                    var employeePriceChange = GetPriceChange(emp.ID, SelectedDate, slotStart, slotEnd);

                                                                    if (priceChanges.Count() > 0)
                                                                    {
                                                                        foreach (var item in priceChanges)
                                                                        {

                                                                            if (item.StartDate.Date <= SelectedDate.Date && item.EndDate.Date >= SelectedDate.Date)
                                                                            {
                                                                                if (slotStart >= item.StartDate.TimeOfDay && slotEnd <= item.EndDate.TimeOfDay)
                                                                                {
                                                                                    discountpercentage = item.Percentage;
                                                                                    ChangeFound = true;
                                                                                    ChangeID = item.ID;
                                                                                    TypeOfChange = item.TypeOfChange;
                                                                                    //ListOfTimeSlotsWithDiscount.Add(new TimeSlotModel { TimeSlot = timeSlot, TypeOfChange = item.TypeOfChange, HaveDiscount = true, Percentage = discountpercentage,PriceChangeID = item.ID });
                                                                                    break;

                                                                                }
                                                                                else
                                                                                {

                                                                                    //ListOfTimeSlotsWithDiscount.Add(new TimeSlotModel { TimeSlot = timeSlot, TypeOfChange = item.TypeOfChange, HaveDiscount = false, Percentage = discountpercentage, PriceChangeID = 0 });
                                                                                    ChangeFound = false;
                                                                                }

                                                                            }
                                                                            else
                                                                            {
                                                                                //ListOfTimeSlotsWithDiscount.Add(new TimeSlotModel { TimeSlot = timeSlot, TypeOfChange = item.TypeOfChange, HaveDiscount = false, Percentage = discountpercentage,PriceChangeID = 0 });
                                                                                ChangeFound = false;

                                                                            }

                                                                        }

                                                                        if (ChangeFound)
                                                                        {
                                                                            if (employeePriceChange.EmployeeID != 0)
                                                                            {
                                                                                NewiteratedEmployeeList.Add(new TimeSlotModel
                                                                                {
                                                                                    TimeSlot = timeSlot,
                                                                                    TypeOfChange = TypeOfChange,
                                                                                    HaveDiscount = true,
                                                                                    Percentage = discountpercentage,
                                                                                    PriceChangeID = ChangeID,
                                                                                    EmpHaveDiscount = true,
                                                                                    EmpPercentage = employeePriceChange.Percentage,
                                                                                    EmpPriceChangeID = employeePriceChange.ID,
                                                                                    EmployeeID = emp.ID,
                                                                                    EmpTypeOfChange = employeePriceChange.TypeOfChange
                                                                                });

                                                                            }
                                                                            else
                                                                            {
                                                                                NewiteratedEmployeeList.Add(new TimeSlotModel { EmployeeID = emp.ID, TimeSlot = timeSlot, TypeOfChange = TypeOfChange, HaveDiscount = true, Percentage = discountpercentage, PriceChangeID = ChangeID });
                                                                            }
                                                                        }
                                                                        else
                                                                        {
                                                                            if (employeePriceChange.EmployeeID != 0)
                                                                            {
                                                                                NewiteratedEmployeeList.Add(new TimeSlotModel
                                                                                {
                                                                                    TimeSlot = timeSlot,
                                                                                    TypeOfChange = TypeOfChange,
                                                                                    HaveDiscount = false,
                                                                                    Percentage = discountpercentage,
                                                                                    PriceChangeID = 0,
                                                                                    EmpHaveDiscount = true,
                                                                                    EmployeeID = emp.ID,
                                                                                    EmpPercentage = employeePriceChange.Percentage,
                                                                                    EmpPriceChangeID = employeePriceChange.ID,
                                                                                    EmpTypeOfChange = employeePriceChange.TypeOfChange
                                                                                });

                                                                            }
                                                                            else
                                                                            {
                                                                                NewiteratedEmployeeList.Add(new TimeSlotModel { EmployeeID = emp.ID, TimeSlot = timeSlot, TypeOfChange = TypeOfChange, HaveDiscount = false, Percentage = discountpercentage, PriceChangeID = 0 });
                                                                            }

                                                                        }
                                                                    }
                                                                    else
                                                                    {
                                                                        if (employeePriceChange.EmployeeID != 0)
                                                                        {
                                                                            NewiteratedEmployeeList.Add(new TimeSlotModel
                                                                            {
                                                                                TimeSlot = timeSlot,
                                                                                TypeOfChange = TypeOfChange,
                                                                                HaveDiscount = false,
                                                                                Percentage = discountpercentage,
                                                                                PriceChangeID = 0,
                                                                                EmployeeID = emp.ID,
                                                                                EmpHaveDiscount = true,
                                                                                EmpPercentage = employeePriceChange.Percentage,
                                                                                EmpPriceChangeID = employeePriceChange.ID,
                                                                                EmpTypeOfChange = employeePriceChange.TypeOfChange
                                                                            });

                                                                        }
                                                                        else
                                                                        {
                                                                            NewiteratedEmployeeList.Add(new TimeSlotModel
                                                                            {
                                                                                TimeSlot = timeSlot,
                                                                                TypeOfChange = TypeOfChange,
                                                                                HaveDiscount = false,
                                                                                Percentage = discountpercentage,
                                                                                PriceChangeID = 0,
                                                                                EmployeeID = emp.ID,
                                                                            });
                                                                        }
                                                                    }
                                                                }
                                                                //ListOfEmployeeSlotsCount.Add(new SlotsListWithEmployeeIDModel { NoOfSlots = NewiteratedEmployeeList, EmployeeID = emp.ID });

                                                            }
                                                        }



                                                    }

                                                }

                                            }
                                        }
                                    }
                                    else
                                    {
                                        break;
                                    }

                                }
                                else
                                {
                                    break;
                                }




                                if (DoesSelected)
                                {
                                    break;
                                }
                                else
                                {
                                    SelectedDate = SelectedDate.AddDays(1);
                                }
                            }

                        }
                    }
                }


                deduplicatedList = NewiteratedEmployeeList
        .GroupBy(x => x.TimeSlot) // Group by TimeSlot
        .Select(g =>
        {
            // Try to find an item where HaveDiscount or EmpHaveDiscount is true
            var prioritizedItem = g
                .FirstOrDefault(x => x.HaveDiscount || x.EmpHaveDiscount);

            // If none of the items in the group have discounts, just take the first item
            return prioritizedItem ?? g.First();
        })
        .OrderBy(x => DateTime.Parse(x.TimeSlot.Split('-')[0].Trim())) // Extract and parse the start time for ordering
        .ToList();


                // Now combinedEmployeeSlots contains all employees' unique time slots without any duplicates

                //maxSlotsItem = combinedObject;
                return Json(new { ListOfTimeSlotsWithDiscount = deduplicatedList }, JsonRequestBehavior.AllowGet);


            }

            //if (maxSlotsItem != null)
            //{

            //    if (maxSlotsItem.EmployeeID != 0)
            //    {
            //        return Json(new { ListOfTimeSlotsWithDiscount = deduplicatedList, FinalEmployeeID = maxSlotsItem.EmployeeID }, JsonRequestBehavior.AllowGet);
            //    }
            //    else
            //    {
            //        return Json(new { ListOfTimeSlotsWithDiscount = maxSlotsItem.NoOfSlots, FinalEmployeeID = maxSlotsItem.EmployeeID }, JsonRequestBehavior.AllowGet);
            //    }
            //}
            //else
            //{
            //    var emptyList = new List<TimeSlotModel>();
            //    return Json(new { ListOfTimeSlotsWithDiscount = emptyList, FinalEmployeeID = EmployeeID }, JsonRequestBehavior.AllowGet);
            //}
        }




        [HttpGet]
        public JsonResult GetEmployeeData(int ID)
        {
            var Employee = EmployeeServices.Instance.GetEmployee(ID);
            return Json(new { success = true, Employee = Employee }, JsonRequestBehavior.AllowGet);
        }

    }


}