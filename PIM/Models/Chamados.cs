using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PIM.Models
{
    /// <summary>
    /// Representa um chamado de suporte ou ticket dentro do sistema.
    /// Contém informações sobre o problema, status, prioridade e os usuários envolvidos (solicitante e atribuído).
    /// </summary>
    public class Chamado
    {
        /// <summary>
        /// Chave primária do chamado.
        /// </summary>
        [Key]
        public int ChamadoId { get; set; }

        /// <summary>
        /// Título breve do chamado. Campo obrigatório e com tamanho máximo de 200 caracteres.
        /// </summary>
        [Required(ErrorMessage = "O Título é obrigatório.")]
        [MaxLength(200, ErrorMessage = "O Título deve ter no máximo 200 caracteres.")]
        public string? Titulo { get; set; }

        /// <summary>
        /// Descrição detalhada do chamado.
        /// </summary>
        public string? Descricao { get; set; }

        /// <summary>
        /// Categoria ou tipo do chamado (Ex: 'Erro de Sistema', 'Solicitação', 'Dúvida').
        /// </summary>
        public string? Categoria { get; set; }

        /// <summary>
        /// Nível de prioridade do chamado (Ex: 'Baixa', 'Média', 'Alta', 'Urgente').
        /// </summary>
        public string? Prioridade { get; set; }

        /// <summary>
        /// Status atual do chamado. O valor padrão é "Aberto".
        /// </summary>
        public string? Status { get; set; } = "Aberto";

        /// <summary>
        /// Data e hora em que o chamado foi aberto. O valor padrão é a hora atual na criação.
        /// </summary>
        public DateTime DataAbertura { get; set; } = DateTime.Now;

        /// <summary>
        /// Data em que o chamado foi atribuído a um técnico ou usuário. Pode ser nulo.
        /// </summary>
        public DateTime? DataAtribuicao { get; set; }

        /// <summary>
        /// Data em que o chamado foi fechado ou resolvido. Pode ser nulo.
        /// </summary>
        public DateTime? DataFechamento { get; set; }

        /// <summary>
        /// Nome do arquivo anexado ao chamado, se houver.
        /// </summary>
        public string? NomeArquivoAnexo { get; set; }

        /// <summary>
        /// Caminho (local/URL) do arquivo anexado.
        /// </summary>
        public string? CaminhoArquivoAnexo { get; set; }

        // RELACIONAMENTO COM USUÁRIO (Atribuição)

        /// <summary>
        /// Chave estrangeira para o Usuário responsável por resolver o chamado.
        /// É nulo (`int?`) se o chamado ainda não foi atribuído.
        /// </summary>
        public int? AtribuidoAId { get; set; }
        
        /// <summary>
        /// Propriedade de navegação para o objeto Usuario que está atribuído a este chamado.
        /// </summary>
        [ForeignKey("AtribuidoAId")]
        public Usuario? AtribuidoA { get; set; }

        // RELACIONAMENTO COM USUÁRIO (Solicitante)

        /// <summary>
        /// Chave estrangeira para o Usuário que abriu o chamado (solicitante).
        /// Este campo é obrigatório e não nulo.
        /// </summary>
        [Required(ErrorMessage = "O ID do Solicitante é obrigatório.")]
        public int SolicitanteId { get; set; }
        
        /// <summary>
        /// Propriedade de navegação para o objeto Usuario que solicitou o chamado.
        /// </summary>
        [ForeignKey("SolicitanteId")]
        public Usuario? Solicitante { get; set; }
    }
}