using DoAnHMS.Models;
using DoAnHMS.Models.Payments;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity;
using System.Linq;
using System.Security.Policy;
using System.Web;
using System.Web.Mvc;

namespace DoAnHMS.Controllers
{
    public class HotelController : Controller
    {

        private readonly DoAnHMSEntities db = new DoAnHMSEntities();

        #region Lấy mã
        string LayMaKH()
        {
            var maMAX = db.KhachHangs.ToList().Select(n => n.maKH).Max();
            int MaKH = int.Parse(maMAX.Substring(2)) + 1;
            string KH = String.Concat("000", MaKH.ToString());
            return "KH" + KH.Substring(MaKH.ToString().Length - 1);
        }
        string LayMaPDP()
        {
            var maMax = db.PhieuDatPhongs.ToList().Select(n => n.maPDP).Max();
            int maPDP = int.Parse(maMax.Substring(3)) + 1;
            string PDP = String.Concat("000", maPDP.ToString());
            return "PDP" + PDP.Substring(maPDP.ToString().Length - 1);
        }
        string LayMaPTP()
        {
            var maMax = db.PhieuThuePhongs.ToList().Select(n => n.maPTP).Max();
            int maPDP = int.Parse(maMax.Substring(3)) + 1;
            string PDP = String.Concat("000", maPDP.ToString());
            return "PTP" + PDP.Substring(maPDP.ToString().Length - 1);
        }

        #endregion

        // GET: Hotel
        public ActionResult Index()
        {
            return View(db.Phongs.ToList());
        }

        public ActionResult DatPhong(string maphong)
        {
            var phong = db.Phongs.SingleOrDefault(n => n.maP.Equals(maphong));
            ViewBag.MaPhongID = phong.maP;
            ViewBag.MaPhong = phong.maP + " (" + phong.LoaiPhong.tenLP + ") ";
            return View();
        }

