using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using TheBookingPlatform.Database.Migrations;
using TheBookingPlatform.Entities;
using TheBookingPlatform.Services;
using TheBookingPlatform.ViewModels;
using Newtonsoft.Json.Serialization;
using static TheBookingPlatform.Controllers.AppointmentController;

namespace TheBookingPlatform.Controllers
{
    public class EmployeeRequestController : Controller
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
        public EmployeeRequestController()
        {
        }
        public EmployeeRequestController(AMUserManager userManager, AMSignInManager signInManager)
        {
            UserManager = userManager;
            SignInManager = signInManager;
        }




        public void AcceptRequest(int ID)
        {
            var employeeRequest = EmployeeRequestServices.Instance.GetEmployeeRequest(ID);
            employeeRequest.Status = "Accepted";
            employeeRequest.Accepted = true;
            EmployeeRequestServices.Instance.UpdateEmployeeRequest(employeeRequest);
        }
        public void DeclineRequest(int ID)
        {
            var employeeRequest = EmployeeRequestServices.Instance.GetEmployeeRequest(ID);
            employeeRequest.Status = "Declined";
            employeeRequest.Accepted = false;
            EmployeeRequestServices.Instance.UpdateEmployeeRequest(employeeRequest);
        }



        [HttpPost]
        public JsonResult SendAppointments()
        {
            var loggedInUser = UserManager.FindById(User.Identity.GetUserId());
            var googleCalendar = GoogleCalendarServices.Instance.GetGoogleCalendarServicesWRTBusiness(loggedInUser.Company);
            googleCalendar.InitialSetup = true;
            var eventswtiches = GEventSwitchServices.Instance.GetGEventSwitch().Where(x => x.Business == loggedInUser.Company).FirstOrDefault();
            if(eventswtiches != null)
            {
                eventswtiches.SwitchStatus = true;
                GEventSwitchServices.Instance.UpdateGEventSwitch(eventswtiches);
            }
            else
            {
                eventswtiches = new GEventSwitch();
                eventswtiches.SwitchStatus = true;
                eventswtiches.Business = loggedInUser.Company;
                GEventSwitchServices.Instance.SaveGEventSwitch(eventswtiches);  
            }
            GoogleCalendarServices.Instance.UpdateGoogleCalendarIntegration(googleCalendar);

            return Json(new { success = true }, JsonRequestBehavior.AllowGet);


        }

        public string GenerateonGoogleCalendar(int ID, string Services)
        {
            // Retrieve appointment
            var appointment = AppointmentServices.Instance.GetAppointment(ID);
            if (appointment == null)
            {
                return "Error: Appointment not found.";
            }

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

            // Retrieve logged-in user
            var loggedInUser = UserManager.FindById(User.Identity.GetUserId());
            if (loggedInUser == null)
            {
                return "Error: Logged-in user not found.";
            }

            // Retrieve Google Calendar services
            var googleCalendar = GoogleCalendarServices.Instance.GetGoogleCalendarServicesWRTBusiness(loggedInUser.Company);
            if (googleCalendar == null)
            {
                return "Error: Google Calendar service not found.";
            }

            // Retrieve company
            var company = CompanyServices.Instance.GetCompany().FirstOrDefault(x => x.Business == loggedInUser.Company);
            if (company == null)
            {
                return "Error: Company not found.";
            }

            // Retrieve employee
            var employee = EmployeeServices.Instance.GetEmployee(appointment.EmployeeID);
            if (employee == null)
            {
                return "Error: Employee not found.";
            }

            var url = $"https://www.googleapis.com/calendar/v3/calendars/{employee.GoogleCalendarID}/events";
            var finalUrl = new Uri(url);

            RestClient restClient = new RestClient(finalUrl);
            RestRequest request = new RestRequest();
            var calendarEvent = new Event
            {
                Summary = "Appointment at: " + loggedInUser.Company,
                Description = Services,
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
                return "Saved";
            }
            else
            {
                return "Error: " + response.Content;
            }
        }

