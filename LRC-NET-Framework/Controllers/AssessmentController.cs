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
                    Assessments = Assessments.OrderByDescending(s => s.AssessmentDate);
                    break;
            }

            //Paging
            int pageSize = 15;
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
        public ActionResult AddPersonAssessmentActivity(int? id, int CollegeID)
        {
            //ViewBag.CollegeID = CollegeID;
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

            SelectList FeePayerAssess = new SelectList(db.tb_AssessmentName.OrderBy(s => s.AssessmentName), "AssessmentNameID", "AssessmentName", Assessment.AssessmentNameID /* selected value */);
            ViewBag.AssessmentNameID = FeePayerAssess;

            SelectList AssessmentValue = new SelectList(db.tb_AssessmentValue, "ValueID", "ValueName", Assessment.ValueID /* selected value */);
            ViewBag.ValueID = AssessmentValue;

            SelectList AspNetUsers = new SelectList(db.AspNetUsers.OrderBy(s => s.LastFirstName), "Id", "LastFirstName");
            ViewBag.AssessedBy = AspNetUsers;

            List<tb_ActivityStatus> ActivityStatuses = new List<tb_ActivityStatus>();

            ViewBag.ActivityStatus = db.tb_ActivityStatus.ToList();
            ViewBag.ActivityStatusAfter = new List<SelectListItem>
                {
                    new SelectListItem { Value = "0", Text = "Participated", Selected = false},
                    new SelectListItem { Value = "1", Text = "Not Participated", Selected = false}
                };

            SelectList Activities = new SelectList(db.tb_Activity.OrderBy(s => s.ActivityName), "ActivityID", "ActivityName");
            SelectListItem selListItem = new SelectListItem() { Value = "0", Text = " Select Activity " };
            ViewBag.ActivityID = CommonFunctions.AddFirstItem(Activities, selListItem);

            List<tb_MemberActivity> MemberActivities = new List<tb_MemberActivity>();
            MemberActivities = db.tb_MemberActivity.Where(a => a.MemberID == id).OrderBy(s => s.ActivityID).ToList();

            tb_MemberMaster fm = db.tb_MemberMaster.Find(id);
            ViewBag.MemberName = fm.FirstName + " " + fm.LastName;

            var model = new AssessActivityModels()
            {
                _Assessment = Assessment,
                _Activity = db.tb_Activity.FirstOrDefault(),
                _ActivityStatus = ActivityStatuses,
                _MemberActivity = MemberActivities,
                _MemberAssessments = tb_Assessment.ToList(),
                _CollegeID = CollegeID
            };

            return View(model);
        }

        // POST: Assessment/AddPersonAssessmentActivity
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "admin, organizer")]
        public ActionResult AddPersonAssessmentActivity(int id, string submit, int AssessmentNameID, string AssessedBy, int ActivityID, 
            int ActivityStatusID, int ValueID, AssessActivityModels model, FormCollection formCollection)
        {
            //if (id == null)
            //{
            //    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            //}
            Error error = new Error();
            error.errCode = ErrorDetail.Success;
            List<string> errs = new List<string>();
            string userId = HttpContext.GetOwinContext().Authentication.User.Identity.GetUserId();
            //string uLastFirstName = db.AspNetUsers.Where(s => s.Email == uName).FirstOrDefault().LastFirstName;

            //ViewBag.CollegeID = CollegeID;

            #region Prepare Model (ASSESSMENT Part)

            SelectList FeePayerAssess = new SelectList(db.tb_AssessmentName.OrderBy(s => s.AssessmentName), "AssessmentNameID", "AssessmentName", AssessmentNameID /* selected value */);
            ViewBag.AssessmentNameID = FeePayerAssess;

            SelectList AssessmentValue = new SelectList(db.tb_AssessmentValue, "ValueID", "ValueName", ValueID /* selected value */);
            ViewBag.ValueID = AssessmentValue;

            SelectList AspNetUsers = new SelectList(db.AspNetUsers.OrderBy(s => s.LastFirstName), "Id", "LastFirstName");
            ViewBag.AssessedBy = AspNetUsers;

            model._Assessment.ValueID = ValueID;
            model._Assessment.AssessmentNameID = AssessmentNameID;
            model._Assessment.AssessedBy = AssessedBy;
            model._Assessment.AddedDateTime = DateTime.Now;
            model._MemberAssessments = db.tb_Assessment.Where(f => f.tb_MemberMaster.MemberID == id).ToList();

            #endregion

            #region Prepare Model (ACTIVITY Part)

            model._ActivityStatus = db.tb_ActivityStatus.ToList();

            ViewBag.ActivityStatusAfter = new List<SelectListItem>
            {
                new SelectListItem { Value = "0", Text = "Participated", Selected = false},
                new SelectListItem { Value = "1", Text = "Not Participated", Selected = false}
            };

            SelectList Activities = new SelectList(db.tb_Activity.OrderBy(s => s.ActivityName), "ActivityID", "ActivityName");
            SelectListItem selListItem = new SelectListItem() { Value = "0", Text = " Select Activity " };
            ViewBag.ActivityID = CommonFunctions.AddFirstItem(Activities, selListItem);

            model._MemberActivity = db.tb_MemberActivity.Where(s => s.MemberID == id).ToList();

            tb_MemberMaster fm = db.tb_MemberMaster.Find(id);
            ViewBag.MemberName = fm.FirstName + " " + fm.LastName;

            #endregion

            if (submit == "Submit") //Adding Assessment
            {
                if (ModelState.IsValid)
                {
                    try
                    {
                        model._Assessment.AddedBy = userId;
                        db.tb_Assessment.Add(model._Assessment);
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

                    model._MemberAssessments = db.tb_Assessment.Where(f => f.tb_MemberMaster.MemberID == id).ToList();
                    return View(model);
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

                    bool? isParticipated = null;
                    if (formCollection["afterTheFact"] == "0")
                        isParticipated = true;
                    else if (formCollection["afterTheFact"] == "1")
                        isParticipated = false;

                    ActivityByMemberModels activityCRUD = new ActivityByMemberModels();
                    errs = activityCRUD.AddEditActivity(id, ActivityID, ActivityStatusID, isParticipated, model._Activity.ActivityDate, model._Activity.ActivityNote, userId);

                    if (errs == null || errs.Count == 0)
                        return RedirectToAction("MembersBySchool", "Home", new { @CollegeID = model._CollegeID });
                    else
                    {
                        ViewData["ErrorList"] = errs;

                        ActivityByMemberModels memberActivities = new ActivityByMemberModels();
                        memberActivities.MemberCollection = memberActivities.GetFullListOfMembers();

                        return View(memberActivities);
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
                    ViewData["ErrorList"] = errs;

                    ActivityByMemberModels memberActivities = new ActivityByMemberModels();
                    memberActivities.MemberCollection = memberActivities.GetFullListOfMembers();

                    return View(memberActivities);
                }                //if (ModelState.IsValid)
                //{
                //    try
                //    {
                //        tb_Activity activity = db.tb_Activity.Find(ActivityID);
                //        activity.ActivityDate = model._Activity.ActivityDate;
                //        activity.ActivityNote = model._Activity.ActivityNote;
                //        activity.ModifiedDateTime = DateTime.UtcNow;
                //        activity.ModifiedBy = userId;

                //        tb_MemberActivity oldMemberActivity = db.tb_MemberActivity.Where(p => p.ActivityID == ActivityID && p.MemberID == id).FirstOrDefault(); //id = AssessActivity._Assessment.MemberID (changed to id)
                //        if (oldMemberActivity == null) //Action isnt assigned for this member
                //        {
                //            tb_MemberActivity memberActivity = new tb_MemberActivity();
                //            memberActivity.MemberID = id ?? default(int); //id = AssessActivity._Assessment.MemberID (changed to id);
                //            memberActivity.ActivityID = ActivityID;
                //            memberActivity.ActivityStatusID = ActivityStatusID;
                //            if (ActivityStatusID == 1) // 1 - Committed
                //            {
                //                memberActivity.Membership = true;
                //                memberActivity.MembershipCommitment++;
                //            }
                //            else
                //                memberActivity.Membership = false;

                //            db.tb_MemberActivity.Add(memberActivity);
                //        }
                //        else
                //        {
                //            if (ActivityStatusID == 1) // 1 - Committed
                //            {
                //                oldMemberActivity.Membership = true;
                //                oldMemberActivity.MembershipCommitment++;
                //            }
                //            else
                //            {
                //                oldMemberActivity.Membership = false;
                //                if (oldMemberActivity.ActivityStatusID == 1 && oldMemberActivity.MembershipCommitment > 0) //commited before so reduce qty of commitments
                //                    oldMemberActivity.MembershipCommitment--;
                //            }
                //            oldMemberActivity.ActivityStatusID = ActivityStatusID;
                //            db.Entry(oldMemberActivity).State = EntityState.Modified;
                //        }
                //        db.SaveChanges();
                //        model._MemberActivity = db.tb_MemberActivity.Where(s => s.MemberID == id).ToList();
                //    }
                //    catch (DbEntityValidationException ex)
                //    {
                //        error.errCode = ErrorDetail.DataImportError;
                //        error.errMsg = ErrorDetail.GetMsg(error.errCode);
                //        foreach (DbEntityValidationResult validationError in ex.EntityValidationErrors)
                //        {
                //            error.errMsg += ". Object: " + validationError.Entry.Entity.ToString();
                //            foreach (DbValidationError err in validationError.ValidationErrors)
                //            {
                //                error.errMsg += ". " + err.ErrorMessage;
                //            }
                //        }
                //        errs.Add("Error #" + error.errCode.ToString() + "!" + error.errMsg);
                //        ViewData["ActivityErrorList"] = errs;
                //        return View(model);
                //    }
                //}
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

        // GET: DeleteAssessment/5
        [Authorize(Roles = "admin, organizer")]
        public ActionResult DeleteAssessment(int? AssessmentID, int? MemberId, int? CollegeID)
        {
            Error error = new Error();
            error.errCode = ErrorDetail.Success;
            if (AssessmentID == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            using (LRCEntities context = new LRCEntities())
            {
                try
                {
                    tb_Assessment assessment = context.tb_Assessment.Find(AssessmentID);
                    if (assessment == null)
                    {
                        return HttpNotFound();
                    }
                    context.tb_Assessment.Remove(assessment);
                    context.SaveChanges();
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
                }
            }
            return RedirectToAction("AddPersonAssessmentActivity", new { @id = MemberId, CollegeID = CollegeID });
        }

        // GET: DeleteActivity/5
        [Authorize(Roles = "admin, organizer")]
        public ActionResult DeleteActivity(int? ActivityID, int? MemberId, int? CollegeID)
        {
            Error error = new Error();
            error.errCode = ErrorDetail.Success;
            if (ActivityID == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            using (LRCEntities context = new LRCEntities())
            {
                try
                {
                    tb_MemberActivity activity = context.tb_MemberActivity.Find(ActivityID);
                    if (activity == null)
                    {
                        return HttpNotFound();
                    }
                    context.tb_MemberActivity.Remove(activity);
                    context.SaveChanges();
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
                }
            }
            return RedirectToAction("AddPersonAssessmentActivity", new { @id = MemberId, CollegeID = CollegeID });
        }
        
        // GET: Assessment/Add/5
        [Authorize(Roles = "admin, organizer")]
        public ActionResult AddAssessment(int? id)
        {
            if (id == null)
                return RedirectToAction("Index", "Errors", new { name = "Bad Request", description = "Member ID as parameter cannot be NULL" });

            List<string> errs = new List<string>();
            tb_Assessment MA = new tb_Assessment { MemberID = id ?? 0, AssessmentDate = DateTime.UtcNow };
            ViewBag.AssessmentNameID = new SelectList(db.tb_AssessmentName, "AssessmentNameID", "AssessmentName"/*, tb_Assessment.AssessmentNameID*/);
            ViewBag.ValueID = new SelectList(db.tb_AssessmentValue, "ValueID", "ValueName"/*, tb_Assessment.ValueID*/ /* selected value */);
            ViewBag.AssessedBy = new SelectList(db.AspNetUsers.OrderBy(s => s.LastFirstName), "Id", "LastFirstName");
            ViewBag.MemberAssessments = db.tb_Assessment.Where(f => f.tb_MemberMaster.MemberID == id).ToList();
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
        public ActionResult AddAssessment([Bind(Include = "AssessmentID,MemberID,AssessmentNameID,AssessmentDesc,ValueID,AssessmentDate,AssessedBy,Evaluation")] tb_Assessment model/*, string AssessedBy*/)
        {
            Error error = new Error();
            error.errCode = ErrorDetail.Success;
            List<string> errs = new List<string>();

            ViewBag.AssessmentNameID = new SelectList(db.tb_AssessmentName, "AssessmentNameID", "AssessmentName", model.AssessmentNameID);
            ViewBag.ValueID = new SelectList(db.tb_AssessmentValue, "ValueID", "ValueName", model.ValueID /* selected value */);
            ViewBag.MemberID = new SelectList(db.tb_MemberMaster, "MemberID", "LastName", model.MemberID);
            ViewBag.AssessedBy = new SelectList(db.AspNetUsers.OrderBy(s => s.LastFirstName), "Id", "LastFirstName", model.AssessedBy);

            if (ModelState.IsValid)
            {
                try
                {
                    var userId = HttpContext.GetOwinContext().Authentication.User.Identity.GetUserId();
                    model.AddedBy = userId;
                    model.AddedDateTime = DateTime.UtcNow;
                    db.tb_Assessment.Add(model);

                    // Next commented block of code in case if we may have only 1 assessment fot a Member
                    //var memberAssessment = db.tb_Assessment.Where(s => s.MemberID == model.MemberID).FirstOrDefault();
                    //if (memberAssessment == null) //Add new member assessment
                    //{
                    //    model.AddedBy = userId;
                    //    model.AddedDateTime = DateTime.UtcNow;
                    //    db.tb_Assessment.Add(model);
                    //}

                    //else // Edit previous member assessment
                    //{
                    //    memberAssessment.ModifiedBy = userId;
                    //    memberAssessment.AssessmentDate = model.AssessmentDate;
                    //    memberAssessment.AssessedBy = model.AssessedBy;
                    //    memberAssessment.AssessmentNameID = model.AssessmentNameID;
                    //    memberAssessment.ValueID = model.ValueID;
                    //    memberAssessment.Evaluation = model.Evaluation;
                    //    db.Entry(memberAssessment).State = EntityState.Modified;
                    //}

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

        // POST: Assessment/Add/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "admin, organizer")]
        public ActionResult AddBackgroundStory([Bind(Include = "AssessmentID,MemberID,AssessmentNameID,AssessmentDesc,ValueID,AssessmentDate,AssessedBy,BackgroundStory,Fears,AttitudeTowardUnion,IDdLeaders,FollowUp")] tb_Assessment model/*, string AssessedBy*/)
        {
            Error error = new Error();
            error.errCode = ErrorDetail.Success;
            List<string> errs = new List<string>();

            ViewBag.AssessmentNameID = new SelectList(db.tb_AssessmentName, "AssessmentNameID", "AssessmentName", model.AssessmentNameID);
            ViewBag.ValueID = new SelectList(db.tb_AssessmentValue, "ValueID", "ValueName", model.ValueID /* selected value */);
            ViewBag.MemberID = new SelectList(db.tb_MemberMaster, "MemberID", "LastName", model.MemberID);
            ViewBag.AssessedBy = new SelectList(db.AspNetUsers, "Id", "LastFirstName", model.AssessedBy).OrderBy(s => s.Value);

            if (String.IsNullOrEmpty(model.Evaluation))
            {
                errs.Add("Required field!" + "Evaluation text area can not be empty");
                ViewData["ErrorList"] = errs; return View(model);
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var uName = HttpContext.GetOwinContext().Authentication.User.Identity.GetUserName();
                    model.AddedBy = uName;
                    model.AddedDateTime = DateTime.UtcNow;
                    db.tb_Assessment.Attach(model);
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
