using System;
using System.ComponentModel.DataAnnotations;

namespace PIM.Models
{
    public class Usuario
    {
        [Key]
        public int Id { get; set; }

        [Required, MaxLength(50)]
        public string? Username { get; set; }

        [Required, MaxLength(100)]
        public string? Email { get; set; }

        [MaxLength(100)]
        public string? SenhaHash { get; set; }

        [Required, MaxLength(20)]
        public string? Role { get; set; } // Ex: Admin, Tecnico, Usuario

        public DateTime CreatedAt { get; set; } = DateTime.Now; // Inicializa automaticamente

        [MaxLength(20)]
        public string? Telefone { get; set; }

        // Adicionar os que estão faltando
        public string? Status { get; set; }
        public string ?Endereco { get; set; } // sem acento
        public DateTime? DataNascimento { get; set; }
        public string ?Observacoes { get; set; }
    }
}

