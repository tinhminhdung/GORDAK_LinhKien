using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Antigravity.ECommerce.Models;
using Antigravity.ECommerce.Services;
using Antigravity.ECommerce.Framework;
using System;

namespace Antigravity.ECommerce.Controllers
{
    [Authorize]
    [Permission("Customers", ActionType.View)]
    public class AdminCustomerController : Controller
    {
        public IActionResult Index(string kw = "", int? rank = null, int? status = null,
            string sort = "CreatedAt", string order = "DESC", int page = 1, int size = 20)
        {
            var data = SCustomer.Search(kw, rank, status, sort, order, page, size);
            
            ViewBag.Keyword = kw;
            ViewBag.Rank = rank;
            ViewBag.Status = status;
            ViewBag.SortColumn = sort;
            ViewBag.SortOrder = order;
            ViewBag.PageIndex = page;
            ViewBag.PageSize = size;
            ViewBag.TotalCount = data.Count > 0 ? data[0].TotalCount : 0;
            ViewBag.TotalPages = (int)Math.Ceiling((double)ViewBag.TotalCount / size);

            return View(data);
        }


        [Permission("Customers", ActionType.View)]
        public IActionResult Detail(int id, string kw = "", int? status = null, int page = 1, int size = 10)
        {
            var customer = SCustomer.GetById(id);
            if (customer == null) return NotFound();

            var orders = SCustomer.GetOrderHistory(id, kw, status, page, size);
            ViewBag.Orders = orders;
            
            ViewBag.Keyword = kw;
            ViewBag.Status = status;
            ViewBag.PageIndex = page;
            ViewBag.PageSize = size;
            ViewBag.TotalCount = orders.Count > 0 ? orders[0].TotalCount : 0;
            ViewBag.TotalPages = (int)Math.Ceiling((double)ViewBag.TotalCount / size);

            return View(customer);
        }


        [Permission("Customers", ActionType.Create)]
        public IActionResult Create()
        {
            ViewBag.Provinces = BaseConnectionSql.Query<Province>("SELECT ProvinceId, Name FROM Provinces ORDER BY Name", null);
            return View(new Customer());
        }

        [HttpPost]
        [Permission("Customers", ActionType.Create)]
        public IActionResult Create(Customer model)
        {
            try
            {
                if (string.IsNullOrEmpty(model.FullName) || string.IsNullOrEmpty(model.Phone))
                {
                    ModelState.AddModelError("", "Vui lòng nhập Họ tên và Số điện thoại.");
                    return View(model);
                }

                // BUG #11: Validate unique phone
                var existing = SCustomer.GetByPhone(model.Phone);
                if (existing != null)
                {
                    ModelState.AddModelError("", "Số điện thoại này đã được sử dụng bởi khách hàng khác.");
                    return View(model);
                }

                SCustomer.Insert(model);
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View(model);
            }
        }


        [Permission("Customers", ActionType.Edit)]
        public IActionResult Edit(int id)
        {
            var customer = SCustomer.GetById(id);
            if (customer == null) return NotFound();
            
            ViewBag.Provinces = BaseConnectionSql.Query<Province>("SELECT ProvinceId, Name FROM Provinces ORDER BY Name", null);
            if (customer.ProvinceId.HasValue)
            {
                ViewBag.Wards = BaseConnectionSql.Query<Ward>("SELECT WardId, Name FROM Wards WHERE ProvinceId = @Id ORDER BY Name",
                    new Microsoft.Data.SqlClient.SqlParameter("@Id", customer.ProvinceId.Value));
            }
            
            return View(customer);
        }

        [HttpPost]
        [Permission("Customers", ActionType.Edit)]
        public IActionResult Edit(Customer model)
        {
            try
            {
                if (string.IsNullOrEmpty(model.FullName) || string.IsNullOrEmpty(model.Phone))
                {
                    ModelState.AddModelError("", "Vui lòng nhập Họ tên và Số điện thoại.");
                    return View(model);
                }

                // BUG #11: Validate unique phone (excluding current user)
                var existing = SCustomer.GetByPhone(model.Phone);
                if (existing != null && existing.CustomerId != model.CustomerId)
                {
                    ModelState.AddModelError("", "Số điện thoại này đã được sử dụng bởi khách hàng khác.");
                    return View(model);
                }

                SCustomer.Update(model);
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View(model);
            }
        }

        [HttpPost]
        [Permission("Customers", ActionType.Delete)]
        public IActionResult Delete(int id)
        {
            try
            {
                SCustomer.Delete(id);
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        [Permission("Customers", ActionType.Delete)]
        public IActionResult BulkDelete([FromBody] System.Collections.Generic.List<int> ids)
        {
            try
            {
                if (ids == null || ids.Count == 0) return Json(new { success = false });
                foreach (var id in ids)
                {
                    SCustomer.Delete(id);
                }
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        [Permission("Customers", ActionType.Edit)]
        public IActionResult RefreshRank(int id)
        {
            try
            {
                SCustomer.RefreshRank(id);
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [Permission("Customers", ActionType.Export)]
        public IActionResult ExportCSV(string kw = "", int? rank = null, int? status = null)
        {
            var data = SCustomer.Search(kw, rank, status, "CreatedAt", "DESC", 1, 1000000);
            var builder = new System.Text.StringBuilder();
            builder.AppendLine("CustomerId,FullName,Phone,Email,MemberRank,TotalSpent,TotalOrders,Status,CreatedAt");
            foreach (var item in data)
            {
                builder.AppendLine($"{item.CustomerId},{item.FullName.Replace(",", " ")},{item.Phone},{item.Email},{item.MemberRankName},{item.TotalSpent},{item.TotalOrders},{(item.Status == 1 ? "Active" : "Locked")},{item.CreatedAt:yyyy-MM-dd}");
            }
            return File(System.Text.Encoding.UTF8.GetPreamble().Concat(System.Text.Encoding.UTF8.GetBytes(builder.ToString())).ToArray(), "text/csv", $"Customers_{DateTime.Now:yyyyMMddHHmm}.csv");
        }
    }
}
