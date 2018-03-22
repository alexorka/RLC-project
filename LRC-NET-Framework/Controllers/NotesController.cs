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
    public class NotesController : Controller
    {
        private LRCEntities db = new LRCEntities();

        // GET: Notes
        public ActionResult Index(string sortOrder, string searchString, int? page)
        {
            var MemberNotes = db.tb_MemberNotes.Include(t => t.tb_NoteType).Include(t => t.tb_MemberMaster);
            ViewBag.NameSortParm = String.IsNullOrEmpty(sortOrder) ? "Date" : "";
            //Searching @ Filtering
            if (!String.IsNullOrEmpty(searchString))
            {
                MemberNotes = MemberNotes.Where(s => s.tb_MemberMaster.LastName.ToUpper().Contains(searchString.ToUpper())
                                       || s.tb_MemberMaster.FirstName.ToUpper().Contains(searchString.ToUpper()));
            }
            //Sorting
            switch (sortOrder)
            {
                case "Name desc":
                    //Activities = Activities.OrderByDescending(s => s.ActivityName);
                    break;
                case "Date":
                    MemberNotes = MemberNotes.OrderBy(s => s.AddedDateTime);
                    break;
                //case "Date desc":
                //    tb_MemberMasters = tb_MemberMasters.OrderByDescending(s => s.HireDate);
                //    break;
                default:
                    MemberNotes = MemberNotes.OrderBy(s => s.AddedDateTime);
                    break;
            }

            //Paging
            int pageSize = 3;
            int pageNumber = (page ?? 1);

            ViewData["MemberQty"] = MemberNotes.Count();


            return View(MemberNotes.ToPagedList(pageNumber, pageSize));
            //return View(MemberNotes.ToList());
        }

        // GET: Notes/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            tb_MemberNotes tb_MemberNotes = db.tb_MemberNotes.Find(id);
            if (tb_MemberNotes == null)
            {
                return HttpNotFound();
            }
            return View(tb_MemberNotes);
        }

        // GET: Notes/Create
        public ActionResult Create()
        {
            ViewBag.NoteTypeID = new SelectList(db.tb_NoteType, "NoteTypeID", "NoteType");
            ViewBag.MemberID = new SelectList(db.tb_MemberMaster, "MemberID", "LastName");
            return View();
        }

        // POST: Notes/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "MemberNotesID,MemberID,Notes,NoteTypeID,NoteDate,TakenBy,AddedBy,AddedDateTime,ModifiedBy,ModifiedDateTime")] tb_MemberNotes tb_MemberNotes)
        {
            if (ModelState.IsValid)
            {
                db.tb_MemberNotes.Add(tb_MemberNotes);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.NoteTypeID = new SelectList(db.tb_NoteType, "NoteTypeID", "NoteType", tb_MemberNotes.NoteTypeID);
            ViewBag.MemberID = new SelectList(db.tb_MemberMaster, "MemberID", "LastName", tb_MemberNotes.MemberID);
            return View(tb_MemberNotes);
        }

        // GET: Notes/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            tb_MemberNotes tb_MemberNotes = db.tb_MemberNotes.Find(id);
            if (tb_MemberNotes == null)
            {
                return HttpNotFound();
            }
            ViewBag.NoteTypeID = new SelectList(db.tb_NoteType, "NoteTypeID", "NoteType", tb_MemberNotes.NoteTypeID);
            ViewBag.MemberID = new SelectList(db.tb_MemberMaster, "MemberID", "LastName", tb_MemberNotes.MemberID);
            return View(tb_MemberNotes);
        }

        // POST: Notes/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "MemberNotesID,MemberID,Notes,NoteTypeID,NoteDate,TakenBy,AddedBy,AddedDateTime,ModifiedBy,ModifiedDateTime")] tb_MemberNotes tb_MemberNotes)
        {
            if (ModelState.IsValid)
            {
                db.Entry(tb_MemberNotes).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.NoteTypeID = new SelectList(db.tb_NoteType, "NoteTypeID", "NoteType", tb_MemberNotes.NoteTypeID);
            ViewBag.MemberID = new SelectList(db.tb_MemberMaster, "MemberID", "LastName", tb_MemberNotes.MemberID);
            return View(tb_MemberNotes);
        }

        // GET: Notes/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            tb_MemberNotes tb_MemberNotes = db.tb_MemberNotes.Find(id);
            if (tb_MemberNotes == null)
            {
                return HttpNotFound();
            }
            return View(tb_MemberNotes);
        }

        // POST: Notes/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            tb_MemberNotes tb_MemberNotes = db.tb_MemberNotes.Find(id);
            db.tb_MemberNotes.Remove(tb_MemberNotes);
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
