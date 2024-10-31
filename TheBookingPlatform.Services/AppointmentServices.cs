using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Runtime.Remoting.Contexts;
using System.Text;
using System.Threading.Tasks;
using TheBookingPlatform.Database;
using TheBookingPlatform.Database.Migrations;
using TheBookingPlatform.Entities;

namespace TheBookingPlatform.Services
{
    public class AppointmentServices
    {
        #region Singleton
        public static AppointmentServices Instance
        {
            get
            {
                if (instance == null) instance = new AppointmentServices();
                return instance;
            }
        }
        private static AppointmentServices instance { get; set; }
        private AppointmentServices()
        {
        }
        #endregion

        public List<Appointment> GetAppointment(string SearchTerm = "")
        {
            using (var context = new DSContext())
            {
                if (SearchTerm != "")
                {
                    return context.Appointments.AsNoTracking().Where(p => p.IsCancelled == false && p.Service != null && p.Service.ToLower()
                                            .Contains(SearchTerm.ToLower()))
                                            .OrderBy(x => x.Date)
                                            .ToList();
                }
                else
                {
                    return context.Appointments.AsNoTracking().Where(x => x.IsCancelled == false).OrderBy(x => x.Date).ToList();
                }
            }
        }

        public Appointment GetAppointmentWithGCalEventID(string GCalEventID)
        {
            using (var context = new DSContext())
            {
                return context.Appointments.AsNoTracking().Where(x=>x.GoogleCalendarEventID == GCalEventID).FirstOrDefault();
            }
        }

        public List<Appointment> OtherRecurrencesAppointments(int RepeatedAppointmentID)
        {
            using (var context = new DSContext())
            {
                return context.Appointments.AsNoTracking().Where(x=>x.FirstRepeatedID == RepeatedAppointmentID).ToList();
            }
        }

        public async Task<List<Appointment>> GetAppointmentAsync(string SearchTerm = "")
        {
            using (var context = new DSContext())
            {
                if (SearchTerm != "")
                {
                    return await context.Appointments.AsNoTracking().Where(p => p.IsCancelled == false && p.Service != null && p.Service.ToLower()
                                            .Contains(SearchTerm.ToLower()))
                                            .OrderBy(x => x.Date)
                                            .ToListAsync();
                }
                else
                {
                    return await context.Appointments.AsNoTracking().Where(x => x.IsCancelled == false).OrderBy(x => x.Date).ToListAsync();
                }
            }
        }

        public bool IsNewCustomer(string businesName, int CustomerID)
        {
            using (var context = new DSContext())
            {
                return context.Appointments.AsNoTracking().Where(x => x.Business == businesName && x.CustomerID == CustomerID && x.Date < DateTime.Now).Any();

            }
        }
        public class AppointmentDto
        {
            public int ID { get; set; }
            public string Business { get; set; }
            public string Service { get; set; }
            public string ServiceDiscount { get; set; }
            public string ServiceDuration { get; set; }

        }
        public List<AppointmentDto> GetFilteredAppointments()
        {
            using (var context = new DSContext())
            {
                var result = context.Appointments
                    .Where(a => a.FromGCAL == false && a.Service != null &&
                        // Count of commas in Service
                        (a.Service.Length - a.Service.Replace(",", "").Length) !=
                        // Count of commas in ServiceDuration
                        (a.ServiceDuration.Length - a.ServiceDuration.Replace(",", "").Length)

                        || // OR condition

                        // Count of commas in Service
                        (a.Service.Length - a.Service.Replace(",", "").Length) !=
                        // Count of commas in ServiceDiscount
                        (a.ServiceDiscount.Length - a.ServiceDiscount.Replace(",", "").Length)
                    )
                    .Select(a => new AppointmentDto
                    {
                        ID = a.ID,
                        Business = a.Business,
                        Service = a.Service,
                        ServiceDiscount = a.ServiceDiscount,
                        ServiceDuration = a.ServiceDuration
                    })
                    .ToList();

                return result;
            }
        }




        public List<Appointment> GetAppointmentBookingWRTBusiness(string businesName, bool Deleted, bool isCancelled, int EmployeeID)
        {
            using (var context = new DSContext())
            {

                return context.Appointments.AsNoTracking().Where(x => x.Business == businesName && x.DELETED == Deleted && x.IsCancelled == isCancelled && x.EmployeeID == EmployeeID).OrderBy(x => x.Date).ToList();

            }
        }

