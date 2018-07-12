using System;
using System.Collections.Generic;
using System.Data.Entity.Validation;
using System.Linq;
using System.Data.Entity;
using System.Web;
using System.Web.Mvc;
using System.Net;

namespace LRC_NET_Framework.Controllers
{
    public class ImportController : Controller
    {
        private LRCEntities db = new LRCEntities();

        // GET: Member Import Errors
        [Authorize(Roles = "admin, organizer")]
        public ActionResult MemberImportErrors()
        {
            var tb_MemberError = db.tb_MemberError.ToList();
            return View(tb_MemberError);
        }

        // GET: Schedule Import Errors
        [Authorize(Roles = "admin, organizer")]
        public ActionResult ScheduleImportErrors()
        {
            var tb_Schedule_Error = db.tb_Schedule_Error.ToList();
            return View(tb_Schedule_Error);
        }

        // GET: Delete Member Import Errors
        [Authorize(Roles = "admin, organizer")]
        public ActionResult DeleteMemberRecord(int errId)
        {
            using (LRCEntities context = new LRCEntities())
            {
                var me = context.tb_MemberError.Find(errId);
                context.tb_MemberError.Remove(me);
                try
                {
                    context.SaveChanges();
                }
                catch (DbEntityValidationException ex)
                {

                }
            }
            return RedirectToAction("MemberImportErrors");
        }

        // GET: Delete Schedule Import Errors
        [Authorize(Roles = "admin, organizer")]
        public ActionResult DeleteScheduleRecord(int errId)
        {
            using (LRCEntities context = new LRCEntities())
            {
                var me = context.tb_Schedule_Error.Find(errId);
                context.tb_Schedule_Error.Remove(me);
                try
                {
                    context.SaveChanges();
                }
                catch (DbEntityValidationException ex)
                {

                }
            }
            return RedirectToAction("ScheduleImportErrors");
        }

        // GET: Colleges Mapping (Member CBU Import)
        [Authorize(Roles = "admin, organizer")]
        public ActionResult CollegesMapping()
        {
            ViewBag.Campuses = db.tb_CampusMapping.Include(c => c.tb_Campus).Where(m => m.MemberMappingCode != null).ToList();
            var campusNames = new SelectList(db.tb_Campus.Where(m => m.IsMain == true), "CampusID", "CampusName");
            ViewBag.CampusId = campusNames.OrderBy(t => t.Text);
            return View();
        }

        // POST: Colleges Mapping (Member CBU Import)
        [Authorize(Roles = "admin, organizer")]
        [HttpPost]
        public ActionResult CollegesMapping([Bind(Include = "ID,MemberMappingCode")] tb_CampusMapping tb_CampusMapping, int? CampusId)
        {
            if (CampusId == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            using (LRCEntities context = new LRCEntities())
            {
                tb_CampusMapping campusMapping = new tb_CampusMapping
                {
                    CampusID = CampusId ?? 0,
                    MemberMappingCode = tb_CampusMapping.MemberMappingCode
                };
                context.tb_CampusMapping.Add(campusMapping);
                try
                {
                    context.SaveChanges();
                }
                catch (DbEntityValidationException ex)
                {

                }
            }
            ViewBag.Campuses = db.tb_CampusMapping.Include(c => c.tb_Campus).Where(m => m.MemberMappingCode != null).ToList();
            var campusNames = new SelectList(db.tb_Campus.Where(m => m.IsMain == true), "CampusID", "CampusName");
            ViewBag.CampusId = campusNames.OrderBy(t => t.Text);
            return View();
        }


        // GET: Delete College Mapping Record (Member CBU Import)
        [Authorize(Roles = "admin, organizer")]
        public ActionResult RemoveMemberCollegeMapping(int Id)
        {
            using (LRCEntities context = new LRCEntities())
            {
                var rec = context.tb_CampusMapping.Find(Id);
                context.tb_CampusMapping.Remove(rec);
                try
                {
                    context.SaveChanges();
                }
                catch (DbEntityValidationException ex)
                {

                }
            }
            return RedirectToAction("CollegesMapping");
        }


        // GET: Campuses Mapping (Schedule CBU Import)
        [Authorize(Roles = "admin, organizer")]
        public ActionResult CampusesMapping()
        {
            ViewBag.Campuses = db.tb_CampusMapping.Include(c => c.tb_Campus).Where(m => m.ScheduleMappingName != null).ToList();
            var campusNames = new SelectList(db.tb_Campus, "CampusID", "CampusName");
            ViewBag.CampusId = campusNames.OrderBy(t => t.Text);
            return View();
        }

        // POST: Campuses Mapping (Schedule CBU Import)
        [Authorize(Roles = "admin, organizer")]
        [HttpPost]
        public ActionResult CampusesMapping([Bind(Include = "ID,ScheduleMappingName")] tb_CampusMapping tb_CampusMapping, int? CampusId)
        {
            if (CampusId == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            using (LRCEntities context = new LRCEntities())
            {
                tb_CampusMapping campusMapping = new tb_CampusMapping
                {
                    CampusID = CampusId ?? 0,
                    ScheduleMappingName = tb_CampusMapping.ScheduleMappingName
                };
                context.tb_CampusMapping.Add(campusMapping);
                try
                {
                    context.SaveChanges();
                }
                catch (DbEntityValidationException ex)
                {

                }
            }
            ViewBag.Campuses = db.tb_CampusMapping.Include(c => c.tb_Campus).Where(m => m.ScheduleMappingName != null).ToList();
            var campusNames = new SelectList(db.tb_Campus, "CampusID", "CampusName");
            ViewBag.CampusId = campusNames.OrderBy(t => t.Text);
            return View();
        }


        // GET: Delete Campus Mapping Record (Schedule CBU Import)
        [Authorize(Roles = "admin, organizer")]
        public ActionResult RemoveScheduleCampusMapping(int Id)
        {
            using (LRCEntities context = new LRCEntities())
            {
                var rec = context.tb_CampusMapping.Find(Id);
                context.tb_CampusMapping.Remove(rec);
                try
                {
                    context.SaveChanges();
                }
                catch (DbEntityValidationException ex)
                {

                }
            }
            return RedirectToAction("CampusesMapping");
        }
    }
}