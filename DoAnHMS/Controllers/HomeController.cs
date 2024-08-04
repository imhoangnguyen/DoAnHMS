using DoAnHMS.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace DoAnHMS.Controllers
{
    public class HomeController : BaseController
    {
        DoAnHMSEntities db = new DoAnHMSEntities();
        List<CTPhieuThuePhong> cTPhieuThuePhong;
        List<DichVu> dichVu;
        Dictionary<string, DichVu> hm1;

        public class ChartData
        {
            public String label;
            public decimal value;
        }

        public class ChartDashBoard
        {
            public List<ChartData> data1 = new List<ChartData>();
            public List<ChartData> data2 = new List<ChartData>();
            public List<ChartData> data3 = new List<ChartData>();
            public List<ChartData> data4 = new List<ChartData>();
            public List<ChartData> data5 = new List<ChartData>();
        }

        public ActionResult Index()
        {
            List<string> maPDP = db.PhieuDatPhongs.Select(x => x.maPDP).ToList();
            List<string> maPDPinPTP = db.PhieuThuePhongs.Select(x => x.maPDP).ToList();
            for (int i = 0; i < maPDPinPTP.Count; i++)
            {
                string item = maPDPinPTP[i];
                if (maPDP.Contains(item))
                {
                    maPDP.Remove(item);
                }
            }
            //var phieuDatPhongs = db.PhieuDatPhongs
            var lienHe = db.LienHes.Where(x => x.tinhTrang == false).ToList();
            var phanHoi = db.PhanHois.Where(x => x.TinhTrang == false).ToList();
            ViewBag.PDP = maPDP.Count() == 0 ? "" : maPDP.Count().ToString();
            ViewBag.LienHe = lienHe.Count() == 0 ? "" : lienHe.Count().ToString();
            ViewBag.PhanHoi = phanHoi.Count() == 0 ? "" : phanHoi.Count().ToString();

            var chartDashBoard = new ChartDashBoard();

            string query = "SELECT [maPTP] ,[maP] ,[ngaySD] ,[maDV] ,[soLuong]   FROM [dbo].[CTPhieuThuePhong]";
            cTPhieuThuePhong = db.Database.SqlQuery<CTPhieuThuePhong>(query).ToList();


            query = "SELECT [maDV]       ,[tenDV]       ,[gia]   FROM [dbo].[DichVu]";
            dichVu = db.Database.SqlQuery<DichVu>(query).ToList();

            hm1 = new Dictionary<string, DichVu>();
            foreach (DichVu item in dichVu)
            {
                hm1[item.maDV] = item;
            }






            var hm = chart1();
            var hm2 = chart2();
            var hm3 = chart3();
            var hm4 = chart4();
            var hm5 = chart5();

            chartDashBoard.data1 = hm.Values.ToList().OrderBy(x => x.label).ToList();
            chartDashBoard.data2 = hm2.Values.ToList().OrderBy(x => x.label).ToList();
            chartDashBoard.data3 = hm3.Values.ToList().OrderBy(x => x.label).ToList();
            chartDashBoard.data4 = hm4.Values.ToList().OrderBy(x => x.label).ToList();
            chartDashBoard.data5 = hm5.Values.ToList().OrderBy(x => x.label).ToList();




            return View(chartDashBoard);
        }

        public Dictionary<String, ChartData> chart1()
        {
            var hm = new Dictionary<String, ChartData>();
            foreach (var item in cTPhieuThuePhong)
            {
                var k = $"{item.ngaySD.Month}/{item.ngaySD.Year}";
                if (hm.ContainsKey(k))
                {
                    var val = hm[k].value;
                    hm[k].value = val + hm1[item.maDV].gia * item.soLuong;
                }
                else
                {
                    hm[k] = new ChartData()
                    {
                        label = k,
                        value = hm1[item.maDV].gia * item.soLuong
                    };

                    var _1Month = item.ngaySD.AddMonths(1);
                    var k1 = $"{_1Month.Month}/{_1Month.Year}";

                    if (!hm.ContainsKey(k1))
                    {
                        hm[k1] = new ChartData()
                        {
                            label = k1,
                            value = 0
                        };
                    }


                    _1Month = item.ngaySD.AddMonths(-1);
                    k1 = $"{_1Month.Month}/{_1Month.Year}";

                    if (!hm.ContainsKey(k1))
                    {
                        hm[k1] = new ChartData()
                        {
                            label = k1,
                            value = 0
                        };
                    }

                }
            }

            return hm;
        }

        public Dictionary<String, ChartData> chart2()
        {

            var hm2 = new Dictionary<String, ChartData>();
            foreach (var item in cTPhieuThuePhong)
            {
                var k = $"{item.ngaySD.Year}";
                if (hm2.ContainsKey(k))
                {
                    var val = hm2[k].value;
                    hm2[k].value = val + hm1[item.maDV].gia * item.soLuong;
                }
                else
                {
                    hm2[k] = new ChartData()
                    {
                        label = k,
                        value = hm1[item.maDV].gia * item.soLuong
                    };

                    var _1 = item.ngaySD.AddYears(1);
                    var k1 = $"{_1.Year}";

                    if (!hm2.ContainsKey(k1))
                    {
                        hm2[k1] = new ChartData()
                        {
                            label = k1,
                            value = 0
                        };
                    }


                    _1 = item.ngaySD.AddYears(-1);
                    k1 = $"{_1.Year}";

                    if (!hm2.ContainsKey(k1))
                    {
                        hm2[k1] = new ChartData()
                        {
                            label = k1,
                            value = 0
                        };
                    }

                }
            }
            return hm2;
        }


        public Dictionary<String, ChartData> chart3()
        {

            var nv = db.NhanViens.ToList();


            var hm = new Dictionary<String, ChartData>();
            var x = 0;

            foreach (var item in cTPhieuThuePhong)
            {

                var k = $"{nv[x % nv.Count].maNV}:{nv[x % nv.Count].tenNV}";
                if (hm.ContainsKey(k))
                {
                    var _x = hm[k].value;
                    hm[k].value = _x + hm1[item.maDV].gia * item.soLuong;
                }
                else
                {
                    hm[k] = new ChartData() { label = k, value = 0 };
                }
                x++;
            }
            return hm;
        }


        public Dictionary<String, ChartData> chart4()
        {




            var hm = new Dictionary<String, ChartData>();


            foreach (var item in cTPhieuThuePhong)
            {
                var k = item.maP.Substring(0, 2);
                k = k.Replace("P", "Tầng ");

                if (hm.ContainsKey(k))
                {
                    var x = hm[k].value;
                    hm[k].value = x + hm1[item.maDV].gia * item.soLuong;
                }
                else
                {
                    hm[k] = new ChartData() { label = k, value = hm1[item.maDV].gia * item.soLuong };
                }
            }
            return hm;
        }




        public Dictionary<String, ChartData> chart5()
        {

            var _l = db.DichVus.ToList();
            var _hm = new Dictionary<String, DichVu>();

            foreach (var item in _l)
            {
                _hm[item.maDV] = item;
            }



            var hm = new Dictionary<String, ChartData>();


            foreach (var item in cTPhieuThuePhong)
            {
                var k = $"{item.maDV}:{_hm[item.maDV].tenDV}";

                if (hm.ContainsKey(k))
                {
                    var x = hm[k].value;
                    hm[k].value = x + hm1[item.maDV].gia * item.soLuong;
                }
                else
                {
                    hm[k] = new ChartData() { label = k, value = hm1[item.maDV].gia * item.soLuong };
                }

            }
            return hm;
        }
    }
}