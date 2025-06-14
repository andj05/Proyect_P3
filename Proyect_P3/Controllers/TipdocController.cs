using iTextSharp.text;
using iTextSharp.text.pdf;
using Proyect_P3.Models; 
using Proyect_P3.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using Proyect_P3.Utilities; 
using System.Linq;
using System.Web.Mvc;

namespace Proyect_P3.Controllers
{
    public class TipdocController : Controller
    {
        // GET: Tipdoc
        public ActionResult TipdocQuery()
        {
            List<TipdocQueryViewModels> lst = null;

            using (DBMVCEntities db = new DBMVCEntities())
            {
                // Realizamos el LINQ para consultar la DB
                lst = (from d in db.TIPDOCs
                       where d.ESTATUS == 1
                       orderby d.DESCRIPCION
                       select new TipdocQueryViewModels
                       {
                           Id = d.ID,
                           TipoDoc = d.TIPDOC1,
                           Descripcion = d.DESCRIPCION,
                           Origen = d.ORIGEN,
                           Contador = d.CONTADOR
                       }).ToList();
            }

            return View(lst); // Enviamos la data de la DB a la vista
        }

        [HttpGet]
        public ActionResult Add()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Add(AddTipdocViewModels model)
        {
            if (!ModelState.IsValid) // Validamos el modelo
            {
                return View(model);
            }

            using (var db = new DBMVCEntities())
            {
                // Obtener el último contador para el tipo de documento seleccionado
                var lastCounter = db.TIPDOCs
                    .Where(t => t.TIPDOC1 == model.TipoDoc)
                    .OrderByDescending(t => t.CONTADOR)
                    .Select(t => t.CONTADOR)
                    .FirstOrDefault();

                // Validar el formato del último contador
                int nextCounter = 1; // Valor inicial por defecto
                if (!string.IsNullOrEmpty(lastCounter) && lastCounter.Length > 3)
                {
                    // Extraer el número del contador y sumarle 1
                    if (int.TryParse(lastCounter.Substring(3), out int parsedCounter))
                    {
                        nextCounter = parsedCounter + 1;
                    }
                }

                // Generar el nuevo contador
                model.Contador = $"{model.TipoDoc}{nextCounter:D4}"; // Formato: FAC0001, REC0001, etc.

                // Crear un objeto de tipo TIPDOC
                TIPDOC tipdoc = new TIPDOC
                {
                    TIPDOC1 = model.TipoDoc,
                    ORIGEN = model.Origen,
                    DESCRIPCION = model.Descripcion,
                    CONTADOR = model.Contador, // Contador generado automáticamente
                    CTADEBITO = model.CuentaDebito,
                    CTACREDITO = model.CuentaCredito,
                    ESTATUS = model.Estatus
                };

                // Agregar el registro a la base de datos
                db.TIPDOCs.Add(tipdoc);
                db.SaveChanges(); // Guardar los cambios
            }

            // Redireccionar a la vista TipdocQuery
            return Redirect(Url.Content("~/Tipdoc/TipdocQuery"));
        }

        [HttpGet]
        public ActionResult Edit(int Id)
        {
            EditTipdocViewModels model = new EditTipdocViewModels();

            using (var db = new DBMVCEntities())
            {
                var tipdoc = db.TIPDOCs.Find(Id); // Buscamos el registro por ID

                model.Id = tipdoc.ID;
                model.TipoDoc = tipdoc.TIPDOC1;
                model.Origen = tipdoc.ORIGEN;
                model.Descripcion = tipdoc.DESCRIPCION;
                model.Contador = tipdoc.CONTADOR;
                model.CuentaDebito = tipdoc.CTADEBITO;
                model.CuentaCredito = tipdoc.CTACREDITO;
                model.Estatus = tipdoc.ESTATUS ?? 0;
            }

            return View(model); // Enviamos el modelo a la vista
        }

