using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using LRC_NET_Framework;

namespace LRC_NET_Framework.Controllers
{
    public class AssessmentController : Controller
    {
        private LRCEntities db = new LRCEntities();

        // GET: Assessment
        public ActionResult Index()
        {
            var tb_Assessment = db.tb_Assessment.Include(t => t.tb_AssessmentName).Include(t => t.tb_MemberMaster);
            return View(tb_Assessment.ToList());
        }

        // GET: Assessment/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            tb_Assessment tb_Assessment = db.tb_Assessment.Find(id);
            if (tb_Assessment == null)
            {
                return HttpNotFound();
            }
            return View(tb_Assessment);
        }

        // GET: Assessment/Create
        public ActionResult Create(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ViewBag.AssessmentNameID = new SelectList(db.tb_AssessmentName, "AssessmentNameID", "AssessmentName");
            ViewBag.MemberID = new SelectList(db.tb_MemberMaster, "MemberID", "LastName");
            return View();
        }

        // POST: Assessment/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "AssessmentID,MemberID,AssessmentNameID,AssessmentDesc,Value,AssessmentDate,AssesedBy,AddedBy,AddedDateTime,ModifiedBy,ModifiedDateTime")] tb_Assessment tb_Assessment)
        {
            if (ModelState.IsValid)
            {
                db.tb_Assessment.Add(tb_Assessment);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.AssessmentNameID = new SelectList(db.tb_AssessmentName, "AssessmentNameID", "AssessmentName", tb_Assessment.AssessmentNameID);
            ViewBag.MemberID = new SelectList(db.tb_MemberMaster, "MemberID", "LastName", tb_Assessment.MemberID);
            return View(tb_Assessment);
        }

        // GET: Assessment/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            tb_Assessment tb_Assessment = db.tb_Assessment.Find(id);
            if (tb_Assessment == null)
            {
                return HttpNotFound();
            }
            ViewBag.AssessmentNameID = new SelectList(db.tb_AssessmentName, "AssessmentNameID", "AssessmentName", tb_Assessment.AssessmentNameID);
            ViewBag.MemberID = new SelectList(db.tb_MemberMaster, "MemberID", "LastName", tb_Assessment.MemberID);
            return View(tb_Assessment);
        }

        // POST: Assessment/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "AssessmentID,MemberID,AssessmentNameID,AssessmentDesc,Value,AssessmentDate,AssesedBy,AddedBy,AddedDateTime,ModifiedBy,ModifiedDateTime")] tb_Assessment tb_Assessment)
        {
            if (ModelState.IsValid)
            {
                db.Entry(tb_Assessment).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.AssessmentNameID = new SelectList(db.tb_AssessmentName, "AssessmentNameID", "AssessmentName", tb_Assessment.AssessmentNameID);
            ViewBag.MemberID = new SelectList(db.tb_MemberMaster, "MemberID", "LastName", tb_Assessment.MemberID);
            return View(tb_Assessment);
        }

        // GET: Assessment/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            tb_Assessment tb_Assessment = db.tb_Assessment.Find(id);
            if (tb_Assessment == null)
            {
                return HttpNotFound();
            }
            return View(tb_Assessment);
        }

        // POST: Assessment/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            tb_Assessment tb_Assessment = db.tb_Assessment.Find(id);
            db.tb_Assessment.Remove(tb_Assessment);
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
