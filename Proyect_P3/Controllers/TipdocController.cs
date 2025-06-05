using iTextSharp.text;
using iTextSharp.text.pdf;
using Proyect_P3.Models; 
using Proyect_P3.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
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
        // Método HTTP GET que genera un PDF para un documento específico basado en su ID
        // Archivo PDF para descarga o error 404 si no se encuentra
        [HttpGet]
        public ActionResult GeneratePdf(int id)
        {
            // Inicializar modelo que contendrá los datos del documento
            DetailsTipdocQueryViewModels model = null;

            // Conexión a la base de datos usando Entity Framework
            using (var db = new DBMVCEntities())
            {
                // Buscar el documento por ID en la tabla TIPDOC
                var tipdoc = db.TIPDOCs.Find(id);

                // Si el documento existe, mapear los datos al modelo de vista
                if (tipdoc != null)
                {
                    model = new DetailsTipdocQueryViewModels
                    {
                        Id = tipdoc.ID,                              // ID único del documento
                        TipoDoc = tipdoc.TIPDOC1,                   // Tipo de documento
                        Descripcion = tipdoc.DESCRIPCION,           // Descripción del documento
                        Origen = tipdoc.ORIGEN,                     // Origen del documento
                        Contador = tipdoc.CONTADOR,                 // Contador del sistema
                        CuentaDebito = tipdoc.CTADEBITO,           // Cuenta contable de débito
                        CuentaCredito = tipdoc.CTACREDITO,         // Cuenta contable de crédito
                        Estatus = tipdoc.ESTATUS ?? 0              // Estado (activo/inactivo), default 0
                    };
                }
            }

            // Si no se encontró el documento, retornar error 404
            if (model == null)
            {
                return HttpNotFound();
            }

            // Generar el PDF con los datos del modelo
            var pdfBytes = GenerateFormattedPdf(model);

            // Crear nombre de archivo único con timestamp
            var fileName = $"TipoDocumento_{model.TipoDoc}_{model.Id}_{DateTime.Now:yyyyMMdd_HHmmss}.pdf";

            // Retornar el archivo PDF para descarga
            return File(pdfBytes, "application/pdf", fileName);
        }

        // Método privado que genera el contenido del PDF formateado
 
       // Array de bytes del PDF generado
        private byte[] GenerateFormattedPdf(DetailsTipdocQueryViewModels model)
        {
            // Usar MemoryStream para manejar el PDF en memoria
            using (var ms = new MemoryStream())
            {
                // Crear documento PDF con tamaño A4 y márgenes (izq, der, arriba, abajo)
                var document = new Document(PageSize.A4, 40, 40, 50, 50);

                // Crear escritor PDF vinculado al documento y al stream
                var writer = PdfWriter.GetInstance(document, ms);

                // Abrir el documento para comenzar a agregar contenido
                document.Open();

                // === DEFINICIÓN DE FUENTES ===
                // Fuente para títulos principales - Helvetica Bold 18pt
                var titleFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 18, BaseColor.BLACK);

                // Fuente para etiquetas/labels - Helvetica Bold 9pt
                var labelFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 9, BaseColor.BLACK);

                // Fuente para valores/contenido regular - Helvetica 10pt
                var valueFont = FontFactory.GetFont(FontFactory.HELVETICA, 10, BaseColor.BLACK);

                // Fuente para encabezados de tabla - Helvetica Bold 9pt
                var tableHeaderFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 9, BaseColor.BLACK);

                // Fuente para celdas de tabla - Helvetica 9pt
                var tableCellFont = FontFactory.GetFont(FontFactory.HELVETICA, 9, BaseColor.BLACK);

                // Fuente para pie de página - Helvetica 8pt gris
                var footerFont = FontFactory.GetFont(FontFactory.HELVETICA, 8, BaseColor.GRAY);

                // === CONSTRUCCIÓN DEL DOCUMENTO ===
                // Agregar encabezado con título, número de documento y logo
                AddSimpleHeader(document, model, titleFont, labelFont, valueFont);

                // Agregar información del documento (tipo, descripción, origen, estado)
                AddDocumentInfo(document, model, labelFont, valueFont);

                // Agregar espacio en blanco pequeño para separación
                document.Add(new Paragraph(" ", FontFactory.GetFont(FontFactory.HELVETICA, 6)));

                // Agregar tabla principal con la configuración del documento
                AddMainTable(document, model, tableHeaderFont, tableCellFont);

                // Agregar sección de resumen/totales
                AddSummarySection(document, model, labelFont, valueFont);

                // Agregar pie de página con información del sistema
                AddSimpleFooter(document, footerFont);

                // Cerrar el documento (importante para finalizar la escritura)
                document.Close();

                // Retornar el contenido del PDF como array de bytes
                return ms.ToArray();
            }
        }


        // Agrega el encabezado del documento con título, información básica y logo

        private void AddSimpleHeader(Document document, DetailsTipdocQueryViewModels model, Font titleFont, Font labelFont, Font valueFont)
        {
            // Crear tabla para el encabezado con 2 columnas
            var headerTable = new PdfPTable(2);
            headerTable.WidthPercentage = 100;           // Ocupar todo el ancho disponible
            headerTable.SetWidths(new float[] { 2f, 1f }); // Columna izq más ancha que la derecha
            headerTable.SpacingAfter = 15;               // Espacio después de la tabla

            // === CELDA IZQUIERDA - INFORMACIÓN DEL DOCUMENTO ===
            var leftCell = new PdfPCell();
            leftCell.Border = PdfPCell.NO_BORDER;        // Sin bordes visibles
            leftCell.VerticalAlignment = Element.ALIGN_TOP; // Alinear contenido arriba

            // Título principal del documento
            var titleParagraph = new Paragraph("TIPO DE DOCUMENTO", titleFont);
            titleParagraph.SpacingAfter = 5;             // Espacio después del título
            leftCell.AddElement(titleParagraph);

            // Número de documento con formato de 6 dígitos
            var docNumberParagraph = new Paragraph();
            docNumberParagraph.Add(new Chunk("Documento n.° ", labelFont));
            docNumberParagraph.Add(new Chunk($"{model.Id:D6}", valueFont)); // D6 = 6 dígitos con ceros
            leftCell.AddElement(docNumberParagraph);

            // Fecha actual de generación
            var dateParagraph = new Paragraph();
            dateParagraph.Add(new Chunk("Fecha       ", labelFont));
            dateParagraph.Add(new Chunk(DateTime.Now.ToString("dd/MM/yyyy"), valueFont));
            leftCell.AddElement(dateParagraph);

            // === CELDA DERECHA - ÁREA DEL LOGO ===
            var rightCell = new PdfPCell();
            rightCell.Border = PdfPCell.NO_BORDER;       // Sin bordes visibles
            rightCell.MinimumHeight = 80;                // Altura mínima para el logo
            rightCell.VerticalAlignment = Element.ALIGN_MIDDLE;   // Centrar verticalmente
            rightCell.HorizontalAlignment = Element.ALIGN_CENTER; // Centrar horizontalmente

            try
            {
                // Intentar cargar la imagen del logo desde el servidor
                string imagePath = Server.MapPath("~/Content/images/Code_panda.png");

                // Verificar si el archivo de imagen existe
                if (System.IO.File.Exists(imagePath))
                {
                    // Cargar la imagen usando iTextSharp
                    var logoImage = Image.GetInstance(imagePath);

                    // === CÁLCULO DE ESCALA PROPORCIONAL ===
                    // Definir dimensiones máximas para la imagen
                    float maxWidth = 70f;   // Ancho máximo permitido
                    float maxHeight = 70f;  // Altura máxima permitida

                    // Calcular factores de escala para mantener proporciones
                    float scaleWidth = maxWidth / logoImage.Width;   // Factor de escala horizontal
                    float scaleHeight = maxHeight / logoImage.Height; // Factor de escala vertical

                    // Usar el menor factor para mantener la imagen dentro de los límites
                    float scale = Math.Min(scaleWidth, scaleHeight);

                    // Aplicar la escala calculada (multiplicar por 100 para porcentaje)
                    logoImage.ScalePercent(scale * 100);

                    // Centrar la imagen en la celda
                    logoImage.Alignment = Element.ALIGN_CENTER;

                    // Agregar la imagen escalada a la celda
                    rightCell.AddElement(logoImage);
                }
                else
                {
                    // === FALLBACK 1: Si no se encuentra la imagen ===
                    // Crear texto alternativo con fondo gris
                    var logoText = new Paragraph("Sistema\nDocumental",
                        FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 12, BaseColor.WHITE));
                    logoText.Alignment = Element.ALIGN_CENTER;
                    rightCell.BackgroundColor = new BaseColor(60, 60, 60); // Fondo gris oscuro
                    rightCell.AddElement(logoText);
                }
            }
            catch (Exception)
            {
                // === FALLBACK 2: Si hay error al cargar la imagen ===
                // Crear texto alternativo con fondo gris (mismo que fallback 1)
                var logoText = new Paragraph("Sistema\nDocumental",
                    FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 12, BaseColor.WHITE));
                logoText.Alignment = Element.ALIGN_CENTER;
                rightCell.BackgroundColor = new BaseColor(60, 60, 60);
                rightCell.AddElement(logoText);
            }

            // Agregar ambas celdas a la tabla del encabezado
            headerTable.AddCell(leftCell);
            headerTable.AddCell(rightCell);

            // Agregar la tabla completa al documento
            document.Add(headerTable);
        }


        /// Agrega la información detallada del documento (tipo, descripción, origen, estado)

        private void AddDocumentInfo(Document document, DetailsTipdocQueryViewModels model, Font labelFont, Font valueFont)
        {
            // Crear tabla de una sola columna para la información del documento
            var infoTable = new PdfPTable(1);
            infoTable.WidthPercentage = 100;             // Ocupar todo el ancho
            infoTable.SpacingAfter = 10;                 // Espacio después de la tabla

            // Celda principal sin bordes para contener toda la información
            var infoCell = new PdfPCell();
            infoCell.Border = PdfPCell.NO_BORDER;        // Sin bordes visibles
            infoCell.Padding = 0;                        // Sin padding interno

            // Mostrar el tipo de documento como subtítulo
            var titleParagraph = new Paragraph($"{model.TipoDoc ?? "N/A"}",
                FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 12, BaseColor.BLACK));
            titleParagraph.SpacingAfter = 3;             // Pequeño espacio después
            infoCell.AddElement(titleParagraph);

            // Mostrar descripción si existe
            if (!string.IsNullOrEmpty(model.Descripcion))
            {
                var descParagraph = new Paragraph(model.Descripcion, valueFont);
                descParagraph.SpacingAfter = 8;          // Espacio después de la descripción
                infoCell.AddElement(descParagraph);
            }

            // Crear línea con información de origen y estado
            var detailsParagraph = new Paragraph();
            detailsParagraph.Add(new Chunk($"Origen: {model.Origen ?? "N/A"} | ", labelFont));

            // Determinar texto del estado basado en el valor numérico
            var statusText = (model.Estatus == 1 ? "Activo" : "Inactivo");
            detailsParagraph.Add(new Chunk($"Estado: {statusText}", labelFont));

            infoCell.AddElement(detailsParagraph);

            // Agregar la celda a la tabla y la tabla al documento
            infoTable.AddCell(infoCell);
            document.Add(infoTable);
        }

        // Agrega la tabla principal con la configuración del documento

        private void AddMainTable(Document document, DetailsTipdocQueryViewModels model, Font headerFont, Font cellFont)
        {
            // Crear tabla con 4 columnas
            var mainTable = new PdfPTable(4);
            mainTable.WidthPercentage = 100;             // Ocupar todo el ancho
            mainTable.SetWidths(new float[] { 3f, 1f, 2f, 2f }); // Anchos relativos de columnas
            mainTable.SpacingAfter = 15;                 // Espacio después de la tabla

            // === ENCABEZADOS DE COLUMNA ===
            AddTableHeader(mainTable, "Configuración", headerFont);  // Descripción de la configuración
            AddTableHeader(mainTable, "Tipo", headerFont);           // Tipo de dato
            AddTableHeader(mainTable, "Valor", headerFont);          // Valor actual
            AddTableHeader(mainTable, "Estado", headerFont);         // Estado de configuración

            // === LÍNEA SEPARADORA DEBAJO DE ENCABEZADOS ===
            // Agregar celdas vacías con borde superior para crear línea separadora
            for (int i = 0; i < 4; i++)
            {
                var separatorCell = new PdfPCell();
                separatorCell.BorderWidth = 0;           // Sin bordes por defecto
                separatorCell.BorderWidthTop = 1;        // Solo borde superior
                separatorCell.BorderColorTop = BaseColor.BLACK; // Color negro para la línea
                separatorCell.FixedHeight = 1;           // Altura mínima para la línea
                mainTable.AddCell(separatorCell);
            }

            // === FILAS DE DATOS ===

            // Fila 1: Información del contador del sistema
            AddTableRow(mainTable, "Contador del sistema", "Numérico",
                       model.Contador.ToString(), "Configurado", cellFont);

            // Fila 2: Información de cuenta de débito
            var debitoValue = string.IsNullOrEmpty(model.CuentaDebito) ? "No configurada" : model.CuentaDebito;
            var debitoStatus = string.IsNullOrEmpty(model.CuentaDebito) ? "Pendiente" : "Configurado";
            AddTableRow(mainTable, "Cuenta de débito", "Contable",
                       debitoValue, debitoStatus, cellFont);

            // Fila 3: Información de cuenta de crédito
            var creditoValue = string.IsNullOrEmpty(model.CuentaCredito) ? "No configurada" : model.CuentaCredito;
            var creditoStatus = string.IsNullOrEmpty(model.CuentaCredito) ? "Pendiente" : "Configurado";
            AddTableRow(mainTable, "Cuenta de crédito", "Contable",
                       creditoValue, creditoStatus, cellFont);

            // Agregar la tabla completa al documento
            document.Add(mainTable);
        }


        // Agrega una celda de encabezado a la tabla

        private void AddTableHeader(PdfPTable table, string text, Font font)
        {
            // Crear celda de encabezado
            var cell = new PdfPCell(new Phrase(text, font));
            cell.Border = PdfPCell.NO_BORDER;            // Sin bordes visibles
            cell.PaddingBottom = 8;                      // Espacio abajo del texto
            cell.PaddingTop = 5;                         // Espacio arriba del texto
            table.AddCell(cell);
        }

        /// Agrega una fila de datos a la tabla principal

        private void AddTableRow(PdfPTable table, string config, string tipo, string valor, string estado, Font font)
        {
            // === CELDA DE CONFIGURACIÓN ===
            var configCell = new PdfPCell(new Phrase(config, font));
            configCell.Border = PdfPCell.NO_BORDER;      // Sin bordes
            configCell.PaddingTop = 5;                   // Espaciado vertical
            configCell.PaddingBottom = 5;
            table.AddCell(configCell);

            // === CELDA DE TIPO ===
            var tipoCell = new PdfPCell(new Phrase(tipo, font));
            tipoCell.Border = PdfPCell.NO_BORDER;
            tipoCell.PaddingTop = 5;
            tipoCell.PaddingBottom = 5;
            table.AddCell(tipoCell);

            // === CELDA DE VALOR ===
            var valorCell = new PdfPCell(new Phrase(valor, font));
            valorCell.Border = PdfPCell.NO_BORDER;
            valorCell.PaddingTop = 5;
            valorCell.PaddingBottom = 5;
            table.AddCell(valorCell);

            // === CELDA DE ESTADO (con color condicional) ===
            // Cambiar color según el estado: negro para "Configurado", gris para otros
            var estadoColor = estado == "Configurado" ? BaseColor.BLACK : BaseColor.GRAY;
            var estadoCell = new PdfPCell(new Phrase(estado,
                FontFactory.GetFont(FontFactory.HELVETICA, 9, estadoColor)));
            estadoCell.Border = PdfPCell.NO_BORDER;
            estadoCell.PaddingTop = 5;
            estadoCell.PaddingBottom = 5;
            table.AddCell(estadoCell);
        }

        // Agrega la sección de resumen con mensaje de confirmación y estado final

        private void AddSummarySection(Document document, DetailsTipdocQueryViewModels model, Font labelFont, Font valueFont)
        {
            // === LÍNEA SEPARADORA ===
            // Crear línea separadora horizontal
            var line = new Chunk(new iTextSharp.text.pdf.draw.LineSeparator(
                0.5f,           // Grosor de la línea
                100f,           // Porcentaje del ancho (100% = todo el ancho)
                BaseColor.BLACK, // Color de la línea
                Element.ALIGN_CENTER, // Alineación
                -2              // Offset vertical
            ));
            document.Add(new Paragraph(line));

            // === TABLA DE RESUMEN ===
            var summaryTable = new PdfPTable(2);
            summaryTable.WidthPercentage = 100;          // Ocupar todo el ancho
            summaryTable.SetWidths(new float[] { 3f, 1f }); // Columna izq más ancha
            summaryTable.SpacingBefore = 10;             // Espacio antes de la tabla
            summaryTable.SpacingAfter = 20;              // Espacio después de la tabla

            // === CELDA IZQUIERDA - MENSAJE DE CONFIRMACIÓN ===
            var leftCell = new PdfPCell();
            leftCell.Border = PdfPCell.NO_BORDER;        // Sin bordes
            leftCell.VerticalAlignment = Element.ALIGN_TOP; // Alinear arriba

            // Mensaje de confirmación en negrita
            var messageParagraph = new Paragraph("¡Configuración completada!",
                FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 11, BaseColor.BLACK));
            leftCell.AddElement(messageParagraph);

            // === CELDA DERECHA - ESTADO FINAL ===
            var rightCell = new PdfPCell();
            rightCell.Border = PdfPCell.NO_BORDER;       // Sin bordes
            rightCell.HorizontalAlignment = Element.ALIGN_RIGHT; // Alinear a la derecha

            // Determinar texto del estado final
            var statusText = model.Estatus == 1 ? "Activo" : "Inactivo";

            // Crear párrafo con etiqueta y valor del estado
            var statusParagraph = new Paragraph();
            statusParagraph.Add(new Chunk("Estado:        ", labelFont));
            statusParagraph.Add(new Chunk(statusText,
                FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 12, BaseColor.BLACK)));
            statusParagraph.Alignment = Element.ALIGN_RIGHT; // Alinear contenido a la derecha
            rightCell.AddElement(statusParagraph);

            // Agregar ambas celdas a la tabla
            summaryTable.AddCell(leftCell);
            summaryTable.AddCell(rightCell);

            // Agregar tabla de resumen al documento
            document.Add(summaryTable);
        }


        // Agrega el pie de página con información del sistema y contacto

        private void AddSimpleFooter(Document document, Font footerFont)
        {
            // === LÍNEA SEPARADORA SUPERIOR ===
            // Crear línea separadora gris antes del pie de página
            var line = new Chunk(new iTextSharp.text.pdf.draw.LineSeparator(
                0.5f,           // Grosor de línea
                100f,           // Ancho completo
                BaseColor.GRAY, // Color gris (más suave que el anterior)
                Element.ALIGN_CENTER,
                -2
            ));
            document.Add(new Paragraph(line));

            // === TABLA DEL PIE DE PÁGINA ===
            var footerTable = new PdfPTable(2);
            footerTable.WidthPercentage = 100;           // Ocupar todo el ancho
            footerTable.SpacingBefore = 8;               // Espacio antes de la tabla
            footerTable.SetWidths(new float[] { 1f, 1f }); // Columnas de igual ancho

            // === CELDA IZQUIERDA - INFORMACIÓN DEL SISTEMA ===
            var contactCell = new PdfPCell();
            contactCell.Border = PdfPCell.NO_BORDER;     // Sin bordes

            var contactParagraph = new Paragraph();
            // Título de la sección en gris y negrita
            contactParagraph.Add(new Chunk("Información del sistema\n",
                FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 8, BaseColor.GRAY)));
            // Nombre del sistema
            contactParagraph.Add(new Chunk("Sistema de Tipo de documentos\n", footerFont));
            // Fecha y hora de generación del PDF
            contactParagraph.Add(new Chunk($"Generado: {DateTime.Now:dd/MM/yyyy HH:mm}", footerFont));
            contactCell.AddElement(contactParagraph);

            // === CELDA DERECHA - INFORMACIÓN DE CONTACTO ===
            var projectCell = new PdfPCell();
            projectCell.Border = PdfPCell.NO_BORDER;     // Sin bordes
            projectCell.HorizontalAlignment = Element.ALIGN_RIGHT; // Alineación a la derecha

            var projectParagraph = new Paragraph();
            // Título de la sección de contacto
            projectParagraph.Add(new Chunk("Contacto\n",
                FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 8, BaseColor.GRAY)));
            // Nombre del proyecto
            projectParagraph.Add(new Chunk("Proyecto_P3_Anddy_Jara\n", footerFont));
            // Email de contacto
            projectParagraph.Add(new Chunk("jaraj6023@gmail.com \n", footerFont));

            projectParagraph.Add(new Chunk("https://github.com/andj05/Proyect_P3.git", footerFont));
            projectParagraph.Alignment = Element.ALIGN_RIGHT; // Alinear texto a la derecha
            projectCell.AddElement(projectParagraph);

            // Agregar ambas celdas a la tabla del pie de página
            footerTable.AddCell(contactCell);
            footerTable.AddCell(projectCell);

            // Agregar tabla del pie de página al documento
            document.Add(footerTable);
        }
    }
}