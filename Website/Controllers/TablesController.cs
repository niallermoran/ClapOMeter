using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Website.Controllers
{
    public class TablesController : Controller
    {

        public ActionResult Index()
        {
            ViewBag.Title = "Raw Sound Data";

            return View();
        }

    }
}
