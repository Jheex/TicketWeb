using Microsoft.AspNetCore.Mvc.Rendering;
using PIM.Models;
using System.Collections.Generic;
using System; // Necessário para Math.Ceiling, se incluído

namespace PIM.ViewModels
{
    /// <summary>
    /// Contém as listas de opções disponíveis para os filtros na tela de listagem de tickets.
    /// As listas são preenchidas com objetos <see cref="SelectListItem"/> para uso em Dropdowns HTML.
    /// </summary>
    public class TicketFilterOptions
    {
        /// <summary>
        /// Lista de opções de Status disponíveis para filtragem.
        /// </summary>
        public List<SelectListItem> Statuses { get; set; } = new List<SelectListItem>();
        
        /// <summary>
        /// Lista de opções de Analistas/Técnicos disponíveis para filtragem e atribuição.
        /// </summary>
        public List<SelectListItem> Analysts { get; set; } = new List<SelectListItem>();
    }

    /// <summary>
    /// View Model principal utilizada para exibir a lista de chamados (tickets) em uma "Card View" ou tabela.
    /// Encapsula a lista de tickets, os dados de paginação, as opções de filtro e o estado atual dos filtros.
    /// </summary>
    public class TicketsCardViewModel
    {
        // Lista de chamados (tickets)
        /// <summary>
        /// A lista de objetos <see cref="Chamado"/> a serem exibidos na página atual.
        /// </summary>
        public List<Chamado> Tickets { get; set; } = new List<Chamado>();

        // Paginação
        /// <summary>
        /// O número da página atual.
        /// </summary>
        public int PageNumber { get; set; } = 1;

        /// <summary>
        /// O número máximo de tickets por página.
        /// </summary>
        public int PageSize { get; set; } = 10; // Adicionando valor padrão para clareza

        /// <summary>
        /// O número total de tickets que satisfazem os filtros aplicados.
        /// </summary>
        public int TotalItems { get; set; } = 0;

        // Filtros disponíveis na view
        /// <summary>
        /// As opções disponíveis para os filtros de status e analistas.
        /// </summary>
        public TicketFilterOptions FilterOptions { get; set; } = new TicketFilterOptions();

        // Filtros selecionados pelo usuário
        /// <summary>
        /// O status selecionado atualmente pelo usuário para filtrar a lista.
        /// </summary>
        public string? SelectedStatus { get; set; }

        /// <summary>
        /// O ID do analista selecionado atualmente pelo usuário para filtrar a lista.
        /// </summary>
        public int? SelectedAnalystId { get; set; }
        
        /// <summary>
        /// Propriedade calculada que retorna o número total de páginas.
        /// </summary>
        public int TotalPages => (int)Math.Ceiling((double)TotalItems / PageSize);
    }
}