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
    public class tb_MemberMasterController : Controller
    {
        private LRCEntities db = new LRCEntities();

        // GET: tb_MemberMaster
        public ActionResult Index(string sortOrder, string searchString, int? page)
        {
            var tb_MemberMasters = db.tb_MemberMasters.Include(t => t.tb_Area).Include(t => t.tb_Department).Include(t => t.tb_Dues).Include(t => t.tb_LatestUnionAssessment).Include(t => t.tb_MemberPhoneNumbers).Include(t => t.tb_Dues);

            tb_MemberMasters.Select(t => t.tb_Department.tb_College);

            ViewBag.NameSortParm = String.IsNullOrEmpty(sortOrder) ? "Name desc" : "";

            if (!String.IsNullOrEmpty(searchString))
            {
                tb_MemberMasters = tb_MemberMasters.Where(s => s.LastName.ToUpper().Contains(searchString.ToUpper())
                                       || s.FirstName.ToUpper().Contains(searchString.ToUpper()));
            }

            switch (sortOrder)
            {
                case "Name desc":
                    tb_MemberMasters = tb_MemberMasters.OrderByDescending(s => s.LastName);
                    break;
                //case "Date":
                //    tb_MemberMasters = tb_MemberMasters.OrderBy(s => s.HireDate);
                //    break;
                //case "Date desc":
                //    tb_MemberMasters = tb_MemberMasters.OrderByDescending(s => s.HireDate);
                //    break;
                default:
                    tb_MemberMasters = tb_MemberMasters.OrderBy(s => s.LastName);
                    break;
            }
            int pageSize = 3;
            int pageNumber = (page ?? 1);


            return View(tb_MemberMasters.ToPagedList(pageNumber, pageSize));
        }

        // GET: tb_MemberMaster/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            tb_MemberMaster tb_MemberMaster = db.tb_MemberMasters.Find(id);
            if (tb_MemberMaster == null)
            {
                return HttpNotFound();
            }
            return View(tb_MemberMaster);
        }

        // GET: tb_MemberMaster/Create
        public ActionResult Create()
        {
            ViewBag.AreaID = new SelectList(db.tb_Area, "AreaID", "AreaName");
            ViewBag.DepartmentID = new SelectList(db.tb_Department, "DepartmentID", "DepartmentName");
            ViewBag.DuesID = new SelectList(db.tb_Dues, "DuesID", "DuesName");
            ViewBag.LatestUnionAssessment = new SelectList(db.tb_LatestUnionAssessment, "LatestUnionAssessment", "LatestUnionAssessmentDesc");
            ViewBag.PhoneRecID = new SelectList(db.tb_MemberPhoneNumbers, "PhoneRecID", "PhoneType");
            ViewBag.GenderID = new SelectList(db.tb_Gender, "GenderID", "GenderName");
            return View();
        }

        // POST: tb_MemberMaster/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "MemberID,MemberIDNumber,LastName,FirstName,MiddleName,DepartmentID,AreaID,CopeStatus,CopeAmount,Counselors,CampaignVolunteer,LatestUnionAssessment,MailCodeID,DuesCategoryEffDate,UnionInitiationDate,HireDate,DateOfBirth,GenderID,RetiredEffDate,DeactivateEffDate,DeactivateReasonID,LeadershipPositionID,PoliticalAssessmentID,ParticipatePolitical,PoliticalActivitiesID,MemberAddressID,PhoneRecID,DuesID,AddedBy,AddedDateTime,ModifiedBy,ModifiedDateTime")] tb_MemberMaster tb_MemberMaster)
        {
            if (ModelState.IsValid)
            {
                db.tb_MemberMasters.Add(tb_MemberMaster);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.AreaID = new SelectList(db.tb_Area, "AreaID", "AreaName", tb_MemberMaster.AreaID);
            ViewBag.DepartmentID = new SelectList(db.tb_Department, "DepartmentID", "DepartmentName", tb_MemberMaster.DepartmentID);
            ViewBag.DuesID = new SelectList(db.tb_Dues, "DuesID", "DuesName", tb_MemberMaster.DuesID);
            ViewBag.LatestUnionAssessment = new SelectList(db.tb_LatestUnionAssessment, "LatestUnionAssessment", "LatestUnionAssessmentDesc", tb_MemberMaster.LatestUnionAssessment);
            ViewBag.PhoneRecID = new SelectList(db.tb_MemberPhoneNumbers, "PhoneRecID", "PhoneType", tb_MemberMaster.PhoneRecID);
            ViewBag.GenderID = new SelectList(db.tb_Gender, "GenderID", "GenderName", tb_MemberMaster.GenderID);
            return View(tb_MemberMaster);
        }

        // GET: tb_MemberMaster/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            tb_MemberMaster tb_MemberMaster = db.tb_MemberMasters.Find(id);
            if (tb_MemberMaster == null)
            {
                return HttpNotFound();
            }
            ViewBag.AreaID = new SelectList(db.tb_Area, "AreaID", "AreaName", tb_MemberMaster.AreaID);
            ViewBag.DepartmentID = new SelectList(db.tb_Department, "DepartmentID", "DepartmentName", tb_MemberMaster.DepartmentID);
            ViewBag.DuesID = new SelectList(db.tb_Dues, "DuesID", "DuesName", tb_MemberMaster.DuesID);
            ViewBag.LatestUnionAssessment = new SelectList(db.tb_LatestUnionAssessment, "LatestUnionAssessment", "LatestUnionAssessmentDesc", tb_MemberMaster.LatestUnionAssessment);
            ViewBag.PhoneRecID = new SelectList(db.tb_MemberPhoneNumbers, "PhoneRecID", "PhoneType", tb_MemberMaster.PhoneRecID);
            ViewBag.GenderID = new SelectList(db.tb_Gender, "GenderID", "GenderName", tb_MemberMaster.GenderID);
            return View(tb_MemberMaster);
        }

        // POST: tb_MemberMaster/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "MemberID,MemberIDNumber,LastName,FirstName,MiddleName,DepartmentID,AreaID,CopeStatus,CopeAmount,Counselors,CampaignVolunteer,LatestUnionAssessment,MailCodeID,DuesCategoryEffDate,UnionInitiationDate,HireDate,DateOfBirth,GenderID,RetiredEffDate,DeactivateEffDate,DeactivateReasonID,LeadershipPositionID,PoliticalAssessmentID,ParticipatePolitical,PoliticalActivitiesID,MemberAddressID,PhoneRecID,DuesID,AddedBy,AddedDateTime,ModifiedBy,ModifiedDateTime")] tb_MemberMaster tb_MemberMaster)
        {
            if (ModelState.IsValid)
            {
                db.Entry(tb_MemberMaster).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.AreaID = new SelectList(db.tb_Area, "AreaID", "AreaName", tb_MemberMaster.AreaID);
            ViewBag.DepartmentID = new SelectList(db.tb_Department, "DepartmentID", "DepartmentName", tb_MemberMaster.DepartmentID);
            ViewBag.DuesID = new SelectList(db.tb_Dues, "DuesID", "DuesName", tb_MemberMaster.DuesID);
            ViewBag.LatestUnionAssessment = new SelectList(db.tb_LatestUnionAssessment, "LatestUnionAssessment", "LatestUnionAssessmentDesc", tb_MemberMaster.LatestUnionAssessment);
            ViewBag.PhoneRecID = new SelectList(db.tb_MemberPhoneNumbers, "PhoneRecID", "PhoneType", tb_MemberMaster.PhoneRecID);
            ViewBag.GenderID = new SelectList(db.tb_Gender, "GenderID", "GenderName", tb_MemberMaster.GenderID);
            return View(tb_MemberMaster);
        }

        // GET: tb_MemberMaster/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            tb_MemberMaster tb_MemberMaster = db.tb_MemberMasters.Find(id);
            if (tb_MemberMaster == null)
            {
                return HttpNotFound();
            }
            return View(tb_MemberMaster);
        }

        // POST: tb_MemberMaster/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            tb_MemberMaster tb_MemberMaster = db.tb_MemberMasters.Find(id);
            db.tb_MemberMasters.Remove(tb_MemberMaster);
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
