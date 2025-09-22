using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema; // Para [NotMapped]

namespace PIM.Models
{
    public class Usuario
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "O nome de usuário é obrigatório.")]
        [StringLength(50, MinimumLength = 3, ErrorMessage = "O nome deve ter entre 3 e 50 caracteres.")]
        public string? Username { get; set; }

        [Required(ErrorMessage = "O email é obrigatório.")]
        [EmailAddress(ErrorMessage = "O formato do email é inválido.")]
        [StringLength(100, ErrorMessage = "O email deve ter no máximo 100 caracteres.")]
        public string? Email { get; set; }

        [Required(ErrorMessage = "A senha é obrigatória.")]
        [StringLength(100, MinimumLength = 8, ErrorMessage = "A senha deve ter no mínimo 8 caracteres.")]
        [DataType(DataType.Password)]
        public string? SenhaHash { get; set; }

        [NotMapped] // Esta propriedade não é mapeada para o banco de dados
        [Required(ErrorMessage = "A confirmação da senha é obrigatória.")]
        [DataType(DataType.Password)]
        [Display(Name = "Confirmar Senha")]
        [Compare("SenhaHash", ErrorMessage = "As senhas não coincidem.")]
        public string? ConfirmarSenha { get; set; }

        [Required(ErrorMessage = "O nível de acesso é obrigatório.")]
        [StringLength(20, ErrorMessage = "O campo 'Role' deve ter no máximo 20 caracteres.")]
        public string? Role { get; set; } // Ex: Admin, Tecnico, Usuario

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [Required(ErrorMessage = "O telefone é obrigatório.")]
        [StringLength(20, ErrorMessage = "O telefone deve ter no máximo 20 caracteres.")]
        [RegularExpression(@"^\(?\d{2}\)?[\s-]?\d{4,5}-?\d{4}$", ErrorMessage = "O formato do telefone é inválido.")]
        public string? Telefone { get; set; }

        [Required(ErrorMessage = "O status é obrigatório.")]
        public string? Status { get; set; }

        [Required(ErrorMessage = "O endereço é obrigatório.")]
        public string? Endereco { get; set; }

        [Required(ErrorMessage = "A data de nascimento é obrigatória.")]
        [DataType(DataType.Date)]
        public DateTime? DataNascimento { get; set; }

        public string? Observacoes { get; set; }
    }
}