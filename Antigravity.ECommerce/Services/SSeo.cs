using Microsoft.AspNetCore.Http;
using System.Text;
using System.Xml.Linq;
using Antigravity.ECommerce.Services;
using Antigravity.ECommerce.Framework;

namespace Antigravity.ECommerce.Services
{
    public static class SSeo
    {
        public static void RefreshSitemapAndCache()
        {
            // Clear Cache immediately
            SCache.ClearAll();

            // Run Sitemap Generation in Background Task
            Task.Run(() => {
                try {
                    // Note: Since we are in a background thread, we don't have HttpContext.
                    // We need to either pass the BaseUrl or determine it from settings or assume it's set in a known place.
                    // For simplicity, we will fetch the site URL if possible or just generate relative paths if needed, 
                    // but sitemaps NEED absolute URLs.
                    
                    // We will use a static method to generate sitemap that takes a baseUrl.
                    // However, we don't have the baseUrl here. 
                    // Usually, for background tasks, we'd have a configurated SiteUrl in settings.
                    
                    GenerateFullSitemap();
                } catch (Exception ex) {
                    // Fail-safe: ensure we have something written or at least logged in a real app
                    System.Diagnostics.Debug.WriteLine("Sitemap Error: " + ex.Message);
                }
            });
        }

        public static void GenerateFullSitemap(string? baseUrl = null)
        {
            try
            {
                if (string.IsNullOrEmpty(baseUrl))
                {
                    baseUrl = SSetting.GetValue("SiteUrl");
                    if (string.IsNullOrEmpty(baseUrl)) baseUrl = "https://localhost:7057";
                }
                if (baseUrl.EndsWith("/")) baseUrl = baseUrl.TrimEnd('/');

                string logo = SSetting.GetValue("Logo");

                var ns = XNamespace.Get("http://www.sitemaps.org/schemas/sitemap/0.9");
                var imageNs = XNamespace.Get("http://www.google.com/schemas/sitemap-image/1.1");
                
                var root = new XElement(ns + "urlset", 
                    new XAttribute(XName.Get("image", "http://www.w3.org/2000/xmlns/"), imageNs.NamespaceName)
                );

                void AddUrl(string path, string priority, string frequency, string? title = null, IEnumerable<string>? images = null)
                {
                    // Fallback logo
                    if (images == null || !images.Any(x => !string.IsNullOrWhiteSpace(x))) 
                    {
                        if (!string.IsNullOrEmpty(logo))
                        {
                            images = new List<string> { logo };
                        }
                    }

                    var urlEl = new XElement(ns + "url",
                        new XElement(ns + "loc", baseUrl + path),
                        new XElement(ns + "changefreq", frequency),
                        new XElement(ns + "priority", priority)
                    );

                    if (images != null)
                    {
                        var addedImages = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                        foreach (var img in images)
                        {
                            if (string.IsNullOrWhiteSpace(img)) continue;
                            string imageUrl = img.StartsWith("http") ? img : baseUrl + img;
                            if (addedImages.Contains(imageUrl)) continue; // Tránh trùng lặp ảnh trong cùng 1 bài viết
                            addedImages.Add(imageUrl);

                            var imgEl = new XElement(imageNs + "image",
                                new XElement(imageNs + "loc", imageUrl)
                            );
                            if (!string.IsNullOrEmpty(title))
                            {
                                imgEl.Add(new XElement(imageNs + "title", title));
                            }
                            urlEl.Add(imgEl);
                        }
                    }
                    root.Add(urlEl);
                }

                // 1. Home & Module Roots
                AddUrl("/", "1.0", "daily");
                AddUrl("/san-pham.html", "0.9", "daily");
                AddUrl("/tin-tuc.html", "0.9", "daily");
                AddUrl("/video.html", "0.8", "weekly");
                AddUrl("/chu-de-anh.html", "0.8", "weekly");
                AddUrl("/tai-lieu.html", "0.7", "weekly");
                AddUrl("/faq.html", "0.7", "weekly");

                // 2. Categories
                var cats = FCategory.GetAll().Where(x => x.Status == 1);
                foreach (var cat in cats)
                {
                    string path = cat.CategoryType switch {
                        1 => $"/san-pham/{cat.Slug}.html",
                        2 => $"/tin-tuc/{cat.Slug}.html",
                        3 => $"/video/{cat.Slug}",
                        4 => $"/bai-viet/{cat.Slug}.html",
                        6 => $"/chu-de-anh/{cat.Slug}",
                        7 => $"/faq/{cat.Slug}",
                        8 => $"/tai-lieu/{cat.Slug}",
                        _ => ""
                    };
                    if (!string.IsNullOrEmpty(path))
                        AddUrl(path, "0.8", "weekly", cat.Name, new[] { cat.Image ?? "" });
                }

                // 3. Products (Detail) - Fetch all active products
                var products = SProduct.Search(null, null, 1, null, null, null, "ProductId", "DESC", 1, 50000);
                foreach (var p in products)
                {
                    var imgList = new List<string>();
                    if (!string.IsNullOrEmpty(p.MainImage)) imgList.Add(p.MainImage);
                    if (!string.IsNullOrEmpty(p.ImageGallery)) 
                    {
                        var extraImgs = p.ImageGallery.Split(',', StringSplitOptions.RemoveEmptyEntries);
                        imgList.AddRange(extraImgs);
                    }
                    AddUrl($"/san-pham/detail/{p.Slug}-{p.ProductId}.html", "0.9", "daily", p.Name, imgList);
                }

                // 4. News (Detail) - Fetch all active news
                var newsList = SNews.GetAll(10000).Where(x => x.Status == 1);
                foreach (var n in newsList)
                {
                    AddUrl($"/tin-tuc/detail/{n.Slug}.html", "0.7", "weekly", n.Title, new[] { n.Image ?? "" });
                }

                // 5. Videos (Detail)
                var videos = SVideo.GetAll().Where(x => x.Status == 1);
                foreach (var v in videos)
                {
                    if (!string.IsNullOrEmpty(v.Slug))
                        AddUrl($"/video/detail/{v.Slug}", "0.6", "monthly", v.Title, new[] { v.ThumbnailUrl ?? "" });
                }

                // 6. Galleries (Detail)
                var galleries = SGallery.GetAll().Where(x => x.Status == 1);
                foreach (var g in galleries)
                {
                    if (!string.IsNullOrEmpty(g.Slug))
                        AddUrl($"/album/{g.Slug}", "0.6", "monthly", g.AlbumName, new[] { g.CoverImage ?? "" });
                }

                // 7. FAQs (Detail)
                var faqs = SFAQ.GetAll().Where(x => x.Status == 1);
                foreach (var f in faqs)
                {
                    if (!string.IsNullOrEmpty(f.Slug))
                        AddUrl($"/faq/{f.Slug}", "0.5", "monthly", f.Question, null);
                }

                // 8. Documents (Detail)
                var docs = SDocument.GetAll().Where(x => x.Status == 1);
                foreach (var d in docs)
                {
                    if (!string.IsNullOrEmpty(d.Slug))
                        AddUrl($"/tai-lieu/{d.Slug}", "0.5", "monthly", d.Title, null);
                }

                var doc = new XDocument(new XDeclaration("1.0", "utf-8", "yes"), root);
                var wwwrootPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
                if (!Directory.Exists(wwwrootPath)) Directory.CreateDirectory(wwwrootPath);
                
                var pathFile = Path.Combine(wwwrootPath, "sitemap.xml");
                if (File.Exists(pathFile)) File.Delete(pathFile); // Đảm bảo xóa file cũ
                doc.Save(pathFile);
            }
            catch { }
        }
    }
}
