using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace SPPipAPi.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            ViewBag.Title = "Home Page this is how conntrond sdfdsfdsf s dsfsfdsfdf sdfdsf";

            return View();
        }
    }
}
