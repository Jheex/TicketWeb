using System.ComponentModel.DataAnnotations;

namespace PIM.Models
{
    /// <summary>
    /// View Model utilizada para a tela de login do usuário.
    /// Contém os campos necessários para autenticação (Email e Senha), além da opção de persistência da sessão.
    /// </summary>
    public class LoginViewModel
    {
        /// <summary>
        /// Endereço de email do usuário. É um campo obrigatório e validado para garantir o formato correto.
        /// </summary>
        [Required(ErrorMessage = "O campo Email é obrigatório.")]
        [EmailAddress(ErrorMessage = "O formato do email é inválido.")] // Valida formato de e-mail
        public string Email { get; set; } = null!;

        /// <summary>
        /// Senha do usuário. É um campo obrigatório e marcado como tipo de dado 'Password' para ocultação na interface.
        /// </summary>
        [Required(ErrorMessage = "O campo Senha é obrigatório.")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = null!;

        /// <summary>
        /// Indica se a sessão do usuário deve ser persistida (lembrar-se do login em acessos futuros).
        /// </summary>
        public bool RememberMe { get; set; } // Mantém opção de lembrar
    }
}