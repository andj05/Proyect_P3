using Proyect_P3.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace Proyect_P3.Metodos
{
    public class TiposMetodos
    {
        private static TiposMetodos _instance = null;

        public TiposMetodos() { }

        public static TiposMetodos Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new TiposMetodos();
                }
                return _instance;
            }
        }

        public List<Tipos> Listar()
        {
            List<Tipos> oListaTipos = new List<Tipos>();
            using (SqlConnection oCnn = new SqlConnection(Conexion.Bd))
            {
                try
                {
                    System.Diagnostics.Debug.WriteLine("=== INICIANDO LISTAR TIPOS ===");

                    SqlCommand cmd = new SqlCommand("SELECT * FROM TIPOS WHERE Estatus = 1", oCnn);
                    cmd.CommandType = CommandType.Text;

                    oCnn.Open();
                    SqlDataReader dr = cmd.ExecuteReader();

                    while (dr.Read())
                    {
                        var imagenBytes = dr["Imagen"] as byte[];

                        oListaTipos.Add(new Tipos()
                        {
                            IDTipo = Convert.ToInt32(dr["IDTipo"]),
                            Descripcion = dr["Descripcion"]?.ToString(),
                            Imagen = imagenBytes,
                            Estatus = dr["Estatus"] as bool?,
                            FechaRegistro = dr["FechaRegistro"] as DateTime?
                        });
                    }

                    dr.Close();
                    System.Diagnostics.Debug.WriteLine($"Total de tipos cargados: {oListaTipos.Count}");
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"ERROR en Listar: {ex.Message}");
                }
            }
            return oListaTipos;
        }

        public bool Registrar(Tipos oTipos)
        {
            bool respuesta = false;
            using (SqlConnection oConn = new SqlConnection(Conexion.Bd))
            {
                try
                {
                    System.Diagnostics.Debug.WriteLine("=== REGISTRAR TIPO ===");
                    System.Diagnostics.Debug.WriteLine($"Descripcion: '{oTipos.Descripcion}'");
                    System.Diagnostics.Debug.WriteLine($"Estatus: {oTipos.Estatus}");

                    oConn.Open();

                    // Usar INSERT directo por ahora
                    string sql = @"
                        INSERT INTO TIPOS (Descripcion, Imagen, Estatus, FechaRegistro)
                        VALUES (@Descripcion, @Imagen, @Estatus, @FechaRegistro)";

                    SqlCommand cmd = new SqlCommand(sql, oConn);
                    cmd.Parameters.AddWithValue("@Descripcion", oTipos.Descripcion ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Imagen", (object)oTipos.Imagen ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@Estatus", oTipos.Estatus ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@FechaRegistro", oTipos.FechaRegistro ?? (object)DBNull.Value);

                    int filasAfectadas = cmd.ExecuteNonQuery();
                    respuesta = filasAfectadas > 0;

                    System.Diagnostics.Debug.WriteLine($"Filas afectadas: {filasAfectadas}, Resultado: {respuesta}");
                }
                catch (Exception ex)
                {
                    respuesta = false;
                    System.Diagnostics.Debug.WriteLine($"ERROR SQL: {ex.Message}");
                }
            }
            return respuesta;
        }

        public bool Modificar(Tipos oTipos)
        {
            bool respuesta = false;
            using (SqlConnection oConn = new SqlConnection(Conexion.Bd))
            {
                try
                {
                    oConn.Open();

                    string sql = @"
                        UPDATE TIPOS 
                        SET Descripcion = @Descripcion, 
                            Imagen = @Imagen, 
                            Estatus = @Estatus
                        WHERE IDTipo = @IDTipo";

                    SqlCommand cmd = new SqlCommand(sql, oConn);
                    cmd.Parameters.AddWithValue("@IDTipo", oTipos.IDTipo);
                    cmd.Parameters.AddWithValue("@Descripcion", oTipos.Descripcion ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Imagen", (object)oTipos.Imagen ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@Estatus", oTipos.Estatus ?? (object)DBNull.Value);

                    int filasAfectadas = cmd.ExecuteNonQuery();
                    respuesta = filasAfectadas > 0;
                }
                catch (Exception ex)
                {
                    respuesta = false;
                    System.Diagnostics.Debug.WriteLine("Error al modificar tipo: " + ex.Message);
                }
            }
            return respuesta;
        }

        public bool Eliminar(int idTipo)
        {
            bool respuesta = false;
            using (SqlConnection oConn = new SqlConnection(Conexion.Bd))
            {
                try
                {
                    oConn.Open();

                    string sql = "UPDATE TIPOS SET Estatus = 0 WHERE IDTipo = @IDTipo";
                    SqlCommand cmd = new SqlCommand(sql, oConn);
                    cmd.Parameters.AddWithValue("@IDTipo", idTipo);

                    int filasAfectadas = cmd.ExecuteNonQuery();
                    respuesta = filasAfectadas > 0;
                }
                catch (Exception ex)
                {
                    respuesta = false;
                    System.Diagnostics.Debug.WriteLine("Error al eliminar tipo: " + ex.Message);
                }
            }
            return respuesta;
        }

        public Tipos ObtenerPorId(int idTipo)
        {
            Tipos oTipo = null;
            using (SqlConnection oCnn = new SqlConnection(Conexion.Bd))
            {
                try
                {
                    SqlCommand cmd = new SqlCommand("SELECT * FROM TIPOS WHERE IDTipo = @IDTipo", oCnn);
                    cmd.Parameters.AddWithValue("@IDTipo", idTipo);
                    cmd.CommandType = CommandType.Text;

                    oCnn.Open();
                    SqlDataReader dr = cmd.ExecuteReader();

                    if (dr.Read())
                    {
                        oTipo = new Tipos()
                        {
                            IDTipo = Convert.ToInt32(dr["IDTipo"]),
                            Descripcion = dr["Descripcion"]?.ToString(),
                            Imagen = dr["Imagen"] as byte[],
                            Estatus = dr["Estatus"] as bool?,
                            FechaRegistro = dr["FechaRegistro"] as DateTime?
                        };
                    }
                    dr.Close();
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine("Error al obtener tipo por ID: " + ex.Message);
                }
            }
            return oTipo;
        }
    }
}