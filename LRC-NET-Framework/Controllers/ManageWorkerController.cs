using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using LRC_NET_Framework;
using LRC_NET_Framework.Models;
using PagedList;
using PagedList.Mvc;

namespace LRC_NET_Framework.Controllers
{
    public class ManageWorkerController : Controller
    {
        private LRCEntities db = new LRCEntities();

        // GET: ManageWorker
        public ActionResult Index(int? id)
        {
            id = 1; // test REMOVE IT
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            tb_MemberMaster Worker = db.tb_MemberMaster.Find(id); //.Include(t => t.tb_Area).Include(t => t.tb_Department).Include(t => t.tb_Division).Include(t => t.tb_Dues).Include(t => t.tb_Gender).Include(t => t.tb_LatestUnionAssessment)
            if (Worker == null)
            {
                return HttpNotFound();
            }
            tb_MemberAddress ma = Worker.tb_MemberAddress.Where(t => t.MemberID == id).Where(t => t.IsPrimary == true).FirstOrDefault();
            ViewBag.MemberAddress = ma.HomeStreet1 + " " + ma.HomeStreet2 + ", " + ma.tb_CityState.CityName + ", " + ma.tb_CityState.CityAlias + ", " + ma.ZipCode;

            //tb_AssessmentName assessmentName = new tb_AssessmentName();
            //assessmentName = db.tb_AssessmentName;
            List<tb_AssessmentName> AssessmentNames = new List<tb_AssessmentName>();
            AssessmentNames = db.tb_AssessmentName.ToList();

            var model = new ManageWorkerModels()
            {
                _Worker = Worker,
                _AssessmentName = AssessmentNames
            };
            return View(model);
        }

        // GET: ManageWorker/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            tb_MemberMaster tb_MemberMaster = db.tb_MemberMaster.Find(id);
            if (tb_MemberMaster == null)
            {
                return HttpNotFound();
            }
            return View(tb_MemberMaster);
        }

        // GET: ManageWorker/Create
        public ActionResult Create()
        {
            ViewBag.AreaID = new SelectList(db.tb_Area, "AreaID", "AreaName");
            ViewBag.DepartmentID = new SelectList(db.tb_Department, "DepartmentID", "DepartmentName");
            ViewBag.DivisionID = new SelectList(db.tb_Division, "DivisionID", "DivisionName");
            ViewBag.DuesID = new SelectList(db.tb_Dues, "DuesID", "DuesName");
            ViewBag.GenderID = new SelectList(db.tb_Gender, "GenderID", "GenderName");
            ViewBag.LatestUnionAssessmentID = new SelectList(db.tb_LatestUnionAssessment, "LatestUnionAssessmentID", "LatestUnionAssessmentDesc");
            return View();
        }

