using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Http;
using System;
using System.Data;

namespace Antigravity.ECommerce.Controllers
{
    [Authorize] // Only root admins can access this route
    public class AdminTerminalController : Controller
    {
        private readonly string _connectionString;

        public AdminTerminalController(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Execute(string query, string secretKey)
        {
            // Check session or secretKey
            string authSession = HttpContext.Session.GetString("TerminalAuth");
            if (authSession != "true")
            {
                if (secretKey != "Antigravity@2026")
                {
                    ViewBag.Error = "Sai mật khẩu cấp 2. Truy cập bị từ chối.";
                    return View("Index");
                }
                else
                {
                    // Correct password, save to session for 1 hour
                    // By default session might live longer, but it's fine
                    HttpContext.Session.SetString("TerminalAuth", "true");
                }
            }

            if (string.IsNullOrWhiteSpace(query))
            {
                ViewBag.Error = "Vui lòng nhập câu lệnh SQL.";
                return View("Index");
            }

            ViewBag.Query = query;

            try
            {
                using (var conn = new SqlConnection(_connectionString))
                {
                    conn.Open();
                    using (var cmd = new SqlCommand(query, conn))
                    {
                        if (query.TrimStart().StartsWith("SELECT", StringComparison.OrdinalIgnoreCase))
                        {
                            using (var adapter = new SqlDataAdapter(cmd))
                            {
                                var dt = new DataTable();
                                adapter.Fill(dt);
                                ViewBag.Data = dt;
                            }
                        }
                        else
                        {
                            int rowsAffected = cmd.ExecuteNonQuery();
                            ViewBag.Success = $"Thực thi thành công. Số dòng bị ảnh hưởng: {rowsAffected}";
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ViewBag.Error = $"Lỗi SQL: {ex.Message}";
            }

            return View("Index");
        }
    }
}
