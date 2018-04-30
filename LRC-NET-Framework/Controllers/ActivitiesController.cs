﻿using System;
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
                    //Activities = Activities.OrderByDescending(s => s.ActivityName);
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
            int pageSize = 3;
            int pageNumber = (page ?? 1);

            ViewData["MemberQty"] = Activities.Count();


            return View(Activities.ToPagedList(pageNumber, pageSize));
        }

        //// GET: Activities/Details/5
        //public ActionResult Details(int? id)
        //{
        //    if (id == null)
        //    {
        //        return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        //    }
        //    tb_Activity tb_Activity = db.tb_Activity.Find(id);
        //    if (tb_Activity == null)
        //    {
        //        return HttpNotFound();
        //    }
        //    return View(tb_Activity);
        //}

        // GET: Activities/Create
        [Authorize(Roles = "admin, organizer")]
        public ActionResult AddActivity(int? id)
        {
            id = 1;
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            tb_MemberActivity tb_MemberActivity = db.tb_MemberActivity.Find(id);
            if (tb_MemberActivity == null)
            {
                return HttpNotFound();
            }
            ViewBag.ActivityID = new SelectList(db.tb_Activity, "ActivityID", "ActivityName", tb_MemberActivity.ActivityID);
            ViewBag.Activities = db.tb_MemberActivity.ToList();
            ViewBag.ActivityStatusID = db.tb_ActivityStatus.ToList();
            return View(tb_MemberActivity);
        }

        // POST: Activities/AddActivity
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "admin, organizer")]
        public ActionResult AddActivity([Bind(Include = "MemberID,ActivityID,ActivityStatusID,ActivityDate,ActivityName,ActivityNote")] tb_MemberActivity tb_MemberActivity,
            string ActivityNote, DateTime? ActivityDate, FormCollection formCollection)
        {
            tb_Activity activity = db.tb_Activity.Find(tb_MemberActivity.ActivityID);
            //DateTime myDate = DateTime.Parse(formCollection["tb_Activity.ActivityDate"]);
            activity.ActivityDate = DateTime.Parse(formCollection["tb_Activity.ActivityDate"]);
            activity.ActivityNote = formCollection["tb_Activity.ActivityNote"];

            if (ModelState.IsValid)
            {
                db.tb_MemberActivity.Add(tb_MemberActivity);
                db.SaveChanges();

                db.tb_Activity.Attach(activity);
                db.SaveChanges();

                return RedirectToAction("AddActivity");
            }

            return View(tb_MemberActivity);
        }


        // GET: Assessment/NotSure
        [Authorize(Roles = "admin, organizer")]
        public ActionResult NotSure()
        {
            return PartialView("NotSure");
        }

        //// GET: Activities/Edit/5
        //public ActionResult Edit(int? id)
        //{
        //    if (id == null)
        //    {
        //        return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        //    }
        //    tb_Activity tb_Activity = db.tb_Activity.Find(id);
        //    if (tb_Activity == null)
        //    {
        //        return HttpNotFound();
        //    }
        //    return View(tb_Activity);
        //}

        //// POST: Activities/Edit/5
        //// To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        //// more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public ActionResult Edit([Bind(Include = "ActivityID,ActivityStatusID,ActivityDate,ActivityName,ActivityNote,AddedBy,AddedDateTime,ModifiedBy,ModifiedDateTime")] tb_Activity tb_Activity)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        db.Entry(tb_Activity).State = EntityState.Modified;
        //        db.SaveChanges();
        //        return RedirectToAction("Index");
        //    }
        //    return View(tb_Activity);
        //}

        //// GET: Activities/Delete/5
        //public ActionResult Delete(int? id)
        //{
        //    if (id == null)
        //    {
        //        return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        //    }
        //    tb_Activity tb_Activity = db.tb_Activity.Find(id);
        //    if (tb_Activity == null)
        //    {
        //        return HttpNotFound();
        //    }
        //    return View(tb_Activity);
        //}

        //// POST: Activities/Delete/5
        //[HttpPost, ActionName("Delete")]
        //[ValidateAntiForgeryToken]
        //public ActionResult DeleteConfirmed(int id)
        //{
        //    tb_Activity tb_Activity = db.tb_Activity.Find(id);
        //    db.tb_Activity.Remove(tb_Activity);
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
