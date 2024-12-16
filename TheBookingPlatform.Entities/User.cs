using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace TheBookingPlatform.Entities
{
    public class User : IdentityUser
    {
        public string Name { get; set; }
        public int IntervalCalendar { get; set; } = 15;
        public string Password { get; set; }
        public string Role { get; set; }
        public string Company { get; set; }
        public bool IsActive { get; set; } = true;
        public bool IsInTrialPeriod { get; set; }
        public bool IsPaid { get; set; }
        public int Package { get; set; }
        public string LastPaymentDate { get; set; }
        public DateTime RegisteredDate { get; set; }
        public string ReadNotifications { get; set; }
        public async Task<ClaimsIdentity> GenerateUserIdentityAsync(UserManager<User> manager)
        {
            // Note the authenticationType must match the one defined in CookieAuthenticationOptions.AuthenticationType
            var userIdentity = await manager.CreateIdentityAsync(this, DefaultAuthenticationTypes.ApplicationCookie);
            userIdentity.AddClaim(new Claim("Name", this.Name));
            // Add custom user claims here
            return userIdentity;
        }
    }
}
