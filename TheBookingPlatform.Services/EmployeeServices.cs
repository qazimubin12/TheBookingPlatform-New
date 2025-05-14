using TheBookingPlatform.Database;
using TheBookingPlatform.Entities;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheBookingPlatform.Services
{
    public class EmployeeServices
    {
        #region Singleton
        public static EmployeeServices Instance
        {
            get
            {
                if (instance == null) instance = new EmployeeServices();
                return instance;
            }
        }
        private static EmployeeServices instance { get; set; }
        private EmployeeServices()
        {
        }
        #endregion
        public class ValidationModel
        {
            public int MainID { get; set; }
            public int Count { get; set; }
        }

        public List<Employee> GetEmployee(string SearchTerm = "")
        {
            using (var context = new DSContext())
            {
                if (SearchTerm != "")
                {
                    return context.Employees.Where(p => p.IsDeleted == false && p.Name != null && p.Name.ToLower()
                                            .Contains(SearchTerm.ToLower()))
                                            .OrderBy(x => x.DisplayOrder)
                                            .ToList();
                }
                else
                {
                    return context.Employees.Where(x=>x.IsDeleted == false).OrderBy(x => x.DisplayOrder).ToList();
                }
            }
        }

        public Employee GetEmployeeWithLinkedUserID(string UserID)
        {
            using (var context = new DSContext())
            {
                return context.Employees.Where(x => x.LinkedEmployee == UserID && x.IsActive).FirstOrDefault();

            }
        }

        public Employee GetEmployeeWithLinkedGoogleCalendarID(string googleClaendaRId)
        {
            using (var context = new DSContext())
            {
                return context.Employees.Where(x => x.GoogleCalendarID == googleClaendaRId).FirstOrDefault();

            }
        }
        public List<Employee> GetBulkEmployees(List<int> IDs)
        {
            using (var context = new DSContext())
            {
                return context.Employees.Where(x => IDs.Contains(x.ID)).ToList();

            }
        }


        public List<Employee> GetEmployeeWRTBusiness(string Business,string SearchTerm = "")
        {
            using (var context = new DSContext())
            {
                if (SearchTerm != "")
                {
                    return context.Employees.Where(p => p.Business == Business &&  p.IsDeleted == false && p.Name != null && p.Name.ToLower()
                                            .Contains(SearchTerm.ToLower()))
                                            .OrderBy(x => x.DisplayOrder)
                                            .ToList();
                }
                else
                {
                    return context.Employees.Where(x => x.Business == Business &&  x.IsDeleted == false).OrderBy(x => x.DisplayOrder).ToList();
                }
            }
        }



        //public List<Employee> GetEmployeeWRTBusiness(string Business,bool IsActive,bool AllowOnlineBooking,List<int> ListOfIDs)
        //{
        //    using (var context = new DSContext())
        //    {

        //        return context.Employees.Where(x => x.Business == Business && x.IsActive == IsActive && x.AllowOnlineBooking ==AllowOnlineBooking && ListOfIDs.Contains(x.ID) && x.IsDeleted == false).OrderBy(x => x.DisplayOrder).ToList();

        //    }
        //}

        public List<Employee> GetEmployeeWRTBusiness(bool IsActive, bool AllowOnlineBooking, List<int> ListOfIDs,string Business)
        {
            using (var context = new DSContext())
            {

                return context.Employees.Where(x => x.Business == Business && x.IsActive == IsActive && x.AllowOnlineBooking == AllowOnlineBooking && ListOfIDs.Contains(x.ID) && x.IsDeleted == false).OrderBy(x => x.DisplayOrder).ToList();

            }
        }

        public List<Employee> GetEmployeeWRTBusiness(bool IsActive, List<int> ListOfIDs )
        {
            using (var context = new DSContext())
            {

                return context.Employees.Where(x => x.IsActive == IsActive  && ListOfIDs.Contains(x.ID) && x.IsDeleted == false).OrderBy(x => x.DisplayOrder).ToList();

            }
        }

        public List<Employee> GetEmployeeWRTBusiness(bool IsActive, List<int> ListOfIDs, string Business)
        {
            using (var context = new DSContext())
            {

                return context.Employees.Where(x => x.Business == Business && x.IsActive == IsActive && ListOfIDs.Contains(x.ID) && x.IsDeleted == false).OrderBy(x => x.DisplayOrder).ToList();

            }
        }



        public List<Employee> GetEmployeeWRTBusiness(bool IsActive, bool AllowOnlineBooking, List<int> ListOfIDs)
        {
            using (var context = new DSContext())
            {

                return context.Employees.Where(x =>  x.IsActive == IsActive && x.AllowOnlineBooking == AllowOnlineBooking && ListOfIDs.Contains(x.ID) && x.IsDeleted == false).OrderBy(x => x.DisplayOrder).ToList();

            }
        }
        public List<Employee> GetEmployeeWRTBusiness(string Business, bool IsActive, List<int> ListOfIDs)
        {
            using (var context = new DSContext())
            {

                return context.Employees.Where(x => x.Business == Business && x.IsActive == IsActive  && ListOfIDs.Contains(x.ID) && x.IsDeleted == false).OrderBy(x => x.DisplayOrder).ToList();

            }
        }


        public List<Employee> GetEmployeeWRTBusinesss( bool IsActive, List<int> ListOfIDs)
        {
            using (var context = new DSContext())
            {

                return context.Employees.Where(x => x.IsActive == IsActive && ListOfIDs.Contains(x.ID) && x.IsDeleted == false).OrderBy(x => x.DisplayOrder).ToList();

            }
        }
        public List<Employee> GetEmployeeWRTBusiness(string Business, bool IsActive)
        {
            using (var context = new DSContext())
            {

                return context.Employees.Where(x => x.Business == Business && x.IsActive == IsActive).OrderBy(x => x.DisplayOrder).ToList();

            }
        }
        public List<Employee> GetEmployeeWRTBusiness(bool IsActive)
        {
            using (var context = new DSContext())
            {

                return context.Employees.Where(x =>x.IsActive == IsActive).OrderBy(x => x.DisplayOrder).ToList();

            }
        }

        public List<int> GetEmployeeWRTBusinessOnlyID(string Business, bool IsActive)
        {
            using (var context = new DSContext())
            {

                return context.Employees.Where(x => x.Business == Business && x.IsActive == IsActive && x.IsDeleted == false).OrderBy(x => x.DisplayOrder).Select(x=>x.ID).ToList();

            }
        }


        public List<Employee> GetDeletedEmployee()
        {
            using (var context = new DSContext())
            {
                return context.Employees.Where(x => x.IsDeleted == true).ToList();
            }
        }

        public Employee GetEmployee(int ID)
        {
            using (var context = new DSContext())
            {

                return context.Employees.Find(ID);

            }
        }

        public void SaveEmployee(Employee Employee)
        {
            try
            {
                using (var context = new DSContext())
                {
                    context.Employees.Add(Employee);
                    context.SaveChanges();
                }
            }
            catch (Exception ex)
            {

                throw;
            }
           
        }

        public void UpdateEmployee(Employee Employee)
        {
            using (var context = new DSContext())
            {
                context.Entry(Employee).State = EntityState.Modified;
                context.SaveChanges();
            }
        }

        public string DeleteEmployee(int ID)
        {
            using (var context = new DSContext())
            {
                var query = $"SELECT A.ID AS MainID, COUNT(B.ID) AS Count FROM Employees A LEFT JOIN CalendarManages B ON CONCAT(',', B.ManageOf, ',') LIKE CONCAT('%,', A.ID, ',%') GROUP BY A.ID HAVING COUNT(B.ID) > 0;";
                var DataForValidation = context.Database.SqlQuery<ValidationModel>(query).ToList();
                if (DataForValidation.Any(x => x.MainID == ID))
                {
                    return "Cannot Delete it, because this is linked with another Calendar Manages";
                }

                query = $"SELECT A.ID AS MainID, COUNT(B.ID) AS Count FROM Employees A LEFT JOIN EmployeeServices B ON CONCAT(',', B.EmployeeID, ',') LIKE CONCAT('%,', A.ID, ',%') GROUP BY A.ID HAVING COUNT(B.ID) > 0;";
                DataForValidation = context.Database.SqlQuery<ValidationModel>(query).ToList();
                if (DataForValidation.Any(x => x.MainID == ID))
                {
                    return "Cannot Delete it, because this is linked with another employee assigned service";
                }

                query = $"SELECT A.ID AS MainID, COUNT(B.ID) AS Count FROM Employees A LEFT JOIN Appointments B ON CONCAT(',', B.employeeID, ',') LIKE CONCAT('%,', A.ID, ',%') GROUP BY A.ID HAVING COUNT(B.ID) > 0;";
                DataForValidation = context.Database.SqlQuery<ValidationModel>(query).ToList();
                if (DataForValidation.Any(x => x.MainID == ID))
                {
                    return "Cannot Delete it, because this is linked with another appointment";
                }

                query = $"SELECT A.ID AS MainID, COUNT(B.ID) AS Count FROM Employees A LEFT JOIN TimeTables B ON CONCAT(',', B.employeeID, ',') LIKE CONCAT('%,', A.ID, ',%') GROUP BY A.ID HAVING COUNT(B.ID) > 0;";
                DataForValidation = context.Database.SqlQuery<ValidationModel>(query).ToList();
                if (DataForValidation.Any(x => x.MainID == ID))
                {
                    return "Cannot Delete it, because this is linked with timetables";
                }
                else
                {
                    var Employee = context.Employees.Find(ID);
                    context.Employees.Remove(Employee);
                    context.SaveChanges();
                    return "Deleted Successfully";
                }
            }
        }
    }
}
