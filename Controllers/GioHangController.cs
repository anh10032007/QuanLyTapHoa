using QuanLyTapHoa.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace QuanLyTapHoa.Controllers
{
    public class GioHangController : Controller
    {
        QL_TapHoaEntities db = new QL_TapHoaEntities();

        // Lấy giỏ hàng từ Session (Trả về đối tượng Cart)
        public Cart LayGioHang()
        {
            Cart cart = Session["GioHang"] as Cart;
            if (cart == null)
            {
                cart = new Cart();
                Session["GioHang"] = cart;
            }
            return cart;
        }

        // 1. Trang giỏ hàng
        public ActionResult Index()
        {
            Cart cart = LayGioHang();

            // Nếu giỏ hàng trống thì có thể thông báo hoặc vẫn hiện view
            if (cart.SoLuongMatHang() == 0)
            {
                ViewBag.ThongBao = "Giỏ hàng đang trống";
            }
            return View(cart); // Truyền model Cart sang View
        }

        // 2. Thêm vào giỏ hàng
        public ActionResult Them(int iMaSP, string strURL)
        {
            Cart cart = LayGioHang();

            var sp = db.tblSanPhams.Find(iMaSP);
            if (sp == null || sp.SoLuongTon <= 0)
            {
                TempData["Error"] = "Sản phẩm đã hết hàng";
                return Redirect(strURL);
            }

            // kiểm tra số lượng trong giỏ
            var item = cart.list.FirstOrDefault(x => x.MaSP == iMaSP);
            int soLuongDangCo = item != null ? item.SoLuong : 0;

            if (soLuongDangCo + 1 > sp.SoLuongTon)
            {
                TempData["Error"] = "Không thể mua quá số lượng tồn";
                return Redirect(strURL);
            }

            cart.Them(iMaSP);
            Session["GioHang"] = cart;

            return Redirect(strURL);
        }


        // 3. Mua Ngay (Thêm và chuyển đến trang giỏ hàng)
        public ActionResult MuaNgay(int iMaSP)
        {
            Cart cart = LayGioHang();
            cart.Them(iMaSP);
            Session["GioHang"] = cart;
            return RedirectToAction("Index");
        }

        // 4. Cập nhật số lượng
        // FormCollection f dùng để lấy giá trị từ input name="txtSoLuong"
        public ActionResult CapNhatGioHang(int iMaSP, FormCollection f)
        {
            Cart cart = LayGioHang();
            int soLuongMoi = int.Parse(f["txtSoLuong"]);

            var sp = db.tblSanPhams.Find(iMaSP);
            var item = cart.list.FirstOrDefault(n => n.MaSP == iMaSP);

            if (item != null && sp != null)
            {
                if (soLuongMoi <= 0)
                {
                    cart.list.Remove(item);
                }
                else if (soLuongMoi > sp.SoLuongTon)
                {
                    TempData["Error"] = "Số lượng vượt quá tồn kho";
                }
                else
                {
                    item.SoLuong = soLuongMoi;
                }
            }

            Session["GioHang"] = cart;
            return RedirectToAction("Index");
        }


        // 5. Xóa sản phẩm
        public ActionResult XoaGioHang(int iMaSP)
        {
            Cart cart = LayGioHang();
            cart.Xoa(iMaSP);
            Session["GioHang"] = cart;
            return RedirectToAction("Index");
        }

        // 6. TRANG XÁC NHẬN ĐẶT HÀNG (GET)
        [HttpGet]
        public ActionResult DatHang()
        {
            // Kiểm tra đăng nhập
            if (Session["User"] == null)
            {
                return RedirectToAction("Login", "User");
            }

            // Kiểm tra giỏ hàng
            Cart cart = LayGioHang();
            if (cart.SoLuongMatHang() == 0)
            {
                return RedirectToAction("Index", "Home");
            }

            // Lấy thông tin khách hàng để điền sẵn vào form
            tblKhachHang kh = (tblKhachHang)Session["User"];
            ViewBag.KhachHang = kh;

            // Truyền giỏ hàng sang View để hiển thị danh sách sản phẩm bên cạnh
            return View(cart);
        }

        // 7. XỬ LÝ ĐẶT HÀNG (POST)
        [HttpPost]
        public ActionResult DatHang(FormCollection collection)
        {
            try
            {
                Cart cart = LayGioHang();
                tblKhachHang kh = (tblKhachHang)Session["User"];

                // A. Tạo dữ liệu cho bảng Đơn Hàng (tblDonHang)
                tblDonHang ddh = new tblDonHang();
                ddh.MaKH = kh.MaKH;
                ddh.NgayDat = DateTime.Now;
                ddh.TongTien = cart.TongThanhTien();

      

                db.tblDonHangs.Add(ddh);
                db.SaveChanges();


                foreach (var item in cart.list)
                {
                    var sp = db.tblSanPhams.Find(item.MaSP);
                    if (sp == null || sp.SoLuongTon < item.SoLuong)
                    {
                        TempData["Error"] = "Sản phẩm không đủ số lượng tồn";
                        return RedirectToAction("Index", "GioHang");
                    }

                    // Tạo chi tiết đơn hàng
                    tblChiTietDonHang ctdh = new tblChiTietDonHang
                    {
                        MaDon = ddh.MaDon,
                        MaSP = item.MaSP,
                        SoLuong = item.SoLuong,
                        DonGiaLucMua = item.DonGia
                    };

                    db.tblChiTietDonHangs.Add(ctdh);

                    sp.SoLuongTon -= item.SoLuong;

                    if (sp.SoLuongTon <= 0)
                    {
                        sp.TrangThai = false;
                    }
                }

                db.SaveChanges();          
                Session["GioHang"] = null;

                // Chuyển hướng đến trang thông báo thành công
                return RedirectToAction("XacNhanDonHang");
            }
            catch (Exception ex)
            {
            
                return RedirectToAction("Index");
            }
        }
     
        public ActionResult XacNhanDonHang()
        {
            return View();
        }
    }
}