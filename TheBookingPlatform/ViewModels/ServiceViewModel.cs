using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using TheBookingPlatform.Entities;

namespace TheBookingPlatform.ViewModels
{
  

    public class ServiceListingViewModel
    {
        public ServiceModelInServices DeletedServices { get; set; }

        public List<ServiceModel> Services { get; set; }
        public string SearchTerm { get; set; }
        public string ServiceCategory { get; set; }

    }

    public class ServiceModel
    {
        public List<Service> Services { get; set; }
        public Company Company { get; set; }
        public ServiceCategory ServiceCategory { get; set; }
    
    }

    public class ServiceModelFORBK
    {
        public Service Service { get; set; }
        public bool BestSeller { get; set; }
        public string Type { get; set; }
    }

    public class ServiceModelForBooking
    {
        public List<ServiceModelFORBK> Services { get; set; }
        public Company Company { get; set; }
        public ServiceCategory ServiceCategory { get; set; }
    }
    public class ServiceModelInServices
    {
        public List<Service> Services { get; set; }
        public Company Company { get; set; }
        public string ServiceCategory { get; set; }
        public int DisplayOrder { get; set; }
        public int ID { get; set; }
    }
    public class ServicePriceListModel
    {
        public List<Service> Services { get; set; }
        public string Currency { get; set; }
        public ServiceCategory ServiceCategory { get; set; }
    }
    public class ServiceActionViewModel
    {
        public int ID { get; set; }
        public string Business { get; set; }
        public string Name { get; set; }
        public bool IsActive { get; set; }
        public string Category { get; set; }
        public List<ServiceCategory> ServiceCategories { get; set; }
        public float Price { get; set; }
        public string VAT { get; set; }
        public List<Vat> Vats { get; set; }

        //If the Services Doesn't Require Processing Time
        public string Duration { get; set; }

        //If the Service Required Processing Time
        public string Setup { get; set; }
        public string Processing { get; set; }
        public string Finish { get; set; }

        /// <summary>
        /// ////////////////
        /// </summary>
        public string BufferTime { get; set; }

        public string ServiceEmployee { get; set; } //Will Be Send to View 
        public string Employee { get; set; }   // Will be retrieved from View
        public List<Employee> Employees { get; set; }


        //If Tools Needed
        public string Tool { get; set; }
        public List<Resource> Tools { get; set; } //Tool Type

        //If Room Needed
        public string Room { get; set; }
        public List<Resource> Rooms { get; set; } //Room Type

        //Addtional Notes
        public string Notes { get; set; }

        //OnlineBooking
        public bool CanBookOnline { get; set; }
        public bool DoesRequiredProcessing { get;  set; }
        public int NumberofSessions { get;  set; }
        public bool AddOn { get;  set; }
        public float PromoPrice { get;  set; }
    }
}