        public List<Appointment> GetAppointmentBookingWRTBusiness(string businesName, bool Deleted, bool isCancelled)
        {
            using (var context = new DSContext())
            {

                return context.Appointments.AsNoTracking().Where(x => x.Business == businesName && x.DELETED == Deleted && x.IsCancelled == isCancelled).OrderBy(x => x.Date).ToList();

            }
        }
        public bool GetAppointmentBookingWRTBusiness(string businessName, bool deleted, bool isCancelled, DateTime startDate, DateTime endDate, int numberOfDaysBehind, int customerID)
        {
            using (var context = new DSContext())
            {
                // Step 1: Check if the specified CustomerID has any appointments within StartDate and EndDate
                bool hasAppointmentInDateRange = context.Appointments.AsNoTracking()
                    .Any(x => x.Business == businessName &&
                              x.DELETED == deleted &&
                              x.IsCancelled == isCancelled &&
                              x.CustomerID == customerID &&
                              x.Date >= startDate &&
                              x.Date <= endDate);

                if (!hasAppointmentInDateRange)
                {
                    // If there are no appointments in the primary range, return false
                    return false;
                }

                // Step 2: Calculate past date range for additional check
                var pastStartDate = startDate.AddDays(-numberOfDaysBehind);

                // Step 3: Check if the same CustomerID has any appointments in the past range (StartDate - numberOfDaysBehind to StartDate)
                bool hasPastAppointment = context.Appointments.AsNoTracking()
                    .Any(x => x.Business == businessName &&
                              x.DELETED == deleted &&
                              x.IsCancelled == isCancelled &&
                              x.CustomerID == customerID &&
                              x.Date >= pastStartDate &&
                              x.Date < startDate);

                // Step 4: Return true if there's an appointment in the past range; otherwise, return false
                return hasPastAppointment;
            }
        }


        public List<Appointment> GetAppointmentBookingWRTBusiness(string businesName, bool Deleted, int CustomerID)
        {
            using (var context = new DSContext())
            {

                return context.Appointments.AsNoTracking().Where(x => x.Business == businesName && x.DELETED == Deleted && x.CustomerID == CustomerID).OrderBy(x => x.Date).ToList();

            }
        }
        public List<Appointment> GetAppointmentBookingWRTBusiness(bool Deleted, bool isCancelled, int EmployeeID)
        {
            using (var context = new DSContext())
            {

                return context.Appointments.AsNoTracking().Where(x => x.DELETED == Deleted && x.IsCancelled == isCancelled && x.EmployeeID == EmployeeID).OrderBy(x => x.Date).ToList();

            }
        }


        public List<Appointment> GetAppointmentBookingWRTBusiness(int Day, int Month, int Year, int EmployeeID, bool Deleted, bool IsCancelled)
        {
            using (var context = new DSContext())
            {

                return context.Appointments.AsNoTracking().Where(x => x.Date.Day == Day && x.Date.Month == Month && x.Date.Year == Year && x.DELETED == Deleted && x.IsCancelled == IsCancelled && x.EmployeeID == EmployeeID).OrderBy(x => x.Date).ToList();

            }
        }


        public List<Appointment> GetAppointmentBookingWRTBusiness(string businesName, int Day, int Month, int Year, int EmployeeID, bool Deleted, bool IsCancelled)
        {
            using (var context = new DSContext())
            {

                return context.Appointments.AsNoTracking().Where(x => x.Business == businesName && x.Date.Day == Day && x.Date.Month == Month && x.Date.Year == Year && x.DELETED == Deleted && x.IsCancelled == IsCancelled && x.EmployeeID == EmployeeID).OrderBy(x => x.Date).ToList();

            }
        }

        public List<Appointment> GetAppointmentBookingWRTBusiness(string businesName, int Day, int Month, int Year, bool Deleted, bool IsCancelled)
        {
            using (var context = new DSContext())
            {

                return context.Appointments.AsNoTracking().Where(x => x.Business == businesName && x.Date.Day == Day && x.Date.Month == Month && x.Date.Year == Year && x.DELETED == Deleted && x.IsCancelled == IsCancelled).OrderBy(x => x.Date).ToList();

            }
        }

