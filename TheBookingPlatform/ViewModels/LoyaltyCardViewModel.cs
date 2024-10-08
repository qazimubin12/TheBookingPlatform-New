using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using TheBookingPlatform.Entities;

namespace TheBookingPlatform.ViewModels
{
    public class LoyaltyCardListingViewModel
    {
        public List<LoyaltyCardModel> LoyaltyCards { get; set; }
        public string SearchTerm { get; set; }
    }

    public class LoyaltyCardAssignmentViewModel
    {
        public List<LoyaltyCardAssignmentModel> LoyaltyCardAssignments { get; set; }
    }

    public class LoyaltyCardAssignmentModel
    {
        public string LoyaltyCardName { get; set; }
        public int LoyaltyCardDays { get; set; }
        public LoyaltyCardAssignment LoyaltyCardAssignment { get; set; }
        public Customer Customer { get; set; }
        public float LoyaltyCardUsage { get; set; }
    }
    public class LoyaltyCardModel
    {
        public LoyaltyCard LoyaltyCard { get; set; }
        public List<LoyaltyCardPromotion> LoyaltyCardPromotionsIndex { get; set; }
        public Dictionary<float, (int, List<Service>)> LoyaltyCardPromotions { get; set; }
    }



    public class LoyaltyCardIssueViewModel
    {
        public List<Customer> Customers { get; set; }

        public List<LoyaltyCard> LoyaltyCards { get; set; }
        public string CardNumber { get; set; }
        public int Customer { get; set; }
    }

    public class LoyaltyCardPromotionViewModel
    {
        public LoyaltyCard LoyaltyCard { get; set; }
        public List<int> AlreadyHaveServices { get; set; }
        public LoyaltyCardPromotion LoyaltyCardPromotion { get; set; }
        public List<Service> Services { get; set; }
        public float Percentage { get; set; }

        public int ID { get; set; }
    
    
    }

    public class LoyaltyCardAssignmentActionViewModel
    {
        public List<Customer> Customer { get; set; }
        public LoyaltyCard LoyaltyCard { get; set; }
        public int ID { get; set; }
        public int CustomerID { get; set; }
        public int LoyaltyCardID { get; set; }
        public string CardNumber { get; set; }
        public int Days { get; set; }
        public float CashBack { get; set; }
        public DateTime Date { get; set; }
    }

    public class LoyaltyCardActionViewModel
    {
        public List<Service> Services { get; set; }
        public List<int> AlreadyIncludedServices { get; set; }
        public int ID { get; set; }
        public int AssignmentID { get; set; }
        public string Name { get; set; }
        public int Days { get; set; }
        public string ServiceListAlready { get; set; }
        public bool IsActive { get; set; }
        public List<float> Percentages { get; set; }

        public List<LoyaltyCardPromotionActionModel> LoyaltyCardPromotions { get; set; }
    }

    public class LoyaltyCardPromotionActionModel
    {
        public int LoyaltyCardPromotionID { get; set; }
        public float Percentage { get; set; }
        public List<int> ServiceInts { get; set; }
        public List<Service> Services { get; set; }
    }
}