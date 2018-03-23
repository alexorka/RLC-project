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
using LRC_NET_Framework.Models;
using System.Data.Entity.Validation;
//using static LRC_NET_Framework.Models.MemberMaster.MemberFilterViewModel;

namespace LRC_NET_Framework.Controllers
{
    public class MemberMasterController : Controller
    {
        private LRCEntities db = new LRCEntities();

        public static SelectList AddFirstItem(SelectList origList, SelectListItem firstItem)
        {
            List<SelectListItem> newList = origList.ToList();
            newList.Insert(0, firstItem);

            //var selectedItem = newList.FirstOrDefault(item => item.Selected);
            var selectedItem = newList.First();
            var selectedItemValue = String.Empty;
            if (selectedItem != null)
            {
                selectedItemValue = selectedItem.Value;
            }

            return new SelectList(newList, "Value", "Text", selectedItemValue);
        }

        // GET: tb_MemberMaster
        public ActionResult Index(string sortOrder, string searchString, int? page, int? CollegeID, int? DepartmentID)
        {
            var tb_MemberMasters = db.tb_MemberMaster.Include(t => t.tb_Area).Include(t => t.tb_Department).Include(t => t.tb_Dues).Include(t => t.tb_LatestUnionAssessment).Include(t => t.tb_Dues);

            tb_MemberMasters.Select(t => t.tb_Department.tb_College);
            ViewData["MemberQty"] = tb_MemberMasters.Count();

            //SelectList colleges = new SelectList(db.tb_College, "CollegeID", "CollegeDesc");

            ViewBag.NameSortParm = String.IsNullOrEmpty(sortOrder) ? "Name desc" : "";

            //Searching @ Filtering
            if (!String.IsNullOrEmpty(searchString))
            {
                tb_MemberMasters = tb_MemberMasters.Where(s => s.LastName.ToUpper().Contains(searchString.ToUpper())
                                       || s.FirstName.ToUpper().Contains(searchString.ToUpper()));
            }
            if (CollegeID != null && CollegeID != 0)
            {
                tb_MemberMasters = tb_MemberMasters.Where(f => f.tb_Department.CollegeID == CollegeID);
            }
            if (DepartmentID != null && DepartmentID != 0)
            {
                tb_MemberMasters = tb_MemberMasters.Where(f => f.DepartmentID == DepartmentID);
            }
            List<tb_College> colleges = db.tb_College.ToList();
            // устанавливаем начальный элемент, который позволит выбрать всех
            //_colleges.Insert(0, new tb_College { CollegeDesc = "All", CollegeID = 0 });
            ViewBag.Colleges = new SelectList(colleges, "CollegeID", "CollegeName");
            List<tb_Department> departments = db.tb_Department.ToList();
            tb_MemberMaster tb_MemberMaster = db.tb_MemberMaster.FirstOrDefault();
            SelectList Departments = new SelectList(db.tb_Department, "DepartmentID", "DepartmentName", tb_MemberMaster.DepartmentID);
            SelectListItem selListItem = new SelectListItem() { Value = "0", Text = " + Filter by Department " };
            ViewBag.DepartmentID = AddFirstItem(Departments, selListItem);
            SelectList Colleges = new SelectList(db.tb_College, "CollegeID", "CollegeName", tb_MemberMaster.tb_Department.CollegeID);
            selListItem = new SelectListItem() { Value = "0", Text = " + Filter by College " };
            ViewBag.CollegeID = AddFirstItem(Colleges, selListItem);

            //Sorting
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

            //Paging
            int pageSize = 3;
            int pageNumber = (page ?? 1);

            return View(tb_MemberMasters.ToPagedList(pageNumber, pageSize));
        }

        // GET: tb_MemberMaster/Edit/5
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
            ViewBag.CollegeID = new SelectList(db.tb_College, "CollegeID", "CollegeDesc", tb_MemberMaster.tb_Department.CollegeID);
            ViewBag.DivisionID = new SelectList(db.tb_Division, "DivisionID", "DivisionName", tb_MemberMaster.DivisionID);
            ViewBag.AreaID = new SelectList(db.tb_Area, "AreaID", "AreaName", tb_MemberMaster.AreaID);
            ViewBag.DepartmentID = new SelectList(db.tb_Department, "DepartmentID", "DepartmentName", tb_MemberMaster.DepartmentID);
            ViewBag.DuesID = new SelectList(db.tb_Dues, "DuesID", "DuesName", tb_MemberMaster.DuesID);
            ViewBag.LatestUnionAssessmentID = new SelectList(db.tb_LatestUnionAssessment, "LatestUnionAssessmentID", "LatestUnionAssessmentDesc", tb_MemberMaster.LatestUnionAssessmentID);
            ViewBag.PhoneRecID = new SelectList(db.tb_MemberPhoneNumbers, "PhoneRecID", "PhoneType", tb_MemberMaster.PhoneRecID);
            ViewBag.GenderID = new SelectList(db.tb_Gender, "GenderID", "GenderName", tb_MemberMaster.GenderID);
            return View(tb_MemberMaster);
        }

