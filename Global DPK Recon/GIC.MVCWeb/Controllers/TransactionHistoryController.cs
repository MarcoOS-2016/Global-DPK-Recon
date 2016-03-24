using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace GIC.MVCWeb.Controllers
{
    public class TransactionHistoryController : Controller
    {
        //
        // GET: /DataQuery/

        public ActionResult Index()
        {
            ViewBag.Message = "This is Data Query web page!";

            List<SelectListItem> list = new List<SelectListItem>
            {
                new SelectListItem { Text = "C10000", Value = "1"},
                new SelectListItem { Text = "C40000", Value = "2"},
                new SelectListItem { Text = "C60000", Value = "3"},
                new SelectListItem { Text = "BRH", Value = "4"},
                new SelectListItem { Text = "DAO", Value = "5"},
                new SelectListItem { Text = "E10000", Value = "6"},
                new SelectListItem { Text = "I10000", Value = "7"},
                new SelectListItem { Text = "M10000", Value = "8"}
            };

            ViewData["ccnlist"] = new SelectList(list);

            return View();
        }

        //
        // GET: /DataQuery/Details/5

        public ActionResult Details(int id)
        {
            return View();
        }        
    }
}
