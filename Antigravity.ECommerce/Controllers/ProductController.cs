using Microsoft.AspNetCore.Mvc;
using Antigravity.ECommerce.Framework;
using Antigravity.ECommerce.Models;
using Antigravity.ECommerce.Services;
using System.Linq;
using System.Text.Json;
using System.Collections.Generic;

namespace Antigravity.ECommerce.Controllers
{
    public class ProductController : Controller
    {
        #region 1. Danh sách sản phẩm
        /// <summary>
        /// Trang hiển thị danh sách tất cả sản phẩm (Không có bộ lọc)
        /// </summary>
        public IActionResult Index(int page = 1)
        {
            int pageSize = int.TryParse(SSetting.GetValue("Product_PageSize"), out int pps) ? pps : 12;

            // Sử dụng SCache để lưu trữ kết quả truy vấn, giảm tải cho Database
            var products = SCache.GetOrSet($"Product_Index_Page_{page}_{pageSize}", () => FProduct.Search(null, null, 1, null, null, null, "CreatedAt", "DESC", page, pageSize), 60);
            
            ViewBag.PageCount = products.Count > 0 ? (int)System.Math.Ceiling((double)products[0].TotalCount / pageSize) : 0;
            ViewBag.PageIndex = page;
            ViewBag.Category = new Category { Name = "Tất cả sản phẩm" };

            // Cấu hình thẻ meta SEO cho trang danh sách tổng
            ViewData["Title"] = "Tất cả sản phẩm";
            ViewData["Description"] = "Khám phá toàn bộ sản phẩm chính hãng với giá tốt nhất.";

            return View("Category", products);
        }

        /// <summary>
        /// Trang hiển thị danh sách sản phẩm theo Danh mục cụ thể (Có hỗ trợ lọc giá, sắp xếp)
        /// </summary>
        public IActionResult Category(string slug, decimal? priceMin, decimal? priceMax, string sort = "newest", int page = 1)
        {
            // Lấy thông tin danh mục từ DB, có đệm Cache 60 phút
            var category = SCache.GetOrSet($"Category_{slug}", () => FCategory.GetBySlug(slug), 60);
            if (category == null) return NotFound();

            // Ánh xạ tham số sắp xếp từ URL sang cột trong DB
            string sortColumn = sort == "price_desc" || sort == "price_asc" ? "Price" : "CreatedAt";
            string sortOrder = sort == "price_asc" ? "ASC" : "DESC";

            int pageSize = int.TryParse(SSetting.GetValue("Product_PageSize"), out int pps) ? pps : 12;

            // Tạo khóa Cache linh hoạt dựa trên các bộ lọc
            string cacheKey = $"Product_Category_{category.CategoryId}_{priceMin}_{priceMax}_{sort}_{page}_{pageSize}";
            
            // Lấy toàn bộ ID của danh mục con cháu để truyền vào SQL Stored Procedure
            var allCatIds = SCategory.GetDescendantIds(category.CategoryId);
            string catIdString = string.Join(",", allCatIds);

            // Giảm thời gian Cache xuống 5 phút nếu người dùng có tương tác lọc giá/sắp xếp để tránh phình rác bộ nhớ (Cache Bloat)
            int cacheMinutes = (priceMin.HasValue || priceMax.HasValue || sort != "newest") ? 5 : 60;
            var products = SCache.GetOrSet(cacheKey, () => FProduct.Search(null, catIdString, 1, null, priceMin, priceMax, sortColumn, sortOrder, page, pageSize), cacheMinutes);
            
            ViewBag.Category = category;
            ViewBag.PageCount = products.Count > 0 ? (int)System.Math.Ceiling((double)products[0].TotalCount / pageSize) : 0;
            ViewBag.PageIndex = page;

            // Truyền dữ liệu SEO của danh mục ra View
            ViewData["Title"] = !string.IsNullOrEmpty(category.SeoTitle) ? category.SeoTitle : category.Name;
            ViewData["Description"] = category.SeoDescription;
            ViewData["Keywords"] = category.SeoKeywords;
            if (!string.IsNullOrEmpty(category.Image)) ViewData["Image"] = category.Image;

            return View(products);
        }

