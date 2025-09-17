using Microsoft.AspNetCore.Mvc.Rendering;
using PIM.Models;
using System.Collections.Generic;

namespace PIM.ViewModels
{
    public class TicketFilterOptions
    {
        public List<SelectListItem> Statuses { get; set; } = new();
        public List<SelectListItem> Analysts { get; set; } = new();
    }

    public class TicketsCardViewModel
    {
        // Lista de chamados (tickets)
        public List<Chamado> Tickets { get; set; } = new();

        // Paginação
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalItems { get; set; }

        // Filtros disponíveis na view
        public TicketFilterOptions FilterOptions { get; set; } = new();

        // Filtros selecionados pelo usuário
        public string? SelectedStatus { get; set; }
        public int? SelectedAnalystId { get; set; }
    }
}
