using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheBookingPlatform.Entities
{
    public class Holiday:BaseEntity
    {
        public string Name { get; set; }
        public DateTime Date { get; set; }
    }
}
