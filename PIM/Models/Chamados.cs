using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PIM.Models
{
    public class Chamado
    {
        [Key]
        public int ChamadoId { get; set; }

        [Required, MaxLength(200)]
        public string? Titulo { get; set; }

        public string? Descricao { get; set; }

        public string? Categoria { get; set; }

        public string? Prioridade { get; set; }

        public string? Status { get; set; } = "Aberto";

        public DateTime DataAbertura { get; set; } = DateTime.Now;

        public DateTime? DataAtribuicao { get; set; }

        public DateTime? DataFechamento { get; set; }

        public string? NomeArquivoAnexo { get; set; }

        public string? CaminhoArquivoAnexo { get; set; }

        // RELACIONAMENTO COM USUÁRIO (quem foi atribuído)
        public int? AtribuidoAId { get; set; }
        [ForeignKey("AtribuidoAId")]
        public Usuario? AtribuidoA { get; set; }

        // RELACIONAMENTO COM USUÁRIO SOLICITANTE (quem criou o chamado)
        public int SolicitanteId { get; set; }   // precisa existir
        [ForeignKey("SolicitanteId")]
        public Usuario? Solicitante { get; set; }
    }
}
