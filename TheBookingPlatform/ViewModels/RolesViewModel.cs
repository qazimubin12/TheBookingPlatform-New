using Microsoft.AspNet.Identity.EntityFramework;
using System.Collections.Generic;


namespace TheBookingPlatform.ViewModels
{
    public class RoleListingViewModel
    {
        public IEnumerable<IdentityRole> Roles { get; set; }
        public string SearchTerm { get; set; }
    }

    public class RoleActionViewModel
    {
        public string ID { get; set; }
        public string Name { get; set; }
    }
}