using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheBookingPlatform.Entities
{
    public class GoogleCalendarIntegration:BaseEntity
    {
        public string ClientID { get; set; }
        public string ApiKEY { get; set; }
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }
        public bool Disabled { get; set; }
        public bool InitialSetup { get; set; }

        public string TypeOfIntegration { get; set; } //All In One or Separate for each employee
        public DateTime ExpirationDate { get; set; }
    }
}
