using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using QuanLyTapHoa.Models;
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

        // ---------------------------------------------------------
        // GET: Hiển thị form Sửa thông tin cá nhân
        // ---------------------------------------------------------
        public ActionResult EditProfile()
        {
            // 1. Kiểm tra đăng nhập
            if (Session["User"] == null || Session["Role"].ToString() != "User")
            {
                return RedirectToAction("Login");
            }

            // 2. Lấy thông tin khách hàng từ Session
            // Lưu ý: Phải query lại từ DB để đảm bảo dữ liệu mới nhất và Entity Framework theo dõi được thay đổi
            tblKhachHang sessionUser = (tblKhachHang)Session["User"];
            var kh = db.tblKhachHangs.Find(sessionUser.MaKH); // Giả sử khóa chính là MaKH

            if (kh == null)
            {
                return RedirectToAction("Login");
            }

            return View(kh);
        }

        // ---------------------------------------------------------
        // POST: Xử lý lưu thông tin đã sửa
        // ---------------------------------------------------------
        [HttpPost]
        public ActionResult EditProfile(tblKhachHang model, string NhapLaiMatKhau)
        {
            // Lấy lại user từ DB dựa trên ID ẩn trong form
            var kh = db.tblKhachHangs.Find(model.MaKH);

            if (kh == null)
            {
                ViewBag.Error = "Không tìm thấy thông tin người dùng!";
                return View(model);
            }

            if (ModelState.IsValid)
            {
                // 1. Cập nhật các thông tin cơ bản
                kh.TenKH = model.TenKH;
                kh.DiaChi = model.DiaChi;
                kh.SDT = model.SDT;
                kh.Email = model.Email; // Nếu cho phép sửa email

                // 2. Kiểm tra trùng lặp Email/Tên đăng nhập (trừ chính mình ra)
                var checkExist = db.tblKhachHangs.FirstOrDefault(x => x.TenDangNhap == model.TenDangNhap && x.MaKH != model.MaKH);
                if (checkExist != null)
                {
                    ViewBag.Error = "Tên đăng nhập này đã bị người khác sử dụng!";
                    return View(model);
                }

                // 3. Xử lý đổi mật khẩu (Nếu người dùng nhập mật khẩu mới)
                // Logic: Nếu ô mật khẩu trong form không để trống và khác mật khẩu cũ
                if (!string.IsNullOrEmpty(model.MatKhau) && model.MatKhau != kh.MatKhau)
                {
                    if (model.MatKhau != NhapLaiMatKhau)
                    {
                        ViewBag.Error = "Mật khẩu xác nhận không khớp!";
                        return View(model);
                    }
                    // Cập nhật mật khẩu mới
                    kh.MatKhau = model.MatKhau;
                }

                // Lưu ý: Nếu model.MatKhau để trống hoặc null, giữ nguyên mật khẩu cũ trong DB (Entity Framework tự lo việc này nếu bạn không gán đè null vào)
                // Tuy nhiên, để an toàn, tốt nhất nên gán lại mật khẩu cũ nếu người dùng không nhập gì:
                if (string.IsNullOrEmpty(model.MatKhau))
                {
                    // Giữ nguyên mật khẩu cũ
                    // (Đoạn này tuỳ thuộc vào cách bạn làm View, nếu View bind thẳng vào model.MatKhau thì phải cẩn thận)
                }

                try
                {
                    db.SaveChanges();

                    // 4. Cập nhật lại Session để hiển thị đúng tên mới trên Header
                    Session["User"] = kh;
                    Session["TenHienThi"] = kh.TenKH;

                    TempData["Success"] = "Cập nhật thông tin thành công!";
                    return RedirectToAction("EditProfile"); // Load lại trang để thấy thông báo
                }
                catch (Exception ex)
                {
                    ViewBag.Error = "Có lỗi xảy ra: " + ex.Message;
                }
            }

            return View(model);
        }
        // ---------------------------------------------------------
        // GET: Xem thông tin chi tiết (Profile)
        // ---------------------------------------------------------
        // ---------------------------------------------------------
        // GET: Xem thông tin chi tiết (Profile)
        // ---------------------------------------------------------
        public ActionResult Profile()
        {
            // 1. Kiểm tra đăng nhập
            if (Session["User"] == null)
            {
                return RedirectToAction("Login");
            }

            // 2. Kiểm tra vai trò: Chỉ cho phép "User" (Khách hàng) xem trang này
            // Nếu là Admin hoặc Nhân viên thì đá về trang Admin hoặc Home
            if (Session["Role"] != null && Session["Role"].ToString() != "User")
            {
                // Tùy chọn: Chuyển hướng về trang Admin nếu muốn
                // return RedirectToAction("Index", "Admin"); 
                return RedirectToAction("Index", "Home");
            }

            // 3. Lấy thông tin user hiện tại
            var sessionUser = Session["User"] as tblKhachHang;

            // Kiểm tra kỹ lại lần nữa để tránh lỗi Null
            if (sessionUser == null)
            {
                return RedirectToAction("Login");
            }

            // 4. Truy vấn lại từ Database
            var kh = db.tblKhachHangs.Find(sessionUser.MaKH);

            if (kh == null)
            {
                Session.Clear();
                return RedirectToAction("Login");
            }

            return View(kh);
        }
        // ---------------------------------------------------------
        // GET: Xem lịch sử đơn hàng
        // ---------------------------------------------------------
        public ActionResult OrderHistory()
        {
            // 1. Kiểm tra đăng nhập
            if (Session["User"] == null)
            {
                return RedirectToAction("Login");
            }

            // 2. Lấy thông tin user hiện tại
            var user = Session["User"] as tblKhachHang;

            // 3. Lấy danh sách đơn hàng của khách này, sắp xếp giảm dần theo ngày (mới nhất lên đầu)
            // LƯU Ý: Đảm bảo tên bảng đơn hàng của bạn là 'tblDonHangs' hoặc 'tblDonHang'
            var orders = db.tblDonHangs.Where(n => n.MaKH == user.MaKH)
                                       .OrderByDescending(n => n.NgayDat)
                                       .ToList();

            return View(orders);
        }
        // ---------------------------------------------------------
        // GET: Xem chi tiết đơn hàng
        // ---------------------------------------------------------
        public ActionResult OrderDetail(int id)
        {
            // 1. Kiểm tra đăng nhập
            if (Session["User"] == null)
            {
                return RedirectToAction("Login");
            }

            // 2. Lấy user hiện tại
            var user = Session["User"] as tblKhachHang;

            // 3. Truy vấn đơn hàng theo ID và phải đúng là của khách hàng này (Bảo mật)
            var order = db.tblDonHangs.FirstOrDefault(n => n.MaDon == id && n.MaKH == user.MaKH);

            if (order == null)
            {
                return HttpNotFound(); // Hoặc chuyển hướng về trang lỗi
            }

            // Trả về view kèm theo thông tin đơn hàng (bao gồm cả chi tiết sản phẩm nhờ Nav Properties)
            return View(order);
        }

    }
}