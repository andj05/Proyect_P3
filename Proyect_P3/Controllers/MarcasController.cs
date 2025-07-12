using Proyect_P3.Metodos;
using Proyect_P3.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Web.Mvc;

namespace Proyect_P3.Controllers
{
    public class MarcasController : Controller
    {
        public ActionResult Marcas()
        {
            if (Session["Usuario"] == null)
                return RedirectToAction("Login", "Acceder");
            return View();
        }

        [HttpGet]
        public JsonResult ConsultaMarcas()
        {
            System.Diagnostics.Debug.WriteLine("=== INICIANDO CONSULTA MARCAS ===");

            try
            {
                List<Marcas> oLista = new List<Marcas>();
                oLista = MarcasMetodos.Instance.Listar();

                System.Diagnostics.Debug.WriteLine($"Método Listar retornó {oLista?.Count ?? 0} elementos");

                // Convertir imágenes a Base64 para la vista
                if (oLista != null && oLista.Count > 0)
                {
                    foreach (var marca in oLista)
                    {
                        if (marca.Imagen != null && marca.Imagen.Length > 0)
                        {
                            if (string.IsNullOrEmpty(marca.ImagenBase64))
                            {
                                marca.ImagenBase64 = Convert.ToBase64String(marca.Imagen);
                                System.Diagnostics.Debug.WriteLine($"Marca {marca.IdMarca}: ImagenBase64 creada en controlador");
                            }
                            else
                            {
                                System.Diagnostics.Debug.WriteLine($"Marca {marca.IdMarca}: ImagenBase64 ya existe desde Listar()");
                            }
                        }
                        else
                        {
                            System.Diagnostics.Debug.WriteLine($"Marca {marca.IdMarca}: Sin imagen");
                        }
                    }
                }

                System.Diagnostics.Debug.WriteLine($"Retornando {oLista.Count} marcas");

                // ✅ SOLUCIÓN: Crear JsonResult con MaxJsonLength aumentado
                var jsonResult = Json(new { data = oLista }, JsonRequestBehavior.AllowGet);
                jsonResult.MaxJsonLength = int.MaxValue; // Sin límite

                return jsonResult;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"EXCEPCIÓN en ConsultaMarcas: {ex.Message}");
                return Json(new { data = new List<Marcas>(), error = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public JsonResult InsertarMarcas(Marcas oMarca, string ImagenBase64)
        {
            System.Diagnostics.Debug.WriteLine("=== CONTROLADOR MARCAS ===");

            try
            {
                // Verificar que lleguen los datos
                if (oMarca == null)
                {
                    System.Diagnostics.Debug.WriteLine("ERROR: oMarca es null");
                    return Json(new { respuesta = false, error = "Objeto nulo" });
                }

                // Log de datos recibidos
                System.Diagnostics.Debug.WriteLine($"IdMarca: {oMarca.IdMarca}");
                System.Diagnostics.Debug.WriteLine($"Descripcion: '{oMarca.Descripcion}'");
                System.Diagnostics.Debug.WriteLine($"Estatus: {oMarca.Estatus}");
                System.Diagnostics.Debug.WriteLine($"ImagenBase64 recibida: {!string.IsNullOrEmpty(ImagenBase64)}");

                // Validar descripción
                if (string.IsNullOrWhiteSpace(oMarca.Descripcion))
                {
                    System.Diagnostics.Debug.WriteLine("ERROR: Descripción vacía");
                    return Json(new { respuesta = false, error = "Descripción requerida" });
                }

                // Procesar imagen si existe
                if (!string.IsNullOrEmpty(ImagenBase64))
                {
                    try
                    {
                        oMarca.Imagen = Convert.FromBase64String(ImagenBase64);
                        System.Diagnostics.Debug.WriteLine($"Imagen convertida: {oMarca.Imagen.Length} bytes");
                    }
                    catch (Exception imgEx)
                    {
                        System.Diagnostics.Debug.WriteLine($"ERROR al convertir imagen: {imgEx.Message}");
                        return Json(new { respuesta = false, error = "Error al procesar imagen" });
                    }
                }

                // Establecer valores por defecto para nuevos registros
                if (oMarca.IdMarca == 0)
                {
                    oMarca.FechaRegistro = DateTime.Now;
                    oMarca.Estatus = oMarca.Estatus ?? true;
                    System.Diagnostics.Debug.WriteLine("Es nuevo registro - estableciendo defaults");
                }

                bool respuesta = false;

                if (oMarca.IdMarca == 0)
                {
                    System.Diagnostics.Debug.WriteLine("Ejecutando REGISTRAR...");
                    respuesta = MarcasMetodos.Instance.Registrar(oMarca);
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("Ejecutando MODIFICAR...");
                    respuesta = MarcasMetodos.Instance.Modificar(oMarca);
                }

                System.Diagnostics.Debug.WriteLine($"Resultado final: {respuesta}");

                // IMPORTANTE: Devolver respuesta clara
                if (respuesta)
                {
                    return Json(new { respuesta = true, mensaje = "Operación exitosa" });
                }
                else
                {
                    return Json(new { respuesta = false, error = "Error en la operación de base de datos" });
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"EXCEPCIÓN en controlador: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"StackTrace: {ex.StackTrace}");
                return Json(new { respuesta = false, error = $"Error interno: {ex.Message}" });
            }
        }

        [HttpPost]
        public JsonResult BorrarMarcas(int idMarca)
        {
            bool respuesta = false;
            respuesta = MarcasMetodos.Instance.Eliminar(idMarca);
            return Json(new { respuesta = respuesta }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult ObtenerMarcaPorId(int idMarca)
        {
            Marcas oMarca = MarcasMetodos.Instance.ObtenerPorIdMarca(idMarca);
            return Json(new { data = oMarca }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult ConsultaMarcasActivas()
        {
            List<Marcas> oLista = new List<Marcas>();
            oLista = MarcasMetodos.Instance.Listar();
            var marcasActivas = oLista.FindAll(m => m.Estatus == true);
            return Json(new { data = marcasActivas }, JsonRequestBehavior.AllowGet);
        }
    }
}