using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http; // Necessário para IFormFile

namespace PIM.ViewModels
{
    /// <summary>
    /// View Model utilizada para a criação ou edição de um novo chamado (ticket) através de um formulário.
    /// Contém propriedades específicas para a coleta de dados do usuário e manipulação de arquivos.
    /// </summary>
    public class ChamadoViewModel
    {
        /// <summary>
        /// ID do chamado. Usado para operações de edição (update).
        /// </summary>
        public int Id { get; set; } // chave primária

        /// <summary>
        /// Título ou assunto do chamado. Campo obrigatório.
        /// </summary>
        [Required(ErrorMessage = "O campo Título é obrigatório.")]
        [Display(Name = "Título / Assunto")]
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// ID do usuário que está solicitando/abrindo o chamado. Campo obrigatório.
        /// </summary>
        [Required(ErrorMessage = "É obrigatório selecionar o solicitante.")]
        [Display(Name = "Solicitante / Usuário")]
        public int UserId { get; set; }

        /// <summary>
        /// Categoria do chamado. Campo obrigatório.
        /// </summary>
        [Required(ErrorMessage = "O campo Categoria é obrigatório.")]
        public string Category { get; set; } = string.Empty;

        /// <summary>
        /// Prioridade definida para o chamado. Campo obrigatório.
        /// </summary>
        [Required(ErrorMessage = "O campo Prioridade é obrigatório.")]
        public string Priority { get; set; } = string.Empty;

        /// <summary>
        /// Descrição detalhada do problema ou solicitação. Opcional.
        /// </summary>
        [Display(Name = "Descrição Detalhada")]
        public string? Description { get; set; }

        /// <summary>
        /// Representa o arquivo anexado enviado pelo formulário. Não é persistido no banco de dados diretamente,
        /// mas sim utilizado para upload no servidor. Opcional.
        /// </summary>
        [Display(Name = "Anexo")]
        public IFormFile? Attachment { get; set; }
    }

}