        // POST: ManageWorker/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "MemberID,MemberIDNumber,LastName,FirstName,MiddleName,DepartmentID,AreaID,CopeStatus,CopeAmount,Counselors,CampaignVolunteer,LatestUnionAssessmentID,MailCodeID,DuesCategoryEffDate,UnionInitiationDate,HireDate,DateOfBirth,GenderID,RetiredEffDate,DeactivateEffDate,DeactivateReasonID,LeadershipPositionID,PoliticalAssessmentID,ParticipatePolitical,PoliticalActivitiesID,MemberAddressID,PhoneRecID,DuesID,AddedBy,AddedDateTime,ModifiedBy,ModifiedDateTime,DivisionID")] tb_MemberMaster tb_MemberMaster)
        {
            if (ModelState.IsValid)
            {
                db.tb_MemberMaster.Add(tb_MemberMaster);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.AreaID = new SelectList(db.tb_Area, "AreaID", "AreaName", tb_MemberMaster.AreaID);
            ViewBag.DepartmentID = new SelectList(db.tb_Department, "DepartmentID", "DepartmentName", tb_MemberMaster.DepartmentID);
            ViewBag.DivisionID = new SelectList(db.tb_Division, "DivisionID", "DivisionName", tb_MemberMaster.DivisionID);
            ViewBag.DuesID = new SelectList(db.tb_Dues, "DuesID", "DuesName", tb_MemberMaster.DuesID);
            ViewBag.GenderID = new SelectList(db.tb_Gender, "GenderID", "GenderName", tb_MemberMaster.GenderID);
            ViewBag.LatestUnionAssessmentID = new SelectList(db.tb_LatestUnionAssessment, "LatestUnionAssessmentID", "LatestUnionAssessmentDesc", tb_MemberMaster.LatestUnionAssessmentID);
            return View(tb_MemberMaster);
        }

        // GET: ManageWorker/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            tb_MemberMaster tb_MemberMaster = db.tb_MemberMaster.Find(id);
            if (tb_MemberMaster == null)
            {
                return HttpNotFound();
            }
            ViewBag.AreaID = new SelectList(db.tb_Area, "AreaID", "AreaName", tb_MemberMaster.AreaID);
            ViewBag.DepartmentID = new SelectList(db.tb_Department, "DepartmentID", "DepartmentName", tb_MemberMaster.DepartmentID);
            ViewBag.DivisionID = new SelectList(db.tb_Division, "DivisionID", "DivisionName", tb_MemberMaster.DivisionID);
            ViewBag.DuesID = new SelectList(db.tb_Dues, "DuesID", "DuesName", tb_MemberMaster.DuesID);
            ViewBag.GenderID = new SelectList(db.tb_Gender, "GenderID", "GenderName", tb_MemberMaster.GenderID);
            ViewBag.LatestUnionAssessmentID = new SelectList(db.tb_LatestUnionAssessment, "LatestUnionAssessmentID", "LatestUnionAssessmentDesc", tb_MemberMaster.LatestUnionAssessmentID);
            return View(tb_MemberMaster);
        }

        // POST: ManageWorker/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "MemberID,MemberIDNumber,LastName,FirstName,MiddleName,DepartmentID,AreaID,CopeStatus,CopeAmount,Counselors,CampaignVolunteer,LatestUnionAssessmentID,MailCodeID,DuesCategoryEffDate,UnionInitiationDate,HireDate,DateOfBirth,GenderID,RetiredEffDate,DeactivateEffDate,DeactivateReasonID,LeadershipPositionID,PoliticalAssessmentID,ParticipatePolitical,PoliticalActivitiesID,MemberAddressID,PhoneRecID,DuesID,AddedBy,AddedDateTime,ModifiedBy,ModifiedDateTime,DivisionID")] tb_MemberMaster tb_MemberMaster)
        {
            if (ModelState.IsValid)
            {
                db.Entry(tb_MemberMaster).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.AreaID = new SelectList(db.tb_Area, "AreaID", "AreaName", tb_MemberMaster.AreaID);
            ViewBag.DepartmentID = new SelectList(db.tb_Department, "DepartmentID", "DepartmentName", tb_MemberMaster.DepartmentID);
            ViewBag.DivisionID = new SelectList(db.tb_Division, "DivisionID", "DivisionName", tb_MemberMaster.DivisionID);
            ViewBag.DuesID = new SelectList(db.tb_Dues, "DuesID", "DuesName", tb_MemberMaster.DuesID);
            ViewBag.GenderID = new SelectList(db.tb_Gender, "GenderID", "GenderName", tb_MemberMaster.GenderID);
            ViewBag.LatestUnionAssessmentID = new SelectList(db.tb_LatestUnionAssessment, "LatestUnionAssessmentID", "LatestUnionAssessmentDesc", tb_MemberMaster.LatestUnionAssessmentID);
            return View(tb_MemberMaster);
        }

        // GET: ManageWorker/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            tb_MemberMaster tb_MemberMaster = db.tb_MemberMaster.Find(id);
            if (tb_MemberMaster == null)
            {
                return HttpNotFound();
            }
            return View(tb_MemberMaster);
        }

        // POST: ManageWorker/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            tb_MemberMaster tb_MemberMaster = db.tb_MemberMaster.Find(id);
            db.tb_MemberMaster.Remove(tb_MemberMaster);
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
