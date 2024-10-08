using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TheBookingPlatform.Entities;
namespace TheBookingPlatform.Entities
{
    public class GiftCard:BaseEntity
    {
        public string Name { get; set; }
        public string Code { get; set; }
        public int Days { get; set; }
        public bool IsActive { get; set; }
        public DateTime Date { get; set; }
        public string GiftCardAmount { get; set; }
        public string GiftCardImage { get; set; }

    }
}
