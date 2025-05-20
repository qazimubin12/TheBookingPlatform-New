using Microsoft.Ajax.Utilities;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using OfficeOpenXml.FormulaParsing.Excel.Functions.DateTime;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Xml.Linq;
using TheBookingPlatform.Entities;
using TheBookingPlatform.Services;
using TheBookingPlatform.ViewModels;
using TheBookingPlatform;


namespace TheBookingPlatform.Controllers
{
    public class EmployeeController : Controller
    {
        // GET: Employee
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

        public EmployeeController()
        {
        }

        public EmployeeController(AMUserManager userManager, AMSignInManager signInManager)
        {
            UserManager = userManager;
            SignInManager = signInManager;
        }

        #endregion
        [NoCache]
        public ActionResult Index(string SearchTerm = "")
        {
            EmployeeListingViewModel model = new EmployeeListingViewModel();
            var LoggedInUser = UserManager.FindById(User.Identity.GetUserId()); if (LoggedInUser == null) { return RedirectToAction("Login", "Account"); }
            if (LoggedInUser.Role == "Super Admin")
            {
                var Employees = EmployeeServices.Instance.GetEmployee(SearchTerm).OrderBy(x => x.DisplayOrder).ToList();
                var company = CompanyServices.Instance.GetCompany().Where(x => x.Business == LoggedInUser.Company).FirstOrDefault();
                var employeeRequest = EmployeeRequestServices.Instance.GetEmployeeRequestByBusiness(company.ID);
                foreach (var item in employeeRequest)
                {
                    if (item.Accepted)
                    {
                        var employee = EmployeeServices.Instance.GetEmployee(item.EmployeeID);
                        if (!Employees.Select(x => x.ID).ToList().Contains(employee.ID))
                        {
                            Employees.Add(employee);
                        }
                    }
                }

                var ListOfEmployeeModel = new List<EmployeeModel>();
                foreach (var item in Employees)
                {
                    var ServiceForEmployee = EmployeeServiceServices.Instance.GetEmployeeServiceWRTBusiness(LoggedInUser.Company, item.ID).Select(x => x.ServiceID).ToList();
                    var ServiceList = new List<Service>();
                    foreach (var service in ServiceForEmployee)
                    {
                        var Service = ServiceServices.Instance.GetService(service);
                        ServiceList.Add(Service);
                    }
                    var count = 0;
                    var calendarmanage = CalendarManageServices.Instance.GetCalendarManage().Where(x => x.UserID == item.LinkedEmployee).FirstOrDefault();
                    if (calendarmanage != null && calendarmanage.ManageOf != null)
                    {
                        count = calendarmanage.ManageOf.Split(',').ToList().Count();
                    }
                    ListOfEmployeeModel.Add(new EmployeeModel { Employee = item, Services = ServiceList, ManageAccessCount = count });
                }
                model.Employees = ListOfEmployeeModel;

            }
            else
            {
                var Employees = EmployeeServices.Instance.GetEmployee(SearchTerm).Where(x => x.Business == LoggedInUser.Company).ToList().OrderBy(x => x.DisplayOrder).ToList();
                var company = CompanyServices.Instance.GetCompany().Where(x => x.Business == LoggedInUser.Company).FirstOrDefault();
                var employeeRequest = EmployeeRequestServices.Instance.GetEmployeeRequestByBusiness(company.ID);
                foreach (var item in employeeRequest)
                {
                    if (item.Accepted)
                    {
                        var employee = EmployeeServices.Instance.GetEmployee(item.EmployeeID);
                        if (!Employees.Select(X => X.ID).Contains(employee.ID))
                        {
                            Employees.Add(employee);
                        }
                    }
                }


                var ListOfEmployeeModel = new List<EmployeeModel>();
                foreach (var item in Employees)
                {

                    var ServiceForEmployee = EmployeeServiceServices.Instance.GetEmployeeServiceWRTBusiness(LoggedInUser.Company, item.ID).Select(x => x.ServiceID).ToList();
                    var ServiceList = new List<Service>();

                    foreach (var service in ServiceForEmployee)
                    {
                        var Service = ServiceServices.Instance.GetService(service);
                        ServiceList.Add(Service);
                    }


                    var count = 0;
                    var calendarmanage = CalendarManageServices.Instance.GetCalendarManage().Where(x => x.UserID == item.LinkedEmployee).FirstOrDefault();
                    if (calendarmanage != null && calendarmanage.ManageOf != null)
                    {
                        count = calendarmanage.ManageOf.Split(',').ToList().Count();
                    }
                    ListOfEmployeeModel.Add(new EmployeeModel { Employee = item, Services = ServiceList, ManageAccessCount = count });
                }
                model.Employees = ListOfEmployeeModel;

            }
            var listofCalendarAccess = new List<string>
            {
                "Do not limit",
                "Do not show previous days",
                "1 day before",
                "3 days before",
                "7 days before",
                "1 month before",
                "3 months before",
                "6 months before"
            };
            model.CalendarHistoryAccessList = listofCalendarAccess;
            model.Types = new List<string>
            {
                "Percentage",
                "Freelancer",
                "Time to Time"
            };
            return View(model);
        }


