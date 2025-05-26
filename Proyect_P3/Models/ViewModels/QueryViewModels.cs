
using System.ComponentModel.DataAnnotations;
namespace Proyect_P3.Models.ViewModels
{
    public class QueryViewModels
    {
        public int _id { get; set; }
        public string _Email { get; set; }
        public int? _Edad { get; set; }

    }
    public class  AddUserViewModels
    {
        [Required]
        [Display(Name = "Nombre de Usuario")]
        public string Nombre { get; set; }

        [Required]
        [Display(Name = " Usuario")]
        public string Usuario { get; set; }

        [Required]
        [Display(Name = "Correo")]
        public string Email { get; set; }

        [Required]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [Required]
        [Display(Name = "Confirmar Password")]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Las contraseñas no coinciden")]
        public string ConfirmPassword { get; set; }

        [Required]
        [Display(Name = "Edad")]
        public int Edad { get; set; }
    }

    public class EditUserViewModels
    {
        public int Id { get; set; }
        [Required]
        [Display(Name = "Nombre de Usuario")]
        public string Nombre { get; set; }
        [Required]
        [Display(Name = "Correo")]
        public string Email { get; set; }

        [Required]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [Required]
        [Display(Name = "Confirmar Password")]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Las contraseñas no coinciden")]
        public string ConfirmPassword { get; set; }

        [Required]
        [Display(Name = "Edad")]
        public int? Edad { get; set; }
    }
}