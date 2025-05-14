using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using TheBookingPlatform.Entities;

namespace TheBookingPlatform.ViewModels
{
    public class GiftCardListingViewModel
    {
        public List<GiftCard> GiftCards { get; set; }
        public Company Company { get; set; }
        public string SearchTerm { get; set; }
    }

    public class GiftCardActionViewModel
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public string Code { get; set; }
        public bool IsActive { get; set; }
        public DateTime Date { get; set; }
        public int Days { get; set; }
        public string GiftCardAmount { get; set; }
        public string GiftCardImage { get; set; }

    }

    public class GiftCardAssignmentListingViewModel
    {
        public List<GiftCardAssignmentModel> GiftCardAssignments { get; set; }
    }

    public class GiftCardAssignmentModel
    {
        public GiftCard GiftCard { get; set; }
        public Customer Customer { get; set; }
        public GiftCardAssignment GiftCardAssignment { get; set; }
    }


    public class GiftCardHistoryViewModel
    {
        public List<History> Histories { get; set; }
    }
    public class GiftCardAssignmentActionViewModel
    {
        public List<GiftCard> GiftCards { get; set; }
        public int ID { get; set; }
        public int GiftCardID { get; set; }
        public GiftCard GiftCard { get; set; }
        public int CustomerID { get; set; }
        public Customer Customer { get; set; }
        public DateTime AssignedDate { get; set; }
        public float Balance { get; set; }
        public float AssignedAmount { get; set; }

        public List<Customer> Customers { get;  set; }
    }
}