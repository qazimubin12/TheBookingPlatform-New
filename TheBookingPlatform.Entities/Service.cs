using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheBookingPlatform.Entities
{
    public class Service : BaseEntity
    {
        public string Name { get; set; }
        public bool AddOn { get; set; }
        public string Category { get; set; }
        public float Price { get; set; }
        public string VAT { get; set; }

        //If the Services Doesn't Require Processing Time
        public string Duration { get; set; }

        //If the Service Required Processing Time
        public bool DoesRequiredProcessing { get; set; }
        public string Setup { get; set; }
        public string Processing { get; set; }
        public string Finish { get; set; }
        public float PromoPrice { get; set; }
        public int DisplayOrder { get; set; }
        /// <summary>
        /// ////////////////
        /// </summary>


        //If Tools Needed
        public string Tool { get; set; }

        //If Room Needed
        public string Room { get; set; }


        //Addtional Notes
        public string Notes { get; set; }

        //OnlineBooking
        public bool CanBookOnline { get; set; }

        public bool IsActive { get; set; }

        public int NumberofSessions { get; set; }
    }
}
