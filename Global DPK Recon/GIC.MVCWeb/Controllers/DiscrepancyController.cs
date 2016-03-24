using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace GIC.MVCWeb.Controllers
{
    public class DiscrepancyController : Controller
    {
        //
        // GET: /Admin/

        public ActionResult Index()
        {
            ViewBag.Message = "This is Admin web page!";
            
            return View();
        }

    }
}
