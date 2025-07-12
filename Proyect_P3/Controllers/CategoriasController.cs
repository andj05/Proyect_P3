using Proyect_P3.Metodos;
using Proyect_P3.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Web.Mvc;

namespace Proyect_P3.Controllers
{
    public class CategoriasController : Controller
    {
        public ActionResult Categorias()
        {
            if (Session["Usuario"] == null)
                return RedirectToAction("Login", "Acceder");
            return View();
        }

        [HttpGet]
        public JsonResult ConsultaCategorias()
        {
            System.Diagnostics.Debug.WriteLine("=== INICIANDO CONSULTA CATEGORIAS ===");

            try
            {
                List<Categorias> oLista = new List<Categorias>();
                oLista = CategoriasMetodos.Instance.Listar();

                System.Diagnostics.Debug.WriteLine($"Método Listar retornó {oLista?.Count ?? 0} elementos");

                // Convertir imágenes a Base64 igual que en Tipos
                if (oLista != null && oLista.Count > 0)
                {
                    foreach (var categoria in oLista)
                    {
                        if (categoria.Imagen != null && categoria.Imagen.Length > 0)
                        {
                            try
                            {
                                categoria.ImagenBase64 = Convert.ToBase64String(categoria.Imagen);
                                System.Diagnostics.Debug.WriteLine($"Categoria {categoria.IdCategoria}: ImagenBase64 creada - {categoria.ImagenBase64.Length} caracteres");
                            }
                            catch (Exception imgEx)
                            {
                                System.Diagnostics.Debug.WriteLine($"Error al convertir imagen para categoria {categoria.IdCategoria}: {imgEx.Message}");
                                categoria.ImagenBase64 = null;
                            }
                        }
                        else
                        {
                            System.Diagnostics.Debug.WriteLine($"Categoria {categoria.IdCategoria}: Sin imagen");
                        }
                    }
                }

                System.Diagnostics.Debug.WriteLine($"Retornando {oLista.Count} categorias con imágenes convertidas");

                // Solución: Aumentar MaxJsonLength como en Tipos
                var jsonResult = Json(new { data = oLista }, JsonRequestBehavior.AllowGet);
                jsonResult.MaxJsonLength = int.MaxValue; // Sin límite

                return jsonResult;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"EXCEPCIÓN en ConsultaCategorias: {ex.Message}");
                return Json(new { data = new List<Categorias>(), error = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public JsonResult InsertarCategorias(Categorias oCat, string ImagenBase64)
        {
            System.Diagnostics.Debug.WriteLine("=== INSERTAR CATEGORIA CONTROLADOR ===");

            try
            {
                if (oCat == null)
                {
                    System.Diagnostics.Debug.WriteLine("ERROR: oCat es null");
                    return Json(new { respuesta = false, error = "Objeto nulo" }, JsonRequestBehavior.AllowGet);
                }

                System.Diagnostics.Debug.WriteLine($"IdCategoria: {oCat.IdCategoria}");
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
                        // Para modificaciones, mantener imagen existente
                        if (oCat.IdCategoria > 0)
                        {
                            oCat.Imagen = null; // No cambiar imagen si hay error
                        }
                    }
                }
                else
                {
                    // IMPORTANTE: Para modificaciones sin nueva imagen, dejar en null
                    if (oCat.IdCategoria > 0)
                    {
                        oCat.Imagen = null; // Esto le dice al método que no cambie la imagen
                        System.Diagnostics.Debug.WriteLine("✅ Modificación SIN nueva imagen - manteniendo imagen existente");
                    }
                }

                // Establecer valores por defecto para nuevos registros
                if (oCat.IdCategoria == 0)
                {
                    oCat.FechaRegistro = DateTime.Now;
                    // Si no se especifica Estatus, usar true por defecto
                    if (!oCat.Estatus.HasValue)
                    {
                        oCat.Estatus = true;
                    }
                }

                bool respuesta = false;

                if (oCat.IdCategoria == 0)
                {
                    System.Diagnostics.Debug.WriteLine("Ejecutando REGISTRAR...");
                    respuesta = CategoriasMetodos.Instance.Registrar(oCat);
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("Ejecutando MODIFICAR...");
                    respuesta = CategoriasMetodos.Instance.Modificar(oCat);
                }

                System.Diagnostics.Debug.WriteLine($"Resultado final: {respuesta}");

                if (respuesta)
                {
                    return Json(new { respuesta = true, mensaje = "Categoria guardada exitosamente" }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    // Mensaje más específico
                    string errorMsg = oCat.IdCategoria == 0 ?
                        "Error al crear la categoria" :
                        "Error al modificar la categoria - posible duplicado o problema de validación";

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
        public JsonResult BorrarCategorias(int id)
        {
            bool respuesta = false;
            respuesta = CategoriasMetodos.Instance.Eliminar(id);
            return Json(new { respuesta = respuesta }, JsonRequestBehavior.AllowGet);
        }

        // Métodos útiles como en Tipos
        [HttpGet]
        public JsonResult ObtenerCategoriaPorId(int idCategoria)
        {
            Categorias oCategoria = CategoriasMetodos.Instance.ObtenerPorId(idCategoria);

            // Convertir imagen si existe
            if (oCategoria != null && oCategoria.Imagen != null && oCategoria.Imagen.Length > 0)
            {
                try
                {
                    oCategoria.ImagenBase64 = Convert.ToBase64String(oCategoria.Imagen);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error al convertir imagen para categoria {oCategoria.IdCategoria}: {ex.Message}");
                }
            }

            return Json(new { data = oCategoria }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult ConsultaCategoriasActivas()
        {
            List<Categorias> oLista = new List<Categorias>();
            oLista = CategoriasMetodos.Instance.Listar();
            var categoriasActivas = oLista.FindAll(c => c.Estatus == true);

            // Convertir imágenes
            foreach (var categoria in categoriasActivas)
            {
                if (categoria.Imagen != null && categoria.Imagen.Length > 0)
                {
                    try
                    {
                        categoria.ImagenBase64 = Convert.ToBase64String(categoria.Imagen);
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Error al convertir imagen: {ex.Message}");
                    }
                }
            }

            return Json(new { data = categoriasActivas }, JsonRequestBehavior.AllowGet);
        }

        // Método para testing
        [HttpGet]
        public JsonResult TestConexion()
        {
            try
            {
                List<Categorias> test = CategoriasMetodos.Instance.Listar();
                return Json(new { success = true, count = test.Count, message = "Conexión exitosa" }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }
    }
}