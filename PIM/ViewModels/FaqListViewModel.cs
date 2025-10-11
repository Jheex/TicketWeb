using PIM.Models;
using System.Collections.Generic;

namespace PIM.ViewModels
{
    public class FaqListViewModel
    {
        // A lista de FAQs da página atual
        public IEnumerable<Faq> Faqs { get; set; }

        // Informações da Paginação
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalItems { get; set; }
        
        // Informação do Filtro
        public string CurrentFilter { get; set; }
        
        // Propriedade auxiliar para calcular o total de páginas
        public int TotalPages => (int)Math.Ceiling(TotalItems / (double)PageSize);
    }
}