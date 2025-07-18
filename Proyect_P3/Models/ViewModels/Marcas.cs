using System;

public class Marcas
{
    public int IdMarca { get; set; }
    public string Descripcion { get; set; }
    public bool? Estatus { get; set; }
    public DateTime? FechaRegistro { get; set; }
    public byte[] Imagen { get; set; }
    public string ImagenBase64 { get; set; } 
}