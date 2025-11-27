using EmerceWebsite_Shop_master.Models;
using System;
using System.Web.Mvc;
using System.Linq;
using System.Collections.Generic;

namespace EmerceWebsite_Shop_master.Controllers
{
    public class QLĐHController : Controller
    {
        private int currentShopId = 1;

        public ActionResult DanhSach() { return View(); }

        public ActionResult ChiTiet(int id)
        {
            ViewBag.OrderID = id;
            return View();
        }

        [HttpPost]
        public JsonResult GetOrders()
        {
    
            using (DatabaseDataContext db = new DatabaseDataContext())
            {
                var orders = (from o in db.Orders
                              join oi in db.OrderItems on o.OrderID equals oi.OrderID
                              join ps in db.ProductShops on oi.ProductID equals ps.ProductID
                              join c in db.Customers on o.CustomerID equals c.CustomerID
                              where ps.ShopID == currentShopId
                              select new
                              {
                                  o.OrderID,
                                  o.OrderDate,
                                  o.TotalAmount,
                                  o.OrderStatus,
                                  CustomerName = c.FullName,
                                  c.CustomerAddress
                              }).Distinct().ToList();
                return Json(orders);
            }
        }

        [HttpPost]
        public JsonResult GetOrderDetails(int id)
        {
          
            using (DatabaseDataContext db = new DatabaseDataContext())
            {
                var orderInfo = (from o in db.Orders
                                 join c in db.Customers on o.CustomerID equals c.CustomerID
                                 join s in db.Shipments on o.OrderID equals s.OrderID into shipmentGroup
                                 from s in shipmentGroup.DefaultIfEmpty()
                                 join p in db.Payments on o.OrderID equals p.OrderID into paymentGroup
                                 from p in paymentGroup.DefaultIfEmpty()
                                 where o.OrderID == id
                                 select new { o.OrderID, o.OrderDate, o.TotalAmount, o.OrderStatus, CustomerName = c.FullName, c.CustomerAddress, PaymentMethod = p.PaymentMethod, ShipmentStatus = s.ShipmentStatus }).FirstOrDefault();

                var orderItems = (from oi in db.OrderItems
                                  join product in db.Products on oi.ProductID equals product.ProductID
                                  where oi.OrderID == id
                                  select new { product.ProductName, oi.Quantity, oi.Price }).ToList();

                return Json(new { order = orderInfo, items = orderItems });
            }
        }

        // --- PHẦN CẬP NHẬT TRẠNG THÁI ---
        [HttpPost]
        public string UpdateStatus(int orderId, string newStatus)
        {
            try
            {
                using (DatabaseDataContext db = new DatabaseDataContext())
                {
                    Order order = db.Orders.SingleOrDefault(o => o.OrderID == orderId);
                    if (order != null)
                    {
                        string oldStatus = order.OrderStatus;
                        order.OrderStatus = newStatus;

                        // Cập nhật Shipment nếu có
                        if (newStatus == "Đang giao")
                        {
                            Shipment shipment = db.Shipments.SingleOrDefault(s => s.OrderID == orderId);
                            if (shipment != null)
                            {
                                shipment.ShipmentStatus = "Đang vận chuyển";
                            }
                        }

                    
                        // --- TẠO THÔNG BÁO: TRẠNG THÁI ĐƠN HÀNG THAY ĐỔI ---
                        
                        ShopNotification noti = new ShopNotification();
                        noti.ShopID = currentShopId;
                        noti.Title = "📦 Cập nhật đơn hàng #" + orderId;
                        noti.Message = "Trạng thái thay đổi từ '" + oldStatus + "' sang '" + newStatus + "'";
                        noti.Type = "ORDER";
                        noti.CreatedDate = DateTime.Now;
                        noti.IsRead = false;
                        noti.LinkUrl = "/QLĐH/ChiTiet/" + orderId;

                        db.ShopNotifications.InsertOnSubmit(noti);
                        // ====================================================

                        db.SubmitChanges();
                        return "Cập nhật trạng thái thành công!";
                    }
                    return "Không tìm thấy Đơn hàng.";
                }
            }
            catch (Exception ex)
            {
                return "Lỗi cập nhật trạng thái: " + ex.Message;
            }
        }
    }
}