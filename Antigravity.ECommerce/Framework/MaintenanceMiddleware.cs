using Microsoft.AspNetCore.Http;
using Antigravity.ECommerce.Services;
using System.Threading.Tasks;

namespace Antigravity.ECommerce.Framework
{
    public class MaintenanceMiddleware
    {
        private readonly RequestDelegate _next;

        public MaintenanceMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var path = context.Request.Path.Value?.ToLower();

            // Cho phép truy cập Admin, Static files và các trang bypass
            if (path.StartsWith("/admin") || 
                path.StartsWith("/assets") || 
                path.StartsWith("/lib") || 
                path.StartsWith("/error") ||
                path.StartsWith("/install") ||
                path.Contains("login"))
            {
                await _next(context);
                return;
            }

            // Lấy cấu hình hệ thống
            var settings = SSetting.GetViewModel();
            if (settings.MaintenanceMode)
            {
                context.Response.ContentType = "text/html; charset=utf-8";
                string html = $@"
                    <!DOCTYPE html>
                    <html>
                    <head>
                        <title>Website đang bảo trì</title>
                        <meta charset='utf-8'>
                        <style>
                            body {{ text-align: center; padding: 150px; font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif; background: #f8f9fa; color: #333; }}
                            h1 {{ font-size: 50px; color: #dc3545; }}
                            .content {{ font-size: 20px; color: #666; margin-top: 20px; }}
                            .logo {{ margin-bottom: 30px; }}
                        </style>
                    </head>
                    <body>
                        <div class='logo'>
                            <img src='{settings.Logo}' alt='Logo' style='max-height: 100px;'>
                        </div>
                        <h1>CHẾ ĐỘ BẢO TRÌ</h1>
                        <div class='content'>{settings.MaintenanceMessage}</div>
                        <hr style='max-width: 500px; margin: 40px auto; border: 0; border-top: 1px solid #dee2e6;'>
                        <p>Vui lòng quay lại sau. Xin cảm ơn!</p>
                    </body>
                    </html>";
                await context.Response.WriteAsync(html);
                return;
            }

            await _next(context);
        }
    }
}
