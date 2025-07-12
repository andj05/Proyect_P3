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

                // 🔥 CONVERTIR IMÁGENES A BASE64 IGUAL QUE EN MARCAS
                if (oLista != null && oLista.Count > 0)
                {
                    foreach (var tipo in oLista)
                    {
                        if (tipo.Imagen != null && tipo.Imagen.Length > 0)
                        {
                            try
                            {
                                tipo.ImagenBase64 = Convert.ToBase64String(tipo.Imagen);
                                System.Diagnostics.Debug.WriteLine($"Tipo {tipo.IDTipo}: ImagenBase64 creada - {tipo.ImagenBase64.Length} caracteres");
                            }
                            catch (Exception imgEx)
                            {
                                System.Diagnostics.Debug.WriteLine($"Error al convertir imagen para tipo {tipo.IDTipo}: {imgEx.Message}");
                                tipo.ImagenBase64 = null;
                            }
                        }
                        else
                        {
                            System.Diagnostics.Debug.WriteLine($"Tipo {tipo.IDTipo}: Sin imagen");
                        }
                    }
                }

                System.Diagnostics.Debug.WriteLine($"Retornando {oLista.Count} tipos con imágenes convertidas");

                // 🔥 SOLUCIÓN: Aumentar MaxJsonLength como en Marcas
                var jsonResult = Json(new { data = oLista }, JsonRequestBehavior.AllowGet);
                jsonResult.MaxJsonLength = int.MaxValue; // Sin límite

                return jsonResult;
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
                        // 🔥 Para modificaciones, mantener imagen existente
                        if (oCat.IDTipo > 0)
                        {
                            oCat.Imagen = null; // No cambiar imagen si hay error
                        }
                    }
                }
                else
                {
                    // 🔥 IMPORTANTE: Para modificaciones sin nueva imagen, dejar en null
                    if (oCat.IDTipo > 0)
                    {
                        oCat.Imagen = null; // Esto le dice al método que no cambie la imagen
                        System.Diagnostics.Debug.WriteLine("✅ Modificación SIN nueva imagen - manteniendo imagen existente");
                    }
                }

                // Establecer valores por defecto para nuevos registros
                if (oCat.IDTipo == 0)
                {
                    oCat.FechaRegistro = DateTime.Now;
                    // 🔥 Si no se especifica Estatus, usar true por defecto
                    if (!oCat.Estatus.HasValue)
                    {
                        oCat.Estatus = true;
                    }
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

                if (respuesta)
                {
                    return Json(new { respuesta = true, mensaje = "Tipo guardado exitosamente" }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    // Mensaje más específico
                    string errorMsg = oCat.IDTipo == 0 ?
                        "Error al crear el tipo" :
                        "Error al modificar el tipo - posible duplicado o problema de validación";

                    return Json(new { respuesta = false, error = errorMsg }, JsonRequestBehavior.AllowGet);
                }
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

        // 🔥 AGREGAR MÉTODOS ÚTILES COMO EN MARCAS
        [HttpGet]
        public JsonResult ObtenerTipoPorId(int idTipo)
        {
            Tipos oTipo = TiposMetodos.Instance.ObtenerPorId(idTipo);

            // Convertir imagen si existe
            if (oTipo != null && oTipo.Imagen != null && oTipo.Imagen.Length > 0)
            {
                try
                {
                    oTipo.ImagenBase64 = Convert.ToBase64String(oTipo.Imagen);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error al convertir imagen para tipo {oTipo.IDTipo}: {ex.Message}");
                }
            }

            return Json(new { data = oTipo }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult ConsultaTiposActivos()
        {
            List<Tipos> oLista = new List<Tipos>();
            oLista = TiposMetodos.Instance.Listar();
            var tiposActivos = oLista.FindAll(t => t.Estatus == true);

            // Convertir imágenes
            foreach (var tipo in tiposActivos)
            {
                if (tipo.Imagen != null && tipo.Imagen.Length > 0)
                {
                    try
                    {
                        tipo.ImagenBase64 = Convert.ToBase64String(tipo.Imagen);
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Error al convertir imagen: {ex.Message}");
                    }
                }
            }

            return Json(new { data = tiposActivos }, JsonRequestBehavior.AllowGet);
        }

        // 🔥 MÉTODO PARA TESTING
        [HttpGet]
        public JsonResult TestConexion()
        {
            try
            {
                List<Tipos> test = TiposMetodos.Instance.Listar();
                return Json(new { success = true, count = test.Count, message = "Conexión exitosa" }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }
    }
}