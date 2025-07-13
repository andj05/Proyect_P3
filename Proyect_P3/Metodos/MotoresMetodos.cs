using Proyect_P3.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace Proyect_P3.Metodos
{
    public class MotoresMetodos
    {
        private static MotoresMetodos _instance = null;

        public MotoresMetodos() { }

        public static MotoresMetodos Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new MotoresMetodos();
                }
                return _instance;
            }
        }

        public List<Motores> Listar()
        {
            List<Motores> oListaMotores = new List<Motores>();
            using (SqlConnection oCnn = new SqlConnection(Conexion.Bd))
            {
                try
                {
                    System.Diagnostics.Debug.WriteLine("=== INICIANDO LISTAR MOTORES ===");

                    SqlCommand cmd = new SqlCommand("SELECT * FROM MOTORES ORDER BY IdMotor DESC", oCnn);
                    cmd.CommandType = CommandType.Text;

                    oCnn.Open();
                    SqlDataReader dr = cmd.ExecuteReader();

                    while (dr.Read())
                    {
                        var motor = new Motores()
                        {
                            IdMotor = Convert.ToInt32(dr["IdMotor"]),
                            Descripcion = dr["Descripcion"]?.ToString()?.Trim(),
                            Combustible = dr["Combustible"]?.ToString()?.Trim(),
                            Transmisión = dr["Transmisión"]?.ToString()?.Trim()
                        };

                        oListaMotores.Add(motor);
                    }

                    dr.Close();
                    System.Diagnostics.Debug.WriteLine($"Total motores cargados: {oListaMotores.Count}");
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"ERROR al listar motores: {ex.Message}");
                }
            }
            return oListaMotores;
        }

        public bool Registrar(Motores oMotor)
        {
            bool respuesta = false;
            using (SqlConnection oConn = new SqlConnection(Conexion.Bd))
            {
                try
                {
                    System.Diagnostics.Debug.WriteLine("=== REGISTRAR MOTOR ===");

                    oConn.Open();

                    string sql = @"
                        INSERT INTO MOTORES (Descripcion, Combustible, Transmisión)
                        VALUES (@Descripcion, @Combustible, @Transmision)";

                    SqlCommand cmd = new SqlCommand(sql, oConn);
                    cmd.Parameters.AddWithValue("@Descripcion", oMotor.Descripcion ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Combustible", oMotor.Combustible ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Transmision", oMotor.Transmisión ?? (object)DBNull.Value);

                    int filasAfectadas = cmd.ExecuteNonQuery();
                    respuesta = filasAfectadas > 0;

                    System.Diagnostics.Debug.WriteLine($"Motor registrado: {respuesta}");
                }
                catch (Exception ex)
                {
                    respuesta = false;
                    System.Diagnostics.Debug.WriteLine($"ERROR al registrar motor: {ex.Message}");
                }
            }
            return respuesta;
        }

        public bool Modificar(Motores oMotor)
        {
            bool respuesta = false;
            using (SqlConnection oConn = new SqlConnection(Conexion.Bd))
            {
                try
                {
                    System.Diagnostics.Debug.WriteLine("=== MODIFICAR MOTOR ===");

                    oConn.Open();

                    string sql = @"
                        UPDATE MOTORES 
                        SET Descripcion = @Descripcion, 
                            Combustible = @Combustible, 
                            Transmisión = @Transmision
                        WHERE IdMotor = @IdMotor";

                    SqlCommand cmd = new SqlCommand(sql, oConn);
                    cmd.Parameters.AddWithValue("@IdMotor", oMotor.IdMotor);
                    cmd.Parameters.AddWithValue("@Descripcion", oMotor.Descripcion ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Combustible", oMotor.Combustible ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Transmision", oMotor.Transmisión ?? (object)DBNull.Value);

                    int filasAfectadas = cmd.ExecuteNonQuery();
                    respuesta = filasAfectadas > 0;

                    System.Diagnostics.Debug.WriteLine($"Motor modificado: {respuesta}");
                }
                catch (Exception ex)
                {
                    respuesta = false;
                    System.Diagnostics.Debug.WriteLine($"ERROR al modificar motor: {ex.Message}");
                }
            }
            return respuesta;
        }

        public bool Eliminar(int idMotor)
        {
            bool respuesta = false;
            using (SqlConnection oConn = new SqlConnection(Conexion.Bd))
            {
                try
                {
                    System.Diagnostics.Debug.WriteLine($"=== ELIMINAR MOTOR {idMotor} ===");

                    oConn.Open();

                    // Verificar si el motor está siendo usado por algún vehículo
                    string sqlVerificar = "SELECT COUNT(*) FROM VEHICULOS WHERE IdMotor = @IdMotor AND Estatus = 1";
                    SqlCommand cmdVerificar = new SqlCommand(sqlVerificar, oConn);
                    cmdVerificar.Parameters.AddWithValue("@IdMotor", idMotor);
                    
                    int vehiculosUsando = (int)cmdVerificar.ExecuteScalar();
                    
                    if (vehiculosUsando > 0)
                    {
                        System.Diagnostics.Debug.WriteLine($"No se puede eliminar. Motor usado por {vehiculosUsando} vehículos");
                        return false;
                    }

                    // Eliminación física ya que no tiene campo Estatus
                    string sql = "DELETE FROM MOTORES WHERE IdMotor = @IdMotor";
                    SqlCommand cmd = new SqlCommand(sql, oConn);
                    cmd.Parameters.AddWithValue("@IdMotor", idMotor);

                    int filasAfectadas = cmd.ExecuteNonQuery();
                    respuesta = filasAfectadas > 0;

                    System.Diagnostics.Debug.WriteLine($"Motor eliminado: {respuesta}");
                }
                catch (Exception ex)
                {
                    respuesta = false;
                    System.Diagnostics.Debug.WriteLine($"ERROR al eliminar motor: {ex.Message}");
                }
            }
            return respuesta;
        }

        public Motores ObtenerPorId(int idMotor)
        {
            Motores oMotor = null;
            using (SqlConnection oCnn = new SqlConnection(Conexion.Bd))
            {
                try
                {
                    System.Diagnostics.Debug.WriteLine($"=== OBTENER MOTOR POR ID: {idMotor} ===");

                    SqlCommand cmd = new SqlCommand("SELECT * FROM MOTORES WHERE IdMotor = @IdMotor", oCnn);
                    cmd.Parameters.AddWithValue("@IdMotor", idMotor);
                    cmd.CommandType = CommandType.Text;

                    oCnn.Open();
                    SqlDataReader dr = cmd.ExecuteReader();

                    if (dr.Read())
                    {
                        oMotor = new Motores()
                        {
                            IdMotor = Convert.ToInt32(dr["IdMotor"]),
                            Descripcion = dr["Descripcion"]?.ToString()?.Trim(),
                            Combustible = dr["Combustible"]?.ToString()?.Trim(),
                            Transmisión = dr["Transmisión"]?.ToString()?.Trim()
                        };

                        System.Diagnostics.Debug.WriteLine($"✅ Motor encontrado: {oMotor.Descripcion}");
                    }

                    dr.Close();
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"ERROR al obtener motor por ID: {ex.Message}");
                }
            }
            return oMotor;
        }
    }
}