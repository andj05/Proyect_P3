using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Services.Protocols;
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

        [HttpGet]
        public ActionResult Add()
        {
            return View(); 
        }

        [HttpPost]
        public ActionResult Add(AddUserViewModels model)
        {
            if (!ModelState.IsValid) // Si el modelo no es válido, regresa la vista con los errores
            {
                return View(model);
            }

            using (var db = new DBMVCEntities())
            {
                // Creamos un objeto de tipo USER
                USER user = new USER
                {
                    Nombre = model.Nombre,
                    Usuario = model.Usuario, 
                    Email = model.Email,
                    Password = model.Password,
                    Edad = model.Edad,
                    idEstatus = 1 // 1 es el estatus activo
                };

                // Agregamos el usuario a la base de datos
                db.USERS.Add(user);
                db.SaveChanges(); // Guardamos los cambios en la base de datos
            }

            // Redireccionamos a la vista Query
            return Redirect(Url.Content("~/User/Query"));
        }

        [HttpGet]
        public ActionResult Edit(int Id)
        {
            EditUserViewModels model = new EditUserViewModels();

            using (var db = new DBMVCEntities()) 
            {
                var user = db.USERS.Find(Id); // buscamos el usuario por ID

                model.Nombre = user.Nombre;
                model.Edad = user.Edad;
                model.Email = user.Email;
                model.Password = user.Password;
                model.Id =user.ID;

            }
                return View(model); // aqui enviamos el modelo a la vista
        }

        [HttpPost]
        public ActionResult Edit(EditUserViewModels model)
        {
            using (var db = new DBMVCEntities())
            {
                var oUser = db.USERS.Find(model.Id); // buscamos el usuario por ID

                // Verificamos si cada campo tiene un valor antes de actualizarlo
                if (!string.IsNullOrWhiteSpace(model.Email))
                {
                    oUser.Email = model.Email; // actualizamos el email
                }

                if (!string.IsNullOrWhiteSpace(model.Nombre))
                {
                    oUser.Nombre = model.Nombre; // actualizamos el nombre
                }

                if (model.Edad.HasValue)
                {
                    oUser.Edad = model.Edad.Value; // actualizamos la edad
                }

                if (!string.IsNullOrWhiteSpace(model.Password))
                {
                    oUser.Password = model.Password; // actualizamos la contraseña
                }

                db.Entry(oUser).State = System.Data.EntityState.Modified; // marcamos el usuario como modificado
                db.SaveChanges(); // guardamos los cambios
            }

            return Redirect(Url.Content("~/User/Query")); // redireccionamos a la vista Query
        }

        [HttpPost]
        public JsonResult Delete(int id)
        {
            try
            {
                using (var db = new DBMVCEntities())
                {
                    var ouser = db.USERS.Find(id);
                    if (ouser != null)
                    {
                        ouser.idEstatus = 3; // Marca el registro como eliminado
                        db.Entry(ouser).State = System.Data.EntityState.Modified; // Marca como modificado
                        db.SaveChanges();
                        return Json("1"); // Éxito
                    }
                }
            }
            catch (Exception)
            {
                return Json("0"); // Error
            }

            return Json("0"); // Error por defecto
        }

    }
}