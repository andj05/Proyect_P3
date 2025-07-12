using Proyect_P3.Metodos;
using Proyect_P3.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Web.Mvc;

namespace Proyect_P3.Controllers
{
    public class TiposController : Controller
    {
        public ActionResult Tipos()
        {
            if (Session["Usuario"] == null)
                return RedirectToAction("Login", "Acceder");
            return View();
        }

        [HttpGet]
        public JsonResult ConsultaTipos()
        {
            System.Diagnostics.Debug.WriteLine("=== INICIANDO CONSULTA TIPOS ===");

            try
            {
                List<Tipos> oLista = new List<Tipos>();
                oLista = TiposMetodos.Instance.Listar();

                System.Diagnostics.Debug.WriteLine($"Método Listar retornó {oLista?.Count ?? 0} elementos");

                // Convertir imágenes a Base64 para la vista
                if (oLista != null && oLista.Count > 0)
                {
                    foreach (var tipo in oLista)
                    {
                        if (tipo.Imagen != null && tipo.Imagen.Length > 0)
                        {
                            // Crear propiedad temporal para JavaScript
                            ViewBag.ImagenBase64 = Convert.ToBase64String(tipo.Imagen);
                        }
                    }
                }

                return Json(new { data = oLista }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"EXCEPCIÓN en ConsultaTipos: {ex.Message}");
                return Json(new { data = new List<Tipos>(), error = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public JsonResult InsertarTipos(Tipos oCat, string ImagenBase64)
        {
            System.Diagnostics.Debug.WriteLine("=== INSERTAR TIPO CONTROLADOR ===");

            try
            {
                if (oCat == null)
                {
                    System.Diagnostics.Debug.WriteLine("ERROR: oCat es null");
                    return Json(new { respuesta = false, error = "Objeto nulo" }, JsonRequestBehavior.AllowGet);
                }

                System.Diagnostics.Debug.WriteLine($"IDTipo: {oCat.IDTipo}");
                System.Diagnostics.Debug.WriteLine($"Descripcion: '{oCat.Descripcion}'");
                System.Diagnostics.Debug.WriteLine($"Estatus: {oCat.Estatus}");
                System.Diagnostics.Debug.WriteLine($"ImagenBase64 recibida: {!string.IsNullOrEmpty(ImagenBase64)}");

                // Validar descripción
                if (string.IsNullOrWhiteSpace(oCat.Descripcion))
                {
                    System.Diagnostics.Debug.WriteLine("ERROR: Descripción vacía");
                    return Json(new { respuesta = false, error = "Descripción requerida" }, JsonRequestBehavior.AllowGet);
                }

                // Procesar imagen si existe
                if (!string.IsNullOrEmpty(ImagenBase64))
                {
                    try
                    {
                        oCat.Imagen = Convert.FromBase64String(ImagenBase64);
                        System.Diagnostics.Debug.WriteLine($"Imagen convertida: {oCat.Imagen.Length} bytes");
                    }
                    catch (Exception imgEx)
                    {
                        System.Diagnostics.Debug.WriteLine($"ERROR al convertir imagen: {imgEx.Message}");
                        oCat.Imagen = null;
                    }
                }

                // Establecer valores por defecto para nuevos registros
                if (oCat.IDTipo == 0)
                {
                    oCat.FechaRegistro = DateTime.Now;
                    oCat.Estatus = oCat.Estatus ?? true; // Usar Estatus, no Activo
                }

                bool respuesta = false;

                if (oCat.IDTipo == 0)
                {
                    System.Diagnostics.Debug.WriteLine("Ejecutando REGISTRAR...");
                    respuesta = TiposMetodos.Instance.Registrar(oCat);
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("Ejecutando MODIFICAR...");
                    respuesta = TiposMetodos.Instance.Modificar(oCat);
                }

                System.Diagnostics.Debug.WriteLine($"Resultado final: {respuesta}");
                return Json(new { respuesta = respuesta }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"EXCEPCIÓN en controlador: {ex.Message}");
                return Json(new { respuesta = false, error = $"Error interno: {ex.Message}" }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public JsonResult BorrarTipos(int id)
        {
            bool respuesta = false;
            respuesta = TiposMetodos.Instance.Eliminar(id);
            return Json(new { respuesta = respuesta }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult TestConexion()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(Conexion.Bd))
                {
                    conn.Open();
                    return Json(new { success = true, mensaje = "Conexión exitosa" }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }
    }
}