        /// <summary>
        /// Trang tìm kiếm sản phẩm (Hỗ trợ tìm kiếm không dấu, gần đúng)
        /// </summary>
        public IActionResult Search(string q = "", decimal? priceMin = null, decimal? priceMax = null, string sort = "newest", int page = 1)
        {
            if (string.IsNullOrWhiteSpace(q))
                return RedirectToAction("Index");

            // Ánh xạ tham số sắp xếp từ URL sang cột trong DB
            string sortColumn = sort == "price_desc" || sort == "price_asc" ? "Price" : "CreatedAt";
            string sortOrder = sort == "price_asc" ? "ASC" : "DESC";

            int pageSize = int.TryParse(SSetting.GetValue("Product_PageSize"), out int pps) ? pps : 12;

            // Xử lý key cache linh hoạt
            string cacheKey = $"Product_Search_{q}_{priceMin}_{priceMax}_{sort}_{page}_{pageSize}";
            
            int cacheMinutes = (priceMin.HasValue || priceMax.HasValue || sort != "newest") ? 5 : 60;
            var products = SCache.GetOrSet(cacheKey, () => FProduct.Search(q, null, 1, null, priceMin, priceMax, sortColumn, sortOrder, page, pageSize), cacheMinutes);
            
            ViewBag.Keyword = q;
            ViewBag.PageCount = products.Count > 0 ? (int)System.Math.Ceiling((double)products[0].TotalCount / pageSize) : 0;
            ViewBag.PageIndex = page;
            
            ViewData["Title"] = "Tìm kiếm: " + q;
            ViewData["Description"] = "Kết quả tìm kiếm cho từ khóa: " + q;

            // Dùng chung View "Category" để hiển thị
            return View("Category", products);
        }
        #endregion

        #region 2. Chi tiết sản phẩm
        /// <summary>
        /// Trang xem chi tiết một sản phẩm
        /// </summary>
        public IActionResult Detail(string slug, int id)
        {
            // Load thông tin sản phẩm có cache theo ID (ưu tiên ID để tránh lỗi khi đổi slug)
            var product = SCache.GetOrSet($"Product_Detail_Id_{id}", () => FProduct.GetById(id), 60);
            if (product == null) return NotFound();

            // Tăng lượt xem
            SSeo.IncrementViewCount("Products", "ProductId", id);

            // 301 Redirect nếu slug trong URL không khớp slug thực (SEO-friendly)
            if (!string.IsNullOrEmpty(product.Slug) && product.Slug != slug)
            {
                return RedirectPermanent($"/san-pham/detail/{product.Slug}-{id}.html");
            }

            // Xử lý lấy danh sách Sản phẩm liên quan (Hiển thị ở dưới cùng trang chi tiết)
            List<Product> relatedList = new List<Product>();
            
            // Bước 1: Ưu tiên lấy các sản phẩm do Admin chỉ định cấu hình thủ công (Dựa trên chuỗi SKU)
            if (!string.IsNullOrEmpty(product.RelatedProducts))
            {
                relatedList = SCache.GetOrSet($"Product_Related_{product.ProductId}", () => {
                    var skus = product.RelatedProducts.Split(',', System.StringSplitOptions.RemoveEmptyEntries);
                    return FProduct.GetBySkus(skus);
                }, 60);
            }
            
            // Bước 2: Nếu Admin không cài đặt hoặc thiếu số lượng (< 6), tự động lấy thêm sản phẩm cùng danh mục
            if (relatedList.Count < 6)
            {
                var catRelated = SCache.GetOrSet($"Product_CatRelated_{product.ProductId}", () => {
                    return FProduct.Search(null, product.CategoryIds, 1, null, null, null, "CreatedAt", "DESC", 1, 12)
                                         .Where(x => x.ProductId != product.ProductId)
                                         .ToList();
                }, 60);
                
                foreach (var p in catRelated)
                {
                    if (relatedList.Count >= 6) break;
                    if (!relatedList.Any(x => x.ProductId == p.ProductId))
                    {
                        relatedList.Add(p);
                    }
                }
            }
            
            ViewBag.RelatedProducts = relatedList.Take(6).ToList();

            // Lấy danh sách Phụ kiện liên quan
            List<Product> accessoriesList = new List<Product>();
            if (!string.IsNullOrEmpty(product.Accessories))
            {
                accessoriesList = SCache.GetOrSet($"Product_Accessories_{product.ProductId}", () => {
                    var skus = product.Accessories.Split(',', System.StringSplitOptions.RemoveEmptyEntries);
                    return FProduct.GetBySkus(skus);
                }, 60);
            }
            ViewBag.Accessories = accessoriesList;

            // Xử lý Sản phẩm đã xem (Viewed Products) lưu qua Cookie
            string viewedCookie = Request.Cookies["ViewedProducts"] ?? "";
            var viewedListIds = viewedCookie.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList();
            
            // Xóa ID hiện tại nếu đã có để đưa lên đầu
            viewedListIds.Remove(product.ProductId.ToString());
            viewedListIds.Insert(0, product.ProductId.ToString());
            
            // Giữ lại tối đa 10 sản phẩm
            if (viewedListIds.Count > 10) viewedListIds = viewedListIds.Take(10).ToList();
            
            // Cập nhật lại Cookie
            Response.Cookies.Append("ViewedProducts", string.Join(",", viewedListIds), new CookieOptions { Expires = System.DateTime.Now.AddDays(30) });

            // Lấy data sản phẩm đã xem (trừ sản phẩm hiện tại)
            var viewedSkusToFetch = viewedListIds.Skip(1).Take(5).ToList();
            List<Product> viewedProducts = new List<Product>();
            if (viewedSkusToFetch.Count > 0)
            {
                foreach(var idStr in viewedSkusToFetch)
                {
                    if (int.TryParse(idStr, out int vId))
                    {
                        var vProd = SCache.GetOrSet($"Product_Detail_Id_{vId}", () => FProduct.GetById(vId), 60);
                        if (vProd != null) viewedProducts.Add(vProd);
                    }
                }
            }
            ViewBag.ViewedProducts = viewedProducts;

            // Lấy thông tin thống kê số sao đánh giá
            var reviewStats = FReview.GetProductStats(product.ProductId);
            var reviews = FReview.GetByProductId(product.ProductId, null, 1, 10);
            
            ViewBag.ReviewStats = reviewStats;
            ViewBag.Reviews = reviews;
            ViewData["SchemaReviewStats"] = reviewStats;

            // Load sidebar data: Cam kết từ Gordak
            ViewBag.Commitments = SAdvertising.GetByPosition("Product_Commitment");
            // Load sidebar data: Chứng nhận đại lý chính thức
            ViewBag.DealerCert = SAdvertising.GetByPosition("Product_DealerCert");
            // Load settings cho Zalo link
            ViewBag.Settings = SSetting.GetViewModel();

            // Xây dựng SEO Meta Tags động cho trang chi tiết
            ViewData["Title"] = !string.IsNullOrEmpty(product.SeoTitle) ? product.SeoTitle : product.Name;
            ViewData["Description"] = !string.IsNullOrEmpty(product.SeoDescription) ? product.SeoDescription : product.ShortDescription;
            ViewData["Keywords"] = product.SeoKeywords;

            if (!string.IsNullOrEmpty(product.MainImage)) ViewData["Image"] = product.MainImage;

            return View(product);
        }
        #endregion

