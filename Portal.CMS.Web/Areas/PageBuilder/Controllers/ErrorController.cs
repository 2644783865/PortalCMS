﻿using System.Web.Mvc;

namespace Portal.CMS.Web.Areas.PageBuilder.Controllers
{
    public class ErrorController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult NotFound()
        {
            return View();
        }

        public ActionResult SignedOut()
        {
            return View();
        }
    }
}