using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace DIY_v2.Controllers
{
    public class HomeController : Controller
    {
        // GET: Home
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult MyDIY()
        {
            return View();
        }
       
        public ActionResult MyDIYChangeLight_2()
        {
            return View();
        }
        public ActionResult MyDIYChangFaucet()
        {
            return View();
        }

        public ActionResult MyDIYChangeLight_1()
        {
            return View();
        }

        public ActionResult MyDIYChangePipe_1()
        {
            return View();
        }
        public  ActionResult test()
        {
            return View();
        }
      
    }
}
