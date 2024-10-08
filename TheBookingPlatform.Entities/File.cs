using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheBookingPlatform.Entities
{
    public class File:BaseEntity
    {
        public string Name { get; set; }
        public string URL { get; set; }
        public string Size { get; set; }
        public string UploadedBy { get; set; }
        public DateTime DateTime { get; set; }
        public int CustomerID { get; set; }
    }
}
