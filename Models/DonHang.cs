using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace QuanLyTapHoa.Models
{
    public class DonHang
    {
        public int MaDH { get; set; }
        public string TenKhachHang { get; set; }
        public DateTime NgayDat { get; set; }
        public decimal TongTien { get; set; }
        public int TrangThai { get; set; } 
    }
}