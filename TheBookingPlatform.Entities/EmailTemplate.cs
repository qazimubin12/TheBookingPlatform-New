using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheBookingPlatform.Entities
{
    public class EmailTemplate:BaseEntity
    {
        public string Name { get; set; }
        public string Duration { get; set; }  //The time after the notification is sent
        public string TemplateCode { get; set; }
        public bool IsActive { get; set; } = true;
    }
}
