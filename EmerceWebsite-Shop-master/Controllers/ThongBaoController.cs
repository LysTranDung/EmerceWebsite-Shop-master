// File: Controllers/ThongBaoController.cs
using System;
using System.Linq;
using System.Web.Mvc;
using EmerceWebsite_Shop_master.Models;

namespace EmerceWebsite_Shop_master.Controllers
{
    public class ThongBaoController : Controller
    {
        DatabaseDataContext db = new DatabaseDataContext();

        // 1. TRANG QUẢN LÝ THÔNG BÁO (HIỂN THỊ DANH SÁCH)
        public ActionResult ThongbaoIndex()
        {
            int shopId = 1; // Giả định ID Shop
            var list = db.ShopNotifications
                         .Where(n => n.ShopID == shopId)
                         .OrderByDescending(n => n.CreatedDate)
                         .ToList();

            // Vào trang này coi như đã đọc hết
            foreach (var item in list.Where(x => x.IsRead == false))
            {
                item.IsRead = true;
            }
            db.SubmitChanges();

            return View(list);
        }

        // 2. API: Lấy dữ liệu cho Script chạy ngầm (để hiện Popup Alert)
        [HttpGet]
        public JsonResult GetNewNotifications()
        {
            int shopId = 1;
            // Logic đếm số tin chưa đọc để hiện số đỏ lên Menu
            int unreadCount = db.ShopNotifications.Count(n => n.ShopID == shopId && n.IsRead == false);

            // Lấy tin mới nhất để hiện Popup
            var latest = db.ShopNotifications
                           .Where(n => n.ShopID == shopId)
                           .OrderByDescending(n => n.CreatedDate)
                           .Select(n => new { n.Title, n.Message })
                           .FirstOrDefault();

            return Json(new { success = true, unread = unreadCount, latest = latest }, JsonRequestBehavior.AllowGet);
        }
    }
}