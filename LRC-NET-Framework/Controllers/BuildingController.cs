using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using LRC_NET_Framework;
using LRC_NET_Framework.Models;

namespace LRC_NET_Framework.Controllers
{
    public class BuildingController : Controller
    {
        private LRCEntities db = new LRCEntities();

        // GET: Building
        public ActionResult Index()
        {
            var tb_Building = db.tb_Building.Include(t => t.tb_Campus).OrderBy(c =>c.tb_Campus.CampusName).ThenBy(b => b.BuildingName);
            return View(tb_Building.ToList());
        }


        // GET: Home/AddBuilding
        public ActionResult AddBuilding()
        {
            AddBuildingModel buildingModel = new AddBuildingModel
            {
                _Campuses = new SelectList(db.tb_Campus.OrderBy(s => s.CampusName), "CampusID", "CampusName"),
                _tb_College = db.tb_College
            };
            buildingModel._Colleges = new List<SelectListItem>
            { new SelectListItem() {Value = "0", Text = "-- Select One --" }}.Concat(db.tb_College.Select(x => new SelectListItem
            { Value = x.CollegeID.ToString(), Text = x.CollegeName }));

            return View(buildingModel);
        }

        // POST: Home/AddBuilding
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "admin, organizer")]
        public ActionResult AddBuilding(HttpPostedFileBase file, [Bind(Include = "_CollegeID,_CampusID,_BuildingName")] AddBuildingModel model)
        {
            Error error = new Error();
            error.errCode = ErrorDetail.Success;
            error.errMsg = ErrorDetail.GetMsg(error.errCode);
            List<string> errs = new List<string>();

            model._Colleges = new List<SelectListItem>
            { new SelectListItem() {Value = "0", Text = "-- Select One --" }}.Concat(db.tb_College.Select(x => new SelectListItem
            { Value = x.CollegeID.ToString(), Text = x.CollegeName }));
            model._Campuses = new SelectList(db.tb_Campus.OrderBy(s => s.CampusName), "CampusID", "CampusName", model._CampusID);
            model._tb_College = db.tb_College;

            if (file != null && file.ContentLength > 0)
                try
                {
                    var extension = file.FileName.Split('.').Last().ToUpper();
                    if (extension != "PDF" && extension != "JPG")
                    {
                        ViewBag.Message = "Selected file type is not PDF or JPEG";
                        return View(model);
                    }

                    string path = Path.Combine(Server.MapPath(MvcApplication.BuildingsFolder), file.FileName);
                    file.SaveAs(path);
                    ViewBag.Message = "File uploaded successfully";
                }
                catch (Exception ex)
                {
                    ViewBag.Message = "ERROR:" + ex.Message.ToString();
                }
            else
            {
                ViewBag.Message = "You have not specified a file.";

                return View(model);
            }

            var buildings = db.tb_Building.Where(s => s.BuildingName.ToUpper() == model._BuildingName.ToUpper());
            //Check dublicates
            if (buildings.ToList().Count == 0)
            {
                tb_Building building = new tb_Building()
                {
                    CampusID = model._CampusID,
                    BuildingName = model._BuildingName,
                    ImagePath = file.FileName
                };
                db.tb_Building.Add(building);
            }
            else
            {
                tb_Building building = buildings.FirstOrDefault();
                building.CampusID = model._CampusID;
                building.BuildingName = model._BuildingName;
                building.ImagePath = file.FileName;
                db.tb_Building.Attach(building);
            }
            try
            {
                db.SaveChanges();
                model._tb_College = db.tb_College;
            }
            catch (Exception ex)
            {
                error.errCode = ErrorDetail.DataImportError;
                error.errMsg = ErrorDetail.GetMsg(error.errCode);
                errs.Add("Error #" + error.errCode.ToString() + "!" + error.errMsg + ". " + ex.Message);
                ViewData["ErrorList"] = errs;
                return View(model);
            }
            model._tb_College = db.tb_College;
            return View(model);
        }
        // GET: Building/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            tb_Building tb_Building = db.tb_Building.Find(id);
            if (tb_Building == null)
            {
                return HttpNotFound();
            }
            ViewBag.CampusID = new SelectList(db.tb_Campus.OrderBy(c => c.CollegeCode).ThenBy(c => c.CampusName), "CampusID", "CampusName", tb_Building.CampusID);
            return View(tb_Building);
        }

        // POST: Building/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "BuildingID,BuildingName,CampusID,ImagePath")] tb_Building model, HttpPostedFileBase file)
        {
            ViewBag.CampusID = new SelectList(db.tb_Campus, "CampusID", "CampusName", model.CampusID);
            bool isFileSelected = false;
            if (file != null && file.ContentLength > 0)
                try
                {
                    var extension = file.FileName.Split('.').Last().ToUpper();
                    if (extension != "PDF" && extension != "JPG")
                    {
                        ViewBag.Message = "Selected file type is not PDF or JPEG";
                        return View(model);
                    }

                    string path = Path.Combine(Server.MapPath(MvcApplication.BuildingsFolder), file.FileName);
                    file.SaveAs(path);
                    ViewBag.Message = "File uploaded successfully";
                    isFileSelected = true;
                }
                catch (Exception ex)
                {
                    ViewBag.Message = "ERROR:" + ex.Message.ToString();
                    return View(model);
                }
            //else
            //{
            //    ViewBag.Message = "You have not specified a file";
            //    return View(model);
            //}

            if (ModelState.IsValid)
            {
                if (isFileSelected)
                    model.ImagePath = file.FileName;
                db.Entry(model).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(model);
        }

        // GET: Building/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            tb_Building tb_Building = db.tb_Building.Find(id);
            if (tb_Building == null)
            {
                return HttpNotFound();
            }
            var isPresent = db.tb_SemesterTaught.Any(b => b.BuildingID == id);
            if (isPresent)
            {
                ModelState.AddModelError("User Name Error", "This building can not be deleted from database because is presents in the current table of semester");
            }
            if (!ModelState.IsValid)
            {
                Error error = new Error();
                List<string> errs = new List<string>();
                foreach (var state in ModelState)
                    foreach (var err in state.Value.Errors)
                        error.errMsg += " " + err.ErrorMessage;
                errs.Add("Warnings!" + error.errMsg);
                ViewData["ErrorList"] = errs;
            }
            return View(tb_Building);
        }

        // POST: Building/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            var isPresent = db.tb_SemesterTaught.Any(b => b.BuildingID == id);
            if (!isPresent)
            {
                tb_Building tb_Building = db.tb_Building.Find(id);
                db.tb_Building.Remove(tb_Building);
                db.SaveChanges();
            }
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
