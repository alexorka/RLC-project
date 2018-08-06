using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Validation;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using LRC_NET_Framework.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
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
            var uName = HttpContext.GetOwinContext().Authentication.User.Identity.GetUserName();

            if (tb_Assessment.Count() == 0) //1st Assessment
            {
                Assessment.AddedBy = uName;
                Assessment.AddedDateTime = DateTime.Now;
                Assessment.AssessedBy = uName;
                Assessment.AssessmentDate = DateTime.Now;
                Assessment.AssessmentDesc = String.Empty;
                Assessment.AssessmentNameID = 7; // Add Fee Payer Assess
                Assessment.MemberID = id ?? default(int);
                Assessment.ModifiedBy = uName;
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
        public ActionResult AddPersonAssessmentActivity(int? id, string submit, int AssessmentNameID, string AssessedBy, int ActivityID, int ActivityStatusID, int ValueID, AssessActivityModels model, int? CollegeID)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Error error = new Error();
            error.errCode = ErrorDetail.Success;
            List<string> errs = new List<string>();
            string uName = HttpContext.GetOwinContext().Authentication.User.Identity.GetUserName();

            ViewBag.CollegeID = CollegeID;

            SelectList FeePayerAssess = new SelectList(db.tb_AssessmentName, "AssessmentNameID", "AssessmentName", AssessmentNameID /* selected value */);
            ViewBag.AssessmentNameID = FeePayerAssess;

            SelectList AssessmentValue = new SelectList(db.tb_AssessmentValue, "ValueID", "ValueName", ValueID /* selected value */);
            ViewBag.ValueID = AssessmentValue;

            SelectList Activities = new SelectList(db.tb_Activity, "ActivityID", "ActivityName");
            ViewBag.ActivityID = Activities;

            SelectList AspNetUsers = new SelectList(db.AspNetUsers, "Id", "Email");
            ViewBag.AssessedBy = AspNetUsers;

            List<tb_ActivityStatus> ActivityStatuses = new List<tb_ActivityStatus>();
            ActivityStatuses = db.tb_ActivityStatus.ToList();

            List<tb_MemberActivity> MemberActivities = new List<tb_MemberActivity>();
            MemberActivities = db.tb_MemberActivity.ToList();

            model._Assessment.ValueID = ValueID;
            model._Assessment.AssessmentNameID = AssessmentNameID;
            model._Assessment.AssessedBy = AssessedBy;
            model._Assessment.AddedDateTime = DateTime.Now;
            model._ActivityStatus = ActivityStatuses;
            model._MemberActivity = MemberActivities;

            if (submit == "Submit") //Adding Assessment
            {
                if (ModelState.IsValid)
                {
                    try
                    {
                        tb_Assessment oldAssessment = db.tb_Assessment.Find(model._Assessment.MemberID);
                        var Assessments = db.tb_Assessment;
                        if (oldAssessment == null)
                        {
                            model._Assessment.AddedBy = uName;
                            db.tb_Assessment.Add(model._Assessment);
                        }
                        else
                        {
                            oldAssessment.AssessmentID = Assessments.FirstOrDefault().AssessmentID;
                            oldAssessment.AddedBy = uName;
                            oldAssessment.AddedDateTime = model._Assessment.AddedDateTime;
                            var uEmail = db.AspNetUsers.Find(AssessedBy).Email;
                            oldAssessment.AssessedBy = uEmail; //from ViewBag.AssessedBy
                            oldAssessment.AssessmentDate = model._Assessment.AssessmentDate;
                            oldAssessment.AssessmentNameID = AssessmentNameID; //from ViewBag.AssessmentNameID
                            oldAssessment.ModifiedBy = model._Assessment.ModifiedBy;
                            oldAssessment.ValueID = ValueID; // from ViewBag.ValueID
                            oldAssessment.AssessmentDesc = model._Assessment.AssessmentDesc;
                            db.Entry(oldAssessment).State = EntityState.Modified;

                            model._Assessment = oldAssessment;
                        }
                        db.SaveChanges();
                    }
                    catch (DbEntityValidationException ex)
                    {
                        error.errCode = ErrorDetail.DataImportError;
                        error.errMsg = ErrorDetail.GetMsg(error.errCode);
                        foreach (DbEntityValidationResult validationError in ex.EntityValidationErrors)
                        {
                            error.errMsg += ". Object: " + validationError.Entry.Entity.ToString();
                            foreach (DbValidationError err in validationError.ValidationErrors)
                            {
                                error.errMsg += ". " + err.ErrorMessage;
                            }
                        }
                        errs.Add("Error #" + error.errCode.ToString() + "!" + error.errMsg);
                        ViewData["AssessmentErrorList"] = errs;
                        return View(model);
                    }

                    return View(model);
                    //return RedirectToAction("Create");
                }
                else
                {
                    error.errCode = ErrorDetail.DataImportError;
                    error.errMsg = ErrorDetail.GetMsg(error.errCode);
                    foreach (var state in ModelState)
                    {
                        foreach (var err in state.Value.Errors)
                        {
                            error.errMsg += ". " + err.ErrorMessage;
                        }
                    }
                    errs.Add("Error #" + error.errCode.ToString() + "!" + error.errMsg);
                    ViewData["AssessmentErrorList"] = errs;
                    return View(model);
                }
            }

            else if (submit == "Assign") //Adding Person to Activity
            {
                if (ModelState.IsValid)
                {
                    try
                    {
                        IQueryable<tb_MemberActivity> memberActivities = db.tb_MemberActivity;
                        memberActivities = memberActivities.Where(p => p.ActivityID == ActivityID).Where(p => p.MemberID == id); //id = AssessActivity._Assessment.MemberID (changed to id)
                        if (memberActivities.Count() == 0) //Action isnt assigned for this member
                        {
                            tb_Activity activity = db.tb_Activity.Find(ActivityID);
                            activity.ActivityDate = model._Activity.ActivityDate;
                            activity.ActivityNote = model._Activity.ActivityNote;
                            activity.ModifiedDateTime = DateTime.UtcNow;
                            activity.ModifiedBy = uName;

                            tb_MemberActivity memberActivity = new tb_MemberActivity();
                            memberActivity.MemberID = id ?? default(int); //id = AssessActivity._Assessment.MemberID (changed to id);
                            memberActivity.ActivityID = ActivityID;
                            memberActivity.ActivityStatusID = ActivityStatusID;
                            if (ActivityStatusID == 1) // 1 - Committed
                                memberActivity.Membership = true;
                            else
                                memberActivity.Membership = false;
                            memberActivity.MembershipCommitment++;

                            db.tb_MemberActivity.Add(memberActivity);
                            db.SaveChanges();

                            model._MemberActivity = db.tb_MemberActivity.ToList();
                        }
                    }
                    catch (DbEntityValidationException ex)
                    {
                        error.errCode = ErrorDetail.DataImportError;
                        error.errMsg = ErrorDetail.GetMsg(error.errCode);
                        foreach (DbEntityValidationResult validationError in ex.EntityValidationErrors)
                        {
                            error.errMsg += ". Object: " + validationError.Entry.Entity.ToString();
                            foreach (DbValidationError err in validationError.ValidationErrors)
                            {
                                error.errMsg += ". " + err.ErrorMessage;
                            }
                        }
                        errs.Add("Error #" + error.errCode.ToString() + "!" + error.errMsg);
                        ViewData["ActivityErrorList"] = errs;
                        return View(model);
                    }
                }
            }
            else
            {
                error.errCode = ErrorDetail.DataImportError;
                error.errMsg = ErrorDetail.GetMsg(error.errCode);
                foreach (var state in ModelState)
                {
                    foreach (var err in state.Value.Errors)
                    {
                        error.errMsg += ". " + err.ErrorMessage;
                    }
                }
                errs.Add("Error #" + error.errCode.ToString() + "!" + error.errMsg);
                ViewData["ActivityErrorList"] = errs;
                return View(model);
            }

            return View(model);
            //return RedirectToAction("AddPersonAssessmentActivity");
        }

        // GET: Assessment/Edit/5
        [Authorize(Roles = "admin, organizer")]
        public ActionResult AddAssessment(int? id)
        {
            if (id == null)
                return RedirectToAction("Index", "Errors", new { name = "Bad Request", description = "Member ID as parameter cannot be NULL" });

            List<string> errs = new List<string>();
            tb_Assessment MA = new tb_Assessment { MemberID = id ?? 0, AssessmentDate = DateTime.UtcNow };
            ViewBag.AssessmentNameID = new SelectList(db.tb_AssessmentName, "AssessmentNameID", "AssessmentName"/*, tb_Assessment.AssessmentNameID*/);
            ViewBag.ValueID = new SelectList(db.tb_AssessmentValue, "ValueID", "ValueName"/*, tb_Assessment.ValueID*/ /* selected value */);

            errs.Add("Empty");
            ViewData["ErrorList"] = errs;

            return View(MA);
        }

        // POST: Assessment/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "admin, organizer")]
        public ActionResult AddAssessment([Bind(Include = "AssessmentID,MemberID,AssessmentNameID,AssessmentDesc,ValueID,AssessmentDate,AssessedBy, BackgroundStory,Fears,AttitudeTowardUnion,IDdLeaders,FollowUp")] tb_Assessment model)
        {
            Error error = new Error();
            error.errCode = ErrorDetail.Success;
            List<string> errs = new List<string>();

            ViewBag.AssessmentNameID = new SelectList(db.tb_AssessmentName, "AssessmentNameID", "AssessmentName", model.AssessmentNameID);
            ViewBag.ValueID = new SelectList(db.tb_AssessmentValue, "ValueID", "ValueName", model.ValueID /* selected value */);
            ViewBag.MemberID = new SelectList(db.tb_MemberMaster, "MemberID", "LastName", model.MemberID);

            if (ModelState.IsValid)
            {
                try
                {
                    var uName = HttpContext.GetOwinContext().Authentication.User.Identity.GetUserName();
                    model.AddedBy = uName;
                    model.AddedDateTime = DateTime.UtcNow;
                    db.tb_Assessment.Add(model);
                    db.SaveChanges();
                }
                catch (DbEntityValidationException ex)
                {
                    error.errCode = ErrorDetail.DataImportError;
                    error.errMsg = ErrorDetail.GetMsg(error.errCode);
                    foreach (DbEntityValidationResult validationError in ex.EntityValidationErrors)
                    {
                        error.errMsg += ". Object: " + validationError.Entry.Entity.ToString();
                        foreach (DbValidationError err in validationError.ValidationErrors)
                        {
                            error.errMsg += ". " + err.ErrorMessage;
                        }
                    }
                    errs.Add("Error #" + error.errCode.ToString() + "!" + error.errMsg);
                    ViewData["ErrorList"] = errs;
                    return View(model);
                }
                return RedirectToAction("Details", "Home", new { @id = model.MemberID });
            }
            else
            {
                error.errCode = ErrorDetail.DataImportError;
                error.errMsg = ErrorDetail.GetMsg(error.errCode);
                foreach (var state in ModelState)
                {
                    foreach (var err in state.Value.Errors)
                    {
                        error.errMsg += ". " + err.ErrorMessage;
                    }
                }
                errs.Add("Error #" + error.errCode.ToString() + "!" + error.errMsg);
                ViewData["ErrorList"] = errs;
                return View(model);
            }
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
