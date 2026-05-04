using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Antigravity.ECommerce.Framework
{
    /// <summary>
    /// Middleware tự động phục vụ ảnh WebP thumbnail thay vì ảnh gốc.
    /// ZERO-CONFIG: Không cần sửa bất kỳ View nào. Tất cả ảnh trong /uploads/
    /// sẽ tự động được trả về bản WebP nhẹ nếu trình duyệt hỗ trợ.
    /// 
    /// ẢNH GỐC KHÔNG BAO GIỜ BỊ THAY ĐỔI - Middleware chỉ ĐỌC ảnh gốc để tạo thumb.
    /// </summary>
    public class ImageOptimizeMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IWebHostEnvironment _env;

        private static readonly string[] ImageExtensions = { ".jpg", ".jpeg", ".png", ".bmp", ".gif" };

        public ImageOptimizeMiddleware(RequestDelegate next, IWebHostEnvironment env)
        {
            _next = next;
            _env = env;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var path = context.Request.Path.Value;

            // Chỉ xử lý request ảnh trong /uploads/
            if (path == null || !path.StartsWith("/uploads/", StringComparison.OrdinalIgnoreCase))
            {
                await _next(context);
                return;
            }

            // Bỏ qua nếu đang request file .thumbs/ trực tiếp
            if (path.Contains("/.thumbs/", StringComparison.OrdinalIgnoreCase))
            {
                await _next(context);
                return;
            }

            // Nếu tính năng tối ưu bị tắt -> Trả về ảnh gốc ngay lập tức
            if (!Services.ImageOptimizerService.EnableOptimization)
            {
                await _next(context);
                return;
            }

            // Chỉ xử lý các file ảnh
            var ext = Path.GetExtension(path).ToLowerInvariant();
            if (Array.IndexOf(ImageExtensions, ext) < 0)
            {
                await _next(context);
                return;
            }

            // Kiểm tra trình duyệt có hỗ trợ WebP không
            var acceptHeader = context.Request.Headers["Accept"].ToString();
            if (!acceptHeader.Contains("image/webp", StringComparison.OrdinalIgnoreCase))
            {
                await _next(context);
                return;
            }

            // Tìm file gốc
            var originalPhysicalPath = Path.Combine(_env.WebRootPath, path.TrimStart('/').Replace("/", Path.DirectorySeparatorChar.ToString()));
            if (!File.Exists(originalPhysicalPath))
            {
                await _next(context);
                return;
            }

            // Tìm thumbnail
            var thumbPhysicalPath = Services.ImageOptimizerService.GetThumbnailPath(originalPhysicalPath);

            // Lazy Generation: Nếu chưa có thumb → Tạo ngay lần đầu (ẢNH GỐC KHÔNG ĐỤNG)
            if (!File.Exists(thumbPhysicalPath))
            {
                Services.ImageOptimizerService.GenerateThumbnail(originalPhysicalPath);
            }

            // Nếu có thumb → Trả về WebP
            if (File.Exists(thumbPhysicalPath))
            {
                var fileInfo = new FileInfo(thumbPhysicalPath);
                var etag = $"\"{fileInfo.LastWriteTimeUtc.Ticks:x}\"";

                context.Response.Headers.Append("Cache-Control", "public, max-age=0, must-revalidate");
                context.Response.Headers.Append("ETag", etag);

                if (context.Request.Headers.ContainsKey("If-None-Match") && context.Request.Headers["If-None-Match"] == etag)
                {
                    context.Response.StatusCode = 304;
                    return;
                }

                context.Response.ContentType = "image/webp";
                context.Response.Headers.Append("X-Image-Optimized", "true");

                await context.Response.SendFileAsync(thumbPhysicalPath);
                return;
            }

            // Fallback: Trả ảnh gốc
            await _next(context);
        }
    }
}
