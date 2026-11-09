using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace Antigravity.ECommerce.Controllers
{
    [Authorize]
    public class AdminTerminalController : Controller
    {
        private readonly string _connectionString;
        private readonly IWebHostEnvironment _env;

        public AdminTerminalController(IConfiguration configuration, IWebHostEnvironment env)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection") ?? "";
            _env = env;
        }

        public IActionResult Index()
        {
            ViewBag.ScriptExists = System.IO.File.Exists(Path.Combine(_env.ContentRootPath, "scripts", "Hub_Integration_Migration.sql"));
            ViewBag.FixScriptExists = System.IO.File.Exists(Path.Combine(_env.ContentRootPath, "scripts", "Fix_Hub_VAT_Prices.sql"));
            return View();
        }

        private bool CheckAuth(string? secretKey)
        {
            string? authSession = HttpContext.Session.GetString("TerminalAuth");
            if (authSession == "true") return true;

            if (secretKey == "Antigravity@2026")
            {
                HttpContext.Session.SetString("TerminalAuth", "true");
                return true;
            }
            return false;
        }

        private (bool ok, int batchesExecuted, int totalRowsAffected, List<string> messages, DataTable? lastTable, string? error) ExecuteSqlScript(string sqlText, bool stripUseDatabase = false)
        {
            var messages = new List<string>();
            DataTable? lastTable = null;
            int batchesExecuted = 0;
            int totalRows = 0;

            if (stripUseDatabase)
            {
                // Bỏ lệnh USE [...] cứng để luôn chạy vào đúng Database của ConnectionString trên Live
                sqlText = Regex.Replace(sqlText, @"^\s*USE\s+\[?[^\];\r\n]+\]?\s*;?\s*$", "", RegexOptions.Multiline | RegexOptions.IgnoreCase);
            }

            // Tách các batch theo từ khóa GO đứng riêng trên 1 dòng
            var batches = Regex.Split(sqlText, @"^\s*GO\s*(?:--.*)?$", RegexOptions.Multiline | RegexOptions.IgnoreCase);

            try
            {
                using var conn = new SqlConnection(_connectionString);
                conn.InfoMessage += (s, e) =>
                {
                    if (!string.IsNullOrWhiteSpace(e.Message))
                        messages.Add(e.Message);
                };
                conn.Open();

                foreach (var rawBatch in batches)
                {
                    string batch = rawBatch.Trim();
                    if (string.IsNullOrWhiteSpace(batch)) continue;

                    using var cmd = new SqlCommand(batch, conn) { CommandTimeout = 120 };
                    if (Regex.IsMatch(batch, @"^\s*SELECT\b", RegexOptions.IgnoreCase) && !Regex.IsMatch(batch, @"\b(CREATE|ALTER|INSERT|UPDATE|DELETE)\b", RegexOptions.IgnoreCase))
                    {
                        using var adapter = new SqlDataAdapter(cmd);
                        var dt = new DataTable();
                        adapter.Fill(dt);
                        lastTable = dt;
                        batchesExecuted++;
                    }
                    else
                    {
                        int rows = cmd.ExecuteNonQuery();
                        if (rows > 0) totalRows += rows;
                        batchesExecuted++;
                    }
                }

                return (true, batchesExecuted, totalRows, messages, lastTable, null);
            }
            catch (Exception ex)
            {
                return (false, batchesExecuted, totalRows, messages, lastTable, $"Lỗi tại Batch #{batchesExecuted + 1}: {ex.Message}");
            }
        }

        [HttpPost]
        public IActionResult Execute(string query, string secretKey, IFormFile? sqlFile)
        {
            ViewBag.ScriptExists = System.IO.File.Exists(Path.Combine(_env.ContentRootPath, "scripts", "Hub_Integration_Migration.sql"));

            if (!CheckAuth(secretKey))
            {
                ViewBag.Error = "Sai mật khẩu cấp 2 (Mật khẩu mặc định: Antigravity@2026). Truy cập bị từ chối.";
                return View("Index");
            }

            if (sqlFile != null && sqlFile.Length > 0)
            {
                using var reader = new StreamReader(sqlFile.OpenReadStream(), Encoding.UTF8, true);
                query = reader.ReadToEnd();
            }

            if (string.IsNullOrWhiteSpace(query))
            {
                ViewBag.Error = "Vui lòng nhập câu lệnh SQL hoặc chọn file .sql cần chạy.";
                return View("Index");
            }

            ViewBag.Query = query;
            var res = ExecuteSqlScript(query, stripUseDatabase: true);

            if (res.ok)
            {
                ViewBag.Success = $"Thực thi thành công {res.batchesExecuted} cụm lệnh (GO batches). Số dòng tác động: {res.totalRowsAffected}.";
                ViewBag.Messages = res.messages;
                ViewBag.Data = res.lastTable;
            }
            else
            {
                ViewBag.Error = res.error;
                ViewBag.Messages = res.messages;
            }

            return View("Index");
        }

        [HttpPost]
        public IActionResult RunFixHubVatPrices(string secretKey)
        {
            string scriptPath = Path.Combine(_env.ContentRootPath, "scripts", "Fix_Hub_VAT_Prices.sql");
            ViewBag.ScriptExists = System.IO.File.Exists(Path.Combine(_env.ContentRootPath, "scripts", "Hub_Integration_Migration.sql"));
            ViewBag.FixScriptExists = System.IO.File.Exists(scriptPath);

            if (!CheckAuth(secretKey))
            {
                ViewBag.Error = "Sai mật khẩu cấp 2 (Mật khẩu mặc định: Antigravity@2026). Truy cập bị từ chối.";
                return View("Index");
            }

            if (!System.IO.File.Exists(scriptPath))
            {
                ViewBag.Error = $"Không tìm thấy file kịch bản tại: {scriptPath}";
                return View("Index");
            }

            string sqlText = System.IO.File.ReadAllText(scriptPath, Encoding.UTF8);
            ViewBag.Query = sqlText;

            var res = ExecuteSqlScript(sqlText, stripUseDatabase: true);
            if (res.ok)
            {
                ViewBag.Success = $"✅ Đã thực thi thành công file Fix_Hub_VAT_Prices.sql ({res.batchesExecuted} cụm lệnh GO)! Thủ tục SP_Hub_SyncProduct đã được cập nhật và toàn bộ sản phẩm đã được chuẩn hóa giá VAT 8%.";
                ViewBag.Messages = res.messages;
                ViewBag.Data = res.lastTable;
            }
            else
            {
                ViewBag.Error = res.error;
                ViewBag.Messages = res.messages;
                ViewBag.Data = res.lastTable;
            }

            return View("Index");
        }

        [HttpPost]
        public IActionResult RunHubMigration(string secretKey)
        {
            string scriptPath = Path.Combine(_env.ContentRootPath, "scripts", "Hub_Integration_Migration.sql");
            ViewBag.ScriptExists = System.IO.File.Exists(scriptPath);

            if (!CheckAuth(secretKey))
            {
                ViewBag.Error = "Sai mật khẩu cấp 2 (Mật khẩu mặc định: Antigravity@2026). Truy cập bị từ chối.";
                return View("Index");
            }

            if (!System.IO.File.Exists(scriptPath))
            {
                ViewBag.Error = $"Không tìm thấy file migration tại: {scriptPath}";
                return View("Index");
            }

            string sqlText = System.IO.File.ReadAllText(scriptPath, Encoding.UTF8);
            ViewBag.Query = sqlText;

            var res = ExecuteSqlScript(sqlText, stripUseDatabase: true);
            if (res.ok)
            {
                ViewBag.Success = $"✅ Đã cập nhật thành công toàn bộ Bảng & Thủ tục (Stored Procedures) từ file Hub_Integration_Migration.sql ({res.batchesExecuted} cụm lệnh GO)!";
                ViewBag.Messages = res.messages;
            }
            else
            {
                ViewBag.Error = res.error;
                ViewBag.Messages = res.messages;
            }

            return View("Index");
        }
    }
}
