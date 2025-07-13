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
        public bool GuardarFotos(int idCliente, int idArticulo, List<byte[]> fotos)
        {
            bool respuesta = false;
            using (SqlConnection oConn = new SqlConnection(Conexion.Bd))
            {
                try
                {
                    System.Diagnostics.Debug.WriteLine($"=== GUARDANDO {fotos.Count} FOTOS PARA VEHÍCULO {idArticulo} ===");

                    oConn.Open();

                    // Primero eliminar fotos existentes (si es una actualización)
                    EliminarFotosPorArticulo(idCliente, idArticulo, oConn);

                    // Guardar nuevas fotos
                    for (int i = 0; i < fotos.Count; i++)
                    {
                        string sql = @"
                            INSERT INTO ARTICULOSFOTOS (IDCliente, IDArticulo, SecPhoto, FOTO)
                            VALUES (@IDCliente, @IDArticulo, @SecPhoto, @FOTO)";

                        SqlCommand cmd = new SqlCommand(sql, oConn);
                        cmd.Parameters.AddWithValue("@IDCliente", idCliente);
                        cmd.Parameters.AddWithValue("@IDArticulo", idArticulo);
                        cmd.Parameters.AddWithValue("@SecPhoto", i + 1); // Secuencia desde 1
                        cmd.Parameters.AddWithValue("@FOTO", fotos[i]);

                        cmd.ExecuteNonQuery();
                        System.Diagnostics.Debug.WriteLine($"✅ Foto {i + 1} guardada");
                    }

                    respuesta = true;
                    System.Diagnostics.Debug.WriteLine($"✅ {fotos.Count} fotos guardadas exitosamente");
                }
                catch (Exception ex)
                {
                    respuesta = false;
                    System.Diagnostics.Debug.WriteLine($"❌ ERROR al guardar fotos: {ex.Message}");
                }
            }
            return respuesta;
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
                            IDCliente = Convert.ToInt32(dr["IDCliente"]),
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

        // 📸 OBTENER FOTOS EN FORMATO BASE64 PARA WEB
        public List<string> ObtenerFotosBase64PorArticulo(int idArticulo)
        {
            List<string> fotosBase64 = new List<string>();
            var fotos = ObtenerFotosPorArticulo(idArticulo);

            foreach (var foto in fotos)
            {
                if (foto.FOTO != null && foto.FOTO.Length > 0)
                {
                    string base64String = Convert.ToBase64String(foto.FOTO);
                    fotosBase64.Add($"data:image/jpeg;base64,{base64String}");
                }
            }

            return fotosBase64;
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

        // 🖼️ OBTENER PRIMERA FOTO (PARA THUMBNAIL)
        public string ObtenerPrimeraFotoBase64(int idArticulo)
        {
            string fotoBase64 = null;
            using (SqlConnection oCnn = new SqlConnection(Conexion.Bd))
            {
                try
                {
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
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"❌ ERROR al obtener primera foto: {ex.Message}");
                }
            }
            return fotoBase64;
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
    }
}