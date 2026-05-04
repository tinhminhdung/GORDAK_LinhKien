using Microsoft.AspNetCore.Mvc;
using Antigravity.ECommerce.Framework;
using Antigravity.ECommerce.Models;

namespace Antigravity.ECommerce.Controllers
{
    public class PageController : Controller
    {
        public IActionResult Detail(string slug)
        {
            var category = FCategory.GetBySlug(slug);
            if (category == null || category.CategoryType != 0)
            {
                return NotFound();
            }

            ViewData["Title"] = category.SeoTitle ?? category.Name;
            ViewData["Description"] = category.SeoDescription;
            
            return View(category);
        }
    }
}
