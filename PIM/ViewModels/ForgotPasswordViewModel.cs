using System.ComponentModel.DataAnnotations;

namespace PIM.Models
{
    /// <summary>
    /// View Model utilizada para a tela de "Esqueci a Senha".
    /// Contém apenas o campo de email necessário para iniciar o processo de recuperação de senha.
    /// </summary>
    public class ForgotPasswordViewModel
    {
        /// <summary>
        /// O endereço de email do usuário para o qual a recuperação de senha será enviada.
        /// Este campo é obrigatório e deve ser um endereço de email válido.
        /// </summary>
        [Required(ErrorMessage = "O campo Email é obrigatório.")]
        [EmailAddress(ErrorMessage = "Informe um email válido.")]
        public string Email { get; set; } = string.Empty;
    }
}
