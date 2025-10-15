using PIM.Models;
using System.Collections.Generic;
using System; // Necessário para Math.Ceiling

namespace PIM.ViewModels
{
    /// <summary>
    /// View Model utilizada para exibir uma lista de Perguntas Frequentes (FAQs) na View,
    /// incluindo dados para paginação e o critério de filtro atual.
    /// </summary>
    public class FaqListViewModel
    {
        /// <summary>
        /// A lista de objetos <see cref="Faq"/> a serem exibidos na página atual.
        /// </summary>
        public IEnumerable<Faq> Faqs { get; set; } = new List<Faq>();

        // Informações da Paginação
        
        /// <summary>
        /// O número da página atual que está sendo exibida.
        /// </summary>
        public int PageNumber { get; set; }
        
        /// <summary>
        /// O número máximo de itens (FAQs) por página.
        /// </summary>
        public int PageSize { get; set; }
        
        /// <summary>
        /// O número total de itens (FAQs) que satisfazem o filtro.
        /// </summary>
        public int TotalItems { get; set; }
        
        // Informação do Filtro
        
        /// <summary>
        /// O termo de pesquisa ou critério de filtro atualmente aplicado à lista.
        /// </summary>
        public string? CurrentFilter { get; set; }
        
        /// <summary>
        /// Propriedade auxiliar calculada que retorna o número total de páginas necessárias.
        /// </summary>
        public int TotalPages => (int)Math.Ceiling(TotalItems / (double)PageSize);
    }
}
