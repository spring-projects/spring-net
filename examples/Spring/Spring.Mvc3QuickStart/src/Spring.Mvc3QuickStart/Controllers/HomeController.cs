using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Spring.Mvc3QuickStart.Controllers
{
    public class HomeController : Controller
    {
        public string Message { get; set; }

        public ActionResult Index()
        {
            //simple asp.net mvc sample app hard-coded message commented out b/c we
            // can property-inject it via the container instead
            // (gratuitous container usage trick, but demonstrates that the container
            //  is resolving HomeController)
            
            //ViewData["Message"] = "Welcome to ASP.NET MVC!";

            //set the message value in the viewdata bag based on the property value
            // as injected into the controller from the container
            ViewData["Message"] = Message;

            return View();
        }

        public ActionResult About()
        {
            return View();
        }
    }
}
