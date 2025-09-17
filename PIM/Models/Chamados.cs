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

        // Apenas a chave estrangeira e a propriedade de navegação são necessárias
        public int? AtribuidoA_AdminId { get; set; }
        
        [ForeignKey("AtribuidoA_AdminId")]
        public Admin? AtribuidoA { get; set; }

        public string? NomeArquivoAnexo { get; set; }

        public string? CaminhoArquivoAnexo { get; set; }
    }
}