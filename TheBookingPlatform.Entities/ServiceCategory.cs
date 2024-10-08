using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheBookingPlatform.Entities
{
    public class ServiceCategory:BaseEntity
    {
        public string Name { get; set; }
        public int DisplayOrder { get; set; }
    }
}
