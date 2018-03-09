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
            return RedirectToAction("Index", "tb_MemberMaster");
            // getting all Member objects from DB
            //var members = db.tb_MemberMasters.Include(p => p.tb_Area).Include(p => p.tb_Department);

            //return View(members.ToList());
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

        public ActionResult MemberDetails(int id)
        {
            var members = db.tb_MemberMasters.Include(p => p.tb_Area).Include(p => p.tb_Department);
            tb_MemberMaster c = members.FirstOrDefault(com => com.MemberID == id);
            if (c != null)
                return PartialView(c);
            return HttpNotFound();
        }


    }
}