        public async Task<List<Appointment>> GetAppointmentBookingWRTBusinessAsync(string businesName, int Day, int Month, int Year, bool Deleted, bool IsCancelled)
        {
            using (var context = new DSContext())
            {
                // Create a DateTime object to simplify date comparison
                var targetDate = new DateTime(Year, Month, Day);

                // Query optimization
                return await context.Appointments
                    .AsNoTracking()
                    .Where(x => x.Business == businesName
                                && x.Date.Year == Year
                                && x.Date.Month == Month
                                && x.Date.Day == Day
                                && x.DELETED == Deleted
                                && x.IsCancelled == IsCancelled)
                    .OrderBy(x => x.Date)
                    .ToListAsync();
            }
        }


        public async Task<List<Appointment>> GetAppointmentBookingWRTBusinessAsync(string businesName, int Day, int Month, int Year, bool Deleted, bool IsCancelled, int EmployeeID)
        {
            using (var context = new DSContext())
            {
                // Create a DateTime object to simplify date comparison
                var targetDate = new DateTime(Year, Month, Day);

                // Query optimization
                return await context.Appointments
                    .AsNoTracking() // Disable change tracking for better performance
                    .Where(x => x.Business == businesName
                                && x.Date.Year == Year
                                && x.Date.Month == Month
                                && x.Date.Day == Day
                                && x.DELETED == Deleted
                                && x.EmployeeID == EmployeeID
                                && x.IsCancelled == IsCancelled)
                    .OrderBy(x => x.Date)
                    .ToListAsync();
            }
        }


        public List<Appointment> GetAllAppointmentWRTBusiness(string Business, int CustomerID, int Day, int Month, int Year)
        {
            using (var context = new DSContext())
            {

                return context.Appointments.AsNoTracking().Where(x => x.Business == Business && x.Date.Day == Day && x.Date.Month == Month && x.Date.Year == Year && x.DELETED == false && x.CustomerID == CustomerID).OrderBy(x => x.Date).ToList();
            }
        }

        public bool GetAllAppointmentWRTBusiness(string Business, string EmployeeCalendarID, DateTime StartDate, DateTime StartTime, DateTime EndTime)
        {
            using (var context = new DSContext())
            {
                return context.Appointments.AsNoTracking()
                    .Where(x => x.Business == Business
                        && x.Date.Day == StartDate.Day &&
                        x.Date.Month == StartDate.Month &&
                        x.Date.Year == StartDate.Year 
                        && x.Time.Hour == StartTime.Hour 
                        && x.Time.Minute == StartTime.Minute
                        && x.EndTime.Hour == EndTime.Minute 
                        && x.EndTime.Minute == EndTime.Minute
                        && x.IsCancelled == false
                        && x.DELETED == false
                        && x.GoogleCalendarEventID == EmployeeCalendarID)
                    .Any();
            }
        }

        public Appointment GetAllAppointmentWithGCalEventID(string googleEventID)
        {
            using (var context = new DSContext())
            {

                return context.Appointments.AsNoTracking().AsNoTracking() // Disable change tracking for better performance

                    .Where(x=>x.GoogleCalendarEventID == googleEventID && x.FromGCAL == true).FirstOrDefault();
            }
        }

        public bool HasPreviousAppointments(string companyId, int customerId, int currentAppointmentId, DateTime today)
        {
            using (var context = new DSContext())
            {
                return context.Appointments.AsNoTracking()
                    .AsNoTracking() // Disable change tracking for better performance
                    .Where(appointment => appointment.Business == companyId
                                          && appointment.CustomerID == customerId
                                          && appointment.ID != currentAppointmentId
                                          && appointment.Date < today)
                    .Any();
            }
        }

        public List<Appointment> GetAllAppointmentWRTBusiness(string Business, bool isDeleted)
        {
            using (var context = new DSContext())
            {

                return context.Appointments.AsNoTracking().Where(x => x.Business == Business && x.DELETED == isDeleted).OrderBy(x => x.Date).ToList();
            }
        }


