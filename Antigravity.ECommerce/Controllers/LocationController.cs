using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Antigravity.ECommerce.Models;
using Antigravity.ECommerce.Services;
using System.Collections.Generic;

namespace Antigravity.ECommerce.Controllers
{
    public class LocationController : Controller
    {
        [HttpGet]
        public IActionResult GetTinhThanh()
        {
            var sql = "SELECT ProvinceId AS Ma_Tinh, Name AS Ten_Tinh, Type AS Loai FROM Provinces ORDER BY Name";
            var list = BaseConnectionSql.Query<TinhThanh>(sql, null);
            return Json(list);
        }

        [HttpGet]
        public IActionResult GetPhuongXa(int maTinh)
        {
            var sql = "SELECT WardId AS Ma_Xa, Name AS Ten_Xa, Type AS Loai, ProvinceId AS Ma_Tinh FROM Wards WHERE ProvinceId = @MaTinh ORDER BY Name";
            var prm = new SqlParameter[] { new SqlParameter("@MaTinh", maTinh) };
            var list = BaseConnectionSql.Query<PhuongXa>(sql, prm);
            return Json(list);
        }
    }
}
