using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data.Entity;
using System.Data;

namespace LRC_NET_Framework.Controllers
{
    public class HomeController : Controller
    {
        //MemberMasterContext db = new MemberMasterContext();
        LRCEntities db = new LRCEntities();

        public ActionResult Index()
        {
            // getting all Member objects from DB

            //var members = db.tb_MemberMasters.Join(db.tb_Area, m => m.AreaID, a=> a.AreaID, (m, a) => new // результат
            //{
            //    MemberIDNumber = m.MemberIDNumber,
            //    FirstName = m.FirstName,
            //    LastName = m.LastName,
            //    DepartmentID = m.DepartmentID,
            //    AreaName = a.AreaName
            //});

            var members = db.tb_MemberMasters.Include(p => p.tb_Area);
            members = members.Include(p => p.tb_Department);

            // Filter: db.tb_MemberMasters.Where(x=>x.AreaID==1);
            // передаем все объекты в динамическое свойство Books в ViewBag
            //ViewBag.MemberMasterModel = members;
            //// getting all Member objects from DB
            //IEnumerable<MemberMasterModel> members = db.tb_MemberMaster;
            //// передаем все объекты в динамическое свойство Books в ViewBag
            //ViewBag.MemberMasterModel = members;
            // возвращаем представление
            return View(members.ToList());
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }


    }
}