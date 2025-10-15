using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PIM.Data;
using System.Linq;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Authorization; // Adicionado Authorize, comum para relatórios gerenciais

namespace PIM.Controllers
{
    /// <summary>
    /// Controlador responsável por gerar relatórios gerenciais e dados agregados
    /// para visualização em dashboards (gráficos) e exportação.
    /// </summary>
    [Authorize] // Assume-se que relatórios gerenciais exigem autenticação
    public class RelatorioGerencialController : Controller
    {
        private readonly AppDbContext _context;

        /// <summary>
        /// Inicializa uma nova instância do controlador RelatorioGerencialController.
        /// </summary>
        /// <param name="context">O contexto do banco de dados (AppDbContext) injetado via DI.</param>
        public RelatorioGerencialController(AppDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Exibe a View principal do Relatório Gerencial (geralmente uma página vazia ou com placeholders para gráficos).
        /// </summary>
        /// <returns>A View Relatorio.</returns>
        [HttpGet]
        public IActionResult Relatorio()
        {
            return View();
        }

        // ------------------------------------------------------------------
        // ENDPOINT PARA DADOS AGREGADOS (GRÁFICOS)
        // ------------------------------------------------------------------

        /// <summary>
        /// Obtém dados agregados de chamados para alimentar gráficos e KPIs.
        /// Os dados são retornados em formato JSON.
        /// </summary>
        /// <param name="periodo">Filtro de período (ex: "30d", "90d"). Não implementado na query, mas disponível para extensão.</param>
        /// <param name="tecnico">Filtro por técnico. Não implementado na query, mas disponível para extensão.</param>
        /// <returns>Um objeto JSON contendo totais, taxa de conclusão, e agrupamentos por técnicos e categorias.</returns>
        [HttpGet]
        public async Task<IActionResult> ObterDados(string periodo = "30d", string tecnico = "todos")
        {
            // Nota: Adicionar lógica de filtragem de 'periodo' e 'tecnico' aqui, se necessário.
            var chamados = await _context.Chamados.Include(c => c.AtribuidoA).ToListAsync();

            var totalChamados = chamados.Count;
            
            // Contagem por Status, usando comparação case-insensitive para maior robustez
            var abertos = chamados.Count(c => c.Status != null && c.Status.Trim().ToLower() == "aberto");
            var andamento = chamados.Count(c => c.Status != null && 
                (c.Status.Trim().ToLower() == "em andamento" || 
                 c.Status.Trim().ToLower() == "andamento" || 
                 c.Status.Trim().ToLower() == "em atendimento" || 
                 c.Status.Trim().ToLower() == "em progresso"));
            var finalizados = chamados.Count(c => c.Status != null &&
                (c.Status.Trim().ToLower() == "fechado" || 
                 c.Status.Trim().ToLower() == "concluído" || 
                 c.Status.Trim().ToLower() == "concluido"));

            // Cálculo da Taxa de Conclusão
            double taxaConclusao = totalChamados > 0
                ? Math.Round((double)finalizados / totalChamados * 100, 1)
                : 0;

            // Agrupa por técnico responsável
            var tecnicos = chamados
                .Where(c => c.AtribuidoA != null)
                // Usa o operador ?? para garantir que a chave de agrupamento não seja nula (evitando CS8602/CS8604)
                .GroupBy(c => c.AtribuidoA!.Username ?? "Não Atribuído") 
                .Select(g => new
                {
                    Tecnico = g.Key,
                    Abertos = g.Count(c => c.Status != null && c.Status.Trim().ToLower() == "aberto"),
                    Andamento = g.Count(c => c.Status != null && 
                        (c.Status.Trim().ToLower() == "em andamento" ||
                         c.Status.Trim().ToLower() == "andamento" ||
                         c.Status.Trim().ToLower() == "em atendimento" ||
                         c.Status.Trim().ToLower() == "em progresso")),
                    Finalizados = g.Count(c => c.Status != null &&
                        (c.Status.Trim().ToLower() == "fechado" ||
                         c.Status.Trim().ToLower() == "concluído" ||
                         c.Status.Trim().ToLower() == "concluido"))
                })
                .ToList();

            // Agrupa por categoria do chamado
            var categorias = chamados
                .GroupBy(c => c.Categoria ?? "Outros")
                .Select(g => new
                {
                    Categoria = g.Key,
                    Total = g.Count()
                })
                .ToList();

            return Json(new
            {
                abertos,
                andamento,
                finalizados,
                taxaConclusao,
                tecnicos,
                categorias
            });
        }

        // ------------------------------------------------------------------
        // NOVO ENDPOINT PARA EXPORTAÇÃO COMPLETA DE DADOS BRUTOS (CSV)
        // ------------------------------------------------------------------
        
        /// <summary>
        /// Obtém e retorna uma lista de dados brutos de chamados, formatados para exportação (ex: para CSV).
        /// Os dados são retornados em formato JSON.
        /// </summary>
        /// <param name="periodo">Filtro de período (ex: "30d"). Não implementado na query, mas disponível para extensão.</param>
        /// <param name="tecnico">Filtro por técnico. Não implementado na query, mas disponível para extensão.</param>
        /// <returns>Um objeto JSON contendo a lista de chamados com campos selecionados e formatados.</returns>
        [HttpGet]
        public async Task<IActionResult> ExportarChamados(string periodo = "30d", string tecnico = "todos")
        {
            // Inclui o AtribuídoA (Analista) e Solicitante (Usuário que abriu) para o relatório completo
            var chamadosBrutos = await _context.Chamados
                .Include(c => c.AtribuidoA) 
                .Include(c => c.Solicitante)
                .ToListAsync();

            if (chamadosBrutos == null || !chamadosBrutos.Any())
            {
                return Ok(new List<object>()); 
            }

            // Mapeia os dados brutos para o formato de exportação
            var dadosExportacao = chamadosBrutos.Select(c => new
            {
                c.ChamadoId, 
                c.Titulo,
                DataAbertura = c.DataAbertura.ToString("dd/MM/yyyy HH:mm"), // Formato de data mais legível
                c.Categoria,
                c.Status,
                ClienteSolicitante = c.Solicitante?.Username, 
                AtribuidoA = c.AtribuidoA?.Username,
                c.Prioridade, 
                c.Descricao 
            }).ToList();

            return Ok(dadosExportacao);
        }
    }
}