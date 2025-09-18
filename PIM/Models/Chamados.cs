using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PIM.Models
{
    public class Chamado
    {
        public int ChamadoId { get; set; }

        [Required]
        public int SolicitanteId { get; set; }

        public string? Solicitante { get; set; }

        [Required]
        public string? Titulo { get; set; }

        public string? Descricao { get; set; }

        public string? Categoria { get; set; }

        [Required]
        public string? Prioridade { get; set; }

        [Required]
        public string Status { get; set; } = "Aberto";

        public DateTime DataAbertura { get; set; } = DateTime.Now;

        public DateTime? DataFechamento { get; set; }

        public int? AtribuidoA_AdminId { get; set; }

        [ForeignKey("AtribuidoA_AdminId")]
        public Admin? AtribuidoA { get; set; }

        public DateTime? DataAtribuicao { get; set; } // <- Adicionado

        public string? NomeArquivoAnexo { get; set; }

        public string? CaminhoArquivoAnexo { get; set; }
    }
}
