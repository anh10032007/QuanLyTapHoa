using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace QuanLyTapHoa.Models
{
    public class Account
    {
        public string USERNAME { get; set; }
        public string PASSWORD { get; set; }

        public string TenKH { get; set; }
        public string SDT { get; set; }
        public string Email { get; set; }
        public string DiaChi { get; set; }
        public int TrangThai { get; set; }
        public string VaiTro { get; set; }
    }
}