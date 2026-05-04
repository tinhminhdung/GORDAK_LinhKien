using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Antigravity.ECommerce.Models;
using Antigravity.ECommerce.Services;
using Antigravity.ECommerce.Framework;
using System.Text.RegularExpressions;

namespace Antigravity.ECommerce.Controllers
{
    [Authorize]
    [Permission("SeoAudit")]
    public class AdminSeoController : Controller
    {
        [HttpGet]
        public IActionResult ClearCache()
        {
            SCache.ClearAll();
            return Json(new { success = true, message = "Đã xóa toàn bộ bộ nhớ đệm hệ thống. Sitemap và dữ liệu sẽ được cập nhật mới nhất." });
        }

        public IActionResult Index(string module = "all", string kw = "", string scoreFilter = "all", int page = 1)
        {
            var audit = new SeoAuditReport();
            
            // Normalize inputs
            module = module?.Trim() ?? "all";
            kw = kw?.Trim()?.ToLower() ?? "";
            scoreFilter = scoreFilter?.Trim() ?? "all";
            if (page < 1) page = 1;

            // ── 1. PRODUCTS ──
            var products = FProduct.Search(null, null, null, null, null, null, "CreatedAt", "DESC", 1, 5000);
            foreach (var p in products)
            {
                var item = new SeoAuditItem
                {
                    Module = "Sản phẩm",
                    ModuleIcon = "bx-package",
                    ModuleColor = "primary",
                    ItemId = p.ProductId,
                    ItemName = p.Name,
                    EditUrl = $"/AdminProduct/Edit/{p.ProductId}",
                    ImageUrl = p.MainImage,
                    Slug = p.Slug
                };

                if (string.IsNullOrWhiteSpace(p.SeoTitle))
                    item.Issues.Add(new SeoIssue("Thiếu SEO Title", "Google sẽ tự chọn tiêu đề từ nội dung trang, có thể không đúng ý bạn.", SeoSeverity.Error,
                        "Vào trang Chỉnh sửa → Tab SEO → Nhập \"SEO Title\" dài 30-60 ký tự, chứa từ khóa chính ở đầu. Ví dụ: \"iPhone 15 Pro Max 256GB Chính Hãng - Giá Rẻ\"",
                        $"/AdminProduct/Edit/{p.ProductId}"));
                else if (p.SeoTitle.Length < 30)
                    item.Issues.Add(new SeoIssue("SEO Title quá ngắn", $"Chỉ có {p.SeoTitle.Length} ký tự. Google khuyến nghị từ 30-60 ký tự để hiển thị đẹp nhất.", SeoSeverity.Warning,
                        "Thêm từ khóa phụ hoặc tên thương hiệu vào cuối tiêu đề. Ví dụ: \"Tên SP - Thương hiệu | Tên Shop\"",
                        $"/AdminProduct/Edit/{p.ProductId}"));
                else if (p.SeoTitle.Length > 65)
                    item.Issues.Add(new SeoIssue("SEO Title quá dài", $"Có {p.SeoTitle.Length} ký tự. Google sẽ cắt bớt và thêm dấu \"...\".", SeoSeverity.Warning,
                        "Rút gọn tiêu đề, giữ từ khóa quan trọng nhất ở đầu, bỏ các từ không cần thiết",
                        $"/AdminProduct/Edit/{p.ProductId}"));

                if (string.IsNullOrWhiteSpace(p.SeoDescription))
                    item.Issues.Add(new SeoIssue("Thiếu SEO Description", "Google sẽ tự chọn đoạn văn bản ngẫu nhiên từ trang làm mô tả.", SeoSeverity.Error,
                        "Vào chỉnh sửa → Nhập \"Mô tả SEO\" 120-160 ký tự. Viết hấp dẫn để khách hàng muốn click. Nên chứa: từ khóa chính + lợi ích + CTA (kêu gọi hành động)",
                        $"/AdminProduct/Edit/{p.ProductId}"));
                else if (p.SeoDescription.Length < 100)
                    item.Issues.Add(new SeoIssue("SEO Description quá ngắn", $"Chỉ {p.SeoDescription.Length} ký tự. Nên viết 120-160 ký tự.", SeoSeverity.Warning,
                        "Bổ sung thêm thông tin: Giá, khuyến mãi, miễn phí vận chuyển, bảo hành...",
                        $"/AdminProduct/Edit/{p.ProductId}"));

                if (string.IsNullOrWhiteSpace(p.MainImage))
                    item.Issues.Add(new SeoIssue("Thiếu ảnh đại diện", "Sản phẩm không có ảnh = không thể hiển thị trên Google Shopping, không có ảnh trong sitemap.", SeoSeverity.Error,
                        "Upload ảnh sản phẩm chất lượng cao (tối thiểu 600x600px). Ưu tiên ảnh nền trắng, rõ nét.",
                        $"/AdminProduct/Edit/{p.ProductId}"));

                if (string.IsNullOrWhiteSpace(p.Slug))
                    item.Issues.Add(new SeoIssue("Thiếu Slug (URL)", "URL sẽ không thân thiện khiến Google khó đọc.", SeoSeverity.Error,
                        "Nhập slug dạng: ten-san-pham-tu-khoa (không dấu, cách bằng dấu gạch ngang)",
                        $"/AdminProduct/Edit/{p.ProductId}"));

                if (string.IsNullOrWhiteSpace(p.ShortDescription))
                    item.Issues.Add(new SeoIssue("Thiếu mô tả ngắn", "Mô tả ngắn giúp Google hiểu sản phẩm nhanh hơn, cải thiện snippet.", SeoSeverity.Warning,
                        "Viết 1-2 câu tóm tắt đặc điểm nổi bật nhất của sản phẩm (50-200 ký tự)",
                        $"/AdminProduct/Edit/{p.ProductId}"));

                if (string.IsNullOrWhiteSpace(p.DetailDescription))
                    item.Issues.Add(new SeoIssue("Thiếu nội dung chi tiết", "Nội dung mỏng (thin content) sẽ bị Google đánh giá thấp, khó xếp hạng.", SeoSeverity.Error,
                        "Viết mô tả chi tiết ít nhất 300 ký tự. Bao gồm: Tính năng, thông số kỹ thuật, ưu điểm, cách sử dụng.",
                        $"/AdminProduct/Edit/{p.ProductId}"));
                else
                {
                    var textLen = Regex.Replace(p.DetailDescription, "<[^>]*>", "").Trim().Length;
                    if (textLen < 300)
                        item.Issues.Add(new SeoIssue("Nội dung chi tiết quá ít", $"Chỉ ~{textLen} ký tự thuần văn bản. Nên ít nhất 300.", SeoSeverity.Warning,
                            "Bổ sung: Thông số kỹ thuật, hướng dẫn sử dụng, câu hỏi thường gặp, so sánh với SP khác.",
                            $"/AdminProduct/Edit/{p.ProductId}"));
                    if (textLen > 2000)
                        item.Issues.Add(new SeoIssue("✓ Nội dung phong phú", $"{textLen} ký tự - Bài viết dài giúp xếp hạng cao hơn!", SeoSeverity.Good, "", ""));
                }

                if (string.IsNullOrWhiteSpace(p.SeoKeywords))
                    item.Issues.Add(new SeoIssue("Thiếu từ khóa SEO", "Nên bổ sung 3-5 từ khóa phụ liên quan.", SeoSeverity.Info,
                        "Nhập các từ khóa mà khách hàng thường tìm kiếm sản phẩm này, cách nhau bởi dấu phẩy.",
                        $"/AdminProduct/Edit/{p.ProductId}"));

                if (p.Price <= 0)
                    item.Issues.Add(new SeoIssue("Giá sản phẩm = 0", "Google Product Schema yêu cầu giá > 0 để hiển thị Rich Snippet (Sao đánh giá, Giá tiền).", SeoSeverity.Warning,
                        "Cập nhật giá bán thực tế cho sản phẩm.",
                        $"/AdminProduct/Edit/{p.ProductId}"));

                item.CalculateScore();
                audit.Products.Add(item);
            }

            // ── 2. NEWS ──
            var newsList = SNews.GetAll(2000);
            foreach (var n in newsList)
            {
                var item = new SeoAuditItem
                {
                    Module = "Tin tức",
                    ModuleIcon = "bx-news",
                    ModuleColor = "info",
                    ItemId = n.NewsId,
                    ItemName = n.Title,
                    EditUrl = $"/AdminNews/Edit/{n.NewsId}",
                    ImageUrl = n.Image,
                    Slug = n.Slug
                };

                if (string.IsNullOrWhiteSpace(n.SeoTitle))
                    item.Issues.Add(new SeoIssue("Thiếu SEO Title", "Bài viết chưa có tiêu đề SEO riêng.", SeoSeverity.Error,
                        "Vào Chỉnh sửa bài viết → Nhập SEO Title 30-60 ký tự. Nên khác với tiêu đề chính, bổ sung từ khóa tìm kiếm.",
                        $"/AdminNews/Edit/{n.NewsId}"));
                else if (n.SeoTitle.Length < 30)
                    item.Issues.Add(new SeoIssue("SEO Title quá ngắn", $"Chỉ {n.SeoTitle.Length} ký tự.", SeoSeverity.Warning,
                        "Thêm từ khóa phụ, năm hiện tại (2026), hoặc từ thu hút như: \"Mới nhất\", \"Chi tiết\"",
                        $"/AdminNews/Edit/{n.NewsId}"));

                if (string.IsNullOrWhiteSpace(n.SeoDescription))
                    item.Issues.Add(new SeoIssue("Thiếu SEO Description", "Chưa có mô tả SEO.", SeoSeverity.Error,
                        "Viết mô tả hấp dẫn 120-160 ký tự. Đây là dòng text hiển thị dưới tiêu đề trên Google.",
                        $"/AdminNews/Edit/{n.NewsId}"));

                if (string.IsNullOrWhiteSpace(n.Image))
                    item.Issues.Add(new SeoIssue("Thiếu ảnh đại diện", "Bài viết không có thumbnail trong kết quả tìm kiếm và OG share.", SeoSeverity.Error,
                        "Upload ảnh đại diện bài viết (tỷ lệ 16:9, tối thiểu 1200x630px cho OG).",
                        $"/AdminNews/Edit/{n.NewsId}"));

                if (string.IsNullOrWhiteSpace(n.Slug))
                    item.Issues.Add(new SeoIssue("Thiếu Slug", "URL không thân thiện.", SeoSeverity.Error,
                        "Nhập slug dạng: tieu-de-bai-viet-tu-khoa",
                        $"/AdminNews/Edit/{n.NewsId}"));

                if (string.IsNullOrWhiteSpace(n.DetailDescription))
                    item.Issues.Add(new SeoIssue("Bài viết trống nội dung", "Bài viết không có nội dung.", SeoSeverity.Error,
                        "Viết nội dung bài viết ít nhất 500 ký tự. Bài viết dài hơn 1500 từ có xu hướng xếp hạng cao hơn.",
                        $"/AdminNews/Edit/{n.NewsId}"));
                else
                {
                    var textLen = Regex.Replace(n.DetailDescription, "<[^>]*>", "").Trim().Length;
                    if (textLen < 500)
                        item.Issues.Add(new SeoIssue("Nội dung quá ngắn", $"~{textLen} ký tự. Nên ít nhất 500.", SeoSeverity.Warning,
                            "Bổ sung thêm thông tin, phân tích, hình ảnh minh họa, liên kết nội bộ.",
                            $"/AdminNews/Edit/{n.NewsId}"));
                    if (textLen > 3000)
                        item.Issues.Add(new SeoIssue("✓ Bài viết chất lượng", $"{textLen} ký tự - Rất tốt cho SEO!", SeoSeverity.Good, "", ""));
                }

                if (string.IsNullOrWhiteSpace(n.SeoKeywords))
                    item.Issues.Add(new SeoIssue("Thiếu từ khóa SEO", "Nên bổ sung từ khóa.", SeoSeverity.Info,
                        "Thêm 3-5 từ khóa mà người đọc thường tìm kiếm chủ đề này.",
                        $"/AdminNews/Edit/{n.NewsId}"));

                item.CalculateScore();
                audit.News.Add(item);
            }

            // ── 3. CATEGORIES ──
            var categories = FCategory.GetAll().Where(x => x.Status == 1).ToList();
            foreach (var c in categories)
            {
                string editUrl = c.CategoryType switch {
                    1 => $"/AdminCategory/Edit/{c.CategoryId}",
                    2 => $"/AdminNewsCategory/Edit/{c.CategoryId}",
                    3 => $"/AdminVideoCategory/Edit/{c.CategoryId}",
                    6 => $"/AdminGalleryCategory/Edit/{c.CategoryId}",
                    7 => $"/AdminFAQCategory/Edit/{c.CategoryId}",
                    8 => $"/AdminDocumentCategory/Edit/{c.CategoryId}",
                    _ => $"/AdminCategory/Edit/{c.CategoryId}"
                };
                string catTypeName = c.CategoryType switch {
                    1 => "SP", 2 => "Tin", 3 => "Video", 6 => "Ảnh", 7 => "FAQ", 8 => "TL",
                    _ => "Khác"
                };

                var item = new SeoAuditItem
                {
                    Module = "Danh mục",
                    ModuleIcon = "bx-category",
                    ModuleColor = "warning",
                    ItemId = c.CategoryId,
                    ItemName = $"[{catTypeName}] {c.Name}",
                    EditUrl = editUrl,
                    ImageUrl = c.Image,
                    Slug = c.Slug
                };

                if (string.IsNullOrWhiteSpace(c.SeoTitle))
                    item.Issues.Add(new SeoIssue("Thiếu SEO Title", "Danh mục chưa có tiêu đề SEO.", SeoSeverity.Warning,
                        "Nhập tiêu đề SEO cho trang danh mục. Ví dụ: \"Điện Thoại Giá Rẻ - Mua Online Chính Hãng\"",
                        editUrl));

                if (string.IsNullOrWhiteSpace(c.SeoDescription))
                    item.Issues.Add(new SeoIssue("Thiếu SEO Description", "Chưa có mô tả SEO.", SeoSeverity.Warning,
                        "Viết mô tả 120-160 ký tự mô tả danh mục này chứa gì.",
                        editUrl));

                if (string.IsNullOrWhiteSpace(c.Image))
                    item.Issues.Add(new SeoIssue("Thiếu ảnh đại diện", "Không có ảnh trong sitemap.", SeoSeverity.Warning,
                        "Upload ảnh đại diện cho danh mục để hiển thị trên sitemap và khi chia sẻ mạng xã hội.",
                        editUrl));

                if (string.IsNullOrWhiteSpace(c.Slug))
                    item.Issues.Add(new SeoIssue("Thiếu Slug", "URL không thân thiện.", SeoSeverity.Error,
                        "Nhập slug cho danh mục. Ví dụ: dien-thoai, may-tinh-xach-tay",
                        editUrl));

                if (string.IsNullOrWhiteSpace(c.Description))
                    item.Issues.Add(new SeoIssue("Thiếu mô tả danh mục", "Nên thêm mô tả.", SeoSeverity.Info,
                        "Viết 1-2 câu mô tả danh mục để Google hiểu nội dung.",
                        editUrl));

                item.CalculateScore();
                audit.Categories.Add(item);
            }

            // ── 4. VIDEOS ──
            var videos = SVideo.Search("", null, null, "CreatedAt", "DESC", 1, 2000);
            foreach (var v in videos)
            {
                var item = new SeoAuditItem
                {
                    Module = "Video",
                    ModuleIcon = "bx-video",
                    ModuleColor = "danger",
                    ItemId = v.VideoId,
                    ItemName = v.Title,
                    EditUrl = $"/AdminVideo/Edit/{v.VideoId}",
                    ImageUrl = v.ThumbnailUrl ?? $"https://img.youtube.com/vi/{v.YoutubeId}/mqdefault.jpg",
                    Slug = v.Slug
                };

                if (string.IsNullOrWhiteSpace(v.SeoTitle))
                    item.Issues.Add(new SeoIssue("Thiếu SEO Title", "Nên có tiêu đề SEO riêng cho video.", SeoSeverity.Error, "Sửa video -> Nhập SEO Title", $"/AdminVideo/Edit/{v.VideoId}"));
                
                if (string.IsNullOrWhiteSpace(v.SeoDescription))
                    item.Issues.Add(new SeoIssue("Thiếu SEO Description", "SEO Description giúp tăng tỷ lệ click từ Google.", SeoSeverity.Error, "Sửa video -> Nhập SEO Description", $"/AdminVideo/Edit/{v.VideoId}"));

                if (string.IsNullOrWhiteSpace(v.Slug))
                    item.Issues.Add(new SeoIssue("Thiếu Slug (URL)", "URL không thân thiện.", SeoSeverity.Error, "Sửa video -> Nhập Slug", $"/AdminVideo/Edit/{v.VideoId}"));

                if (string.IsNullOrWhiteSpace(v.YoutubeId))
                    item.Issues.Add(new SeoIssue("Thiếu Youtube ID", "Video không có ID Youtube.", SeoSeverity.Error, "Nhập Youtube ID", $"/AdminVideo/Edit/{v.VideoId}"));

                item.CalculateScore();
                audit.Videos.Add(item);
            }

            // ── 5. GALLERY ──
            var galleries = SGallery.Search("", null, null, "CreatedAt", "DESC", 1, 2000);
            foreach (var g in galleries)
            {
                var item = new SeoAuditItem
                {
                    Module = "Thư viện ảnh",
                    ModuleIcon = "bx-images",
                    ModuleColor = "success",
                    ItemId = g.GalleryId,
                    ItemName = g.AlbumName,
                    EditUrl = $"/AdminGallery/Edit/{g.GalleryId}",
                    ImageUrl = g.CoverImage,
                    Slug = g.Slug
                };

                if (string.IsNullOrWhiteSpace(g.SeoTitle))
                    item.Issues.Add(new SeoIssue("Thiếu SEO Title", "Album ảnh cần có tiêu đề SEO.", SeoSeverity.Error, "Sửa album -> Nhập SEO Title", $"/AdminGallery/Edit/{g.GalleryId}"));

                if (string.IsNullOrWhiteSpace(g.SeoDescription))
                    item.Issues.Add(new SeoIssue("Thiếu SEO Description", "Mô tả SEO giúp Google phân loại bộ sưu tập ảnh tốt hơn.", SeoSeverity.Error, "Sửa album -> Nhập SEO Description", $"/AdminGallery/Edit/{g.GalleryId}"));

                if (string.IsNullOrWhiteSpace(g.Slug))
                    item.Issues.Add(new SeoIssue("Thiếu Slug (URL)", "URL không thân thiện.", SeoSeverity.Error, "Sửa album -> Nhập Slug", $"/AdminGallery/Edit/{g.GalleryId}"));

                if (g.ImageCount == 0)
                    item.Issues.Add(new SeoIssue("Album trống", "Album chưa có ảnh nào.", SeoSeverity.Error, "Thêm ảnh vào album", $"/AdminGallery/Edit/{g.GalleryId}"));

                item.CalculateScore();
                audit.Galleries.Add(item);
            }

            // ── 6. FAQ ──
            var faqs = SFAQ.Search("", null, null, "CreatedAt", "DESC", 1, 2000);
            foreach (var f in faqs)
            {
                var item = new SeoAuditItem
                {
                    Module = "FAQ",
                    ModuleIcon = "bx-help-circle",
                    ModuleColor = "secondary",
                    ItemId = f.FAQId,
                    ItemName = f.Question,
                    EditUrl = $"/AdminFAQ/Edit/{f.FAQId}",
                    ImageUrl = null,
                    Slug = f.Slug
                };

                if (string.IsNullOrWhiteSpace(f.SeoTitle))
                    item.Issues.Add(new SeoIssue("Thiếu SEO Title", "Câu hỏi FAQ cần tiêu đề SEO tối ưu.", SeoSeverity.Warning, "Sửa FAQ -> Nhập SEO Title", $"/AdminFAQ/Edit/{f.FAQId}"));

                if (string.IsNullOrWhiteSpace(f.Slug))
                    item.Issues.Add(new SeoIssue("Thiếu Slug (URL)", "Mỗi câu hỏi nên có URL riêng để Google index index Rich Snippets.", SeoSeverity.Error, "Sửa FAQ -> Nhập Slug", $"/AdminFAQ/Edit/{f.FAQId}"));

                if (string.IsNullOrWhiteSpace(f.Answer))
                    item.Issues.Add(new SeoIssue("Thiếu câu trả lời", "FAQ không có câu trả lời.", SeoSeverity.Error, "Viết nội dung câu trả lời", $"/AdminFAQ/Edit/{f.FAQId}"));

                item.CalculateScore();
                audit.FAQs.Add(item);
            }

            // ── 7. DOCUMENTS ──
            var docs = SDocument.Search("", null, null, "CreatedAt", "DESC", 1, 2000);
            foreach (var d in docs)
            {
                var item = new SeoAuditItem
                {
                    Module = "Tài liệu",
                    ModuleIcon = "bx-file",
                    ModuleColor = "dark",
                    ItemId = d.DocumentId,
                    ItemName = d.Title,
                    EditUrl = $"/AdminDocument/Edit/{d.DocumentId}",
                    ImageUrl = null,
                    Slug = d.Slug
                };

                if (string.IsNullOrWhiteSpace(d.SeoTitle))
                    item.Issues.Add(new SeoIssue("Thiếu SEO Title", "Tài liệu cần tiêu đề SEO để khách hàng dễ tìm thấy trên Google Search PDF.", SeoSeverity.Error, "Sửa tài liệu -> Nhập SEO Title", $"/AdminDocument/Edit/{d.DocumentId}"));

                if (string.IsNullOrWhiteSpace(d.Slug))
                    item.Issues.Add(new SeoIssue("Thiếu Slug (URL)", "URL không thân thiện.", SeoSeverity.Error, "Sửa tài liệu -> Nhập Slug", $"/AdminDocument/Edit/{d.DocumentId}"));

                if (string.IsNullOrWhiteSpace(d.FilePath))
                    item.Issues.Add(new SeoIssue("Thiếu file đính kèm", "Tài liệu không có file.", SeoSeverity.Error, "Upload file", $"/AdminDocument/Edit/{d.DocumentId}"));

                item.CalculateScore();
                audit.Documents.Add(item);
            }



            audit.CalculateSummary();

            // ── 9. FILTERING & PAGINATION ──
            var allItems = audit.GetAllItems().OrderBy(x => x.Score).ToList();
            
            // Apply Module Filter
            if (module != "all")
            {
                string expectedModuleName = module switch {
                    "product" => "Sản phẩm",
                    "news" => "Tin tức",
                    "category" => "Danh mục",
                    "video" => "Video",
                    "gallery" => "Thư viện ảnh",
                    "faq" => "FAQ",
                    "document" => "Tài liệu",
                    _ => module
                };
                allItems = allItems.Where(x => x.Module == expectedModuleName).ToList();
            }

            // Apply Keyword Filter
            if (!string.IsNullOrEmpty(kw))
            {
                allItems = allItems.Where(x => 
                    (x.ItemName?.ToLower()?.Contains(kw) ?? false) || 
                    (x.Slug?.ToLower()?.Contains(kw) ?? false) ||
                    x.Issues.Any(i => i.Title.ToLower().Contains(kw) || i.Description.ToLower().Contains(kw))
                ).ToList();
            }

            // Apply Score Filter
            if (scoreFilter != "all")
            {
                if (scoreFilter == "bad") allItems = allItems.Where(x => x.Score < 50).ToList();
                else if (scoreFilter == "medium") allItems = allItems.Where(x => x.Score >= 50 && x.Score < 80).ToList();
                else if (scoreFilter == "good") allItems = allItems.Where(x => x.Score >= 80).ToList();
            }

            // Pagination params
            int pageSize = 50;
            audit.TotalItems = allItems.Count;
            audit.TotalPages = (int)Math.Ceiling(audit.TotalItems / (double)pageSize);
            audit.CurrentPage = page > audit.TotalPages && audit.TotalPages > 0 ? audit.TotalPages : page;
            audit.CurrentModule = module;
            audit.CurrentKw = kw;
            audit.CurrentScoreFilter = scoreFilter;

            audit.PagedItems = allItems
                .Skip((audit.CurrentPage - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            // Pass Gemini API Key to view
            ViewBag.GeminiApiKey = SSetting.GetValue("GeminiApiKey") ?? "";

            return View(audit);
        }

        [HttpPost]
        public async Task<IActionResult> AISuggest([FromBody] AISuggestRequest request)
        {
            var apiKey = SSetting.GetValue("GeminiApiKey") ?? "";

            // 1. Thu thập dữ liệu thực tế từ DB
            string itemName = request.ItemName ?? "";
            string shortDesc = "";
            string seoTitle = "";
            string seoDesc = "";
            string seoKeywords = "";
            string slug = "";

            try
            {
                switch (request.Module.ToLower())
                {
                    case "product":
                    case "sản phẩm":
                        var p = FProduct.Search(null, null, null, null, null, null, "ProductId", "ASC", 1, 5000)
                            .FirstOrDefault(x => x.ProductId == request.ItemId);
                        if (p != null) { itemName = p.Name; shortDesc = p.ShortDescription ?? ""; seoTitle = p.SeoTitle ?? ""; seoDesc = p.SeoDescription ?? ""; seoKeywords = p.SeoKeywords ?? ""; slug = p.Slug ?? ""; }
                        break;
                    case "news":
                    case "tin tức":
                        var n = SNews.GetAll(5000).FirstOrDefault(x => x.NewsId == request.ItemId);
                        if (n != null) { itemName = n.Title; shortDesc = n.ShortDescription ?? ""; seoTitle = n.SeoTitle ?? ""; seoDesc = n.SeoDescription ?? ""; seoKeywords = n.SeoKeywords ?? ""; slug = n.Slug ?? ""; }
                        break;
                    case "category":
                    case "danh mục":
                        var c = FCategory.GetAll().FirstOrDefault(x => x.CategoryId == request.ItemId);
                        if (c != null) { itemName = c.Name; shortDesc = c.Description ?? ""; seoTitle = c.SeoTitle ?? ""; seoDesc = c.SeoDescription ?? ""; seoKeywords = c.SeoKeywords ?? ""; slug = c.Slug ?? ""; }
                        break;
                    case "video":
                        var v = SVideo.GetById(request.ItemId);
                        if (v != null) { itemName = v.Title; shortDesc = v.SeoDescription ?? ""; seoTitle = v.SeoTitle ?? ""; seoDesc = v.SeoDescription ?? ""; seoKeywords = v.SeoKeywords ?? ""; slug = v.Slug ?? ""; }
                        break;
                    case "gallery":
                    case "thư viện ảnh":
                        var g = SGallery.GetById(request.ItemId);
                        if (g != null) { itemName = g.AlbumName; shortDesc = g.Description ?? ""; seoTitle = g.SeoTitle ?? ""; seoDesc = g.SeoDescription ?? ""; seoKeywords = g.SeoKeywords ?? ""; slug = g.Slug ?? ""; }
                        break;
                    case "faq":
                        var f = SFAQ.GetById(request.ItemId);
                        if (f != null) { itemName = f.Question; shortDesc = f.Answer ?? ""; seoTitle = f.SeoTitle ?? ""; seoDesc = f.SeoDescription ?? ""; seoKeywords = f.SeoKeywords ?? ""; slug = f.Slug ?? ""; }
                        break;
                    case "Tài liệu":
                        var d = SDocument.GetById(request.ItemId);
                        if (d != null) { itemName = d.Title; shortDesc = d.FileSize ?? ""; seoTitle = d.SeoTitle ?? ""; seoDesc = d.SeoDescription ?? ""; seoKeywords = d.SeoKeywords ?? ""; slug = d.Slug ?? ""; }
                        break;
                }
            }
            catch { /* ignore */ }

            // 2. Xây dựng prompt cho AI (Yêu cầu cấu trúc thẻ tag để bóc tách dữ liệu)
            var prompt = $@"Bạn là chuyên gia SEO hàng đầu Việt Nam. Hãy phân tích nội dung sau và đưa ra các thẻ SEO tối ưu nhất.

**Nhiệm vụ:** Trả về bộ thẻ SEO (Title, Description, Keywords) hoàn hảo cho mục này.

**YÊU CẦU BẮT BUỘC (TUYỆT ĐỐI TUÂN THỦ):** 
Hệ thống phần mềm của chúng tôi sẽ parse nội dung của bạn. Bạn PHẢI trả về đúng 3 thẻ HTML tùy chỉnh dưới đây, không được bọc trong markdown code block (như ```html):
[TITLE]Viết SEO Title tối ưu (30-60 ký tự) vào đây[/TITLE]
[DESC]Viết SEO Description hấp dẫn (120-160 ký tự) vào đây[/DESC]
[KEY]Viết các từ khóa (cách nhau bởi dấu phẩy) vào đây[/KEY]

**Thông tin nội dung cần phân tích:**
- Loại nội dung: {request.Module}
- Tên/Tiêu đề: {itemName}
- Mô tả ngắn hiện tại: {(string.IsNullOrEmpty(shortDesc) ? "(Chưa có)" : shortDesc)}
- SEO Title hiện tại: {(string.IsNullOrEmpty(seoTitle) ? "(Chưa có)" : seoTitle)}
- SEO Description hiện tại: {(string.IsNullOrEmpty(seoDesc) ? "(Chưa có)" : seoDesc)}
- Từ khóa hiện tại: {(string.IsNullOrEmpty(seoKeywords) ? "(Chưa có)" : seoKeywords)}
- Slug hiện tại: {(string.IsNullOrEmpty(slug) ? "(Chưa có)" : slug)}

**Hướng dẫn trả lời:** 
Hãy trả lời bằng tiếng Việt. Sau khi in ra 3 thẻ bắt buộc ở trên, bạn CÓ THỂ viết thêm các mẹo tối ưu khác ở phía dưới bằng văn bản bình thường.";

            // 3. Gọi Gemini AI với cơ chế Auto-Discovery Model
            string aiErrorLog = "";
            if (!string.IsNullOrWhiteSpace(apiKey))
            {
                apiKey = apiKey.Trim();
                try
                {
                    using var httpClient = new HttpClient();
                    httpClient.Timeout = TimeSpan.FromSeconds(30);

                    // Bước 3a: Gọi ListModels để tự động tìm model đang hoạt động
                    var listModelsUrl = $"https://generativelanguage.googleapis.com/v1beta/models?key={apiKey}";
                    var listResp = await httpClient.GetAsync(listModelsUrl);
                    
                    if (!listResp.IsSuccessStatusCode)
                    {
                        aiErrorLog = "Không thể lấy danh sách Model từ Google: " + await listResp.Content.ReadAsStringAsync();
                    }
                    else
                    {
                        var listJson = await listResp.Content.ReadAsStringAsync();
                        using var listDoc = System.Text.Json.JsonDocument.Parse(listJson);
                        
                        // Lọc ra các model hỗ trợ generateContent, ưu tiên flash > pro
                        var availableModels = new List<string>();
                        if (listDoc.RootElement.TryGetProperty("models", out var modelsArr))
                        {
                            foreach (var m in modelsArr.EnumerateArray())
                            {
                                var modelName = m.GetProperty("name").GetString() ?? "";
                                // Chỉ lấy model hỗ trợ generateContent
                                if (m.TryGetProperty("supportedGenerationMethods", out var methods))
                                {
                                    bool supportsGenerate = false;
                                    foreach (var method in methods.EnumerateArray())
                                    {
                                        if (method.GetString() == "generateContent") { supportsGenerate = true; break; }
                                    }
                                    if (supportsGenerate)
                                    {
                                        // modelName có dạng "models/gemini-2.0-flash", cắt bỏ prefix "models/"
                                        availableModels.Add(modelName.Replace("models/", ""));
                                    }
                                }
                            }
                        }

                        // Sắp xếp ưu tiên: flash trước (nhanh + rẻ), rồi pro
                        var prioritized = availableModels
                            .OrderByDescending(n => n.Contains("flash") ? 2 : n.Contains("pro") ? 1 : 0)
                            .ThenByDescending(n => n) // Mới nhất (version cao) lên trước
                            .Take(3) // Chỉ thử tối đa 3 model
                            .ToList();

                        if (!prioritized.Any())
                        {
                            aiErrorLog = "API Key hợp lệ nhưng không tìm thấy model nào hỗ trợ generateContent. Danh sách model trả về: " + string.Join(", ", availableModels);
                        }

                        // Bước 3b: Thử từng model cho đến khi thành công
                        foreach (var modelName in prioritized)
                        {
                            try
                            {
                                var geminiUrl = $"https://generativelanguage.googleapis.com/v1beta/models/{modelName}:generateContent?key={apiKey}";
                                var body = new
                                {
                                    contents = new[] { new { parts = new[] { new { text = prompt } } } },
                                    generationConfig = new { temperature = 1.0, maxOutputTokens = 1024 }
                                };
                                var json = System.Text.Json.JsonSerializer.Serialize(body);
                                var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
                                var response = await httpClient.PostAsync(geminiUrl, content);

                                if (response.IsSuccessStatusCode)
                                {
                                    var resultJson = await response.Content.ReadAsStringAsync();
                                    using var doc = System.Text.Json.JsonDocument.Parse(resultJson);
                                    var text = doc.RootElement.GetProperty("candidates")[0].GetProperty("content").GetProperty("parts")[0].GetProperty("text").GetString();
                                    return Json(new { success = true, source = "gemini", result = text, model = modelName });
                                }
                                else
                                {
                                    aiErrorLog += $"[{modelName}] Error: " + await response.Content.ReadAsStringAsync() + "<br/>";
                                }
                            }
                            catch (Exception ex)
                            {
                                aiErrorLog += $"[{modelName}] Exception: " + ex.Message + "<br/>";
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    aiErrorLog += "HTTP Client Error: " + ex.Message;
                }
            }

            // 4. Fallback cuối cùng: Gợi ý LOCAL thông minh
            var localSuggestion = GenerateLocalSuggestion(request.Module, itemName, shortDesc, seoKeywords);
            return Json(new { success = true, source = "local", result = localSuggestion, errorLog = aiErrorLog });
        }

        [HttpPost]
        public IActionResult ApplySeo([FromBody] ApplySeoRequest request)
        {
            if (request == null || request.ItemId <= 0)
                return Json(new { success = false, message = "Dữ liệu không hợp lệ." });

            try
            {
                int result = 0;
                string updateBy = User.Identity?.Name ?? "Gemini AI";

                switch (request.Module.ToLower())
                {
                    case "product":
                    case "sản phẩm":
                        var p = FProduct.GetById(request.ItemId);
                        if (p != null)
                        {
                            p.SeoTitle = request.SeoTitle;
                            p.SeoDescription = request.SeoDescription;
                            p.SeoKeywords = request.SeoKeywords;
                            p.UpdatedBy = updateBy;
                            result = FProduct.Update(p);
                        }
                        break;

                    case "news":
                    case "tin tức":
                        var n = SNews.GetById(request.ItemId);
                        if (n != null)
                        {
                            n.SeoTitle = request.SeoTitle;
                            n.SeoDescription = request.SeoDescription;
                            n.SeoKeywords = request.SeoKeywords;
                            n.UpdatedBy = updateBy;
                            result = SNews.Update(n);
                        }
                        break;

                    case "category":
                    case "danh mục":
                        var c = FCategory.GetById(request.ItemId);
                        if (c != null)
                        {
                            c.SeoTitle = request.SeoTitle;
                            c.SeoDescription = request.SeoDescription;
                            c.SeoKeywords = request.SeoKeywords;
                            c.UpdatedBy = updateBy;
                            result = FCategory.Update(c);
                        }
                        break;

                    case "video":
                        var v = SVideo.GetById(request.ItemId);
                        if (v != null)
                        {
                            v.SeoTitle = request.SeoTitle;
                            v.SeoDescription = request.SeoDescription;
                            v.SeoKeywords = request.SeoKeywords;
                            v.UpdatedBy = updateBy;
                            result = SVideo.Update(v);
                        }
                        break;

                    case "gallery":
                    case "thư viện ảnh":
                        var g = SGallery.GetById(request.ItemId);
                        if (g != null)
                        {
                            g.SeoTitle = request.SeoTitle;
                            g.SeoDescription = request.SeoDescription;
                            g.SeoKeywords = request.SeoKeywords;
                            g.UpdatedBy = updateBy;
                            result = SGallery.Update(g);
                        }
                        break;

                    case "faq":
                        var faq = SFAQ.GetById(request.ItemId);
                        if (faq != null)
                        {
                            faq.SeoTitle = request.SeoTitle;
                            faq.SeoDescription = request.SeoDescription;
                            faq.SeoKeywords = request.SeoKeywords;
                            faq.UpdatedBy = updateBy;
                            result = SFAQ.Update(faq);
                        }
                        break;

                    case "document":
                        var doc = SDocument.GetById(request.ItemId);
                        if (doc != null)
                        {
                            doc.SeoTitle = request.SeoTitle;
                            doc.SeoDescription = request.SeoDescription;
                            doc.SeoKeywords = request.SeoKeywords;
                            doc.UpdatedBy = updateBy;
                            result = SDocument.Update(doc);
                        }
                        break;

                    default:
                        return Json(new { success = false, message = $"Module '{request.Module}' chưa hỗ trợ lưu nhanh." });
                }

                if (result > 0)
                {
                    FAdminLog.Insert(new AdminLog {
                        Username = updateBy,
                        Action = "Update",
                        Module = "Quản lý SEO",
                        Description = $"Cập nhật gợi ý SEO thông minh cho [{request.Module}] (ID={request.ItemId}: {request.SeoTitle})",
                        IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString()
                    });
                    return Json(new { success = true, message = "Đã cập nhật SEO thành công vào hệ thống!" });
                }
                else
                {
                    return Json(new { success = false, message = "Không tìm thấy dữ liệu để cập nhật hoặc lỗi DB." });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Lỗi: {ex.Message}" });
            }
        }

        private string GenerateLocalSuggestion(string module, string name, string desc, string keywords)
        {
            var year = DateTime.Now.Year;
            var sb = new System.Text.StringBuilder();

            switch (module.ToLower())
            {
                case "sản phẩm":
                case "product":
                    sb.AppendLine($"[TITLE]{name} - Chính Hãng, Giá Tốt Nhất {year}[/TITLE]");
                    sb.AppendLine($"[DESC]Mua {name} chính hãng giá tốt nhất {year}. ✅ Bảo hành 12 tháng ✅ Giao hàng toàn quốc ✅ Đổi trả 30 ngày. Đặt hàng ngay![/DESC]");
                    sb.AppendLine($"[KEY]{name}, mua {name}, {name} giá rẻ, {name} chính hãng, {name} {year}[/KEY]");
                    sb.AppendLine();
                    sb.AppendLine($"💡 MẸO BỔ SUNG:");
                    sb.AppendLine($"• Thêm ảnh sản phẩm từ nhiều góc (5-8 ảnh)");
                    sb.AppendLine($"• Viết mô tả chi tiết ≥ 500 từ (thông số, ưu điểm, so sánh)");
                    break;
                case "tin tức":
                case "news":
                    sb.AppendLine($"[TITLE]{name} - Thông Tin Mới Nhất {year}[/TITLE]");
                    sb.AppendLine($"[DESC]Tìm hiểu chi tiết về {name}. Cập nhật mới nhất {year} với thông tin chính xác và phân tích chuyên sâu. Đọc ngay![/DESC]");
                    sb.AppendLine($"[KEY]{name}, kiến thức {name}, thông tin {name}[/KEY]");
                    sb.AppendLine();
                    sb.AppendLine($"💡 MẸO BỔ SUNG:");
                    sb.AppendLine($"• Viết bài dài ≥ 1500 từ, chia heading H2/H3 rõ ràng");
                    sb.AppendLine($"• Thêm 3-5 link nội bộ đến sản phẩm liên quan");
                    break;
                case "video":
                    sb.AppendLine($"[TITLE]{name} - Video Trải Nghiệm Mới Nhất {year}[/TITLE]");
                    sb.AppendLine($"[DESC]Xem video chi tiết {name}. Hướng dẫn, trải nghiệm trực quan. ✅ Video chất lượng khung hình cao. Cập nhật {year}.[/DESC]");
                    sb.AppendLine($"[KEY]{name}, video {name}, hướng dẫn {name}, đánh giá {name}[/KEY]");
                    break;
                case "thư viện ảnh":
                case "gallery":
                    sb.AppendLine($"[TITLE]{name} - Bộ Sưu Tập Ảnh Thực Tế {year}[/TITLE]");
                    sb.AppendLine($"[DESC]Khám phá bộ sưu tập {name} mới nhất. 📸 Ảnh chất lượng cao, hình ảnh thực tế đa góc nhìn. Xem chi tiết ngay![/DESC]");
                    sb.AppendLine($"[KEY]{name}, album ảnh {name}, thư viện ảnh {name}, hình ảnh {name}[/KEY]");
                    break;
                case "faq":
                    sb.AppendLine($"[TITLE]Hỏi đáp: {name} - Giải Đáp Chi Tiết {year}[/TITLE]");
                    sb.AppendLine($"[DESC]Thông tin giải đáp thắc mắc: {name}. Câu trả lời chính xác, đáng tin cậy. Tra cứu ngay![/DESC]");
                    sb.AppendLine($"[KEY]{name}, hỏi đáp {name}, câu hỏi thường gặp {name}[/KEY]");
                    break;
                case "tài liệu":
                case "document":
                    sb.AppendLine($"[TITLE]Tải {name} - Tài Liệu Chuyên Sâu {year}[/TITLE]");
                    sb.AppendLine($"[DESC]Tải tài liệu {name} đầy đủ, nguồn uy tín. 📄 Update {year}. Download tài liệu miễn phí và an toàn![/DESC]");
                    sb.AppendLine($"[KEY]{name}, tải {name}, download {name}, tài liệu {name}[/KEY]");
                    break;
                case "danh mục":
                case "category":
                    sb.AppendLine($"[TITLE]{name} - Đa Dạng Mẫu Mã, Cập Nhật {year}[/TITLE]");
                    sb.AppendLine($"[DESC]Khám phá danh mục {name}. Cung cấp đầy đủ thông tin, sản phẩm chất lượng cao, uy tín và bảo hành dài hạn. Mua ngay![/DESC]");
                    sb.AppendLine($"[KEY]{name}, danh mục {name}, tổng hợp {name}[/KEY]");
                    break;
                case "banner/qc":
                case "banner":
                    sb.AppendLine($"[TITLE]{name} - Chương Trình Ưu Đãi Đặc Biệt[/TITLE]");
                    sb.AppendLine($"[DESC]Chương trình {name} với nhiều ưu đãi hấp dẫn. Xem ngay chi tiết để không bỏ lỡ![/DESC]");
                    sb.AppendLine($"[KEY]{name}, khuyến mãi {name}, ưu đãi {name}[/KEY]");
                    break;
                default:
                    sb.AppendLine($"[TITLE]{name} - Chi Tiết & Đánh Giá {year}[/TITLE]");
                    sb.AppendLine($"[DESC]Tìm hiểu chi tiết về {name}. Xem đầy đủ thông tin, mô tả, hình ảnh và đánh giá mới nhất {year}.[/DESC]");
                    sb.AppendLine($"[KEY]{name}, chi tiết {name}, thông tin {name}[/KEY]");
                    sb.AppendLine();
                    sb.AppendLine($"💡 GỢI Ý CHUNG cho \"{name}\":");
                    sb.AppendLine($"• Đặt tiêu đề/tên đầy đủ, chứa từ khóa chính");
                    sb.AppendLine($"• Upload ảnh/media chất lượng cao");
                    break;
            }

            sb.AppendLine();
            sb.AppendLine("⚠️ Đây là gợi ý mẫu từ hệ thống.");

            return sb.ToString();
        }
    }

    public class AISuggestRequest
    {
        public string Module { get; set; } = "";
        public int ItemId { get; set; }
        public string? ItemName { get; set; }
    }

    public class ApplySeoRequest
    {
        public string Module { get; set; } = "";
        public int ItemId { get; set; }
        public string SeoTitle { get; set; } = "";
        public string SeoDescription { get; set; } = "";
        public string SeoKeywords { get; set; } = "";
    }

    public enum SeoSeverity { Good, Info, Warning, Error }

    public class SeoIssue
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public SeoSeverity Severity { get; set; }
        public string HowToFix { get; set; }
        public string FixUrl { get; set; }

        public SeoIssue(string title, string desc, SeoSeverity sev, string howToFix = "", string fixUrl = "")
        {
            Title = title;
            Description = desc;
            Severity = sev;
            HowToFix = howToFix;
            FixUrl = fixUrl;
        }
    }

    public class SeoAuditItem
    {
        public string Module { get; set; } = "";
        public string ModuleIcon { get; set; } = "bx-info-circle";
        public string ModuleColor { get; set; } = "secondary";
        public int ItemId { get; set; }
        public string ItemName { get; set; } = "";
        public string EditUrl { get; set; } = "";
        public string? ImageUrl { get; set; }
        public string? Slug { get; set; }
        public int Score { get; set; }
        public List<SeoIssue> Issues { get; set; } = new();

        public void CalculateScore()
        {
            if (!Issues.Any()) { Score = 100; return; }
            int penalty = 0;
            foreach (var issue in Issues)
            {
                switch (issue.Severity)
                {
                    case SeoSeverity.Error: penalty += 20; break;
                    case SeoSeverity.Warning: penalty += 10; break;
                    case SeoSeverity.Info: penalty += 3; break;
                    case SeoSeverity.Good: penalty -= 5; break;
                }
            }
            Score = Math.Max(0, Math.Min(100, 100 - penalty));
        }

        public string ScoreClass => Score >= 80 ? "success" : Score >= 50 ? "warning" : "danger";
        public string ScoreLabel => Score >= 80 ? "Tốt" : Score >= 50 ? "Cần cải thiện" : "Yếu";
    }

    public class SeoAuditReport
    {
        public List<SeoAuditItem> Products { get; set; } = new();
        public List<SeoAuditItem> News { get; set; } = new();
        public List<SeoAuditItem> Categories { get; set; } = new();
        public List<SeoAuditItem> Videos { get; set; } = new();
        public List<SeoAuditItem> Galleries { get; set; } = new();
        public List<SeoAuditItem> FAQs { get; set; } = new();
        public List<SeoAuditItem> Documents { get; set; } = new();


        public int AvgProductScore { get; set; }
        public int AvgNewsScore { get; set; }
        public int AvgCategoryScore { get; set; }
        public int AvgVideoScore { get; set; }
        public int AvgGalleryScore { get; set; }
        public int AvgFAQScore { get; set; }
        public int AvgDocumentScore { get; set; }

        public int OverallScore { get; set; }

        public int TotalErrors { get; set; }
        public int TotalWarnings { get; set; }
        public int TotalGood { get; set; }

        public int TotalItems { get; set; }
        public int TotalPages { get; set; }
        public int CurrentPage { get; set; }
        public string CurrentModule { get; set; } = "all";
        public string CurrentKw { get; set; } = "";
        public string CurrentScoreFilter { get; set; } = "all";
        public List<SeoAuditItem> PagedItems { get; set; } = new();

        public List<SeoAuditItem> GetAllItems() =>
            Products.Concat(News).Concat(Categories).Concat(Videos)
                    .Concat(Galleries).Concat(FAQs).Concat(Documents).ToList();

        public void CalculateSummary()
        {
            AvgProductScore = Products.Any() ? (int)Products.Average(x => x.Score) : 100;
            AvgNewsScore = News.Any() ? (int)News.Average(x => x.Score) : 100;
            AvgCategoryScore = Categories.Any() ? (int)Categories.Average(x => x.Score) : 100;
            AvgVideoScore = Videos.Any() ? (int)Videos.Average(x => x.Score) : 100;
            AvgGalleryScore = Galleries.Any() ? (int)Galleries.Average(x => x.Score) : 100;
            AvgFAQScore = FAQs.Any() ? (int)FAQs.Average(x => x.Score) : 100;
            AvgDocumentScore = Documents.Any() ? (int)Documents.Average(x => x.Score) : 100;


            var allItems = GetAllItems();
            OverallScore = allItems.Any() ? (int)allItems.Average(x => x.Score) : 100;

            TotalErrors = allItems.Sum(x => x.Issues.Count(i => i.Severity == SeoSeverity.Error));
            TotalWarnings = allItems.Sum(x => x.Issues.Count(i => i.Severity == SeoSeverity.Warning));
            TotalGood = allItems.Count(x => x.Score >= 80);
        }
    }
}
