using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Proyect_P3.Models;
using Proyect_P3.Models.ViewModels;

namespace Proyect_P3.Controllers
{
    public class UserController : Controller
    {
        // GET: User
        public ActionResult Query()
        {

            List<QueryViewModels> lst = null;

            using (DBMVCEntities db = new DBMVCEntities()) 
            {
                //Select * from USERS where mSTATUS_id = 1 Order by Email

                //relaizmos el linq para consultar la DB
                lst = (from d in  db.USERS 
                       where d.idEstatus == 1
                       orderby d.Email

                       // llenamos el modelo que se llama QueryViewModels
                       select new QueryViewModels 
                       {
                           _Email = d.Email,
                           _Edad = d.Edad,
                           _id = d.ID,
                       }).ToList();
            }

            return View(lst); // aqui envio la data de la DB a la vista
        }
    }
}