using EmerceWebsite_Shop_master.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace EmerceWebsite_Shop_master.Controllers
{
    public class ThongKeController : Controller
    {
        // === CÁC ACTION TRẢ VỀ VIEW (TRANG) ===

        // GET: /ThongKe/DoanhThu
        // Trả về trang Thống kê doanh thu chi tiết dạng bảng
        public ActionResult ThongKe()
        {
            return View();
        }

        // GET: /ThongKe/BaoCao
        // Trả về trang Báo cáo tổng quan với các chỉ số và biểu đồ
        public ActionResult BaoCao()
        {
            return View();
        }

        // === ACTION API ĐỂ LẤY DỮ LIỆU ===

        [HttpPost]
        public JsonResult GetData(string filterType, DateTime? startDate, DateTime? endDate)
        {
            int currentShopId = 1; // Giả định ShopID = 1

            DateTime? start = null;
            DateTime? end = null;
            var today = DateTime.Today;

            // Xác định khoảng thời gian dựa vào bộ lọc
            switch (filterType)
            {
                case "today":
                    start = today;
                    end = today.AddDays(1);
                    break;
                case "this_month":
                    start = new DateTime(today.Year, today.Month, 1);
                    end = start.Value.AddMonths(1);
                    break;
                case "this_quarter":
                    int quarterNumber = (today.Month - 1) / 3 + 1;
                    start = new DateTime(today.Year, (quarterNumber - 1) * 3 + 1, 1);
                    end = start.Value.AddMonths(3);
                    break;
                case "custom":
                    if (!startDate.HasValue || !endDate.HasValue)
                        return Json(new { error = "Vui lòng chọn ngày." });
                    start = startDate.Value;
                    end = endDate.Value.AddDays(1);
                    break;
                case "all": // Xử lý tùy chọn "Tất cả"
                    // Không cần gán start, end
                    break;
            }

            try
            {
                using (DatabaseDataContext db = new DatabaseDataContext())
                {
                    var productIdsOfShop = db.ProductShops
                                             .Where(ps => ps.ShopID == currentShopId)
                                             .Select(ps => ps.ProductID)
                                             .ToList();

                    // Tạo một câu truy vấn IQueryable để có thể tái sử dụng
                    var query = (from order in db.Orders
                                 join item in db.OrderItems on order.OrderID equals item.OrderID
                                 where productIdsOfShop.Contains(item.ProductID)
                                       && order.OrderStatus == "Đã thanh toán"
                                 select order).Distinct();

                    // Áp dụng bộ lọc thời gian nếu có
                    if (start.HasValue && end.HasValue)
                    {
                        query = query.Where(o => o.OrderDate >= start && o.OrderDate < end);
                    }

                    // Lấy kết quả về bộ nhớ
                    var validOrdersInRange = query.ToList();

                    // --- Bắt đầu tính toán ---

                    var revenueByDate = validOrdersInRange
                        .Where(o => o.OrderDate.HasValue)
                        .GroupBy(o => o.OrderDate.Value.Date)
                        .Select(g => new
                        {
                            Date = g.Key,
                            OrderCount = g.Count(),
                            TotalRevenue = g.Sum(o => o.TotalAmount)
                        })
                        .OrderBy(r => r.Date)
                        .ToList();

                    var totalRevenue = validOrdersInRange.Sum(o => (decimal?)o.TotalAmount) ?? 0;
                    var totalOrders = validOrdersInRange.Count();

                    var validOrderIds = validOrdersInRange.Select(o => o.OrderID).ToList();

                    var totalProductsSold = db.OrderItems
                                              .Where(item => validOrderIds.Contains(item.OrderID))
                                              .Sum(item => (int?)item.Quantity) ?? 0;

                    var bestSellingProductQuery = (from item in db.OrderItems
                                                   where validOrderIds.Contains(item.OrderID)
                                                   group item by item.Product into g // Group by đối tượng Product
                                                   orderby g.Sum(i => i.Quantity) descending
                                                   select new
                                                   {
                                                       Product = g.Key,
                                                       TotalQuantity = g.Sum(i => i.Quantity)
                                                   })
                                                   .FirstOrDefault();

                    string bestSellingProduct = "N/A";
                    if (bestSellingProductQuery != null)
                    {
                        bestSellingProduct = $"{bestSellingProductQuery.Product.ProductName} ({bestSellingProductQuery.TotalQuantity})";
                    }

                    var report = new
                    {
                        RevenueByDate = revenueByDate,
                        TotalRevenue = totalRevenue,
                        TotalOrders = totalOrders,
                        TotalProductsSold = totalProductsSold,
                        BestSellingProduct = bestSellingProduct
                    };

                    return Json(report);
                }
            }
            catch (Exception ex)
            {
                return Json(new { error = "Đã có lỗi xảy ra ở phía máy chủ: " + ex.Message });
            }
        }
    }
}