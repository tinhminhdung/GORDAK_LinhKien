using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Antigravity.ECommerce.Models;
using Antigravity.ECommerce.Services;
using Antigravity.ECommerce.Framework;
using System;
using System.Collections.Generic;

namespace Antigravity.ECommerce.Controllers
{
    [Authorize]
    [Permission("Orders", ActionType.View)]
    public class AdminOrderController : Controller
    {
        public IActionResult Index(string kw = "", int? status = null, int? paymentStatus = null, 
            int? provinceId = null, int? wardId = null, bool? requiresVat = null,
            DateTime? dateMin = null, DateTime? dateMax = null,
            string sort = "CreatedAt", string order = "DESC", int page = 1, int size = 20)
        {
            var data = SOrder.Search(kw, status, paymentStatus, provinceId, wardId, requiresVat, dateMin, dateMax, sort, order, page, size);
            
            ViewBag.Keyword = kw;
            ViewBag.Status = status;
            ViewBag.PaymentStatus = paymentStatus;
            ViewBag.ProvinceId = provinceId;
            ViewBag.WardId = wardId;
            ViewBag.RequiresVat = requiresVat;
            ViewBag.DateMin = dateMin?.ToString("yyyy-MM-dd");
            ViewBag.DateMax = dateMax?.ToString("yyyy-MM-dd");
            ViewBag.SortColumn = sort;
            ViewBag.SortOrder = order;
            ViewBag.PageIndex = page;
            ViewBag.PageSize = size;
            ViewBag.TotalCount = data.Count > 0 ? data[0].TotalCount : 0;
            ViewBag.TotalPages = (int)Math.Ceiling((double)ViewBag.TotalCount / size);
            
            // Get count for each tab in one call
            var counts = SOrder.GetStatusCounts();
            ViewBag.CountAll = counts.All;
            ViewBag.CountNew = counts.New;
            ViewBag.CountConfirmed = counts.Confirmed;
            ViewBag.CountShipping = counts.Shipping;
            ViewBag.CountCompleted = counts.Completed;
            ViewBag.CountCancelled = counts.Cancelled;
            ViewBag.CountReturned = counts.Returned;
            
            ViewBag.Provinces = BaseConnectionSql.Query<Province>("SELECT ProvinceId, Name FROM Provinces ORDER BY Name", null);
            if (provinceId.HasValue)
            {
                ViewBag.Wards = BaseConnectionSql.Query<Ward>("SELECT WardId, Name FROM Wards WHERE ProvinceId = @Id ORDER BY Name",
                    new Microsoft.Data.SqlClient.SqlParameter("@Id", provinceId.Value));
            }

            return View(data);
        }


        [Permission("Orders", ActionType.View)]
        public IActionResult Detail(int id)
        {
            var order = SOrder.GetById(id);
            if (order == null) return NotFound();

            ViewBag.Items = SOrder.GetItemsByOrderId(id);
            ViewBag.History = SOrder.GetHistoryByOrderId(id);
            
            ViewBag.Provinces = BaseConnectionSql.Query<Province>("SELECT ProvinceId, Name FROM Provinces ORDER BY Name", null);
            if (order.ProvinceId.HasValue)
            {
                var provinces = ViewBag.Provinces as List<Province>;
                if (provinces != null && string.IsNullOrEmpty(order.ProvinceName))
                {
                    order.ProvinceName = provinces.FirstOrDefault(p => p.ProvinceId == order.ProvinceId.Value)?.Name;
                }

                ViewBag.Wards = BaseConnectionSql.Query<Ward>("SELECT WardId, Name FROM Wards WHERE ProvinceId = @Id ORDER BY Name",
                    new Microsoft.Data.SqlClient.SqlParameter("@Id", order.ProvinceId.Value));
                    
                var wards = ViewBag.Wards as List<Ward>;
                if (order.WardId.HasValue && wards != null && string.IsNullOrEmpty(order.WardName))
                {
                    order.WardName = wards.FirstOrDefault(w => w.WardId == order.WardId.Value)?.Name;
                }
            }

            return View(order);
        }


        [Permission("Orders", ActionType.Create)]
        public IActionResult Create()
        {
            ViewBag.Products = SProduct.Search(null, null, 1, null, null, null, "Name", "ASC", 1, 1000);
            ViewBag.Provinces = BaseConnectionSql.Query<Province>("SELECT ProvinceId, Name FROM Provinces ORDER BY Name", null);
            return View();
        }

