using System.ComponentModel.DataAnnotations;

namespace PIM.Models
{
    public class ForgotPasswordViewModel
    {
        [Required(ErrorMessage = "O campo Email é obrigatório.")]
        [EmailAddress(ErrorMessage = "Informe um email válido.")]
        public string Email { get; set; }
    }
}
