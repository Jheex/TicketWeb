using System.ComponentModel.DataAnnotations;

namespace PIM.ViewModels
{
    public class ChamadoViewModel
{
    public int Id { get; set; } // chave primária
    [Required(ErrorMessage = "O campo Título é obrigatório.")]
    [Display(Name = "Título / Assunto")]
    public string Title { get; set; } = string.Empty;

    [Required(ErrorMessage = "É obrigatório selecionar o solicitante.")]
    [Display(Name = "Solicitante / Usuário")]
    public int UserId { get; set; }

    [Required(ErrorMessage = "O campo Categoria é obrigatório.")]
    public string Category { get; set; } = string.Empty;

    [Required(ErrorMessage = "O campo Prioridade é obrigatório.")]
    public string Priority { get; set; } = string.Empty;

    [Display(Name = "Descrição Detalhada")]
    public string? Description { get; set; }

    [Display(Name = "Anexo")]
    public IFormFile? Attachment { get; set; }
}

}