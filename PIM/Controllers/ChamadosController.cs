using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;

// Usando o namespace do seu ViewModel. Ajuste se necessário.
using PIM.ViewModels; 

namespace PIM.Controllers
{
    [ApiController]
    [Route("api/[controller]")] // Define a rota base da API para api/chamados
    public class ChamadosController : ControllerBase
    {
        // --- SIMULAÇÃO DE UM BANCO DE DADOS ---
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
        /// Endpoint para obter todos os chamados para o dashboard.
        /// </summary>
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
        /// Aprova (conclui) um chamado pelo ID.
        /// </summary>
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
        /// Rejeita um chamado pelo ID.
        /// </summary>
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
    /// Modelo de dados simplificado para a API.
    /// </summary>
    public class ChamadoApiModel
    {
        public int Id { get; set; }
        public string? Title { get; set; }
        public string? Category { get; set; }
        public string? Priority { get; set; }
        public string? Status { get; set; }
    }
}
