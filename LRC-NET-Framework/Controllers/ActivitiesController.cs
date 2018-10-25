using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Validation;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using LRC_NET_Framework;
using LRC_NET_Framework.Models;
using Microsoft.AspNet.Identity;
using Newtonsoft.Json;
using PagedList;
using PagedList.Mvc;

namespace LRC_NET_Framework.Controllers
{
    public class ActivitiesController : Controller
    {
        private LRCEntities db = new LRCEntities();

        // GET: Activities
        [Authorize(Roles = "admin, organizer")]
        public ActionResult Index(string sortOrder, string searchString, int? page)
        {
            var Activities = db.tb_Activity.Include(t => t.tb_MemberActivity);

            ViewBag.NameSortParm = String.IsNullOrEmpty(sortOrder) ? "Name desc" : "";
            //Searching @ Filtering
            if (!String.IsNullOrEmpty(searchString))
            {
                Activities = Activities.Where(s => s.ActivityName.ToUpper().Contains(searchString.ToUpper()));
            }
            //Sorting
            switch (sortOrder)
            {
                case "Name desc":
                    Activities = Activities.OrderByDescending(s => s.ActivityName);
                    break;
                //case "Date":
                //    tb_MemberMasters = tb_MemberMasters.OrderBy(s => s.HireDate);
                //    break;
                //case "Date desc":
                //    tb_MemberMasters = tb_MemberMasters.OrderByDescending(s => s.HireDate);
                //    break;
                default:
                    Activities = Activities.OrderBy(s => s.ActivityName);
                    break;
            }

            //Paging
            int pageSize = 20;
            int pageNumber = (page ?? 1);

            ViewData["MemberQty"] = Activities.Count();


            return View(Activities.ToPagedList(pageNumber, pageSize));
        }

        // GET: Activities/Create
        [Authorize(Roles = "admin, organizer")]
        public ActionResult CreateActivity(string callback, int? memberId, int? returnCollegeID)
        {
            ActivityModels activityModel = new ActivityModels {
                _Callback = callback,
                _MemberId = memberId ?? 0,
                _ReturnCollegeID = returnCollegeID ?? 0,
                _Campuses = new SelectList(db.tb_Campus.OrderBy(s => s.CampusName), "CampusID", "CampusName"),
                _ActivityDate = DateTime.UtcNow
            };
            activityModel._Colleges = new List<SelectListItem>
            { new SelectListItem() {Value = "0", Text = "-- Select One --" }}.Concat(db.tb_College.Select(x => new SelectListItem
                { Value = x.CollegeID.ToString(), Text = x.CollegeName }));

            return View(activityModel);
        }

        // POST: Activities/Create
        [HttpPost]
        [Authorize(Roles = "admin, organizer")]
        public ActionResult CreateActivity([Bind(Include = "_ActivityName,_ActivityDate,_ActivityNote,_CollegeID,_CampusID,_Callback,_MemberId,_ReturnCollegeID")] ActivityModels model, FormCollection formCollection)
        {
            Error error = new Error();
            error.errCode = ErrorDetail.Success;
            error.errMsg = ErrorDetail.GetMsg(error.errCode);
            List<string> errs = new List<string>();

            model._Colleges = new List<SelectListItem>
            { new SelectListItem() {Value = "0", Text = "-- Select One --" }}.Concat(db.tb_College.Select(x => new SelectListItem
            { Value = x.CollegeID.ToString(), Text = x.CollegeName }));
            model._Campuses = new SelectList(db.tb_Campus.Where(s => s.CollegeID == model._CollegeID).OrderBy(s => s.CampusName), "CampusID", "CampusName");

            var userId = HttpContext.GetOwinContext().Authentication.User.Identity.GetUserId();
            using (LRCEntities context = new LRCEntities())
            {
                tb_Activity tb_Activity = new tb_Activity
                {
                    ActivityName = model._ActivityName,
                    ActivityDate = model._ActivityDate,
                    CampusID = model._CampusID,
                    ActivityNote = model._ActivityNote,
                    AddedBy = userId,
                    AddedDateTime = DateTime.UtcNow
                };
                context.tb_Activity.Add(tb_Activity);
                try
                {
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
                    errs.Add("Error #" + error.errCode.ToString() + "!" + error.errMsg);
                    ViewData["ErrorList"] = errs;
                    return View(model);
                }
            }
            switch (model._Callback)
            {
                case "AddActivityByMember":
                    return RedirectToAction("AddActivityByMember");
                case "AddActivity":
                    return RedirectToAction("AddActivity", new { @id = model._MemberId });
                case "AddPersonAssessmentActivity":
                    return RedirectToAction("AddPersonAssessmentActivity", "Assessment", new { @id = model._MemberId, CollegeID = model._ReturnCollegeID });
                default:
                    break;
            }
            return View(model);
        }

