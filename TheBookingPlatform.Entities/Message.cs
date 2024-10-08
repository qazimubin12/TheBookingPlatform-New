using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheBookingPlatform.Entities
{
    public class Message:BaseEntity
    {
        public DateTime Date { get; set; }
        public string Type { get; set; }
        public string Status { get; set; }
        public string Subject { get; set; }
        public string Channel { get; set; }
        public string SentBy { get; set; }
        public int CustomerID { get; set; }
    }
}