        [HttpPost]
        [Permission("Orders", ActionType.Create)]
        public IActionResult Create([FromBody] OrderCreateModel model)
        {
            if (model == null || model.Order == null || model.Items == null || model.Items.Count == 0)
            {
                return Json(new { success = false, message = "Dữ liệu không hợp lệ" });
            }

            try
            {
                model.Order.UpdatedBy = "Admin";
                int newId = SOrder.Insert(model.Order, model.Items);
                return Json(new { success = true, orderId = newId });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        [Permission("Orders", ActionType.Edit)]
        public IActionResult UpdateStatus(int id, int newStatus, string? trackingCode, string? note)
        {
            try
            {
                SOrder.UpdateStatus(id, newStatus, trackingCode, note, "Admin");
                
                string emailWarning = "";
                if (newStatus == 1) // 1 = Xác nhận
                {
                    try
                    {
                        var order = SOrder.GetById(id);
                        if (order != null && !string.IsNullOrEmpty(order.CustomerEmail))
                        {
                            var replacements = new Dictionary<string, string>
                            {
                                { "CustomerName", order.CustomerName },
                                { "OrderCode", order.OrderCode },
                                { "TotalAmount", order.TotalAmount.ToString("N0") + "đ" },
                                { "OrderDate", order.CreatedAt.ToString("dd/MM/yyyy HH:mm") },
                                { "StatusName", "Đã xác nhận" }
                            };
                            SEmailSender.SendFromTemplateAsync("OrderConfirm", order.CustomerEmail, $"Xác nhận đơn hàng #{order.OrderCode}", replacements);
                        }
                    }
                    catch (Exception ex)
                    {
                        emailWarning = "Lỗi khi thiết lập gửi email: " + ex.Message;
                    }
                }

                return Json(new { success = true, warning = emailWarning });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        [Permission("Orders", ActionType.Edit)]
        public IActionResult UpdateTracking(int id, string trackingCode)
        {
            try
            {
                var order = SOrder.GetById(id);
                if (order == null) return Json(new { success = false, message = "Không tìm thấy đơn hàng" });
                
                order.TrackingCode = trackingCode;
                SOrder.Update(order);
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        [Permission("Orders", ActionType.Edit)]
        public IActionResult UpdateAdminNote(int id, string adminNote)
        {
            try
            {
                var order = SOrder.GetById(id);
                if (order == null) return Json(new { success = false, message = "Không tìm thấy đơn hàng" });
                
                order.AdminNote = adminNote;
                SOrder.Update(order);
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        [Permission("Orders", ActionType.Edit)]
        public IActionResult Update(Order model)
        {
            try
            {
                var order = SOrder.GetById(model.OrderId);
                if (order == null) return Json(new { success = false, message = "Không tìm thấy đơn hàng" });

                // Update allowed fields
                order.CustomerName = model.CustomerName;
                order.CustomerPhone = model.CustomerPhone;
                order.ShippingAddress = model.ShippingAddress;
                order.WardId = model.WardId;
                order.ProvinceId = model.ProvinceId;
                order.PaymentStatus = model.PaymentStatus;
                order.ShippingMethod = model.ShippingMethod;
                order.TrackingCode = model.TrackingCode;
                order.AdminNote = model.AdminNote;
                order.UpdatedBy = "Admin";

                SOrder.Update(order);
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        [Permission("Orders", ActionType.Delete)]
        public IActionResult Delete(int id)
        {
            try
            {
                var order = SOrder.GetById(id);
                if (order == null) return Json(new { success = false, message = "Không tìm thấy đơn hàng" });

                SOrder.Delete(id);
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        [Permission("Orders", ActionType.Delete)]
        public IActionResult BulkDelete([FromBody] List<int> ids)
        {
            try
            {
                if (ids == null || !ids.Any()) return Json(new { success = false, message = "Chưa chọn đơn hàng" });
                
                int successCount = 0;
                int failCount = 0;

                foreach (var id in ids)
                {
                    var order = SOrder.GetById(id);
                    if (order != null)
                    {
                        SOrder.Delete(id);
                        successCount++;
                    }
                    else
                    {
                        failCount++;
                    }
                }

                if (failCount > 0)
                {
                    return Json(new { success = true, message = $"Đã xóa {successCount} đơn hàng. {failCount} đơn không tìm thấy hoặc đã bị xóa trước đó." });
                }

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }


        [Permission("Orders", ActionType.View)]
        public IActionResult Print(int id)
        {
            var order = SOrder.GetById(id);
            if (order == null) return NotFound();

            ViewBag.Items = SOrder.GetItemsByOrderId(id);
            return View(order);
        }

        [Permission("Orders", ActionType.Export)]
        public IActionResult ExportCSV(string kw = "", int? status = null, int? paymentStatus = null, 
            int? provinceId = null, int? wardId = null, bool? requiresVat = null,
            DateTime? dateMin = null, DateTime? dateMax = null)
        {
            var orders = SOrder.Search(kw, status, paymentStatus, provinceId, wardId, requiresVat, dateMin, dateMax, "CreatedAt", "DESC", 1, 100000);

            var builder = new System.Text.StringBuilder();
            builder.AppendLine("OrderId,OrderCode,CustomerName,CustomerPhone,TotalAmount,OrderStatus,PaymentMethod,CreatedAt");

            foreach (var item in orders)
            {
                string statusName = item.OrderStatus switch {
                    0 => "Mới", 1 => "Xác nhận", 2 => "Đang giao", 3 => "Hoàn tất", 4 => "Đã hủy", 5 => "Trả hàng", _ => ""
                };
                builder.AppendLine($"{item.OrderId},{item.OrderCode},{item.CustomerName.Replace(",", " ")},{item.CustomerPhone},{item.TotalAmount},{statusName},{item.PaymentMethod},{item.CreatedAt:yyyy-MM-dd HH:mm}");
            }

            return File(System.Text.Encoding.UTF8.GetPreamble().Concat(System.Text.Encoding.UTF8.GetBytes(builder.ToString())).ToArray(), "text/csv", $"Orders_{DateTime.Now:yyyyMMddHHmm}.csv");
        }

        public class OrderCreateModel
        {
            public Order Order { get; set; } = new Order();
            public List<OrderItem> Items { get; set; } = new List<OrderItem>();
        }
    }
}
