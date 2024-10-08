using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheBookingPlatform.Entities
{
    public class Review:BaseEntity
    {
        public int CustomerID { get; set; }
        public float Rating { get; set; }
        public int EmployeeID { get; set; }
        public DateTime Date { get; set; }
        public string Feedback { get; set; }
        public int AppointmentID { get; set; }
        public bool FeedbackReminder { get; set; }
        public bool EmailOpened { get; set; }
        public bool EmailClicked { get; set; }

    }
}
