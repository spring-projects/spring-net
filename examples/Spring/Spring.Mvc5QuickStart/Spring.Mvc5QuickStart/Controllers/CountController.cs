using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Spring.Mvc5QuickStart.Controllers
{
    public class CountController : Controller
    {
        private readonly Counter _counter;

        public CountController(Counter counter)
        {
            _counter = counter;
        }

        public ActionResult Index()
        {
            ViewBag.Message = _counter.Count;

            return View();
        }
    }
}
