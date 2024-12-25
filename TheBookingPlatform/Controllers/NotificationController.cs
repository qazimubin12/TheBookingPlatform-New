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
            var notifications = NotificationServices.Instance.GetNotification();
            var listofNotifications = new List<NotificationModel>();
            foreach (var item in notifications)
            {
                if (listofNotifications.Select(x => x.Code).Contains(item.Code) == false)
                {
                    listofNotifications.Add(new NotificationModel { Code = item.Code, Link = item.Link, Title = item.Title,Date = item.Date.ToString("yyyy-MM-dd HH:mm") });
                }
            }
            model.Notifications = listofNotifications;
            return View(model);
        }

        [HttpGet]
        public ActionResult Action(string Code = "")
        {
            NotificationActionViewModel model = new NotificationActionViewModel();
            if(Code != "")
            {
                var notification = NotificationServices.Instance.GetNotificationWRTBusiness(Code).FirstOrDefault();
                model.Title = notification.Title;
                model.Code = notification.Code;
                model.Link = notification.Link; 
            }
            return View(model);
        }

        [HttpPost]
        public JsonResult Action(NotificationActionViewModel model)
        {
            var companies = CompanyServices.Instance.GetCompany();
            var code = GenerateRandomCode(8);
            foreach (var item in companies)
            {

                var notification = new Notification();
                notification.Business = item.Business;
                notification.Title = model.Title;
                notification.Link = model.Link;
                notification.Date = DateTime.Now;
                notification.Code = code;
                NotificationServices.Instance.SaveNotification(notification);

            }
            return Json(new { success = true }, JsonRequestBehavior.AllowGet);
        }

        static string GenerateRandomCode(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            var random = new Random();
            var code = new char[length];

            for (int i = 0; i < length; i++)
            {
                code[i] = chars[random.Next(chars.Length)];
            }

            return new string(code);
        }
        [HttpPost]
        public JsonResult UpdateNotification(NotificationActionViewModel model)
        {
          
            var companies = CompanyServices.Instance.GetCompany();
            var notifications = NotificationServices.Instance.GetNotificationWRTBusiness(model.Code);
            foreach (var notification in notifications)
            {
                notification.Title = model.Title;
                notification.Link = model.Link;
                notification.Date = DateTime.Now;
                NotificationServices.Instance.UpdateNotification(notification);

            }
            return Json(new { success = true }, JsonRequestBehavior.AllowGet);

        }


        [HttpGet]
        public ActionResult Delete(string Code)
        {
            NotificationActionViewModel model = new NotificationActionViewModel();
            model.Code = Code;
            return PartialView("_Delete", model);
        }


        [HttpPost]
        public ActionResult Delete(NotificationActionViewModel model)
        {
            var notifications = NotificationServices.Instance.GetNotificationWRTBusiness(model.Code);
            foreach (var notification in notifications)
            {
                NotificationServices.Instance.DeleteNotification(notification.ID);
            }

            return Json(new { success = true }, JsonRequestBehavior.AllowGet);
        }
    }
}