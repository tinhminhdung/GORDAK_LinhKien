using Microsoft.AspNetCore.Mvc;
using System.IO;
using System;
using Microsoft.Data.SqlClient;
using Antigravity.ECommerce.Services;
using Microsoft.AspNetCore.Hosting;
using System.Text.RegularExpressions;

namespace Antigravity.ECommerce.Controllers
{
    // Controller này không có [Authorize] để ai cũng có thể gọi (phục vụ cài đặt Database ban đầu)
    public class InstallController : Controller
    {
        private readonly IWebHostEnvironment _env;

        public InstallController(IWebHostEnvironment env)
        {
            _env = env;
        }

        private bool IsDatabaseInstalled()
        {
            try
            {
                using (var conn = new SqlConnection(BaseConnectionSql.ConnectionString))
                {
                    conn.Open();
                    using (var cmd = new SqlCommand("SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'AdminUsers'", conn))
                    {
                        int count = (int)cmd.ExecuteScalar();
                        return count > 0;
                    }
                }
            }
            catch
            {
                return false; // Nếu lỗi kết nối, cứ coi như chưa cài đặt
            }
        }

        [HttpGet]
        public IActionResult Index()
        {
            if (IsDatabaseInstalled())
            {
                TempData["IsError"] = false;
                TempData["Message"] = "🔒 HỆ THỐNG ĐÃ ĐƯỢC CÀI ĐẶT TRƯỚC ĐÓ.\nĐể đảm bảo an toàn dữ liệu, tính năng cài đặt đã bị khóa.\nNếu muốn cài đặt lại, bạn phải xóa toàn bộ bảng trong Database thủ công.";
                ViewBag.IsLocked = true;
            }
            return View();
        }

        [HttpPost]
        public IActionResult SetupDatabase()
        {
            if (IsDatabaseInstalled())
            {
                TempData["IsError"] = true;
                TempData["Message"] = "🔒 TỪ CHỐI CÀI ĐẶT: Cơ sở dữ liệu đã tồn tại các bảng của hệ thống.";
                return RedirectToAction("Index");
            }

            try
            {
                // Tìm file SQLQuery.sql ở thư mục gốc của project (cùng cấp với thư mục wwwroot, Controllers...)
                string sqlFilePath = Path.Combine(_env.ContentRootPath, "SQLQuery.sql");

                if (!System.IO.File.Exists(sqlFilePath))
                {
                    TempData["IsError"] = true;
                    TempData["Message"] = $"❌ Không tìm thấy file SQLQuery.sql tại đường dẫn: {sqlFilePath}";
                    return RedirectToAction("Index");
                }

                string script = System.IO.File.ReadAllText(sqlFilePath);

                // SQL Server Management Studio dùng từ khóa GO để chia các batch lệnh. 
                // SqlCommand không hiểu GO nên phải cắt chuỗi ra thành nhiều lệnh nhỏ.
                var commandStrings = Regex.Split(script, @"^\s*GO\s*$", RegexOptions.Multiline | RegexOptions.IgnoreCase);

                int successCount = 0;
                int errorCount = 0;
                string lastError = "";

                using (var conn = new SqlConnection(BaseConnectionSql.ConnectionString))
                {
                    conn.Open();
                    foreach (var commandString in commandStrings)
                    {
                        if (!string.IsNullOrWhiteSpace(commandString))
                        {
                            try
                            {
                                using (var cmd = new SqlCommand(commandString, conn))
                                {
                                    cmd.ExecuteNonQuery();
                                    successCount++;
                                }
                            }
                            catch (SqlException ex)
                            {
                                errorCount++;
                                lastError = ex.Message;
                                // Bỏ qua lỗi từng lệnh nhỏ (như trùng lặp dữ liệu) để tiếp tục chạy đến cuối script (Tạo Stored Procedure)
                            }
                        }
                    }
                }

                TempData["IsError"] = false;
                if (errorCount > 0)
                {
                    TempData["Message"] = $"⚠️ Cài đặt Database hoàn tất! (Chạy thành công {successCount} lệnh, Bỏ qua {errorCount} lệnh lỗi).\nLỗi cuối cùng: {lastError}\nBây giờ bạn có thể truy cập trang chủ hoặc trang Admin.";
                }
                else
                {
                    TempData["Message"] = "✅ Cài đặt Database thành công! Đã chạy toàn bộ script trong SQLQuery.sql.\nBây giờ bạn có thể truy cập trang chủ hoặc trang Admin.";
                }
            }
            catch (Exception ex)
            {
                TempData["IsError"] = true;
                TempData["Message"] = $"❌ Lỗi trong quá trình chạy script: {ex.Message}\n\n{ex.StackTrace}";
            }

            return RedirectToAction("Index");
        }
    }
}
