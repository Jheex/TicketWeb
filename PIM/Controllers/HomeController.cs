using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PIM.Data;
using PIM.Models;
using PIM.ViewModels;
using System.Linq;
using System.Collections.Generic; // Necessário para .Any() e List<>

namespace PIM.Controllers
{
    /// <summary>
    /// Controlador principal da aplicação, responsável por exibir a Dashboard inicial.
    /// Este controlador aplica filtros, gerencia a paginação e calcula os principais KPIs (Key Performance Indicators)
    /// baseados nos dados de chamados.
    /// </summary>
    public class HomeController : Controller
    {
        private readonly AppDbContext _context;

        /// <summary>
        /// Inicializa uma nova instância do controlador HomeController.
        /// </summary>
        /// <param name="context">O contexto do banco de dados (AppDbContext) injetado via DI.</param>
        public HomeController(AppDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Exibe a página principal da Dashboard, aplicando filtros e paginação nos chamados.
        /// </summary>
        /// <param name="filters">O <see cref="DashboardFilterViewModel"/> contendo os filtros de data, status, prioridade e atribuição.</param>
        /// <param name="pageNumber">O número da página atual para a paginação da tabela (padrão é 1).</param>
        /// <param name="pageSize">O número de itens por página na tabela (padrão é 5).</param>
        /// <returns>A View Index com o <see cref="DashboardBIViewModel"/> preenchido com dados e KPIs.</returns>
        public IActionResult Index(DashboardFilterViewModel filters, int pageNumber = 1, int pageSize = 5)
        {
            // Inicia a consulta com as inclusões necessárias (joins)
            var query = _context.Chamados
                .Include(c => c.AtribuidoA) // Navegação para Usuario Atribuído
                .Include(c => c.Solicitante) // Navegação para Usuario Solicitante
                .AsQueryable();

            // Aplicando filtros
            if (filters.StartDate.HasValue)
                query = query.Where(c => c.DataAbertura >= filters.StartDate.Value);

            if (filters.EndDate.HasValue)
                query = query.Where(c => c.DataAbertura <= filters.EndDate.Value);

            if (filters.Status != null && filters.Status.Any())
                query = query.Where(c => filters.Status.Contains(c.Status ?? string.Empty));

            if (filters.Priority != null && filters.Priority.Any())
                query = query.Where(c => filters.Priority.Contains(c.Prioridade ?? string.Empty));

            if (filters.AssignedToId != null && filters.AssignedToId.Any())
                query = query.Where(c => c.AtribuidoAId.HasValue && filters.AssignedToId.Contains(c.AtribuidoAId.Value));

            var totalItems = query.Count();

            // Chamados paginados
            var chamadosPaginados = query
                .OrderByDescending(c => c.DataAbertura)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            // Cálculo do tempo médio de resolução (TTR - Time to Resolution)
            var chamadosFechados = query.Where(c => c.DataFechamento.HasValue).ToList();
            double tempoMedioResolucao = chamadosFechados.Any()
                ? chamadosFechados.Average(c => (c.DataFechamento!.Value - c.DataAbertura).TotalHours)
                : 0;

            // Montando ViewModel
            var viewModel = new DashboardBIViewModel
            {
                // Cálculo dos KPls (baseado na query filtrada)
                TotalChamadosAbertos = query.Count(c => c.Status == "Aberto"),
                TotalChamadosFechados = query.Count(c => c.Status == "Fechado"),
                TempoMedioResolucaoHoras = tempoMedioResolucao,
                // Cálculo do SLA (Service Level Agreement)
                SlaPercentual = totalItems > 0 ? (double)query.Count(c => c.Status == "Fechado") / totalItems * 100 : 0,
                
                TabelaDetalhada = chamadosPaginados,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalItems = totalItems,
                
                // Opções de filtros disponíveis (carregadas do DB)
                FilterOptions = new DashboardFilterOptions
                {
                    Statuses = _context.Chamados.Select(c => c.Status ?? string.Empty).Distinct()
                                                 .Select(s => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem { Text = s, Value = s })
                                                 .ToList(),
                    Priorities = _context.Chamados.Select(c => c.Prioridade ?? string.Empty).Distinct()
                                                 .Select(p => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem { Text = p, Value = p })
                                                 .ToList(),
                    Analysts = _context.Usuarios
                                                 .Select(a => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem { Text = a.Username, Value = a.Id.ToString() })
                                                 .ToList()
                },
                
                // Armazena os filtros selecionados para manter o estado da UI
                SelectedStatus = filters.Status,
                SelectedPriority = filters.Priority,
                SelectedAnalystId = filters.AssignedToId
            };

            return View(viewModel);
        }
    }
}