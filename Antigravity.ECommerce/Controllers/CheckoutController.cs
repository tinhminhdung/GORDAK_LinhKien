using Microsoft.AspNetCore.Mvc;
using Antigravity.ECommerce.Services;
using Antigravity.ECommerce.Models;

namespace Antigravity.ECommerce.Controllers
{
    /// <summary> Trang thanh toán đơn hàng - Sử dụng LocationController để lấy data tỉnh/huyện/xã </summary>
    public class CheckoutController : Controller
    {
        public IActionResult Index()
        {
            ViewBag.Provinces = BaseConnectionSql.Query<ProvinceData>("SELECT ProvinceId, Name FROM Provinces ORDER BY Name", null);
            return View(new Order());
        }

        [HttpPost]
        public IActionResult Process(Order model)
        {
            // Xử lý lưu đơn hàng (Sẽ hoàn thiện ở Phase sau)
            return RedirectToAction("Success");
        }

        public IActionResult Success()
        {
            return View();
        }
    }

    public class ProvinceData
    {
        public int ProvinceId { get; set; }
        public string Name { get; set; } = string.Empty;
    }
}
