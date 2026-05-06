using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Antigravity.ECommerce.Models;
using Antigravity.ECommerce.Services;
using Antigravity.ECommerce.Framework;

namespace Antigravity.ECommerce.Controllers
{
    [Authorize]
    [Permission("Categories", ActionType.View)] // Sử dụng quyền chung của danh mục hoặc tạo mới nếu cần
    public class AdminProductOptionController : Controller
    {
        public IActionResult Index(int type = 1)
        {
            var options = SProductOption.GetAll().Where(x => x.Type == type).OrderBy(x => x.SortOrder).ToList();
            ViewBag.Type = type;
            
            // Set title based on type
            ViewBag.PageTitle = type == 1 ? "Thương hiệu" : (type == 2 ? "Tình trạng" : "Bảo hành");
            
            return View(options);
        }

        [Permission("Categories", ActionType.Create)]
        public IActionResult Create(int type = 1)
        {
            ViewBag.Type = type;
            ViewBag.PageTitle = type == 1 ? "Thương hiệu" : (type == 2 ? "Tình trạng" : "Bảo hành");
            return View(new ProductOption { Type = type, Status = 1, SortOrder = 0 });
        }

        [HttpPost]
        [Permission("Categories", ActionType.Create)]
        public IActionResult Create(ProductOption model)
        {
            if (ModelState.IsValid)
            {
                SProductOption.Insert(model);
                return RedirectToAction("Index", new { type = model.Type });
            }
            return View(model);
        }

        [Permission("Categories", ActionType.Edit)]
        public IActionResult Edit(int id)
        {
            var option = SProductOption.GetById(id);
            if (option == null) return NotFound();
            
            ViewBag.Type = option.Type;
            ViewBag.PageTitle = option.Type == 1 ? "Thương hiệu" : (option.Type == 2 ? "Tình trạng" : "Bảo hành");
            
            return View(option);
        }

        [HttpPost]
        [Permission("Categories", ActionType.Edit)]
        public IActionResult Edit(ProductOption model)
        {
            if (ModelState.IsValid)
            {
                SProductOption.Update(model);
                return RedirectToAction("Index", new { type = model.Type });
            }
            return View(model);
        }

        [HttpPost]
        [Permission("Categories", ActionType.Delete)]
        public IActionResult Delete(int id)
        {
            try
            {
                SProductOption.Delete(id);
                return Json(new { success = true });
            }
            catch (System.Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
    }
}
