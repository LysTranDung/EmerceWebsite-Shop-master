using System;
using System.Collections.Generic; // Cần thiết cho List
using System.Linq;
using System.Web.Mvc;
using EmerceWebsite_Shop_master.Models;

namespace EmerceWebsite_Shop_master.Controllers
{
    public class QLReviewController : Controller
    {
        private DatabaseDataContext db = new DatabaseDataContext();

        // GET: QLReview
        public ActionResult QLReviewIndex()
        {
            // Query dữ liệu và map vào ViewModel
            List<ReviewViewModel> reviewsData = db.Reviews
                               .OrderByDescending(r => r.ReviewDate)
                               .Select(r => new ReviewViewModel
                               {
                                   Review = r,
                                   ProductName = r.Product.ProductName,
                                   CustomerName = r.Customer.FullName,
                                   // Lấy phản hồi đầu tiên
                                   Response = r.ReviewResponses.FirstOrDefault()
                               }).ToList();

            ViewBag.Title = "Quản lý Đánh giá & Phản hồi";

            // Truyền thẳng Model vào View
            return View(reviewsData);
        }

        // POST: QLReview/RespondToReview
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult RespondToReview(int reviewId, string responseText)
        {
            if (string.IsNullOrWhiteSpace(responseText))
            {
                return Json(new { success = false, message = "Nội dung phản hồi không được để trống." });
            }

            var review = db.Reviews.FirstOrDefault(r => r.ReviewID == reviewId);
            if (review == null)
            {
                return Json(new { success = false, message = "Đánh giá không tồn tại." });
            }

            var existingResponse = db.ReviewResponses.FirstOrDefault(rs => rs.ReviewID == reviewId);

            if (existingResponse != null)
            {
                existingResponse.ResponseText = responseText.Trim();
                existingResponse.ResponseDate = DateTime.Now;
            }
            else
            {
                var newResponse = new ReviewResponse
                {
                    ReviewID = reviewId,
                    ShopUserID = 1, // ID Admin/Shop giả định
                    ResponseText = responseText.Trim(),
                    ResponseDate = DateTime.Now
                };
                db.ReviewResponses.InsertOnSubmit(newResponse);
            }

            try
            {
                db.SubmitChanges();
                return Json(new { success = true, message = "Phản hồi đã được gửi/cập nhật thành công!" });
            }
            catch (Exception)
            {
                return Json(new { success = false, message = "Lỗi Database khi lưu dữ liệu." });
            }
        }
    }
}