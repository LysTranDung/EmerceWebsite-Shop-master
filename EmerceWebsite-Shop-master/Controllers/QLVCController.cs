using EmerceWebsite_Shop_master.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace EmerceWebsite_Shop_master.Controllers
{
    public class QLVCController : Controller
    {
        DatabaseDataContext db = new DatabaseDataContext();
        int currentShopId = 1;

        public ActionResult DanhSach() { return View(); }
        public ActionResult TaoMoi() { return View(); }

        // LẤY DANH SÁCH VOUCHER 
        [HttpPost]
        public JsonResult GetDiscounts()
        {
            // BƯỚC 1: Lấy dữ liệu thô từ Database (Chưa format ngày tháng)
            var rawList = (from d in db.Discounts
                           join ds in db.DiscountShops on d.DiscountID equals ds.DiscountID
                           where ds.ShopID == currentShopId && (d.IsDelete == false || d.IsDelete == null)
                           orderby d.EndDate descending
                           select new
                           {
                               d.DiscountID,
                               d.DiscountCode,
                               d.DiscountPercentage,
                               d.DiscountDescription,
                               d.Quantity,
                               d.DiscountStatus,
                               d.StartDate, 
                               d.EndDate   
                           }).ToList(); 

          
            var formattedList = rawList.Select(x => new
            {
                x.DiscountID,
                x.DiscountCode,
                x.DiscountPercentage,
                x.DiscountDescription,
                x.Quantity,
                x.DiscountStatus,
                StartDateStr = x.StartDate.HasValue ? x.StartDate.Value.ToString("dd/MM/yyyy") : "",
                EndDateStr = x.EndDate.HasValue ? x.EndDate.Value.ToString("dd/MM/yyyy") : "",
                IsExpired = x.EndDate.HasValue && x.EndDate.Value < DateTime.Now
            }).ToList();

            return Json(formattedList);
        }

        // TẠO VOUCHER (KÈM THÔNG BÁO)
        [HttpPost]
        public JsonResult Insert(Discount model)
        {
            try
            {
                if (string.IsNullOrEmpty(model.DiscountCode))
                    return Json(new { success = false, message = "Thiếu mã" });

                model.IsDelete = false;
                db.Discounts.InsertOnSubmit(model);
                db.SubmitChanges();

                DiscountShop ds = new DiscountShop { DiscountID = model.DiscountID, ShopID = currentShopId };
                db.DiscountShops.InsertOnSubmit(ds);
                db.SubmitChanges();

                // --- THÔNG BÁO: VOUCHER MỚI ---
                ShopNotification noti = new ShopNotification();
                noti.ShopID = currentShopId;
                noti.Title = "🎫 Voucher mới";
                noti.Message = "Mã " + model.DiscountCode + " (" + model.DiscountPercentage + "%) đã tạo.";
                noti.Type = "VOUCHER";
                noti.IsRead = false;
                noti.CreatedDate = DateTime.Now;
                noti.LinkUrl = "/QLVC/DanhSach";
                db.ShopNotifications.InsertOnSubmit(noti);
                db.SubmitChanges();

                return Json(new { success = true, message = "Thêm thành công!" });
            }
            catch (Exception ex) { return Json(new { success = false, message = ex.Message }); }
        }

        // API 3: XÓA
        [HttpPost]


        public JsonResult Delete(int id)
        {
            var d = db.Discounts.FirstOrDefault(x => x.DiscountID == id);
            if (d != null)
            {


                // KIỂM TRA  Ngày hết hạn
                if (d.EndDate.HasValue && d.EndDate.Value > DateTime.Now)
                {
                    return Json(new { success = false, message = "Không thể xóa. Voucher chưa hết hạn sử dụng." });
                }

          

                if (d.Quantity.HasValue && d.Quantity.Value > 0)
                {
                 
                    int timesUsed = 0;
                    if (d.Quantity.Value > timesUsed) {
                         return Json(new { success = false, message = "Không thể xóa vì Voucher vẫn còn lượt sử dụng." });
                    }
                  
                }


                // Nếu vượt qua tất cả các kiểm tra
                d.IsDelete = true;
                db.SubmitChanges();

                return Json(new { success = true, message = "Xóa Voucher thành công!" });
            }
            return Json(new { success = false, message = "Không tìm thấy Voucher cần xóa." });
        }
    }
}