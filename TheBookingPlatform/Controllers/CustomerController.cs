using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using OfficeOpenXml;
using Stripe;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TheBookingPlatform.Entities;
using TheBookingPlatform.Services;
using TheBookingPlatform.ViewModels;

namespace TheBookingPlatform.Controllers
{
    public class CustomerController : Controller
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

        public CustomerController()
        {
        }

        public CustomerController(AMUserManager userManager, AMSignInManager signInManager)
        {
            UserManager = userManager;
            SignInManager = signInManager;
        }

        #endregion
        // GET: Customer
        [NoCache]
        public ActionResult Index(string SearchTerm = "")
        {
            CustomerListingViewModel model = new CustomerListingViewModel();
            var customerModel = new List<CustomerModel>();
            model.SearchTerm = SearchTerm;
            var LoggedInUser = UserManager.FindById(User.Identity.GetUserId()); if (LoggedInUser == null) { return RedirectToAction("Login", "Account"); }
            if (LoggedInUser.Role != "Super Admin")
            {
                var customers = CustomerServices.Instance.GetCustomersWRTBusiness(LoggedInUser.Company, SearchTerm);
                foreach (var item in customers)
                {
                    customerModel.Add(new CustomerModel {IsBlocked=item.IsBlocked, ID = item.ID, Email = item.Email, FirstName = item.FirstName, LastName = item.LastName, MobileNumber = item.MobileNumber });
                }
            }
            else
            {
                var customers = CustomerServices.Instance.GetCustomer(SearchTerm);
                foreach (var item in customers)
                {
                    customerModel.Add(new CustomerModel { IsBlocked = item.IsBlocked, ID = item.ID, Email = item.Email, FirstName = item.FirstName, LastName = item.LastName, MobileNumber = item.MobileNumber });
                }
            }
            model.Customers = customerModel;
            return View(model);
        }

        [HttpPost]
        public ActionResult ChangeBlockStatus(int ID)
        {
            var Customer = CustomerServices.Instance.GetCustomer(ID);
            if (Customer.IsBlocked == false)
            {
                Customer.IsBlocked = true;
                CustomerServices.Instance.UpdateCustomer(Customer);
            }
            else
            {
                Customer.IsBlocked = false;
                CustomerServices.Instance.UpdateCustomer(Customer);
            }
            return Json(new { success = true });

        }

        [HttpPost]
        public JsonResult GetReferralBalance(int CustomerID)
        {
            var customer = CustomerServices.Instance.GetCustomer(CustomerID);
            float ReferralBalance = 0;
            if (customer != null)
            {
                ReferralBalance = customer.ReferralBalance;
            }
            return Json(new { success = true, ReferralBalance = ReferralBalance }, JsonRequestBehavior.AllowGet);
        }