        public JsonResult GetCampussesByCollegeId(int CollegeId)
        {
            List<tb_Activity> activities = new List<tb_Activity>();
            string json = String.Empty;
            using (LRCEntities context = new LRCEntities())
            {
                SelectList Campuses = new SelectList(context.tb_Campus.Where(s => s.CollegeID == CollegeId).OrderBy(s => s.CampusName), "CampusID", "CampusName");
                json = JsonConvert.SerializeObject(Campuses, Formatting.Indented);
            }
            return Json(json, JsonRequestBehavior.AllowGet);
        }


        // GET: Activities/Create
        [Authorize(Roles = "admin, organizer")]
        public ActionResult AddActivity(int id)
        {
            List<string> errs = new List<string>();

            ActivityByMemberModels memberActivities = new ActivityByMemberModels();
            using (LRCEntities context = new LRCEntities())
            {
                var tb_MemberMaster = context.tb_MemberMaster.ToList();
                ViewBag.ActivityStatus = context.tb_ActivityStatus.ToList();
                ViewBag.ActivityStatusAfter = new List<SelectListItem>
                {
                    new SelectListItem { Value = "0", Text = "Participated", Selected = false},
                    new SelectListItem { Value = "1", Text = "Not Participated", Selected = false}
                };

                SelectList Activities = new SelectList(db.tb_Activity.OrderBy(s => s.ActivityName), "ActivityID", "ActivityName");
                SelectListItem selListItem = new SelectListItem() { Value = "0", Text = " Select Activity " };
                ViewBag.ActivityID = CommonFunctions.AddFirstItem(Activities, selListItem);
            }
            memberActivities.MemberID = id;
            ViewBag.MemberActivities = db.tb_MemberActivity.Where(a => a.MemberID == id).OrderBy(s => s.ActivityID).ToList();
            tb_MemberMaster fm = db.tb_MemberMaster.Find(id);
            ViewBag.MemberName = fm.LastName + ", " + fm.FirstName;
            return View(memberActivities);
        }

