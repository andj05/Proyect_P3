using Proyect_P3.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace Proyect_P3.Metodos
{
    public class CategoriasMetodos
    {
        private static CategoriasMetodos _instance = null;

        public CategoriasMetodos() { }

        public static CategoriasMetodos Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new CategoriasMetodos();
                }
                return _instance;
            }
        }

        public List<Categorias> Listar()
        {
            List<Categorias> oListaCategorias = new List<Categorias>();
            using (SqlConnection oCnn = new SqlConnection(Conexion.Bd))
            {
                try
                {
                    System.Diagnostics.Debug.WriteLine("=== INICIANDO LISTAR CATEGORIAS ===");

                    // Query igual que en Tipos - funcionando
                    SqlCommand cmd = new SqlCommand("SELECT * FROM CATEGORIAS WHERE Estatus = 1 ORDER BY IdCategoria DESC", oCnn);
                    cmd.CommandType = CommandType.Text;

                    oCnn.Open();
                    SqlDataReader dr = cmd.ExecuteReader();

                    int contador = 0;
                    while (dr.Read())
                    {
                        contador++;
                        var imagenBytes = dr["Imagen"] as byte[];

                        // Debug de cada registro
                        System.Diagnostics.Debug.WriteLine($"=== CATEGORIA {contador} ===");
                        System.Diagnostics.Debug.WriteLine($"IdCategoria: {dr["IdCategoria"]}");
                        System.Diagnostics.Debug.WriteLine($"Descripcion: {dr["Descripcion"]}");
                        System.Diagnostics.Debug.WriteLine($"Estatus: {dr["Estatus"]} (tipo: {dr["Estatus"]?.GetType()})");
                        System.Diagnostics.Debug.WriteLine($"FechaRegistro: {dr["FechaRegistro"]}");
                        System.Diagnostics.Debug.WriteLine($"Imagen: {(imagenBytes != null ? $"{imagenBytes.Length} bytes" : "NULL")}");

                        var categoria = new Categorias()
                        {
                            IdCategoria = Convert.ToInt32(dr["IdCategoria"]),
                            Descripcion = dr["Descripcion"]?.ToString(),
                            Imagen = imagenBytes,
                            Estatus = dr["Estatus"] as bool?,
                            FechaRegistro = dr["FechaRegistro"] as DateTime?
                        };

                        oListaCategorias.Add(categoria);
                        System.Diagnostics.Debug.WriteLine($"Categoria agregada a la lista. Total: {oListaCategorias.Count}");
                    }

                    dr.Close();
                    System.Diagnostics.Debug.WriteLine($"=== RESUMEN FINAL ===");
                    System.Diagnostics.Debug.WriteLine($"Total categorias cargadas: {oListaCategorias.Count}");

                    // Debug final
                    foreach (var categoria in oListaCategorias)
                    {
                        System.Diagnostics.Debug.WriteLine($"ID: {categoria.IdCategoria}, Desc: {categoria.Descripcion}, Status: {categoria.Estatus}, Img: {(categoria.Imagen != null ? "SÍ" : "NO")}");
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"ERROR CRÍTICO al listar categorias: {ex.Message}");
                    System.Diagnostics.Debug.WriteLine($"StackTrace: {ex.StackTrace}");
                }
            }
            return oListaCategorias;
        }

        public bool Registrar(Categorias oCategorias)
        {
            bool respuesta = false;
            using (SqlConnection oConn = new SqlConnection(Conexion.Bd))
            {
                try
                {
                    System.Diagnostics.Debug.WriteLine("=== REGISTRAR CATEGORIA ===");
                    System.Diagnostics.Debug.WriteLine($"Descripcion: '{oCategorias.Descripcion}'");
                    System.Diagnostics.Debug.WriteLine($"Estatus: {oCategorias.Estatus}");
                    System.Diagnostics.Debug.WriteLine($"Imagen: {(oCategorias.Imagen != null ? $"{oCategorias.Imagen.Length} bytes" : "NULL")}");

                    oConn.Open();

                    // INSERT directo funcional
                    string sql = @"
                        INSERT INTO CATEGORIAS (Descripcion, Imagen, Estatus, FechaRegistro)
                        VALUES (@Descripcion, @Imagen, @Estatus, @FechaRegistro)";

                    SqlCommand cmd = new SqlCommand(sql, oConn);
                    cmd.Parameters.AddWithValue("@Descripcion", oCategorias.Descripcion ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Imagen", (object)oCategorias.Imagen ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@Estatus", oCategorias.Estatus ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@FechaRegistro", oCategorias.FechaRegistro ?? (object)DBNull.Value);

                    // DEBUG: Mostrar parámetros
                    System.Diagnostics.Debug.WriteLine("=== PARÁMETROS ENVIADOS ===");
                    System.Diagnostics.Debug.WriteLine($"@Descripcion: {oCategorias.Descripcion}");
                    System.Diagnostics.Debug.WriteLine($"@Estatus: {oCategorias.Estatus}");
                    System.Diagnostics.Debug.WriteLine($"@FechaRegistro: {oCategorias.FechaRegistro}");
                    System.Diagnostics.Debug.WriteLine($"@Imagen: {(oCategorias.Imagen != null ? $"{oCategorias.Imagen.Length} bytes" : "NULL")}");

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

        public bool Modificar(Categorias oCategorias)
        {
            bool respuesta = false;
            using (SqlConnection oConn = new SqlConnection(Conexion.Bd))
            {
                try
                {
                    System.Diagnostics.Debug.WriteLine("=== MODIFICAR CATEGORIA ===");
                    System.Diagnostics.Debug.WriteLine($"IdCategoria: {oCategorias.IdCategoria}");
                    System.Diagnostics.Debug.WriteLine($"Descripcion: {oCategorias.Descripcion}");
                    System.Diagnostics.Debug.WriteLine($"Estatus: {oCategorias.Estatus}");
                    System.Diagnostics.Debug.WriteLine($"Imagen: {(oCategorias.Imagen != null ? $"{oCategorias.Imagen.Length} bytes" : "NULL - no se cambiará")}");

                    oConn.Open();

                    // UPDATE condicional para imagen (igual que Tipos)
                    string sql;
                    SqlCommand cmd;

                    if (oCategorias.Imagen != null && oCategorias.Imagen.Length > 0)
                    {
                        // Actualizar CON nueva imagen
                        sql = @"
                            UPDATE CATEGORIAS 
                            SET Descripcion = @Descripcion, 
                                Imagen = @Imagen, 
                                Estatus = @Estatus
                            WHERE IdCategoria = @IdCategoria";

                        cmd = new SqlCommand(sql, oConn);
                        cmd.Parameters.AddWithValue("@IdCategoria", oCategorias.IdCategoria);
                        cmd.Parameters.AddWithValue("@Descripcion", oCategorias.Descripcion ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@Imagen", oCategorias.Imagen);
                        cmd.Parameters.AddWithValue("@Estatus", oCategorias.Estatus ?? (object)DBNull.Value);

                        System.Diagnostics.Debug.WriteLine("✅ Actualizando CON nueva imagen");
                    }
                    else
                    {
                        // Actualizar SIN cambiar imagen
                        sql = @"
                            UPDATE CATEGORIAS 
                            SET Descripcion = @Descripcion, 
                                Estatus = @Estatus
                            WHERE IdCategoria = @IdCategoria";

                        cmd = new SqlCommand(sql, oConn);
                        cmd.Parameters.AddWithValue("@IdCategoria", oCategorias.IdCategoria);
                        cmd.Parameters.AddWithValue("@Descripcion", oCategorias.Descripcion ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@Estatus", oCategorias.Estatus ?? (object)DBNull.Value);

                        System.Diagnostics.Debug.WriteLine("✅ Actualizando SIN cambiar imagen");
                    }

                    int filasAfectadas = cmd.ExecuteNonQuery();
                    respuesta = filasAfectadas > 0;

                    System.Diagnostics.Debug.WriteLine($"Filas afectadas: {filasAfectadas}, Resultado: {respuesta}");

                    if (!respuesta)
                    {
                        System.Diagnostics.Debug.WriteLine("❌ No se actualizó ninguna fila - revisar IdCategoria o validaciones");
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

        public bool Eliminar(int idCategoria)
        {
            bool respuesta = false;
            using (SqlConnection oConn = new SqlConnection(Conexion.Bd))
            {
                try
                {
                    System.Diagnostics.Debug.WriteLine($"=== ELIMINAR CATEGORIA {idCategoria} ===");

                    oConn.Open();

                    // Eliminación lógica (igual que Tipos)
                    string sql = "UPDATE CATEGORIAS SET Estatus = 0 WHERE IdCategoria = @IdCategoria";
                    SqlCommand cmd = new SqlCommand(sql, oConn);
                    cmd.Parameters.AddWithValue("@IdCategoria", idCategoria);

                    int filasAfectadas = cmd.ExecuteNonQuery();
                    respuesta = filasAfectadas > 0;

                    System.Diagnostics.Debug.WriteLine($"Filas afectadas: {filasAfectadas}, Resultado: {respuesta}");
                }
                catch (Exception ex)
                {
                    respuesta = false;
                    System.Diagnostics.Debug.WriteLine($"ERROR al eliminar categoria: {ex.Message}");
                }
            }
            return respuesta;
        }

        public Categorias ObtenerPorId(int idCategoria)
        {
            Categorias oCategoria = null;
            using (SqlConnection oCnn = new SqlConnection(Conexion.Bd))
            {
                try
                {
                    System.Diagnostics.Debug.WriteLine($"=== OBTENER CATEGORIA POR ID: {idCategoria} ===");

                    SqlCommand cmd = new SqlCommand("SELECT * FROM CATEGORIAS WHERE IdCategoria = @IdCategoria", oCnn);
                    cmd.Parameters.AddWithValue("@IdCategoria", idCategoria);
                    cmd.CommandType = CommandType.Text;

                    oCnn.Open();
                    SqlDataReader dr = cmd.ExecuteReader();

                    if (dr.Read())
                    {
                        var imagenBytes = dr["Imagen"] as byte[];

                        oCategoria = new Categorias()
                        {
                            IdCategoria = Convert.ToInt32(dr["IdCategoria"]),
                            Descripcion = dr["Descripcion"]?.ToString(),
                            Imagen = imagenBytes,
                            Estatus = dr["Estatus"] as bool?,
                            FechaRegistro = dr["FechaRegistro"] as DateTime?
                        };

                        System.Diagnostics.Debug.WriteLine($"✅ Categoria encontrada: {oCategoria.Descripcion}, Imagen: {(imagenBytes != null ? "SÍ" : "NO")}");
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine($"❌ No se encontró categoria con ID: {idCategoria}");
                    }

                    dr.Close();
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"ERROR al obtener categoria por ID: {ex.Message}");
                }
            }
            return oCategoria;
        }
    }
}