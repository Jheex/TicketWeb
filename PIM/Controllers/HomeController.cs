using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PIM.Data;
using PIM.Models;
using PIM.ViewModels;
using System.Linq;

namespace PIM.Controllers
{
    public class HomeController : Controller
    {
        private readonly AppDbContext _context;

        public HomeController(AppDbContext context)
        {
            _context = context;
        }

        public IActionResult Index(DashboardFilterViewModel filters, int pageNumber = 1, int pageSize = 5)
        {
            var query = _context.Chamados
                .Include(c => c.AtribuidoA)
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
                query = query.Where(c => c.AtribuidoA_AdminId.HasValue && filters.AssignedToId.Contains(c.AtribuidoA_AdminId.Value));

            var totalItems = query.Count();

            // Chamados paginados
            var chamadosPaginados = query
                .OrderByDescending(c => c.DataAbertura)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            // Cálculo do tempo médio com proteção para sequência vazia
            var chamadosFechados = query.Where(c => c.DataFechamento.HasValue).ToList();
            double tempoMedioResolucao = chamadosFechados.Any()
                ? chamadosFechados.Average(c => (c.DataFechamento.Value - c.DataAbertura).TotalHours)
                : 0;

            // Montando ViewModel
            var viewModel = new DashboardBIViewModel
            {
                TotalChamadosAbertos = query.Count(c => c.Status == "Aberto"),
                TotalChamadosFechados = query.Count(c => c.Status == "Fechado"),
                TempoMedioResolucaoHoras = tempoMedioResolucao,
                SlaPercentual = totalItems > 0 ? (double)query.Count(c => c.Status == "Fechado") / totalItems * 100 : 0,
                TabelaDetalhada = chamadosPaginados,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalItems = totalItems,
                FilterOptions = new DashboardFilterOptions
                {
                    Statuses = _context.Chamados.Select(c => c.Status ?? string.Empty).Distinct()
                                                 .Select(s => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem { Text = s, Value = s })
                                                 .ToList(),
                    Priorities = _context.Chamados.Select(c => c.Prioridade ?? string.Empty).Distinct()
                                                 .Select(p => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem { Text = p, Value = p })
                                                 .ToList(),
                    Analysts = _context.Admins
                                                 .Select(a => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem { Text = a.Username, Value = a.Id.ToString() })
                                                 .ToList()
                },
                SelectedStatus = filters.Status,
                SelectedPriority = filters.Priority,
                SelectedAnalystId = filters.AssignedToId
            };

            return View(viewModel);
        }
    }
}
