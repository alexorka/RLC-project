using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using LRC_NET_Framework;
using PagedList;
using PagedList.Mvc;

namespace LRC_NET_Framework.Controllers
{
    public class SchoolListController : Controller
    {
        private LRCEntities db = new LRCEntities();

        // GET: SchoolList
        public ActionResult Index(string sortOrder, string searchString, int? page, int? CollegeID)
        {
            CollegeID = 1; // test
            var tb_SemesterTaught = db.tb_SemesterTaught.Include(t => t.tb_MemberMaster).
                Include(t => t.tb_Semesters).Include(t => t.tb_Class).Include(t => t.tb_Room);
            tb_SemesterTaught.Select(t => t.tb_MemberMaster.tb_Department.tb_College);
            tb_SemesterTaught.Select(t => t.tb_MemberMaster.tb_Department.tb_College);
            var tb_College = db.tb_College.Where(f => f.CollegeID == CollegeID).FirstOrDefault(); ;
            ViewBag.CollegeName = tb_College.CollegeDesc;

            //Sorting
            switch (sortOrder)
            {
                case "Name desc":
                    tb_SemesterTaught = tb_SemesterTaught.OrderByDescending(s => s.MemberID);
                    break;
                //case "Date":
                //    tb_MemberMasters = tb_MemberMasters.OrderBy(s => s.HireDate);
                //    break;
                //case "Date desc":
                //    tb_MemberMasters = tb_MemberMasters.OrderByDescending(s => s.HireDate);
                //    break;
                default:
                    tb_SemesterTaught = tb_SemesterTaught.OrderBy(s => s.MemberID);
                    break;
            }
            //Paging
            int pageSize = 3;
            int pageNumber = (page ?? 1);
            return View(tb_SemesterTaught.ToPagedList(pageNumber, pageSize));
        }

        // GET: SchoolList/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            tb_SemesterTaught tb_SemesterTaught = db.tb_SemesterTaught.Find(id);
            if (tb_SemesterTaught == null)
            {
                return HttpNotFound();
            }
            return View(tb_SemesterTaught);
        }

        // GET: SchoolList/Create
        public ActionResult Create()
        {
            ViewBag.MemberID = new SelectList(db.tb_MemberMaster, "MemberID", "LastName");
            ViewBag.SemesterRecID = new SelectList(db.tb_Semesters, "SemesterRecID", "FiscalYear");
            ViewBag.ClassID = new SelectList(db.tb_Class, "ClassID", "ClassName");
            ViewBag.RoomID = new SelectList(db.tb_Room, "RoomID", "RoomName");
            return View();
        }

        // POST: SchoolList/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "SemesterTaughtID,SemesterRecID,MemberID,RoomID,ClassID,ClassStart,ClassEnd,ClassWeekDayID")] tb_SemesterTaught tb_SemesterTaught)
        {
            if (ModelState.IsValid)
            {
                db.tb_SemesterTaught.Add(tb_SemesterTaught);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.MemberID = new SelectList(db.tb_MemberMaster, "MemberID", "LastName", tb_SemesterTaught.MemberID);
            ViewBag.SemesterRecID = new SelectList(db.tb_Semesters, "SemesterRecID", "FiscalYear", tb_SemesterTaught.SemesterRecID);
            ViewBag.ClassID = new SelectList(db.tb_Class, "ClassID", "ClassName", tb_SemesterTaught.ClassID);
            ViewBag.RoomID = new SelectList(db.tb_Room, "RoomID", "RoomName", tb_SemesterTaught.RoomID);
            return View(tb_SemesterTaught);
        }

        // GET: SchoolList/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            tb_SemesterTaught tb_SemesterTaught = db.tb_SemesterTaught.Find(id);
            if (tb_SemesterTaught == null)
            {
                return HttpNotFound();
            }
            ViewBag.MemberID = new SelectList(db.tb_MemberMaster, "MemberID", "LastName", tb_SemesterTaught.MemberID);
            ViewBag.SemesterRecID = new SelectList(db.tb_Semesters, "SemesterRecID", "FiscalYear", tb_SemesterTaught.SemesterRecID);
            ViewBag.ClassID = new SelectList(db.tb_Class, "ClassID", "ClassName", tb_SemesterTaught.ClassID);
            ViewBag.RoomID = new SelectList(db.tb_Room, "RoomID", "RoomName", tb_SemesterTaught.RoomID);
            return View(tb_SemesterTaught);
        }

        // POST: SchoolList/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "SemesterTaughtID,SemesterRecID,MemberID,RoomID,ClassID,ClassStart,ClassEnd,ClassWeekDayID")] tb_SemesterTaught tb_SemesterTaught)
        {
            if (ModelState.IsValid)
            {
                db.Entry(tb_SemesterTaught).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.MemberID = new SelectList(db.tb_MemberMaster, "MemberID", "LastName", tb_SemesterTaught.MemberID);
            ViewBag.SemesterRecID = new SelectList(db.tb_Semesters, "SemesterRecID", "FiscalYear", tb_SemesterTaught.SemesterRecID);
            ViewBag.ClassID = new SelectList(db.tb_Class, "ClassID", "ClassName", tb_SemesterTaught.ClassID);
            ViewBag.RoomID = new SelectList(db.tb_Room, "RoomID", "RoomName", tb_SemesterTaught.RoomID);
            return View(tb_SemesterTaught);
        }

        // GET: SchoolList/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            tb_SemesterTaught tb_SemesterTaught = db.tb_SemesterTaught.Find(id);
            if (tb_SemesterTaught == null)
            {
                return HttpNotFound();
            }
            return View(tb_SemesterTaught);
        }

        // POST: SchoolList/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            tb_SemesterTaught tb_SemesterTaught = db.tb_SemesterTaught.Find(id);
            db.tb_SemesterTaught.Remove(tb_SemesterTaught);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
