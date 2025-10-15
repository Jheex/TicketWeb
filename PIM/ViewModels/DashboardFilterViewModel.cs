using System;
using System.Collections.Generic;

namespace PIM.ViewModels
{
    /// <summary>
    /// View Model utilizada para receber e encapsular os critérios de filtro e paginação 
    /// que o usuário aplica ao Dashboard de Business Intelligence.
    /// </summary>
    public class DashboardFilterViewModel
    {
        /// <summary>
        /// Data inicial (Data de Abertura) para filtragem dos chamados. Opcional.
        /// </summary>
        public DateTime? StartDate { get; set; }
        
        /// <summary>
        /// Data final (Data de Abertura) para filtragem dos chamados. Opcional.
        /// </summary>
        public DateTime? EndDate { get; set; }
        
        /// <summary>
        /// Lista de status de chamados selecionados para filtragem. Opcional.
        /// </summary>
        public List<string>? Status { get; set; }
        
        /// <summary>
        /// Lista de prioridades de chamados selecionadas para filtragem. Opcional.
        /// </summary>
        public List<string>? Priority { get; set; }
        
        /// <summary>
        /// Lista de IDs de usuários (Analistas/Técnicos) aos quais os chamados estão atribuídos. Opcional.
        /// </summary>
        public List<int>? AssignedToId { get; set; }
        
        /// <summary>
        /// Lista de IDs de usuários que solicitaram (Requesters) os chamados. Opcional.
        /// </summary>
        public List<int>? RequesterId { get; set; }

        // Propriedades para paginação
        
        /// <summary>
        /// Número da página atual para a tabela detalhada do dashboard. Padrão é 1.
        /// </summary>
        public int PageNumber { get; set; } = 1;
        
        /// <summary>
        /// Tamanho da página (número de itens por página) para a tabela detalhada. Padrão é 5.
        /// </summary>
        public int PageSize { get; set; } = 5;
    }
}
