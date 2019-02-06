using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using LRC_NET_Framework;

namespace LRC_NET_Framework.Controllers
{
    public class CategoriesController : Controller
    {
        private LRCEntities db = new LRCEntities();

        // GET: Categories
        public ActionResult Index()
        {
            return View(db.tb_Categories.ToList());
        }

        // GET: Categories/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Categories/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "CategoryName")] tb_Categories tb_Categories)
        {
            if (ModelState.IsValid)
            {
                db.tb_Categories.Add(tb_Categories);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(tb_Categories);
        }

        // GET: Categories/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            tb_Categories tb_Categories = db.tb_Categories.Find(id);
            if (tb_Categories == null)
            {
                return HttpNotFound();
            }
            return View(tb_Categories);
        }

        // POST: Categories/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "CategoryID,CategoryName")] tb_Categories tb_Categories)
        {
            if (ModelState.IsValid)
            {
                db.Entry(tb_Categories).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(tb_Categories);
        }

        // GET: Categories/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            tb_Categories tb_Categories = db.tb_Categories.Find(id);
            if (tb_Categories == null)
            {
                return HttpNotFound();
            }
            return View(tb_Categories);
        }

        // POST: Categories/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            tb_Categories tb_Categories = db.tb_Categories.Find(id);
            db.tb_Categories.Remove(tb_Categories);
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
