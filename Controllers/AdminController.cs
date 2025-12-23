using QuanLyTapHoa.Models;
using System;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace QuanLyTapHoa.Controllers
{
    public class AdminController : Controller
    {
        QL_TapHoaEntities db = new QL_TapHoaEntities();

        public ActionResult Products()
        {
            var ds = db.tblSanPhams.Select(x => new SanPham
                {
                    MaSP = x.MaSP,
                    TenSP = x.TenSP,
                    GiaBan = x.GiaBan,
                    SoLuongTon = x.SoLuongTon,
                    TrangThai = x.TrangThai,
                    AnhSP = x.AnhSP
                })
                .ToList();

            return View(ds); 
        }

        // GET
        public ActionResult Create()
        {
            ViewBag.DanhMuc = db.tblDanhMucs.ToList();
            ViewBag.NhaCungCap = db.tblNhaCungCaps.ToList();
            return View();
        }



        // POST
        [HttpPost]
        public ActionResult Create(SanPham sp, HttpPostedFileBase UploadImage)
        {
            string fileName = "noimage.png";

            if (UploadImage != null && UploadImage.ContentLength > 0)
            {
                fileName = System.IO.Path.GetFileName(UploadImage.FileName);
                string path = Server.MapPath("~/Content/Images/" + fileName);
                UploadImage.SaveAs(path);
            }

            tblSanPham entity = new tblSanPham
            {
                TenSP = sp.TenSP,
                MaDM = sp.MaDM,
                MaNCC = sp.MaNCC,
                GiaBan = sp.GiaBan,
                SoLuongTon = sp.SoLuongTon,
                MoTaChiTiet = sp.MoTaChiTiet,
                AnhSP = fileName,
                TrangThai = true,
                LaSanPhamMoi = false,
                LaSanPhamNoiBat = false
            };

            db.tblSanPhams.Add(entity);
            db.SaveChanges();

            return RedirectToAction("Products");
        }



        //GET
        public ActionResult EditProduct(int id)
        {
            var x = db.tblSanPhams.Find(id);
            if (x == null) return HttpNotFound();

            ViewBag.DanhMuc = db.tblDanhMucs.ToList();
            ViewBag.NhaCungCap = db.tblNhaCungCaps.ToList();

            SanPham sp = new SanPham
            {
                MaSP = x.MaSP,
                TenSP = x.TenSP,
                MaDM = x.MaDM ?? 1,
                MaNCC = x.MaNCC ?? 1,
                GiaBan = x.GiaBan,
                SoLuongTon = x.SoLuongTon,
                MoTaChiTiet = x.MoTaChiTiet,
                AnhSP = x.AnhSP,
                TrangThai = x.TrangThai
            };

            return View(sp);
        }


        // POST
        [HttpPost]
        public ActionResult EditProduct(SanPham sp, HttpPostedFileBase UploadImage)
        {
            var x = db.tblSanPhams.Find(sp.MaSP);
            if (x == null) return HttpNotFound();

            // xử lý ảnh
            if (UploadImage != null && UploadImage.ContentLength > 0)
            {
                string fileName = System.IO.Path.GetFileName(UploadImage.FileName);
                string path = Server.MapPath("~/Content/images/" + fileName);
                UploadImage.SaveAs(path);
                x.AnhSP = fileName;
            }

            x.TenSP = sp.TenSP;
            x.MaDM = sp.MaDM;
            x.MaNCC = sp.MaNCC;
            x.GiaBan = sp.GiaBan;
            x.SoLuongTon = sp.SoLuongTon;
            x.MoTaChiTiet = sp.MoTaChiTiet;
            x.TrangThai = sp.TrangThai;

            db.SaveChanges();
            return RedirectToAction("Products");
        }



        public ActionResult Delete(int id)
        {
            var sp = db.tblSanPhams.Find(id);
            db.tblSanPhams.Remove(sp);
            db.SaveChanges();
            return RedirectToAction("Products");
        }

        // GET + LIST
        public ActionResult DanhMuc()
        {
            var data = db.tblDanhMucs
                .Select(dm => new DanhMuc
                {
                    MaDM = dm.MaDM,
                    TenDM = dm.TenDM,
                    SoSanPham = db.tblSanPhams.Count(sp => sp.MaDM == dm.MaDM)
                })
                .ToList();

            return View(data);
        }

        // POST - THÊM
        [HttpPost]
        public ActionResult DanhMuc(tblDanhMuc dm)
        {
            if (!string.IsNullOrWhiteSpace(dm.TenDM))
            {
                db.tblDanhMucs.Add(dm);
                db.SaveChanges();
            }
            return RedirectToAction("DanhMuc");
        }

        // XÓA
        public ActionResult DeleteCategory(int id)
        {
            int count = db.tblSanPhams.Count(sp => sp.MaDM == id);

            if (count > 0)
            {
                TempData["Error"] = "Không thể xóa danh mục đã có sản phẩm";
                return RedirectToAction("DanhMuc");
            }

            var dm = db.tblDanhMucs.Find(id);
            if (dm != null)
            {
                db.tblDanhMucs.Remove(dm);
                db.SaveChanges();
            }

            return RedirectToAction("DanhMuc");
        }


        // ================= NHÀ CUNG CẤP =================

        // GET + LIST
        public ActionResult NhaCungCap()
        {
            var data = db.tblNhaCungCaps
                .Select(ncc => new NhaCungCap
                {
                    MaNCC = ncc.MaNCC,
                    TenNCC = ncc.TenNCC,
                    SoSanPham = db.tblSanPhams.Count(sp => sp.MaNCC == ncc.MaNCC)
                })
                .ToList();

            return View(data);
        }

        // POST - THÊM
        [HttpPost]
        public ActionResult NhaCungCap(tblNhaCungCap ncc)
        {
            if (!string.IsNullOrWhiteSpace(ncc.TenNCC))
            {
                db.tblNhaCungCaps.Add(ncc);
                db.SaveChanges();
            }

            return RedirectToAction("NhaCungCap");
        }

        // XÓA
        public ActionResult DeleteSupplier(int id)
        {
            int count = db.tblSanPhams.Count(sp => sp.MaNCC == id);

            if (count > 0)
            {
                TempData["Error"] = "Không thể xóa nhà cung cấp đã có sản phẩm";
                return RedirectToAction("NhaCungCap");
            }

            var ncc = db.tblNhaCungCaps.Find(id);
            if (ncc != null)
            {
                db.tblNhaCungCaps.Remove(ncc);
                db.SaveChanges();
            }

            return RedirectToAction("NhaCungCap");
        }

        //=========================================
        // 1. Danh sách đơn hàng
        public ActionResult QuanLyDonHang()
        { 

            // Lấy danh sách từ bảng database thật
            var ds = db.tblDonHangs.OrderByDescending(x => x.NgayDat).ToList();
            return View(ds);
        }

        // 2. Chi tiết đơn hàng
        // Đổi int id thành int? id để tránh lỗi "null entry for parameter 'id'"
        public ActionResult OrderDetail(int? id)
        {


            if (id == null) return RedirectToAction("QuanLyDonHang");

            var donHang = db.tblDonHangs.Find(id);
            if (donHang == null) return HttpNotFound();

            return View(donHang);
        }

        // 3. Duyệt đơn hàng
        public ActionResult DuyetDon(int? id)
        {
            if (id == null) return RedirectToAction("QuanLyDonHang");

            var dh = db.tblDonHangs.Find(id);
            if (dh != null)
            {
                dh.TrangThai = 1; // Giả sử 1 là Hoàn thành
                db.SaveChanges();
            }
            return RedirectToAction("QuanLyDonHang");
        }

    }
}