        [HttpPost]
        public ActionResult Edit(EditTipdocViewModels model)
        {
            if (!ModelState.IsValid) // Validamos el modelo
            {
                return View(model);
            }

            using (var db = new DBMVCEntities())
            {
                var tipdoc = db.TIPDOCs.Find(model.Id); // Buscamos el registro por ID

                // Actualizamos los campos
                tipdoc.TIPDOC1 = model.TipoDoc;
                tipdoc.ORIGEN = model.Origen;
                tipdoc.DESCRIPCION = model.Descripcion;
                tipdoc.CTADEBITO = model.CuentaDebito;
                tipdoc.CTACREDITO = model.CuentaCredito;
                tipdoc.ESTATUS = model.Estatus;

                db.Entry(tipdoc).State = System.Data.EntityState.Modified; // Marcamos como modificado
                db.SaveChanges(); // Guardamos los cambios
            }

            return Redirect(Url.Content("~/Tipdoc/TipdocQuery")); // Redireccionamos a la vista TipdocQuery
        }

        [HttpPost]
        public JsonResult Delete(int id)
        {
            try
            {
                using (var db = new DBMVCEntities())
                {
                    var tipdoc = db.TIPDOCs.Find(id);
                    if (tipdoc != null)
                    {
                        tipdoc.ESTATUS = 3; // Marcamos el registro como inactivo
                        db.Entry(tipdoc).State = System.Data.EntityState.Modified; // Marcamos como modificado
                        db.SaveChanges();
                        return Json("1"); 
                    }
                }
            }
            catch (Exception)
            {
                return Json("0"); 
            }

            return Json("0"); 
        }

        [HttpGet]
        public ActionResult Details(int id)
        {
            DetailsTipdocQueryViewModels model = null;

            using (var db = new DBMVCEntities())
            {
                var tipdoc = db.TIPDOCs.Find(id); // Buscamos el registro por ID
                if (tipdoc != null)
                {
                    model = new DetailsTipdocQueryViewModels
                    {
                        Id = tipdoc.ID,
                        TipoDoc = tipdoc.TIPDOC1,
                        Descripcion = tipdoc.DESCRIPCION,
                        Origen = tipdoc.ORIGEN,
                        Contador = tipdoc.CONTADOR,
                        CuentaDebito = tipdoc.CTADEBITO,
                        CuentaCredito = tipdoc.CTACREDITO,
                        Estatus = tipdoc.ESTATUS ?? 0
                    };
                }
            }

            if (model == null)
            {
                return HttpNotFound(); // Si no se encuentra el registro, devolvemos un error 404
            }

            return View(model); // Enviamos el modelo a la vista
        }

        [HttpGet]
        public ActionResult GeneratePdf(int id)
        {
            DetailsTipdocQueryViewModels model = null;

            using (var db = new DBMVCEntities())
            {
                var tipdoc = db.TIPDOCs.Find(id);
                if (tipdoc != null)
                {
                    model = new DetailsTipdocQueryViewModels
                    {
                        Id = tipdoc.ID,
                        TipoDoc = tipdoc.TIPDOC1,
                        Descripcion = tipdoc.DESCRIPCION,
                        Origen = tipdoc.ORIGEN,
                        Contador = tipdoc.CONTADOR,
                        CuentaDebito = tipdoc.CTADEBITO,
                        CuentaCredito = tipdoc.CTACREDITO,
                        Estatus = tipdoc.ESTATUS ?? 0
                    };
                }
            }

            if (model == null)
            {
                return HttpNotFound();
            }

            // Use PdfGenerator to generate the PDF
            var pdfGenerator = new PdfGenerator();
            var pdfBytes = pdfGenerator.GenerateFormattedPdf(model);

            var fileName = $"TipoDocumento_{model.TipoDoc}_{model.Id}_{DateTime.Now:yyyyMMdd_HHmmss}.pdf";
            return File(pdfBytes, "application/pdf", fileName);
        }

    }
}