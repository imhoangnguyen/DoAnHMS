using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace DoAnHMS.Models.Payments
{
    public class BookingViewModel
    {
        public string MaP { get; set; }
        public string TenKH { get; set; }
        public string SDT { get; set; }
        public string CMND_Passport { get; set; }
        public string Email { get; set; }
        public DateTime NgayDen { get; set; }
        public DateTime NgayDi { get; set; }
        public int TypePayment { get; set; }
        public int TypePayMentVN { get; set; }
        public int SoLuong { get; set; }

    }
}