using System;

namespace Proyect_P3.Models.ViewModels
{
    public class Categorias
    {
        public int IdCategoria { get; set; }
        public string Descripcion { get; set; }
        public bool? Estatus { get; set; }
        public DateTime? FechaRegistro { get; set; }
        public byte[] Imagen { get; set; }
        public string ImagenBase64 { get; set; } 
    }
}