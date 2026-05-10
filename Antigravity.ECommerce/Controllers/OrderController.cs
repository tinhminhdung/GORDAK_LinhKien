using Microsoft.AspNetCore.Mvc;
using Antigravity.ECommerce.Services;
using System.Linq;

namespace Antigravity.ECommerce.Controllers
{
    public class OrderController : Controller
    {
        [HttpGet]
        public IActionResult Tracking(string phone, string orderCode)
        {
            var data = SOrder.Search(null, null, null, null, null, null, null, null, "CreatedAt", "DESC", 1, 500);
            if (!string.IsNullOrEmpty(phone) && !string.IsNullOrEmpty(orderCode))
            {
                var order = data.FirstOrDefault(o => o.CustomerPhone == phone && o.OrderCode == orderCode);
                ViewBag.OrderResult = order;
                ViewBag.Searched = true;
            }
            return View();
        }
    }
}
