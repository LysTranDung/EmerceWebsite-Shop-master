using System;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using EmerceWebsite_Shop_master.Models; 

namespace EmerceWebsite_Shop_master.Controllers
{
    public class QLCHController : Controller
    {
        private DatabaseDataContext db = new DatabaseDataContext();

        // GET: QLCH/ThongTin
        public ActionResult ThongTin()
        {
            // Lấy thông tin cửa hàng đầu tiên 
            Shop shop = db.Shops.FirstOrDefault();

            if (shop == null)
            {
                // Nếu chưa có dữ liệu, tạo một đối tượng Shop mới
                shop = new Shop();
            }

            return View(shop);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult UpdateThongTin(Shop model, HttpPostedFileBase ShopLogoFile)
        {
            if (ModelState.IsValid)
            {
                Shop shopToUpdate = db.Shops.FirstOrDefault();

                if (shopToUpdate == null)
                {
                    // Trường hợp chưa có bản ghi Shop nào, tạo mới và thêm vào DB
                    shopToUpdate = new Shop();
                    db.Shops.InsertOnSubmit(shopToUpdate);
                }

                // --- 1. Xử lý File Ảnh (Logo) ---
                if (ShopLogoFile != null && ShopLogoFile.ContentLength > 0)
                {
                    try
                    {
                        string folderPath = Server.MapPath("~/img/shop/");

                        if (!System.IO.Directory.Exists(folderPath))
                        {
                            System.IO.Directory.CreateDirectory(folderPath);
                        }

                        string fileName = Path.GetFileNameWithoutExtension(ShopLogoFile.FileName)
                                        + "_" + DateTime.Now.Ticks
                                        + Path.GetExtension(ShopLogoFile.FileName);

                        string path = Path.Combine(folderPath, fileName);

                        // Lưu file vật lý
                        ShopLogoFile.SaveAs(path);

                        // Lưu đường dẫn tương đối vào Database
                        shopToUpdate.LogoUrl = "~/img/shop/" + fileName;
                    }
                    catch (Exception ex)
                    {
                        return Json(new { success = false, message = "Lỗi khi lưu file: " + ex.Message });
                    }
                }
        

                // --- Cập nhật các trường thông tin khác ---
                shopToUpdate.ShopName = model.ShopName;
                shopToUpdate.ShopLocation = model.ShopLocation;
                shopToUpdate.ShopPolicy = model.ShopPolicy;
                shopToUpdate.IsActive = model.IsActive; // Giả sử IsActive là bool

                // Cập nhật Database
                db.SubmitChanges();

                return Json(new { success = true, message = "Cập nhật thông tin cửa hàng thành công!" });
            }

            // Dữ liệu không hợp lệ
            var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
            return Json(new { success = false, message = "Dữ liệu không hợp lệ: " + string.Join("; ", errors) });
        }
    }
}