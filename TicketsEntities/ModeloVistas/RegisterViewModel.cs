using System.ComponentModel.DataAnnotations;

namespace AplicacionDeTickets.Models.ViewModels
{
    public class RegisterViewModel
    {
        [Required(ErrorMessage = "El ID de usuario es obligatorio")]
        [StringLength(50, ErrorMessage = "El ID de usuario debe tener entre 3 y 50 caracteres", MinimumLength = 3)]
        [Display(Name = "ID de Usuario")]
        public string ID_Usuario { get; set; }

        [Required(ErrorMessage = "El correo electrónico es obligatorio")]
        [EmailAddress(ErrorMessage = "Formato de correo electrónico inválido")]
        [Display(Name = "Correo electrónico")]
        public string Email { get; set; }

        [Required(ErrorMessage = "El nombre es obligatorio")]
        [StringLength(50, ErrorMessage = "El nombre debe tener entre 2 y 50 caracteres", MinimumLength = 2)]
        [Display(Name = "Nombre")]
        public string Nombre { get; set; }

        [Required(ErrorMessage = "El primer apellido es obligatorio")]
        [StringLength(80, ErrorMessage = "El primer apellido debe tener entre 2 y 80 caracteres", MinimumLength = 2)]
        [Display(Name = "Primer Apellido")]
        public string Primer_Apellido { get; set; }

        [Required(ErrorMessage = "El segundo apellido es obligatorio")]
        [StringLength(80, ErrorMessage = "El segundo apellido debe tener entre 2 y 80 caracteres", MinimumLength = 2)]
        [Display(Name = "Segundo Apellido")]
        public string Segundo_Apellido { get; set; }

        [Required(ErrorMessage = "La contraseña es obligatoria")]
        [StringLength(255, ErrorMessage = "La contraseña debe tener entre 6 y 255 caracteres", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Contraseña")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirmar contraseña")]
        [Compare("Password", ErrorMessage = "La contraseña y la confirmación no coinciden")]
        public string ConfirmPassword { get; set; }

        [Required(ErrorMessage = "El rol de usuario es obligatorio")]
        [Display(Name = "Rol de Usuario")]
        public string Rol_Usuario { get; set; }
    }
}