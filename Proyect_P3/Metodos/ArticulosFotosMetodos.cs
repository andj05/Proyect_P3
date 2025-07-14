using Proyect_P3.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace Proyect_P3.Metodos
{
    public class ArticulosFotosMetodos
    {
        private static ArticulosFotosMetodos _instance = null;

        public ArticulosFotosMetodos() { }

        public static ArticulosFotosMetodos Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new ArticulosFotosMetodos();
                }
                return _instance;
            }
        }

        // 📸 GUARDAR FOTOS DE UN VEHÍCULO
        public ResultadoGuardarFotos GuardarFotos(int idArticulo, List<byte[]> fotos)
        {
            var resultado = new ResultadoGuardarFotos { Exito = false, MensajeError = "" };
            using (SqlConnection oConn = new SqlConnection(Conexion.Bd))
            {
                try
                {
                    System.Diagnostics.Debug.WriteLine($"=== GUARDARFOTOS: INICIO ===");
                    System.Diagnostics.Debug.WriteLine($"[INFO] idArticulo={idArticulo}, fotos.Count={fotos?.Count ?? 0}");

                    if (idArticulo <= 0)
                    {
                        resultado.MensajeError = "[ERROR] idArticulo inválido.";
                        System.Diagnostics.Debug.WriteLine(resultado.MensajeError);
                        return resultado;
                    }

                    if (fotos == null || fotos.Count == 0)
                    {
                        resultado.MensajeError = "[ERROR] La lista de fotos está vacía o es nula.";
                        System.Diagnostics.Debug.WriteLine(resultado.MensajeError);
                        return resultado;
                    }

                    oConn.Open();

                    // Eliminar fotos existentes
                    EliminarFotosPorArticulo(idArticulo, oConn);

                    // Guardar nuevas fotos
                    for (int i = 0; i < fotos.Count; i++)
                    {
                        var fotoBytes = fotos[i];
                        if (fotoBytes == null || fotoBytes.Length == 0)
                        {
                            System.Diagnostics.Debug.WriteLine($"[WARN] Foto {i + 1} está vacía, se omite.");
                            continue;
                        }

                        System.Diagnostics.Debug.WriteLine($"[INFO] Insertando foto {i + 1} de tamaño {fotoBytes.Length} bytes");

                        string sql = @"
                    INSERT INTO ARTICULOSFOTOS (IDArticulo, SecPhoto, FOTO)
                    VALUES (@IDArticulo, @SecPhoto, @FOTO)";

                        using (SqlCommand cmd = new SqlCommand(sql, oConn))
                        {
                            cmd.Parameters.AddWithValue("@IDArticulo", idArticulo);
                            cmd.Parameters.AddWithValue("@SecPhoto", i + 1);
                            cmd.Parameters.AddWithValue("@FOTO", fotoBytes);

                            int rows = cmd.ExecuteNonQuery();
                            if (rows > 0)
                                System.Diagnostics.Debug.WriteLine($"✅ Foto {i + 1} guardada correctamente");
                            else
                                System.Diagnostics.Debug.WriteLine($"[ERROR] No se insertó la foto {i + 1}");
                        }
                    }

                    resultado.Exito = true;
                    System.Diagnostics.Debug.WriteLine($"✅ {fotos.Count} fotos procesadas para el vehículo {idArticulo}");
                }
                catch (Exception ex)
                {
                    resultado.Exito = false;
                    resultado.MensajeError = ex.Message;
                    System.Diagnostics.Debug.WriteLine($"❌ ERROR al guardar fotos: {ex.Message}");
                    if (ex.InnerException != null)
                    {
                        resultado.MensajeError += " | " + ex.InnerException.Message;
                        System.Diagnostics.Debug.WriteLine($"❌ InnerException: {ex.InnerException.Message}");
                    }
                }
            }
            return resultado;
        }

        // Nuevo método privado para eliminar fotos solo por idArticulo
        private void EliminarFotosPorArticulo(int idArticulo, SqlConnection connection)
        {
            try
            {
                string sql = "DELETE FROM ARTICULOSFOTOS WHERE IDArticulo = @IDArticulo";
                SqlCommand cmd = new SqlCommand(sql, connection);
                cmd.Parameters.AddWithValue("@IDArticulo", idArticulo);

                int deleted = cmd.ExecuteNonQuery();
                System.Diagnostics.Debug.WriteLine($"🗑️ {deleted} fotos anteriores eliminadas");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ ERROR al eliminar fotos anteriores: {ex.Message}");
            }
        }

        // 📸 OBTENER FOTOS DE UN VEHÍCULO
        public List<ArticulosFotos> ObtenerFotosPorArticulo(int idArticulo)
        {
            List<ArticulosFotos> fotos = new List<ArticulosFotos>();
            using (SqlConnection oCnn = new SqlConnection(Conexion.Bd))
            {
                try
                {
                    System.Diagnostics.Debug.WriteLine($"=== OBTENIENDO FOTOS PARA VEHÍCULO {idArticulo} ===");

                    string sql = @"
                        SELECT IDCliente, IDArticulo, SecPhoto, FOTO
                        FROM ARTICULOSFOTOS 
                        WHERE IDArticulo = @IDArticulo
                        ORDER BY SecPhoto";

                    SqlCommand cmd = new SqlCommand(sql, oCnn);
                    cmd.Parameters.AddWithValue("@IDArticulo", idArticulo);

                    oCnn.Open();
                    SqlDataReader dr = cmd.ExecuteReader();

                    while (dr.Read())
                    {
                        var foto = new ArticulosFotos()
                        {
                            IDArticulo = Convert.ToInt32(dr["IDArticulo"]),
                            SecPhoto = Convert.ToInt32(dr["SecPhoto"]),
                            FOTO = dr["FOTO"] as byte[] ?? new byte[0]
                        };

                        fotos.Add(foto);
                    }

                    dr.Close();
                    System.Diagnostics.Debug.WriteLine($"✅ {fotos.Count} fotos obtenidas");
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"❌ ERROR al obtener fotos: {ex.Message}");
                }
            }
            return fotos;
        }

        // 🗑️ ELIMINAR FOTOS POR ARTÍCULO
        private void EliminarFotosPorArticulo(int idCliente, int idArticulo, SqlConnection connection)
        {
            try
            {
                string sql = "DELETE FROM ARTICULOSFOTOS WHERE IDCliente = @IDCliente AND IDArticulo = @IDArticulo";
                SqlCommand cmd = new SqlCommand(sql, connection);
                cmd.Parameters.AddWithValue("@IDCliente", idCliente);
                cmd.Parameters.AddWithValue("@IDArticulo", idArticulo);

                int deleted = cmd.ExecuteNonQuery();
                System.Diagnostics.Debug.WriteLine($"🗑️ {deleted} fotos anteriores eliminadas");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ ERROR al eliminar fotos anteriores: {ex.Message}");
            }
        }

        // 🗑️ ELIMINAR FOTOS POR ARTÍCULO (MÉTODO PÚBLICO)
        public bool EliminarFotos(int idCliente, int idArticulo)
        {
            bool respuesta = false;
            using (SqlConnection oConn = new SqlConnection(Conexion.Bd))
            {
                try
                {
                    oConn.Open();
                    EliminarFotosPorArticulo(idCliente, idArticulo, oConn);
                    respuesta = true;
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"❌ ERROR al eliminar fotos: {ex.Message}");
                    respuesta = false;
                }
            }
            return respuesta;
        }

        // 🗑️ ELIMINAR UNA FOTO ESPECÍFICA
        public bool EliminarFoto(int idCliente, int idArticulo, int secPhoto)
        {
            bool respuesta = false;
            using (SqlConnection oConn = new SqlConnection(Conexion.Bd))
            {
                try
                {
                    string sql = @"
                        DELETE FROM ARTICULOSFOTOS 
                        WHERE IDCliente = @IDCliente AND IDArticulo = @IDArticulo AND SecPhoto = @SecPhoto";

                    SqlCommand cmd = new SqlCommand(sql, oConn);
                    cmd.Parameters.AddWithValue("@IDCliente", idCliente);
                    cmd.Parameters.AddWithValue("@IDArticulo", idArticulo);
                    cmd.Parameters.AddWithValue("@SecPhoto", secPhoto);

                    oConn.Open();
                    int deleted = cmd.ExecuteNonQuery();
                    respuesta = deleted > 0;

                    System.Diagnostics.Debug.WriteLine($"🗑️ Foto {secPhoto} eliminada: {respuesta}");
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"❌ ERROR al eliminar foto específica: {ex.Message}");
                }
            }
            return respuesta;
        }

        // 📊 CONTAR FOTOS POR ARTÍCULO
        public int ContarFotos(int idArticulo)
        {
            int cantidad = 0;
            using (SqlConnection oCnn = new SqlConnection(Conexion.Bd))
            {
                try
                {
                    string sql = "SELECT COUNT(*) FROM ARTICULOSFOTOS WHERE IDArticulo = @IDArticulo";
                    SqlCommand cmd = new SqlCommand(sql, oCnn);
                    cmd.Parameters.AddWithValue("@IDArticulo", idArticulo);

                    oCnn.Open();
                    cantidad = (int)cmd.ExecuteScalar();
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"❌ ERROR al contar fotos: {ex.Message}");
                }
            }
            return cantidad;
        }

        // 📸 REORDENAR FOTOS
        public bool ReordenarFotos(int idCliente, int idArticulo, List<int> nuevoOrden)
        {
            bool respuesta = false;
            using (SqlConnection oConn = new SqlConnection(Conexion.Bd))
            {
                SqlTransaction transaction = null;
                try
                {
                    oConn.Open();
                    transaction = oConn.BeginTransaction();

                    for (int i = 0; i < nuevoOrden.Count; i++)
                    {
                        string sql = @"
                            UPDATE ARTICULOSFOTOS 
                            SET SecPhoto = @NuevoOrden
                            WHERE IDCliente = @IDCliente AND IDArticulo = @IDArticulo AND SecPhoto = @OrdenAntiguo";

                        SqlCommand cmd = new SqlCommand(sql, oConn, transaction);
                        cmd.Parameters.AddWithValue("@NuevoOrden", i + 1);
                        cmd.Parameters.AddWithValue("@IDCliente", idCliente);
                        cmd.Parameters.AddWithValue("@IDArticulo", idArticulo);
                        cmd.Parameters.AddWithValue("@OrdenAntiguo", nuevoOrden[i]);

                        cmd.ExecuteNonQuery();
                    }

                    transaction.Commit();
                    respuesta = true;
                    System.Diagnostics.Debug.WriteLine($"✅ Fotos reordenadas exitosamente");
                }
                catch (Exception ex)
                {
                    transaction?.Rollback();
                    System.Diagnostics.Debug.WriteLine($"❌ ERROR al reordenar fotos: {ex.Message}");
                }
            }
            return respuesta;
        }

        // 🔧 MÉTODOS CORREGIDOS Y AGREGADOS para ArticulosFotosMetodos

        // 🖼️ OBTENER PRIMERA FOTO SOLO BASE64 (SIN PREFIJO) - NUEVO MÉTODO
        public string ObtenerPrimeraFotoBase64SinPrefijo(int idArticulo)
        {
            string fotoBase64 = null;
            using (SqlConnection oCnn = new SqlConnection(Conexion.Bd))
            {
                try
                {
                    System.Diagnostics.Debug.WriteLine($"🖼️ Obteniendo primera foto para artículo {idArticulo}");

                    string sql = @"
                SELECT TOP 1 FOTO
                FROM ARTICULOSFOTOS 
                WHERE IDArticulo = @IDArticulo
                ORDER BY SecPhoto";

                    SqlCommand cmd = new SqlCommand(sql, oCnn);
                    cmd.Parameters.AddWithValue("@IDArticulo", idArticulo);

                    oCnn.Open();
                    byte[] foto = cmd.ExecuteScalar() as byte[];

                    if (foto != null && foto.Length > 0)
                    {
                        fotoBase64 = Convert.ToBase64String(foto);
                        System.Diagnostics.Debug.WriteLine($"✅ Primera foto obtenida: {fotoBase64.Length} caracteres");
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine($"❌ No se encontró primera foto para artículo {idArticulo}");
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"❌ ERROR al obtener primera foto: {ex.Message}");
                }
            }
            return fotoBase64;
        }

        // 📸 OBTENER FOTOS EN FORMATO BASE64 SIN PREFIJO (NUEVO MÉTODO)
        public List<string> ObtenerFotosBase64SinPrefijo(int idArticulo)
        {
            List<string> fotosBase64 = new List<string>();
            using (SqlConnection oCnn = new SqlConnection(Conexion.Bd))
            {
                try
                {
                    System.Diagnostics.Debug.WriteLine($"📸 Obteniendo todas las fotos para artículo {idArticulo}");

                    string sql = @"
                SELECT FOTO
                FROM ARTICULOSFOTOS 
                WHERE IDArticulo = @IDArticulo
                ORDER BY SecPhoto";

                    SqlCommand cmd = new SqlCommand(sql, oCnn);
                    cmd.Parameters.AddWithValue("@IDArticulo", idArticulo);

                    oCnn.Open();
                    SqlDataReader dr = cmd.ExecuteReader();

                    while (dr.Read())
                    {
                        byte[] foto = dr["FOTO"] as byte[];
                        if (foto != null && foto.Length > 0)
                        {
                            string base64String = Convert.ToBase64String(foto);
                            fotosBase64.Add(base64String);
                        }
                    }

                    dr.Close();
                    System.Diagnostics.Debug.WriteLine($"✅ {fotosBase64.Count} fotos obtenidas en Base64");
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"❌ ERROR al obtener fotos Base64: {ex.Message}");
                }
            }
            return fotosBase64;
        }

        // 🔧 CORREGIR EL MÉTODO EXISTENTE ObtenerPrimeraFotoBase64 (MANTENER COMPATIBILIDAD)
        public string ObtenerPrimeraFotoBase64(int idArticulo)
        {
            string fotoBase64 = null;
            using (SqlConnection oCnn = new SqlConnection(Conexion.Bd))
            {
                try
                {
                    System.Diagnostics.Debug.WriteLine($"🖼️ Obteniendo primera foto CON PREFIJO para artículo {idArticulo}");

                    string sql = @"
                SELECT TOP 1 FOTO
                FROM ARTICULOSFOTOS 
                WHERE IDArticulo = @IDArticulo
                ORDER BY SecPhoto";

                    SqlCommand cmd = new SqlCommand(sql, oCnn);
                    cmd.Parameters.AddWithValue("@IDArticulo", idArticulo);

                    oCnn.Open();
                    byte[] foto = cmd.ExecuteScalar() as byte[];

                    if (foto != null && foto.Length > 0)
                    {
                        fotoBase64 = $"data:image/jpeg;base64,{Convert.ToBase64String(foto)}";
                        System.Diagnostics.Debug.WriteLine($"✅ Primera foto con prefijo obtenida");
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine($"❌ No se encontró primera foto para artículo {idArticulo}");
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"❌ ERROR al obtener primera foto: {ex.Message}");
                }
            }
            return fotoBase64;
        }

        // 🔧 CORREGIR EL MÉTODO EXISTENTE ObtenerFotosBase64PorArticulo (MANTENER COMPATIBILIDAD)
        public List<string> ObtenerFotosBase64PorArticulo(int idArticulo)
        {
            List<string> fotosBase64 = new List<string>();
            using (SqlConnection oCnn = new SqlConnection(Conexion.Bd))
            {
                try
                {
                    System.Diagnostics.Debug.WriteLine($"📸 Obteniendo todas las fotos CON PREFIJO para artículo {idArticulo}");

                    string sql = @"
                SELECT FOTO
                FROM ARTICULOSFOTOS 
                WHERE IDArticulo = @IDArticulo
                ORDER BY SecPhoto";

                    SqlCommand cmd = new SqlCommand(sql, oCnn);
                    cmd.Parameters.AddWithValue("@IDArticulo", idArticulo);

                    oCnn.Open();
                    SqlDataReader dr = cmd.ExecuteReader();

                    while (dr.Read())
                    {
                        byte[] foto = dr["FOTO"] as byte[];
                        if (foto != null && foto.Length > 0)
                        {
                            string base64String = Convert.ToBase64String(foto);
                            fotosBase64.Add($"data:image/jpeg;base64,{base64String}");
                        }
                    }

                    dr.Close();
                    System.Diagnostics.Debug.WriteLine($"✅ {fotosBase64.Count} fotos con prefijo obtenidas");
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"❌ ERROR al obtener fotos Base64: {ex.Message}");
                }
            }
            return fotosBase64;
        }
    }
}