        // GET: EmployeeRequest
        public ActionResult Index()
        {
            EmployeeRequestListingViewModel model = new EmployeeRequestListingViewModel();
            var loggedInUser = UserManager.FindById(User.Identity.GetUserId());
            var company = CompanyServices.Instance.GetCompany().Where(x => x.Business == loggedInUser.Company).FirstOrDefault();
            model.LoggedInCompany = company.Business;
            var listofEmployeeRequestModel = new List<EmployeeRequestModel>();
            if (loggedInUser.Role != "Super Admin")
            {
                var employeeRequest = EmployeeRequestServices.Instance.GetEmployeeRequestByBusiness(company.ID);
                foreach (var item in employeeRequest)
                {

                    var companyFrom = CompanyServices.Instance.GetCompany(item.CompanyIDFrom);
                    var companyFor = CompanyServices.Instance.GetCompany(item.CompanyIDFor);
                    var employee = EmployeeServices.Instance.GetEmployee(item.EmployeeID);
                    listofEmployeeRequestModel.Add(new EmployeeRequestModel
                    {
                        EmployeeRequest = item,
                        CompanyFrom = companyFrom != null ? companyFrom.Business : "-",
                        CompanyFor = companyFor != null ? companyFor.Business : "-",
                        Employee = employee != null ? employee.Name : "-"
                    });
                }


            }
            else
            {
                var employeeRequest = EmployeeRequestServices.Instance.GetEmployeeRequest();
                foreach (var item in employeeRequest)
                {
                    var companyFrom = CompanyServices.Instance.GetCompany(item.CompanyIDFrom);
                    var employee = EmployeeServices.Instance.GetEmployee(item.EmployeeID);
                    listofEmployeeRequestModel.Add(new EmployeeRequestModel
                    {
                        EmployeeRequest = item,
                        CompanyFrom = companyFrom != null ? companyFrom.Business : "-",
                        Employee = employee != null ? employee.Name : "-"
                    });


                }
            }
            model.EmployeeRequests = listofEmployeeRequestModel;



            var listofFranchiseRequestModel = new List<FranchiseRequestModel>();
            if (loggedInUser.Role != "Super Admin")
            {
                var FranchiseRequest = FranchiseRequestServices.Instance.GetFranchiseRequestByBusiness(company.ID);
                foreach (var item in FranchiseRequest)
                {

                    var companyFrom = CompanyServices.Instance.GetCompany(item.CompanyIDFrom);
                    var companyFor = CompanyServices.Instance.GetCompany(item.CompanyIDFor);
                    var user = UserManager.FindById(item.UserID);
                    listofFranchiseRequestModel.Add(new FranchiseRequestModel
                    {
                        FranchiseRequest = item,
                        CompanyFrom = companyFrom != null ? companyFrom.Business : "-",
                        CompanyFor = companyFor != null ? companyFor.Business : "-",
                        User = user != null ? user.Name : "-"
                    });
                }


            }
            else
            {
                var FranchiseRequest = FranchiseRequestServices.Instance.GetFranchiseRequest();
                foreach (var item in FranchiseRequest)
                {
                    var companyFrom = CompanyServices.Instance.GetCompany(item.CompanyIDFrom);
                    var user = UserManager.FindById(item.UserID);
                    listofFranchiseRequestModel.Add(new FranchiseRequestModel
                    {
                        FranchiseRequest = item,
                        CompanyFrom = companyFrom != null ? companyFrom.Business : "-",
                        User = user != null ? user.Name : "-"
                    });


                }
            }
            model.FranchiseRequests = listofFranchiseRequestModel;
            model.GoogleCalendarIntegration = GoogleCalendarServices.Instance.GetGoogleCalendarServicesWRTBusiness(loggedInUser.Company);
            return View(model);
        }


        [HttpGet]
        public ActionResult Action(int ID = 0)
        {
            EmployeeRequestActionViewModel model = new EmployeeRequestActionViewModel();
            var loggedInUser = UserManager.FindById(User.Identity.GetUserId());
            var company = CompanyServices.Instance.GetCompany().Where(x => x.Business == loggedInUser.Company).FirstOrDefault();

            var listofEmployees = new List<Employee>();

            if (ID != 0)
            {
                var employeerequest = EmployeeRequestServices.Instance.GetEmployeeRequest(ID);
                model.ID = employeerequest.ID;
                model.EmployeeID = employeerequest.EmployeeID;
                model.Accepted = employeerequest.Accepted;
                model.Status = employeerequest.Status;
                model.CompanyIDFrom = employeerequest.CompanyIDFrom;
                model.CompanyIDFor = employeerequest.CompanyIDFor;
            }
            return View(model);
        }