        public List<Appointment> GetAllAppointmentWRTBusiness(string Business, bool isDeleted,bool fromGcal)
        {
            using (var context = new DSContext())
            {

                return context.Appointments.AsNoTracking().Where(x => x.Business == Business && x.FromGCAL == fromGcal && x.DELETED == isDeleted).OrderBy(x => x.Date).ToList();
            }
        }
        public List<Appointment> GetAllAppointmentWRTBusinessTodaynFuture(string Business, bool isDeleted,bool IsCancelled)
        {
            using (var context = new DSContext())
            {
                var today = DateTime.Now.Date; // Get today's date (ignore the time part)

                return context.Appointments
                    .AsNoTracking()
                    .Where(x => x.Business == Business && x.DELETED == isDeleted && x.IsCancelled == IsCancelled && x.GoogleCalendarEventID != null && x.Date >= today) // Filter today's and future appointments
                    .OrderBy(x => x.Date)
                    .ToList();
            }
        }

        public List<Appointment> GetAllAppointmentWRTBusiness(string Business, bool isDeleted, int CustomerID)
        {
            using (var context = new DSContext())
            {

                return context.Appointments.AsNoTracking().Where(x => x.Business == Business && x.DELETED == isDeleted && x.CustomerID == CustomerID).OrderBy(x => x.Date).ToList();
            }
        }
        public List<Appointment> CheckForFutureAppointments(int CustomerID, string Business)
        {
            using (var context = new DSContext())
            {

                return context.Appointments.AsNoTracking()
                              .Where(x => x.CustomerID == CustomerID
                                     && x.Business == Business
                                     && x.DELETED == false).ToList();
            }
        }
        public List<Appointment> CheckForFutureAppointments(int EmployeeID, string Business, bool isCancelled)
        {
            using (var context = new DSContext())
            {

                return context.Appointments.AsNoTracking()
                              .Where(x => x.EmployeeID == EmployeeID
                                     && x.Business == Business
                                     && x.IsCancelled == isCancelled
                                     && x.DELETED == false).ToList();
            }
        }
        public List<Appointment> GetAllAppointmentWRTBusiness(bool isDeleted)
        {
            using (var context = new DSContext())
            {

                return context.Appointments.AsNoTracking().Where(x => x.DELETED == isDeleted).OrderBy(x => x.Date).ToList();
            }
        }



        public List<Appointment> GetAllAppointmentWRTBusiness(string Business, bool isDeleted, List<int> EmployeeIDs, DateTime StartDate, DateTime EndDate, List<string> AbsenseServiceIDs, bool IsCancelled, List<string> SelectedStatus)
        {
            using (var context = new DSContext())
            {
                var totalServices = context.Appointments.AsNoTracking()
                .Where(x => x.Business == Business
                  && x.DELETED == false
                  && SelectedStatus.Contains(x.Status.Trim())
                  && x.IsCancelled == IsCancelled
                  && EmployeeIDs.Contains(x.EmployeeID)
                  && !AbsenseServiceIDs.Contains(x.Service)
                  && x.Date >= StartDate && x.Date <= EndDate)
                  .ToList();
                return totalServices;
            }
        }

        public List<Appointment> GetAllAppointment(string SearchTerm = "")
        {
            using (var context = new DSContext())
            {
                if (SearchTerm != "")
                {
                    return context.Appointments.AsNoTracking().Where(p => p.Service != null && p.Service.ToLower()
                                            .Contains(SearchTerm.ToLower()))
                                            .OrderBy(x => x.Date)
                                            .ToList();
                }
                else
                {
                    return context.Appointments.AsNoTracking().OrderBy(x => x.Date).ToList();
                }
            }
        }
        public List<Appointment> GetAppointmentBooking(string SearchTerm = "")
        {
            using (var context = new DSContext())
            {
                if (SearchTerm != "")
                {
                    return context.Appointments.AsNoTracking().Where(p => p.Service != null && p.Service.ToLower()
                                            .Contains(SearchTerm.ToLower()))
                                            .OrderBy(x => x.Date)
                                            .ToList();
                }
                else
                {
                    return context.Appointments.AsNoTracking().OrderBy(x => x.Date).ToList();
                }
            }
        }

        public List<Appointment> GetAppointmentsByCustomerProfile(string Business, int CustomerID, bool IsDeleted)
        {
            using (var context = new DSContext())
            {
                return context.Appointments.AsNoTracking().Where(x => x.Business == Business && x.CustomerID == CustomerID && x.DELETED == IsDeleted).OrderBy(x => x.Date).ToList();

            }
        }

