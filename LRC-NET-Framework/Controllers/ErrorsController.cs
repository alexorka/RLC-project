using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace LRC_NET_Framework.Controllers
{
    public class ErrorsController : Controller
    {
        // GET: Errors
        public ActionResult Index(string name, string description)
        {
            List<string> errs = new List<string>();
            errs.Add(name + "!" + description);

            ViewData["ErrorList"] = errs;
            return View();
        }
    }
}