using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Proyect_P3.Controllers
{
    public class CerrarSeccionController : Controller
    {
        // GET: CerrarSeccion
        public ActionResult Logoff()
        {
            Session["Usuario"] = null;
            return RedirectToAction("Login","Acceder");
        }
    }
}