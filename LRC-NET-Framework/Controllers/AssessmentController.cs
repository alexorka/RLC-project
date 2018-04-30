using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using LRC_NET_Framework.Models;
using PagedList;
using PagedList.Mvc;

namespace LRC_NET_Framework.Controllers
{
    public class AssessmentController : Controller
    {
        private LRCEntities db = new LRCEntities();

        // GET: Assessment
        [Authorize(Roles = "admin, organizer")]
        public ActionResult Index(string sortOrder, string searchString, int? page)
        {
            var Assessments = db.tb_Assessment.Include(t => t.tb_AssessmentName).Include(t => t.tb_MemberMaster).Include(t => t.tb_AssessmentValue);

            ViewBag.NameSortParm = String.IsNullOrEmpty(sortOrder) ? "Date" : "";
            //Searching @ Filtering
            if (!String.IsNullOrEmpty(searchString))
            {
                Assessments = Assessments.Where(s => s.tb_MemberMaster.LastName.ToUpper().Contains(searchString.ToUpper())
                                       || s.tb_MemberMaster.FirstName.ToUpper().Contains(searchString.ToUpper()));
            }
            //Sorting
            switch (sortOrder)
            {
                case "Name desc":
                    //Activities = Activities.OrderByDescending(s => s.ActivityName);
                    break;
                case "Date":
                    Assessments = Assessments.OrderBy(s => s.AssessmentDate);
                    break;
                //case "Date desc":
                //    tb_MemberMasters = tb_MemberMasters.OrderByDescending(s => s.HireDate);
                //    break;
                default:
                    Assessments = Assessments.OrderBy(s => s.AssessmentDate);
                    break;
            }

            //Paging
            int pageSize = 3;
            int pageNumber = (page ?? 1);

            ViewData["MemberQty"] = Assessments.Count();

            return View(Assessments.ToPagedList(pageNumber, pageSize));
            //return View(Assessments.ToList());
        }

        //// GET: Assessment/Details/5
        //public ActionResult Details(int? id)
        //{
        //    if (id == null)
        //    {
        //        return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        //    }
        //    tb_Assessment tb_Assessment = db.tb_Assessment.Find(id);
        //    if (tb_Assessment == null)
        //    {
        //        return HttpNotFound();
        //    }
        //    return View(tb_Assessment);
        //}

