using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace QuanLyTapHoa.Controllers
{
    public class HomeController : Controller
    {
      
        QL_TapHoaEntities db = new QL_TapHoaEntities();


        public ActionResult Index()
        {
            var list = db.tblSanPhams
                .Where(x => x.TrangThai == true && x.SoLuongTon > 0)
                .ToList();

            return View(list);
        }



        public ActionResult _DanhMuc()
        {
            return PartialView(db.tblDanhMucs.ToList());
        }

        public ActionResult _NhaCungCap()
        {
            return PartialView(db.tblNhaCungCaps.ToList());
        }

    
        public ActionResult TimTheoDanhMuc(int id)
        {
            List<tblSanPham> list = db.tblSanPhams.Where(x => x.MaDM == id).ToList();
            return View("Index", list);
        }

        // 5. Tìm kiếm theo Nhà cung cấp (ID)
        public ActionResult TimTheoNhaCungCap(int id)
        {
            // Giả sử cột khóa ngoại trong tblSanPham là MaNCC
            List<tblSanPham> list = db.tblSanPhams.Where(x => x.MaNCC == id).ToList();
            return View("Index", list);
        }

        // 6. Tìm kiếm theo từ khóa (Tên sản phẩm)
        public ActionResult TimKiem(string keyword)
        {
            // Giả sử cột tên là TenSanPham
            List<tblSanPham> list = db.tblSanPhams
                .Where(x => x.TenSP.ToLower().Contains(keyword.ToLower()))
                .ToList();
            return View("Index", list);
        }

        // 7. Chi tiết sản phẩm
        public ActionResult Detail(int id)
        {
            // 1. Tìm sản phẩm chính
            var sanpham = db.tblSanPhams.FirstOrDefault(x => x.MaSP == id);
            if (sanpham == null)
            {
                return HttpNotFound();
            }

            // 2. Lấy danh sách sản phẩm liên quan (Cùng danh mục, khác ID hiện tại, lấy 4 cái)
            // Giả sử khóa ngoại là MaDanhMuc
            var sanphamLienQuan = db.tblSanPhams
                .Where(s => s.MaDM == sanpham.MaDM && s.MaSP != id)
                .Take(4)
                .ToList();

            ViewBag.SanphamLienQuan = sanphamLienQuan;

            return View(sanpham);
        }

        // 8. Partial View cho form Tìm kiếm nâng cao (Dropdown list)
        public ActionResult _TimKiemNangCao()
        {
            // Tạo Dropdown cho Danh mục
            ViewBag.ListDanhMuc = new SelectList(db.tblDanhMucs.ToList(), "MaDanhMuc", "TenDanhMuc");

            // Tạo Dropdown cho Nhà cung cấp
            ViewBag.ListNhaCungCap = new SelectList(db.tblNhaCungCaps.ToList(), "MaNCC", "TenNhaCungCap");

            return PartialView();
        }

        // 9. Xử lý logic Tìm kiếm nâng cao
        public ActionResult TimKiemNangCao(FormCollection collect)
        {
            var priceRange = collect["price"];
            var maDanhMuc = collect["MaDanhMuc"];
            var maNCC = collect["MaNCC"];
            var sort = collect["sort"];

            List<tblSanPham> list = new List<tblSanPham>();

            // SỬA 1: Đổi double thành decimal
            decimal min = -1;
            decimal max = -1;

            switch (priceRange)
            {
                case "0": // Dưới 50.000
                    min = 0;
                    max = 50000;
                    // Lưu ý: Nếu cột trong DB tên là GiaBan thì giữ nguyên x.GiaBan
                    // Dùng (x.GiaBan ?? 0) để xử lý trường hợp giá bị null trong DB
                    list = db.tblSanPhams.Where(x => (x.GiaBan ?? 0) >= min && (x.GiaBan ?? 0) <= max).ToList();
                    break;

                case "1": // 50.000 - 100.000
                    min = 50000;
                    max = 100000;
                    list = db.tblSanPhams.Where(x => (x.GiaBan ?? 0) >= min && (x.GiaBan ?? 0) <= max).ToList();
                    break;

                case "2": // Trên 100.000
                    min = 100000;
                    list = db.tblSanPhams.Where(x => (x.GiaBan ?? 0) >= min).ToList();
                    break;

                default: // Tất cả
                    list = db.tblSanPhams.ToList();
                    break;
            }

            // ... (Các phần code lọc theo danh mục, NCC và sort giữ nguyên như cũ) ...

            // Code đoạn dưới giữ nguyên từ câu trả lời trước
            if (!String.IsNullOrEmpty(maDanhMuc))
            {
                int idDanhMuc = Convert.ToInt32(maDanhMuc);
                list = list.Where(x => x.MaDM == idDanhMuc).ToList();
            }

            if (!String.IsNullOrEmpty(maNCC))
            {
                int idNCC = Convert.ToInt32(maNCC);
                list = list.Where(x => x.MaNCC == idNCC).ToList();
            }

            if (!String.IsNullOrEmpty(sort))
            {
                if (sort == "0")
                    list = list.OrderBy(s => s.GiaBan).ToList();
                else
                    list = list.OrderByDescending(s => s.GiaBan).ToList();
            }

            return View("Index", list);
        }
    } 
}