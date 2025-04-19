using Microsoft.Ajax.Utilities;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity.Infrastructure;
using System.Diagnostics;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using System.Xml.Linq;
using TheBookingPlatform.Entities;
using TheBookingPlatform.Services;
using TheBookingPlatform.ViewModels;

namespace TheBookingPlatform.Controllers
{
    public class ServiceController : Controller
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

        public ServiceController()
        {
        }

        public ServiceController(AMUserManager userManager, AMSignInManager signInManager)
        {
            UserManager = userManager;
            SignInManager = signInManager;
        }

        #endregion
        // GET: Service

        [NoCache]
        public ActionResult Index(string SearchTerm = "", string ServiceCategory = "")
        {
            ServiceListingViewModel model = new ServiceListingViewModel();
            var ServicesList = new List<ServiceModel>();
            model.ServiceCategory = ServiceCategory;
            var LoggedInUser = UserManager.FindById(User.Identity.GetUserId()); if (LoggedInUser == null) { return RedirectToAction("Login", "Account"); }
            var company = CompanyServices.Instance.GetCompany().Where(x => x.Business == LoggedInUser.Company).FirstOrDefault();
            if (LoggedInUser.Role != "Super Admin")
            {
                var categories = ServicesCategoriesServices.Instance.GetServiceCategories(SearchTerm).Where(x => x.Business == LoggedInUser.Company).OrderBy(x => x.DisplayOrder).ToList();
                foreach (var item in categories)
                {
                    var ServicesWRTCategory = ServiceServices.Instance.GetService().Where(x => x.Category.Trim() == item.Name.Trim() && x.Business == LoggedInUser.Company).OrderBy(x => x.DisplayOrder).ToList();
                    ServicesList.Add(new ServiceModel { ServiceCategory = item, Services = ServicesWRTCategory, Company = company });
                }
            }
            else
            {
                var categories = ServicesCategoriesServices.Instance.GetServiceCategories(SearchTerm).OrderBy(x => x.DisplayOrder).ToList();
                foreach (var item in categories)
                {
                    var ServicesWRTCategory = ServiceServices.Instance.GetService().Where(x => x.Category.Trim() == item.Name.Trim()).OrderBy(x => x.DisplayOrder).ToList();
                    ServicesList.Add(new ServiceModel { ServiceCategory = item, Services = ServicesWRTCategory, Company = company });
                }
            }
            var DeletedServices = new ServiceModelInServices();

            model.Services = ServicesList.OrderBy(X => X.ServiceCategory.DisplayOrder).ToList();
            var DeletedServicesList = ServiceServices.Instance.GetService().Where(x => x.IsActive == false  && x.Business == LoggedInUser.Company).OrderBy(x => x.DisplayOrder).ToList();
            DeletedServices.Services = DeletedServicesList;
            DeletedServices.Company = company;
            DeletedServices.ServiceCategory = "DELETED";
            model.DeletedServices = DeletedServices;
            return View(model);
        }







        [HttpPost]
        public ActionResult UpdateDisplayOrder(int serviceID, int newOrder)
        {
            var Service = ServiceServices.Instance.GetService(serviceID);
            Service.DisplayOrder = newOrder;
            ServiceServices.Instance.UpdateService(Service);
            return Json(new { success = true });
        }




        [HttpGet]
        public ActionResult ActionCategory()
        {
            ServiceCategoriesActionViewModel model = new ServiceCategoriesActionViewModel();
            return PartialView("_ActionCategory", model);
        }




        [HttpGet]
        public ActionResult ActionResource(string Type)
        {
            ResourceActionViewModel model = new ResourceActionViewModel();
            model.Type = Type;
            return PartialView("_ActionResource", model);
        }


        [HttpGet]
        public ActionResult ActionEmployee()
        {
            EmployeeActionViewModel model = new EmployeeActionViewModel();
            return PartialView("_ActionEmployee", model);
        }
        public JsonResult GetCategories()
        {
            // Retrieve data from the database
            var LoggedInUser = UserManager.FindById(User.Identity.GetUserId());

            List<ServiceCategory> serviceCategories = ServicesCategoriesServices.Instance.GetServiceCategoriesWRTBusiness(LoggedInUser.Company); // fetch your data here

            // Return data as JSON
            return Json(serviceCategories, JsonRequestBehavior.AllowGet);
        }



