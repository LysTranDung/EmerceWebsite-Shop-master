using EmerceWebsite_Shop_master.Models; // << Đảm bảo tên namespace này đúng
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace EmerceWebsite_Shop_master.Controllers // << Đảm bảo tên namespace này đúng
{
    public class QLSPController : Controller
    {
        //================================================================
        // CÁC ACTION TRẢ VỀ VIEW (CÁC TRANG HTML)
        //================================================================

        // GET: /QLSP/DSSP
        // Hiển thị trang danh sách sản phẩm
        public ActionResult DSSP()
        {
            return View();
        }

        // GET: /QLSP/ThemSP
        // Hiển thị form để thêm sản phẩm mới
        public ActionResult ThemSP()
        {
            return View();
        }

        // GET: /QLSP/ChinhSua/5
        // Hiển thị form để sửa sản phẩm với id tương ứng
        public ActionResult ChinhSua(int id)
        {
            // Truyền ProductID sang View để JavaScript có thể dùng ID này gọi API lấy chi tiết sản phẩm
            ViewBag.ProductID = id;
            return View();
        }


        //================================================================
        // CÁC ACTION API (XỬ LÝ LOGIC, TRẢ VỀ DỮ LIỆU JSON/STRING)
        //================================================================

        /// <summary>
        /// API để lấy danh sách sản phẩm của một cửa hàng.
        /// Được gọi bằng AJAX từ trang DSSP.
        /// </summary>
        [HttpPost] // Hoặc có thể dùng HttpGet nếu muốn
        public JsonResult GetProducts()
        {
            try
            {
                int currentShopId = 1; // Giả định: chủ shop đang đăng nhập quản lý ShopID = 1

                DatabaseDataContext db = new DatabaseDataContext();

                var products_qr = from p in db.Products
                                  join ps in db.ProductShops on p.ProductID equals ps.ProductID
                                  where ps.ShopID == currentShopId && (p.IsDelete == null || p.IsDelete == false)
                                  select new
                                  {
                                      p.ProductID,
                                      p.ProductName,
                                      p.Price,
                                      p.Stock,
                                      p.Brand
                                  };

                return Json(products_qr.ToList(), JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {
                // Trả về một mảng rỗng nếu có lỗi để tránh làm sập JavaScript ở client
                return Json(new List<object>(), JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// API để lấy thông tin chi tiết của một sản phẩm.
        /// Được gọi bằng AJAX từ trang ChinhSua.
        /// </summary>
        [HttpPost]
        public JsonResult GetProductDetails(int id)
        {
            try
            {
                DatabaseDataContext db = new DatabaseDataContext();
                var product = db.Products.FirstOrDefault(p => p.ProductID == id);
                return Json(product, JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {
                return Json(null, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// API để thêm một sản phẩm mới.
        /// Được gọi bằng AJAX từ trang ThemSP.
        /// </summary>
        [HttpPost]
        public string Insert()
        {
            try
            {
                string productName = Request.Form["txt_ProductName"];
                string description = Request.Form["txt_Description"];
                decimal price = decimal.Parse(Request.Form["txt_Price"]);
                int stock = int.Parse(Request.Form["txt_Stock"]);
                string brand = Request.Form["txt_Brand"];

                if (string.IsNullOrWhiteSpace(productName))
                {
                    return "Tên sản phẩm không được để trống.";
                }

                DatabaseDataContext db = new DatabaseDataContext();

                Product new_product = new Product
                {
                    ProductName = productName,
                    ProductDescription = description,
                    Price = price,
                    Stock = stock,
                    Brand = brand,
                    IsDelete = false
                };

                db.Products.InsertOnSubmit(new_product);
                db.SubmitChanges();

                int currentShopId = 1; // Giả định ShopID = 1
                ProductShop productShopLink = new ProductShop
                {
                    ProductID = new_product.ProductID,
                    ShopID = currentShopId,
                    IsDelete = false
                };

                db.ProductShops.InsertOnSubmit(productShopLink);
                db.SubmitChanges();

                return "Thêm mới sản phẩm thành công!";
            }
            catch (Exception ex)
            {
                return "Thêm mới thất bại. Chi tiết lỗi: " + ex.Message;
            }
        }

        /// <summary>
        /// API để cập nhật thông tin một sản phẩm.
        /// Được gọi bằng AJAX từ trang ChinhSua.
        /// </summary>
        [HttpPost]
        public string Update()
        {
            try
            {
                int productId = int.Parse(Request.Form["txt_ProductID_hide"]);
                string productName = Request.Form["txt_ProductName"];
                string description = Request.Form["txt_Description"];
                decimal price = decimal.Parse(Request.Form["txt_Price"]);
                int stock = int.Parse(Request.Form["txt_Stock"]);
                string brand = Request.Form["txt_Brand"];

                DatabaseDataContext db = new DatabaseDataContext();

                Product product_to_update = db.Products.FirstOrDefault(p => p.ProductID == productId);

                if (product_to_update != null)
                {
                    product_to_update.ProductName = productName;
                    product_to_update.ProductDescription = description;
                    product_to_update.Price = price;
                    product_to_update.Stock = stock;
                    product_to_update.Brand = brand;

                    db.SubmitChanges();
                    return "Cập nhật thông tin sản phẩm thành công!";
                }
                else
                {
                    return "Không tìm thấy sản phẩm để cập nhật.";
                }
            }
            catch (Exception ex)
            {
                return "Cập nhật thất bại. Chi tiết lỗi: " + ex.Message;
            }
        }


        /// <summary>
        /// API để xóa (xóa mềm) một sản phẩm.
        /// Được gọi bằng AJAX từ trang DSSP.
        /// </summary>
        [HttpPost]
        public string Delete(int id)
        {
            try
            {
                DatabaseDataContext db = new DatabaseDataContext();

                Product product_to_delete = db.Products.FirstOrDefault(p => p.ProductID == id);

                if (product_to_delete != null)
                {
                    product_to_delete.IsDelete = true;
                    db.SubmitChanges();
                    return "Xóa sản phẩm thành công!";
                }
                else
                {
                    return "Không tìm thấy sản phẩm để xóa.";
                }
            }
            catch (Exception ex)
            {
                return "Xóa thất bại. Chi tiết lỗi: " + ex.Message;
            }
        }
    }
}