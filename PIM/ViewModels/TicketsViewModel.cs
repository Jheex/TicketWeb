using System;
using System.ComponentModel.DataAnnotations;

namespace PIM.Models
{
    public class Ticket
    {
        public int TicketId { get; set; }

        [Required]
        public string Titulo { get; set; } = string.Empty;

        public string Descricao { get; set; } = string.Empty;

        [Required]
        public string Status { get; set; } = "Pendente"; // Pendente, Aprovado, Rejeitado

        [Required]
        public DateTime DataCriacao { get; set; } = DateTime.Now;

        public string? ComentarioAdmin { get; set; } // Comentário do admin ao aprovar/rejeitar
    }
}
