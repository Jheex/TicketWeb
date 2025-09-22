using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using PIM.Data;
using PIM.Models;
using PIM.ViewModels;
using System.Linq;

namespace PIM.Controllers
{
    public class DashboardController : Controller
    {
        private readonly AppDbContext _context;

        public DashboardController(AppDbContext context)
        {
            _context = context;
        }

        public IActionResult Index(string status, string priority, int? assignedToId, int? requesterId, int pageNumber = 1, int pageSize = 5)
        {
            // --- Lógica para a Tabela Detalhada (que recebe os filtros) ---
            var query = _context.Chamados
                                .Include(c => c.AtribuidoA)
                                .Include(c => c.Solicitante)
                                .AsQueryable();

            if (!string.IsNullOrEmpty(status))
                query = query.Where(c => c.Status == status);

            if (!string.IsNullOrEmpty(priority))
                query = query.Where(c => c.Prioridade == priority);

            if (requesterId.HasValue)
                query = query.Where(c => c.SolicitanteId == requesterId.Value);

            if (assignedToId.HasValue)
                query = query.Where(c => c.AtribuidoAId == assignedToId.Value);

            int totalItems = query.Count();

            var chamadosPaginados = query
                                        .OrderByDescending(c => c.DataAbertura)
                                        .Skip((pageNumber - 1) * pageSize)
                                        .Take(pageSize)
                                        .ToList();

            // --- Lógica para os KPIs (sempre baseada em TODOS os chamados) ---
            var chamadosKpiQuery = _context.Chamados
                .AsNoTracking()
                .Select(c => new { c.Status, c.DataAbertura, c.DataFechamento });

            // total geral (para cálculo do SLA)
            int totalChamadosCount = chamadosKpiQuery.Count();

            // Fechados
            var totalChamadosFechados = chamadosKpiQuery
                .Count(c => c.Status != null &&
                            (c.Status.Trim().ToLower() == "fechado" ||
                             c.Status.Trim().ToLower() == "concluído" ||
                             c.Status.Trim().ToLower() == "concluido"));

            // Abertos
            var totalChamadosAbertos = chamadosKpiQuery
                .Count(c => c.Status != null && c.Status.Trim().ToLower() == "aberto");

            // Em andamento (cobre variações comuns)
            var totalChamadosEmAndamento = chamadosKpiQuery.Count(c =>
                c.Status != null && (
                    c.Status.Trim().ToLower() == "em andamento" ||
                    c.Status.Trim().ToLower() == "andamento" ||
                    c.Status.Trim().ToLower() == "em atendimento" ||
                    c.Status.Trim().ToLower() == "em progresso" ||
                    c.Status.Trim().ToLower() == "atribuído" ||
                    c.Status.Trim().ToLower() == "atribuido"
                ));

            // TTR (horas)
            var tempoMedioResolucaoHoras = _context.Chamados
                .AsNoTracking()
                .Where(c => c.DataFechamento.HasValue)
                .Select(c => new { c.DataAbertura, c.DataFechamento })
                .AsEnumerable()
                .Select(x => (x.DataFechamento!.Value - x.DataAbertura).TotalHours)
                .DefaultIfEmpty(0)
                .Average();

            // SLA percentual
            var slaPercentual = totalChamadosCount > 0
                ? (double)totalChamadosFechados / totalChamadosCount * 100
                : 0;

            // --- Monta o ViewModel com todos os dados ---
            var viewModel = new DashboardBIViewModel
            {
                TotalChamadosAbertos = totalChamadosAbertos,
                TotalChamadosEmAndamento = totalChamadosEmAndamento,
                TotalChamadosFechados = totalChamadosFechados,
                TempoMedioResolucaoHoras = tempoMedioResolucaoHoras,
                SlaPercentual = slaPercentual,
                TabelaDetalhada = chamadosPaginados,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalItems = totalItems,
                FilterOptions = new DashboardFilterOptions()
            };

            // --- Preenche as opções dos filtros ---

            // Status
            viewModel.FilterOptions.Statuses = _context.Chamados
                .Select(c => c.Status ?? string.Empty)
                .Distinct()
                .Select(s => new SelectListItem
                {
                    Text = s,
                    Value = s,
                    Selected = !string.IsNullOrEmpty(status) && s == status
                })
                .ToList();
            viewModel.FilterOptions.Statuses.Insert(0, new SelectListItem { Text = "Selecione status", Value = "", Selected = string.IsNullOrEmpty(status) });

            // Prioridades
            viewModel.FilterOptions.Priorities = _context.Chamados
                .Select(c => c.Prioridade ?? string.Empty)
                .Distinct()
                .Select(p => new SelectListItem
                {
                    Text = p,
                    Value = p,
                    Selected = !string.IsNullOrEmpty(priority) && p == priority
                })
                .ToList();
            viewModel.FilterOptions.Priorities.Insert(0, new SelectListItem { Text = "Selecione prioridade", Value = "", Selected = string.IsNullOrEmpty(priority) });

            // Analistas
            viewModel.FilterOptions.Analysts = _context.Usuarios
                .Select(a => new SelectListItem
                {
                    Text = a.Username,
                    Value = a.Id.ToString(),
                    Selected = assignedToId.HasValue && a.Id == assignedToId.Value
                })
                .ToList();
            viewModel.FilterOptions.Analysts.Insert(0, new SelectListItem { Text = "Selecione analista", Value = "", Selected = !assignedToId.HasValue });

            // Solicitantes
            viewModel.FilterOptions.Requesters = _context.Usuarios
                .Select(u => new SelectListItem
                {
                    Text = u.Username,
                    Value = u.Id.ToString(),
                    Selected = requesterId.HasValue && u.Id == requesterId.Value
                })
                .ToList();
            viewModel.FilterOptions.Requesters.Insert(0, new SelectListItem { Text = "Selecione solicitante", Value = "", Selected = !requesterId.HasValue });

            // Selected lists
            viewModel.SelectedStatus = string.IsNullOrEmpty(status) ? new List<string>() : new List<string> { status };
            viewModel.SelectedPriority = string.IsNullOrEmpty(priority) ? new List<string>() : new List<string> { priority };
            viewModel.SelectedAnalystId = assignedToId.HasValue ? new List<int> { assignedToId.Value } : new List<int>();
            viewModel.SelectedRequesterId = requesterId.HasValue ? new List<int> { requesterId.Value } : new List<int>();

            return View("~/Views/Home/Index.cshtml", viewModel);
        }
    }
}
