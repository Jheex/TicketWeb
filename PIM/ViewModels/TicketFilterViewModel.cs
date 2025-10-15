using System;
using System.Collections.Generic;

namespace PIM.ViewModels
{
    /// <summary>
    /// View Model utilizada para receber os critérios de filtro de tickets de um formulário ou query string.
    /// Permite filtrar a lista de chamados por datas, status e o responsável pela atribuição.
    /// </summary>
    public class TicketFilterViewModel
    {
        /// <summary>
        /// Filtro por data de abertura mínima (início do período). Opcional.
        /// </summary>
        public DateTime? StartDate { get; set; }

        /// <summary>
        /// Filtro por data de abertura máxima (fim do período). Opcional.
        /// </summary>
        public DateTime? EndDate { get; set; }

        /// <summary>
        /// Filtro por status do chamado (Ex: Aberto, Pendente, Fechado). Opcional.
        /// </summary>
        public string? Status { get; set; }

        /// <summary>
        /// Filtro pelo ID do usuário (administrador/técnico) responsável pelo chamado. Opcional.
        /// </summary>
        public int? AssignedToId { get; set; }
    }
}