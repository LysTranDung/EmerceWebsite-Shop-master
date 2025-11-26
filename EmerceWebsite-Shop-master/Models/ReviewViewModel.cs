using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EmerceWebsite_Shop_master.Models
{
    // Class này dùng để hứng dữ liệu từ LINQ query trong Controller
    // Tách biệt với các class tự sinh của DatabaseDataContext
    public class ReviewViewModel
    {
        public Review Review { get; set; }
        public string ProductName { get; set; }
        public string CustomerName { get; set; }
        public ReviewResponse Response { get; set; } // Có thể null nếu chưa phản hồi
    }
}