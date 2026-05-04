using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Antigravity.ECommerce.Models;
using Antigravity.ECommerce.Services;
using Antigravity.ECommerce.Framework;

namespace Antigravity.ECommerce.Controllers
{
    [Authorize]
    [Permission("Advertising")]
    public class AdminAdvertisingController : Controller
    {
        public IActionResult Index(string kw = "", string position = "", int? status = null, string sort = "CreatedAt", string order = "DESC", int page = 1, int size = 20)
        {
            var list = SAdvertising.Search(kw, position, status, sort, order, page, size);
            ViewBag.SortColumn = sort;
            ViewBag.SortOrder = order;
            
            ViewBag.Keyword = kw;
            ViewBag.Position = position;
            ViewBag.Status = status;
            ViewBag.PageIndex = page;
            ViewBag.PageSize = size;
            ViewBag.TotalCount = list.Count > 0 ? list[0].TotalCount : 0;
            
            return View(list);
        }

        public IActionResult Create()
        {
            return View(new Advertising { SortOrder = 0, Status = 1 });
        }

        [HttpPost]
        public IActionResult Create(Advertising model)
        {
            if (ModelState.IsValid)
            {
                model.CreatedBy = User.Identity?.Name ?? "Admin";
                SAdvertising.Insert(model);
                SSeo.RefreshSitemapAndCache();
                return RedirectToAction("Index");
            }
            return View(model);
        }

        public IActionResult Edit(int id)
        {
            var item = SAdvertising.GetById(id);
            if (item == null) return NotFound();
            return View(item);
        }

        [HttpPost]
        public IActionResult Edit(Advertising model)
        {
            if (ModelState.IsValid)
            {
                model.UpdatedBy = User.Identity?.Name ?? "Admin";
                SAdvertising.Update(model);
                SSeo.RefreshSitemapAndCache();
                return RedirectToAction("Index");
            }
            return View(model);
        }

        [HttpPost]
        public IActionResult Delete(int id)
        {
            var result = SAdvertising.Delete(id);
            if (result > 0) {
                SSeo.RefreshSitemapAndCache();
                return Json(new { success = true });
            }
            return Json(new { success = false, message = "Xóa thất bại" });
        }

        [HttpPost]
        public IActionResult BulkDelete([FromBody] List<int> ids)
        {
            if (ids == null || ids.Count == 0) return Json(new { success = false });
            int result = SAdvertising.BulkDelete(ids);
            if (result > 0) SSeo.RefreshSitemapAndCache();
            return Json(new { success = result > 0 });
        }

        [HttpPost]
        public IActionResult BulkUpdateStatus([FromBody] BulkUpdateModel model)
        {
            if (model.Ids == null || model.Ids.Count == 0) return Json(new { success = false });
            int result = SAdvertising.BulkUpdateStatus(model.Ids, model.Status);
            if (result > 0) SSeo.RefreshSitemapAndCache();
            return Json(new { success = result > 0 });
        }

        [HttpPost]
        public IActionResult RestoreDefaults(string section)
        {
            // Xóa dữ liệu cũ theo position
            var existing = SAdvertising.GetByPosition(section);
            if (existing != null && existing.Count > 0)
            {
                foreach (var item in existing)
                    SAdvertising.Delete(item.AdvertisingId);
            }

            var createdBy = User.Identity?.Name ?? "System";

            switch (section)
            {
                case "Home_WhyChoose":
                    var whyDefaults = new List<Advertising>
                    {
                        new Advertising { Title = "HÀNG CHÍNH HÃNG", Description = "Cam kết 100% sản phẩm chính hãng Gordak.", VideoUrl = "fa-solid fa-shield-halved", Position = "Home_WhyChoose", SortOrder = 1, Status = 1 },
                        new Advertising { Title = "BẢO HÀNH CHÍNH HÃNG", Description = "Bảo hành dài hạn theo tiêu chuẩn hãng.", VideoUrl = "fa-solid fa-certificate", Position = "Home_WhyChoose", SortOrder = 2, Status = 1 },
                        new Advertising { Title = "LINH KIỆN SẴN CÓ", Description = "Đầy đủ linh kiện thay thế, dễ dàng sửa chữa.", VideoUrl = "fa-solid fa-gear", Position = "Home_WhyChoose", SortOrder = 3, Status = 1 },
                        new Advertising { Title = "HỖ TRỢ KỸ THUẬT", Description = "Đội ngũ kỹ thuật giàu kinh nghiệm.", VideoUrl = "fa-solid fa-headset", Position = "Home_WhyChoose", SortOrder = 4, Status = 1 },
                        new Advertising { Title = "GIÁ TỐT NHẤT", Description = "Giá cạnh tranh cho đại lý, dự án và khách hàng.", VideoUrl = "fa-solid fa-hand-holding-dollar", Position = "Home_WhyChoose", SortOrder = 5, Status = 1 }
                    };
                    foreach (var d in whyDefaults) { d.CreatedBy = createdBy; SAdvertising.Insert(d); }
                    break;

                case "Home_BrandStat":
                    var statDefaults = new List<Advertising>
                    {
                        new Advertising { Title = "20+", Description = "NĂM KINH NGHIỆM", VideoUrl = "fa-solid fa-clock-rotate-left", Position = "Home_BrandStat", SortOrder = 1, Status = 1 },
                        new Advertising { Title = "50+", Description = "QUỐC GIA TIN DÙNG", VideoUrl = "fa-solid fa-globe", Position = "Home_BrandStat", SortOrder = 2, Status = 1 },
                        new Advertising { Title = "100+", Description = "SẢN PHẨM CHẤT LƯỢNG", VideoUrl = "fa-solid fa-sliders", Position = "Home_BrandStat", SortOrder = 3, Status = 1 },
                        new Advertising { Title = "100%", Description = "CHÍNH HÃNG", VideoUrl = "fa-solid fa-shield-halved", Position = "Home_BrandStat", SortOrder = 4, Status = 1 }
                    };
                    foreach (var d in statDefaults) { d.CreatedBy = createdBy; SAdvertising.Insert(d); }
                    break;

                case "Home_BrandIntro":
                    var introItem = new Advertising
                    {
                        Title = "VỀ THƯƠNG HIỆU GORDAK",
                        Image = "/assets/images/z7775799762257_f058bd78c3b1f94e77da2c4eda69efb3.jpg",
                        Description = "Gordak là thương hiệu nổi tiếng trong lĩnh vực thiết bị hàn và sửa chữa điện tử, được thành lập từ năm 1999. Với hơn 20 năm phát triển, Gordak không ngừng nghiên cứu và cải tiến công nghệ để mang đến những sản phẩm chất lượng cao, độ bền vượt trội và hiệu suất ổn định.",
                        VideoUrl = "Các sản phẩm của Gordak được tin dùng rộng rãi trong lĩnh vực sửa chữa điện tử, sản xuất công nghiệp, phòng thí nghiệm và đào tạo nghề tại hơn 50 quốc gia trên thế giới.",
                        Link = "/gioi-thieu.html",
                        Position = "Home_BrandIntro",
                        SortOrder = 1,
                        Status = 1,
                        CreatedBy = createdBy
                    };
                    SAdvertising.Insert(introItem);
                    break;

                default:
                    return Json(new { success = false, message = "Phần không hợp lệ" });
            }

            SSeo.RefreshSitemapAndCache();
            return Json(new { success = true });
        }

        public class BulkUpdateModel {
            public List<int> Ids { get; set; } = new();
            public int Status { get; set; }
        }
    }
}
