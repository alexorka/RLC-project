using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using LRC_NET_Framework.Models;

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
            //var Assessments = new tb_Assessment();
            //var Activities = new tb_Activity();
            //var modelCollection = new AssessActivityModels();
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var tb_Assessment = db.tb_Assessment.Include(t => t.tb_MemberMaster);
            tb_Assessment = tb_Assessment.Where(f => f.tb_MemberMaster.MemberID == id);
            tb_Assessment Assessment = new tb_Assessment();
            if (tb_Assessment.Count() == 0)
            {
                Assessment.AddedBy = 1;
                Assessment.AddedDateTime = DateTime.Now;
                Assessment.AssesedBy = 1;
                Assessment.AssessmentDate = DateTime.Now;
                Assessment.AssessmentDesc = String.Empty;
                Assessment.AssessmentNameID = 7; // Add Fee Payer Assess
                Assessment.MemberID = id ?? default(int);
                Assessment.ModifiedBy = 1;
                Assessment.ModifiedDateTime = DateTime.Now;
                Assessment.Value = 0;
            }
            else
                Assessment = tb_Assessment.FirstOrDefault();

            SelectList FeePayerAssess = new SelectList(db.tb_AssessmentName, "AssessmentNameID", "AssessmentName", 0 /*0 - default FeePayerAssess Id (-Fee Payer Assess-)*/);
            ViewBag.AssessmentNameID = FeePayerAssess;

            SelectList Activities = new SelectList(db.tb_Activity, "ActivityID", "ActivityNote");
            ViewBag.ActivityID = Activities;

            //SelectList ActivityStatuses = new SelectList(db.tb_ActivityStatus, "ActivityStatusID", "ActivityStatusName");
            //ViewBag.ActivityStatusID = ActivityStatuses;

            var model = new AssessActivityModels()
            {
                _Assessment = Assessment,
                _Activity = db.tb_Activity.FirstOrDefault()
            };

            return View(model);
            //return View(Assessment);
        }

        // POST: Assessment/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(string submit, int AssessmentNameID, AssessActivityModels tb_Assessment)
            //[Bind(Include = "_Assessment.AssessmentID,_Assessment.MemberID,_Assessment.AssessmentNameID," +
            //"_Assessment.AssessmentDesc,_Assessment.Value,_Assessment.AssessmentDate,_Assessment.AssesedBy,_Assessment.AddedBy," +
            //"_Assessment.AddedDateTime,_Assessment.ModifiedBy,_Assessment.ModifiedDateTime")] AssessActivityModels tb_Assessment)
                                   //[Bind(Include = "ActivityID,ActivityStatusID,ActivityDate,ActivityNote")] AssessActivityModels tb_Activity)
        {
            if (submit == "Submit Assessment")
            { 
            tb_Assessment._Assessment.AddedDateTime = DateTime.Now;
            tb_Assessment oldAssessment = db.tb_Assessment.Find(tb_Assessment._Assessment.MemberID);
                if (ModelState.IsValid)
                {
                    var Assessments = db.tb_Assessment;
                    if (oldAssessment == null)
                    {
                        db.tb_Assessment.Add(tb_Assessment._Assessment);
                    }
                    else
                    {
                        oldAssessment.AssessmentID = Assessments.FirstOrDefault().AssessmentID;
                        oldAssessment.AddedBy = tb_Assessment._Assessment.AddedBy;
                        oldAssessment.AddedDateTime = tb_Assessment._Assessment.AddedDateTime;
                        oldAssessment.AssesedBy = tb_Assessment._Assessment.AssesedBy;
                        oldAssessment.AssessmentDate = tb_Assessment._Assessment.AssessmentDate;
                        oldAssessment.AssessmentDesc = tb_Assessment._Assessment.AssessmentDesc;
                        oldAssessment.AssessmentNameID = AssessmentNameID; //from ViewBag.AssessmentNameID
                        oldAssessment.ModifiedBy = tb_Assessment._Assessment.ModifiedBy;
                        oldAssessment.AddedBy = tb_Assessment._Assessment.AddedBy;
                        oldAssessment.Value = tb_Assessment._Assessment.Value;
                        db.Entry(oldAssessment).State = EntityState.Modified;
                    }
                    db.SaveChanges();
                    return RedirectToAction("Index");
                }
            }
            else if (submit == "Submit Activity")
            {

            }

            //ViewBag.AssessmentNameID = new SelectList(db.tb_AssessmentName, "AssessmentNameID", "AssessmentName", tb_Assessment.AssessmentNameID);
            //ViewBag.MemberID = new SelectList(db.tb_MemberMaster, "MemberID", "LastName", tb_Assessment.MemberID);
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