        // POST: Activities/AddActivity
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "admin, organizer")]
        public ActionResult AddActivity([Bind(Include = "MemberID,ActivityID,ActivityStatusID")] ActivityByMemberModels model, FormCollection formCollection)
        {
            Error error = new Error();
            error.errCode = ErrorDetail.Success;
            List<string> errs = new List<string>();

            #region Prepare Model
            ViewBag.ActivityStatus = db.tb_ActivityStatus.ToList();
            ViewBag.ActivityStatusAfter = new List<SelectListItem>
                {
                    new SelectListItem { Value = "0", Text = "Participated", Selected = false},
                    new SelectListItem { Value = "1", Text = "Not Participated", Selected = false}
                };

            SelectList Activities = new SelectList(db.tb_Activity.OrderBy(s => s.ActivityName), "ActivityID", "ActivityName");
            SelectListItem selListItem = new SelectListItem() { Value = "0", Text = " Select Activity " };
            ViewBag.ActivityID = CommonFunctions.AddFirstItem(Activities, selListItem);
            #endregion

            if (ModelState.IsValid)
            {

                bool? isParticipated = null;
                if (formCollection["afterTheFact"] == "0")
                    isParticipated = true;
                else if (formCollection["afterTheFact"] == "1")
                    isParticipated = false;

                string userId = HttpContext.GetOwinContext().Authentication.User.Identity.GetUserId();

                ActivityByMemberModels activityCRUD = new ActivityByMemberModels();
                errs = activityCRUD.AddEditActivity(model.MemberID, model.ActivityID, model.ActivityStatusID, isParticipated, DateTime.Parse(formCollection["tb_Activity.ActivityDate"]), formCollection["tb_Activity.ActivityNote"], userId);

                if (errs == null || errs.Count == 0)
                    return RedirectToAction("Details", "Home", new { @id = model.MemberID });
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
            }
        }

        // GET: Activities/AddActivityByMember
        [Authorize(Roles = "admin, organizer")]
        public ActionResult AddActivityByMember()
        {
            //https://www.youtube.com/watch?v=z3ViZ6XA884 Asp.Net MVC - Searchable and Multi-select Drop DownList Using Chosen Plugin
            //Chosen Plugin https://harvesthq.github.io/chosen/
            ActivityByMemberModels memberActivities = new ActivityByMemberModels();
            using (LRCEntities context = new LRCEntities())
            {
                var tb_MemberMaster = context.tb_MemberMaster.ToList();
                List<SelectListItem> members = new List<SelectListItem>();
                foreach (var member in tb_MemberMaster)
                {
                    members.Add(new SelectListItem()
                    {
                        Text = member.LastName + ", " + member.FirstName,
                        Value = member.MemberID.ToString()
                    });
                    memberActivities.MemberCollection = members.OrderBy(s => s.Text).ToList();
                }

                ViewBag.ActivityStatus = context.tb_ActivityStatus.ToList();
                ViewBag.ActivityStatusAfter = new List<SelectListItem>
                {
                    new SelectListItem { Value = "0", Text = "Participated", Selected = false},
                    new SelectListItem { Value = "1", Text = "Not Participated", Selected = false}
                };

                SelectList Activities = new SelectList(db.tb_Activity.OrderBy(s => s.ActivityName), "ActivityID", "ActivityName");
                SelectListItem selListItem = new SelectListItem() { Value = "0", Text = " Select Activity " };
                ViewBag.ActivityID = CommonFunctions.AddFirstItem(Activities, selListItem);
            }

            return View(memberActivities);
        }

        // POST: Activities/AddActivityByMember
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "admin, organizer")]
        public ActionResult AddActivityByMember([Bind(Include = "ActivityID")] ActivityByMemberModels model, FormCollection formCollection)
        {
            Error error = new Error();
            error.errCode = ErrorDetail.Success;
            List<string> errs = new List<string>();

            bool result = Int32.TryParse(formCollection["MemberID"], out int mId); //MemberActivityID
            model.MemberID = mId;
            result = Int32.TryParse(formCollection["ActivityStatusID"], out int asID); //ActivityStatusID  (Before The Fact)
            model.ActivityStatusID = asID;

            #region Prepare Model
            ViewBag.ActivityStatus = db.tb_ActivityStatus.ToList();
            ViewBag.ActivityStatusAfter = new List<SelectListItem>
                {
                    new SelectListItem { Value = "0", Text = "Participated", Selected = false},
                    new SelectListItem { Value = "1", Text = "Not Participated", Selected = false}
                };

            SelectList Activities = new SelectList(db.tb_Activity.OrderBy(s => s.ActivityName), "ActivityID", "ActivityName");
            SelectListItem selListItem = new SelectListItem() { Value = "0", Text = " Select Activity " };
            ViewBag.ActivityID = CommonFunctions.AddFirstItem(Activities, selListItem);
            #endregion

            if (ModelState.IsValid)
            {

                bool? isParticipated = null;
                if (formCollection["afterTheFact"] == "0")
                    isParticipated = true;
                else if (formCollection["afterTheFact"] == "1")
                    isParticipated = false;

                string userId = HttpContext.GetOwinContext().Authentication.User.Identity.GetUserId();

                ActivityByMemberModels activityCRUD = new ActivityByMemberModels();
                errs = activityCRUD.AddEditActivity(model.MemberID, model.ActivityID, model.ActivityStatusID, isParticipated, DateTime.Parse(formCollection["tb_Activity.ActivityDate"]), formCollection["tb_Activity.ActivityNote"], userId);

                if (errs == null || errs.Count == 0)
                    return RedirectToAction("Index", "Activities", new { @id = model.MemberID });
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
            }
        }

        public JsonResult GetActivityDetailsById(int activityID, int memberID)
        {
            List<tb_Activity> activities = new List<tb_Activity>();
            string json = String.Empty;
            using (LRCEntities context = new LRCEntities())
            {
                if (activityID == 0)
                {
                    ActivityJSON activity = new ActivityJSON {
                        ActivityDate = String.Empty,
                        ActivityNote = String.Empty,
                        AddedBy = String.Empty,
                        AddedDateTime = String.Empty,
                        ActivityStatusBeforeTheFact = "Not Defined",
                        ActivityStatusAfterTheFact = "Not Defined"
                    };
                    json = JsonConvert.SerializeObject(activity, Formatting.Indented);
                }
                else
                {
                    var tb_Activity = context.tb_Activity.Where(a => a.ActivityID == activityID).ToList<tb_Activity>().FirstOrDefault();
                    var tb_MemberActivity = context.tb_MemberActivity.Where(a => a.MemberID == memberID && a.ActivityID == activityID).ToList<tb_MemberActivity>().FirstOrDefault();
                    string addedDateTime = String.Empty;
                    var d = tb_Activity.AddedDateTime ?? DateTime.MinValue;
                    if (d == DateTime.MinValue)
                        addedDateTime = " - ";
                    else
                        addedDateTime = d.ToString("yyyy-MM-dd");

                    string beforeTheFact = "Not Defined";
                    string afterTheFact = "Not Defined";
                    if (tb_MemberActivity != null)
                    {
                        if (tb_MemberActivity.Participated.GetValueOrDefault())
                            afterTheFact = "Participated";
                        else
                            afterTheFact = "Not Participated";
                        if (!tb_MemberActivity.Participated.HasValue)
                            afterTheFact = "Not Defined";

                        beforeTheFact = tb_MemberActivity.tb_ActivityStatus.ActivityStatusName;
                    }

                    ActivityJSON activity = new ActivityJSON
                    {
                        ActivityDate = tb_Activity.ActivityDate.ToString("yyyy-MM-dd"),
                        ActivityNote = tb_Activity.ActivityNote,
                        AddedBy = context.AspNetUsers.Where(s => s.Id == tb_Activity.AddedBy).FirstOrDefault().LastFirstName,
                        AddedDateTime = addedDateTime,
                        ActivityStatusBeforeTheFact = beforeTheFact,
                        ActivityStatusAfterTheFact = afterTheFact
                    };
                    json = JsonConvert.SerializeObject(activity, Formatting.Indented);
                }
            }
            return Json(json, JsonRequestBehavior.AllowGet);
        }

        [Authorize(Roles = "admin, organizer")]
        public ActionResult NotSure()
        {
            return PartialView("NotSure");
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
