using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheBookingPlatform.Entities
{
    public class Resource:BaseEntity
    {
        public string Name { get; set; }
        public string Type { get; set; } //Tool or Room
        public string Availability { get; set; }
        public string Services { get; set; }
    }
}
