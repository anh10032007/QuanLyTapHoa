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
            return View(db.tblSanPhams.ToList());
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
        [HttpGet]
        public ActionResult TimKiemNangCao()
        {
            // Lấy dữ liệu cho Dropdown
            ViewBag.ListDanhMuc = new SelectList(db.tblDanhMucs.ToList(), "MaDM", "TenDM"); // Kiểm tra lại tên cột MaDM/MaDanhMuc trong DB của bạn
            ViewBag.ListNhaCungCap = new SelectList(db.tblNhaCungCaps.ToList(), "MaNCC", "TenNCC");

            return View();
        }

        // 2. POST: Xử lý tìm kiếm khi người dùng nhấn nút "Tìm ngay"
        [HttpPost] // Hoặc dùng [HttpGet] nếu muốn hiện tham số trên URL
        public ActionResult KetQuaTimKiemNangCao(FormCollection collect)
        {
            var priceRange = collect["price"];
            var maDanhMuc = collect["MaDanhMuc"];
            var maNCC = collect["MaNCC"];
            var sort = collect["sort"];

            var list = db.tblSanPhams.AsQueryable(); // Dùng AsQueryable để tối ưu truy vấn

            // 1. Lọc theo giá
            if (!string.IsNullOrEmpty(priceRange))
            {
                switch (priceRange)
                {
                    case "0": // Dưới 50k
                        list = list.Where(x => x.GiaBan < 50000);
                        break;
                    case "1": // 50k - 100k
                        list = list.Where(x => x.GiaBan >= 50000 && x.GiaBan <= 100000);
                        break;
                    case "2": // Trên 100k
                        list = list.Where(x => x.GiaBan > 100000);
                        break;
                }
            }

            // 2. Lọc theo Danh mục
            if (!string.IsNullOrEmpty(maDanhMuc))
            {
                int idDM = int.Parse(maDanhMuc);
                list = list.Where(x => x.MaDM == idDM);
            }

            // 3. Lọc theo Nhà cung cấp
            if (!string.IsNullOrEmpty(maNCC))
            {
                int idNCC = int.Parse(maNCC);
                list = list.Where(x => x.MaNCC == idNCC);
            }

            // 4. Sắp xếp
            if (!string.IsNullOrEmpty(sort))
            {
                if (sort == "asc")
                    list = list.OrderBy(s => s.GiaBan);
                else
                    list = list.OrderByDescending(s => s.GiaBan);
            }

            // Trả về View Index để hiển thị danh sách kết quả
            return View("Index", list.ToList());
        }
    } 
}