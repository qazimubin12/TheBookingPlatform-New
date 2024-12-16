using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TheBookingPlatform.Entities;
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
            if(ID != 0)
            {
                var notification = NotificationServices.Instance.GetNotification(ID);
                model.Title = notification.Title;
                model.Link = notification.Link; 
                model.ID = notification.ID;
            }
            return View(model);
        }

        [HttpPost]
        public JsonResult Action(NotificationActionViewModel model)
        {
            var companies = CompanyServices.Instance.GetCompany();
            foreach (var item in companies)
            {

                var notification = new Notification();
                notification.Business = item.Business;
                notification.Title = model.Title;
                notification.Link = model.Link;
                notification.Date = DateTime.Now;
                NotificationServices.Instance.SaveNotification(notification);

            }
            return Json(new { success = true }, JsonRequestBehavior.AllowGet);
        }


        [HttpPost]
        public JsonResult UpdateNotification(NotificationActionViewModel model)
        {
            var notification = NotificationServices.Instance.GetNotification(model.ID);
            notification.Title = model.Title;
            notification.Link = model.Link;
            NotificationServices.Instance.UpdateNotification(notification);
            return Json(new { success = true }, JsonRequestBehavior.AllowGet);

        }


        [HttpGet]
        public ActionResult Delete(int ID)
        {
            NotificationActionViewModel model = new NotificationActionViewModel();
            var Notification = NotificationServices.Instance.GetNotification(ID);
            model.ID = Notification.ID;
            return PartialView("_Delete", model);
        }


        [HttpPost]
        public ActionResult Delete(NotificationActionViewModel model)
        {
            var Notification = NotificationServices.Instance.GetNotification(model.ID);
            NotificationServices.Instance.DeleteNotification(Notification.ID);

            return Json(new { success = true }, JsonRequestBehavior.AllowGet);
        }
    }
}