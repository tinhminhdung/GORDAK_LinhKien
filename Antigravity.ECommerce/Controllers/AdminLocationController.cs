using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Antigravity.ECommerce.Services;
using Antigravity.ECommerce.Framework;

using Microsoft.Data.SqlClient;
using System.Text.Json;
using System.Net.Http;

namespace Antigravity.ECommerce.Controllers
{
    /// <summary> Quản trị dữ liệu Tỉnh/Thành - Quận/Huyện - Phường/Xã </summary>
    [Authorize]
    [Permission("Location")]
    public class AdminLocationController : Controller
    {
        public IActionResult Index(string type = "province", int? provinceId = null, int page = 1)
        {
            // Đếm số lượng dữ liệu
            var provinces = BaseConnectionSql.Query<Antigravity.ECommerce.Models.Province>("SELECT ProvinceId, Name, Type FROM Provinces ORDER BY Name", null);
            ViewBag.ProvinceCount = provinces.Count;
            
            var wardCount = BaseConnectionSql.ExecuteScalar("SELECT COUNT(*) FROM Wards", null);
            ViewBag.WardCount = wardCount != null ? Convert.ToInt32(wardCount) : 0;
            
            int pageSize = 50;
            
            if (type == "ward")
            {
                string countSql = "SELECT COUNT(*) FROM Wards w";
                string dataSql = "SELECT w.WardId, w.Name, w.Type, w.ProvinceId, p.Name as ProvinceName FROM Wards w LEFT JOIN Provinces p ON w.ProvinceId = p.ProvinceId";
                
                List<SqlParameter> prms1 = new List<SqlParameter>();
                List<SqlParameter> prms2 = new List<SqlParameter>();
                if (provinceId.HasValue)
                {
                    countSql += " WHERE w.ProvinceId = @pId";
                    dataSql += " WHERE w.ProvinceId = @pId";
                    prms1.Add(new SqlParameter("@pId", provinceId.Value));
                    prms2.Add(new SqlParameter("@pId", provinceId.Value));
                }
                
                dataSql += $" ORDER BY w.Name OFFSET {(page - 1) * pageSize} ROWS FETCH NEXT {pageSize} ROWS ONLY";
                
                var totalRecords = Convert.ToInt32(BaseConnectionSql.ExecuteScalar(countSql, prms1.ToArray()) ?? 0);
                var wards = BaseConnectionSql.Query<Antigravity.ECommerce.Models.Ward>(dataSql, prms2.ToArray());
                
                ViewBag.Wards = wards;
                ViewBag.TotalPages = (int)Math.Ceiling((double)totalRecords / pageSize);
                ViewBag.CurrentPage = page;
                ViewBag.ProvinceId = provinceId;
            }
            else 
            {
                ViewBag.Provinces = provinces;
            }
            
            ViewBag.Type = type;
            ViewBag.AllProvinces = provinces; // Để hiện trong dropdown filter
            
            return View();
        }

        /// <summary> Lấy danh sách xã theo tỉnh (AJAX) </summary>
        [HttpGet]
        public IActionResult GetWards(int provinceId)
        {
            var list = BaseConnectionSql.Query<Antigravity.ECommerce.Models.Ward>("SELECT WardId, Name, Type, ProvinceId FROM Wards WHERE ProvinceId = @Id ORDER BY Name",
                new SqlParameter("@Id", provinceId));
            return Json(list);
        }
    }
}