        // POST: tb_MemberMaster/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "MemberID,MemberIDNumber,CollegeID, LastName,FirstName,MiddleName,DepartmentID,AreaID,CopeStatus,CopeAmount,Counselors,CampaignVolunteer,LatestUnionAssessmentID,MailCodeID,DuesCategoryEffDate,UnionInitiationDate,HireDate,DateOfBirth,GenderID,RetiredEffDate,DeactivateEffDate,DeactivateReasonID,LeadershipPositionID,PoliticalAssessmentID,ParticipatePolitical,PoliticalActivitiesID,MemberAddressID,PhoneRecID,DuesID,AddedBy,AddedDateTime,ModifiedBy,ModifiedDateTime")] tb_MemberMaster tb_MemberMaster)
        {
            if (ModelState.IsValid)
            {
                //db.Entry(tb_MemberMaster).State = EntityState.Modified;
                db.tb_MemberMaster.Attach(tb_MemberMaster);
                var entry = db.Entry(tb_MemberMaster);

                //entry.State = EntityState.Modified;
                //entry.Property(e => e.tb_Department.CollegeID).IsModified = true;
                entry.Property(e => e.DivisionID).IsModified = true;
                entry.Property(e => e.DepartmentID).IsModified = true;
                entry.Property(e => e.HireDate).IsModified = true;
                //db.Entry(tb_MemberMaster).State = EntityState.Modified;

                try
                {
                    db.SaveChanges();
                }
                catch (DbEntityValidationException ex)
                {
                    foreach (DbEntityValidationResult validationError in ex.EntityValidationErrors)
                    {
                        Response.Write("Object: " + validationError.Entry.Entity.ToString());
                        Response.Write("");
                        foreach (DbValidationError err in validationError.ValidationErrors)
                        {
                            Response.Write(err.ErrorMessage + "");
                        }
                    }
                }
                return RedirectToAction("Index", "Home", null);
            }
            ViewBag.AreaID = new SelectList(db.tb_Area, "AreaID", "AreaName", tb_MemberMaster.AreaID);
            ViewBag.DepartmentID = new SelectList(db.tb_Department, "DepartmentID", "DepartmentName", tb_MemberMaster.DepartmentID);
            ViewBag.DuesID = new SelectList(db.tb_Dues, "DuesID", "DuesName", tb_MemberMaster.DuesID);
            ViewBag.LatestUnionAssessmentID = new SelectList(db.tb_LatestUnionAssessment, "LatestUnionAssessmentID", "LatestUnionAssessmentDesc", tb_MemberMaster.LatestUnionAssessmentID);
            ViewBag.PhoneRecID = new SelectList(db.tb_MemberPhoneNumbers, "PhoneRecID", "PhoneType", tb_MemberMaster.PhoneRecID);
            ViewBag.GenderID = new SelectList(db.tb_Gender, "GenderID", "GenderName", tb_MemberMaster.GenderID);
            return View(tb_MemberMaster);
        }

        //// GET: tb_MemberMaster
        //public ActionResult MembersByCollege(string sortOrder, string searchString, int? page, int? CollegeID)
        //{
        //    var tb_MemberMasters = db.tb_MemberMaster.Include(t => t.tb_Department).Include(t => t.tb_SemesterTaught);

        //    tb_MemberMasters.Select(t => t.tb_Department.tb_College);
        //    tb_MemberMasters.Select(t => t.tb_SemesterTaught);
        //    //tb_MemberMasters = tb_MemberMasters.Where(t => t.tb_SemesterTaught.CollegeID == CollegeID);
        //    ViewData["MemberQty"] = tb_MemberMasters.Count();

        //    ViewBag.NameSortParm = String.IsNullOrEmpty(sortOrder) ? "Name desc" : "";

        //    //Searching @ Filtering
        //    if (!String.IsNullOrEmpty(searchString))
        //    {
        //        tb_MemberMasters = tb_MemberMasters.Where(s => s.LastName.ToUpper().Contains(searchString.ToUpper())
        //                               || s.FirstName.ToUpper().Contains(searchString.ToUpper()));
        //    }
        //    if (CollegeID != null && CollegeID != 0)
        //    {
        //        tb_MemberMasters = tb_MemberMasters.Where(f => f.tb_Department.CollegeID == CollegeID);
        //        tb_MemberMaster tb_MemberMaster = db.tb_MemberMaster.FirstOrDefault();
        //        var tb_College = db.tb_College.Where(f => f.CollegeID == CollegeID).FirstOrDefault();;
        //        ViewBag.CollegeName = tb_College.CollegeDesc;
        //    }
        //    //List<tb_College> colleges = db.tb_College.ToList();
        //    //Sorting
        //    switch (sortOrder)
        //    {
        //        case "Name desc":
        //            tb_MemberMasters = tb_MemberMasters.OrderByDescending(s => s.LastName);
        //            break;
        //        default:
        //            tb_MemberMasters = tb_MemberMasters.OrderBy(s => s.LastName);
        //            break;
        //    }

