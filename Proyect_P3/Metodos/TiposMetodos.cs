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

                    // 🔥 QUERY IGUAL QUE EN MARCAS - FUNCIONANDO
                    SqlCommand cmd = new SqlCommand("SELECT * FROM TIPOS WHERE Estatus = 1 ORDER BY IDTipo DESC", oCnn);
                    cmd.CommandType = CommandType.Text;

                    oCnn.Open();
                    SqlDataReader dr = cmd.ExecuteReader();

                    int contador = 0;
                    while (dr.Read())
                    {
                        contador++;
                        var imagenBytes = dr["Imagen"] as byte[];

                        // Debug de cada registro
                        System.Diagnostics.Debug.WriteLine($"=== TIPO {contador} ===");
                        System.Diagnostics.Debug.WriteLine($"IDTipo: {dr["IDTipo"]}");
                        System.Diagnostics.Debug.WriteLine($"Descripcion: {dr["Descripcion"]}");
                        System.Diagnostics.Debug.WriteLine($"Estatus: {dr["Estatus"]} (tipo: {dr["Estatus"]?.GetType()})");
                        System.Diagnostics.Debug.WriteLine($"FechaRegistro: {dr["FechaRegistro"]}");
                        System.Diagnostics.Debug.WriteLine($"Imagen: {(imagenBytes != null ? $"{imagenBytes.Length} bytes" : "NULL")}");

                        var tipo = new Tipos()
                        {
                            IDTipo = Convert.ToInt32(dr["IDTipo"]),
                            Descripcion = dr["Descripcion"]?.ToString(),
                            Imagen = imagenBytes,
                            Estatus = dr["Estatus"] as bool?,
                            FechaRegistro = dr["FechaRegistro"] as DateTime?
                        };

                        oListaTipos.Add(tipo);
                        System.Diagnostics.Debug.WriteLine($"Tipo agregado a la lista. Total: {oListaTipos.Count}");
                    }

                    dr.Close();
                    System.Diagnostics.Debug.WriteLine($"=== RESUMEN FINAL ===");
                    System.Diagnostics.Debug.WriteLine($"Total tipos cargados: {oListaTipos.Count}");

                    // Debug final
                    foreach (var tipo in oListaTipos)
                    {
                        System.Diagnostics.Debug.WriteLine($"ID: {tipo.IDTipo}, Desc: {tipo.Descripcion}, Status: {tipo.Estatus}, Img: {(tipo.Imagen != null ? "SÍ" : "NO")}");
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"ERROR CRÍTICO al listar tipos: {ex.Message}");
                    System.Diagnostics.Debug.WriteLine($"StackTrace: {ex.StackTrace}");
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
                    System.Diagnostics.Debug.WriteLine($"Imagen: {(oTipos.Imagen != null ? $"{oTipos.Imagen.Length} bytes" : "NULL")}");

                    oConn.Open();

                    // 🔥 INSERT DIRECTO FUNCIONAL
                    string sql = @"
                        INSERT INTO TIPOS (Descripcion, Imagen, Estatus, FechaRegistro)
                        VALUES (@Descripcion, @Imagen, @Estatus, @FechaRegistro)";

                    SqlCommand cmd = new SqlCommand(sql, oConn);
                    cmd.Parameters.AddWithValue("@Descripcion", oTipos.Descripcion ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Imagen", (object)oTipos.Imagen ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@Estatus", oTipos.Estatus ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@FechaRegistro", oTipos.FechaRegistro ?? (object)DBNull.Value);

                    // DEBUG: Mostrar parámetros
                    System.Diagnostics.Debug.WriteLine("=== PARÁMETROS ENVIADOS ===");
                    System.Diagnostics.Debug.WriteLine($"@Descripcion: {oTipos.Descripcion}");
                    System.Diagnostics.Debug.WriteLine($"@Estatus: {oTipos.Estatus}");
                    System.Diagnostics.Debug.WriteLine($"@FechaRegistro: {oTipos.FechaRegistro}");
                    System.Diagnostics.Debug.WriteLine($"@Imagen: {(oTipos.Imagen != null ? $"{oTipos.Imagen.Length} bytes" : "NULL")}");

                    int filasAfectadas = cmd.ExecuteNonQuery();
                    respuesta = filasAfectadas > 0;

                    System.Diagnostics.Debug.WriteLine($"Filas afectadas: {filasAfectadas}, Resultado: {respuesta}");
                }
                catch (Exception ex)
                {
                    respuesta = false;
                    System.Diagnostics.Debug.WriteLine($"ERROR SQL COMPLETO: {ex.Message}");

                    if (ex is SqlException sqlEx)
                    {
                        System.Diagnostics.Debug.WriteLine($"SQL Error Number: {sqlEx.Number}");
                        System.Diagnostics.Debug.WriteLine($"SQL Error State: {sqlEx.State}");
                        System.Diagnostics.Debug.WriteLine($"SQL Error Severity: {sqlEx.Class}");
                        System.Diagnostics.Debug.WriteLine($"SQL Error Line: {sqlEx.LineNumber}");
                    }

                    System.Diagnostics.Debug.WriteLine($"STACK TRACE: {ex.StackTrace}");
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
                    System.Diagnostics.Debug.WriteLine("=== MODIFICAR TIPO ===");
                    System.Diagnostics.Debug.WriteLine($"IDTipo: {oTipos.IDTipo}");
                    System.Diagnostics.Debug.WriteLine($"Descripcion: {oTipos.Descripcion}");
                    System.Diagnostics.Debug.WriteLine($"Estatus: {oTipos.Estatus}");
                    System.Diagnostics.Debug.WriteLine($"Imagen: {(oTipos.Imagen != null ? $"{oTipos.Imagen.Length} bytes" : "NULL - no se cambiará")}");

                    oConn.Open();

                    // 🔥 UPDATE CONDICIONAL PARA IMAGEN (igual que Marcas)
                    string sql;
                    SqlCommand cmd;

                    if (oTipos.Imagen != null && oTipos.Imagen.Length > 0)
                    {
                        // Actualizar CON nueva imagen
                        sql = @"
                            UPDATE TIPOS 
                            SET Descripcion = @Descripcion, 
                                Imagen = @Imagen, 
                                Estatus = @Estatus
                            WHERE IDTipo = @IDTipo";

                        cmd = new SqlCommand(sql, oConn);
                        cmd.Parameters.AddWithValue("@IDTipo", oTipos.IDTipo);
                        cmd.Parameters.AddWithValue("@Descripcion", oTipos.Descripcion ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@Imagen", oTipos.Imagen);
                        cmd.Parameters.AddWithValue("@Estatus", oTipos.Estatus ?? (object)DBNull.Value);

                        System.Diagnostics.Debug.WriteLine("✅ Actualizando CON nueva imagen");
                    }
                    else
                    {
                        // Actualizar SIN cambiar imagen
                        sql = @"
                            UPDATE TIPOS 
                            SET Descripcion = @Descripcion, 
                                Estatus = @Estatus
                            WHERE IDTipo = @IDTipo";

                        cmd = new SqlCommand(sql, oConn);
                        cmd.Parameters.AddWithValue("@IDTipo", oTipos.IDTipo);
                        cmd.Parameters.AddWithValue("@Descripcion", oTipos.Descripcion ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@Estatus", oTipos.Estatus ?? (object)DBNull.Value);

                        System.Diagnostics.Debug.WriteLine("✅ Actualizando SIN cambiar imagen");
                    }

                    int filasAfectadas = cmd.ExecuteNonQuery();
                    respuesta = filasAfectadas > 0;

                    System.Diagnostics.Debug.WriteLine($"Filas afectadas: {filasAfectadas}, Resultado: {respuesta}");

                    if (!respuesta)
                    {
                        System.Diagnostics.Debug.WriteLine("❌ No se actualizó ninguna fila - revisar IDTipo o validaciones");
                    }
                }
                catch (Exception ex)
                {
                    respuesta = false;
                    System.Diagnostics.Debug.WriteLine($"❌ ERROR AL MODIFICAR: {ex.Message}");

                    if (ex is SqlException sqlEx)
                    {
                        System.Diagnostics.Debug.WriteLine($"SQL Error Number: {sqlEx.Number}");
                        System.Diagnostics.Debug.WriteLine($"SQL Error State: {sqlEx.State}");
                        System.Diagnostics.Debug.WriteLine($"SQL Error Severity: {sqlEx.Class}");
                        System.Diagnostics.Debug.WriteLine($"SQL Error Line: {sqlEx.LineNumber}");
                    }
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
                    System.Diagnostics.Debug.WriteLine($"=== ELIMINAR TIPO {idTipo} ===");

                    oConn.Open();

                    // 🔥 ELIMINACIÓN LÓGICA (igual que Marcas)
                    string sql = "UPDATE TIPOS SET Estatus = 0 WHERE IDTipo = @IDTipo";
                    SqlCommand cmd = new SqlCommand(sql, oConn);
                    cmd.Parameters.AddWithValue("@IDTipo", idTipo);

                    int filasAfectadas = cmd.ExecuteNonQuery();
                    respuesta = filasAfectadas > 0;

                    System.Diagnostics.Debug.WriteLine($"Filas afectadas: {filasAfectadas}, Resultado: {respuesta}");
                }
                catch (Exception ex)
                {
                    respuesta = false;
                    System.Diagnostics.Debug.WriteLine($"ERROR al eliminar tipo: {ex.Message}");
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
                    System.Diagnostics.Debug.WriteLine($"=== OBTENER TIPO POR ID: {idTipo} ===");

                    SqlCommand cmd = new SqlCommand("SELECT * FROM TIPOS WHERE IDTipo = @IDTipo", oCnn);
                    cmd.Parameters.AddWithValue("@IDTipo", idTipo);
                    cmd.CommandType = CommandType.Text;

                    oCnn.Open();
                    SqlDataReader dr = cmd.ExecuteReader();

                    if (dr.Read())
                    {
                        var imagenBytes = dr["Imagen"] as byte[];

                        oTipo = new Tipos()
                        {
                            IDTipo = Convert.ToInt32(dr["IDTipo"]),
                            Descripcion = dr["Descripcion"]?.ToString(),
                            Imagen = imagenBytes,
                            Estatus = dr["Estatus"] as bool?,
                            FechaRegistro = dr["FechaRegistro"] as DateTime?
                        };

                        System.Diagnostics.Debug.WriteLine($"✅ Tipo encontrado: {oTipo.Descripcion}, Imagen: {(imagenBytes != null ? "SÍ" : "NO")}");
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine($"❌ No se encontró tipo con ID: {idTipo}");
                    }

                    dr.Close();
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"ERROR al obtener tipo por ID: {ex.Message}");
                }
            }
            return oTipo;
        }
    }
}