        #region 3. Tính năng Đánh giá (AJAX API)
        /// <summary>
        /// API Xử lý hành động gửi đánh giá của khách hàng
        /// </summary>
        [HttpPost]
        public IActionResult SubmitReview(int productId, int rating, string? title, string? content)
        {
            // Bắt buộc đăng nhập để đánh giá
            var sessionStr = HttpContext.Session.GetString("CustomerSession");
            if (string.IsNullOrEmpty(sessionStr))
                return Json(new { success = false, message = "Vui lòng đăng nhập để đánh giá" });

            var customer = JsonSerializer.Deserialize<Customer>(sessionStr);
            if (customer == null)
                return Json(new { success = false, message = "Phiên đăng nhập không hợp lệ" });

            if (rating < 1 || rating > 5)
                return Json(new { success = false, message = "Số sao không hợp lệ" });

            // Kiểm tra chống SPAM (mỗi sản phẩm chỉ đánh giá 1 lần)
            if (FReview.HasReviewed(customer.CustomerId, productId))
                return Json(new { success = false, message = "Bạn đã đánh giá sản phẩm này rồi" });

            var product = FProduct.GetById(productId);
            if (product == null)
                return Json(new { success = false, message = "Sản phẩm không tồn tại" });

            // Kiểm tra xem khách hàng này đã Từng mua hàng thành công sản phẩm này chưa (Trạng thái = 3: Đã hoàn thành)
            var checkPurchaseResult = BaseConnectionSql.ExecuteScalar("SELECT COUNT(1) FROM OrderDetails d JOIN Orders o ON d.OrderId = o.OrderId WHERE o.CustomerId = @Cid AND o.Status = 3 AND d.ProductId = @Pid", 
                new Microsoft.Data.SqlClient.SqlParameter[] { 
                    new Microsoft.Data.SqlClient.SqlParameter("@Cid", customer.CustomerId),
                    new Microsoft.Data.SqlClient.SqlParameter("@Pid", productId)});
            bool hasPurchased = checkPurchaseResult != null && Convert.ToInt32(checkPurchaseResult) > 0;

            var review = new ProductReview
            {
                ProductId = productId,
                CustomerId = customer.CustomerId,
                Rating = rating,
                Title = title,
                Content = content,
                IsVerifiedPurchase = hasPurchased, // Đánh dấu mác "Đã mua hàng" hiển thị lên UI
                Status = 1 // Ở đây set mặc định là 1 (Hiện luôn). Có thể đổi thành 0 nếu muốn duyệt trước.
            };

            FReview.Insert(review);
            
            // Cực kỳ quan trọng: Phải xóa Cache của sản phẩm này để UI cập nhật số sao ngay lập tức
            SCache.Remove("Product_Detail_Id_" + product.ProductId);
            
            return Json(new { success = true, message = "Đánh giá của bạn đã được gửi!" });
        }

        /// <summary>
        /// API Load thêm danh sách đánh giá khi người dùng bấm Chuyển trang (Phân trang AJAX)
        /// </summary>
        [HttpGet]
        public IActionResult GetReviews(int productId, int? rating = null, int page = 1)
        {
            var reviews = FReview.GetByProductId(productId, rating, page, 5);
            var totalCount = reviews.Count > 0 ? reviews[0].TotalCount : 0;
            return Json(new { reviews = reviews, totalCount = totalCount, page = page });
        }
        #endregion
    }
}