        //public bool GetConflictingAppointmentIfAny(DateTime date, DateTime time,string Business)
        //{
        //    using (var context = new DSContext())
        //    {
        //        return context.Appointments.AsNoTracking().Any(a => a.Date.ToString("yyyy-MM-dd") == date.ToString("yyyy-MM-dd")
        //        && a.Time.ToString("HH:mm") == time.ToString("HH:mm")
        //        && a.Business == Business && a.IsCancelled == false && a.DELETED == false);
        //    }
        //}

        public Appointment GetAppointment(int ID)
        {
            using (var context = new DSContext())
            {

                return context.Appointments.Find(ID);

            }
        }

        public void SaveAppointment(Appointment Appointment)
        {
            using (var context = new DSContext())
            {
                context.Appointments.Add(Appointment);
                context.SaveChanges();
            }
        }

        public void UpdateAppointment(Appointment Appointment)
        {
            try
            {
                using (var context = new DSContext())
                {
                    context.Entry(Appointment).State = EntityState.Modified;
                    context.SaveChanges();
                }
            }
            catch (Exception ex)
            {

                throw;
            }

        }

        public void UpdateAppointmentNew(Appointment appointment)
        {
            try
            {
                using (var context = new DSContext())
                {
                    // Attach the entity to the context if it's not already being tracked
                    context.Appointments.Attach(appointment);

                    // Mark only the properties that need updating
                    context.Entry(appointment).Property(a => a.IsCancelled).IsModified = true;
                    context.Entry(appointment).Property(a => a.IsPaid).IsModified = true;
                    // Repeat for any other properties that need to be updated

                    context.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                // Log the exception here if needed
                throw;
            }
        }

        public void DeleteAppointment(int ID)
        {
            using (var context = new DSContext())
            {

                var Appointment = context.Appointments.Find(ID);
                context.Appointments.Remove(Appointment);
                context.SaveChanges();
            }
        }


        public string UpdateEvent(string id, string start, string end, int EmployeeID)
        {
            using (var context = new DSContext())
            {

                using (var transaction = context.Database.BeginTransaction())
                {
                    try
                    {
                        // Parse the event data received from FullCalendar
                        int eventId = Convert.ToInt32(id);
                        string[] parts = start.Split(' ');
                        string month = parts[1];
                        int day = int.Parse(parts[2]);
                        int year = int.Parse(parts[3]);
                        string time = parts[4];

                        // Parse the month abbreviation to a numerical value
                        int monthNumber = DateTime.ParseExact(month, "MMM", System.Globalization.CultureInfo.InvariantCulture).Month;

                        // Combine the extracted information into a format that can be parsed
                        string formattedDateTime = $"{year}-{monthNumber:D2}-{day:D2}T{time}";

                        // Define the format for parsing
                        string format = "yyyy-MM-ddTHH:mm:ss";

                        // Parse the formatted string to a DateTime object
                        DateTime startDate = DateTime.ParseExact(formattedDateTime, format, System.Globalization.CultureInfo.InvariantCulture);



                        string[] partsEnd = end.Split(' ');
                        string monthEnd = partsEnd[1];
                        int dayEnd = int.Parse(partsEnd[2]);
                        int yearEnd = int.Parse(partsEnd[3]);
                        string timeEnd = partsEnd[4];

                        // Parse the month abbreviation to a numerical value
                        int monthNumberEnd = DateTime.ParseExact(monthEnd, "MMM", System.Globalization.CultureInfo.InvariantCulture).Month;

                        // Combine the extracted information into a format that can be parsed
                        string formattedDateTimeEnd = $"{yearEnd}-{monthNumberEnd:D2}-{dayEnd:D2}T{timeEnd}";

                        // Define the format for parsing
                        string formatEnd = "yyyy-MM-ddTHH:mm:ss";

                        // Parse the formatted string to a DateTime object
                        DateTime endDate = DateTime.ParseExact(formattedDateTimeEnd, formatEnd, System.Globalization.CultureInfo.InvariantCulture);

                        // TODO: Perform database update here
                        var appointment = AppointmentServices.instance.GetAppointment(eventId);
                        //appointment.Date = startDate;
                        appointment.Time = startDate;
                        appointment.EndTime = endDate;
                        appointment.EmployeeID = EmployeeID;
                        AppointmentServices.instance.UpdateAppointment(appointment);
                        // If the update was successful, commit the transaction
                        transaction.Commit();

                        var response = "Success";

                        return response;
                    }
                    catch (Exception ex)
                    {
                        // Handle any exceptions that may occur during the update process
                        transaction.Rollback(); // Roll back the transaction

                        var response = "Fail";
                        return response;

                    }
                }
            }
        }




        public float GetCashDeposit(DateTime StartDate, DateTime EndDate, string Business, bool IsCancelled, List<int> SelectedEmployeeIds, List<string> AbsenseServiceIDs)
        {
            using (var context = new DSContext())
            {
                var totalDeposit = context.Appointments.AsNoTracking()
      .Where(x => x.Business == Business
                  && x.DELETED == false
                  && x.IsCancelled == IsCancelled
                  && x.DepositMethod == "Cash"
                  && SelectedEmployeeIds.Contains(x.EmployeeID)
                  && !AbsenseServiceIDs.Contains(x.Service)
 && DbFunctions.TruncateTime(x.Date) >= StartDate.Date
                                && DbFunctions.TruncateTime(x.Date) <= StartDate.Date)
      .Select(X => X.Deposit);
                if (totalDeposit.Any())
                {
                    // If there are matching appointments, calculate the sum
                    return totalDeposit.Sum();
                }
                else
                {
                    // If there are no matching appointments, return 0
                    return 0;
                }
            }
        }

        public float GetOnlineDeposit(DateTime StartDate, DateTime EndDate, string Business, bool IsCancelled, List<int> SelectedEmployeeIds, List<string> AbsenseServiceIDs)
        {
            using (var context = new DSContext())
            {
                var totalDeposit = context.Appointments.AsNoTracking()
      .Where(x => x.Business == Business
                  && x.DELETED == false
                  && x.IsCancelled == IsCancelled
                  && x.DepositMethod == "Online"
                  && SelectedEmployeeIds.Contains(x.EmployeeID)
                  && !AbsenseServiceIDs.Contains(x.Service)
 && DbFunctions.TruncateTime(x.Date) >= StartDate.Date
                                && DbFunctions.TruncateTime(x.Date) <= StartDate.Date).Select(X => X.Deposit);


                if (totalDeposit.Any())
                {
                    // If there are matching appointments, calculate the sum
                    return totalDeposit.Sum();
                }
                else
                {
                    // If there are no matching appointments, return 0
                    return 0;
                }
            }
        }
        public float GetPinDeposit(DateTime StartDate, DateTime EndDate, string Business, bool IsCancelled, List<int> SelectedEmployeeIds, List<string> AbsenseServiceIDs)
        {
            using (var context = new DSContext())
            {

                var totalDeposit = context.Appointments.AsNoTracking()
                 .Where(x => x.Business == Business
                  && x.DELETED == false
                  && x.IsCancelled == IsCancelled
                  && x.DepositMethod == "Pin"
                  && SelectedEmployeeIds.Contains(x.EmployeeID)
                  && !AbsenseServiceIDs.Contains(x.Service)
                     && DbFunctions.TruncateTime(x.Date) >= StartDate.Date
                                && DbFunctions.TruncateTime(x.Date) <= StartDate.Date)
                 .Select(X => X.Deposit);
                if (totalDeposit.Any())
                {
                    // If there are matching appointments, calculate the sum
                    return totalDeposit.Sum();
                }
                else
                {
                    // If there are no matching appointments, return 0
                    return 0;
                }

            }
        }

        public bool GetAppointmentBookingWRTBusinessNEW2(string company, bool Deleted, bool isCancelled, DateTime DaysAgo, int CustomerID)
        {
            using (var context = new DSContext())
            {

                return context.Appointments.AsNoTracking().Where(x => x.Business == company && x.DELETED == Deleted && x.CustomerID == CustomerID && x.IsCancelled == isCancelled && x.Date >= DaysAgo).Any();

            }
        }

        public bool GetAppointmentBookingWRTBusinessNEW2(string company, bool Deleted, bool isCancelled, int CustomerID)
        {
            using (var context = new DSContext())
            {

                return context.Appointments.AsNoTracking().Where(x => x.Business == company && x.DELETED == Deleted && x.IsCancelled == isCancelled && x.CustomerID == CustomerID && x.Date > DateTime.Now).Any();

            }
        }


        public List<int> GetLostClients(string Company, bool Deleted, bool IsCancelled, DateTime FirstLimit, int DaysBehindLimit, List<int> customerIds)
        {
            using (var context = new DSContext())
            {
                // Calculate the date limit (FirstLimit minus DaysBehindLimit days)
                DateTime dateLimit = FirstLimit.AddDays(-DaysBehindLimit);

                // Query to get lost clients based on the specified conditions
                var lostClients = context.Appointments
                    .Where(a => a.Business == Company &&
                                a.DELETED == Deleted &&
                                a.IsCancelled == IsCancelled &&
                                customerIds.Contains(a.CustomerID)) // Only include specified customer IDs
                    .GroupBy(a => a.CustomerID)
                    .Where(g => g.Max(a => a.Date) < FirstLimit &&         // Last appointment before FirstLimit
                                g.Max(a => a.Date) >= dateLimit &&         // Last appointment within the date range
                                !g.Any(a => a.Date >= FirstLimit))         // No future or current appointments from FirstLimit onwards
                    .Select(g => g.Key)                                    // Select the CustomerID
                    .ToList();

                return lostClients;
            }
        }


        public List<int> GetAppointmentBookingWRTBusinessNEW(string company, bool Deleted, bool isCancelled)
        {
            using (var context = new DSContext())
            {
                return context.Appointments.AsNoTracking()
                    .Where(x => x.Business == company &&
                                x.DELETED == Deleted &&
                                x.IsCancelled == isCancelled &&
                                x.Date >= DateTime.Now)
                    .Select(x => x.CustomerID)
                    .Distinct() // Optional: to ensure unique CustomerIDs
                    .ToList();
            }
        }

        //public List<int> GetLostClients(string company, bool Deleted, bool isCancelled, DateTime DaysAgo)
        //{
        //    using (var context = new DSContext())
        //    {
        //        // Get a list of customer IDs who had appointments before 30 days ago and have no recent appointments.
        //        var customersWithPastAppointments = context.Appointments
        //            .AsNoTracking()
        //            .Where(x => x.Business == company
        //                        && x.DELETED == Deleted
        //                        && x.IsCancelled == isCancelled
        //                        && x.Date >= DaysAgo)
        //            .Select(x => x.CustomerID)
        //            .Distinct()
        //            .ToList();

        //        // Return the list of customers who do not have any future appointments
        //        var lostCustomers = customersWithPastAppointments.Where(customerID =>
        //            !context.Appointments
        //            .AsNoTracking()
        //            .Where(x => x.Business == company
        //                        && x.DELETED == Deleted
        //                        && x.IsCancelled == isCancelled
        //                        && x.CustomerID == customerID
        //                        && x.Date > DateTime.Now)
        //            .Any()
        //        ).ToList();

        //        return lostCustomers;
        //    }
        //}




        public List<Appointment> GetAllAppointmentWRTBusiness(string company, bool isDeleted, int employeeID, DateTime startDate, DateTime endDate, bool IsCancelled)
        {
            using (var context = new DSContext())
            {
                return context.Appointments.AsNoTracking()
                    .Where(x => x.Business == company
                                && x.DELETED == isDeleted
                                && x.IsCancelled == IsCancelled
                                && x.EmployeeID == employeeID
                                && DbFunctions.TruncateTime(x.Date) >= startDate.Date
                                && DbFunctions.TruncateTime(x.Date) <= endDate.Date)
                    .ToList();
            }
        }
        public List<Appointment> GetAllAppointmentWRTBusiness(DateTime startDate, DateTime endDate, string company, bool isCancelled, List<int> selectedEmployees)
        {
            using (var context = new DSContext())
            {

                return context.Appointments.AsNoTracking().Where(x => x.Business == company && x.DELETED == false && x.IsCancelled == isCancelled && selectedEmployees.Contains(x.EmployeeID) && x.Color.Trim() != "darkgray" && DbFunctions.TruncateTime(x.Date) >= startDate.Date && DbFunctions.TruncateTime(x.Date) <= endDate.Date).ToList();

            }

        }
    }
}
