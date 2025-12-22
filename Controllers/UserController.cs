using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using QuanLyTapHoa;

namespace QuanLyTapHoa.Controllers
{
    public class UserController : Controller
    {
        QL_TapHoaEntities db = new QL_TapHoaEntities();

        // GET: Đăng nhập
        public ActionResult Login()
        {
            return View();
        }

        // POST: Xử lý Đăng nhập
        [HttpPost]
        public ActionResult Login(FormCollection collect)
        {
            // 1. Sửa dòng này: Lấy dữ liệu từ input có name="TenDangNhap"
            var sTaiKhoan = collect["TenDangNhap"];
            var sMatKhau = collect["MatKhau"];

            if (string.IsNullOrEmpty(sTaiKhoan) || string.IsNullOrEmpty(sMatKhau))
            {
                ViewBag.Error = "Vui lòng nhập tên đăng nhập và mật khẩu!";
                return View();
            }

            // 2. Logic kiểm tra: Chấp nhận cả Tên đăng nhập HOẶC Email
            // (Vì người dùng có thể nhập "khach1" hoặc "khach1@test.com")
            tblKhachHang kh = db.tblKhachHangs.FirstOrDefault(x =>
                (x.TenDangNhap == sTaiKhoan || x.Email == sTaiKhoan) && x.MatKhau == sMatKhau);

            if (kh != null)
            {
                // Kiểm tra xem tài khoản có bị khóa không (nếu có cột TrangThai)
                if (kh.TrangThai == false)
                {
                    ViewBag.Error = "Tài khoản của bạn đang bị khóa!";
                    return View();
                }

                // Đăng nhập thành công
                Session["User"] = kh;
                Session["TenHienThi"] = kh.TenKH; // Lưu tên hiển thị (Nguyễn Văn A)

                return RedirectToAction("Index", "Home");
            }
            else
            {
                ViewBag.Error = "Tên đăng nhập hoặc Mật khẩu không đúng!";
                // Lưu lại tên đăng nhập để người dùng không phải gõ lại
                ViewBag.Email = sTaiKhoan; // (Lưu ý: Trong View bạn đang dùng @ViewBag.Email để điền lại value)
                return View();
            }
        }

        // Đăng xuất
        public ActionResult Logout()
        {
            Session.Clear();
            return RedirectToAction("Login");
        }

        // GET: Đăng ký
        public ActionResult Register()
        {
            return View();
        }

        // POST: Xử lý Đăng ký
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Register(tblKhachHang kh, string MatKhauXacNhan)
        {
            if (ModelState.IsValid)
            {
                // Kiểm tra mật khẩu xác nhận
                if (kh.MatKhau != MatKhauXacNhan)
                {
                    ViewBag.Error = "Mật khẩu xác nhận không khớp!";
                    return View(kh);
                }

                // Kiểm tra Tên đăng nhập đã tồn tại chưa
                var checkUser = db.tblKhachHangs.FirstOrDefault(x => x.TenDangNhap == kh.TenDangNhap);
                if (checkUser != null)
                {
                    ViewBag.Error = "Tên đăng nhập này đã có người dùng!";
                    return View(kh);
                }

                try
                {
                    // Gán các giá trị mặc định nếu null
                    // Cột tên trong hình là NgayDangKy
                    kh.NgayDangKy = DateTime.Now;
                    if (kh.TrangThai == null) kh.TrangThai = true; // Mặc định là hoạt động

                    db.tblKhachHangs.Add(kh);
                    db.SaveChanges();

                    TempData["Success"] = "Đăng ký thành công! Hãy đăng nhập.";
                    return RedirectToAction("Login");
                }
                catch (Exception ex)
                {
                    ViewBag.Error = "Lỗi: " + ex.Message;
                }
            }
            return View(kh);
        }
    }
}