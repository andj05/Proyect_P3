using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;

namespace Proyect_P3.Metodos
{
    public class Conexion
    {
        // Cadena de conexión directa (como respaldo)
        public static string db = @"Server=ANDDY1;Database=DBMVC;User ID=sa;Password=A123456;TrustServerCertificate=True;MultipleActiveResultSets=true";

        // Cadena de conexión desde web.config/app.config
        public static string Bd = ConfigurationManager.ConnectionStrings["cnnDbString"]?.ConnectionString ?? db;
    }

    public class Connection
    {
        public static string GetConnectionString()
        {
            return Conexion.Bd;
        }

        public static string GetConnectionString(string connectionName)
        {
            return ConfigurationManager.ConnectionStrings[connectionName]?.ConnectionString;
        }
    }
}