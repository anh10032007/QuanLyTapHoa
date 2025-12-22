using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using QuanLyTapHoa.Models;

namespace QuanLyTapHoa.Controllers
{
    public class AccountController : Controller
    {
        QL_TapHoaEntities db = new QL_TapHoaEntities();

        // GET : display
        // Post: logical handling

        public ActionResult Login() // phương thức GET
        {
            if (Session["User"] != null)
            {
                return RedirectToAction("Index");
            }
            return View();
        }

        // Đăng nhập (phương thức POST)
        [HttpPost]
        public ActionResult Login(Account model)
        {
            if (ModelState.IsValid)
            {
                var kh = db.tblKhachHangs.FirstOrDefault(u => u.TenDangNhap == model.Username && u.MatKhau == model.Password);
                if (kh != null)
                {
                    Session["User"] = kh;
                    Session["TenHienThi"] = kh.TenKH;
                    Session["Role"] = "Khách hàng";
                    return RedirectToAction("Index", "Home");
                }

                // 2. Kiểm tra bảng Nhân viên (Admin)
                var nv = db.tblNhanViens.FirstOrDefault(u => u.TenDangNhap == model.Username && u.MatKhau == model.Password);
                if (nv != null)
                {
                    Session["User"] = nv;
                    Session["TenHienThi"] = nv.TenNV;
                    Session["Role"] = "Admin";
                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    ViewBag.Error = "Sai mật khẩu hoặc tên đăng nhập rồi hẹ hẹ hẹ";
                    return View();
                }
            }
            return View(model);
        }


        public ActionResult Register() // phương thức GET
        {
            return View();
        }

        // Đăng ký (phương thức POST)
        [HttpPost]
        public ActionResult Register(Account model)
        {
            if (ModelState.IsValid)
            {
                var existedUser = db.tblKhachHangs.FirstOrDefault(x => x.TenDangNhap == model.Username); // Kiểm tra tên đăng nhập đã tồn tại
                if (existedUser != null)
                {
                    ViewBag.Error = "Tên đăng nhập đã tồn tại!";
                    return View(model);
                }
                model.NgayDangKy = DateTime.Now;
                model.TrangThai = true;


                var newKH = new tblKhachHang
                {
                    TenDangNhap = model.Username,
                    MatKhau = model.Password,
                    Email = model.Email,
                    SDT = model.SDT,
                    DiaChi = model.DiaChi,
                    NgayDangKy = model.NgayDangKy, 
                    TrangThai = model.TrangThai    
                };
                // Lưu dữ liệu
                db.tblKhachHangs.Add(newKH);
                db.SaveChanges();
                return RedirectToAction("Login"); // quay lại đăng nhập
            }
            return View();
        }

        // Đăng xuất
        public ActionResult Logout()
        {
            Session.Clear(); // Xóa tất cả thông tin trong Session
            return RedirectToAction("Login");
        }
    }
}