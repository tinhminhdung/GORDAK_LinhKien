using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Mail;
using Antigravity.ECommerce.Models;

namespace Antigravity.ECommerce.Services
{
    public class SEmailSender
    {
        public static System.Collections.Concurrent.ConcurrentQueue<string> ErrorQueue = new System.Collections.Concurrent.ConcurrentQueue<string>();

        public static void SendFromTemplateAsync(string templateKey, string toEmail, string subject, Dictionary<string, string> replacements)
        {
            Task.Run(() => 
            {
                var (success, error) = SendFromTemplate(templateKey, toEmail, subject, replacements);
                if (!success && !string.IsNullOrEmpty(error))
                {
                    ErrorQueue.Enqueue($"Lỗi gửi mail đến {toEmail}: {error}");
                }
            });
        }

        public static (bool Success, string? ErrorMessage) SendEmail(string toEmail, string subject, string body)
        {
            try
            {
                var settings = SSetting.GetViewModel();
                if (string.IsNullOrEmpty(settings.SmtpServer) || string.IsNullOrEmpty(settings.SmtpEmail) || string.IsNullOrEmpty(settings.SmtpPassword))
                {
                    return (false, "Cấu hình SMTP chưa đầy đủ (Server, Email hoặc Password trống)");
                }

                var smtpClient = new SmtpClient(settings.SmtpServer)
                {
                    Port = settings.SmtpPort > 0 ? settings.SmtpPort : 587,
                    UseDefaultCredentials = false,
                    Credentials = new NetworkCredential(settings.SmtpEmail, settings.SmtpPassword),
                    EnableSsl = true,
                };

                var mailMessage = new MailMessage
                {
                    From = new MailAddress(settings.SmtpEmail, settings.SiteTitle),
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = true,
                };
                mailMessage.To.Add(toEmail);

                smtpClient.Send(mailMessage);
                return (true, null);
            }
            catch (Exception ex)
            {
                return (false, $"Lỗi gửi email: {ex.Message}");
            }
        }

        /// <summary>
        /// Gửi email dùng template từ Settings. Hỗ trợ biến: {CustomerName}, {OrderCode}, {TotalAmount}, {OrderDate}, {StatusName}, {NewPassword}, {SiteName}
        /// </summary>
        public static (bool Success, string? ErrorMessage) SendFromTemplate(string templateKey, string toEmail, string subject, Dictionary<string, string> replacements)
        {
            var settings = SSetting.GetViewModel();
            string template = templateKey switch
            {
                "OrderConfirm" => settings.EmailTemplateOrderConfirm ?? "",
                "OrderStatus" => settings.EmailTemplateOrderStatus ?? "",
                "PasswordReset" => settings.EmailTemplatePasswordReset ?? "",
                _ => ""
            };

            // Use default template if none configured
            if (string.IsNullOrWhiteSpace(template))
            {
                template = GetDefaultTemplate(templateKey);
            }

            // Replace variables
            replacements["SiteName"] = settings.SiteTitle;
            foreach (var kv in replacements)
            {
                template = template.Replace($"{{{kv.Key}}}", kv.Value);
            }

            return SendEmail(toEmail, subject, template);
        }

        private static string GetDefaultTemplate(string key)
        {
            return key switch
            {
                "OrderConfirm" => "<div style='font-family:Arial,sans-serif;max-width:600px;margin:0 auto;'>" +
                    "<h2 style='color:#405189;'>✅ Đặt hàng thành công!</h2>" +
                    "<p>Xin chào <strong>{CustomerName}</strong>,</p>" +
                    "<p>Cảm ơn bạn đã đặt hàng tại <strong>{SiteName}</strong>. Đơn hàng <strong>{OrderCode}</strong> đã được tiếp nhận.</p>" +
                    "<table style='width:100%;border-collapse:collapse;margin:16px 0;'>" +
                    "<tr><td style='padding:8px;border:1px solid #eee;'>Mã đơn hàng:</td><td style='padding:8px;border:1px solid #eee;font-weight:bold;'>{OrderCode}</td></tr>" +
                    "<tr><td style='padding:8px;border:1px solid #eee;'>Ngày đặt:</td><td style='padding:8px;border:1px solid #eee;'>{OrderDate}</td></tr>" +
                    "<tr><td style='padding:8px;border:1px solid #eee;'>Tổng tiền:</td><td style='padding:8px;border:1px solid #eee;color:#f06548;font-weight:bold;'>{TotalAmount}</td></tr>" +
                    "</table>" +
                    "<p>Chúng tôi sẽ xử lý đơn hàng trong thời gian sớm nhất.</p>" +
                    "<p style='color:#999;font-size:12px;'>Email tự động, vui lòng không trả lời.</p></div>",

                "OrderStatus" => "<div style='font-family:Arial,sans-serif;max-width:600px;margin:0 auto;'>" +
                    "<h2 style='color:#405189;'>📦 Cập nhật đơn hàng</h2>" +
                    "<p>Xin chào <strong>{CustomerName}</strong>,</p>" +
                    "<p>Đơn hàng <strong>{OrderCode}</strong> đã được cập nhật: <strong style='color:#0ab39c;'>{StatusName}</strong></p>" +
                    "<p>Cảm ơn bạn đã mua hàng tại <strong>{SiteName}</strong>!</p>" +
                    "<p style='color:#999;font-size:12px;'>Email tự động, vui lòng không trả lời.</p></div>",

                "PasswordReset" => "<div style='font-family:Arial,sans-serif;max-width:600px;margin:0 auto;'>" +
                    "<h2 style='color:#405189;'>🔐 Khôi phục mật khẩu</h2>" +
                    "<p>Xin chào <strong>{CustomerName}</strong>,</p>" +
                    "<p>Mật khẩu mới của bạn tại <strong>{SiteName}</strong> là:</p>" +
                    "<div style='background:#f3f3f9;padding:16px;border-radius:8px;text-align:center;margin:16px 0;'>" +
                    "<span style='font-size:24px;font-weight:bold;letter-spacing:2px;color:#405189;'>{NewPassword}</span></div>" +
                    "<p>Vui lòng đăng nhập và đổi mật khẩu ngay sau đó.</p>" +
                    "<p style='color:#999;font-size:12px;'>Email tự động, vui lòng không trả lời.</p></div>",

                _ => "<p>{Content}</p>"
            };
        }
    }
}
