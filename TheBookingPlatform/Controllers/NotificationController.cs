using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TheBookingPlatform.Services;
using TheBookingPlatform.ViewModels;

namespace TheBookingPlatform.Controllers
{
    public class NotificationController : Controller
    {
        // GET: Notification
        public ActionResult Index()
        {
            NotificatonListingViewModel model = new NotificatonListingViewModel();
            model.Notifications = NotificationServices.Instance.GetNotification();
            return View(model);
        }

        [HttpGet]
        public ActionResult Action(int ID = 0)
        {
            NotificationActionViewModel model = new NotificationActionViewModel();
            model.Companies = CompanyServices.Instance.GetCompany();
            if(ID != 0)
            {
                var notification = NotificationServices.Instance.GetNotification(ID);
                model.Title = notification.Title;
                model.Link = notification.Link; 
            }
            return View(model);
        }
    }
}