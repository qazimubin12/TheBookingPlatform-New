using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using TheBookingPlatform.Entities;

namespace TheBookingPlatform.ViewModels
{
    public class CustomerListingViewModel
    {
        public List<CustomerModel> Customers { get; set; }
        public string SearchTerm { get; set; }
    }

    public class CustomerModel
    {
        public int ID { get; set; }
        public string FirstName { get; set; }
        public bool IsBlocked { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string MobileNumber { get; set; }
    }

    public class CustomerActionViewModel:BaseViewModel
    {
        public string Business { get; set; }

        public int ID { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Gender { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string Email { get; set; }
        public string MobileNumber { get; set; }
        public string Address { get; set; }
        public int PostalCode { get; set; }
        public string City { get; set; }
        public string ProfilePicture { get; set; }



        public string AdditionalInformation { get; set; }
        public string AdditionalInvoiceInformation { get; set; }
        public string WarningInformation { get; set; }
        public bool HaveNewsLetter { get; set; }

        //public List<Form> Forms { get; set; }
        //public List<FormSubmission> FormSubmissions { get; set; }

        //public List<Invoice> Invoices { get; set; }
        //public List<Log> Logs { get; set; }
        //public List<Appointment> Appointments { get; set; }

    }
}