        // GET: Assessment/AddPersonAssessmentActivity
        [Authorize(Roles = "admin, organizer")]
        public ActionResult AddPersonAssessmentActivity(int? id, int? CollegeID)
        {
            ViewBag.CollegeID = CollegeID;
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
                //Assessment.AssesedBy = 1;
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

            SelectList AspNetUsers = new SelectList(db.AspNetUsers, "Id", "Email");
            ViewBag.AssessedBy = AspNetUsers;

            List<tb_ActivityStatus> ActivityStatuses = new List<tb_ActivityStatus>();
            ActivityStatuses = db.tb_ActivityStatus.ToList();

            List<tb_MemberActivity> MemberActivities = new List<tb_MemberActivity>();
            MemberActivities = db.tb_MemberActivity.ToList();

            //List<AspNetUsers> AspNetUsers = new List<AspNetUsers>();
            //AspNetUsers = db.AspNetUsers.ToList();

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

        // POST: Assessment/AddPersonAssessmentActivity
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "admin, organizer")]
        public ActionResult AddPersonAssessmentActivity(int id, string submit, int AssessmentNameID, string AssessedBy, int ActivityID, int ActivityStatusID, int ValueID, AssessActivityModels AssessActivity, int? CollegeID)
        {
            ViewBag.CollegeID = CollegeID;
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
                        oldAssessment.AssesedBy = AssessedBy; //from ViewBag.AssessedBy
                        oldAssessment.AssessmentDate = AssessActivity._Assessment.AssessmentDate;
                        oldAssessment.AssessmentDesc = AssessActivity._Assessment.AssessmentDesc;
                        oldAssessment.AssessmentNameID = AssessmentNameID; //from ViewBag.AssessmentNameID
                        oldAssessment.ModifiedBy = AssessActivity._Assessment.ModifiedBy;
                        oldAssessment.ValueID = ValueID; // from ViewBag.ValueID
                        db.Entry(oldAssessment).State = EntityState.Modified;
                    }
                    db.SaveChanges();
                    //return RedirectToAction("Create");
                }

                else if (submit == "Assign") //Adding Person to Activity
                {
                    IQueryable<tb_MemberActivity> memberActivities = db.tb_MemberActivity;
                    memberActivities = memberActivities.Where(p => p.ActivityID == ActivityID).Where(p => p.MemberID == id); //id = AssessActivity._Assessment.MemberID (changed to id)
                    if (memberActivities.Count() == 0) //Action isnt assigned for this member
                    {
                        tb_MemberActivity memberActivity = new tb_MemberActivity();
                        //tb_MemberActivity = db.tb_MemberActivity.Where(f => f.MemberID == AssessActivity._Assessment.MemberID);
                        memberActivity.MemberID = id; //id = AssessActivity._Assessment.MemberID (changed to id);
                        memberActivity.ActivityID = ActivityID;
                        memberActivity.ActivityStatusID = ActivityStatusID;
                        if (ActivityStatusID == 1) // 1 - Committed
                            memberActivity.Membership = true;
                        else
                            memberActivity.Membership = false;
                        memberActivity.MembershipCommitment++;

                        db.tb_MemberActivity.Add(memberActivity);
                        db.SaveChanges();
                        //return RedirectToAction("Create");
                    }
                }
            }
            return RedirectToAction("AddPersonAssessmentActivity");
        }

        // GET: Assessment/Edit/5
        [Authorize(Roles = "admin, organizer")]
        public ActionResult AddAssessment(int? id)
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
            ViewBag.ValueID = new SelectList(db.tb_AssessmentValue, "ValueID", "ValueName", tb_Assessment.ValueID /* selected value */);
            //ViewBag.MemberID = new SelectList(db.tb_MemberMaster, "MemberID", "LastName", tb_Assessment.MemberID);
            return View(tb_Assessment);
        }

        // POST: Assessment/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "admin, organizer")]
        public ActionResult AddAssessment([Bind(Include = "AssessmentID,MemberID,AssessmentNameID,AssessmentDesc,ValueID,AssessmentDate,BackgroundStory,Fears,AttitudeTowardUnion,IDdLeaders,FollowUp")] tb_Assessment tb_Assessment)
        {
            if (ModelState.IsValid)
            {
                db.tb_Assessment.Add(tb_Assessment);
                db.SaveChanges();
                return RedirectToAction("Details", "Home", new { @id = tb_Assessment.MemberID });
                //return RedirectToAction("Index");
            }
            ViewBag.AssessmentNameID = new SelectList(db.tb_AssessmentName, "AssessmentNameID", "AssessmentName", tb_Assessment.AssessmentNameID);
            ViewBag.ValueID = new SelectList(db.tb_AssessmentValue, "ValueID", "ValueName", tb_Assessment.ValueID /* selected value */);
            ViewBag.MemberID = new SelectList(db.tb_MemberMaster, "MemberID", "LastName", tb_Assessment.MemberID);
            return View(tb_Assessment);
        }

        // GET: Assessment/NotSure
        [Authorize(Roles = "admin, organizer")]
        public ActionResult NotSure()
        {
            return PartialView("NotSure");
        }


        //// GET: Assessment/Delete/5
        //public ActionResult Delete(int? id)
        //{
        //    if (id == null)
        //    {
        //        return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        //    }
        //    tb_Assessment tb_Assessment = db.tb_Assessment.Find(id);
        //    if (tb_Assessment == null)
        //    {
        //        return HttpNotFound();
        //    }
        //    return View(tb_Assessment);
        //}

        //// POST: Assessment/Delete/5
        //[HttpPost, ActionName("Delete")]
        //[ValidateAntiForgeryToken]
        //public ActionResult DeleteConfirmed(int id)
        //{
        //    tb_Assessment tb_Assessment = db.tb_Assessment.Find(id);
        //    db.tb_Assessment.Remove(tb_Assessment);
        //    db.SaveChanges();
        //    return RedirectToAction("Index");
        //}

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
