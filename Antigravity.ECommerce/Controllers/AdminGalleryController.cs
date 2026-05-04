using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Antigravity.ECommerce.Models;
using Antigravity.ECommerce.Services;
using System.Linq;
using System.Collections.Generic;
using Antigravity.ECommerce.Framework;

namespace Antigravity.ECommerce.Controllers
{
    [Authorize]
    [Permission("Gallery", ActionType.View)]
    public class AdminGalleryController : Controller
    {
        private void LoadCategories()
        {
            ViewBag.Categories = SCategory.GetHierarchical().Where(x => x.CategoryType == 6).ToList();
        }

        public IActionResult Index(string kw = "", int? status = null, int? categoryId = null, string sort = "CreatedAt", string order = "DESC", int page = 1, int size = 20)
        {
            if (page < 1) page = 1;
            var list = SGallery.Search(kw, status, categoryId, sort, order, page, size);
            ViewBag.SortColumn = sort;
            ViewBag.SortOrder = order;
            LoadCategories();
            ViewBag.Keyword = kw;
            ViewBag.Status = status;
            ViewBag.CategoryId = categoryId;
            ViewBag.PageIndex = page;
            ViewBag.PageSize = size;
            ViewBag.TotalCount = list.Count > 0 ? list[0].TotalCount : 0;
            return View(list);
        }


        [Permission("Gallery", ActionType.Create)]
        public IActionResult Create()
        {
            LoadCategories();
            return View(new Gallery { SortOrder = 0, Status = 1 });
        }

        [HttpPost]
        [Permission("Gallery", ActionType.Create)]
        public IActionResult Create(Gallery model)
        {
            if (ModelState.IsValid)
            {
                model.CreatedBy = User.Identity?.Name ?? "Admin";
                int galleryId = SGallery.Insert(model);
                
                // Nếu có ảnh được truyền qua dạng list ẩn, lưu vào GalleryImages
                var imagePaths = Request.Form["GalleryImages"].ToList();
                if (imagePaths.Any())
                {
                    SGalleryImage.BulkInsert(galleryId, imagePaths);
                }

                SSeo.RefreshSitemapAndCache();
                return RedirectToAction("Index");
            }
            LoadCategories();
            return View(model);
        }


        [Permission("Gallery", ActionType.Edit)]
        public IActionResult Edit(int id)
        {
            var item = SGallery.GetById(id);
            if (item == null) return NotFound();
            LoadCategories();
            return View(item);
        }

        [HttpPost]
        [Permission("Gallery", ActionType.Edit)]
        public IActionResult Edit(Gallery model)
        {
            if (ModelState.IsValid)
            {
                model.UpdatedBy = User.Identity?.Name ?? "Admin";
                SGallery.Update(model);
                SSeo.RefreshSitemapAndCache();
                return RedirectToAction("Index");
            }
            LoadCategories();
            return View(model);
        }

        [HttpPost]
        [Permission("Gallery", ActionType.Delete)]
        public IActionResult Delete(int id)
        {
            var result = SGallery.Delete(id);
            if (result > 0) {
                SSeo.RefreshSitemapAndCache();
                return Json(new { success = true });
            }
            return Json(new { success = false, message = "Xóa thất bại" });
        }

        [HttpPost]
        [Permission("Gallery", ActionType.Delete)]
        public IActionResult BulkDelete([FromBody] List<int> ids)
        {
            if (ids == null || ids.Count == 0) return Json(new { success = false });
            int result = SGallery.BulkDelete(ids);
            if (result > 0) SSeo.RefreshSitemapAndCache();
            return Json(new { success = result > 0 });
        }

        [HttpPost]
        [Permission("Gallery", ActionType.Edit)]
        public IActionResult BulkUpdateStatus([FromBody] BulkUpdateModel model)
        {
            if (model.Ids == null || model.Ids.Count == 0) return Json(new { success = false });
            int result = SGallery.BulkUpdateStatus(model.Ids, model.Status);
            if (result > 0) SSeo.RefreshSitemapAndCache();
            return Json(new { success = result > 0 });
        }

        public class BulkUpdateModel
        {
            public List<int> Ids { get; set; } = new();
            public int Status { get; set; }
        }

        // ==========================================
        // AJAX ENDPOINTS CHO GALLERY IMAGES DYNAMIC
        // ==========================================

        public class AddImagesModel
        {
            public int GalleryId { get; set; }
            public List<string> Urls { get; set; } = new();
        }

        public class ReorderImagesModel
        {
            public int GalleryId { get; set; }
            public List<int> ImageIds { get; set; } = new();
        }

        [HttpPost]
        [Permission("Gallery", ActionType.Edit)]
        public IActionResult AddImages([FromBody] AddImagesModel model)
        {
            if (model.GalleryId <= 0 || model.Urls == null || !model.Urls.Any()) return Json(new { success = false });
            int count = SGalleryImage.BulkInsert(model.GalleryId, model.Urls);
            return Json(new { success = count > 0, count = count });
        }

        [HttpPost]
        [Permission("Gallery", ActionType.Edit)]
        public IActionResult RemoveImage(int imageId)
        {
            int result = SGalleryImage.Delete(imageId);
            return Json(new { success = result > 0 });
        }

        [HttpPost]
        [Permission("Gallery", ActionType.Edit)]
        public IActionResult ReorderImages([FromBody] ReorderImagesModel model)
        {
            if (model.GalleryId <= 0 || model.ImageIds == null) return Json(new { success = false });
            SGalleryImage.Reorder(model.GalleryId, model.ImageIds);
            return Json(new { success = true });
        }

        [HttpPost]
        [Permission("Gallery", ActionType.Edit)]
        public IActionResult UpdateImageCaption(int imageId, string caption)
        {
            int result = SGalleryImage.UpdateCaption(imageId, caption);
            return Json(new { success = result > 0 });
        }
    }
}
