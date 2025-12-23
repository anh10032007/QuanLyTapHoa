using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace QuanLyTapHoa.Models
{
    public class SanPham
    {
        public int MaSP { get; set; }
        public string TenSP { get; set; }

        public int MaDM { get; set; }
        public int MaNCC { get; set; }

        public string DonViTinh { get; set; }
        public decimal? GiaBan { get; set; }
        public int? SoLuongTon { get; set; }

        public string AnhSP { get; set; }
        public string MoTaChiTiet { get; set; }

        public bool? TrangThai { get; set; } = true;
        public bool? LaSanPhamMoi { get; set; } = false;
        public bool? LaSanPhamNoiBat { get; set; } = false;
    }

}
