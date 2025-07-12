using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace Proyect_P3.Models.ViewModels
{
    public class TipdocQueryViewModels
    {
        public int Id { get; set; }

 
        [Column("TIPDOC")]
        public string TipoDoc{ get; set; }
        public string Origen { get; set; }
        public string Descripcion { get; set; }

        public string Contador { get; set; }

    }

    public class AddTipdocViewModels
    {
        [Required]
        [Column("TIPDOC")]
        [StringLength(3, ErrorMessage = "El tipo de documento no puede exceder 3 caracteres.")]
        public string TipoDoc { get; set; }

        [StringLength(1, ErrorMessage = "El origen no puede exceder 1 carácter.")]
        public string Origen { get; set; }

        [StringLength(50, ErrorMessage = "La descripción no puede exceder 50 caracteres.")]
        public string Descripcion { get; set; }

        [StringLength(10, ErrorMessage = "El contador no puede exceder 10 caracteres.")]
        public string Contador { get; set; }

        [StringLength(20, ErrorMessage = "La cuenta débito no puede exceder 20 caracteres.")]
        public string CuentaDebito { get; set; }

        [StringLength(20, ErrorMessage = "La cuenta crédito no puede exceder 20 caracteres.")]
        public string CuentaCredito { get; set; }

        public int? Estatus { get; set; } 
    }

    public class EditTipdocViewModels
    {
        [Required]
        public int Id { get; set; }

        [Required]
        [Column("TIPDOC")]
        [Display(Name = "Tipo de Documento")]
        public string TipoDoc { get; set; }

        [Required]
        [Display(Name = "Origen")]
        public string Origen { get; set; }

        [Required]
        [Display(Name = "Descripción")]
        public string Descripcion { get; set; }

        [Display(Name = "Contador")]
        public string Contador { get; set; }

        [Required]
        [Display(Name = "Cuenta Débito")]
        public string CuentaDebito { get; set; }

        [Required]
        [Display(Name = "Cuenta Crédito")]
        public string CuentaCredito { get; set; }

        [Required]
        [Display(Name = "Estatus")]
        public int Estatus { get; set; }
    }

    public class DetailsTipdocQueryViewModels
    {
        [Required]
        public int Id { get; set; }

        [Required]
        [Column("TIPDOC")]
        [Display(Name = "Tipo de Documento")]
        public string TipoDoc { get; set; }

        [Required]
        [Display(Name = "Origen")]
        public string Origen { get; set; }

        [Required]
        [Display(Name = "Descripción")]
        public string Descripcion { get; set; }

        [Required]
        [Display(Name = "Contador")]
        public string Contador { get; set; }

        [Required]
        [Display(Name = "Cuenta Débito")]
        public string CuentaDebito { get; set; }

        [Required]
        [Display(Name = "Cuenta Crédito")]
        public string CuentaCredito { get; set; }

        [Required]
        [Display(Name = "Estatus")]
        public int Estatus { get; set; }
    }

}