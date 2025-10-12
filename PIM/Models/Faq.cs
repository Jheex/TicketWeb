using System;
using System.ComponentModel.DataAnnotations;

namespace PIM.Models
{
    public class Faq
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "A pergunta é obrigatória.")]
        public string ?Pergunta { get; set; }

        [Required(ErrorMessage = "A resposta é obrigatória.")]
        public string ?Resposta { get; set; }

        [Required(ErrorMessage = "A categoria é obrigatória.")]
        public string ?Categoria { get; set; }

        [Required(ErrorMessage = "A ordem é obrigatória.")]
        [Range(1, int.MaxValue, ErrorMessage = "Informe um número válido.")]
        public int Ordem { get; set; }

        public DateTime? DataAtualizacao { get; set; }
    }
}