        [HttpPost]
        public JsonResult Action(string CompanyCode, int EmployeeID, int ID)
        {
            var loggedInUser = UserManager.FindById(User.Identity.GetUserId());
            var companyFor = CompanyServices.Instance.GetCompany().Where(x => x.Business == loggedInUser.Company).FirstOrDefault();
            var companyFrom = CompanyServices.Instance.GetCompany().Where(x => x.CompanyCode == CompanyCode).FirstOrDefault();

            if (ID == 0)
            {
                var employeeRequest = new EmployeeRequest();
                employeeRequest.Business = loggedInUser.Company;
                employeeRequest.CompanyIDFor = companyFor.ID;
                employeeRequest.CompanyIDFrom = companyFrom.ID;
                employeeRequest.Status = "Requested";
                employeeRequest.EmployeeID = EmployeeID;
                employeeRequest.Accepted = false;
                EmployeeRequestServices.Instance.SaveEmployeeRequest(employeeRequest);

                var history = new History();
                history.Date = DateTime.Now;
                history.Business = loggedInUser.Company;
                history.EmployeeName = loggedInUser.Name;
                history.Note = "New Employee Requested, EmployeeID: " + EmployeeID + "";
                HistoryServices.Instance.SaveHistory(history);
                return Json(new { success = true }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                var employeeRequest = EmployeeRequestServices.Instance.GetEmployeeRequest(ID);
                employeeRequest.Business = loggedInUser.Company;
                employeeRequest.CompanyIDFor = companyFor.ID;
                employeeRequest.CompanyIDFrom = companyFrom.ID;
                employeeRequest.Status = "Requested";
                employeeRequest.EmployeeID = EmployeeID;
                employeeRequest.Accepted = false;
                var history = new History();
                history.Date = DateTime.Now;
                history.Business = loggedInUser.Company;
                history.EmployeeName = loggedInUser.Name;
                history.Note = "Employee Request Updated, EmployeeID: " + EmployeeID + "";
                HistoryServices.Instance.SaveHistory(history);

                EmployeeRequestServices.Instance.UpdateEmployeeRequest(employeeRequest);
                return Json(new { success = true }, JsonRequestBehavior.AllowGet);
            }


        }


        [HttpGet]
        public ActionResult Delete(int ID)
        {
            EmployeeRequestActionViewModel model = new EmployeeRequestActionViewModel();
            var EmployeeRequest = EmployeeRequestServices.Instance.GetEmployeeRequest(ID);
            model.ID = EmployeeRequest.ID;
            return PartialView("_Delete", model);
        }


        [HttpPost]
        public ActionResult Delete(EmployeeRequestActionViewModel model)
        {
            var EmployeeRequest = EmployeeRequestServices.Instance.GetEmployeeRequest(model.ID);
            var loggedInUser = UserManager.FindById(User.Identity.GetUserId());
            var calendarmanages = CalendarManageServices.Instance.GetCalendarManage(loggedInUser.Company);
            foreach (var item in calendarmanages)
            {
                var manageoflist = item.ManageOf.Split(',').Select(x => int.Parse(x.Trim())).ToList();
                if (manageoflist.Contains(EmployeeRequest.EmployeeID))
                {
                    manageoflist.Remove(EmployeeRequest.EmployeeID);
                    item.ManageOf = string.Join(",", manageoflist);
                    CalendarManageServices.Instance.UpdateCalendarManage(item);

                }
            }
            EmployeeRequestServices.Instance.DeleteEmployeeRequest(EmployeeRequest.ID);

            return Json(new { success = true }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult ToggleGoogleCalendarIntegration()
        {
            var loggedInUser = UserManager.FindById(User.Identity.GetUserId());

            var googleCalendar = GoogleCalendarServices.Instance.GetGoogleCalendarServicesWRTBusiness(loggedInUser.Company);
            if (googleCalendar.Disabled)
            {
                googleCalendar.Disabled = false;
            }
            else
            {
                googleCalendar.Disabled = true;


            }

            GoogleCalendarServices.Instance.UpdateGoogleCalendarIntegration(googleCalendar);
            return Json(new { success = true }, JsonRequestBehavior.AllowGet);
        }


        public async Task<ActionResult> CallBack(string code, string error, string state)
        {
            OAuthViewModel model = new OAuthViewModel();
            var googleCalendar = GoogleCalendarServices.Instance.GetGoogleCalendarServicesWRTBusiness(state);
            if (googleCalendar == null)
            {
                if (string.IsNullOrWhiteSpace(error))
                {
                    this.GetTokens(code);
                }
            }
            else if (googleCalendar.RefreshToken == null)
            {
                if (string.IsNullOrWhiteSpace(error))
                {
                    this.GetTokens(code);
                }
            }

            model.GoogleCalendarIntegration = GoogleCalendarServices.Instance.GetGoogleCalendarServicesWRTBusiness(state);
            model.Calendars = await GetCalendars();
            model.Employees = EmployeeServices.Instance.GetEmployeeWRTBusiness(state, true);
            return View("IntegrationSettings", model);
        }

        public async Task<ActionResult> IntegrationSettings()
        {
            OAuthViewModel model = new OAuthViewModel();
            var loggedInUser = UserManager.FindById(User.Identity.GetUserId());
            var googleCalendar = GoogleCalendarServices.Instance.GetGoogleCalendarServicesWRTBusiness(loggedInUser.Company);


            model.GoogleCalendarIntegration = GoogleCalendarServices.Instance.GetGoogleCalendarServicesWRTBusiness(loggedInUser.Company);
            model.Calendars = await GetCalendars();
            model.Employees = EmployeeServices.Instance.GetEmployeeWRTBusiness(loggedInUser.Company, true);
            return View("IntegrationSettings", model);
        }



        public ActionResult GoogleOAuthRedirect()
        {
            var CLIENT_ID = "201633868472-3sf5q4hbiqupcf0smo6auch9bku6bech.apps.googleusercontent.com";
            var REDIRECT_URI = Url.Action("GoogleOAuthCallback", "EmployeeRequest", null, Request.Url.Scheme); // Update with actual callback

            // Define the OAuth consent URL with necessary query parameters
            var authorizationUrl = $"https://accounts.google.com/o/oauth2/v2/auth?client_id={CLIENT_ID}&redirect_uri={REDIRECT_URI}&response_type=code&scope=https://www.googleapis.com/auth/calendar&access_type=offline&prompt=consent";

            // Redirect user to Google's consent screen
            return Redirect(authorizationUrl);
        }

        public ActionResult GoogleOAuthCallback(string code)
        {
            if (string.IsNullOrEmpty(code))
            {
                return Json(new { success = false, message = "Authorization code not found" }, JsonRequestBehavior.AllowGet);
            }

            var CLIENT_ID = "201633868472-3sf5q4hbiqupcf0smo6auch9bku6bech.apps.googleusercontent.com";
            var CLIENT_SECRET = "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";
            var REDIRECT_URI = Url.Action("GoogleOAuthCallback", "YourController", null, Request.Url.Scheme); // Must match redirect URI used in Step 1

            RestClient restClient = new RestClient();
            var uri = new Uri("https://oauth2.googleapis.com/token");
            RestRequest request = new RestRequest(uri, Method.Post);

            // Add parameters to request (authorization code exchange)
            request.AddParameter("client_id", CLIENT_ID);
            request.AddParameter("client_secret", CLIENT_SECRET);
            request.AddParameter("code", code);  // The authorization code you received
            request.AddParameter("grant_type", "authorization_code");
            request.AddParameter("redirect_uri", REDIRECT_URI);

            // Execute the POST request
            var response = restClient.Execute(request);

            // Check if the request was successful
            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                JObject tokens = JObject.Parse(response.Content);

                // Store the access token and refresh token
                var accessToken = tokens["access_token"].ToString();
                var refreshToken = tokens["refresh_token"].ToString();

                // Update the Google Calendar service or store tokens as needed
                var googleCalendar = GoogleCalendarServices.Instance.GetGoogleCalendarServicesWRTBusiness(User.Identity.GetUserId());
                googleCalendar.AccessToken = accessToken;
                googleCalendar.RefreshToken = refreshToken;

                GoogleCalendarServices.Instance.UpdateGoogleCalendarIntegration(googleCalendar);

                return Json(new { success = true, accessToken, refreshToken }, JsonRequestBehavior.AllowGet);
            }

            // If token exchange failed
            return Json(new { success = false, message = response.Content }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public async Task<JsonResult> CreateCalendar(string summary, string description)
        {
            var loggedInUser = UserManager.FindById(User.Identity.GetUserId());
            var googleCalendar = GoogleCalendarServices.Instance.GetGoogleCalendarServicesWRTBusiness(loggedInUser.Company);
            googleCalendar.TypeOfIntegration = "All In One";
            GoogleCalendarServices.Instance.UpdateGoogleCalendarIntegration(googleCalendar);
            var url = "https://www.googleapis.com/calendar/v3/calendars";
            var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", googleCalendar.AccessToken);

            var calendar = new
            {
                summary = summary,
                description = description,
            };

            var json = JsonConvert.SerializeObject(calendar);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await client.PostAsync(url, content);
            var responseData = response.Content.ReadAsStringAsync();
            if (response.IsSuccessStatusCode)
            {
                var employees = EmployeeServices.Instance.GetEmployeeWRTBusiness(loggedInUser.Company);
                var modelofGoogle = JsonConvert.DeserializeObject<GoogleCalendarCreateModel>(responseData.Result);
                foreach (var employee in employees)
                {
                    employee.GoogleCalendarID = modelofGoogle.Id;
                    employee.GoogleCalendarName = modelofGoogle.Summary;
                    EmployeeServices.Instance.UpdateEmployee(employee);

                    using (var newclient = new HttpClient())
                    {
                        newclient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", googleCalendar.AccessToken);

                        var watchRequestBody = new
                        {
                            id = Guid.NewGuid().ToString(), // A unique ID for this channel
                            type = "web_hook",
                            address = $"https://app.yourbookingplatform.com/Booking/TestingGoogleToYBP", // Your webhook endpoint
                            @params = new
                            {
                                ttl = "2147483647" // Max integer value (2^31 - 1 seconds, approx 68 years)
                            }
                        };

                        var newcontent = new StringContent(JsonConvert.SerializeObject(watchRequestBody), Encoding.UTF8, "application/json");
                        var newrequestUrl = $"https://www.googleapis.com/calendar/v3/calendars/{employee.GoogleCalendarID}/events/watch";
                        var newresponse = await newclient.PostAsync(newrequestUrl, newcontent);
                        var responseBody = await newresponse.Content.ReadAsStringAsync();

                        if (newresponse.IsSuccessStatusCode)
                        {
                            var history = new History
                            {
                                Note = "Setup Successfully watched",
                                Business = googleCalendar.Business,
                                Date = DateTime.Now
                            };
                            Channel channel = JsonConvert.DeserializeObject<Channel>(responseBody);

                            employee.WatchChannelID = channel.Id;
                            EmployeeServices.Instance.UpdateEmployee(employee);
                            HistoryServices.Instance.SaveHistory(history);
                        }
                        else
                        {
                            var history = new History
                            {
                                Note = "Failed to set up watch " + responseBody,
                                Business = googleCalendar.Business,
                                Date = DateTime.Now
                            };
                            HistoryServices.Instance.SaveHistory(history);
                        }
                    }

                }
                // Step 2: Make the created calendar public
                using (var aclClient = new HttpClient())
                {
                    aclClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", googleCalendar.AccessToken);

                    var aclBody = new
                    {
                        role = "reader", // Public can view the calendar
                        scope = new
                        {
                            type = "default" // Refers to public
                        }
                    };

                    var aclContent = new StringContent(JsonConvert.SerializeObject(aclBody), Encoding.UTF8, "application/json");
                    var aclUrl = $"https://www.googleapis.com/calendar/v3/calendars/{modelofGoogle.Id}/acl";
                    var aclResponse = await aclClient.PostAsync(aclUrl, aclContent);
                    var aclResponseBody = await aclResponse.Content.ReadAsStringAsync();

                    if (aclResponse.IsSuccessStatusCode)
                    {
                        // Log success for making calendar public
                        var history = new History
                        {
                            Note = "Successfully made calendar public",
                            Business = googleCalendar.Business,
                            Date = DateTime.Now
                        };
                        HistoryServices.Instance.SaveHistory(history);
                    }
                    else
                    {
                        // Log failure
                        var history = new History
                        {
                            Note = "Failed to make calendar public: " + aclResponseBody,
                            Business = googleCalendar.Business,
                            Date = DateTime.Now
                        };
                        HistoryServices.Instance.SaveHistory(history);
                    }
                }
            }
            return Json(new { success = true }, JsonRequestBehavior.AllowGet);
        }

        public string RefreshToken(string Company = "")
        {
            try
            {
                var userId = User.Identity.GetUserId();
                if (userId == null)
                {
                    return "Failed: User not authenticated.";
                }

                var LoggedInUser = UserManager.FindById(userId);
                if (LoggedInUser == null)
                {
                    return "Failed: User not found.";
                }

                Company = LoggedInUser.Company;

                var googleCalendar = GoogleCalendarServices.Instance.GetGoogleCalendarServicesWRTBusiness(Company);
                if (googleCalendar == null)
                {
                    return "Failed: Google Calendar service not found for the company.";
                }

                var url = new System.Uri("https://oauth2.googleapis.com/token");
                var restClient = new RestClient(url);
                var request = new RestRequest();

                var ClientID = "201633868472-3sf5q4hbiqupcf0smo6auch9bku6bech.apps.googleusercontent.com";
                var ClientSecret = "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";
                request.AddQueryParameter("client_id", ClientID);
                request.AddQueryParameter("client_secret", ClientSecret);
                request.AddQueryParameter("grant_type", "refresh_token");
                request.AddQueryParameter("refresh_token", googleCalendar.RefreshToken);

                var response = restClient.Post(request);

                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    var tokenResponse = JsonConvert.DeserializeObject<TokenResponseForDB>(response.Content);
                    if (tokenResponse == null)
                    {
                        return "Failed: Unable to deserialize token response.";
                    }

                    googleCalendar.AccessToken = tokenResponse.AccessToken;
                    GoogleCalendarServices.Instance.UpdateGoogleCalendarIntegration(googleCalendar);

                    return "Refreshed";
                }
                else
                {
                    return "Failed: " + response.Content;
                }
            }
            catch (Exception ex)
            {
                // Log the exception details for further analysis
                // Consider using a logging framework like NLog, Serilog, or log4net
                return "Failed: " + ex.Message + " | StackTrace: " + ex.StackTrace;
            }
        }


        public ActionResult ResetGoogleCalendarIntegration()
        {
            var user = UserManager.FindById(User.Identity.GetUserId());
            var googleCalendar = GoogleCalendarServices.Instance.GetGoogleCalendarServicesWRTBusiness(user.Company);
            var client = new RestClient("https://www.googleapis.com");

            var employees = EmployeeServices.Instance.GetEmployeeWRTBusiness(user.Company).Where(x => x.GoogleCalendarID != null);
            foreach (var item in employees)
            {
                // Prepare the request body
                var requestBody = new
                {
                    id = item.WatchChannelID, // The ID of the channel to stop
                    resourceId = item.GoogleCalendarID // The ID of the watched resource (calendar)
                };

                var request = new RestRequest("/calendar/v3/channels/stop", Method.Post);
                request.AddHeader("Authorization", $"Bearer {googleCalendar.AccessToken}");
                request.AddHeader("Content-Type", "application/json");

                // Add the JSON body to the request
                request.AddJsonBody(requestBody);

                try
                {
                    // Make the POST request to stop the watch
                    var response = client.Execute(request);

                    if (response.IsSuccessStatusCode)
                    {
                        var history = new History();
                        history.Date = DateTime.Now;
                        history.Note = "Watched Stopped Successfully";
                        history.Business = item.Business;
                        history.EmployeeName = item.Name;
                        history.Type = "Settings";
                        HistoryServices.Instance.SaveHistory(history);
                    }
                    else
                    {
                        var history = new History();
                        history.Date = DateTime.Now;
                        history.Note = "Error stopping watch" + response.Content;
                        history.Business = item.Business;
                        history.EmployeeName = item.Name;
                        history.Type = "Settings";
                        HistoryServices.Instance.SaveHistory(history);
                    }
                }
                catch (Exception ex)
                {
                    var history = new History();
                    history.Date = DateTime.Now;
                    history.Note = "Error stopping watch" + ex.Message;
                    history.Business = item.Business;
                    history.EmployeeName = item.Name;
                    history.Type = "Settings";
                    HistoryServices.Instance.SaveHistory(history);
                }
            }

            var appointments = AppointmentServices.Instance.GetAllAppointmentWRTBusiness(user.Company, false, true);
            foreach (var item in appointments)
            {
                AppointmentServices.Instance.DeleteAppointment(item.ID);
            }
            var newapps = AppointmentServices.Instance.GetAllAppointmentWRTBusinessTodaynFuture(user.Company, false, false);
            foreach (var item in newapps)
            {
                item.GoogleCalendarEventID = null;
                AppointmentServices.Instance.UpdateAppointment(item);
            }
            GoogleCalendarServices.Instance.DeleteGoogleCalendarIntegration(googleCalendar.ID);
            return RedirectToAction("Index", "EmployeeRequest");
        }

        public class Channel
        {
            public string Kind { get; set; }
            public string Id { get; set; }
            public string ResourceId { get; set; }
            public string ResourceUri { get; set; }
            public string Token { get; set; }
            public long Expiration { get; set; }
        }

        [HttpPost]
        public async Task<JsonResult> CreateCalendarBulk(List<MultiCalendarModel> listOfCalendar)
        {
            var loggedInUser = UserManager.FindById(User.Identity.GetUserId());
            var googleCalendar = GoogleCalendarServices.Instance.GetGoogleCalendarServicesWRTBusiness(loggedInUser.Company);
            googleCalendar.TypeOfIntegration = "Each Individual";
            GoogleCalendarServices.Instance.UpdateGoogleCalendarIntegration(googleCalendar);
            try
            {
                foreach (var item in listOfCalendar)
                {
                    RefreshToken(loggedInUser.Company);
                    var googleCalendarnew = GoogleCalendarServices.Instance.GetGoogleCalendarServicesWRTBusiness(loggedInUser.Company);
                    var url = "https://www.googleapis.com/calendar/v3/calendars";
                    var client = new HttpClient();
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", googleCalendarnew.AccessToken);

                    var calendar = new
                    {
                        summary = item.summary,
                        description = item.description,
                    };

                    var json = JsonConvert.SerializeObject(calendar);
                    var content = new StringContent(json, Encoding.UTF8, "application/json");
                    var response = await client.PostAsync(url, content);
                    var responseData = await response.Content.ReadAsStringAsync();

                    if (response.IsSuccessStatusCode)
                    {
                        // Deserialize the created calendar response
                        var modelofGoogle = JsonConvert.DeserializeObject<GoogleCalendarCreateModel>(responseData);

                        // Retrieve employee
                        var employee = EmployeeServices.Instance.GetEmployee(item.employeeID);
                        employee.GoogleCalendarID = modelofGoogle.Id;
                        employee.GoogleCalendarName = modelofGoogle.Summary;
                        EmployeeServices.Instance.UpdateEmployee(employee);

                        // Set up watch request (existing logic)
                        using (var newclient = new HttpClient())
                        {
                            newclient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", googleCalendarnew.AccessToken);

                            var watchRequestBody = new
                            {
                                id = Guid.NewGuid().ToString(), // A unique ID for this channel
                                type = "web_hook",
                                address = $"https://app.yourbookingplatform.com/Booking/TestingGoogleToYBP", // Your webhook endpoint
                                @params = new
                                {
                                    ttl = "2147483647" // Max integer value (2^31 - 1 seconds, approx 68 years)
                                }
                            };

                            var newcontent = new StringContent(JsonConvert.SerializeObject(watchRequestBody), Encoding.UTF8, "application/json");
                            var newrequestUrl = $"https://www.googleapis.com/calendar/v3/calendars/{employee.GoogleCalendarID}/events/watch";
                            var newresponse = await newclient.PostAsync(newrequestUrl, newcontent);
                            var responseBody = await newresponse.Content.ReadAsStringAsync();

                            if (newresponse.IsSuccessStatusCode)
                            {
                                var history = new History
                                {
                                    Note = "Setup Successfully watched",
                                    Business = googleCalendarnew.Business,
                                    Date = DateTime.Now
                                };
                                Channel channel = JsonConvert.DeserializeObject<Channel>(responseBody);

                                employee.WatchChannelID = channel.Id;
                                EmployeeServices.Instance.UpdateEmployee(employee);
                                HistoryServices.Instance.SaveHistory(history);
                            }
                            else
                            {
                                var history = new History
                                {
                                    Note = "Failed to set up watch " + responseBody,
                                    Business = googleCalendarnew.Business,
                                    Date = DateTime.Now
                                };
                                HistoryServices.Instance.SaveHistory(history);
                            }
                        }

                        // Step 2: Make the created calendar public
                        using (var aclClient = new HttpClient())
                        {
                            aclClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", googleCalendarnew.AccessToken);

                            var aclBody = new
                            {
                                role = "reader", // Public can view the calendar
                                scope = new
                                {
                                    type = "default" // Refers to public
                                }
                            };

                            var aclContent = new StringContent(JsonConvert.SerializeObject(aclBody), Encoding.UTF8, "application/json");
                            var aclUrl = $"https://www.googleapis.com/calendar/v3/calendars/{employee.GoogleCalendarID}/acl";
                            var aclResponse = await aclClient.PostAsync(aclUrl, aclContent);
                            var aclResponseBody = await aclResponse.Content.ReadAsStringAsync();

                            if (aclResponse.IsSuccessStatusCode)
                            {
                                // Log success for making calendar public
                                var history = new History
                                {
                                    Note = "Successfully made calendar public",
                                    Business = googleCalendarnew.Business,
                                    Date = DateTime.Now
                                };
                                HistoryServices.Instance.SaveHistory(history);
                            }
                            else
                            {
                                // Log failure
                                var history = new History
                                {
                                    Note = "Failed to make calendar public: " + aclResponseBody,
                                    Business = googleCalendarnew.Business,
                                    Date = DateTime.Now
                                };
                                HistoryServices.Instance.SaveHistory(history);
                            }
                        }
                    }
                    else
                    {
                        return Json(new { success = false, Message = responseData }, JsonRequestBehavior.AllowGet);
                    }

                }

                return Json(new { success = true }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {

                return Json(new { success = false, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }

        }
        public async Task<List<GCalendars>> GetCalendars()
        {
            var ListofGCalendar = new List<GCalendars>();

            var loggedInUser = UserManager.FindById(User.Identity.GetUserId());
            var googleCalendar = GoogleCalendarServices.Instance.GetGoogleCalendarServicesWRTBusiness(loggedInUser.Company);

            var CLIENT_ID = "201633868472-3sf5q4hbiqupcf0smo6auch9bku6bech.apps.googleusercontent.com";
            var API_KEY = "AIzaSyASKpY6I08IVKFMw3muX39uMzPc5sBDaSc";
            var clientScret = "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";

            var url = $"https://www.googleapis.com/calendar/v3/users/me/calendarList";
            var builder = new StringBuilder(url);
            var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", googleCalendar.AccessToken);
            var response = await client.GetAsync(builder.ToString());

            if (response.IsSuccessStatusCode)
            {
                var contentString = await response.Content.ReadAsStringAsync();
                CalendarList calendarList = JsonConvert.DeserializeObject<CalendarList>(contentString);
                foreach (var item in calendarList.Items)
                {
                    if (item.AccessRole == "owner")
                    {
                        ListofGCalendar.Add(new GCalendars { id = item.Id, summary = item.Summary });
                    }
                }
            }
            else
            {
                RefreshToken(loggedInUser.Company);

                throw new Exception($"Error: {response.StatusCode} - {await response.Content.ReadAsStringAsync()}");
            }
            return ListofGCalendar;


        }



        public ActionResult GetTokens(string code)
        {
            var loggedInUser = UserManager.FindById(User.Identity.GetUserId());

            var newUrl = new System.Uri("https://oauth2.googleapis.com/revoke");
            RestClient restClient = new RestClient(newUrl);
            RestRequest request = new RestRequest();
            var googleCalendarIntegration = GoogleCalendarServices.Instance.GetGoogleCalendarServicesWRTBusiness(loggedInUser.Company);

            restClient = new RestClient("https://oauth2.googleapis.com/token");
            request = new RestRequest();
            var CLIENT_ID = "201633868472-3sf5q4hbiqupcf0smo6auch9bku6bech.apps.googleusercontent.com";
            var API_KEY = "AIzaSyASKpY6I08IVKFMw3muX39uMzPc5sBDaSc";
            var clientScret = "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";

            request.AddQueryParameter("client_id", CLIENT_ID);
            request.AddQueryParameter("client_secret", clientScret);
            request.AddQueryParameter("code", code);
            request.AddQueryParameter("grant_type", "authorization_code");
            request.AddQueryParameter("redirect_uri", "https://app.yourbookingplatform.com/EmployeeRequest/CallBack");

            var response = restClient.Post(request);
            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                var responseData = response.Content;
                TokenResponse tokenResponse = System.Text.Json.JsonSerializer.Deserialize<TokenResponse>(responseData);
                if (googleCalendarIntegration == null)
                {
                    googleCalendarIntegration = new GoogleCalendarIntegration();
                    googleCalendarIntegration.AccessToken = tokenResponse.AccessToken;
                    googleCalendarIntegration.RefreshToken = tokenResponse.RefreshToken;
                    googleCalendarIntegration.Business = loggedInUser.Company;
                    googleCalendarIntegration.ApiKEY = API_KEY;
                    googleCalendarIntegration.ClientID = CLIENT_ID;
                    googleCalendarIntegration.Disabled = false;
                    GoogleCalendarServices.Instance.SaveGoogleCalendarIntegration(googleCalendarIntegration);
                }
                else
                {
                    googleCalendarIntegration.AccessToken = tokenResponse.AccessToken;
                    googleCalendarIntegration.RefreshToken = tokenResponse.RefreshToken;
                    googleCalendarIntegration.Business = loggedInUser.Company;
                    googleCalendarIntegration.ApiKEY = API_KEY;
                    googleCalendarIntegration.ClientID = CLIENT_ID;
                    googleCalendarIntegration.Disabled = false;
                    GoogleCalendarServices.Instance.UpdateGoogleCalendarIntegration(googleCalendarIntegration);
                }
                return RedirectToAction("Index", "EmployeeRequest");
            }
            else
            {
                return RedirectToAction("Index", "EmployeeRequest");
            }
        }

        [HttpGet]
        public ActionResult AuthRedirect(string Company)
        {
            var CLIENT_ID = "201633868472-3sf5q4hbiqupcf0smo6auch9bku6bech.apps.googleusercontent.com";
            const string REDIRECT_URI = "https://app.yourbookingplatform.com/EmployeeRequest/CallBack";
            var scopes = new List<string>() { "https://www.googleapis.com/auth/calendar", "https://www.googleapis.com/auth/calendar.events" };
            var urlBuilder = new StringBuilder();
            urlBuilder.Append("https://accounts.google.com/o/oauth2/v2/auth?");
            var scopeString = string.Join(" ", scopes.Select(Uri.EscapeDataString));
            urlBuilder.Append("scope=" + scopeString);
            urlBuilder.Append("&access_type=offline");
            urlBuilder.Append("&include_granted_scopes=true");
            urlBuilder.Append("&response_type=code");
            urlBuilder.Append("&state="+ Company);
            urlBuilder.Append("&redirect_uri=" + Uri.EscapeDataString(REDIRECT_URI));
            urlBuilder.Append("&client_id=" + Uri.EscapeDataString(CLIENT_ID));
            urlBuilder.Append("&prompt=consent"); // Force re-consent for refresh token

            var redirectUrl = urlBuilder.ToString();
            return Redirect(redirectUrl);

        }

    }
}