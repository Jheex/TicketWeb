using System;

namespace PIM.ViewModels
{
    public class TicketFilterViewModel
    {
        // Filtro por data de abertura mínima
        public DateTime? StartDate { get; set; }

        // Filtro por data de abertura máxima
        public DateTime? EndDate { get; set; }

        // Filtro por status do chamado (Pendente, Aprovado, etc)
        public string? Status { get; set; }

        // Filtro por administrador responsável
        public int? AssignedToId { get; set; }
    }
}
