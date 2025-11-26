using EmerceWebsite_Shop_master.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace EmerceWebsite_Shop_master.Controllers
{
    public class QLSPController : Controller
    {
        DatabaseDataContext db = new DatabaseDataContext();
        int currentShopId = 1;

        // 1. TRANG DANH SÁCH (Chỉ trả về khung HTML, không truyền Model)
        public ActionResult DSSP()
        {
            return View();
        }

        public ActionResult ThemSP() { return View(); }

        public ActionResult ChinhSua(int? id)
        {
            ViewBag.ProductID = id; // Truyền ID để lát JS gọi API lấy chi tiết
            return View();
        }

        // ================= API DATA ================= //

        // API 1: LẤY DANH SÁCH SẢN PHẨM
        [HttpPost]
        public JsonResult GetProducts()
        {
            try
            {
                var list = (from p in db.Products
                            join ps in db.ProductShops on p.ProductID equals ps.ProductID
                            where ps.ShopID == currentShopId && (p.IsDelete == false || p.IsDelete == null)
                            orderby p.ProductID descending
                            select new
                            {
                                p.ProductID,
                                p.ProductName,
                                p.Price,
                                p.Stock,
                                p.Brand
                            }).ToList();
                return Json(list);
            }
            catch { return Json(new List<object>()); }
        }

        // API 2: LẤY CHI TIẾT 1 SẢN PHẨM
        [HttpPost]
        public JsonResult GetProductDetails(int id)
        {
            var p = db.Products.Select(x => new {
                x.ProductID,
                x.ProductName,
                x.Price,
                x.Stock,
                x.Brand,
                x.ProductDescription
            }).FirstOrDefault(x => x.ProductID == id);
            return Json(p);
        }

        // API 3: THÊM SẢN PHẨM
        [HttpPost]
        public JsonResult Insert(Product product)
        {
            try
            {
                product.IsDelete = false;
                db.Products.InsertOnSubmit(product);
                db.SubmitChanges();

                ProductShop ps = new ProductShop { ProductID = product.ProductID, ShopID = currentShopId };
                db.ProductShops.InsertOnSubmit(ps);
                db.SubmitChanges();

                return Json(new { success = true, message = "Thêm thành công!" });
            }
            catch (Exception ex) { return Json(new { success = false, message = ex.Message }); }
        }

        // API 4: CẬP NHẬT (KÈM THÔNG BÁO TỰ ĐỘNG)
        [HttpPost]
        public JsonResult Update(int txt_ProductID_hide, Product product)
        {
            try
            {
                var existing = db.Products.FirstOrDefault(p => p.ProductID == txt_ProductID_hide);
                if (existing != null)
                {
                    existing.ProductName = product.ProductName;
                    existing.Price = product.Price;
                    existing.Stock = product.Stock;
                    existing.Brand = product.Brand;
                    existing.ProductDescription = product.ProductDescription;

                    // --- LOGIC THÔNG BÁO: CẢNH BÁO TỒN KHO ---
                    if (existing.Stock <= 5)
                    {
                        ShopNotification noti = new ShopNotification();
                        noti.ShopID = currentShopId;
                        noti.Title = "⚠️ Cảnh báo tồn kho";
                        noti.Message = "Sản phẩm '" + existing.ProductName + "' sắp hết hàng (" + existing.Stock + ").";
                        noti.Type = "VIOLATION";
                        noti.IsRead = false;
                        noti.CreatedDate = DateTime.Now;
                        noti.LinkUrl = "/QLSP/ChinhSua?id=" + existing.ProductID;
                        db.ShopNotifications.InsertOnSubmit(noti);
                    }
                    // -----------------------------------------

                    db.SubmitChanges();
                    return Json(new { success = true, message = "Cập nhật thành công!" });
                }
                return Json(new { success = false, message = "Không tìm thấy SP" });
            }
            catch (Exception ex) { return Json(new { success = false, message = ex.Message }); }
        }

        // API 5: XÓA
        [HttpPost]
        public JsonResult Delete(int id)
        {
            var p = db.Products.FirstOrDefault(x => x.ProductID == id);
            if (p != null)
            {
                p.IsDelete = true;
                db.SubmitChanges();
                return Json(new { success = true });
            }
            return Json(new { success = false });
        }
    }
}