        //    //Paging
        //    int pageSize = 3;
        //    int pageNumber = (page ?? 1);

        //    return View(tb_MemberMasters.ToPagedList(pageNumber, pageSize));
        //}


        //// GET: tb_MemberMaster/Details/5
        //public ActionResult Details(int? id)
        //{
        //    if (id == null)
        //    {
        //        return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        //    }
        //    tb_MemberMaster tb_MemberMaster = db.tb_MemberMaster.Find(id);
        //    if (tb_MemberMaster == null)
        //    {
        //        return HttpNotFound();
        //    }
        //    return View(tb_MemberMaster);
        //}

        //// GET: tb_MemberMaster/Create
        //public ActionResult Create()
        //{
        //    ViewBag.AreaID = new SelectList(db.tb_Area, "AreaID", "AreaName");
        //    ViewBag.DepartmentID = new SelectList(db.tb_Department, "DepartmentID", "DepartmentName");
        //    ViewBag.DuesID = new SelectList(db.tb_Dues, "DuesID", "DuesName");
        //    ViewBag.LatestUnionAssessmentID = new SelectList(db.tb_LatestUnionAssessment, "LatestUnionAssessmentID", "LatestUnionAssessmentDesc");
        //    ViewBag.PhoneRecID = new SelectList(db.tb_MemberPhoneNumbers, "PhoneRecID", "PhoneType");
        //    ViewBag.GenderID = new SelectList(db.tb_Gender, "GenderID", "GenderName");
        //    return View();
        //}

        //// POST: tb_MemberMaster/Create
        //// To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        //// more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public ActionResult Create([Bind(Include = "MemberID,CollegeID,DivisionID,LastName,FirstName,MiddleName,DepartmentID,AreaID,CopeStatus,CopeAmount,Counselors,CampaignVolunteer,LatestUnionAssessmentID,MailCodeID,DuesCategoryEffDate,UnionInitiationDate,HireDate,DateOfBirth,GenderID,RetiredEffDate,DeactivateEffDate,DeactivateReasonID,LeadershipPositionID,PoliticalAssessmentID,ParticipatePolitical,PoliticalActivitiesID,MemberAddressID,PhoneRecID,DuesID,AddedBy,AddedDateTime,ModifiedBy,ModifiedDateTime")] tb_MemberMaster tb_MemberMaster)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        db.tb_MemberMaster.Add(tb_MemberMaster);
        //        db.SaveChanges();
        //        return RedirectToAction("Index");
        //    }

        //    ViewBag.AreaID = new SelectList(db.tb_Area, "AreaID", "AreaName", tb_MemberMaster.AreaID);
        //    ViewBag.DepartmentID = new SelectList(db.tb_Department, "DepartmentID", "DepartmentName", tb_MemberMaster.DepartmentID);
        //    ViewBag.DuesID = new SelectList(db.tb_Dues, "DuesID", "DuesName", tb_MemberMaster.DuesID);
        //    ViewBag.LatestUnionAssessmentID = new SelectList(db.tb_LatestUnionAssessment, "LatestUnionAssessmentID", "LatestUnionAssessmentDesc", tb_MemberMaster.LatestUnionAssessmentID);
        //    ViewBag.PhoneRecID = new SelectList(db.tb_MemberPhoneNumbers, "PhoneRecID", "PhoneType", tb_MemberMaster.PhoneRecID);
        //    ViewBag.GenderID = new SelectList(db.tb_Gender, "GenderID", "GenderName", tb_MemberMaster.GenderID);
        //    return View(tb_MemberMaster);
        //}

        //// GET: tb_MemberMaster/Delete/5
        //public ActionResult Delete(int? id)
        //{
        //    if (id == null)
        //    {
        //        return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        //    }
        //    tb_MemberMaster tb_MemberMaster = db.tb_MemberMaster.Find(id);
        //    if (tb_MemberMaster == null)
        //    {
        //        return HttpNotFound();
        //    }
        //    return View(tb_MemberMaster);
        //}

        //// POST: tb_MemberMaster/Delete/5
        //[HttpPost, ActionName("Delete")]
        //[ValidateAntiForgeryToken]
        //public ActionResult DeleteConfirmed(int id)
        //{
        //    tb_MemberMaster tb_MemberMaster = db.tb_MemberMaster.Find(id);
        //    db.tb_MemberMaster.Remove(tb_MemberMaster);
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
