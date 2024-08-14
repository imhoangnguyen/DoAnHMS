using DoAnHMS.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace DoAnHMS.Controllers
{
    public class ThongKeController : Controller
    {
        DoAnHMSEntities db = new DoAnHMSEntities();
        // GET: ThongKe
        public ActionResult ByDichVu(string fromDate = "", string toDate = "")
        
        {
            if(fromDate == "" || toDate == "")
            {
                fromDate = "2000-1-1";
                toDate = "2050-1-1";
            }
            string query = "select dv.maDV, dv.tenDV, sum(dv.gia * ctptp.soLuong)" +
                " from DichVu dv, CTPhieuThuePhong ctptp, PhieuThuePhong ptp, HoaDon hd" +
                " where not(dv.maDV = 'DV0000') and (dv.maDV = ctptp.maDV) and (ptp.maPTP = hd.maPTP) and (ptp.maPTP = ctptp.maPTP) and (ptp.ngayThue between '" +
                fromDate + "' and '" + toDate + "') and (ptp.ngayTra between '" +
                fromDate + "' and '" + toDate + "')" +
                " group by dv.maDV, dv.tenDV";

            ViewBag.fromDate = fromDate;
            ViewBag.toDate = toDate;
            var data = db.Database.SqlQuery<DichVu>(query).ToList();

            return View(data);
        }

        public class ChartData
        {
            public string maP;
            public HashSet<String> listDv;
            public int dem;
            public decimal price;
        }
        public ActionResult Index()
        {
            string query = "SELECT [maPTP] ,[maP] ,[ngaySD] ,[maDV] ,[soLuong]   FROM [dbo].[CTPhieuThuePhong]";
            var cTPhieuThuePhong = db.Database.SqlQuery<CTPhieuThuePhong>(query).ToList();

            query = "SELECT [maDV]       ,[tenDV]       ,[gia]   FROM [dbo].[DichVu]";
            var dichVu = db.Database.SqlQuery<DichVu>(query).ToList();

            Dictionary<string, DichVu> hm = new Dictionary<string, DichVu>();
            foreach(DichVu item in dichVu)
            {
                hm[item.maDV] = item;
            }

            Dictionary<string, ChartData> hm1 = new Dictionary<string, ChartData>();

            foreach (CTPhieuThuePhong item in cTPhieuThuePhong)
            {
                if(hm1.ContainsKey(item.maP))
                {
                    hm1[item.maP].price += hm[item.maDV].gia;
                }
                else
                {
                    ChartData data = new ChartData()
                    {
                        maP = item.maP,
                        price = hm[item.maDV].gia * item.soLuong
                    };
                    hm1[item.maP] = data;
                }
            }

            return View(hm1.Values.ToList());
        }




        public class Person
        {
            public string Name { get; set; }
            public int Age { get; set; }
        }


        public ActionResult ExportToCsv()
        {
            string query = "SELECT [maPTP] ,[maP] ,[ngaySD] ,[maDV] ,[soLuong]   FROM [dbo].[CTPhieuThuePhong]";
            var cTPhieuThuePhong = db.Database.SqlQuery<CTPhieuThuePhong>(query).ToList();

            query = "SELECT [maDV]       ,[tenDV]       ,[gia]   FROM [dbo].[DichVu]";
            var dichVu = db.Database.SqlQuery<DichVu>(query).ToList();

            Dictionary<string, DichVu> hm = new Dictionary<string, DichVu>();
            foreach (DichVu item in dichVu)
            {
                hm[item.maDV] = item;
            }

            Dictionary<string, ChartData> hm1 = new Dictionary<string, ChartData>();

            foreach (CTPhieuThuePhong item in cTPhieuThuePhong)
            {
                if (hm1.ContainsKey(item.maP))
                {
                    hm1[item.maP].price += hm[item.maDV].gia;
                    hm1[item.maP].listDv.Add($"{item.maDV}:{item.soLuong}");
                    hm1[item.maP].dem = hm1[item.maP].listDv.Count;
                }
                else
                {
                    HashSet<string> list = new HashSet<string>();
                    list.Add($"{item.maDV}:{item.soLuong}");
                    ChartData data = new ChartData()
                    {
                        maP = item.maP,
                        listDv = list,
                        dem = 1,
                        price = hm[item.maDV].gia * item.soLuong

                    };
                    hm1[item.maP] = data;
                }
            }
            var csvContent = new StringBuilder();
            csvContent.AppendLine("Ma phong,Danh sach dich vu,So dich vu,Tong tien dich vu");
            foreach (var item in hm1.Values)
            {
                var x = new StringBuilder();
                foreach (var item1 in item.listDv)
                {
                    x.Append($"{item1} ");
                }

                csvContent.AppendLine($"{item.maP},{x.ToString().TrimEnd()},{item.dem},{item.price}");
            }

            // Set response headers
            Response.Clear();
            Response.Buffer = true;
            Response.AddHeader("content-disposition", "attachment;filename=ExportedData.csv");
            Response.Charset = "";
            Response.ContentType = "text/csv";

            // Write CSV content to the response stream
            using (var sw = new StringWriter())
            {
                sw.Write(csvContent.ToString());
                Response.Output.Write(sw.ToString());
            }

            Response.Flush();
            Response.End();

            return View(); // Optional: You can return a view or redirect if needed
        }
    }

    
}