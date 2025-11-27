using EmerceWebsite_Shop_master.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
// using EmerceWebsite_Shop_master.Models; 

namespace EmerceWebsite_Shop_master.Controllers
{
    public class QLReviewController : Controller
    {
        private DatabaseDataContext db = new DatabaseDataContext();

        public ActionResult QLReviewIndex()
        {
            List<ReviewViewModel> reviewsData = db.Reviews
                .Select(r => new ReviewViewModel
                {
                    Review = r,
                    ProductName = r.Product.ProductName,
                    CustomerName = r.Customer.FullName,
                    Response = r.ReviewResponses.FirstOrDefault()
                }).ToList();

            ViewBag.Title = "Quản lý Đánh giá & Phản hồi";
            return View(reviewsData);
        }
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
                    ShopUserID = 1, 
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
            catch (Exception ex)
            {
                string detailedError = ex.InnerException != null ? ex.InnerException.Message : ex.Message;

                return Json(new { success = false, message = "LỖI DATABASE KHI LƯU DỮ LIỆU. Chi tiết: " + detailedError });
            }
        } 
    }
}