using System;
using System.Collections.Generic;


namespace Proyect_P3.Models.ViewModels
{
    public class Vehiculos
    {
        public int IDVehiculo { get; set; }
        public int IDUsuario { get; set; }
        public int IdCategoria { get; set; }
        public int IdMarca { get; set; }
        public int IDTipo { get; set; }
        public int IdMotor { get; set; }
        public int CodigoMunicipio { get; set; }
        public string Modelo { get; set; }
        public int Año { get; set; }
        public string Color { get; set; }
        public string Placa { get; set; }
        public string NumeroChasis { get; set; }
        public decimal? Kilometraje { get; set; }
        public decimal Precio { get; set; }
        public string Descripcion { get; set; }
        public string Condicion { get; set; }
        public bool Estatus { get; set; }
        public DateTime FechaRegistro { get; set; }
        public DateTime? FechaActualizacion { get; set; }
        public string Contacto { get; set; }
        public string Observaciones { get; set; }
        public bool Vendido { get; set; }
        public DateTime? FechaVenta { get; set; }
        public bool Destacado { get; set; }
        public int NumeroVistas { get; set; }

        // Propiedades de navegación (opcionales para cuando uses Entity Framework)
        public QueryViewModels Usuario { get; set; }
        public Categorias Categoria { get; set; }
        public Marcas Marca { get; set; }
        public Tipos Tipo { get; set; }
        public Motores Motor { get; set; }
        public Municipios Municipio { get; set; }

        // Propiedades calculadas/adicionales para la vista
        public string NombreUsuario { get; set; }
        public string EmailUsuario { get; set; }
        public string CategoriaNombre { get; set; }
        public string MarcaNombre { get; set; }
        public string TipoNombre { get; set; }
        public string MotorDescripcion { get; set; }
        public string Combustible { get; set; }
        public string Transmision { get; set; }
        public string MunicipioNombre { get; set; }
        public string ProvinciaNombre { get; set; }


        // 📸 NUEVAS PROPIEDADES PARA FOTOS
        public string PrimeraFoto { get; set; }        // Base64 de la primera foto para thumbnail
        public int CantidadFotos { get; set; }         // Número total de fotos
        public List<string> TodasLasFotos { get; set; } // Lista de todas las fotos en Base64 (opcional)

        // Constructor
        public Vehiculos()
        {
            CantidadFotos = 0;
            TodasLasFotos = new List<string>();
        }

        // Para mostrar precios formateados
        public string PrecioFormateado
        {
            get { return Precio.ToString("C"); }
        }

        // Para mostrar el estado del vehículo
        public string EstadoVehiculo
        {
            get
            {
                if (Vendido) return "Vendido";
                if (!Estatus) return "Inactivo";
                return "Disponible";
            }
        }

        // Para mostrar la antigüedad del vehículo
        public int Antiguedad
        {
            get { return DateTime.Now.Year - Año; }
        }
    }
}