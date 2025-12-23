using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using QuanLyTapHoa.Models;

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
        public ActionResult Login(Account model)
        {
            if (ModelState.IsValid)
            {

                if (string.IsNullOrEmpty(model.USERNAME) || string.IsNullOrEmpty(model.PASSWORD))
                {
                    ViewBag.Error = "Vui lòng nhập tên đăng nhập và mật khẩu!";
                    return View();
                }

                // 2. Logic kiểm tra: Chấp nhận cả Tên đăng nhập HOẶC Email
                // Khách hàng đăng nhập
                tblKhachHang kh = db.tblKhachHangs.FirstOrDefault(x => (x.TenDangNhap == model.USERNAME || x.Email == model.USERNAME) && x.MatKhau == model.PASSWORD);

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
                    Session["TenHienThi"] = kh.TenKH;
                    Session["Role"] = "User";

                    return RedirectToAction("Index", "Home");
                }

                // Nhân viên đăng nhập
                var nv = db.tblNhanViens.FirstOrDefault(x => x.TenDangNhap == model.USERNAME && x.MatKhau == model.PASSWORD);
                if (nv != null)
                {
                    if (nv.TrangThai == false)
                    {
                        ViewBag.Error = "Tài khoản nhân viên này đã bị ngưng hoạt động!";
                        return View(model);
                    }

                    Session["User"] = nv;
                    Session["TenHienThi"] = nv.TenNV;

                    // Kiểm tra IDVaiTro để định danh Admin hoặc Nhân viên
                    if (nv.VaiTro == 1)
                        Session["Role"] = "Admin";
                    else
                        Session["Role"] = "Nhân viên";

                    return RedirectToAction("Index", "Home");
                }
                ViewBag.Error = "Tên đăng nhập hoặc Mật khẩu không đúng!";
            }
                return View(model);
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