namespace PIM.Models
{
    /// <summary>
    /// Data Transfer Object (DTO) utilizado para expor informações essenciais de um chamado (ticket)
    /// em uma API ou em uma listagem simplificada na interface.
    /// </summary>
    public class TicketApiModel
    {
        /// <summary>
        /// Identificador único do chamado.
        /// </summary>
        public int Id { get; set; }
        
        /// <summary>
        /// Título ou assunto do chamado.
        /// </summary>
        public string? Title { get; set; }
        
        /// <summary>
        /// Categoria do chamado (Ex: Hardware, Software, Dúvida).
        /// </summary>
        public string? Categoria { get; set; }
        
        /// <summary>
        /// Nível de prioridade do chamado.
        /// </summary>
        public string? Prioridade { get; set; }
        
        /// <summary>
        /// Status atual do chamado (Ex: Aberto, Em Andamento, Fechado).
        /// </summary>
        public string? Status { get; set; }
        
        /// <summary>
        /// Nome do usuário (solicitante ou atribuído) associado ao ticket para exibição.
        /// </summary>
        public string? Usuario { get; set; }
    }
}