using Proyect_P3.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace Proyect_P3.Metodos
{
    public class MarcasMetodos
    {
        private static MarcasMetodos _instance = null;

        public MarcasMetodos() { }

        public static MarcasMetodos Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new MarcasMetodos();
                }
                return _instance;
            }
        }

        public List<Marcas> Listar()
        {
            List<Marcas> oListaMarcas = new List<Marcas>();
            using (SqlConnection oCnn = new SqlConnection(Conexion.Bd))
            {
                try
                {
                    System.Diagnostics.Debug.WriteLine("=== INICIANDO LISTAR MARCAS ===");

                    // CAMBIO CRÍTICO: Usar consulta SQL directa como en Tipos
                    // En lugar de stored procedure que puede tener problemas
                    SqlCommand cmd = new SqlCommand("SELECT * FROM MARCAS WHERE Estatus = 1 ORDER BY IdMarca DESC", oCnn);
                    cmd.CommandType = CommandType.Text;

                    oCnn.Open();
                    SqlDataReader dr = cmd.ExecuteReader();

                    int contador = 0;
                    while (dr.Read())
                    {
                        contador++;
                        var imagenBytes = dr["Imagen"] as byte[];
                        string imagenBase64 = null;

                        // Debug de cada registro
                        System.Diagnostics.Debug.WriteLine($"=== MARCA {contador} ===");
                        System.Diagnostics.Debug.WriteLine($"IdMarca: {dr["IdMarca"]}");
                        System.Diagnostics.Debug.WriteLine($"Descripcion: {dr["Descripcion"]}");
                        System.Diagnostics.Debug.WriteLine($"Estatus: {dr["Estatus"]} (tipo: {dr["Estatus"]?.GetType()})");
                        System.Diagnostics.Debug.WriteLine($"FechaRegistro: {dr["FechaRegistro"]}");
                        System.Diagnostics.Debug.WriteLine($"Imagen: {(imagenBytes != null ? $"{imagenBytes.Length} bytes" : "NULL")}");

                        // CONVERSIÓN A BASE64 - ESTO ES LO QUE FALTA EN TIPOS
                        if (imagenBytes != null && imagenBytes.Length > 0)
                        {
                            try
                            {
                                imagenBase64 = Convert.ToBase64String(imagenBytes);
                                System.Diagnostics.Debug.WriteLine($"ImagenBase64 generada: {imagenBase64.Length} caracteres");
                            }
                            catch (Exception ex)
                            {
                                System.Diagnostics.Debug.WriteLine($"Error al convertir imagen a base64: {ex.Message}");
                            }
                        }

                        var marca = new Marcas()
                        {
                            IdMarca = Convert.ToInt32(dr["IdMarca"]),
                            Descripcion = dr["Descripcion"]?.ToString(),
                            Imagen = imagenBytes,
                            ImagenBase64 = imagenBase64, // ⭐ CLAVE: Esto es lo que necesita Marcas
                            Estatus = dr["Estatus"] as bool?,
                            FechaRegistro = dr["FechaRegistro"] as DateTime?
                        };

                        oListaMarcas.Add(marca);
                        System.Diagnostics.Debug.WriteLine($"Marca agregada a la lista. Total: {oListaMarcas.Count}");
                    }

                    dr.Close();
                    System.Diagnostics.Debug.WriteLine($"=== RESUMEN FINAL ===");
                    System.Diagnostics.Debug.WriteLine($"Total marcas cargadas: {oListaMarcas.Count}");

                    // Debug final: mostrar cada marca cargada
                    foreach (var marca in oListaMarcas)
                    {
                        System.Diagnostics.Debug.WriteLine($"ID: {marca.IdMarca}, Desc: {marca.Descripcion}, Status: {marca.Estatus}, Img: {(marca.ImagenBase64 != null ? "SÍ" : "NO")}");
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"ERROR CRÍTICO al listar marcas: {ex.Message}");
                    System.Diagnostics.Debug.WriteLine($"StackTrace: {ex.StackTrace}");
                }
            }
            return oListaMarcas;
        }

        public bool Registrar(Marcas oMarcas)
        {
            bool respuesta = true;
            using (SqlConnection oConn = new SqlConnection(Conexion.Bd))
            {
                try
                {
                    System.Diagnostics.Debug.WriteLine("=== REGISTRAR EN MÉTODOS ===");
                    System.Diagnostics.Debug.WriteLine($"Conexión: {Conexion.Bd}");

                    oConn.Open();
                    System.Diagnostics.Debug.WriteLine("Conexión abierta exitosamente");

                    SqlCommand cmd = new SqlCommand("sp_insertaMarcas", oConn);
                    cmd.Parameters.AddWithValue("@Descripcion", oMarcas.Descripcion ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Imagen", (object)oMarcas.Imagen ?? DBNull.Value);

                    // Parámetro de salida
                    cmd.Parameters.Add("@Resultado", SqlDbType.Bit).Direction = ParameterDirection.Output;
                    cmd.CommandType = CommandType.StoredProcedure;

                    // DEBUG: Mostrar parámetros
                    System.Diagnostics.Debug.WriteLine("=== PARÁMETROS ENVIADOS AL SP ===");
                    System.Diagnostics.Debug.WriteLine($"@Descripcion: {oMarcas.Descripcion}");
                    System.Diagnostics.Debug.WriteLine($"@Imagen: {(oMarcas.Imagen != null ? $"{oMarcas.Imagen.Length} bytes" : "NULL")}");

                    cmd.ExecuteNonQuery();
                    System.Diagnostics.Debug.WriteLine("ExecuteNonQuery ejecutado");

                    respuesta = Convert.ToBoolean(cmd.Parameters["@Resultado"].Value);
                    System.Diagnostics.Debug.WriteLine($"@Resultado del SP: {respuesta}");
                }
                catch (Exception ex)
                {
                    respuesta = false;
                    System.Diagnostics.Debug.WriteLine($"ERROR SQL COMPLETO: {ex.Message}");
                    System.Diagnostics.Debug.WriteLine($"ERROR SQL NÚMERO: {((SqlException)ex).Number}");
                    System.Diagnostics.Debug.WriteLine($"ERROR SQL STATE: {((SqlException)ex).State}");
                    System.Diagnostics.Debug.WriteLine($"STACK TRACE: {ex.StackTrace}");
                    System.Diagnostics.Debug.WriteLine($"ERROR SQL NÚMERO: {((SqlException)ex).Number}");
                    Console.WriteLine("Error al registrar marca: " + ex.Message);
                }
                return respuesta;
            }
        }

        public bool Modificar(Marcas oMarcas)
        {
            bool respuesta = true;
            using (SqlConnection oConn = new SqlConnection(Conexion.Bd))
            {
                try
                {
                    oConn.Open();
                    SqlCommand cmd = new SqlCommand("sp_ModificaMarcas", oConn);
                    cmd.Parameters.AddWithValue("@IdMarca", oMarcas.IdMarca);
                    cmd.Parameters.AddWithValue("@Descripcion", oMarcas.Descripcion ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Estatus", oMarcas.Estatus ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Imagen", (object)oMarcas.Imagen ?? DBNull.Value);

                    // Parámetro de salida
                    cmd.Parameters.Add("@Resultado", SqlDbType.Bit).Direction = ParameterDirection.Output;
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.ExecuteNonQuery();
                    respuesta = Convert.ToBoolean(cmd.Parameters["@Resultado"].Value);
                }
                catch (Exception ex)
                {
                    respuesta = false;
                    System.Diagnostics.Debug.WriteLine($"ERROR AL MODIFICAR: {ex.Message}");
                    Console.WriteLine("Error al modificar marca: " + ex.Message);
                }
            }
            return respuesta;
        }

        public bool Eliminar(int idMarca)
        {
            bool respuesta = true;
            using (SqlConnection oConn = new SqlConnection(Conexion.Bd))
            {
                try
                {
                    oConn.Open();
                    // Eliminación lógica por IdMarca (cambiar Estatus a 0)
                    string sBorrar = "UPDATE MARCAS SET Estatus = 0 WHERE IdMarca = @IdMarca";
                    SqlCommand cmd = new SqlCommand(sBorrar, oConn);
                    cmd.Parameters.AddWithValue("@IdMarca", idMarca);
                    cmd.CommandType = CommandType.Text;

                    int filasAfectadas = cmd.ExecuteNonQuery();
                    respuesta = filasAfectadas > 0;
                }
                catch (Exception ex)
                {
                    respuesta = false;
                    Console.WriteLine("Error al eliminar marca: " + ex.Message);
                }
            }
            return respuesta;
        }

        public Marcas ObtenerPorIdMarca(int idMarca)
        {
            Marcas oMarca = null;
            using (SqlConnection oCnn = new SqlConnection(Conexion.Bd))
            {
                try
                {
                    SqlCommand cmd = new SqlCommand("SELECT * FROM MARCAS WHERE IdMarca = @IdMarca", oCnn);
                    cmd.Parameters.AddWithValue("@IdMarca", idMarca);
                    cmd.CommandType = CommandType.Text;

                    oCnn.Open();
                    SqlDataReader dr = cmd.ExecuteReader();

                    if (dr.Read())
                    {
                        oMarca = new Marcas()
                        {
                            IdMarca = Convert.ToInt32(dr["IdMarca"]),
                            Descripcion = dr["Descripcion"].ToString(),
                            Imagen = dr["Imagen"] as byte[],
                            Estatus = dr["Estatus"] as bool?,
                            FechaRegistro = dr["FechaRegistro"] as DateTime?
                        };
                    }
                    dr.Close();
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error al obtener marca por IdMarca: " + ex.Message);
                }
            }
            return oMarca;
        }
    }
}