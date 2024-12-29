using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheBookingPlatform.Entities
{
    public class Notification:BaseEntity
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public string Link { get; set; }
        public string Code { get; set; }
        public DateTime Date { get; set; }
        //public string ReadByUsers { get; set; }
    }
}
