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
            if (tb_Assessment.Count() == 0) //1st Assessment
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
                Assessment.ValueID = 1;
            }
            else
                Assessment = tb_Assessment.FirstOrDefault();

            SelectList FeePayerAssess = new SelectList(db.tb_AssessmentName, "AssessmentNameID", "AssessmentName", Assessment.AssessmentNameID /* selected value */);
            ViewBag.AssessmentNameID = FeePayerAssess;

            SelectList AssessmentValue = new SelectList(db.tb_AssessmentValue, "ValueID", "ValueName", Assessment.ValueID /* selected value */);
            ViewBag.ValueID = AssessmentValue;

            SelectList Activities = new SelectList(db.tb_Activity, "ActivityID", "ActivityName");
            ViewBag.ActivityID = Activities;

            List<tb_ActivityStatus> ActivityStatuses = new List<tb_ActivityStatus>();
            ActivityStatuses = db.tb_ActivityStatus.ToList();

            List<tb_MemberActivity> MemberActivities = new List<tb_MemberActivity>();
            MemberActivities = db.tb_MemberActivity.ToList();

            var model = new AssessActivityModels()
            {
                _Assessment = Assessment,
                _Activity = db.tb_Activity.FirstOrDefault(),
                _ActivityStatus = ActivityStatuses,
                _MemberActivity = MemberActivities
            };

            return View(model);
            //return View(Assessment);
        }

        // POST: Assessment/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(string submit, int AssessmentNameID, int ActivityID, int ActivityStatusID, int ValueID, AssessActivityModels AssessActivity)
        {
            if (ModelState.IsValid)
            {
                if (submit == "Submit") //Adding Assessment
                {
                    AssessActivity._Assessment.AddedDateTime = DateTime.Now;
                    tb_Assessment oldAssessment = db.tb_Assessment.Find(AssessActivity._Assessment.MemberID);
                    var Assessments = db.tb_Assessment;
                    if (oldAssessment == null)
                    {
                        db.tb_Assessment.Add(AssessActivity._Assessment);
                    }
                    else
                    {
                        oldAssessment.AssessmentID = Assessments.FirstOrDefault().AssessmentID;
                        oldAssessment.AddedBy = AssessActivity._Assessment.AddedBy;
                        oldAssessment.AddedDateTime = AssessActivity._Assessment.AddedDateTime;
                        oldAssessment.AssesedBy = AssessActivity._Assessment.AssesedBy;
                        oldAssessment.AssessmentDate = AssessActivity._Assessment.AssessmentDate;
                        oldAssessment.AssessmentDesc = AssessActivity._Assessment.AssessmentDesc;
                        oldAssessment.AssessmentNameID = AssessmentNameID; //from ViewBag.AssessmentNameID
                        oldAssessment.ModifiedBy = AssessActivity._Assessment.ModifiedBy;
                        oldAssessment.AddedBy = AssessActivity._Assessment.AddedBy;
                        oldAssessment.ValueID = ValueID; // from ViewBag.ValueID
                        db.Entry(oldAssessment).State = EntityState.Modified;
                    }
                    db.SaveChanges();
                    //return RedirectToAction("Create");
                }

                else if (submit == "Assign") //Adding Person to Activity
                {
                    IQueryable<tb_MemberActivity> memberActivities = db.tb_MemberActivity;
                    memberActivities = memberActivities.Where(p => p.ActivityID == ActivityID).Where(p => p.MemberID == AssessActivity._Assessment.MemberID);
                    if (memberActivities.Count() == 0) //Action isnt assigned for this member
                    {
                        tb_MemberActivity memberActivity = new tb_MemberActivity();
                        //tb_MemberActivity = db.tb_MemberActivity.Where(f => f.MemberID == AssessActivity._Assessment.MemberID);
                        memberActivity.MemberID = AssessActivity._Assessment.MemberID;
                        memberActivity.ActivityID = ActivityID;
                        memberActivity.ActivityStatusID = ActivityStatusID;
                        if (ActivityStatusID == 1) // 1 - Committed
                            memberActivity.Membership = true;
                        else
                            memberActivity.Membership = true;
                        memberActivity.MembershipCommitment++;

                        db.tb_MemberActivity.Add(memberActivity);
                        db.SaveChanges();
                        //return RedirectToAction("Create");
                    }
                }
            }
            return RedirectToAction("Create");
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