        [HttpGet]
        public ActionResult CheckStatusByEmail(string CustomerEmail)
        {
            var customer = CustomerServices.Instance.GetCustomer().Where(x => x.Email.Trim() == CustomerEmail.Trim()).FirstOrDefault();
            if (customer != null)
            {
                var Customer = CustomerServices.Instance.GetCustomer().Where(X => X.Email.Trim().ToLower() == CustomerEmail.Trim().ToLower()).FirstOrDefault();
                return Json(new { success = true, IsBlocked = Customer.IsBlocked }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(new { success = true, IsBlocked = false }, JsonRequestBehavior.AllowGet); ;

            }
        }


        [HttpGet]
        public ActionResult CustomerProfile(int ID)
        {
            var LoggedInUser = UserManager.FindById(User.Identity.GetUserId());
            AppointmentDetailsViewModel model = new AppointmentDetailsViewModel();
            model.Customer = CustomerServices.Instance.GetCustomer(ID);
            model.Files = FileServices.Instance.GetFileWRTBusiness(LoggedInUser.Company, ID);
            model.Histories = HistoryServices.Instance.GetHistoriesWRTBusinessandCustomer(LoggedInUser.Company, model.Customer.FirstName + " " + model.Customer.LastName);
            model.Messages = MessageServices.Instance.GetMessage().Where(x => x.CustomerID == ID && x.Business == LoggedInUser.Company).ToList();

            var AppointmentsListOfCustoemrs = new List<AppointmentModel>();
            var appointmentsforCustomer = AppointmentServices.Instance.GetAllAppointmentWRTBusiness(LoggedInUser.Company, false, ID);
            foreach (var item in appointmentsforCustomer)
            {

                var employee2 = EmployeeServices.Instance.GetEmployee(item.EmployeeID);

                var customer2 = CustomerServices.Instance.GetCustomer(item.CustomerID);
                var serviceList2 = new List<ServiceModelForCustomerProfile>();
                if (item.Service != null)
                {
                    var ServiceListCommand2 = item.Service.Split(',').ToList();
                    var ServiceDuration2 = item.ServiceDuration.Split(',').ToList();
                    var ServiceDiscount = item.ServiceDiscount.Split(',').ToList();

                    for (int i = 0; i < ServiceListCommand2.Count && i < ServiceDuration2.Count; i++)
                    {
                        var Service2 = ServiceServices.Instance.GetService(int.Parse(ServiceListCommand2[i]));
                        if (Service2 != null)
                        {
                            var serviceViewModel2 = new ServiceModelForCustomerProfile
                            {

                                Name = Service2.Name,
                                Duration = ServiceDuration2[i],
                                Price = Service2.Price,
                                Discount = float.Parse(ServiceDiscount[i])

                            };
                            serviceList2.Add(serviceViewModel2);
                        }

                    }
                }
                if (customer2 == null)
                {

                    AppointmentsListOfCustoemrs.Add(new AppointmentModel {IsCancelled = item.IsCancelled, Date = item.Date, Time = item.Time, Status = item.Status, AppointmentEndTime = item.EndTime, ID = item.ID, EmployeeName = employee2.Name, Customer = null, Services = serviceList2 });
                }
                else
                {

                    AppointmentsListOfCustoemrs.Add(new AppointmentModel { IsCancelled = item.IsCancelled, Date = item.Date, Time = item.Time, Status = item.Status, AppointmentEndTime = item.EndTime, ID = item.ID, EmployeeName = employee2.Name, Customer = customer2, Services = serviceList2 });

                }
            }
            model.TotalAppointmentsCustomer = AppointmentsListOfCustoemrs;


            var Customer = CustomerServices.Instance.GetCustomer(ID);
            var listOfLoyaltyCardDetails = new List<History>();
            var LCAssigned = LoyaltyCardServices.Instance.GetLoyaltyCardAssignmentByCustomerID(Customer.ID);
            var historiesofLoyaltyCard = HistoryServices.Instance.GetHistoriesWRTBusinessandCustomer(LoggedInUser.Company, Customer.FirstName + " " + Customer.LastName);
            foreach (var item in historiesofLoyaltyCard)
            {
                if (item.Type.Contains("LoyaltyCard"))
                {
                    listOfLoyaltyCardDetails.Add(item);
                }
            }
            model.LoyaltyCardHistories = historiesofLoyaltyCard;

            return View(model);
        }



        [HttpGet]
        [NoCache]
        public ActionResult Action(int ID = 0)
        {
            CustomerActionViewModel model = new CustomerActionViewModel();
            if (ID != 0)
            {
                var Customer = CustomerServices.Instance.GetCustomer(ID);
                model.ID = Customer.ID;
                model.FirstName = Customer.FirstName;
                model.LastName = Customer.LastName;
                model.Gender = Customer.Gender;
                model.DateOfBirth = Customer.DateOfBirth;
                model.Email = Customer.Email;
                model.MobileNumber = Customer.MobileNumber;
                model.Address = Customer.Address;
                model.PostalCode = Customer.PostalCode;
                model.City = Customer.City;
                model.ProfilePicture = Customer.ProfilePicture;

                model.HaveNewsLetter = Customer.HaveNewsLetter;
                model.AdditionalInformation = Customer.AdditionalInformation;
                model.AdditionalInvoiceInformation = Customer.AdditionalInvoiceInformation;
                model.WarningInformation = Customer.WarningInformation;
            }
            return View(model);
        }

        public JsonResult GetCustomers(string SearchTerm = "")
        {
            var LoggedInUser = UserManager.FindById(User.Identity.GetUserId());
            var customerModel = new List<CustomerModel>();
            if (LoggedInUser.Role != "Super Admin")
            {
                var customers = CustomerServices.Instance.GetCustomersWRTBusiness(LoggedInUser.Company, SearchTerm);
                foreach (var item in customers)
                {
                    customerModel.Add(new CustomerModel { ID = item.ID, Email = item.Email, FirstName = item.FirstName, LastName = item.LastName, MobileNumber = item.MobileNumber });
                }
            }
            else
            {
                var customers = CustomerServices.Instance.GetCustomer(SearchTerm);
                foreach (var item in customers)
                {
                    customerModel.Add(new CustomerModel { ID = item.ID, Email = item.Email, FirstName = item.FirstName, LastName = item.LastName, MobileNumber = item.MobileNumber });
                }
            }
           
            // Return data as JSON
            return Json(customerModel, JsonRequestBehavior.AllowGet);
        }



        private bool IsValidDate(DateTime date)
        {
            
            if (date.Year >= 1900)
            {
                return true;
            }
            else
            {
                return false;   
            }
        }

        public ActionResult Export()
        {
            var LoggedInUser = UserManager.FindById(User.Identity.GetUserId()); if (LoggedInUser == null) { return RedirectToAction("Login", "Account"); }
            var customers = new List<Entities.Customer>();
            if (LoggedInUser.Role != "Super Admin")
            {
                customers = CustomerServices.Instance.GetCustomersWRTBusiness(LoggedInUser.Company, "");
            }
            else
            {
                customers = CustomerServices.Instance.GetCustomer();
            }

            // Create a DataTable and populate it with the site data
            System.Data.DataTable tableData = new System.Data.DataTable();
            tableData.Columns.Add("ID", typeof(int)); // Replace "Column1" with the actual column name
            tableData.Columns.Add("FirstName",typeof(string));
            tableData.Columns.Add("LastName",typeof(string));
            tableData.Columns.Add("Gender",typeof(string));
            tableData.Columns.Add("DateOfBirth",typeof(string));
            tableData.Columns.Add("Email",typeof(string));
            tableData.Columns.Add("PhoneNumber",typeof(string));
            tableData.Columns.Add("MobileNumber",typeof(string));
            tableData.Columns.Add("Address",typeof(string));
            tableData.Columns.Add("PostalCode",typeof(int));
            tableData.Columns.Add("City",typeof(string));
            tableData.Columns.Add("AdditionalInformation",typeof(string));
            tableData.Columns.Add("AdditionalInvoiceInformation",typeof(string));
            tableData.Columns.Add("WarningInformation", typeof(string));
           tableData.Columns.Add("HaveNewsLetter", typeof(string)); 



            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            foreach (var customer in customers)
            {
                DataRow row = tableData.NewRow();

                row["ID"] = customer.ID;
                row["FirstName"] = customer.FirstName;
                row["LastName"] = customer.LastName;
                row["Gender"] = customer.Gender;
                if (customer.DateOfBirth.HasValue && IsValidDate(customer.DateOfBirth.Value))
                {
                    row["DateOfBirth"] = customer.DateOfBirth.Value.ToString("yyyy-MM-dd");
                }
                else
                {
                    row["DateOfBirth"] = DBNull.Value;
                }
                row["Email"] = customer.Email;
                row["MobileNumber"] = customer.MobileNumber;
                row["Address"] = customer.Address;
                row["PostalCode"] = customer.PostalCode;
                row["City"] = customer.City;
                row["AdditionalInformation"] = customer.AdditionalInformation;
                row["AdditionalInvoiceInformation"] = customer.AdditionalInvoiceInformation;
                row["WarningInformation"] = customer.WarningInformation;
                row["HaveNewsLetter"] = customer.HaveNewsLetter.ToString();
                tableData.Rows.Add(row);
            }

            // Create the Excel package
            using (ExcelPackage package = new ExcelPackage())
            {
                // Create a new worksheet
                ExcelWorksheet worksheet = package.Workbook.Worksheets.Add("Customers");

                // Set the column names
                for (int i = 0; i < tableData.Columns.Count; i++)
                {
                    worksheet.Cells[1, i + 1].Value = tableData.Columns[i].ColumnName;
                }

                // Set the row data
                for (int row = 0; row < tableData.Rows.Count; row++)
                {
                    for (int col = 0; col < tableData.Columns.Count; col++)
                    {
                        worksheet.Cells[row + 2, col + 1].Value = tableData.Rows[row][col];
                    }
                }

                // Auto-fit columns for better readability
                worksheet.Cells.AutoFitColumns();

                // Convert the Excel package to a byte array
                byte[] excelBytes = package.GetAsByteArray();

                // Return the Excel file as a downloadable file
                return File(excelBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Customers.xlsx");
            }
        }



        [HttpPost]
        public ActionResult Action(CustomerActionViewModel model)
        {
            var LoggedInUser = UserManager.FindById(User.Identity.GetUserId()); if (LoggedInUser == null) { return RedirectToAction("Login", "Account"); }
            if (model.ID != 0)
            {
                
                var customer = CustomerServices.Instance.GetCustomer(model.ID);
                var oldName = customer.FirstName + " " + customer.LastName;
                customer.ID = model.ID;
                customer.FirstName = model.FirstName;
                customer.LastName = model.LastName;
                customer.Gender = model.Gender;
                customer.DateOfBirth = model.DateOfBirth;
                customer.Email = model.Email;
                customer.MobileNumber = model.MobileNumber;
                customer.Address = model.Address;
                customer.PostalCode = model.PostalCode;
                customer.City = model.City;
                customer.ProfilePicture = model.ProfilePicture;
                customer.Business = LoggedInUser.Company;
                customer.AdditionalInformation = model.AdditionalInformation;
                customer.AdditionalInvoiceInformation = model.AdditionalInvoiceInformation;
                customer.WarningInformation = model.WarningInformation;
                customer.HaveNewsLetter = model.HaveNewsLetter;
                CustomerServices.Instance.UpdateCustomer(customer);


                var history = new History();
                history.Business = LoggedInUser.Company;
                history.CustomerName = customer.FirstName +" "+customer.LastName;
                history.Date = DateTime.Now;
                history.Note = oldName + " Updated to "+history.CustomerName;
                history.EmployeeName = "";
                HistoryServices.Instance.SaveHistory(history);


                foreach (var item in HistoryServices.Instance.GetHistoriesByCustomer(oldName,LoggedInUser.Company))
                {
                    item.CustomerName = customer.FirstName + " " + customer.LastName;
                    HistoryServices.Instance.UpdateHistory(item);
                }
                return Json(new { success = true }, JsonRequestBehavior.AllowGet);

            }
            else
            {
                if (model.Email != null)
                {
                    if (CustomerServices.Instance.GetCustomerWRTBusiness(LoggedInUser.Company, model.Email.ToLower().Trim()) == null)
                    {
                        var customer = new Entities.Customer();
                        customer.FirstName = model.FirstName;
                        customer.LastName = model.LastName;
                        customer.Gender = model.Gender;
                        customer.DateOfBirth = model.DateOfBirth;
                        customer.Email = model.Email;
                        customer.MobileNumber = model.MobileNumber;
                        customer.Address = model.Address;
                        customer.PostalCode = model.PostalCode;
                        customer.City = model.City;
                        customer.Business = LoggedInUser.Company;
                        customer.ProfilePicture = model.ProfilePicture;
                        customer.AdditionalInformation = model.AdditionalInformation;
                        customer.AdditionalInvoiceInformation = model.AdditionalInvoiceInformation;
                        customer.WarningInformation = model.WarningInformation;
                        customer.HaveNewsLetter = model.HaveNewsLetter;
                        customer.DateAdded = DateTime.Now;
                        CustomerServices.Instance.SaveCustomer(customer);
                        var history = new History();
                        history.Business = LoggedInUser.Company;
                        history.CustomerName = customer.FirstName + " " + customer.LastName;
                        history.Date = DateTime.Now;
                        history.Note = "New customer saved: " + history.CustomerName;
                        history.EmployeeName = "";
                        HistoryServices.Instance.SaveHistory(history);
                        return Json(new { success = true }, JsonRequestBehavior.AllowGet);

                    }
                    else
                    {
                        return Json(new { success = false, Message = "This email is already in the system" }, JsonRequestBehavior.AllowGet);

                    }
                }
                else
                {
                    return Json(new { success = false, Message = "Kindly fill the details such as email of the customer" }, JsonRequestBehavior.AllowGet);

                }
            }
        }


        [HttpGet]
        public ActionResult Imported()
        {
            return View("Import");
        }


        [HttpPost]
        public ActionResult Import(HttpPostedFileBase excelfile)
        {
            var LoggedInUser = UserManager.FindById(User.Identity.GetUserId());
            if (excelfile == null || excelfile.ContentLength == 0)
            {
                ViewBag.Error = "Please Select Excel File";
                return View();
            }
            else
            {
                bool isPresent = false;
                var CustomerAddList = new List<Entities.Customer>();
                ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.NonCommercial; // or LicenseContext.Commercial

                if (excelfile != null && excelfile.ContentLength > 0)
                {
                    using (var package = new ExcelPackage(excelfile.InputStream))
                    {
                        var worksheet = package.Workbook.Worksheets[0];
                        var rowCount = worksheet.Dimension.Rows;

                        for (int row = 2; row <= rowCount; row++) // Assuming the first row is header
                        {
                            var customer = new Entities.Customer();
                            customer.FirstName = Convert.ToString(worksheet.Cells[row, 1].Value);
                            customer.LastName = Convert.ToString(worksheet.Cells[row, 2].Value);
                            customer.Business = UserManager.FindById(User.Identity.GetUserId()).Company;

                            customer.Gender = Convert.ToString(worksheet.Cells[row, 3].Value);
                            double numericDate = Convert.ToDouble(worksheet.Cells[row, 4].Value);
                            DateTime dateOfBirth = DateTime.FromOADate(numericDate);
                            customer.DateOfBirth = dateOfBirth;
                            //string dateOfBirthString = worksheet.Cells[row, 4].Value.ToString();

                            //customer.DateOfBirth = DateTime.Parse(dateOfBirthString);


                            customer.Email = Convert.ToString(worksheet.Cells[row, 5].Value);

                            customer.MobileNumber = Convert.ToString(worksheet.Cells[row, 7].Value);
                            customer.Address = Convert.ToString(worksheet.Cells[row, 8].Value);
                            if (worksheet.Cells[row, 9].Value == null)
                            {
                                customer.PostalCode = 0;
                            }
                            else
                            {
                                customer.PostalCode = Convert.ToInt32(worksheet.Cells[row, 9].Value);

                            }
                            customer.City = Convert.ToString(worksheet.Cells[row, 10].Value);
                            customer.AdditionalInformation = Convert.ToString(worksheet.Cells[row, 11].Value);
                            customer.AdditionalInvoiceInformation = Convert.ToString(worksheet.Cells[row, 12].Value);
                            customer.WarningInformation = Convert.ToString(worksheet.Cells[row, 13].Value);
                            var List = CustomerServices.Instance.GetCustomerWRTBusiness(LoggedInUser.Company);


                            if (List.Count != 0)
                            {
                                if (List.Any(x => x.Email.Trim().ToLower() == customer.Email.Trim().ToLower()))
                                {
                                    isPresent = true;
                                }
                                else
                                {
                                    isPresent = false;
                                }

                                if (isPresent == false)
                                {
                                    CustomerAddList.Add(customer);
                                    customer.DateAdded = DateTime.Now;
                                    customer.FirstName = customer.FirstName.Replace("'", "");
                                    customer.LastName = customer.LastName.Replace("'", "");
                                    CustomerServices.Instance.SaveCustomer(customer);
                                }
                            }
                            else
                            {
                                CustomerAddList.Add(customer);
                                customer.DateAdded = DateTime.Now;
                                customer.FirstName = customer.FirstName.Replace("'", "");
                                customer.LastName = customer.LastName.Replace("'", "");
                                CustomerServices.Instance.SaveCustomer(customer);

                            }
                        }

                    }
                    ViewBag.Customers = CustomerAddList;
                    return RedirectToAction("Imported", "Customer");

                }



                else
                {
                    ViewBag.Error = "Incorrect File";
                    return RedirectToAction("Index", "Customer");
                }
            }

            //var Prcoess = Process.GetProcessesByName("EXCEL.EXE").FirstOrDefault();
            //Prcoess.Kill();

        }



        [HttpGet]
        public ActionResult Import()
        {
            return PartialView("_CustomerImport");
        }


        public FileResult DownloadFile(string path)
        {
            if (System.IO.File.Exists(path))
            {
                // Determine the content type (MIME type) of the file
                string contentType = MimeMapping.GetMimeMapping(path);

                // Read the file data into a byte array
                byte[] fileBytes = System.IO.File.ReadAllBytes(path);

                // Create a FileContentResult to send the file to the client
                return File(fileBytes, contentType, Path.GetFileName(path));
            }
            else
            {
                // Return a not found response if the file doesn't exist
                return null;
            }
        }


        [HttpGet]
        public ActionResult Delete(int ID)
        {
            CustomerActionViewModel model = new CustomerActionViewModel();
            var Customer = CustomerServices.Instance.GetCustomer(ID);
            model.ID = Customer.ID;
            return PartialView("_Delete", model);
        }


        [HttpPost]
        public ActionResult Delete(CustomerActionViewModel model)
        {
            
            //var Customer = CustomerServices.Instance.GetCustomer(model.ID);
            //var file = FileServices.Instance.GetFile().Where(X=>X.CustomerID == Customer.ID).ToList();
            //var appointments= AppointmentServices.Instance.GetAppointment().Where(x=>x.CustomerID == model.ID);
            //var giftcardAssignments = GiftCardServices.Instance.GetGiftCardAssignment().Where(x => x.CustomerID == model.ID);
            //var loyaltyCardAssignments = LoyaltyCardServices.Instance.GetLoyaltyCardAssignments().Where(x => x.CustomerID == model.ID);

            //if (file.Count() > 0 || appointments.Count() > 0 || giftcardAssignments.Count() > 0 || loyaltyCardAssignments.Count() > 0)
            //{
            //    return Json(new { success = false,Message="This customer is linked with file, giftCards or appointment, Please delete that data to continue." }, JsonRequestBehavior.AllowGet);
            //}
            //else
            //{
                var message=CustomerServices.Instance.DeleteCustomer(model.ID);

                return Json(new { success = true,Message=message }, JsonRequestBehavior.AllowGet);
            //}
        }
    }
}