using Microsoft.AspNet.Identity.Owin;
using Microsoft.AspNet.Identity;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;
using TheBookingPlatform.Services;
using TheBookingPlatform.ViewModels;
using TheBookingPlatform.Entities;


namespace TheBookingPlatform.Controllers
{
    public class PayRollController : Controller
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

        public PayRollController()
        {
        }

        public PayRollController(AMUserManager userManager, AMSignInManager signInManager)
        {
            UserManager = userManager;
            SignInManager = signInManager;
        }

        #endregion
        // GET: PayRoll
        public ActionResult Index(int Employee = 0, string Type = "")
        {
            PayRollViewModel model = new PayRollViewModel();
            var LoggedInUser = UserManager.FindById(User.Identity.GetUserId());
            model.Employees = EmployeeServices.Instance.GetEmployeeWRTBusiness(LoggedInUser.Company, true);
            var company = CompanyServices.Instance.GetCompany().Where(X => X.Business == LoggedInUser.Company).FirstOrDefault();
            model.Statuses = company.StatusForPayroll.Split(',').ToList();
            model.EmployeeID = Employee;
            model.Type = Type;
            model.Employee = EmployeeServices.Instance.GetEmployee(Employee);
            model.StartDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            model.EndDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.DaysInMonth(DateTime.Now.Year, DateTime.Now.Month));



