using Microsoft.AspNetCore.Http;
using System;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;

namespace Antigravity.ECommerce.Framework
{
    public class VisitorTrackingMiddleware
    {
        private readonly RequestDelegate _next;

        public VisitorTrackingMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var path = context.Request.Path.Value?.ToLower() ?? "";
            
            // Bỏ qua không track /admin và các tài nguyên tĩnh
            if (!path.StartsWith("/admin") && !path.StartsWith("/css") && 
                !path.StartsWith("/js") && !path.StartsWith("/assets") && 
                !path.StartsWith("/lib") && !path.StartsWith("/uploads"))
            {
                var ipAddress = context.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
                var url = context.Request.Path + context.Request.QueryString;
                var userAgent = context.Request.Headers["User-Agent"].ToString();
                var referer = context.Request.Headers["Referer"].ToString();
                
                // Kích hoạt Session
                var sessionId = context.Session.Id;

                // Fire and forget để không cản trở request chính
                _ = Task.Run(() => TrackVisitorAsync(ipAddress, url, userAgent, referer, sessionId));
            }

            await _next(context);
        }

        private async Task TrackVisitorAsync(string ipAddress, string url, string userAgent, string referer, string sessionId)
        {
            try 
            {
                string source = "Direct";
                if (!string.IsNullOrEmpty(referer))
                {
                    if (referer.Contains("google.")) source = "Google";
                    else if (referer.Contains("facebook.com")) source = "Facebook";
                    else source = "Referral";
                }

                var connStr = Antigravity.ECommerce.Services.BaseConnectionSql.ConnectionString;
                using (var conn = new SqlConnection(connStr))
                {
                    await conn.OpenAsync();
                    var cmd = new SqlCommand(@"
                        INSERT INTO VisitorLogs (IPAddress, Url, UserAgent, Referer, Source, SessionId, VisitedAt)
                        VALUES (@IP, @Url, @UA, @Ref, @Source, @SessionId, GETDATE())
                    ", conn);
                    cmd.Parameters.AddWithValue("@IP", ipAddress);
                    cmd.Parameters.AddWithValue("@Url", url);
                    cmd.Parameters.AddWithValue("@UA", string.IsNullOrEmpty(userAgent) ? DBNull.Value : userAgent);
                    cmd.Parameters.AddWithValue("@Ref", string.IsNullOrEmpty(referer) ? DBNull.Value : referer);
                    cmd.Parameters.AddWithValue("@Source", source);
                    cmd.Parameters.AddWithValue("@SessionId", sessionId);
                    
                    await cmd.ExecuteNonQueryAsync();
                }
            } 
            catch {
                // Ignore tracking failures to not disrupt anything
            }
        }
    }
}
