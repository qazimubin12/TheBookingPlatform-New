using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using TheBookingPlatform.Entities;

namespace TheBookingPlatform.ViewModels
{
    public class FranchiseRequestListingViewModel
    {
        public List<FranchiseRequestModel> FranchiseRequests { get; set; }
        public string LoggedInCompany { get; set; }
    }

    public class FranchiseRequestModel
    {
        public FranchiseRequest FranchiseRequest { get; set; }
        public string User { get; set; }
        public string CompanyFrom { get; set; }
        public string CompanyFor { get; set; }

    }

    public class FranchiseRequestActionViewModel
    {
        public List<User> Users { get; set; }
        public List<Company> Companies { get; set; } //Child Companies for that Logged In User Company
        public int ID { get; set; }
        public string UserID { get; set; }
        public string MappedToUserID { get; set; }

        public bool Accepted { get; set; }
        public string Status { get; set; }
        public int CompanyIDFrom { get; set; } 
        public int CompanyIDFor { get; set; } //Logged In User Company
    }
}