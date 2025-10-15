using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;

// Usando o namespace do seu ViewModel. Ajuste se necessário.
using PIM.ViewModels; 

namespace PIM.Controllers
{
    /// <summary>
    /// API Controller responsável por fornecer dados e manipular o status dos chamados.
    /// Esta implementação usa uma lista estática para simular um banco de dados.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")] // Define a rota base da API para api/chamados
    public class ChamadosController : ControllerBase
    {
        // --- SIMULAÇÃO DE UM BANCO DE DADOS ---
        /// <summary>
        /// Lista estática que simula a tabela de chamados no banco de dados.
        /// </summary>
        private static readonly List<ChamadoApiModel> _chamados = new List<ChamadoApiModel>
        {
            new ChamadoApiModel { Id = 1, Title = "Problema com impressora", Category = "Hardware", Priority = "Alta", Status = "Aberto" },
            new ChamadoApiModel { Id = 2, Title = "Software não abre", Category = "Software", Priority = "Média", Status = "Em Andamento" },
            new ChamadoApiModel { Id = 3, Title = "Rede lenta", Category = "Rede", Priority = "Baixa", Status = "Aberto" },
            new ChamadoApiModel { Id = 4, Title = "Criação de novo usuário", Category = "Acesso", Priority = "Média", Status = "Concluído" },
            new ChamadoApiModel { Id = 5, Title = "E-mail não envia", Category = "Software", Priority = "Alta", Status = "Em Andamento" },
            new ChamadoApiModel { Id = 6, Title = "Computador não liga", Category = "Hardware", Priority = "Alta", Status = "Aberto" },
            new ChamadoApiModel { Id = 7, Title = "Instalação de programa", Category = "Software", Priority = "Baixa", Status = "Concluído" },
            new ChamadoApiModel { Id = 8, Title = "Sem acesso à pasta compartilhada", Category = "Rede", Priority = "Média", Status = "Aberto" },
            new ChamadoApiModel { Id = 9, Title = "Erro ao salvar arquivo", Category = "Software", Priority = "Média", Status = "Aberto" },
            new ChamadoApiModel { Id = 10, Title = "Monitor piscando", Category = "Hardware", Priority = "Baixa", Status = "Concluído" }
        };

        /// <summary>
        /// Endpoint para obter todos os chamados. Usado para popular a dashboard.
        /// </summary>
        /// <returns>
        /// Um ActionResult contendo a lista de <see cref="ChamadoApiModel"/> (HTTP 200 Ok) 
        /// ou NotFound se nenhum chamado for encontrado.
        /// </returns>
        [HttpGet] // Rota: GET api/chamados
        public IActionResult GetChamados()
        {
            if (!_chamados.Any())
            {
                return NotFound("Nenhum chamado encontrado."); 
            }

            return Ok(_chamados);
        }

        /// <summary>
        /// Aprova (conclui) um chamado pelo ID, alterando seu status.
        /// </summary>
        /// <param name="id">O identificador único do chamado a ser aprovado.</param>
        /// <returns>
        /// Um ActionResult contendo o chamado atualizado (HTTP 200 Ok) 
        /// ou NotFound se o chamado não for encontrado.
        /// </returns>
        [HttpPost("approve/{id}")]
        public IActionResult Approve(int id)
        {
            var chamado = _chamados.FirstOrDefault(c => c.Id == id);
            if (chamado == null)
                return NotFound();

            chamado.Status = "Concluído";
            return Ok(chamado);
        }

        /// <summary>
        /// Rejeita um chamado pelo ID, alterando seu status.
        /// </summary>
        /// <param name="id">O identificador único do chamado a ser rejeitado.</param>
        /// <returns>
        /// Um ActionResult contendo o chamado atualizado (HTTP 200 Ok) 
        /// ou NotFound se o chamado não for encontrado.
        /// </returns>
        [HttpPost("reject/{id}")]
        public IActionResult Reject(int id)
        {
            var chamado = _chamados.FirstOrDefault(c => c.Id == id);
            if (chamado == null)
                return NotFound();

            chamado.Status = "Rejeitado";
            return Ok(chamado);
        }
    }

    /// <summary>
    /// Modelo de dados simplificado para a API de Chamados.
    /// </summary>
    public class ChamadoApiModel
    {
        /// <summary>
        /// Identificador único do chamado.
        /// </summary>
        public int Id { get; set; }
        
        /// <summary>
        /// Título ou breve descrição do chamado.
        /// </summary>
        public string? Title { get; set; }
        
        /// <summary>
        /// Categoria do chamado (e.g., Hardware, Software, Rede).
        /// </summary>
        public string? Category { get; set; }
        
        /// <summary>
        /// Nível de prioridade do chamado (e.g., Alta, Média, Baixa).
        /// </summary>
        public string? Priority { get; set; }
        
        /// <summary>
        /// Status atual do chamado (e.g., Aberto, Em Andamento, Concluído).
        /// </summary>
        public string? Status { get; set; }
    }
}