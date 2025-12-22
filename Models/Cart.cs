using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace QuanLyTapHoa.Models
{
    public class Cart
    {
        // List chứa các sản phẩm
        public List<CartItem> list;

        public Cart()
        {
            list = new List<CartItem>();
        }

        public Cart(List<CartItem> ds)
        {
            list = ds;
        }

        // Đếm số lượng đầu sản phẩm (ví dụ: Mì, Nước, Gạo => 3 loại)
        public int SoLuongMatHang()
        {
            if (list == null) return 0;
            return list.Count;
        }

        // Đếm tổng số lượng (ví dụ: 2 gói mì + 3 chai nước = 5)
        public int TongSLHang()
        {
            if (list == null) return 0;
            return list.Sum(x => x.SoLuong);
        }

        // Tính tổng tiền giỏ hàng
        public decimal TongThanhTien()
        {
            if (list == null) return 0;
            return list.Sum(x => x.ThanhTien);
        }

        // Thêm sản phẩm
        public int Them(int id)
        {
            try
            {
                // Tìm xem sản phẩm đã có trong list chưa
                CartItem item = list.Find(x => x.MaSP == id);

                if (item == null)
                {
                    // Chưa có thì tạo mới và add vào list
                    item = new CartItem(id);
                    list.Add(item);
                }
                else
                {
                    // Có rồi thì tăng số lượng
                    item.SoLuong++;
                }
                return 1; // Thành công
            }
            catch (Exception)
            {
                return -1; // Lỗi
            }
        }

        // Giảm số lượng
        public int Giam(int id)
        {
            try
            {
                CartItem item = list.Find(x => x.MaSP == id);
                if (item != null)
                {
                    item.SoLuong--;
                    // Nếu giảm xuống 0 hoặc âm thì xóa luôn
                    if (item.SoLuong <= 0)
                    {
                        list.Remove(item);
                    }
                }
                return 1;
            }
            catch (Exception)
            {
                return -1;
            }
        }

        // Xóa hẳn sản phẩm
        public int Xoa(int id)
        {
            try
            {
                CartItem item = list.Find(x => x.MaSP == id);
                if (item != null)
                {
                    list.Remove(item);
                }
                return 1;
            }
            catch (Exception)
            {
                return -1;
            }
        }

        // Cập nhật số lượng theo input (Bổ sung thêm cho tiện)
        public int CapNhatSL(int id, int slMoi)
        {
            try
            {
                CartItem item = list.Find(x => x.MaSP == id);
                if (item != null)
                {
                    if (slMoi <= 0) list.Remove(item);
                    else item.SoLuong = slMoi;
                }
                return 1;
            }
            catch { return -1; }
        }
    }
}