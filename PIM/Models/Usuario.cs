using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema; // Para [NotMapped]

namespace PIM.Models
{
    /// <summary>
    /// Representa um usuário do sistema (cliente, técnico, administrador). 
    /// Contém dados de autenticação e informações cadastrais.
    /// </summary>
    public class Usuario
    {
        /// <summary>
        /// Chave primária e identificador único do usuário.
        /// </summary>
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// Nome de usuário para exibição no sistema. Campo obrigatório, com tamanho entre 3 e 50 caracteres.
        /// </summary>
        [Required(ErrorMessage = "O nome de usuário é obrigatório.")]
        [StringLength(50, MinimumLength = 3, ErrorMessage = "O nome deve ter entre 3 e 50 caracteres.")]
        public string? Username { get; set; }

        /// <summary>
        /// Endereço de email do usuário. Campo obrigatório, deve ser um formato de email válido e ter no máximo 100 caracteres.
        /// </summary>
        [Required(ErrorMessage = "O email é obrigatório.")]
        [EmailAddress(ErrorMessage = "O formato do email é inválido.")]
        [StringLength(100, ErrorMessage = "O email deve ter no máximo 100 caracteres.")]
        public string? Email { get; set; }

        /// <summary>
        /// O hash da senha do usuário (armazenado no banco de dados). Campo obrigatório e deve ter no mínimo 8 caracteres.
        /// </summary>
        [Required(ErrorMessage = "A senha é obrigatória.")]
        [StringLength(100, MinimumLength = 8, ErrorMessage = "A senha deve ter no mínimo 8 caracteres.")]
        [DataType(DataType.Password)]
        public string? SenhaHash { get; set; }

        /// <summary>
        /// Confirmação da senha para fins de validação no formulário. Não é mapeada para o banco de dados (`[NotMapped]`).
        /// </summary>
        [NotMapped] // Esta propriedade não é mapeada para o banco de dados
        [Required(ErrorMessage = "A confirmação da senha é obrigatória.")]
        [DataType(DataType.Password)]
        [Display(Name = "Confirmar Senha")]
        [Compare("SenhaHash", ErrorMessage = "As senhas não coincidem.")]
        public string? ConfirmarSenha { get; set; }

        /// <summary>
        /// Nível de acesso ou papel do usuário no sistema (Ex: Admin, Tecnico, Usuario). Campo obrigatório.
        /// </summary>
        [Required(ErrorMessage = "O nível de acesso é obrigatório.")]
        [StringLength(20, ErrorMessage = "O campo 'Role' deve ter no máximo 20 caracteres.")]
        public string? Role { get; set; } // Ex: Admin, Tecnico, Usuario

        /// <summary>
        /// Data e hora em que o usuário foi criado no sistema.
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        /// <summary>
        /// Número de telefone do usuário. Campo obrigatório e deve seguir um formato válido de telefone.
        /// </summary>
        [Required(ErrorMessage = "O telefone é obrigatório.")]
        [StringLength(20, ErrorMessage = "O telefone deve ter no máximo 20 caracteres.")]
        [RegularExpression(@"^\(?\d{2}\)?[\s-]?\d{4,5}-?\d{4}$", ErrorMessage = "O formato do telefone é inválido.")]
        public string? Telefone { get; set; }

        /// <summary>
        /// Status da conta do usuário (Ex: Ativo, Inativo, Bloqueado). Campo obrigatório.
        /// </summary>
        [Required(ErrorMessage = "O status é obrigatório.")]
        public string? Status { get; set; }

        /// <summary>
        /// Endereço residencial ou comercial do usuário. Campo obrigatório.
        /// </summary>
        [Required(ErrorMessage = "O endereço é obrigatório.")]
        public string? Endereco { get; set; }

        /// <summary>
        /// Data de nascimento do usuário. Campo obrigatório.
        /// </summary>
        [Required(ErrorMessage = "A data de nascimento é obrigatória.")]
        [DataType(DataType.Date)]
        public DateTime? DataNascimento { get; set; }

        /// <summary>
        /// Campo para observações adicionais sobre o usuário.
        /// </summary>
        public string? Observacoes { get; set; }
        
        // Propriedades Não Mapeadas (Usadas para View Models/Exibição)
        
        /// <summary>
        /// [NotMapped] Contador de tickets atualmente em andamento por este usuário. 
        /// Usado para exibição e não persistido no banco.
        /// </summary>
        [NotMapped]
        public int TicketsAndamentoCount { get; set; }

        /// <summary>
        /// [NotMapped] Contador de tickets concluídos por este usuário. 
        /// Usado para exibição e não persistido no banco.
        /// </summary>
        [NotMapped]
        public int TicketsConcluidosCount { get; set; }
        
    }
}