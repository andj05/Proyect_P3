//------------------------------------------------------------------------------
// <auto-generated>
//    This code was generated from a template.
//
//    Manual changes to this file may cause unexpected behavior in your application.
//    Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Proyect_P3.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class USER
    {
        public USER()
        {
            this.ARTICULOSFOTOS = new HashSet<ARTICULOSFOTO>();
            this.VEHICULOS = new HashSet<VEHICULO>();
        }
    
        public int ID { get; set; }
        public string Nombre { get; set; }
        public string Usuario { get; set; }
        public string Password { get; set; }
        public string Email { get; set; }
        public Nullable<int> Edad { get; set; }
        public Nullable<int> idEstatus { get; set; }
    
        public virtual mSTATU mSTATU { get; set; }
        public virtual ICollection<ARTICULOSFOTO> ARTICULOSFOTOS { get; set; }
        public virtual ICollection<VEHICULO> VEHICULOS { get; set; }
    }
}
