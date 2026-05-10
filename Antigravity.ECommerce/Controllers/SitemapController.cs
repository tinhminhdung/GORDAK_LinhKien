using Microsoft.AspNetCore.Mvc;
using Antigravity.ECommerce.Services;
using Antigravity.ECommerce.Models;
using System.Text;
using System.Xml;
using System.Linq;

namespace Antigravity.ECommerce.Controllers
{
    public class SitemapController : Controller
    {
        [Route("sitemap.xml")]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Index()
        {
            string baseUrl = $"{Request.Scheme}://{Request.Host}";
            
            var sb = new StringBuilder();
            sb.AppendLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
            sb.AppendLine("<urlset xmlns=\"http://www.sitemaps.org/schemas/sitemap/0.9\" xmlns:image=\"http://www.google.com/schemas/sitemap-image/1.1\">");

            // 1. HOME
            AddUrl(sb, baseUrl, "1.0", "daily", null, null);

            // 2. FIXED MODULE ROOTS
            AddUrl(sb, $"{baseUrl}/gioi-thieu.html", "0.9", "monthly", "Giới thiệu", null);
            AddUrl(sb, $"{baseUrl}/lien-he.html", "0.9", "monthly", "Liên hệ", null);
            AddUrl(sb, $"{baseUrl}/san-pham.html", "0.9", "daily", "Sản phẩm", null);
            AddUrl(sb, $"{baseUrl}/tin-tuc.html", "0.9", "daily", "Tin tức", null);
            AddUrl(sb, $"{baseUrl}/video.html", "0.8", "weekly", "Video", null);
            AddUrl(sb, $"{baseUrl}/chu-de-anh.html", "0.8", "weekly", "Thư viện ảnh", null);
            AddUrl(sb, $"{baseUrl}/tai-lieu.html", "0.7", "weekly", "Tài liệu", null);
            AddUrl(sb, $"{baseUrl}/faq.html", "0.7", "weekly", "Hỏi đáp", null);

            // 3. CATEGORIES
            try {
                var categories = SCategory.GetAll().Where(x => x.Status == 1).ToList();
                foreach (var cat in categories)
                {
                    string path = "";
                    switch (cat.CategoryType)
                    {
                        case 1: path = $"/san-pham/{cat.Slug}.html"; break;
                        case 2: path = $"/tin-tuc/{cat.Slug}.html"; break;
                        case 3: path = $"/video/{cat.Slug}"; break;
                        case 4: path = $"/bai-viet/{cat.Slug}.html"; break;
                        case 6: path = $"/chu-de-anh/{cat.Slug}"; break;
                        case 7: path = $"/faq/{cat.Slug}"; break;
                        case 8: path = $"/tai-lieu/{cat.Slug}"; break;
                    }
                    if (!string.IsNullOrEmpty(path)) {
                        AddUrl(sb, baseUrl + path, "0.8", "weekly", cat.Name, cat.Image);
                    }
                }
            } catch { }

            // 4. PRODUCTS (Detail)
            try {
                var products = SProduct.Search(null, null, 1, null, null, null, "ProductId", "DESC", 1, 10000);
                foreach (var p in products)
                {
                    AddUrl(sb, $"{baseUrl}/san-pham/detail/{p.Slug}-{p.ProductId}.html", "0.9", "daily", p.Name, p.MainImage);
                }
            } catch { }

            // 5. NEWS (Detail)
            try {
                var news = SNews.GetAll(2000).Where(x => x.Status == 1).ToList();
                foreach (var n in news)
                {
                    AddUrl(sb, $"{baseUrl}/tin-tuc/detail/{n.Slug}.html", "0.7", "weekly", n.Title, n.Image);
                }
            } catch { }

            // 6. VIDEOS (Detail)
            try {
                var videos = SVideo.GetAll().Where(x => x.Status == 1).ToList();
                foreach (var v in videos)
                {
                    if (!string.IsNullOrEmpty(v.Slug))
                        AddUrl(sb, $"{baseUrl}/video/detail/{v.Slug}", "0.6", "monthly", v.Title, v.ThumbnailUrl);
                }
            } catch { }

            // 7. GALLERIES (Detail)
            try {
                var galleries = SGallery.GetAll().Where(x => x.Status == 1).ToList();
                foreach (var g in galleries)
                {
                    if (!string.IsNullOrEmpty(g.Slug))
                        AddUrl(sb, $"{baseUrl}/album/{g.Slug}", "0.6", "monthly", g.AlbumName, g.CoverImage);
                }
            } catch { }

            // 8. FAQs (Detail)
            try {
                var faqs = SFAQ.GetAll().Where(x => x.Status == 1).ToList();
                foreach (var f in faqs)
                {
                    if (!string.IsNullOrEmpty(f.Slug))
                        AddUrl(sb, $"{baseUrl}/faq/{f.Slug}", "0.5", "monthly", f.Question, null);
                }
            } catch { }

            // 9. DOCUMENTS (Detail)
            try {
                var docs = SDocument.GetAll().Where(x => x.Status == 1).ToList();
                foreach (var d in docs)
                {
                    if (!string.IsNullOrEmpty(d.Slug))
                        AddUrl(sb, $"{baseUrl}/tai-lieu/{d.Slug}", "0.5", "monthly", d.Title, null);
                }
            } catch { }

            sb.AppendLine("</urlset>");
            return Content(sb.ToString(), "application/xml", Encoding.UTF8);
        }

        private void AddUrl(StringBuilder sb, string url, string priority, string frequency, string? title, string? image)
        {
            // Lấy logo làm ảnh mặc định nếu không có ảnh cụ thể
            if (string.IsNullOrEmpty(image))
            {
                image = SSetting.GetValue("Logo");
            }
            
            sb.AppendLine("  <url>");
            sb.AppendLine($"    <loc>{XmlEscape(url)}</loc>");
            sb.AppendLine($"    <changefreq>{frequency}</changefreq>");
            sb.AppendLine($"    <priority>{priority}</priority>");
            
            if (!string.IsNullOrEmpty(image))
            {
                string imageUrl = image.StartsWith("http") ? image : $"{Request.Scheme}://{Request.Host}{image}";
                sb.AppendLine("    <image:image>");
                sb.AppendLine($"      <image:loc>{XmlEscape(imageUrl)}</image:loc>");
                if (!string.IsNullOrEmpty(title))
                {
                    sb.AppendLine($"      <image:title>{XmlEscape(title)}</image:title>");
                }
                sb.AppendLine("    </image:image>");
            }
            sb.AppendLine("  </url>");
        }

        private string XmlEscape(string unescaped)
        {
            if (string.IsNullOrEmpty(unescaped)) return "";
            return System.Security.SecurityElement.Escape(unescaped);
        }
    }
}
