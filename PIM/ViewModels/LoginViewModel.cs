using System.ComponentModel.DataAnnotations;

namespace PIM.Models
{
    public class LoginViewModel
    {
        [Required]
        [EmailAddress] // Valida formato de e-mail
        public string Email { get; set; } = null!;

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; } = null!;

        public bool RememberMe { get; set; } // Mantém opção de lembrar
    }
}
