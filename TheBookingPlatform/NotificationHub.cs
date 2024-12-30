using Microsoft.AspNet.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TheBookingPlatform
{
    public class NotificationHub:Hub
    {
        public void Send(string message)
        {
            // Broadcast message to all connected clients
            Clients.All.receiveNotification(message);
        }
    }
}