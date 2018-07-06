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

namespace LRC_NET_Framework.Controllers
{
    public class SemesterTaughtController : Controller
    {
        private LRCEntities db = new LRCEntities();

        // GET: SemesterTaught
        [Authorize(Roles = "admin, organizer")]
        public ActionResult Index()
        {
            var tb_SemesterTaught = db.tb_SemesterTaught.Include(t => t.tb_Semesters).Include(t => t.tb_WeekDay).Include(t => t.tb_MemberMaster);
            return View(tb_SemesterTaught.ToList());
        }

        // GET: SemesterTaught/Details/5
        [Authorize(Roles = "admin, organizer")]
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

        // GET: SemesterTaught/Create
        [Authorize(Roles = "admin, organizer")]
        public ActionResult Create(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            List<tb_WeekDay> DaysOfWeek = new List<tb_WeekDay>();
            DaysOfWeek = db.tb_WeekDay.ToList();

            List<SelectListItem> ScheduleStatuses = new List<SelectListItem>();
            ScheduleStatuses.Add(new SelectListItem() { Text = "Inactive", Value = "0" });
            ScheduleStatuses.Add(new SelectListItem() { Text = "Active", Value = "1" });

            SemesterTaughtModels model = new SemesterTaughtModels()
            {
                _MemberID = id?? 0,
                _CollegeID = 0,
                _Colleges = new SelectList(db.tb_College, "CollegeID", "CollegeName"),
                _CampusID = 0,
                _Campuses = new SelectList(db.tb_Campus, "CampusID", "CampusName"),
                _BuildingID = 0,
                _Buildings = new SelectList(db.tb_Building, "BuildingID", "BuildingName"),
                //_SemesterRecID = semesterRecID,
                _Room = String.Empty,
                _Class = String.Empty,
                _StartTime = new DateTime(),
                _EndTime = new DateTime(),
                _ClassWeekDayID = 0,
                _WeekDays = DaysOfWeek,
                _ScheduleStatusID = 1,
                _ScheduleStatuses = ScheduleStatuses
            };

            return View(model);
        }

        // POST: SemesterTaught/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "admin, organizer")]
        public ActionResult Create(SemesterTaughtModels model, int classWeekDayID)
         {
            //var semesterRecID = db.tb_Semesters.Where(t => t.DateFrom >= DateTime.UtcNow && t.DateTo < DateTime.UtcNow).FirstOrDefault().SemesterRecID;
            bool scheduleStatus = false;
            if (model._ScheduleStatusID == 1)
                scheduleStatus = true;

            int semesterRecID = 0;
            try
            {
                semesterRecID = db.tb_Semesters.Where(t => t.FiscalYear == DateTime.UtcNow.Year.ToString())
                    .Where(t => t.DateFrom <= DateTime.UtcNow)
                    .Where(t => t.DateTo > DateTime.UtcNow).FirstOrDefault().SemesterRecID;
            }
            catch (Exception)
            {
                // not good here. situation when DateTime.UtcNow isn't in semester period. taking next semester then
                semesterRecID = db.tb_Semesters.Where(t => t.DateTo > DateTime.UtcNow).FirstOrDefault().SemesterRecID;
            }

            tb_SemesterTaught newSemesterTaught = new tb_SemesterTaught()
            {
                SemesterRecID = semesterRecID,
                MemberID = model._MemberID,
                BuildingID = model._BuildingID, //not present on the Knock page
                Room = model._Room?? " ",
                Class = model._Class?? " ",
                ClassStart = model._StartTime.TimeOfDay,
                ClassEnd = model._EndTime.TimeOfDay,
                ClassWeekDayID = classWeekDayID,
                ScheduleStatus = scheduleStatus
            };
            ////if (oldAssessment == null)
            ////{
            ////    db.tb_Assessment.Add(AssessActivity._Assessment);
            ////}

            if (ModelState.IsValid)
            {
                db.tb_SemesterTaught.Add(newSemesterTaught);
                db.SaveChanges();
                return RedirectToAction("Details", "Home", new { @id = model._MemberID });
            }

            return View(model);
        }

        // GET: SemesterTaught/AddWeekDays
        [Authorize(Roles = "admin, organizer")]
        public ActionResult AddWeekDays()
        {
            List<string> errs = new List<string>();
            if (TempData["ErrorList"] == null)
            {
                errs.Add("Empty");
            }
            else
                errs = TempData["ErrorList"] as List<string>;

            ViewData["ErrorList"] = errs;

            return View();
        }

        // POST: SemesterTaught/AddWeekDays
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "admin, organizer")]
        public ActionResult AddWeekDays([Bind(Include = "WeekDayName,CbuWeekDay")] tb_WeekDay WeekDay)
        {
            Error error = new Error();
            error.errCode = ErrorDetail.Success;
            error.errMsg = ErrorDetail.GetMsg(error.errCode);
            List<string> errs = new List<string>();

            if (ModelState.IsValid)
            {
                db.tb_WeekDay.Add(WeekDay);
                try
                {
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
                }
            }

            TempData["ErrorList"] = errs;
            return RedirectToAction("AdminTasks", "Account", null);
            //return View(WeekDay);
        }

        // GET: SemesterTaught/Edit/5
        [Authorize(Roles = "admin, organizer")]
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
            //ViewBag.ClassID = new SelectList(db.tb_Class, "ClassID", "ClassName", tb_SemesterTaught.ClassID);
            ViewBag.SemesterRecID = new SelectList(db.tb_Semesters, "SemesterRecID", "FiscalYear", tb_SemesterTaught.SemesterRecID);
            //ViewBag.RoomID = new SelectList(db.tb_Room, "RoomID", "RoomName", tb_SemesterTaught.RoomID);
            ViewBag.ClassWeekDayID = new SelectList(db.tb_WeekDay, "ClassWeekDayID", "WeekDayName", tb_SemesterTaught.ClassWeekDayID);
            ViewBag.MemberID = new SelectList(db.tb_MemberMaster, "MemberID", "MemberIDNumber", tb_SemesterTaught.MemberID);
            return View(tb_SemesterTaught);
        }

        // POST: SemesterTaught/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "admin, organizer")]
        public ActionResult Edit([Bind(Include = "SemesterTaughtID,SemesterRecID,MemberID,RoomID,ClassID,ClassStart,ClassEnd,ClassWeekDayID")] tb_SemesterTaught tb_SemesterTaught)
        {
            if (ModelState.IsValid)
            {
                db.Entry(tb_SemesterTaught).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            //ViewBag.ClassID = new SelectList(db.tb_Class, "ClassID", "ClassName", tb_SemesterTaught.ClassID);
            ViewBag.SemesterRecID = new SelectList(db.tb_Semesters, "SemesterRecID", "FiscalYear", tb_SemesterTaught.SemesterRecID);
            //ViewBag.RoomID = new SelectList(db.tb_Room, "RoomID", "RoomName", tb_SemesterTaught.RoomID);
            ViewBag.ClassWeekDayID = new SelectList(db.tb_WeekDay, "ClassWeekDayID", "WeekDayName", tb_SemesterTaught.ClassWeekDayID);
            ViewBag.MemberID = new SelectList(db.tb_MemberMaster, "MemberID", "MemberIDNumber", tb_SemesterTaught.MemberID);
            return View(tb_SemesterTaught);
        }

        // GET: SemesterTaught/Delete/5
        [Authorize(Roles = "admin, organizer")]
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

        // POST: SemesterTaught/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "admin, organizer")]
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
