using System;

namespace Proyect_P3.Models.ViewModels
{
    public class ArticulosFotos
    {
        public int IDCliente { get; set; }
        public int IDArticulo { get; set; }
        public int SecPhoto { get; set; }
        public byte[] FOTO { get; set; }
    }
}