        [HttpPost]
        public ActionResult UpdateOnlineBooking(int ID)
        {
            var employee = EmployeeServices.Instance.GetEmployee(ID);
            if (employee.AllowOnlineBooking == false)
            {
                employee.AllowOnlineBooking = true;
                EmployeeServices.Instance.UpdateEmployee(employee);
            }
            else
            {
                employee.AllowOnlineBooking = false;
                EmployeeServices.Instance.UpdateEmployee(employee);
            }
            return Json(new { success = true });

        }


        [HttpPost]
        public ActionResult UpdateType(int ID, string Type, float Number)
        {
            var employee = EmployeeServices.Instance.GetEmployee(ID);
            employee.Type = Type;
            employee.Percentage = Number;
            EmployeeServices.Instance.UpdateEmployee(employee);
            return Json(new { success = true, Employee = employee });
        }


        [HttpPost]
        public ActionResult UpdateIsActiveStatus(int ID)
        {
            var employee = EmployeeServices.Instance.GetEmployee(ID);
            if (employee.IsActive == false)
            {
                employee.IsActive = true;
                EmployeeServices.Instance.UpdateEmployee(employee);
            }
            else
            {
                employee.IsActive = false;
                EmployeeServices.Instance.UpdateEmployee(employee);
            }
            return Json(new { success = true });
        }

        [HttpPost]
        public ActionResult UpdateDisplayOrder(int employeeID, int newOrder)
        {
            var Employee = EmployeeServices.Instance.GetEmployee(employeeID);
            Employee.DisplayOrder = newOrder;
            EmployeeServices.Instance.UpdateEmployee(Employee);
            return Json(new { success = true });
        }


        [HttpPost]
        public ActionResult UpdateLimit(int employeeID, string LimitToBeSet)
        {
            var Employee = EmployeeServices.Instance.GetEmployee(employeeID);
            Employee.LimitCalendarHistory = LimitToBeSet;
            EmployeeServices.Instance.UpdateEmployee(Employee);
            return Json(new { success = true });
        }

        public class AsingServiceViewModel
        {
            public ServiceModel Service { get; set; }
            public EmployeeService EmployeeService { get; set; }
        }

