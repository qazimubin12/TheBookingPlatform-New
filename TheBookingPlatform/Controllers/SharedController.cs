using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace TheBookingPlatform.Controllers
{
    public class SharedController : Controller
    {
        // GET: Shared
        public JsonResult UploadImage()
        {
            JsonResult result = new JsonResult();
            result.JsonRequestBehavior = JsonRequestBehavior.AllowGet;
            try
            {
                var file = Request.Files[0];
                var fileNameWithDate = DateTime.Now.ToString("dd-MM-yyyy-HH-mm-ss-ffffff") + file.FileName.Replace(" ", "");
                var fileName = Path.GetFileNameWithoutExtension(file.FileName.Replace(" ","")); // Get the file name without extension
                var fileExtension = Path.GetExtension(file.FileName.Replace(" ",""));
                var fileSize = file.ContentLength / 1024; // File size in kilobytes

                var directoryPath = Server.MapPath("~/Images/");
                var path = Path.Combine(directoryPath, fileNameWithDate);

                if (!Directory.Exists(directoryPath))
                {
                    Directory.CreateDirectory(directoryPath); // Create the directory if it doesn't exist
                }
                file.SaveAs(path);

                result.Data = new { Success = true, FileName = fileName, FileSizeKB = fileSize, DocURL = string.Format("/Images/{0}", fileNameWithDate) };
            }
            catch (Exception ex)
            {
                result.Data = new { Success = false, Message = ex.Message };
                throw;
            }
            return result;
        }




    }
}