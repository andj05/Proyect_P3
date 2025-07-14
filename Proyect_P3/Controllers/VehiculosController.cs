using Proyect_P3.Metodos;
using Proyect_P3.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Mvc;

namespace Proyect_P3.Controllers
{
    public class VehiculosController : Controller
    {
        public ActionResult Index()
        {
            if (Session["Usuario"] == null)
                return RedirectToAction("Login", "Acceder");
            return View();
        }
        public ActionResult Vehiculos()
        {
            if (Session["Usuario"] == null)
                return RedirectToAction("Login", "Acceder");
            return View("Index");
        }

        [HttpGet]
        public JsonResult ConsultaVehiculos()
        {
            System.Diagnostics.Debug.WriteLine("=== INICIANDO CONSULTA VEHICULOS ===");

            try
            {
                List<Vehiculos> oLista = new List<Vehiculos>();
                oLista = VehiculosMetodos.Instance.Listar();

                System.Diagnostics.Debug.WriteLine($"Método Listar retornó {oLista?.Count ?? 0} elementos");

                // 🔥 SOLUCIÓN: Aumentar MaxJsonLength
                var jsonResult = Json(new { data = oLista }, JsonRequestBehavior.AllowGet);
                jsonResult.MaxJsonLength = int.MaxValue; // Sin límite

                return jsonResult;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"EXCEPCIÓN en ConsultaVehiculos: {ex.Message}");
                return Json(new { data = new List<Vehiculos>(), error = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public JsonResult InsertarVehiculos(Vehiculos oVehiculo)
        {
            System.Diagnostics.Debug.WriteLine("=== INSERTAR VEHICULO CONTROLADOR ===");

            try
            {
                if (oVehiculo == null)
                {
                    System.Diagnostics.Debug.WriteLine("ERROR: oVehiculo es null");
                    return Json(new { respuesta = false, error = "Objeto nulo" }, JsonRequestBehavior.AllowGet);
                }

                System.Diagnostics.Debug.WriteLine($"IDVehiculo: {oVehiculo.IDVehiculo}");
                System.Diagnostics.Debug.WriteLine($"Modelo: '{oVehiculo.Modelo}'");
                System.Diagnostics.Debug.WriteLine($"Precio: {oVehiculo.Precio}");
                System.Diagnostics.Debug.WriteLine($"IDUsuario: {oVehiculo.IDUsuario}");

                // Validar campos requeridos
                if (string.IsNullOrWhiteSpace(oVehiculo.Modelo))
                {
                    System.Diagnostics.Debug.WriteLine("ERROR: Modelo requerido");
                    return Json(new { respuesta = false, error = "Modelo requerido" }, JsonRequestBehavior.AllowGet);
                }

                if (oVehiculo.Precio <= 0)
                {
                    System.Diagnostics.Debug.WriteLine("ERROR: Precio debe ser mayor a 0");
                    return Json(new { respuesta = false, error = "Precio debe ser mayor a 0" }, JsonRequestBehavior.AllowGet);
                }

                if (oVehiculo.IDUsuario <= 0)
                {
                    System.Diagnostics.Debug.WriteLine("ERROR: Usuario requerido");
                    return Json(new { respuesta = false, error = "Usuario requerido" }, JsonRequestBehavior.AllowGet);
                }

                if (oVehiculo.Año < 1900 || oVehiculo.Año > DateTime.Now.Year + 1)
                {
                    System.Diagnostics.Debug.WriteLine("ERROR: Año inválido");
                    return Json(new { respuesta = false, error = "Año inválido" }, JsonRequestBehavior.AllowGet);
                }

                // Establecer valores por defecto para nuevos registros
                if (oVehiculo.IDVehiculo == 0)
                {
                    oVehiculo.FechaRegistro = DateTime.Now;
                    oVehiculo.Estatus = true;
                    oVehiculo.Vendido = false;
                    oVehiculo.Destacado = false;
                    oVehiculo.NumeroVistas = 0;
                }

                bool respuesta = false;
                int idInsertado = oVehiculo.IDVehiculo;

                if (oVehiculo.IDVehiculo == 0)
                {
                    System.Diagnostics.Debug.WriteLine("Ejecutando REGISTRAR...");
                    respuesta = VehiculosMetodos.Instance.Registrar(oVehiculo);
                    // Asumiendo que Registrar actualiza oVehiculo.IDVehiculo con el nuevo ID
                    idInsertado = oVehiculo.IDVehiculo;
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("Ejecutando MODIFICAR...");
                    respuesta = VehiculosMetodos.Instance.Modificar(oVehiculo);
                }

                System.Diagnostics.Debug.WriteLine($"Resultado final: {respuesta}");

                if (respuesta)
                {
                    return Json(new { respuesta = true, mensaje = "Vehículo guardado exitosamente", idVehiculo = idInsertado }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    string errorMsg = oVehiculo.IDVehiculo == 0 ?
                        "Error al crear el vehículo" :
                        "Error al modificar el vehículo - posible duplicado o problema de validación";

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
        public JsonResult BorrarVehiculos(int id)
        {
            bool respuesta = false;
            respuesta = VehiculosMetodos.Instance.Eliminar(id);
            return Json(new { respuesta = respuesta }, JsonRequestBehavior.AllowGet);
        }

        // 🔥 MÉTODOS ÚTILES ADICIONALES
        [HttpGet]
        public JsonResult ObtenerVehiculoPorId(int idVehiculo)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"=== OBTENIENDO DETALLES DEL VEHÍCULO {idVehiculo} ===");

                Vehiculos oVehiculo = VehiculosMetodos.Instance.ObtenerPorId(idVehiculo);

                if (oVehiculo != null)
                {
                    try
                    {
                        System.Diagnostics.Debug.WriteLine($"🔍 Obteniendo fotos para vehículo {idVehiculo}");

                        // 📸 Obtener fotos y COMPRIMIRLAS
                        var fotosOriginales = ArticulosFotosMetodos.Instance.ObtenerFotosBase64PorArticulo(idVehiculo);
                        var fotosComprimidas = new List<string>();

                        foreach (var foto in fotosOriginales)
                        {
                            try
                            {
                                // 🗜️ COMPRIMIR CADA FOTO
                                string fotoComprimida = ComprimirImagenBase64(foto);
                                fotosComprimidas.Add(fotoComprimida);

                                System.Diagnostics.Debug.WriteLine($"✅ Foto comprimida: {foto.Length} → {fotoComprimida.Length} caracteres");
                            }
                            catch (Exception ex)
                            {
                                System.Diagnostics.Debug.WriteLine($"❌ Error comprimiendo foto: {ex.Message}");
                                // Si no se puede comprimir, omitir esta foto
                            }
                        }

                        oVehiculo.TodasLasFotos = fotosComprimidas;
                        oVehiculo.CantidadFotos = fotosComprimidas.Count;

                        if (fotosComprimidas.Count > 0)
                        {
                            oVehiculo.PrimeraFoto = fotosComprimidas[0];
                        }

                        System.Diagnostics.Debug.WriteLine($"✅ {fotosComprimidas.Count} fotos comprimidas listas");
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"❌ Error procesando fotos: {ex.Message}");
                        oVehiculo.TodasLasFotos = new List<string>();
                        oVehiculo.CantidadFotos = 0;
                    }

                    VehiculosMetodos.Instance.IncrementarVistas(idVehiculo);
                }

                // 🔧 CONFIGURAR JSON RESULT CON LÍMITE MÁXIMO
                var jsonResult = Json(new { data = oVehiculo }, JsonRequestBehavior.AllowGet);
                jsonResult.MaxJsonLength = int.MaxValue;

                return jsonResult;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Error general: {ex.Message}");
                return Json(new { data = (object)null, error = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        // 🗜️ MÉTODO PARA COMPRIMIR IMÁGENES BASE64
        private string ComprimirImagenBase64(string imagenBase64)
        {
            try
            {
                if (string.IsNullOrEmpty(imagenBase64) || !imagenBase64.Contains(","))
                    return imagenBase64;

                // Extraer el prefijo y los datos base64
                string[] partes = imagenBase64.Split(',');
                if (partes.Length != 2) return imagenBase64;

                string prefijo = partes[0] + ",";
                string datosBase64 = partes[1];

                // Convertir a bytes
                byte[] imageBytes = Convert.FromBase64String(datosBase64);

                using (var ms = new MemoryStream(imageBytes))
                using (var imagen = System.Drawing.Image.FromStream(ms))
                {
                    // 📐 CALCULAR NUEVAS DIMENSIONES
                    int nuevoAncho = imagen.Width;
                    int nuevoAlto = imagen.Height;

                    // Si es muy grande, redimensionar
                    if (imagen.Width > 1000 || imagen.Height > 800)
                    {
                        double ratio = Math.Min(1000.0 / imagen.Width, 800.0 / imagen.Height);
                        nuevoAncho = (int)(imagen.Width * ratio);
                        nuevoAlto = (int)(imagen.Height * ratio);
                    }

                    using (var nuevaImagen = new System.Drawing.Bitmap(nuevoAncho, nuevoAlto))
                    using (var graphics = System.Drawing.Graphics.FromImage(nuevaImagen))
                    {
                        // 🎨 CONFIGURAR CALIDAD
                        graphics.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
                        graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                        graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;

                        graphics.DrawImage(imagen, 0, 0, nuevoAncho, nuevoAlto);

                        using (var outputStream = new MemoryStream())
                        {
                            // 💾 GUARDAR CON COMPRESIÓN JPEG
                            var encoderParams = new System.Drawing.Imaging.EncoderParameters(1);
                            encoderParams.Param[0] = new System.Drawing.Imaging.EncoderParameter(
                                System.Drawing.Imaging.Encoder.Quality, 75L);

                            var jpegEncoder = GetJpegEncoder();
                            nuevaImagen.Save(outputStream, jpegEncoder, encoderParams);

                            byte[] compressedBytes = outputStream.ToArray();
                            string compressedBase64 = Convert.ToBase64String(compressedBytes);

                            return $"data:image/jpeg;base64,{compressedBase64}";
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Error comprimiendo imagen: {ex.Message}");
                return imagenBase64; // Devolver original si hay error
            }
        }

        // 🔧 HELPER PARA OBTENER ENCODER JPEG
        private System.Drawing.Imaging.ImageCodecInfo GetJpegEncoder()
        {
            var codecs = System.Drawing.Imaging.ImageCodecInfo.GetImageEncoders();
            return codecs.FirstOrDefault(codec =>
                codec.FormatID == System.Drawing.Imaging.ImageFormat.Jpeg.Guid);
        }

        [HttpGet]
        public JsonResult ConsultaVehiculosActivos()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("=== CONSULTANDO VEHICULOS ACTIVOS CON FOTOS ===");

                List<Vehiculos> oLista = VehiculosMetodos.Instance.Listar();
                var vehiculosActivos = oLista.FindAll(v => v.Estatus == true && v.Vendido == false);

                System.Diagnostics.Debug.WriteLine($"📋 {vehiculosActivos.Count} vehículos activos encontrados");

                // 📸 Agregar información de fotos a cada vehículo
                foreach (var vehiculo in vehiculosActivos)
                {
                    try
                    {
                        System.Diagnostics.Debug.WriteLine($"🔍 Procesando fotos para vehículo {vehiculo.IDVehiculo}");

                        // 🖼️ Obtener primera foto como thumbnail (YA CON PREFIJO)
                        string primeraFoto = ArticulosFotosMetodos.Instance.ObtenerPrimeraFotoBase64(vehiculo.IDVehiculo);
                        vehiculo.PrimeraFoto = primeraFoto; // Ya viene con data:image/jpeg;base64,

                        // 📊 Contar total de fotos
                        int cantidadFotos = ArticulosFotosMetodos.Instance.ContarFotos(vehiculo.IDVehiculo);
                        vehiculo.CantidadFotos = cantidadFotos;

                        System.Diagnostics.Debug.WriteLine($"✅ Vehículo {vehiculo.IDVehiculo}: {cantidadFotos} fotos, primera foto: {(primeraFoto != null ? "SÍ" : "NO")}");

                        if (primeraFoto != null)
                        {
                            System.Diagnostics.Debug.WriteLine($"📸 Primera foto length: {primeraFoto.Length} caracteres");
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"❌ Error procesando fotos para vehículo {vehiculo.IDVehiculo}: {ex.Message}");
                        vehiculo.PrimeraFoto = null;
                        vehiculo.CantidadFotos = 0;
                    }
                }

                System.Diagnostics.Debug.WriteLine("✅ Procesamiento de fotos completado");

                var jsonResult = Json(new { data = vehiculosActivos }, JsonRequestBehavior.AllowGet);
                jsonResult.MaxJsonLength = int.MaxValue;

                return jsonResult;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Error en ConsultaVehiculosActivos: {ex.Message}");
                return Json(new { data = new List<Vehiculos>(), error = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }


        [HttpGet]
        public JsonResult ConsultaVehiculosPorUsuario(int idUsuario)
        {
            try
            {
                List<Vehiculos> oLista = VehiculosMetodos.Instance.ListarPorUsuario(idUsuario);

                var jsonResult = Json(new { data = oLista }, JsonRequestBehavior.AllowGet);
                jsonResult.MaxJsonLength = int.MaxValue;

                return jsonResult;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error en ConsultaVehiculosPorUsuario: {ex.Message}");
                return Json(new { data = new List<Vehiculos>(), error = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public JsonResult MarcarComoVendido(int idVehiculo)
        {
            try
            {
                bool respuesta = VehiculosMetodos.Instance.MarcarComoVendido(idVehiculo);

                if (respuesta)
                {
                    return Json(new { respuesta = true, mensaje = "Vehículo marcado como vendido" }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(new { respuesta = false, error = "Error al marcar como vendido" }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error al marcar como vendido: {ex.Message}");
                return Json(new { respuesta = false, error = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public JsonResult CambiarDestacado(int idVehiculo, bool destacado)
        {
            try
            {
                // Obtener el vehículo actual
                var vehiculo = VehiculosMetodos.Instance.ObtenerPorId(idVehiculo);
                if (vehiculo == null)
                {
                    return Json(new { respuesta = false, error = "Vehículo no encontrado" }, JsonRequestBehavior.AllowGet);
                }

                // Cambiar solo el estado destacado
                vehiculo.Destacado = destacado;
                bool respuesta = VehiculosMetodos.Instance.Modificar(vehiculo);

                if (respuesta)
                {
                    string mensaje = destacado ? "Vehículo marcado como destacado" : "Vehículo removido de destacados";
                    return Json(new { respuesta = true, mensaje = mensaje }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(new { respuesta = false, error = "Error al cambiar estado destacado" }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error al cambiar destacado: {ex.Message}");
                return Json(new { respuesta = false, error = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet]
        public JsonResult ConsultaVehiculosDestacados()
        {
            try
            {
                List<Vehiculos> oLista = VehiculosMetodos.Instance.Listar();
                var vehiculosDestacados = oLista.FindAll(v => v.Estatus == true && v.Vendido == false && v.Destacado == true);

                var jsonResult = Json(new { data = vehiculosDestacados }, JsonRequestBehavior.AllowGet);
                jsonResult.MaxJsonLength = int.MaxValue;

                return jsonResult;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error en ConsultaVehiculosDestacados: {ex.Message}");
                return Json(new { data = new List<Vehiculos>(), error = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        // 🔧 MÉTODO CORREGIDO EN VehiculosController.cs

        [HttpGet]
        public JsonResult ObtenerDatosFormulario()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("=== OBTENIENDO DATOS DEL FORMULARIO ===");

                // 1. Obtener Categorías
                var categorias = new List<object>();
                try
                {
                    var categoriasLista = CategoriasMetodos.Instance.Listar();
                    if (categoriasLista != null)
                    {
                        categorias = categoriasLista
                            .Where(c => c.Estatus == true)
                            .Select(c => new {
                                IdCategoria = c.IdCategoria,
                                Descripcion = c.Descripcion
                            }).ToList<object>();
                    }
                    System.Diagnostics.Debug.WriteLine($"✅ Categorías cargadas: {categorias.Count}");
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"❌ Error cargando categorías: {ex.Message}");
                    categorias = new List<object>();
                }

                // 2. Obtener Marcas
                var marcas = new List<object>();
                try
                {
                    var marcasLista = MarcasMetodos.Instance.Listar();
                    if (marcasLista != null)
                    {
                        marcas = marcasLista
                            .Where(m => m.Estatus == true)
                            .Select(m => new {
                                IdMarca = m.IdMarca,
                                Descripcion = m.Descripcion,
                                ImagenBase64 = m.ImagenBase64 // Si tienes imágenes
                            }).ToList<object>();
                    }
                    System.Diagnostics.Debug.WriteLine($"✅ Marcas cargadas: {marcas.Count}");
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"❌ Error cargando marcas: {ex.Message}");
                    marcas = new List<object>();
                }

                // 3. Obtener Tipos
                var tipos = new List<object>();
                try
                {
                    var tiposLista = TiposMetodos.Instance.Listar();
                    if (tiposLista != null)
                    {
                        tipos = tiposLista
                            .Where(t => t.Estatus == true)
                            .Select(t => new {
                                IDTipo = t.IDTipo,
                                Descripcion = t.Descripcion
                            }).ToList<object>();
                    }
                    System.Diagnostics.Debug.WriteLine($"✅ Tipos cargados: {tipos.Count}");
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"❌ Error cargando tipos: {ex.Message}");
                    tipos = new List<object>();
                }

                // 4. Obtener Motores
                var motores = new List<object>();
                try
                {
                    var motoresLista = MotoresMetodos.Instance.Listar();
                    if (motoresLista != null)
                    {
                        motores = motoresLista.Select(mo => new {
                            IdMotor = mo.IdMotor,
                            Descripcion = mo.Descripcion,
                            Combustible = mo.Combustible,
                            Transmision = mo.Transmisión, // Nota: corregí "Transmisión" por "Transmision"
                            DescripcionCompleta = $"{mo.Descripcion} - {mo.Combustible} - {mo.Transmisión}"
                        }).ToList<object>();
                    }
                    System.Diagnostics.Debug.WriteLine($"✅ Motores cargados: {motores.Count}");
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"❌ Error cargando motores: {ex.Message}");
                    motores = new List<object>();
                }

                // 5. Obtener Provincias y Municipios
                var provincias = new List<object>();
                var municipios = new List<object>();

                try
                {
                    using (SqlConnection oCnn = new SqlConnection(Conexion.Bd))
                    {
                        oCnn.Open();

                        // Obtener provincias
                        string sqlProvincias = "SELECT CodigoProvincia, NombreProvincia FROM Provincias ORDER BY NombreProvincia";
                        SqlCommand cmdProv = new SqlCommand(sqlProvincias, oCnn);
                        SqlDataReader drProv = cmdProv.ExecuteReader();

                        while (drProv.Read())
                        {
                            provincias.Add(new
                            {
                                CodigoProvincia = Convert.ToInt32(drProv["CodigoProvincia"]),
                                NombreProvincia = drProv["NombreProvincia"]?.ToString() ?? ""
                            });
                        }
                        drProv.Close();

                        // Obtener municipios
                        string sqlMunicipios = "SELECT CodigoMunicipio, NombreMunicipio, CodigoProvincia FROM Municipios ORDER BY NombreMunicipio";
                        SqlCommand cmdMun = new SqlCommand(sqlMunicipios, oCnn);
                        SqlDataReader drMun = cmdMun.ExecuteReader();

                        while (drMun.Read())
                        {
                            municipios.Add(new
                            {
                                CodigoMunicipio = Convert.ToInt32(drMun["CodigoMunicipio"]),
                                NombreMunicipio = drMun["NombreMunicipio"]?.ToString() ?? "",
                                CodigoProvincia = Convert.ToInt32(drMun["CodigoProvincia"])
                            });
                        }
                        drMun.Close();

                        System.Diagnostics.Debug.WriteLine($"✅ Provincias cargadas: {provincias.Count}");
                        System.Diagnostics.Debug.WriteLine($"✅ Municipios cargados: {municipios.Count}");
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"❌ Error cargando provincias/municipios: {ex.Message}");
                    provincias = new List<object>();
                    municipios = new List<object>();
                }

                // 6. Crear respuesta final
                var respuesta = new
                {
                    categorias = categorias,
                    marcas = marcas,
                    tipos = tipos,
                    motores = motores,
                    provincias = provincias,
                    municipios = municipios,
                    success = true,
                    timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
                };

                System.Diagnostics.Debug.WriteLine("=== DATOS PREPARADOS PARA ENVIAR ===");
                System.Diagnostics.Debug.WriteLine($"📊 Resumen final:");
                System.Diagnostics.Debug.WriteLine($"   - Categorías: {categorias.Count}");
                System.Diagnostics.Debug.WriteLine($"   - Marcas: {marcas.Count}");
                System.Diagnostics.Debug.WriteLine($"   - Tipos: {tipos.Count}");
                System.Diagnostics.Debug.WriteLine($"   - Motores: {motores.Count}");
                System.Diagnostics.Debug.WriteLine($"   - Provincias: {provincias.Count}");
                System.Diagnostics.Debug.WriteLine($"   - Municipios: {municipios.Count}");

                // Configurar el JsonResult para evitar problemas de tamaño
                var jsonResult = Json(respuesta, JsonRequestBehavior.AllowGet);
                jsonResult.MaxJsonLength = int.MaxValue; // Sin límite

                return jsonResult;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ ERROR GENERAL en ObtenerDatosFormulario: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"❌ StackTrace: {ex.StackTrace}");

                return Json(new
                {
                    categorias = new List<object>(),
                    marcas = new List<object>(),
                    tipos = new List<object>(),
                    motores = new List<object>(),
                    provincias = new List<object>(),
                    municipios = new List<object>(),
                    success = false,
                    error = ex.Message,
                    timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
                }, JsonRequestBehavior.AllowGet);
            }
        }

        // Añade estos métodos a tu VehiculosController.cs existente:

        [HttpPost]
        public JsonResult SubirFotos(int idVehiculo)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"=== SUBIENDO FOTOS PARA VEHÍCULO {idVehiculo} ===");

                int idUsuario = Session["UsuarioID"] != null ? Convert.ToInt32(Session["UsuarioID"]) : 1;
                List<byte[]> fotos = new List<byte[]>();

                for (int i = 0; i < Request.Files.Count; i++)
                {
                    HttpPostedFileBase archivo = Request.Files[i];
                    if (archivo != null && archivo.ContentLength > 0)
                    {
                        if (!EsTipoImagenValido(archivo.ContentType))
                        {
                            return Json(new { respuesta = false, error = $"Tipo de archivo no válido: {archivo.FileName}" }, JsonRequestBehavior.AllowGet);
                        }
                        if (archivo.ContentLength > 5 * 1024 * 1024)
                        {
                            return Json(new { respuesta = false, error = $"Archivo muy grande: {archivo.FileName}. Máximo 5MB." }, JsonRequestBehavior.AllowGet);
                        }
                        using (System.IO.BinaryReader reader = new System.IO.BinaryReader(archivo.InputStream))
                        {
                            byte[] fotoBytes = reader.ReadBytes(archivo.ContentLength);
                            fotos.Add(fotoBytes);
                        }
                        System.Diagnostics.Debug.WriteLine($"✅ Archivo procesado: {archivo.FileName} ({archivo.ContentLength} bytes)");
                    }
                }

                if (fotos.Count == 0)
                {
                    return Json(new { respuesta = false, error = "No se recibieron archivos válidos" }, JsonRequestBehavior.AllowGet);
                }

                // Guardar fotos en la base de datos
                var resultado = ArticulosFotosMetodos.Instance.GuardarFotos(idVehiculo, fotos);
                if (resultado.Exito)
                {
                    return Json(new
                    {
                        respuesta = true,
                        mensaje = $"{fotos.Count} foto(s) subida(s) exitosamente",
                        cantidad = fotos.Count
                    }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(new { respuesta = false, error = "Error al guardar las fotos: " + resultado.MensajeError }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ ERROR en SubirFotos: {ex.Message}");
                return Json(new { respuesta = false, error = $"Error interno: {ex.Message}" }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet]
        public JsonResult ObtenerFotosVehiculo(int idVehiculo)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"=== OBTENIENDO FOTOS PARA VEHÍCULO {idVehiculo} ===");

                var fotosBase64 = ArticulosFotosMetodos.Instance.ObtenerFotosBase64PorArticulo(idVehiculo);

                return Json(new
                {
                    respuesta = true,
                    fotos = fotosBase64,
                    cantidad = fotosBase64.Count
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ ERROR en ObtenerFotosVehiculo: {ex.Message}");
                return Json(new { respuesta = false, error = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public JsonResult EliminarFoto(int idVehiculo, int secPhoto)
        {
            try
            {
                int idUsuario = Session["UsuarioID"] != null ? Convert.ToInt32(Session["UsuarioID"]) : 1;

                bool resultado = ArticulosFotosMetodos.Instance.EliminarFoto(idUsuario, idVehiculo, secPhoto);

                if (resultado)
                {
                    return Json(new { respuesta = true, mensaje = "Foto eliminada exitosamente" }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(new { respuesta = false, error = "Error al eliminar la foto" }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ ERROR en EliminarFoto: {ex.Message}");
                return Json(new { respuesta = false, error = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet]
        public JsonResult ContarFotosVehiculo(int idVehiculo)
        {
            try
            {
                int cantidad = ArticulosFotosMetodos.Instance.ContarFotos(idVehiculo);

                return Json(new
                {
                    respuesta = true,
                    cantidad = cantidad
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ ERROR en ContarFotosVehiculo: {ex.Message}");
                return Json(new { respuesta = false, error = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        // 🔧 MÉTODO AUXILIAR PARA VALIDAR TIPOS DE IMAGEN
        private bool EsTipoImagenValido(string contentType)
        {
            var tiposValidos = new string[]
            {
        "image/jpeg",
        "image/jpg",
        "image/png",
        "image/gif",
        "image/bmp",
        "image/webp"
            };

            return Array.Exists(tiposValidos, tipo => tipo.Equals(contentType, StringComparison.OrdinalIgnoreCase));
        }

        // 🔧 MÉTODO MODIFICADO PARA INCLUIR FOTOS EN ConsultaVehiculos
        [HttpGet]
        public JsonResult ConsultaVehiculosConFotos()
        {
            System.Diagnostics.Debug.WriteLine("=== CONSULTANDO VEHICULOS CON FOTOS ===");

            try
            {
                List<Vehiculos> oLista = VehiculosMetodos.Instance.Listar();

                // Agregar primera foto a cada vehículo
                foreach (var vehiculo in oLista)
                {
                    string primeraFoto = ArticulosFotosMetodos.Instance.ObtenerPrimeraFotoBase64(vehiculo.IDVehiculo);
                    vehiculo.PrimeraFoto = primeraFoto; // Necesitarás agregar esta propiedad al modelo Vehiculos

                    int cantidadFotos = ArticulosFotosMetodos.Instance.ContarFotos(vehiculo.IDVehiculo);
                    vehiculo.CantidadFotos = cantidadFotos; // También agregar esta propiedad
                }

                var jsonResult = Json(new { data = oLista }, JsonRequestBehavior.AllowGet);
                jsonResult.MaxJsonLength = int.MaxValue;

                return jsonResult;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ EXCEPCIÓN en ConsultaVehiculosConFotos: {ex.Message}");
                return Json(new { data = new List<Vehiculos>(), error = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }


        // Método para búsqueda con filtros
        [HttpGet]
        public JsonResult BuscarVehiculos(string marca = "", string tipo = "", decimal? precioMin = null, decimal? precioMax = null, string provincia = "")
        {
            try
            {
                List<Vehiculos> oLista = VehiculosMetodos.Instance.Listar();
                var vehiculosFiltrados = oLista.FindAll(v => v.Estatus == true && v.Vendido == false);

                // Aplicar filtros
                if (!string.IsNullOrEmpty(marca))
                {
                    vehiculosFiltrados = vehiculosFiltrados.FindAll(v => v.MarcaNombre.ToLower().Contains(marca.ToLower()));
                }

                if (!string.IsNullOrEmpty(tipo))
                {
                    vehiculosFiltrados = vehiculosFiltrados.FindAll(v => v.TipoNombre.ToLower().Contains(tipo.ToLower()));
                }

                if (precioMin.HasValue)
                {
                    vehiculosFiltrados = vehiculosFiltrados.FindAll(v => v.Precio >= precioMin.Value);
                }

                if (precioMax.HasValue)
                {
                    vehiculosFiltrados = vehiculosFiltrados.FindAll(v => v.Precio <= precioMax.Value);
                }

                if (!string.IsNullOrEmpty(provincia))
                {
                    vehiculosFiltrados = vehiculosFiltrados.FindAll(v => v.ProvinciaNombre.ToLower().Contains(provincia.ToLower()));
                }

                var jsonResult = Json(new { data = vehiculosFiltrados }, JsonRequestBehavior.AllowGet);
                jsonResult.MaxJsonLength = int.MaxValue;

                return jsonResult;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error en BuscarVehiculos: {ex.Message}");
                return Json(new { data = new List<Vehiculos>(), error = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }
    }
}