using System;
using System.Collections.Generic;

namespace PIM.Models
{
    /// <summary>
    /// View Model utilizada para passar detalhes de erro para a view, 
    /// como um Request ID para rastreamento no log do servidor.
    /// </summary>
    public class ErrorViewModel
    {
        /// <summary>
        /// Identificador único da requisição (Request ID). Usado para rastreamento de erros.
        /// </summary>
        public string? RequestId { get; set; }

        /// <summary>
        /// Propriedade calculada que retorna true se o RequestId não for nulo ou vazio, 
        /// indicando que ele deve ser exibido na tela.
        /// </summary>
        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
    }
}