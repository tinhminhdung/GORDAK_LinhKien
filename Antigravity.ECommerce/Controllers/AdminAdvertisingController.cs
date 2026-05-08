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
        public IActionResult Index(string kw = "", string position = "", int? status = null, string sort = "SortOrder", string order = "ASC", int page = 1, int size = 20)
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
                return RedirectToAction("Index", new { position = model.Position });
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
                return RedirectToAction("Index", new { position = model.Position });
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
        public IActionResult UpdateSortOrder(int id, int sortOrder)
        {
            var item = SAdvertising.GetById(id);
            if (item == null) return Json(new { success = false, message = "Không tìm thấy" });
            item.SortOrder = sortOrder;
            item.UpdatedBy = User.Identity?.Name ?? "Admin";
            SAdvertising.Update(item);
            return Json(new { success = true });
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

                case "About_Hero":
                    SAdvertising.Insert(new Advertising
                    {
                        Title = "GORDAK",
                        Link = "VỀ THƯƠNG HIỆU",
                        VideoUrl = "Nhà sản xuất thiết bị hàn & rework hàng đầu thế giới",
                        Description = "Gordak là thương hiệu nổi tiếng trong lĩnh vực thiết bị hàn và sửa chữa điện tử. Với công nghệ hiện đại, độ bền vượt trội và hiệu suất ổn định, sản phẩm của Gordak được tin dùng bởi kỹ thuật viên và doanh nghiệp tại nhiều quốc gia trên thế giới.",
                        Image = "/assets/images/z7775799762257_f058bd78c3b1f94e77da2c4eda69efb3.jpg",
                        Position = "About_Hero",
                        SortOrder = 1,
                        Status = 1,
                        CreatedBy = createdBy
                    });
                    break;

                case "About_HeroStat":
                    var heroStatDefaults = new List<Advertising>
                    {
                        new Advertising { Title = "Hơn 30 năm", Description = "Kinh nghiệm trong ngành", VideoUrl = "fa-regular fa-circle-check", Position = "About_HeroStat", SortOrder = 1, Status = 1 },
                        new Advertising { Title = "Phân phối rộng rãi", Description = "Có mặt tại nhiều quốc gia", VideoUrl = "fa-solid fa-globe", Position = "About_HeroStat", SortOrder = 2, Status = 1 },
                        new Advertising { Title = "Đa dạng sản phẩm", Description = "Hơn 100 sản phẩm chất lượng cao", VideoUrl = "fa-solid fa-cubes", Position = "About_HeroStat", SortOrder = 3, Status = 1 },
                        new Advertising { Title = "Chất lượng đảm bảo", Description = "Kiểm định nghiêm ngặt trước khi đến tay khách", VideoUrl = "fa-solid fa-award", Position = "About_HeroStat", SortOrder = 4, Status = 1 }
                    };
                    foreach (var d in heroStatDefaults) { d.CreatedBy = createdBy; SAdvertising.Insert(d); }
                    break;

                case "About_Intro":
                    SAdvertising.Insert(new Advertising
                    {
                        Title = "GIỚI THIỆU VỀ GORDAK",
                        Description = "<p class=\"gt-about-desc\">Với định hướng tập trung vào chất lượng và sự đổi mới, Gordak không ngừng nghiên cứu và cải tiến công nghệ để mang đến những giải pháp hàn và sửa chữa hiệu quả, an toàn và thân thiện với người dùng.</p><ul class=\"gt-check-list\"><li><i class=\"fa-solid fa-check\"></i> Công nghệ kiểm soát nhiệt thông minh</li><li><i class=\"fa-solid fa-check\"></i> Độ bền cao, hoạt động ổn định</li><li><i class=\"fa-solid fa-check\"></i> Thiết kế tối ưu, dễ sử dụng</li><li><i class=\"fa-solid fa-check\"></i> Đáp ứng tiêu chuẩn kỹ thuật quốc tế</li></ul>",
                        Image = "/assets/images/z7775799762257_f058bd78c3b1f94e77da2c4eda69efb3.jpg",
                        Link = "/san-pham.html",
                        Position = "About_Intro",
                        SortOrder = 1,
                        Status = 1,
                        CreatedBy = createdBy
                    });
                    break;

                case "About_CoreValues":
                    var coreDefaults = new List<Advertising>
                    {
                        new Advertising { Title = "CHẤT LƯỢNG HÀNG ĐẦU", Description = "Kiểm soát chặt chẽ từng công đoạn để đảm bảo chất lượng sản phẩm.", VideoUrl = "fa-solid fa-gem", Position = "About_CoreValues", SortOrder = 1, Status = 1 },
                        new Advertising { Title = "ĐỔI MỚI CÔNG NGHỆ", Description = "Không ngừng nghiên cứu và cải tiến để mang lại giải pháp tiên tiến.", VideoUrl = "fa-solid fa-lightbulb", Position = "About_CoreValues", SortOrder = 2, Status = 1 },
                        new Advertising { Title = "KHÁCH HÀNG LÀ TRUNG TÂM", Description = "Luôn lắng nghe và đáp ứng mọi nhu cầu của khách hàng.", VideoUrl = "fa-solid fa-users", Position = "About_CoreValues", SortOrder = 3, Status = 1 },
                        new Advertising { Title = "HỢP TÁC BỀN VỮNG", Description = "Xây dựng mối quan hệ lâu dài, cùng phát triển và thành công.", VideoUrl = "fa-solid fa-handshake", Position = "About_CoreValues", SortOrder = 4, Status = 1 }
                    };
                    foreach (var d in coreDefaults) { d.CreatedBy = createdBy; SAdvertising.Insert(d); }
                    break;

                case "About_Timeline":
                    var tlDefaults = new List<Advertising>
                    {
                        new Advertising { Title = "Khởi đầu & phát triển", Description = "Tập trung nghiên cứu và phát triển thiết bị hàn chất lượng cao.", VideoUrl = "fa-solid fa-rocket", Position = "About_Timeline", SortOrder = 1, Status = 1 },
                        new Advertising { Title = "Mở rộng thị trường", Description = "Sản phẩm Gordak có mặt tại nhiều quốc gia và khu vực.", VideoUrl = "fa-solid fa-globe", Position = "About_Timeline", SortOrder = 2, Status = 1 },
                        new Advertising { Title = "Nâng cao chất lượng", Description = "Liên tục cải tiến công nghệ, tối ưu hiệu suất và độ bền sản phẩm.", VideoUrl = "fa-solid fa-chart-line", Position = "About_Timeline", SortOrder = 3, Status = 1 },
                        new Advertising { Title = "Đổi mới không ngừng", Description = "Phát triển các giải pháp hàn hiện đại, phù hợp với xu hướng mới.", VideoUrl = "fa-solid fa-microchip", Position = "About_Timeline", SortOrder = 4, Status = 1 },
                        new Advertising { Title = "Đồng hành cùng khách hàng", Description = "Luôn lắng nghe và hỗ trợ để mang lại giá trị tốt nhất cho khách hàng.", VideoUrl = "fa-solid fa-heart", Position = "About_Timeline", SortOrder = 5, Status = 1 }
                    };
                    foreach (var d in tlDefaults) { d.CreatedBy = createdBy; SAdvertising.Insert(d); }
                    break;

                case "About_JourneyImages":
                    var jiDefaults = new List<Advertising>
                    {
                        new Advertising { Title = "Nhà máy Gordak", Description = "Nhà máy sản xuất hiện đại tại Trung Quốc.", Image = "/assets/images/z7775799762257_f058bd78c3b1f94e77da2c4eda69efb3.jpg", Position = "About_JourneyImages", SortOrder = 1, Status = 1 },
                        new Advertising { Title = "Dây chuyền sản xuất", Description = "Dây chuyền sản xuất tự động hoá cao.", Image = "/assets/images/z7775799762257_f058bd78c3b1f94e77da2c4eda69efb3.jpg", Position = "About_JourneyImages", SortOrder = 2, Status = 1 },
                        new Advertising { Title = "Sản phẩm tiêu biểu", Description = "Các sản phẩm Gordak được trưng bày.", Image = "/assets/images/z7775799762257_f058bd78c3b1f94e77da2c4eda69efb3.jpg", Position = "About_JourneyImages", SortOrder = 3, Status = 1 }
                    };
                    foreach (var d in jiDefaults) { d.CreatedBy = createdBy; SAdvertising.Insert(d); }
                    break;

                case "Product_Commitment":
                    var commitDefaults = new List<Advertising>
                    {
                        new Advertising { Title = "Sản phẩm chính hãng 100%", Description = "Nhập khẩu và phân phối chính thức tại Việt Nam", VideoUrl = "fa-solid fa-shield-halved", Position = "Product_Commitment", SortOrder = 1, Status = 1 },
                        new Advertising { Title = "Bảo hành chính hãng 12 tháng", Description = "1 đổi 1 trong 7 ngày nếu có lỗi NSX", VideoUrl = "fa-solid fa-certificate", Position = "Product_Commitment", SortOrder = 2, Status = 1 },
                        new Advertising { Title = "Hỗ trợ kỹ thuật trọn đời", Description = "Đội ngũ kỹ thuật giàu kinh nghiệm", VideoUrl = "fa-solid fa-headset", Position = "Product_Commitment", SortOrder = 3, Status = 1 },
                        new Advertising { Title = "Giao hàng toàn quốc", Description = "Miễn phí giao hàng cho đơn từ 1 triệu", VideoUrl = "fa-solid fa-truck-fast", Position = "Product_Commitment", SortOrder = 4, Status = 1 }
                    };
                    foreach (var d in commitDefaults) { d.CreatedBy = createdBy; SAdvertising.Insert(d); }
                    break;

                case "About_Cert":
                    var certDefaults = new List<Advertising>
                    {
                        new Advertising { Title = "Chứng nhận đại lý chính thức", Description = "Được cấp bởi Gordak, xác nhận đại lý phân phối chính hãng.", Image = "/assets/images/z7775799762257_f058bd78c3b1f94e77da2c4eda69efb3.jpg", Position = "About_Cert", SortOrder = 1, Status = 1 },
                        new Advertising { Title = "Chứng nhận ISO 9001:2015", Description = "Hệ thống quản lý chất lượng đạt tiêu chuẩn quốc tế.", Image = "/assets/images/z7775799762257_f058bd78c3b1f94e77da2c4eda69efb3.jpg", Position = "About_Cert", SortOrder = 2, Status = 1 },
                        new Advertising { Title = "Chứng nhận CO, CQ đầy đủ", Description = "Đầy đủ giấy tờ chứng nhận nguồn gốc, chất lượng sản phẩm.", Image = "/assets/images/z7775799762257_f058bd78c3b1f94e77da2c4eda69efb3.jpg", Position = "About_Cert", SortOrder = 3, Status = 1 },
                        new Advertising { Title = "Nhà phân phối chính hãng tại Việt Nam", Description = "Ủy quyền phân phối độc quyền sản phẩm Gordak tại Việt Nam.", Image = "/assets/images/z7775799762257_f058bd78c3b1f94e77da2c4eda69efb3.jpg", Position = "About_Cert", SortOrder = 4, Status = 1 }
                    };
                    foreach (var d in certDefaults) { d.CreatedBy = createdBy; SAdvertising.Insert(d); }
                    break;

                case "Product_DealerCert":
                    SAdvertising.Insert(new Advertising
                    {
                        Title = "GORDAK",
                        Description = "Nhà phân phối chính thức tại Việt Nam",
                        Image = "/assets/images/z7775799762257_f058bd78c3b1f94e77da2c4eda69efb3.jpg",
                        Link = "/gioi-thieu.html",
                        Position = "Product_DealerCert",
                        SortOrder = 1,
                        Status = 1,
                        CreatedBy = createdBy
                    });
                    break;

                case "Contact_Connect":
                    var connectDefaults = new List<Advertising>
                    {
                        new Advertising { Title = "Tư vấn chuyên nghiệp", Description = "Đội ngũ giàu kinh nghiệm,<br>hỗ trợ tận tâm", VideoUrl = "fa-solid fa-user-tie", Position = "Contact_Connect", SortOrder = 1, Status = 1 },
                        new Advertising { Title = "Hỗ trợ nhanh chóng", Description = "Phản hồi kịp thời,<br>giải đáp mọi thắc mắc", VideoUrl = "fa-solid fa-bolt", Position = "Contact_Connect", SortOrder = 2, Status = 1 },
                        new Advertising { Title = "Đồng hành lâu dài", Description = "Cam kết mang đến sản phẩm<br>và dịch vụ tốt nhất", VideoUrl = "fa-solid fa-handshake", Position = "Contact_Connect", SortOrder = 3, Status = 1 },
                        new Advertising { Title = "Sản phẩm chính hãng", Description = "100% sản phẩm<br>chính hãng Gordak", VideoUrl = "fa-solid fa-shield-halved", Position = "Contact_Connect", SortOrder = 4, Status = 1 }
                    };
                    foreach (var d in connectDefaults) { d.CreatedBy = createdBy; SAdvertising.Insert(d); }
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
