using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace DIY_v2.Controllers
{
    public class PartialViewController : Controller
    {
        // GET: PartialView
        public ActionResult ToolPartial()
        {
            return PartialView("ToolPartial");
        }

        public ActionResult TaskerPartial()
        {
            return PartialView("TaskerPartial");
        }

        public ActionResult MemberRegister()
        {
            return View();
        }

        public ActionResult TaskerRegister()
        {
            return View();
        }

        public ActionResult EditTasker()
        {
            return View();
        }
        public ActionResult Messageboard()
        {
            return View();
        }


    }
}