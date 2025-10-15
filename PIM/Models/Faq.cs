using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PIM.Models
{
    /// <summary>
    /// Representa um item de Perguntas Frequentes (FAQ) para fornecer autoatendimento e esclarecer dúvidas comuns.
    /// </summary>
    public class Faq
    {
        /// <summary>
        /// Chave primária e identificador único da FAQ.
        /// </summary>
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// O texto da pergunta da FAQ. Campo obrigatório.
        /// </summary>
        [Required(ErrorMessage = "A pergunta é obrigatória.")]
        public string ?Pergunta { get; set; }

        /// <summary>
        /// O texto da resposta associada à pergunta. Campo obrigatório.
        /// </summary>
        [Required(ErrorMessage = "A resposta é obrigatória.")]
        public string ?Resposta { get; set; }

        /// <summary>
        /// A categoria à qual esta FAQ pertence (para fins de organização e filtragem). Campo obrigatório.
        /// </summary>
        [Required(ErrorMessage = "A categoria é obrigatória.")]
        public string ?Categoria { get; set; }

        /// <summary>
        /// Define a ordem de exibição desta FAQ dentro de sua categoria. Campo obrigatório e deve ser um número positivo.
        /// </summary>
        [Required(ErrorMessage = "A ordem é obrigatória.")]
        [Range(1, int.MaxValue, ErrorMessage = "Informe um número válido.")]
        public int Ordem { get; set; }

        /// <summary>
        /// Data da última atualização da FAQ. Pode ser nula.
        /// </summary>
        public DateTime? DataAtualizacao { get; set; }
    }
}
