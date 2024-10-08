using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheBookingPlatform.Entities
{
    public class Package : BaseEntity
    {
        public string APIKEY { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public decimal VAT { get; set; }
        public int NoOfUsers { get; set; }
        public string Features { get; set; }
    }
}
