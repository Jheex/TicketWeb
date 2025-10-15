using System;
using System.ComponentModel.DataAnnotations;

namespace PIM.Models
{
    /// <summary>
    /// Representa uma solicitação simples ou um ticket dentro do sistema.
    /// Esta classe é usada para rastrear o status e as informações básicas da solicitação.
    /// </summary>
    public class Ticket
    {
        /// <summary>
        /// Chave primária e identificador único do Ticket.
        /// </summary>
        public int TicketId { get; set; }

        /// <summary>
        /// Título ou assunto da solicitação. Campo obrigatório.
        /// </summary>
        [Required(ErrorMessage = "O título do Ticket é obrigatório.")]
        public string Titulo { get; set; } = string.Empty;

        /// <summary>
        /// Descrição detalhada da solicitação.
        /// </summary>
        public string Descricao { get; set; } = string.Empty;

        /// <summary>
        /// Status atual do Ticket. Campo obrigatório com valor padrão "Pendente".
        /// Valores comuns: Pendente, Aprovado, Rejeitado.
        /// </summary>
        [Required(ErrorMessage = "O Status é obrigatório.")]
        public string Status { get; set; } = "Pendente"; 

        /// <summary>
        /// Data e hora em que o Ticket foi criado. O valor padrão é a hora atual.
        /// </summary>
        [Required]
        public DateTime DataCriacao { get; set; } = DateTime.Now;

        /// <summary>
        /// Campo opcional para o comentário ou justificativa de um administrador 
        /// ao aprovar, rejeitar ou processar o ticket.
        /// </summary>
        public string? ComentarioAdmin { get; set; } // Comentário do admin ao aprovar
    }
}