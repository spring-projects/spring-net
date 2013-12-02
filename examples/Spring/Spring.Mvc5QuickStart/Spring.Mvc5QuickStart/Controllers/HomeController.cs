using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Spring.Mvc5QuickStart.Controllers
{
    public class HomeController : Controller
    {
        protected string Message { get; set; }

        public ActionResult Index()
        {
            ViewBag.Message = Message;

            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your app description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
    }
}
