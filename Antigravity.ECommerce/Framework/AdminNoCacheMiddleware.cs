using Microsoft.AspNetCore.Http;
using Antigravity.ECommerce.Services;
using System.Threading.Tasks;

namespace Antigravity.ECommerce.Framework
{
    public class AdminNoCacheMiddleware
    {
        private readonly RequestDelegate _next;

        public AdminNoCacheMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Lưu lại BaseUrl để dùng cho Sitemap background task
            SSeo.CurrentBaseUrl = $"{context.Request.Scheme}://{context.Request.Host}";

            // Nếu là request vào Admin, đánh dấu BypassCache
            if (context.Request.Path.Value != null && 
                (context.Request.Path.Value.StartsWith("/Admin", System.StringComparison.OrdinalIgnoreCase) ||
                 context.Request.Path.Value.StartsWith("/AdminProduct", System.StringComparison.OrdinalIgnoreCase) ||
                 context.Request.Path.Value.StartsWith("/AdminNews", System.StringComparison.OrdinalIgnoreCase) ||
                 context.Request.Path.Value.StartsWith("/AdminVideo", System.StringComparison.OrdinalIgnoreCase) ||
                 context.Request.Path.Value.StartsWith("/AdminGallery", System.StringComparison.OrdinalIgnoreCase) ||
                 context.Request.Path.Value.StartsWith("/AdminFAQ", System.StringComparison.OrdinalIgnoreCase) ||
                 context.Request.Path.Value.StartsWith("/AdminDocument", System.StringComparison.OrdinalIgnoreCase) ||
                 context.Request.Path.Value.StartsWith("/AdminSeo", System.StringComparison.OrdinalIgnoreCase)))
            {
                SCache.BypassCache.Value = true;
            }

            await _next(context);
        }
    }
}
