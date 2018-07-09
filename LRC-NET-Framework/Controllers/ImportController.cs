using System;
using System.Collections.Generic;
using System.Data.Entity.Validation;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace LRC_NET_Framework.Controllers
{
    public class ImportController : Controller
    {
        private LRCEntities db = new LRCEntities();
        // GET: Import
        public ActionResult MemberImportErrors()
        {
            var tb_MemberError = db.tb_MemberError.ToList();
            return View(tb_MemberError);
        }

        // GET: Import
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
    }
}