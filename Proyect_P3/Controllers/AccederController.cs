using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Proyect_P3.Models;

namespace Proyect_P3.Controllers
{
    public class AccederController : Controller
    {
        // GET: Acceder
        public ActionResult Login()
        {
            return View();
        }

        public ActionResult Enter(String Usuario, string Password)
        {
            using (DBMVCEntities db = new DBMVCEntities())
            {
                var read = from d in db.USERS
                           where d.Email == Usuario
                           && d.Password == Password
                           && d.idEstatus == 1
                           select d;

                if (read.Count() > 0)
                {
                    Session["Usuario"] = read.First();
                    // Solo devuelve "1" para indicar éxito
                    return Content("1");
                }
                else
                {
                    return Content("Revisa el Usario y la Contraseña :(");
                }
            }
        }
    }
}