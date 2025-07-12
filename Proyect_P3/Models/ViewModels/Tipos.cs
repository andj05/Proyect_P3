using System;

namespace Proyect_P3.Models.ViewModels
{
    public class Tipos
    {
        public int IDTipo { get; set; }
        public string Descripcion { get; set; }
        public byte[] Imagen { get; set; }
        public bool? Estatus { get; set; }
        public DateTime? FechaRegistro { get; set; }
        public string ImagenBase64 { get; set; } 
    }
}