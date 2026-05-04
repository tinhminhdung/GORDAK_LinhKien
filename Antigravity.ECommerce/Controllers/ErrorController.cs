using Microsoft.AspNetCore.Mvc;
using Antigravity.ECommerce.Services;
using Antigravity.ECommerce.Models;

namespace Antigravity.ECommerce.Controllers
{
    public class ErrorController : Controller
    {
        [Route("Error/{statusCode}")]
        public IActionResult HttpStatusCodeHandler(int statusCode)
        {
            var settings = SSetting.GetViewModel();
            string message = "Đã có lỗi xảy ra!";
            string title = "Lỗi " + statusCode;

            switch (statusCode)
            {
                case 404:
                    title = "Trang không tồn tại";
                    message = !string.IsNullOrEmpty(settings.Error404Message) ? settings.Error404Message : "Rất tiếc, trang bạn đang tìm kiếm không tồn tại hoặc đã bị di dời.";
                    break;
                case 500:
                    title = "Lỗi máy chủ";
                    message = !string.IsNullOrEmpty(settings.Error500Message) ? settings.Error500Message : "Hệ thống đang gặp sự cố kỹ thuật, vui lòng quay lại sau ít phút.";
                    break;
                case 403:
                    title = "Từ chối truy cập";
                    message = !string.IsNullOrEmpty(settings.Error403Message) ? settings.Error403Message : "Bạn không có quyền truy cập vào khu vực này.";
                    break;
            }

            ViewBag.Title = title;
            ViewBag.Message = message;
            ViewBag.StatusCode = statusCode;

            return View("Index");
        }

        [Route("Maintenance")]
        public IActionResult Maintenance()
        {
            var settings = SSetting.GetViewModel();
            if (!settings.MaintenanceMode) return Redirect("/");

            ViewBag.Title = "Hệ thống đang bảo trì";
            ViewBag.Message = !string.IsNullOrEmpty(settings.MaintenanceMessage) ? settings.MaintenanceMessage : "Website đang được nâng cấp để phục vụ bạn tốt hơn. Hẹn gặp lại sớm!";
            
            return View("Index");
        }
    }
}
