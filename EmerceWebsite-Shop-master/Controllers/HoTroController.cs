using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using EmerceWebsite_Shop_master.Models;

namespace EmerceWebsite_Shop_master.Controllers
{
    public class HoTroController : Controller
    {
        private DatabaseDataContext db = new DatabaseDataContext();

        [HttpGet]
        public ActionResult GuiYeuCau()
        {
            return View();
        }

        [HttpPost]
        public ActionResult GuiYeuCau(int? shopId, int? orderId, string noiDung)
        {
            try
            {
                // Lưu đơn khiếu nại
                SupportRequest req = new SupportRequest();
                req.ShopID = shopId ?? 1; // Mặc định shop 1 nếu null
                req.OrderID = orderId;
                req.Content = noiDung;
                req.CreatedDate = DateTime.Now;
                req.Status = "Đang xử lý";

                db.SupportRequests.InsertOnSubmit(req);
                db.SubmitChanges();
                
                // TẠO THÔNG BÁO TỰ ĐỘNG (ALERT)

                ShopNotification noti = new ShopNotification();
                noti.ShopID = req.ShopID;
                noti.Title = "📩 Khiếu nại mới #" + req.RequestID;
                noti.Message = "Nội dung: " + (noiDung.Length > 50 ? noiDung.Substring(0, 50) + "..." : noiDung);
                noti.Type = "SUPPORT"; // Loại thông báo Hỗ trợ
                noti.CreatedDate = DateTime.Now;
                noti.IsRead = false;
                noti.LinkUrl = "/HoTro/LichSuHoTro"; // Link bấm vào xem

                db.ShopNotifications.InsertOnSubmit(noti);
                db.SubmitChanges();
                TempData["ThongBaoSuccess"] = "Gửi khiếu nại thành công! Mã phiếu #" + req.RequestID;
                return RedirectToAction("LichSuHoTro");
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Lỗi: " + ex.Message;
                return View();
            }
        }

        public ActionResult LichSuHoTro()
        {
            List<SupportRequest> list = db.SupportRequests.OrderByDescending(x => x.CreatedDate).ToList();
            return View(list);
        }
    }
}