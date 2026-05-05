using System;
using Microsoft.AspNetCore.Mvc;
using Antigravity.ECommerce.Models;
using Antigravity.ECommerce.Framework;
using Antigravity.ECommerce.Services;

namespace Antigravity.ECommerce.Controllers
{
    public class ContactController : Controller
    {
        [Route("lien-he.html")]
        [Route("Contact")]
        public IActionResult Index()
        {
            var settings = SSetting.GetViewModel();

            var faqs = Antigravity.ECommerce.Services.SCache.GetOrSet("Contact_FAQs", () => 
                Antigravity.ECommerce.Services.SFAQ.GetAll().Where(x => x.Status == 1).Take(6).ToList(), 60);

            var contactConnect = Antigravity.ECommerce.Services.SCache.GetOrSet("Contact_Connect", () =>
                Antigravity.ECommerce.Services.SAdvertising.GetByPosition("Contact_Connect"), 60);

            ViewBag.FAQs = faqs;
            ViewBag.ContactConnect = contactConnect;

            return View(settings);
        }

        [HttpPost]
        public IActionResult SendMessage(string fullName, string email, string phone, string subject, string message)
        {
            if (string.IsNullOrEmpty(fullName) || string.IsNullOrEmpty(email) || string.IsNullOrEmpty(message))
            {
                return Json(new { success = false, message = "Vui lòng điền đầy đủ các thông tin bắt buộc (*)." });
            }

            // Simple Email Regex
            if (!System.Text.RegularExpressions.Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
            {
                return Json(new { success = false, message = "Email không hợp lệ." });
            }

            var msg = new ContactMessage
            {
                FullName = fullName,
                Email = email,
                Phone = phone,
                Subject = subject,
                Message = message,
                Status = 0,
                CreatedAt = DateTime.Now
            };

            int result = FContact.Insert(msg);
            
            if (result > 0)
            {
                return Json(new { success = true, message = "Cảm ơn bạn! Tin nhắn của bạn đã được gửi thành công. Chúng tôi sẽ phản hồi sớm nhất." });
            }

            return Json(new { success = false, message = "Có lỗi xảy ra khi gửi tin nhắn. Vui lòng thử lại sau." });
        }
    }
}