        [HttpGet]
        public ActionResult AssignService(int ID)
        {
            EmployeeServiceActionViewModel model = new EmployeeServiceActionViewModel();

            var ServicesList = new List<ServiceModel>();
            var LoggedInUser = UserManager.FindById(User.Identity.GetUserId()); if (LoggedInUser == null) { return RedirectToAction("Login", "Account"); }
            var company = CompanyServices.Instance.GetCompany().Where(x => x.Business == LoggedInUser.Company).FirstOrDefault();
            if (LoggedInUser.Role != "Super Admin")
            {
                var categories = ServicesCategoriesServices.Instance.GetServiceCategories().Where(x => x.Business == LoggedInUser.Company).OrderBy(x => x.DisplayOrder).ToList();
                foreach (var item in categories)
                {
                    var ServicesWRTCategory = ServiceServices.Instance.GetService().Where(x => x.Category.Trim() == item.Name.Trim() && x.IsActive && x.Business == LoggedInUser.Company).OrderBy(x => x.DisplayOrder).ToList();
                    ServicesList.Add(new ServiceModel { ServiceCategory = item, Services = ServicesWRTCategory, Company = company });
                }
            }
            else
            {
                var categories = ServicesCategoriesServices.Instance.GetServiceCategories().OrderBy(x => x.DisplayOrder).ToList();
                foreach (var item in categories)
                {
                    var ServicesWRTCategory = ServiceServices.Instance.GetService().Where(x => x.Category.Trim() == item.Name.Trim() && x.IsActive).OrderBy(x => x.DisplayOrder).ToList();
                    ServicesList.Add(new ServiceModel { ServiceCategory = item, Services = ServicesWRTCategory, Company = company });
                }

            }
            var employee = EmployeeServices.Instance.GetEmployee(ID);
            model.EmployeeFull = employee;
            model.EmployeeID = employee.ID;
            //var ServiceEmployeeData = "";

            var ServiceEmployees = EmployeeServiceServices.Instance.GetEmployeeServiceWRTBusiness(employee.Business, employee.ID);

            var employeeRequest = EmployeeRequestServices.Instance.GetEmployeeRequestByBusiness(company.ID);
            foreach (var item in employeeRequest)
            {
                if (item.Accepted && item.Business == LoggedInUser.Company)
                {
                    ServiceEmployees.AddRange(EmployeeServiceServices.Instance.GetEmployeeServiceWRTEmployeeID(item.EmployeeID));
                }
            }
            //var AssignedServiceList = new List<AsingServiceViewModel>();
            //foreach (var item in ServicesList)
            //{
            //    EmployeeService assignedServiceEmp = null;
            //    foreach (var service in item.Services)
            //    {
            //        assignedServiceEmp = ServiceEmployees.Where(x=>x.ServiceID == service.ID).FirstOrDefault();
            //        if(assignedServiceEmp != null)
            //        {
            //            break;
            //        }
            //        else
            //        {

            //        }

            //    }
            //    if (assignedServiceEmp != null)
            //    {
            //        AssignedServiceList.Add(new AsingServiceViewModel { Service = item, EmployeeService = assignedServiceEmp });

            //    }
            //    else
            //    {
            //        AssignedServiceList.Add(new AsingServiceViewModel { Service = item });

            //    }
            //    break;
            //}
            model.EmployeeServicesE = ServiceEmployees;
            //foreach (var item in ServiceEmployees)
            //{
            //    if (ServiceEmployeeData == "")
            //    {
            //        var datatoJoin = String.Join(",", item.ServiceID, item.BufferEnabled, item.BufferTime);
            //        ServiceEmployeeData = String.Join("_", datatoJoin);

            //    }
            //    else
            //    {
            //        var datatoJoin = String.Join(",", item.ServiceID, item.BufferEnabled, item.BufferTime);
            //        ServiceEmployeeData = String.Join("_", ServiceEmployeeData, datatoJoin);
            //    }
            //}
            //model.ServiceData = ServiceEmployeeData;
            model.Services = ServicesList;
            return PartialView("_AssignService", model);
        }