        /*public ActionResult DatPhong(KhachHang khachhang, PhieuDatPhong phieudatphong, CTPhieuDatPhong ctphieudatphong)*/
        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult DatPhong(BookingViewModel model)
        {
            // Kiểm tra khách hàng này đã từng đặt phòng hay chưa? (Thông qua cmnd_passport và số điện thoại)
            var checkUser = db.KhachHangs.FirstOrDefault(n => n.cmnd_passport.Equals(model.CMND_Passport) && n.sdt.Equals(model.SDT));
            var maPDP = "";
            if (checkUser == null) // trường hợp chưa tồn tại khách hàng trong hệ thống
            {
                // lưu thông tin khách hàng
                KhachHang kh = new KhachHang();
                kh.maKH = LayMaKH();
                kh.tenKH = model.TenKH;
                kh.gioiTinh = true;
                kh.diaChi = "Việt Nam";
                kh.quocTich = "Việt Nam";
                kh.cmnd_passport = model.CMND_Passport;
                kh.email = model.Email;
                kh.sdt = model.SDT;
                db.KhachHangs.Add(kh);
                db.SaveChanges();

                // Lưu thông tin Phiếu đặt phòng
                PhieuDatPhong pdp = new PhieuDatPhong();
                pdp.maPDP = LayMaPDP();
                pdp.maKH = kh.maKH;
                pdp.ngayDen = model.NgayDen;
                pdp.ngayDi = model.NgayDi;
                pdp.tongTienCoc = 0;
                pdp.soNguoi = model.SoLuong;
                pdp.tinhTrang = false;
                pdp.maNV = "NV0001";
                db.PhieuDatPhongs.Add(pdp);
                maPDP = pdp.maPDP;
                db.SaveChanges();

                // Lưu chi tiết phiếu thuê phòng
                CTPhieuDatPhong ctptp = new CTPhieuDatPhong();
                ctptp.maPDP = pdp.maPDP;
                ctptp.maP = model.MaP;
                ctptp.tienCoc = 0;
                db.CTPhieuDatPhongs.Add(ctptp);
                db.SaveChanges();
            }
            else // đã tồn tại khách hàng
            {
                // Lưu thông tin Phiếu đặt phòng
                PhieuDatPhong pdp = new PhieuDatPhong();
                pdp.maPDP = LayMaPDP();
                maPDP = pdp.maPDP;
                pdp.maKH = checkUser.maKH;
                pdp.ngayDen = model.NgayDen;
                pdp.ngayDi = model.NgayDi;
                pdp.tongTienCoc = 0;
                pdp.soNguoi = 1;
                pdp.tinhTrang = false;
                pdp.maNV = "NV0001";
                db.PhieuDatPhongs.Add(pdp);
                db.SaveChanges();

                // Lưu chi tiết phiếu thuê phòng
                CTPhieuDatPhong ctptp = new CTPhieuDatPhong();
                ctptp.maPDP = pdp.maPDP;
                ctptp.maP = model.MaP;
                ctptp.tienCoc = 0;
                db.CTPhieuDatPhongs.Add(ctptp);
                db.SaveChanges();
            }
            var code = new { Success = false, Code = -1, Url = "" };
            if(model.TypePayment == 2)
            {

                var url = UrlPayMent(model.TypePayMentVN,model.MaP,maPDP, model.SoLuong);
                code = new { Success = true,Code = model.TypePayMentVN, Url = url };
            }
            else if(model.TypePayment == 0)
            {
                code = new { Success = true, Code =2, Url = "https://zalo.me/0366201726" };
            }
            else if(model.TypePayment == 1)
            {
                code = new { Success = true, Code = 2, Url = "https://localhost:44355/Hotel/Announce" };
            }
            return Json(code);
         }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "hoTen,sdt,email")] LienHe lienHe)
        {
            if (ModelState.IsValid)
            {
                lienHe.ngayGui = DateTime.Now;
                lienHe.tinhTrang = false;
                db.LienHes.Add(lienHe);
                db.SaveChanges();
                return RedirectToAction("Announce");
            }
            return RedirectToAction("Index");
        }

        public ActionResult Announce()
        {
            return View();
        }

        public ActionResult PhanHoi()
        {
            return View();
        }

        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult PhanHoi([Bind(Include = "hoTen,sdt,email,noiDung")] PhanHoi phanHoi)
        {
            if (ModelState.IsValid)
            {
                phanHoi.ngayGui = DateTime.UtcNow;
                phanHoi.TinhTrang = false;
                if (phanHoi.noiDung.Length > 200)
                {
                    phanHoi.noiDung = phanHoi.noiDung.Substring(0, 200);
                }
                db.PhanHois.Add(phanHoi);
                db.SaveChanges();
                return RedirectToAction("ThankYou");
            }
            return View(phanHoi);
        }

        public ActionResult ThankYou()
        {
            if (Request.QueryString.Count > 0)
            {
                string vnp_HashSecret = ConfigurationManager.AppSettings["vnp_HashSecret"]; //Chuoi bi mat
                var vnpayData = Request.QueryString;
                VnPayLibrary vnpay = new VnPayLibrary();

                foreach (string s in vnpayData)
                {
                    //get all querystring data
                    if (!string.IsNullOrEmpty(s) && s.StartsWith("vnp_"))
                    {
                        vnpay.AddResponseData(s, vnpayData[s]);
                    }
                }
                string orderCode = Convert.ToString(vnpay.GetResponseData("vnp_TxnRef"));
                long vnpayTranId = Convert.ToInt64(vnpay.GetResponseData("vnp_TransactionNo"));
                string vnp_ResponseCode = vnpay.GetResponseData("vnp_ResponseCode");
                string vnp_TransactionStatus = vnpay.GetResponseData("vnp_TransactionStatus");
                String vnp_SecureHash = Request.QueryString["vnp_SecureHash"];
                String TerminalID = Request.QueryString["vnp_TmnCode"];
                long vnp_Amount = Convert.ToInt64(vnpay.GetResponseData("vnp_Amount")) / 100;
                String bankCode = Request.QueryString["vnp_BankCode"];

                bool checkSignature = vnpay.ValidateSignature(vnp_SecureHash, vnp_HashSecret);
                if (checkSignature)
                {
                    if (vnp_ResponseCode == "00" && vnp_TransactionStatus == "00")
                    {
                        /*var itemOrder = db.Orders.FirstOrDefault(x => x.Code== orderCode);
                        if (itemOrder != null)
                        {
                            itemOrder.Status = 2;//đã thanh toán
                            db.Orders.Attach(itemOrder);
                            db.Entry(itemOrder).State = System.Data.Entity.EntityState.Modified;
                            db.SaveChanges();
                        }
                        //Thanh toan thanh cong
                        ViewBag.InnerText = "Giao dịch được thực hiện thành công. Cảm ơn quý khách đã sử dụng dịch vụ";
                        //log.InfoFormat("Thanh toan thanh cong, OrderId={0}, VNPAY TranId={1}", orderId, vnpayTranId);*/
                        var phieuDP = db.PhieuDatPhongs.FirstOrDefault(x => x.maPDP == orderCode);
                        PhieuThuePhong phieuThue = new PhieuThuePhong();
                        phieuThue.maPDP = phieuDP.maPDP;
                        phieuThue.maPTP = LayMaPTP();
                        phieuThue.maKH = phieuDP.maKH;
                        phieuThue.maNV = "NV0001";
                        phieuThue.ngayThue = phieuDP.ngayDen;
                        phieuThue.ngayTra = phieuDP.ngayDi;
                        db.PhieuThuePhongs.Add(phieuThue);
                        db.SaveChanges();

                        phieuDP.tongTienCoc = vnp_Amount;
                        db.Entry(phieuDP).State = EntityState.Modified;
                        db.SaveChanges();

                    }
                    else
                    {
                        //Thanh toan khong thanh cong. Ma loi: vnp_ResponseCode
                        ViewBag.InnerText = "Có lỗi xảy ra trong quá trình xử lý.Mã lỗi: " + vnp_ResponseCode;
                        //log.InfoFormat("Thanh toan loi, OrderId={0}, VNPAY TranId={1},ResponseCode={2}", orderId, vnpayTranId, vnp_ResponseCode);
                    }
                    //displayTmnCode.InnerText = "Mã Website (Terminal ID):" + TerminalID;
                    //displayTxnRef.InnerText = "Mã giao dịch thanh toán:" + orderId.ToString();
                    //displayVnpayTranNo.InnerText = "Mã giao dịch tại VNPAY:" + vnpayTranId.ToString();
                    //ViewBag.ThanhToanThanhCong = "Số tiền thanh toán (VND):" + vnp_Amount.ToString();
                    //displayBankCode.InnerText = "Ngân hàng thanh toán:" + bankCode;
                }
            }
            return View();
        }
        public string UrlPayMent(int TypePaymentVN, string maP ,string maPDP, int soLuong)
        {
            /*var urlPayment = "";*/
            var loadPhong = db.Phongs.FirstOrDefault(x => x.maP == maP);
            var order = db.LoaiPhongs.FirstOrDefault(x => x.maLP == loadPhong.maLP);
            //Get Config Info
            string vnp_Returnurl = ConfigurationManager.AppSettings["vnp_Returnurl"]; //URL nhan ket qua tra ve 
            string vnp_Url = ConfigurationManager.AppSettings["vnp_Url"]; //URL thanh toan cua VNPAY 
            string vnp_TmnCode = ConfigurationManager.AppSettings["vnp_TmnCode"]; //Ma định danh merchant kết nối (Terminal Id)
            string vnp_HashSecret = ConfigurationManager.AppSettings["vnp_HashSecret"]; //Secret Key


            //Build URL for VNPAY
            VnPayLibrary vnpay = new VnPayLibrary();
            var Price = (long)order.donGia * 100 * soLuong;
            vnpay.AddRequestData("vnp_Version", VnPayLibrary.VERSION);
            vnpay.AddRequestData("vnp_Command", "pay");
            vnpay.AddRequestData("vnp_TmnCode", vnp_TmnCode);
            vnpay.AddRequestData("vnp_Amount", (Price).ToString()); //Số tiền thanh toán. Số tiền không mang các ký tự phân tách thập phân, phần nghìn, ký tự tiền tệ. Để gửi số tiền thanh toán là 100,000 VND (một trăm nghìn VNĐ) thì merchant cần nhân thêm 100 lần (khử phần thập phân), sau đó gửi sang VNPAY là: 10000000
            if (TypePaymentVN == 1)
            {
                vnpay.AddRequestData("vnp_BankCode", "VNPAYQR");
            }
            else if (TypePaymentVN == 2)
            {
                vnpay.AddRequestData("vnp_BankCode", "VNBANK");
            }
            else if (TypePaymentVN == 3)
            {
                vnpay.AddRequestData("vnp_BankCode", "INTCARD");
            }

            var CreatedDate = DateTime.Now;
            vnpay.AddRequestData("vnp_CreateDate",CreatedDate.ToString("yyyyMMddHHmmss"));
            vnpay.AddRequestData("vnp_CurrCode", "VND");
            vnpay.AddRequestData("vnp_IpAddr", Utils.GetIpAddress());
            vnpay.AddRequestData("vnp_Locale", "vn");
            vnpay.AddRequestData("vnp_OrderInfo", "Thanh toan don hang::" + maPDP);
            vnpay.AddRequestData("vnp_OrderType", "other"); //default value: other

            vnpay.AddRequestData("vnp_ReturnUrl", vnp_Returnurl);
            vnpay.AddRequestData("vnp_TxnRef", maPDP); // Mã tham chiếu của giao dịch tại hệ thống của merchant. Mã này là duy nhất dùng để phân biệt các đơn hàng gửi sang VNPAY. Không được trùng lặp trong ngày

            //Add Params of 2.1.0 Version
            //Billing

            string paymentUrl = vnpay.CreateRequestUrl(vnp_Url, vnp_HashSecret);
            /*log.InfoFormat("VNPAY URL: {0}", paymentUrl);*/
            /*Response.Redirect(paymentUrl);*/

            return paymentUrl;
        }
    }
}