﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using TheBookingPlatform.Entities;

namespace TheBookingPlatform.ViewModels
{
    public class NotificatonListingViewModel
    {
        public List<Notification> Notifications { get; set; }
    }
    public class NotificationActionViewModel
    {
        public List<Company> Companies { get; set; }
        public int ID { get; set; }
        public string Link { get; set; }
        public string Title { get; set; }
        public bool ResetRead { get; set; }
    }
}