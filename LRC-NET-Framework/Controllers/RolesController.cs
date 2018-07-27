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
    public class RolesController : Controller
    {
        private LRCEntities db = new LRCEntities();

        // GET: Roles
        [Authorize(Roles = "admin, organizer")]
        public ActionResult Index(string sortOrder, string searchString, int? page)
        {
            var Roles = db.tb_Roles.Include(t => t.tb_MemberRoles).Where(t => t.tb_MemberRoles.Count != 0);

            ViewBag.NameSortParm = String.IsNullOrEmpty(sortOrder) ? "Name desc" : "";
            //Searching @ Filtering
            if (!String.IsNullOrEmpty(searchString))
            {
                Roles = Roles.Where(s => s.RoleName.ToUpper().Contains(searchString.ToUpper()));
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
                    Roles = Roles.OrderBy(s => s.RoleName);
                    break;
            }

            //Paging
            int pageSize = 5;
            int pageNumber = (page ?? 1);

            ViewData["MemberQty"] = Roles.Count();


            return View(Roles.ToPagedList(pageNumber, pageSize));
            //return View(db.tb_Roles.ToList());
        }

        // GET: Roles/Details/5
        [Authorize(Roles = "admin, organizer")]
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            tb_Roles tb_Roles = db.tb_Roles.Find(id);
            if (tb_Roles == null)
            {
                return HttpNotFound();
            }
            return View(tb_Roles);
        }

        // GET: Roles/CreateMemberRoles
        [Authorize(Roles = "admin, organizer")]
        public ActionResult CreateMemberRoles()
        {
            var members = db.tb_MemberMaster;
            List<object> memberList = new List<object>();
            foreach (var member in members)
                memberList.Add(new
                {
                    Id = member.MemberID,
                    Name = member.LastName + ", " + member.FirstName
                });

            CreateRoleModel model = new CreateRoleModel()
            {
                _MemberID = 0,
                _Members = new SelectList(memberList, "Id", "Name"),
                _RoleID = 0,
                _Roles = new SelectList(db.tb_Roles, "RoleID", "RoleName"),
                _BodyID = 0,
                _Bodies = new SelectList(db.tb_Body, "BodyID", "BodyName"),
                _StartDate = DateTime.Now,
                _EndDate = DateTime.Now
            };

            return View(model);
        }

        // POST: Roles/CreateMemberRoles
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "admin, organizer")]
        public ActionResult CreateMemberRoles(CreateRoleModel model)
        {
            if (ModelState.IsValid)
            {
                tb_MemberRoles role = new tb_MemberRoles();
                role.MemberID = model._MemberID;
                role.RoleID = model._RoleID;
                role.BodyID = model._BodyID;
                role.StartDate = model._StartDate;
                role.EndDate = model._EndDate;

                db.tb_MemberRoles.Add(role);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(model);
        }

        // GET: Roles/CreateRole
        [Authorize(Roles = "admin, organizer")]
        public ActionResult CreateRole()
        {
            return View();
        }

        // POST: Roles/CreateRole
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "admin, organizer")]
        public ActionResult CreateRole([Bind(Include = "RoleID,RoleName")] tb_Roles Roles)
        {
            if (ModelState.IsValid)
            {
                db.tb_Roles.Add(Roles);
                db.SaveChanges();
                return RedirectToAction("CreateMemberRoles");
            }

            return View(Roles);
        }

        // GET: Roles/CreateBody
        [Authorize(Roles = "admin, organizer")]
        public ActionResult CreateBody()
        {
            return View();
        }

        // POST: Roles/CreateRole
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "admin, organizer")]
        public ActionResult CreateBody([Bind(Include = "BodyID,BodyName")] tb_Body body)
        {
            if (ModelState.IsValid)
            {
                db.tb_Body.Add(body);
                db.SaveChanges();
                return RedirectToAction("CreateMemberRoles");
            }

            return View(body);
        }

        // GET: Roles/Edit/5

        // GET: Notes/Create
        [Authorize(Roles = "admin, organizer")]
        public ActionResult AddRole(int? id)
        {
            //id = 1; // test REMOVE IT
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            CreateRoleModel model = new CreateRoleModel()
            {
                _MemberID = id ?? 0,
                _RoleID = 1,
                _Roles = new SelectList(db.tb_Roles, "RoleID", "RoleName"),
                _BodyID = 1,
                _Bodies = new SelectList(db.tb_Body, "BodyID", "BodyName"),
                _StartDate = DateTime.Now,
                _EndDate = DateTime.Now,
                _MemberRoles = db.tb_MemberRoles.Where(t => t.MemberID == id).ToList()
            };
            return View(model);
        }

        // POST: Notes/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "admin, organizer")]
        public ActionResult AddRole(CreateRoleModel model)
        {
            tb_MemberRoles memberRoles = new tb_MemberRoles()
            {
                MemberID = model._MemberID,
                RoleID = model._RoleID,
                BodyID = model._BodyID,
                StartDate = model._StartDate,
                EndDate = model._EndDate
            };
            db.tb_MemberRoles.Add(memberRoles);
            db.SaveChanges();
            model._Roles = new SelectList(db.tb_Roles, "RoleID", "RoleName");
            model._Bodies = new SelectList(db.tb_Body, "BodyID", "BodyName");
            model._MemberRoles = db.tb_MemberRoles.Where(t => t.MemberID == model._MemberID).ToList();
            return View(model);
        }

        [Authorize(Roles = "admin, organizer")]
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            tb_Roles tb_Roles = db.tb_Roles.Find(id);
            if (tb_Roles == null)
            {
                return HttpNotFound();
            }
            return View(tb_Roles);
        }

        // POST: Roles/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "admin, organizer")]
        public ActionResult Edit([Bind(Include = "RoleID,RoleName")] tb_Roles tb_Roles)
        {
            if (ModelState.IsValid)
            {
                db.Entry(tb_Roles).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(tb_Roles);
        }

        // GET: Roles/Delete/5
        [Authorize(Roles = "admin, organizer")]
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            tb_Roles tb_Roles = db.tb_Roles.Find(id);
            if (tb_Roles == null)
            {
                return HttpNotFound();
            }
            return View(tb_Roles);
        }

        // POST: Roles/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "admin, organizer")]
        public ActionResult DeleteConfirmed(int id)
        {
            tb_Roles tb_Roles = db.tb_Roles.Find(id);
            db.tb_Roles.Remove(tb_Roles);
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
