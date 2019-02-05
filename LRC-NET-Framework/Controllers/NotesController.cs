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

namespace LRC_NET_Framework.Controllers
{
    public class NotesController : Controller
    {
        private LRCEntities db = new LRCEntities();

        // GET: Notes
        [Authorize(Roles = "admin, organizer")]
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
        [Authorize(Roles = "admin, organizer")]
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
        [Authorize(Roles = "admin, organizer")]
        public ActionResult AddNote(int? id)
        {
            //id = 1; // test REMOVE IT
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            AddNoteModel model = new AddNoteModel()
            {
                _MemberID = id?? 0,
                _NoteDate = DateTime.Now,
                _NoteTypeID = 1,
                _NoteTypes = new SelectList(db.tb_NoteType, "NoteTypeID", "NoteType"),
                _MemberNotes = db.tb_MemberNotes.Where(t => t.MemberID == id).ToList()
            };
            ViewBag._TakenBy = new SelectList(db.AspNetUsers.OrderBy(s => s.LastFirstName), "Id", "LastFirstName");
            tb_MemberMaster fm = db.tb_MemberMaster.Find(id);
            ViewBag.MemberName = fm.FirstName + " " + fm.LastName;

            return View(model);

        }

        // POST: Notes/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "admin, organizer")]
        public ActionResult AddNote([Bind(Include = "_MemberID,_Note,_NoteDate,_NoteTypeID,_TakenBy")]AddNoteModel model)
        {
            Error error = new Error();
            error.errCode = ErrorDetail.Success;
            error.errMsg = ErrorDetail.GetMsg(error.errCode);
            List<string> errs = new List<string>();
            try
            {
                tb_MemberNotes memberNote = new tb_MemberNotes()
                {
                    MemberID = model._MemberID,
                    Notes = model._Note,
                    NoteDate = model._NoteDate,
                    NoteTypeID = model._NoteTypeID,
                    TakenBy = model._TakenBy,
                    AddedDateTime = DateTime.UtcNow
                };
                db.tb_MemberNotes.Add(memberNote);

                model._NoteTypes = new SelectList(db.tb_NoteType, "NoteTypeID", "NoteType");
                ViewBag._TakenBy = new SelectList(db.AspNetUsers.OrderBy(s => s.LastFirstName), "Id", "LastFirstName");
                tb_MemberMaster fm = db.tb_MemberMaster.Find(model._MemberID);
                ViewBag.MemberName = fm.FirstName + " " + fm.LastName;
                model._MemberNotes = db.tb_MemberNotes.Where(t => t.MemberID == model._MemberID).ToList(); //before
                db.SaveChanges();
                model._MemberNotes = db.tb_MemberNotes.Where(t => t.MemberID == model._MemberID).ToList(); //after
            }
            catch (Exception ex)
            {
                error.errCode = ErrorDetail.DataImportError;
                error.errMsg = ErrorDetail.GetMsg(error.errCode);
                errs.Add("Error #" + error.errCode.ToString() + "!" + error.errMsg + ". " + ex.Message);
                ViewData["ErrorList"] = errs;
                return View(model);
            }

            return View(model);
        }


        // GET: Assessment/NotSure
        [Authorize(Roles = "admin, organizer")]
        public ActionResult NotSure()
        {
            return PartialView("NotSure");
        }

        // GET: Notes/Edit/5
        [Authorize(Roles = "admin, organizer")]
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
        [Authorize(Roles = "admin, organizer")]
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
        [Authorize(Roles = "admin, organizer")]
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
        [Authorize(Roles = "admin, organizer")]
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