        [HttpPost]
        public ActionResult AssignService(EmployeeServiceActionViewModel model)
        {
            var LoggedInUser = UserManager.FindById(User.Identity.GetUserId());
            var ServiceEmployees = EmployeeServiceServices.Instance.GetEmployeeServiceWRTBusiness(LoggedInUser.Company, model.EmployeeID);
            foreach (var item in ServiceEmployees)
            {
                EmployeeServiceServices.Instance.DeleteEmployeeService(item.ID);
            }
            if (model.ServiceData != null)
            {
                var ServiceList = model.ServiceData.Split('_').ToList();

                foreach (var item in ServiceList)
                {
                    var SplitData = item.Split(',');
                    var itemID = SplitData[0];
                    var service = ServiceServices.Instance.GetService(int.Parse(itemID));
                    var MainEmployee_Service = new EmployeeService();
                    MainEmployee_Service.Business = service.Business;
                    MainEmployee_Service.ServiceID = service.ID;
                    MainEmployee_Service.BufferEnabled = bool.Parse(SplitData[1]);
                    if (SplitData.Length > 2)
                    {
                        MainEmployee_Service.BufferTime = SplitData[2];
                    }
                    MainEmployee_Service.EmployeeID = model.EmployeeID;
                    EmployeeServiceServices.Instance.SaveEmployeeService(MainEmployee_Service);
                }
            }

            return Json(new { success = true }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        [NoCache]
        public ActionResult Action(int ID = 0)
        {
            EmployeeActionViewModel model = new EmployeeActionViewModel();
            var LoggedInUser = UserManager.FindById(User.Identity.GetUserId());
            var company = CompanyServices.Instance.GetCompanyByName(LoggedInUser.Company);

            var package = PackageServices.Instance.GetPackage(company.Package);
            var currentlyAlloted = EmployeeServices.Instance.GetEmployeeWRTBusiness(LoggedInUser.Company, "").Where(x => x.LinkedEmployee != LoggedInUser.Id).Count();

            if (ID != 0)
            {
                var Employee = EmployeeServices.Instance.GetEmployee(ID);
                model.ID = Employee.ID;
                model.Business = Employee.Business;
                model.Name = Employee.Name;
                model.Photo = Employee.Photo;
                model.Gender = Employee.Gender;
                model.Experience = Employee.Experience;
                model.AllowOnlineBooking = Employee.AllowOnlineBooking;
                model.Description = Employee.Description;
                model.ExpYears = Employee.ExpYears;
                model.Specialization = Employee.Specialization;
                model.LinkedEmployee = Employee.LinkedEmployee;
                if (LoggedInUser.Role == "Super Admin")
                {
                    model.Users = UserManager.Users.ToList();
                }
                else
                {
                    model.Users = UserManager.Users.ToList().Where(x => x.Company == LoggedInUser.Company).ToList();
                }

            }
            else
            {
                if (package != null)
                {
                    if (currentlyAlloted >= package.NoOfUsers)
                    {
                        model.DisableRegister = true;
                    }
                }
                var employeeLinked = EmployeeServices.Instance.GetEmployee().Select(x => x.LinkedEmployee).ToList();
                var users = new List<string>();
                var finalUsersList = new List<User>();
                if (LoggedInUser.Role == "Super Admin")
                {
                    users = UserManager.Users.Select(x => x.Id).ToList();
                    foreach (var item in users)
                    {
                        if (!employeeLinked.Contains(item))
                        {
                            var UserToBeAdded = UserManager.FindById(item);
                            finalUsersList.Add(UserToBeAdded);
                        }
                    }
                }
                else
                {
                    users = UserManager.Users.Where(x => x.Company == LoggedInUser.Company).Select(x => x.Id).ToList();
                    foreach (var item in users)
                    {
                        if (!employeeLinked.Contains(item))
                        {
                            var UserToBeAdded = UserManager.FindById(item);
                            finalUsersList.Add(UserToBeAdded);
                        }
                    }
                }
                model.Users = finalUsersList;
            }
            return View(model);
        }


        [HttpGet]
        public JsonResult HaveAnyAppointments(int ID)
        {
            bool HaveAppointments = AppointmentServices.Instance.GetAppointment().Where(x => x.EmployeeID == ID).Any();
            return Json(new { HaveAppointments = HaveAppointments }, JsonRequestBehavior.AllowGet);
        }


        [HttpPost]
        public ActionResult Action(EmployeeActionViewModel model)
        {
            var LoggedInUser = UserManager.FindById(User.Identity.GetUserId()); if (LoggedInUser == null) { return RedirectToAction("Login", "Account"); }
            if (model.ID != 0)
            {
                var Employee = EmployeeServices.Instance.GetEmployee(model.ID);
                Employee.ID = model.ID;
                Employee.Name = model.Name;
                Employee.Gender = model.Gender;
                Employee.Photo = model.Photo;
                Employee.ExpYears = model.ExpYears;
                Employee.Description = model.Description;
                //Employee.Type = model.Type;
                //Employee.Percentage = model.Percentage;
                Employee.Specialization = model.Specialization;
                Employee.Experience = model.Experience;
                Employee.LinkedEmployee = model.LinkedEmployee;
                EmployeeServices.Instance.UpdateEmployee(Employee);

                var webhooklock = HookLockServices.Instance.GetHookLockWRTBusiness(LoggedInUser.Company, Employee.ID);
                if(webhooklock != null)
                {
                    webhooklock = new HookLock();
                    webhooklock.EmployeeID = Employee.ID;
                    webhooklock.IsLocked = false;
                    webhooklock.Business = LoggedInUser.Company;
                    HookLockServices.Instance.SaveHookLock(webhooklock);
                }
            }
            else
            {
                var Employee = new Employee();
                if (LoggedInUser.Role != "Super Admin")
                {
                    Employee.Business = LoggedInUser.Company;
                }
                Employee.Name = model.Name;
                Employee.Photo = model.Photo;
                Employee.ExpYears = model.ExpYears;
                Employee.Experience = model.Experience;
                Employee.Gender = model.Gender;
                Employee.Description = model.Description;
                Employee.Specialization = model.Specialization;
                Employee.LinkedEmployee = model.LinkedEmployee;
                //Employee.Type = model.Type;
                //Employee.Percentage = model.Percentage;
                EmployeeServices.Instance.SaveEmployee(Employee);
                var webhooklock = HookLockServices.Instance.GetHookLockWRTBusiness(LoggedInUser.Company, Employee.ID);
                if (webhooklock != null)
                {
                    webhooklock = new HookLock();
                    webhooklock.EmployeeID = Employee.ID;
                    webhooklock.IsLocked = false;
                    webhooklock.Business = LoggedInUser.Company;
                    HookLockServices.Instance.SaveHookLock(webhooklock);
                }
            }
            return Json(new { success = true }, JsonRequestBehavior.AllowGet);
        }





        [HttpGet]
        public ActionResult Delete(int ID)
        {
            EmployeeActionViewModel model = new EmployeeActionViewModel();
            var Employee = EmployeeServices.Instance.GetEmployee(ID);
            model.ID = Employee.ID;
            model.IsActive = Employee.IsActive;
            return PartialView("_Delete", model);
        }


        [HttpPost]
        public ActionResult Delete(EmployeeActionViewModel model)
        {
            var Employee = EmployeeServices.Instance.GetEmployee(model.ID);
            var LoggedInUser = UserManager.FindById(User.Identity.GetUserId()); if (LoggedInUser == null) { return RedirectToAction("Login", "Account"); }
            var haveanyfutureappointments = AppointmentServices.Instance.CheckForFutureAppointments(Employee.ID, LoggedInUser.Company, false).Where(x => x.Date.Date > DateTime.Now.Date).ToList();

            if (Employee.IsActive == true && haveanyfutureappointments.Count() == 0)
            {
                Employee.IsActive = false;
                EmployeeServices.Instance.UpdateEmployee(Employee);
                //var message = EmployeeServices.Instance.DeleteEmployee(model.ID);
                return Json(new { success = true, Message = "Set as In Active" }, JsonRequestBehavior.AllowGet);


            }
            else if (Employee.IsActive == true && haveanyfutureappointments.Count() > 0)
            {
                return Json(new { success = false, Message = "Employee have these future appointments:", AppointmentsList = haveanyfutureappointments.Select(x => String.Join(" ", x.Date.ToString("yyyy-MM-dd"), x.Time.ToString("HH:mm"))) }, JsonRequestBehavior.AllowGet);

            }
            else
            {
                Employee.IsActive = true;
                EmployeeServices.Instance.UpdateEmployee(Employee);
                //var message = EmployeeServices.Instance.DeleteEmployee(model.ID);
                return Json(new { success = true, Message = "Set as Activated" }, JsonRequestBehavior.AllowGet);
            }

        }
    }
}