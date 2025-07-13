using Proyect_P3.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace Proyect_P3.Metodos
{
    public class VehiculosMetodos
    {
        private static VehiculosMetodos _instance = null;

        public VehiculosMetodos() { }

        public static VehiculosMetodos Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new VehiculosMetodos();
                }
                return _instance;
            }
        }

        public List<Vehiculos> Listar()
        {
            List<Vehiculos> oListaVehiculos = new List<Vehiculos>();
            using (SqlConnection oCnn = new SqlConnection(Conexion.Bd))
            {
                try
                {
                    System.Diagnostics.Debug.WriteLine("=== INICIANDO LISTAR VEHICULOS ===");

                    string sql = @"
                        SELECT v.*, u.Nombre as NombreUsuario, u.Email as EmailUsuario,
                               c.Descripcion as CategoriaNombre, m.Descripcion as MarcaNombre,
                               t.Descripcion as TipoNombre, mo.Descripcion as MotorDescripcion,
                               mo.Combustible, mo.Transmisión as Transmision,
                               mun.NombreMunicipio as MunicipioNombre, p.NombreProvincia as ProvinciaNombre
                        FROM VEHICULOS v
                        INNER JOIN USERS u ON v.IDUsuario = u.ID
                        INNER JOIN CATEGORIAS c ON v.IdCategoria = c.IdCategoria
                        INNER JOIN MARCAS m ON v.IdMarca = m.IdMarca
                        INNER JOIN TIPOS t ON v.IDTipo = t.IDTipo
                        INNER JOIN MOTORES mo ON v.IdMotor = mo.IdMotor
                        INNER JOIN Municipios mun ON v.CodigoMunicipio = mun.CodigoMunicipio
                        INNER JOIN Provincias p ON mun.CodigoProvincia = p.CodigoProvincia
                        WHERE v.Estatus = 1 
                        ORDER BY v.FechaRegistro DESC";

                    SqlCommand cmd = new SqlCommand(sql, oCnn);
                    cmd.CommandType = CommandType.Text;

                    oCnn.Open();
                    SqlDataReader dr = cmd.ExecuteReader();

                    while (dr.Read())
                    {
                        var vehiculo = new Vehiculos()
                        {
                            IDVehiculo = Convert.ToInt32(dr["IDVehiculo"]),
                            IDUsuario = Convert.ToInt32(dr["IDUsuario"]),
                            IdCategoria = Convert.ToInt32(dr["IdCategoria"]),
                            IdMarca = Convert.ToInt32(dr["IdMarca"]),
                            IDTipo = Convert.ToInt32(dr["IDTipo"]),
                            IdMotor = Convert.ToInt32(dr["IdMotor"]),
                            CodigoMunicipio = Convert.ToInt32(dr["CodigoMunicipio"]),
                            Modelo = dr["Modelo"]?.ToString(),
                            Año = Convert.ToInt32(dr["Año"]),
                            Color = dr["Color"]?.ToString(),
                            Placa = dr["Placa"]?.ToString(),
                            NumeroChasis = dr["NumeroChasis"]?.ToString(),
                            Kilometraje = dr["Kilometraje"] as decimal?,
                            Precio = Convert.ToDecimal(dr["Precio"]),
                            Descripcion = dr["Descripcion"]?.ToString(),
                            Condicion = dr["Condicion"]?.ToString(),
                            Estatus = Convert.ToBoolean(dr["Estatus"]),
                            FechaRegistro = Convert.ToDateTime(dr["FechaRegistro"]),
                            FechaActualizacion = dr["FechaActualizacion"] as DateTime?,
                            Contacto = dr["Contacto"]?.ToString(),
                            Observaciones = dr["Observaciones"]?.ToString(),
                            Vendido = Convert.ToBoolean(dr["Vendido"]),
                            FechaVenta = dr["FechaVenta"] as DateTime?,
                            Destacado = Convert.ToBoolean(dr["Destacado"]),
                            NumeroVistas = Convert.ToInt32(dr["NumeroVistas"]),

                            // Propiedades adicionales
                            NombreUsuario = dr["NombreUsuario"]?.ToString(),
                            EmailUsuario = dr["EmailUsuario"]?.ToString(),
                            CategoriaNombre = dr["CategoriaNombre"]?.ToString(),
                            MarcaNombre = dr["MarcaNombre"]?.ToString(),
                            TipoNombre = dr["TipoNombre"]?.ToString(),
                            MotorDescripcion = dr["MotorDescripcion"]?.ToString(),
                            Combustible = dr["Combustible"]?.ToString(),
                            Transmision = dr["Transmision"]?.ToString(),
                            MunicipioNombre = dr["MunicipioNombre"]?.ToString(),
                            ProvinciaNombre = dr["ProvinciaNombre"]?.ToString()
                        };

                        oListaVehiculos.Add(vehiculo);
                    }

                    dr.Close();
                    System.Diagnostics.Debug.WriteLine($"Total vehículos cargados: {oListaVehiculos.Count}");
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"ERROR al listar vehículos: {ex.Message}");
                }
            }
            return oListaVehiculos;
        }

        public bool Registrar(Vehiculos oVehiculo)
        {
            bool respuesta = false;
            using (SqlConnection oConn = new SqlConnection(Conexion.Bd))
            {
                try
                {
                    System.Diagnostics.Debug.WriteLine("=== REGISTRAR VEHICULO ===");

                    oConn.Open();

                    string sql = @"
                        INSERT INTO VEHICULOS (IDUsuario, IdCategoria, IdMarca, IDTipo, IdMotor, CodigoMunicipio,
                                             Modelo, Año, Color, Placa, NumeroChasis, Kilometraje, Precio,
                                             Descripcion, Condicion, Contacto, Observaciones, Estatus, Destacado)
                        VALUES (@IDUsuario, @IdCategoria, @IdMarca, @IDTipo, @IdMotor, @CodigoMunicipio,
                                @Modelo, @Año, @Color, @Placa, @NumeroChasis, @Kilometraje, @Precio,
                                @Descripcion, @Condicion, @Contacto, @Observaciones, @Estatus, @Destacado)";

                    SqlCommand cmd = new SqlCommand(sql, oConn);
                    cmd.Parameters.AddWithValue("@IDUsuario", oVehiculo.IDUsuario);
                    cmd.Parameters.AddWithValue("@IdCategoria", oVehiculo.IdCategoria);
                    cmd.Parameters.AddWithValue("@IdMarca", oVehiculo.IdMarca);
                    cmd.Parameters.AddWithValue("@IDTipo", oVehiculo.IDTipo);
                    cmd.Parameters.AddWithValue("@IdMotor", oVehiculo.IdMotor);
                    cmd.Parameters.AddWithValue("@CodigoMunicipio", oVehiculo.CodigoMunicipio);
                    cmd.Parameters.AddWithValue("@Modelo", oVehiculo.Modelo ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Año", oVehiculo.Año);
                    cmd.Parameters.AddWithValue("@Color", oVehiculo.Color ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Placa", oVehiculo.Placa ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@NumeroChasis", oVehiculo.NumeroChasis ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Kilometraje", (object)oVehiculo.Kilometraje ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@Precio", oVehiculo.Precio);
                    cmd.Parameters.AddWithValue("@Descripcion", oVehiculo.Descripcion ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Condicion", oVehiculo.Condicion ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Contacto", oVehiculo.Contacto ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Observaciones", oVehiculo.Observaciones ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Estatus", oVehiculo.Estatus);
                    cmd.Parameters.AddWithValue("@Destacado", oVehiculo.Destacado);

                    int filasAfectadas = cmd.ExecuteNonQuery();
                    respuesta = filasAfectadas > 0;

                    System.Diagnostics.Debug.WriteLine($"Vehículo registrado: {respuesta}");
                }
                catch (Exception ex)
                {
                    respuesta = false;
                    System.Diagnostics.Debug.WriteLine($"ERROR al registrar vehículo: {ex.Message}");
                }
            }
            return respuesta;
        }

        public bool Modificar(Vehiculos oVehiculo)
        {
            bool respuesta = false;
            using (SqlConnection oConn = new SqlConnection(Conexion.Bd))
            {
                try
                {
                    System.Diagnostics.Debug.WriteLine("=== MODIFICAR VEHICULO ===");

                    oConn.Open();

                    string sql = @"
                        UPDATE VEHICULOS 
                        SET IdCategoria = @IdCategoria, IdMarca = @IdMarca, IDTipo = @IDTipo, 
                            IdMotor = @IdMotor, CodigoMunicipio = @CodigoMunicipio,
                            Modelo = @Modelo, Año = @Año, Color = @Color, Placa = @Placa,
                            NumeroChasis = @NumeroChasis, Kilometraje = @Kilometraje, Precio = @Precio,
                            Descripcion = @Descripcion, Condicion = @Condicion, Contacto = @Contacto,
                            Observaciones = @Observaciones, Estatus = @Estatus, Destacado = @Destacado,
                            FechaActualizacion = GETDATE()
                        WHERE IDVehiculo = @IDVehiculo";

                    SqlCommand cmd = new SqlCommand(sql, oConn);
                    cmd.Parameters.AddWithValue("@IDVehiculo", oVehiculo.IDVehiculo);
                    cmd.Parameters.AddWithValue("@IdCategoria", oVehiculo.IdCategoria);
                    cmd.Parameters.AddWithValue("@IdMarca", oVehiculo.IdMarca);
                    cmd.Parameters.AddWithValue("@IDTipo", oVehiculo.IDTipo);
                    cmd.Parameters.AddWithValue("@IdMotor", oVehiculo.IdMotor);
                    cmd.Parameters.AddWithValue("@CodigoMunicipio", oVehiculo.CodigoMunicipio);
                    cmd.Parameters.AddWithValue("@Modelo", oVehiculo.Modelo ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Año", oVehiculo.Año);
                    cmd.Parameters.AddWithValue("@Color", oVehiculo.Color ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Placa", oVehiculo.Placa ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@NumeroChasis", oVehiculo.NumeroChasis ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Kilometraje", (object)oVehiculo.Kilometraje ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@Precio", oVehiculo.Precio);
                    cmd.Parameters.AddWithValue("@Descripcion", oVehiculo.Descripcion ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Condicion", oVehiculo.Condicion ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Contacto", oVehiculo.Contacto ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Observaciones", oVehiculo.Observaciones ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Estatus", oVehiculo.Estatus);
                    cmd.Parameters.AddWithValue("@Destacado", oVehiculo.Destacado);

                    int filasAfectadas = cmd.ExecuteNonQuery();
                    respuesta = filasAfectadas > 0;

                    System.Diagnostics.Debug.WriteLine($"Vehículo modificado: {respuesta}");
                }
                catch (Exception ex)
                {
                    respuesta = false;
                    System.Diagnostics.Debug.WriteLine($"ERROR al modificar vehículo: {ex.Message}");
                }
            }
            return respuesta;
        }

        public bool Eliminar(int idVehiculo)
        {
            bool respuesta = false;
            using (SqlConnection oConn = new SqlConnection(Conexion.Bd))
            {
                try
                {
                    System.Diagnostics.Debug.WriteLine($"=== ELIMINAR VEHICULO {idVehiculo} ===");

                    oConn.Open();

                    // Eliminación lógica
                    string sql = "UPDATE VEHICULOS SET Estatus = 0 WHERE IDVehiculo = @IDVehiculo";
                    SqlCommand cmd = new SqlCommand(sql, oConn);
                    cmd.Parameters.AddWithValue("@IDVehiculo", idVehiculo);

                    int filasAfectadas = cmd.ExecuteNonQuery();
                    respuesta = filasAfectadas > 0;

                    System.Diagnostics.Debug.WriteLine($"Vehículo eliminado: {respuesta}");
                }
                catch (Exception ex)
                {
                    respuesta = false;
                    System.Diagnostics.Debug.WriteLine($"ERROR al eliminar vehículo: {ex.Message}");
                }
            }
            return respuesta;
        }

        public Vehiculos ObtenerPorId(int idVehiculo)
        {
            Vehiculos oVehiculo = null;
            using (SqlConnection oCnn = new SqlConnection(Conexion.Bd))
            {
                try
                {
                    System.Diagnostics.Debug.WriteLine($"=== OBTENER VEHICULO POR ID: {idVehiculo} ===");

                    string sql = @"
                        SELECT v.*, u.Nombre as NombreUsuario, u.Email as EmailUsuario,
                               c.Descripcion as CategoriaNombre, m.Descripcion as MarcaNombre,
                               t.Descripcion as TipoNombre, mo.Descripcion as MotorDescripcion,
                               mo.Combustible, mo.Transmisión as Transmision,
                               mun.NombreMunicipio as MunicipioNombre, p.NombreProvincia as ProvinciaNombre
                        FROM VEHICULOS v
                        INNER JOIN USERS u ON v.IDUsuario = u.ID
                        INNER JOIN CATEGORIAS c ON v.IdCategoria = c.IdCategoria
                        INNER JOIN MARCAS m ON v.IdMarca = m.IdMarca
                        INNER JOIN TIPOS t ON v.IDTipo = t.IDTipo
                        INNER JOIN MOTORES mo ON v.IdMotor = mo.IdMotor
                        INNER JOIN Municipios mun ON v.CodigoMunicipio = mun.CodigoMunicipio
                        INNER JOIN Provincias p ON mun.CodigoProvincia = p.CodigoProvincia
                        WHERE v.IDVehiculo = @IDVehiculo";

                    SqlCommand cmd = new SqlCommand(sql, oCnn);
                    cmd.Parameters.AddWithValue("@IDVehiculo", idVehiculo);

                    oCnn.Open();
                    SqlDataReader dr = cmd.ExecuteReader();

                    if (dr.Read())
                    {
                        oVehiculo = new Vehiculos()
                        {
                            IDVehiculo = Convert.ToInt32(dr["IDVehiculo"]),
                            IDUsuario = Convert.ToInt32(dr["IDUsuario"]),
                            IdCategoria = Convert.ToInt32(dr["IdCategoria"]),
                            IdMarca = Convert.ToInt32(dr["IdMarca"]),
                            IDTipo = Convert.ToInt32(dr["IDTipo"]),
                            IdMotor = Convert.ToInt32(dr["IdMotor"]),
                            CodigoMunicipio = Convert.ToInt32(dr["CodigoMunicipio"]),
                            Modelo = dr["Modelo"]?.ToString(),
                            Año = Convert.ToInt32(dr["Año"]),
                            Color = dr["Color"]?.ToString(),
                            Placa = dr["Placa"]?.ToString(),
                            NumeroChasis = dr["NumeroChasis"]?.ToString(),
                            Kilometraje = dr["Kilometraje"] as decimal?,
                            Precio = Convert.ToDecimal(dr["Precio"]),
                            Descripcion = dr["Descripcion"]?.ToString(),
                            Condicion = dr["Condicion"]?.ToString(),
                            Estatus = Convert.ToBoolean(dr["Estatus"]),
                            FechaRegistro = Convert.ToDateTime(dr["FechaRegistro"]),
                            FechaActualizacion = dr["FechaActualizacion"] as DateTime?,
                            Contacto = dr["Contacto"]?.ToString(),
                            Observaciones = dr["Observaciones"]?.ToString(),
                            Vendido = Convert.ToBoolean(dr["Vendido"]),
                            FechaVenta = dr["FechaVenta"] as DateTime?,
                            Destacado = Convert.ToBoolean(dr["Destacado"]),
                            NumeroVistas = Convert.ToInt32(dr["NumeroVistas"]),

                            // Propiedades adicionales
                            NombreUsuario = dr["NombreUsuario"]?.ToString(),
                            EmailUsuario = dr["EmailUsuario"]?.ToString(),
                            CategoriaNombre = dr["CategoriaNombre"]?.ToString(),
                            MarcaNombre = dr["MarcaNombre"]?.ToString(),
                            TipoNombre = dr["TipoNombre"]?.ToString(),
                            MotorDescripcion = dr["MotorDescripcion"]?.ToString(),
                            Combustible = dr["Combustible"]?.ToString(),
                            Transmision = dr["Transmision"]?.ToString(),
                            MunicipioNombre = dr["MunicipioNombre"]?.ToString(),
                            ProvinciaNombre = dr["ProvinciaNombre"]?.ToString()
                        };

                        System.Diagnostics.Debug.WriteLine($"✅ Vehículo encontrado: {oVehiculo.Modelo}");
                    }

                    dr.Close();
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"ERROR al obtener vehículo por ID: {ex.Message}");
                }
            }
            return oVehiculo;
        }

        // Método adicional para marcar como vendido
        public bool MarcarComoVendido(int idVehiculo)
        {
            bool respuesta = false;
            using (SqlConnection oConn = new SqlConnection(Conexion.Bd))
            {
                try
                {
                    oConn.Open();
                    string sql = "UPDATE VEHICULOS SET Vendido = 1, FechaVenta = GETDATE() WHERE IDVehiculo = @IDVehiculo";
                    SqlCommand cmd = new SqlCommand(sql, oConn);
                    cmd.Parameters.AddWithValue("@IDVehiculo", idVehiculo);

                    int filasAfectadas = cmd.ExecuteNonQuery();
                    respuesta = filasAfectadas > 0;
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"ERROR al marcar como vendido: {ex.Message}");
                }
            }
            return respuesta;
        }

        // Método adicional para incrementar vistas
        public bool IncrementarVistas(int idVehiculo)
        {
            bool respuesta = false;
            using (SqlConnection oConn = new SqlConnection(Conexion.Bd))
            {
                try
                {
                    oConn.Open();
                    string sql = "UPDATE VEHICULOS SET NumeroVistas = NumeroVistas + 1 WHERE IDVehiculo = @IDVehiculo";
                    SqlCommand cmd = new SqlCommand(sql, oConn);
                    cmd.Parameters.AddWithValue("@IDVehiculo", idVehiculo);

                    int filasAfectadas = cmd.ExecuteNonQuery();
                    respuesta = filasAfectadas > 0;
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"ERROR al incrementar vistas: {ex.Message}");
                }
            }
            return respuesta;
        }

        // Método para obtener vehículos por usuario
        public List<Vehiculos> ListarPorUsuario(int idUsuario)
        {
            List<Vehiculos> oListaVehiculos = new List<Vehiculos>();
            using (SqlConnection oCnn = new SqlConnection(Conexion.Bd))
            {
                try
                {
                    string sql = @"
                        SELECT v.*, u.Nombre as NombreUsuario, u.Email as EmailUsuario,
                               c.Descripcion as CategoriaNombre, m.Descripcion as MarcaNombre,
                               t.Descripcion as TipoNombre, mo.Descripcion as MotorDescripcion,
                               mo.Combustible, mo.Transmisión as Transmision,
                               mun.NombreMunicipio as MunicipioNombre, p.NombreProvincia as ProvinciaNombre
                        FROM VEHICULOS v
                        INNER JOIN USERS u ON v.IDUsuario = u.ID
                        INNER JOIN CATEGORIAS c ON v.IdCategoria = c.IdCategoria
                        INNER JOIN MARCAS m ON v.IdMarca = m.IdMarca
                        INNER JOIN TIPOS t ON v.IDTipo = t.IDTipo
                        INNER JOIN MOTORES mo ON v.IdMotor = mo.IdMotor
                        INNER JOIN Municipios mun ON v.CodigoMunicipio = mun.CodigoMunicipio
                        INNER JOIN Provincias p ON mun.CodigoProvincia = p.CodigoProvincia
                        WHERE v.IDUsuario = @IDUsuario AND v.Estatus = 1
                        ORDER BY v.FechaRegistro DESC";

                    SqlCommand cmd = new SqlCommand(sql, oCnn);
                    cmd.Parameters.AddWithValue("@IDUsuario", idUsuario);

                    oCnn.Open();
                    SqlDataReader dr = cmd.ExecuteReader();

                    while (dr.Read())
                    {
                        var vehiculo = new Vehiculos()
                        {
                            IDVehiculo = Convert.ToInt32(dr["IDVehiculo"]),
                            IDUsuario = Convert.ToInt32(dr["IDUsuario"]),
                            Modelo = dr["Modelo"]?.ToString(),
                            Año = Convert.ToInt32(dr["Año"]),
                            Precio = Convert.ToDecimal(dr["Precio"]),
                            Estatus = Convert.ToBoolean(dr["Estatus"]),
                            Vendido = Convert.ToBoolean(dr["Vendido"]),
                            Destacado = Convert.ToBoolean(dr["Destacado"]),
                            FechaRegistro = Convert.ToDateTime(dr["FechaRegistro"]),
                            MarcaNombre = dr["MarcaNombre"]?.ToString(),
                            TipoNombre = dr["TipoNombre"]?.ToString(),
                            MunicipioNombre = dr["MunicipioNombre"]?.ToString()
                        };

                        oListaVehiculos.Add(vehiculo);
                    }

                    dr.Close();
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"ERROR al listar vehículos por usuario: {ex.Message}");
                }
            }
            return oListaVehiculos;
        }
    }
}