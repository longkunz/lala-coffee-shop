using System;
using System.IO;
using System.Web;
using System.Web.Mvc;
using LalaCoffee.Helpers;

namespace LalaCoffee.Controllers
{
    [Authorize(Roles = "Admin")]
    public class ImageController : Controller
    {
        [HttpPost]
        public ActionResult ProcessUpload(HttpPostedFileBase file)
        {
            if (file == null || file.ContentLength <= 0)
            {
                return new HttpStatusCodeResult(400, "Vui lòng chọn file tải lên.");
            }

            string rawFileName = Path.GetFileName(file.FileName);
            if (!ImageUploadValidator.IsValidExtension(rawFileName))
            {
                return new HttpStatusCodeResult(400, "Định dạng file không được phép. Chỉ chấp nhận JPG, JPEG, PNG, WEBP, GIF.");
            }

            if (!ImageUploadValidator.IsValidSize(file.ContentLength))
            {
                return new HttpStatusCodeResult(400, "Dung lượng file vượt quá giới hạn cho phép (tối đa 5MB).");
            }

            string uploadDir = Server.MapPath("~/Content/images/");
            if (!Directory.Exists(uploadDir))
            {
                Directory.CreateDirectory(uploadDir);
            }

            string safeFileName = ImageUploadValidator.GenerateSafeFileName(rawFileName);
            string fullPath = Path.Combine(uploadDir, safeFileName);
            file.SaveAs(fullPath);

            return Content("/Content/images/" + safeFileName);
        }
    }
}
