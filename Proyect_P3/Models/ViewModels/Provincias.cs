using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Proyect_P3.Models.ViewModels
{
    public class Provincias
    {
        public int CodigoProvincia { get; set; }
        public string NombreProvincia { get; set; }

        // Propiedad de navegación (para Entity Framework)
        public List<Municipios> Municipios { get; set; }

        // Constructor para inicializar la lista
        public Provincias()
        {
            Municipios = new List<Municipios>();
        }
    }

}