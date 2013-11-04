using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using LETS.ViewModels;
using Orchard.UI.Admin;

namespace LETS.Controllers
{
    public class NoticesAdminController : Controller
    {
        [Admin]
        public ActionResult List(int id)
        {
            return View();
        }
    }
}
