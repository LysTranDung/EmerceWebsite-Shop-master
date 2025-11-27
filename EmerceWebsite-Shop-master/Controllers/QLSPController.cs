using EmerceWebsite_Shop_master.Models;
using System;
using System.Linq;
using System.Web.Mvc;

namespace EmerceWebsite_Shop_master.Controllers
{
    public class QLSPController : Controller
    {

        DatabaseDataContext db = new DatabaseDataContext();
        // Giả sử ShopID hiện tại là 1
        int currentShopId = 1;

        // TRANG DANH SÁCH 
        public ActionResult DSSP()
        {
            return View();
        }

        // TRANG THÊM MỚI
        public ActionResult ThemSP()
        {
   
            return View(new Product());
        }

        // 3. TRANG CHỈNH SỬA
        public ActionResult ChinhSua(int? id)
        {
            ViewBag.ProductID = id ?? 0;
            return View();
        }

      

        //  LẤY DANH SÁCH SẢN PHẨM
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

                return Json(new { success = true, data = list });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi truy vấn Database: " + ex.Message });
            }
        }

        //  LẤY CHI TIẾT 1 SẢN PHẨM 
        [HttpPost]
        public JsonResult GetProductDetails(int id)
        {
            if (id <= 0) return Json(null);

            var p = db.Products.Select(x => new {
                x.ProductID,
                x.ProductName,
                x.Price,
                x.Stock,
                x.Brand,
                // Bổ sung các trường mô tả mới
                x.ProductDescription,
                x.ProductFeature,
                x.DescriptionDetails
            })
            .FirstOrDefault(x => x.ProductID == id);

            return Json(p);
        }

        // THÊM SẢN PHẨM 
        [HttpPost]
        public JsonResult Insert(Product product)
        {
            try
            {
                // Kiểm tra điều kiện bắt buộc
                if (string.IsNullOrEmpty(product.ProductName) || product.Price == null || product.Stock == null)
                {
                    return Json(new { success = false, message = "Vui lòng nhập đầy đủ Tên, Giá và Tồn kho." });
                }

                product.IsDelete = false;

                // Các trường mô tả (ProductDescription, ProductFeature, DescriptionDetails) được gán giá trị 
                // tự động từ form input thông qua Model Binding.
                db.Products.InsertOnSubmit(product);
                db.SubmitChanges();

                ProductShop ps = new ProductShop { ProductID = product.ProductID, ShopID = currentShopId };
                db.ProductShops.InsertOnSubmit(ps);
                db.SubmitChanges();

                return Json(new { success = true, message = "Thêm sản phẩm thành công!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi hệ thống khi thêm sản phẩm: " + ex.Message });
            }
        }

        // CẬP NHẬT 
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
                    existing.ProductFeature = product.ProductFeature;
                    existing.DescriptionDetails = product.DescriptionDetails;
                    
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
                    return Json(new { success = true, message = "Cập nhật sản phẩm thành công!" });
                }
                return Json(new { success = false, message = "Không tìm thấy sản phẩm cần cập nhật." });
            }
            catch (Exception ex) { return Json(new { success = false, message = "Lỗi cập nhật: " + ex.Message }); }
        }

        //  XÓA
        [HttpPost]
        public JsonResult Delete(int id)
        {
            try
            {
                var p = db.Products.FirstOrDefault(x => x.ProductID == id);
                if (p != null)
                {
                    p.IsDelete = true;
                    db.SubmitChanges();
                    return Json(new { success = true, message = "Xóa sản phẩm thành công." });
                }
                return Json(new { success = false, message = "Không tìm thấy sản phẩm cần xóa." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi xóa: " + ex.Message });
            }
        }
    }
}