        public ActionResult Export()
        {
            var LoggedInUser = UserManager.FindById(User.Identity.GetUserId()); if (LoggedInUser == null) { return RedirectToAction("Login", "Account"); }
            var Services = new List<Service>();
            if (LoggedInUser.Role != "Super Admin")
            {
                Services = ServiceServices.Instance.GetService().Where(X => X.Business == LoggedInUser.Company).ToList();
            }
            else
            {
                Services = ServiceServices.Instance.GetService();
            }

            // Create a DataTable and populate it with the site data
            System.Data.DataTable tableData = new System.Data.DataTable();
            tableData.Columns.Add("ID",typeof(int));
            tableData.Columns.Add("Name",typeof(string));
            tableData.Columns.Add("Category",typeof(string));
            tableData.Columns.Add("Price",typeof(string));
            tableData.Columns.Add("VAT",typeof(string));
            tableData.Columns.Add("Duration",typeof(string));
            tableData.Columns.Add("Setup",typeof(string));
            tableData.Columns.Add("Processing",typeof(string));
            tableData.Columns.Add("Finish",typeof(string));
            tableData.Columns.Add("Tool",typeof(string));
            tableData.Columns.Add("Room",typeof(string));
            tableData.Columns.Add("Notes",typeof(string));
            tableData.Columns.Add("CanBookOnline",typeof(string));
            tableData.Columns.Add("Business",typeof(string));
            tableData.Columns.Add("DoesRequiredProcessing",typeof(string));
            tableData.Columns.Add("DisplayOrder", typeof(int));
         


            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            foreach (var Service in Services)
            {
                DataRow row = tableData.NewRow();

                row["ID"] = Service.ID;
                row["Name"] = Service.Name;
                row["Category"] = Service.Category;
                row["Price"] = Service.Price;
                row["VAT"] = Service.VAT;
                row["Duration"] = Service.Duration;
                row["Setup"] = Service.Setup;
                row["Processing"] = Service.Processing;
                row["Finish"] = Service.Finish;
                row["Tool"] = Service.Tool;
                row["Room"] = Service.Room;
                row["Notes"] = Service.Notes;
                row["CanBookOnline"] = Service.CanBookOnline;
                row["Business"] = Service.Business;
                row["DoesRequiredProcessing"] = Service.DoesRequiredProcessing;
                row["DisplayOrder"] = Service.DisplayOrder;

                tableData.Rows.Add(row);
            }

            // Create the Excel package
            using (ExcelPackage package = new ExcelPackage())
            {
                // Create a new worksheet
                ExcelWorksheet worksheet = package.Workbook.Worksheets.Add("Services");

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
                return File(excelBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Services.xlsx");
            }
        }

        public JsonResult GetEmployees()
        {
            // Retrieve data from the database
            var LoggedInUser = UserManager.FindById(User.Identity.GetUserId());

            List<Employee> Employees = EmployeeServices.Instance.GetEmployeeWRTBusiness(LoggedInUser.Company); // fetch your data here

            // Return data as JSON
            return Json(Employees, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetResources(string Type)
        {
            var LoggedInUser = UserManager.FindById(User.Identity.GetUserId());
            // Retrieve data from the database
            List<Resource> Resources = ResourceServices.Instance.GetResource().Where(x => x.Type == Type && x.Business == LoggedInUser.Company).ToList(); // fetch your data here

            // Return data as JSON
            return Json(Resources, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult ActionCategory(ServiceCategoriesActionViewModel model)
        {

            var LoggedInUser = UserManager.FindById(User.Identity.GetUserId()); if (LoggedInUser == null) { return RedirectToAction("Login", "Account"); }

            var newServiceCategory = new ServiceCategory();
            newServiceCategory.Name = model.Name;
            newServiceCategory.Business = LoggedInUser.Company;
            newServiceCategory.Type = model.Type;
            ServicesCategoriesServices.Instance.SaveServiceCategory(newServiceCategory);
            return Json(new {success=true,NameOfCategory=newServiceCategory.Name},JsonRequestBehavior.AllowGet);
        }




        [HttpGet]
        public ActionResult Duplicate(int ID)
        {

            var service = new Service();
            var Service = ServiceServices.Instance.GetService(ID);
            service.Business = Service.Business;
            service.Name = Service.Name+" Duplicate";
            service.Category = Service.Category;
            service.PromoPrice = service.Price = Service.Price;
            service.VAT = Service.VAT;
            service.Duration = Service.Duration;
            service.AddOn = Service.AddOn;
            service.Setup = Service.Setup;
            service.Processing = Service.Processing;
            service.DoesRequiredProcessing = Service.DoesRequiredProcessing;
            service.Finish = Service.Finish;
            service.Tool = Service.Tool;
            service.IsActive = true;
            service.PromoPrice = service.PromoPrice = Service.PromoPrice;
            service.Room = Service.Room;
            service.Notes = Service.Notes;
            service.CanBookOnline = Service.CanBookOnline;
            service.PromoPrice = service.NumberofSessions = Service.NumberofSessions;
            ServiceServices.Instance.SaveService(service);

            return RedirectToAction("Index", "Service");
        }








        [HttpGet]
        [NoCache]
        public ActionResult Action(int ID = 0)
        {
            ServiceActionViewModel model = new ServiceActionViewModel();
            var LoggedInUser = UserManager.FindById(User.Identity.GetUserId()); if (LoggedInUser == null) { return RedirectToAction("Login", "Account"); }
            if (LoggedInUser.Role != "Super Admin")
            {
                model.Tools = ResourceServices.Instance.GetResource().Where(x => x.Type == "Tool" && x.Business == LoggedInUser.Company).ToList();
                model.Rooms = ResourceServices.Instance.GetResource().Where(x => x.Type == "Room" && x.Business == LoggedInUser.Company).ToList();
                model.Vats = VatServices.Instance.GetVat();
                model.ServiceCategories = ServicesCategoriesServices.Instance.GetServiceCategories().Where(x => x.Business == LoggedInUser.Company).ToList();
                var employees = EmployeeServices.Instance.GetEmployeeWRTBusiness(LoggedInUser.Company, true);

                var company = CompanyServices.Instance.GetCompany().Where(X => X.Business == LoggedInUser.Company).FirstOrDefault();
                var employeeRequest = EmployeeRequestServices.Instance.GetEmployeeRequestByBusiness(company.ID);
                foreach (var item in employeeRequest)
                {
                    if (item.Accepted)
                    {
                        if (!employees.Select(x => x.ID).ToList().Contains(item.EmployeeID))
                        {
                            employees.Add(EmployeeServices.Instance.GetEmployee(item.EmployeeID));
                        }
                    }
                }
                model.Employees = employees;
            }
            else
            {
                model.Tools = ResourceServices.Instance.GetResource().Where(x => x.Type == "Tool").ToList();
                model.Rooms = ResourceServices.Instance.GetResource().Where(x => x.Type == "Room").ToList();
                model.Vats = VatServices.Instance.GetVat();
                model.ServiceCategories = ServicesCategoriesServices.Instance.GetServiceCategories().ToList();
                model.Employees = EmployeeServices.Instance.GetEmployee().ToList();
            }

            if (ID != 0)
            {
                var Service = ServiceServices.Instance.GetService(ID);
                model.ID = Service.ID;
                model.Business = Service.Business;
                model.Name = Service.Name;
                model.Category = Service.Category;
                model.Price = Service.Price;
                model.VAT = Service.VAT;
                model.AddOn = Service.AddOn;
                model.Duration = Service.Duration;
                model.PromoPrice = Service.PromoPrice;
                model.Setup = Service.Setup;
                model.Processing = Service.Processing;
                model.DoesRequiredProcessing = Service.DoesRequiredProcessing;
                model.Finish = Service.Finish;
                model.Tool = Service.Tool;
                model.Room = Service.Room;
                model.Notes = Service.Notes;
                model.NumberofSessions = Service.NumberofSessions;
                model.CanBookOnline = Service.CanBookOnline;
                string ServiceEmployee = "";
                var Employee_Service =  EmployeeServiceServices.Instance.GetEmployeeService().Where(x=>x.ServiceID == ID).ToList();
                foreach (var item in Employee_Service)
                {
                    if (ServiceEmployee == "")
                    {
                        ServiceEmployee = String.Join(",", EmployeeServices.Instance.GetEmployee(item.EmployeeID).ID);
                    }
                    else
                    {
                        ServiceEmployee = String.Join(",",ServiceEmployee, EmployeeServices.Instance.GetEmployee(item.EmployeeID).ID);

                    }
                }
                model.ServiceEmployee = ServiceEmployee;

            }
            return View(model);
        }


        [HttpPost]
        public ActionResult Action(ServiceActionViewModel model)
        {

            if (model.ID != 0)
            {
                var Service = ServiceServices.Instance.GetService(model.ID);
                var oldTime = Service.Duration;
                Service.ID = model.ID;
                Service.Name = model.Name;
                Service.Category = model.Category;
                Service.Price = model.Price;
                Service.VAT = model.VAT;
                Service.Duration = model.Duration;
                Service.AddOn = model.AddOn;
                Service.Setup = model.Setup;
                Service.PromoPrice = model.PromoPrice;
                Service.Processing = model.Processing;
                Service.NumberofSessions = model.NumberofSessions;
                Service.Finish = model.Finish;
                Service.DoesRequiredProcessing = model.DoesRequiredProcessing;
                Service.Tool = model.Tool;
                Service.Room = model.Room;
                Service.IsActive = true;
                Service.Notes = model.Notes;
                Service.CanBookOnline = model.CanBookOnline;

                var EmpPreviouslyUsed = EmployeeServiceServices.Instance.GetEmployeeService().Where(x => x.ServiceID == Service.ID).ToList();
                foreach (var item in EmpPreviouslyUsed)
                {
                    EmployeeServiceServices.Instance.DeleteEmployeeService(item.ID);

                }

                if(model.Employee != null)
                {
                    var EmployeeService = model.Employee.Split(',').ToList();


                    foreach (var item in EmployeeService)
                    {
                        var employee = EmployeeServices.Instance.GetEmployee(int.Parse(item));
                        var employeeService_Main = new EmployeeService();
                        employeeService_Main.Business = Service.Business;
                        employeeService_Main.EmployeeID = employee.ID;
                        employeeService_Main.ServiceID = Service.ID;
                        EmployeeServiceServices.Instance.SaveEmployeeService(employeeService_Main);



                    }
                }


               
                ServiceServices.Instance.UpdateService(Service);

                var history = new History();
                history.Business = Service.Business;
                history.EmployeeName = User.Identity.Name;
                history.Date = DateTime.Now;
                history.Note = "Service was updated Old Duration: " + oldTime + " New Duration:" + Service.Duration;
                HistoryServices.Instance.SaveHistory(history);

            }
            else
            {
                var Service = new Service();
                            var LoggedInUser = UserManager.FindById(User.Identity.GetUserId()); if(LoggedInUser == null) { return RedirectToAction("Login", "Account"); }
                if (LoggedInUser.Role != "Super Admin")
                {
                    Service.Business = LoggedInUser.Company;
                }
                Service.Name = model.Name;
                Service.Category = model.Category;
                Service.Price = model.Price;
                Service.VAT = model.VAT;
                Service.Duration = model.Duration;
                Service.AddOn = model.AddOn;
                Service.DoesRequiredProcessing = model.DoesRequiredProcessing;
                Service.Setup = model.Setup;
                Service.Processing = model.Processing;
                Service.NumberofSessions = model.NumberofSessions;
                Service.Finish = model.Finish;
                Service.IsActive = true;
                Service.PromoPrice = model.PromoPrice;
                Service.Tool = model.Tool;
                Service.Room = model.Room;
                Service.Notes = model.Notes;
                Service.CanBookOnline = model.CanBookOnline;

                ServiceServices.Instance.SaveService(Service);
                if (model.Employee != null)
                {
                    var EmployeeService = model.Employee.Split(',').ToList();


                    foreach (var item in EmployeeService)
                    {
                        var employee = EmployeeServices.Instance.GetEmployee(int.Parse(item));
                        var employeeService_Main = new EmployeeService();
                        employeeService_Main.Business = Service.Business;
                        employeeService_Main.EmployeeID = employee.ID;
                        employeeService_Main.ServiceID = Service.ID;
                        EmployeeServiceServices.Instance.SaveEmployeeService(employeeService_Main);



                    }

                }
            }
            return Json(new { success = true }, JsonRequestBehavior.AllowGet);
        }



        [HttpGet]
        public ActionResult Delete(int ID)
        {
            ServiceActionViewModel model = new ServiceActionViewModel();
            var Service = ServiceServices.Instance.GetService(ID);
            model.ID = Service.ID;
            model.IsActive = Service.IsActive;
            return PartialView("_Delete", model);
        }


        [HttpPost]
        public ActionResult Delete(ServiceActionViewModel model)
        {

            var service = ServiceServices.Instance.GetService(model.ID);
            if (service.IsActive == true)
            {
                service.IsActive = false;
                ServiceServices.Instance.UpdateService(service);
                return Json(new { success = true, Message = "Set as In Active" }, JsonRequestBehavior.AllowGet);


            }
            else
            {
                service.IsActive = true;
                ServiceServices.Instance.UpdateService(service);
                return Json(new { success = true, Message = "Set as Activated" }, JsonRequestBehavior.AllowGet);
            }
        }
    }
}