            if (User.IsInRole("Calendar"))
            {
                model.EndDate = DateTime.Now;
                var employee = EmployeeServices.Instance.GetEmployeeWithLinkedUserID(LoggedInUser.Id);
                model.Employee = employee;
                var appointments = AppointmentServices.Instance.GetAllAppointmentWRTBusiness(LoggedInUser.Company, false, model.Employee.ID, model.StartDate, model.EndDate, false).Where(x => x.Color != "darkgray").OrderBy(x => x.Time.TimeOfDay).ToList();
                var serviceIDList = new List<int>();
                var appointmentIDs = new List<int>();
                float TotalAmount = 0;
                float TimeSpend = 0;
                int TotalDurations = 0;
                double TotalPrice = 0;
                var TotalOnlinePriceChange = 0.0;
                var OfflineDiscountCost = 0.0;
                var ServicePrice = 0.0;
                var groupedByDate = appointments
              .GroupBy(a => a.Date.Date);
                foreach (var item in groupedByDate)
                {
                    var firstAppointmentTime = item.OrderBy(X => X.Time.TimeOfDay).Select(x => x.Time).FirstOrDefault();
                    var lastAppointmentEndTime = item.OrderBy(X => X.Time.TimeOfDay).Select(x => x.EndTime).LastOrDefault();
                    TimeSpan duration = lastAppointmentEndTime.TimeOfDay - firstAppointmentTime.TimeOfDay;
                    TimeSpend += float.Parse(duration.TotalMinutes.ToString());
                }
                foreach (var item in appointments)
                {
                    if (item.Status != null)
                    {
                        var statusesInAppointments = item.Status.Split(',').Select(x => x.Trim()).ToList();
                        var statuses = company.StatusForPayroll;
                        if (statusesInAppointments.Any(status => statuses.Contains(status)))
                        {
                            var servicesInAppointments = item.Service.Split(',').Select(x => int.Parse(x)).ToList();

                            foreach (var service in servicesInAppointments)
                            {
                                var PriceChange = PriceChangeServices.Instance.GetPriceChange(item.OnlinePriceChange);

                                var price = ServiceServices.Instance.GetService(service).Price;
                                ServicePrice += price;
                                if (PriceChange != null)
                                {
                                    if (PriceChange.TypeOfChange == "Price Increase")
                                    {
                                        var PriceChangeServiced = price * (PriceChange.Percentage / 100);
                                        TotalOnlinePriceChange += PriceChangeServiced;

                                    }
                                    else
                                    {
                                        var PriceChangeServiced = price * (PriceChange.Percentage / 100);
                                        TotalOnlinePriceChange += PriceChangeServiced;



                                    }
                                }

                            }

                            var servicesplit = item.ServiceDiscount.Split(',').Select(x => float.Parse(x)).ToList();
                            for (int i = 0; i < servicesplit.Count; i++)
                            {
                                var service = ServiceServices.Instance.GetService(servicesInAppointments[i]);
                                var serviceName = service.Name;
                                float discount = servicesplit[i];
                                var finalDiscount = discount / 100;

                                // Your calculations based on serviceId and discount here
                                var offlineDiscountValue = service.Price * finalDiscount;
                                OfflineDiscountCost += offlineDiscountValue;
                            }

                            appointmentIDs.Add(item.ID);

                            var servicedurations = item.ServiceDuration.Split(',').Select(x => x.Trim()).ToList();
                            foreach (var duration in servicedurations)
                            {
                                TotalDurations += ExtractNumberFromString(duration);
                            }
                        }
                    }
                    else
                    {

                        appointmentIDs.Add(item.ID);

                        var servicedurations = item.ServiceDuration.Split(',').Select(x => x.Trim()).ToList();
                        foreach (var duration in servicedurations)
                        {
                            TotalDurations += ExtractNumberFromString(duration);
                        }

                    }
                }
                var invoices = InvoiceServices.Instance.GetInvoice().Where(x => appointmentIDs.Contains(x.AppointmentID)).ToList();
                TotalAmount = invoices.Select(x => x.GrandTotal).Sum();
                float Amount = 0;
                string FinalAmount = "";
                if (employee.Type == "Percentage")
                {
                    TotalPrice = (ServicePrice - OfflineDiscountCost - TotalOnlinePriceChange);
                    Amount = float.Parse(Math.Round((TotalPrice * employee.Percentage) / 100, 2).ToString());
                    FinalAmount = Math.Round(Amount * employee.Percentage, 2) + company.Currency;
                    model.Amount = Amount;
                    model.FinalAmount = FinalAmount;

                }
                else if (employee.Type == "Worked Hours")
                {
                    Amount = float.Parse(Math.Round(TotalDurations / 60.0, 2).ToString());
                    FinalAmount = Math.Round(Amount * employee.Percentage, 2) + company.Currency;
                    model.Amount = Amount;
                    model.FinalAmount = FinalAmount;

                }
                else if (employee.Type == "Time to Time")
                {
                    Amount = float.Parse(Math.Round(TimeSpend / 60.0, 2).ToString());
                    FinalAmount = Math.Round(Amount * employee.Percentage,2) + company.Currency;
                    model.Amount = Amount;
                    model.FinalAmount = FinalAmount;

                }

            }
            return View(model);
        }


        static int ExtractNumberFromString(string input)
        {
            string pattern = @"\d+";
            Match match = Regex.Match(input, pattern);

            if (match.Success)
            {
                return int.Parse(match.Value);
            }
            else
            {
                throw new FormatException("No number found in the input string.");
            }
        }

        [HttpGet]
        public JsonResult GetPayRollData(string requestData)
        {
            var LoggedInUser = UserManager.FindById(User.Identity.GetUserId());
            var model = JsonConvert.DeserializeObject<PayRollModel>(requestData);
            var company = CompanyServices.Instance.GetCompany().Where(X => X.Business == LoggedInUser.Company).FirstOrDefault();
            var employee = EmployeeServices.Instance.GetEmployee(model.EmployeeID);
            var appointments = new List<Appointment>();
            var serviceIDList = new List<int>();
            var appointmentIDs = new List<int>();
            float TotalAmount = 0;
            float TimeSpend = 0;
            int TotalDurations = 0;
            double TotalPrice = 0;
            var TotalOnlinePriceChange = 0.0;
            var OfflineDiscountCost = 0.0;
            var ServicePrice = 0.0;
            if (employee.Type == "Time to Time")
            {
                appointments = AppointmentServices.Instance.GetAllAppointmentWRTBusiness(LoggedInUser.Company, false, model.EmployeeID, model.StartDate, model.EndDate, model.isCancelled).Where(x => x.Color != "darkgray" && model.Status.Contains(x.Status.Trim())).OrderBy(X=>X.Date.Day).ThenBy(x => x.Time.TimeOfDay).ToList();
                var groupedByDate = appointments
       .GroupBy(a => a.Date.Date);
                foreach (var item in groupedByDate)
                {
                    var firstAppointmentTime = item.OrderBy(X => X.Time.TimeOfDay).Select(x => x.Time).FirstOrDefault();
                    var lastAppointmentEndTime = item.OrderBy(X => X.Time.TimeOfDay).Select(x => x.EndTime).LastOrDefault();
                    TimeSpan duration = lastAppointmentEndTime.TimeOfDay - firstAppointmentTime.TimeOfDay;
                    TimeSpend += float.Parse(duration.TotalMinutes.ToString());
                }
            }
            else
            {
                appointments = AppointmentServices.Instance.GetAllAppointmentWRTBusiness(LoggedInUser.Company, false, model.EmployeeID, model.StartDate, model.EndDate, model.isCancelled).Where(x => x.Color != "darkgray" && model.Status.Contains(x.Status)).OrderBy(x => x.Time.TimeOfDay).ToList();
                foreach (var item in appointments)
                {
                    if (item.Status != null)
                    {
                        var statusesInAppointments = item.Status.Split(',').Select(x => x.Trim()).ToList();
                        var statuses = model.Status;
                        if (statusesInAppointments.Any(status => statuses.Contains(status)))
                        {
                            var servicesInAppointments = item.Service.Split(',').Select(x => int.Parse(x)).ToList();

                            foreach (var service in servicesInAppointments)
                            {
                                var PriceChange = PriceChangeServices.Instance.GetPriceChange(item.OnlinePriceChange);

                                var price = ServiceServices.Instance.GetService(service).Price;
                                ServicePrice += price;
                                if (PriceChange != null)
                                {
                                    if (PriceChange.TypeOfChange == "Price Increase")
                                    {
                                        var PriceChangeServiced = price * (PriceChange.Percentage / 100);
                                        TotalOnlinePriceChange += PriceChangeServiced;

                                    }
                                    else
                                    {
                                        var PriceChangeServiced = price * (PriceChange.Percentage / 100);
                                        TotalOnlinePriceChange += PriceChangeServiced;



                                    }
                                }

                            }

                            var servicesplit = item.ServiceDiscount.Split(',').Select(x => float.Parse(x)).ToList();
                            for (int i = 0; i < servicesplit.Count; i++)
                            {

                                var service = ServiceServices.Instance.GetService(servicesInAppointments[i]);
                                var serviceName = service.Name;
                                float discount = servicesplit[i];
                                var finalDiscount = discount / 100;

                                // Your calculations based on serviceId and discount here
                                var offlineDiscountValue = service.Price * finalDiscount;
                                OfflineDiscountCost += offlineDiscountValue;
                            }

                            appointmentIDs.Add(item.ID);

                            var servicedurations = item.ServiceDuration.Split(',').Select(x => x.Trim()).ToList();
                            foreach (var duration in servicedurations)
                            {
                                TotalDurations += ExtractNumberFromString(duration);
                            }
                        }
                    }
                    else
                    {

                        appointmentIDs.Add(item.ID);

                        var servicedurations = item.ServiceDuration.Split(',').Select(x => x.Trim()).ToList();
                        foreach (var duration in servicedurations)
                        {
                            TotalDurations += ExtractNumberFromString(duration);
                        }

                    }
                }

            }



            var invoices = InvoiceServices.Instance.GetInvoices(LoggedInUser.Company, appointmentIDs);
            TotalAmount = invoices.Select(x => x.GrandTotal).Sum();
            float Amount = 0;
            string FinalAmount = "";
            if (employee.Type == "Percentage")
            {
                TotalPrice = (ServicePrice - OfflineDiscountCost - TotalOnlinePriceChange);
                Amount = float.Parse(Math.Round((TotalPrice * model.Percentage) / 100, 2).ToString());
                FinalAmount = Math.Round(Amount * employee.Percentage, 2) + company.Currency;

            }
            else if (employee.Type == "Worked Hours")
            {
                Amount = float.Parse(Math.Round(TotalDurations / 60.0, 2).ToString());
                FinalAmount = Math.Round(Amount * employee.Percentage, 2) + company.Currency;
            }
            else if (employee.Type == "Time to Time")
            {
                Amount = float.Parse(Math.Round(TimeSpend / 60.0, 2).ToString());
                FinalAmount = Math.Round(Amount * employee.Percentage, 2) + company.Currency;
            }
            model.Company = company;
            model.Amount = Amount;
            model.Employee = employee;
            model.FinalAmount = FinalAmount;
            
          
            return Json(new { success = true,Company=company, Amount = Amount, Employee = employee,Percentage= model.Percentage,FinalAmount=FinalAmount,StartDate=model.StartDate.ToString("yyyy-MM-dd"),EndDate=model.EndDate.ToString("yyyy-MM-dd") }, JsonRequestBehavior.AllowGet);
            // Generate the URL for the view

            // Return the URL as a JSON response
        }

        public ActionResult ViewEmployeesPayRoll()
        {
            var LoggedInUser = UserManager.FindById(User.Identity.GetUserId()); 
            var company = CompanyServices.Instance.GetCompany().Where(X => X.Business == LoggedInUser.Company).FirstOrDefault();
            var StartDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            var EndDate = DateTime.Now;
            var employee = new Employee();
            employee = EmployeeServices.Instance.GetEmployeeWithLinkedUserID(LoggedInUser.Id);
            if(employee == null)
            {
                var employeeRequest = EmployeeRequestServices.Instance.GetEmployeeRequestByBusiness(company.ID);
                foreach (var item in employeeRequest)
                {
                    if (item.Accepted)
                    {
                        employee = EmployeeServices.Instance.GetEmployee(item.EmployeeID);
                    }
                }
            }
            var appointments = new List<Appointment>();
            var serviceIDList = new List<int>();
            var appointmentIDs = new List<int>();
            float TotalAmount = 0;
            float TimeSpend = 0;
            int TotalDurations = 0;
            double TotalPrice = 0;
            var TotalOnlinePriceChange = 0.0;
            var OfflineDiscountCost = 0.0;
            var ServicePrice = 0.0;
            if (employee.Type == "Time to Time")
            {
                appointments = AppointmentServices.Instance.GetAllAppointmentWRTBusiness(LoggedInUser.Company, false, employee.ID, StartDate, EndDate, false).Where(x => x.Color != "darkgray" && company.StatusForPayroll.Split(',').ToList().Contains(x.Status.Trim())).OrderBy(x => x.Time.TimeOfDay).ToList();
                var groupedByDate = appointments
   .GroupBy(a => a.Date.Date);


                foreach (var item in groupedByDate)
                {
                    var firstAppointmentTime = item.OrderBy(X => X.Time.TimeOfDay).Select(x => x.Time).FirstOrDefault();
                    var lastAppointmentEndTime = item.OrderBy(X => X.Time.TimeOfDay).Select(x => x.EndTime).LastOrDefault();
                    TimeSpan duration = lastAppointmentEndTime.TimeOfDay - firstAppointmentTime.TimeOfDay;
                    TimeSpend += float.Parse(duration.TotalMinutes.ToString());
                }
            }
            else
            {


                appointments = AppointmentServices.Instance.GetAllAppointmentWRTBusiness(LoggedInUser.Company, false, employee.ID, StartDate, EndDate, false).Where(x => x.Color != "darkgray" && company.StatusForPayroll.Split(',').ToList().Contains(x.Status)).OrderBy(x => x.Time.TimeOfDay).ToList();
                foreach (var item in appointments)
                {
                    if (item.Status != null)
                    {
                        var statusesInAppointments = item.Status.Split(',').Select(x => x.Trim()).ToList();
                        var statuses = company.StatusForPayroll;
                        if (statusesInAppointments.Any(status => statuses.Contains(status)))
                        {
                            var servicesInAppointments = item.Service.Split(',').Select(x => int.Parse(x)).ToList();

                            foreach (var service in servicesInAppointments)
                            {
                                var PriceChange = PriceChangeServices.Instance.GetPriceChange(item.OnlinePriceChange);

                                var price = ServiceServices.Instance.GetService(service).Price;
                                ServicePrice += price;
                                if (PriceChange != null)
                                {
                                    if (PriceChange.TypeOfChange == "Price Increase")
                                    {
                                        var PriceChangeServiced = price * (PriceChange.Percentage / 100);
                                        TotalOnlinePriceChange += PriceChangeServiced;

                                    }
                                    else
                                    {
                                        var PriceChangeServiced = price * (PriceChange.Percentage / 100);
                                        TotalOnlinePriceChange += PriceChangeServiced;



                                    }
                                }

                            }

                            var servicesplit = item.ServiceDiscount.Split(',').Select(x => float.Parse(x)).ToList();
                            for (int i = 0; i < servicesplit.Count; i++)
                            {
                                var service = ServiceServices.Instance.GetService(servicesInAppointments[i]);
                                var serviceName = service.Name;
                                float discount = servicesplit[i];
                                var finalDiscount = discount / 100;

                                // Your calculations based on serviceId and discount here
                                var offlineDiscountValue = service.Price * finalDiscount;
                                OfflineDiscountCost += offlineDiscountValue;
                            }

                            appointmentIDs.Add(item.ID);

                            var servicedurations = item.ServiceDuration.Split(',').Select(x => x.Trim()).ToList();
                            foreach (var duration in servicedurations)
                            {
                                TotalDurations += ExtractNumberFromString(duration);
                            }
                        }
                    }
                    else
                    {

                        appointmentIDs.Add(item.ID);

                        var servicedurations = item.ServiceDuration.Split(',').Select(x => x.Trim()).ToList();
                        foreach (var duration in servicedurations)
                        {
                            TotalDurations += ExtractNumberFromString(duration);
                        }

                    }
                }

            }

            var invoices = InvoiceServices.Instance.GetInvoices(LoggedInUser.Company, appointmentIDs);
            TotalAmount = invoices.Select(x => x.GrandTotal).Sum();
            float Amount = 0;
            string FinalAmount = "";
            if (employee.Type == "Percentage")
            {
                TotalPrice = (ServicePrice - OfflineDiscountCost - TotalOnlinePriceChange);
                Amount = float.Parse(Math.Round((TotalPrice * employee.Percentage) / 100, 2).ToString());
                FinalAmount = Math.Round(Amount * employee.Percentage, 2) + company.Currency;

            }
            else if (employee.Type == "Worked Hours")
            {
                Amount = float.Parse(Math.Round(TotalDurations / 60.0, 2).ToString());
                FinalAmount = Math.Round(Amount * employee.Percentage, 2) + company.Currency;

            }
            else if (employee.Type == "Time to Time")
            {
                Amount = float.Parse(Math.Round(TimeSpend / 60.0, 2).ToString());
                FinalAmount = Math.Round(Amount * employee.Percentage, 2) + company.Currency;

            }

            PayRollViewModel model = new PayRollViewModel();
            model.EmployeeName = employee.Name;
            model.EmployeeSpecialization = employee.Specialization;
            model.Amount = Amount;
            model.FinalAmount = FinalAmount;
            model.Type = employee.Type;
            model.CompanyName = company.Business;
            model.CompanyPhoneNumber = company.PhoneNumber;
            model.CompanyAddress = company.Address;
            model.CompanyLogo = company.Logo;
            model.Percentage = employee.Percentage;
            model.Company = company;
            model.VStartDate = StartDate.ToString("yyyy-MM-dd");
            model.VEndDate = EndDate.ToString("yyyy-MM-dd");
            return View("ViewPayRoll", model);
        }


        public ActionResult ViewPayRoll(string employeeName, string employeeSpecialization, float amount, string finalAmount, float percentage, string companyName, string companyPhoneNumber, string companyAddress,string companyLogo,string StartDate,string EndDate,string Type)
        {
            PayRollViewModel model = new PayRollViewModel();
            model.EmployeeName = employeeName;
            model.EmployeeSpecialization = employeeSpecialization;
            model.Amount = amount;
            model.FinalAmount = finalAmount;
            model.Type = Type;
            model.CompanyName = companyName;
            model.CompanyPhoneNumber = companyPhoneNumber;
            model.CompanyAddress = companyAddress;
            model.CompanyLogo = companyLogo;
            model.Percentage = percentage;
            model.Company = CompanyServices.Instance.GetCompany(companyName).FirstOrDefault();
            model.VStartDate = StartDate;
            model.VEndDate = EndDate;
            return View("ViewPayRoll", model);
        }

    }
}