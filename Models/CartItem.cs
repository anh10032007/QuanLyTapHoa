using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace QuanLyTapHoa.Models
{
    public class CartItem
    {
        QL_TapHoaEntities db = new QL_TapHoaEntities();

        public int MaSP { get; set; }
        public string TenSP { get; set; }
        public string HinhAnh { get; set; }
        public decimal DonGia { get; set; } // Dùng decimal tính tiền chính xác hơn double
        public int SoLuong { get; set; }

        // Thuộc tính tính toán (Read-only)
        public decimal ThanhTien
        {
            get { return SoLuong * DonGia; }
        }

        // Constructor 1: Khởi tạo khi có ID (lấy từ DB)
        public CartItem(int iMaSP)
        {
            this.MaSP = iMaSP;
            // Truy vấn database để lấy thông tin
            var sp = db.tblSanPhams.FirstOrDefault(n => n.MaSP == iMaSP);
            if (sp != null)
            {
                TenSP = sp.TenSP;
                HinhAnh = sp.AnhSP;
                // Ép kiểu về decimal (nếu trong DB là double/float thì cần ép kiểu)
                DonGia = (decimal)(sp.GiaBan ?? 0);
                SoLuong = 1;
            }
        }

        // Constructor 2: Khởi tạo rỗng (nếu cần)
        public CartItem() { }
    }
}