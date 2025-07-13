using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Proyect_P3.Models.ViewModels
{
    public class Municipios
    {
        public int CodigoMunicipio { get; set; }
        public string NombreMunicipio { get; set; }
        public int CodigoProvincia { get; set; }

        // Propiedad de navegación 
        public Provincias Provincia { get; set; }
        public List<Vehiculos> Vehiculos { get; set; }

        // Propiedades adicionales para la vista
        public string NombreProvincia { get; set; }

        // Para mostrar municipio con provincia
        public string MunicipioCompleto
        {
            get
            {
                return !string.IsNullOrEmpty(NombreProvincia)
                    ? $"{NombreMunicipio}, {NombreProvincia}"
                    : NombreMunicipio;
            }
        }

        // Constructor para inicializar la lista
        public Municipios()
        {
            Vehiculos = new List<Vehiculos>();
        }
    }
}