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

        public IActionResult Index(string status, string priority, int? assignedToId, int pageNumber = 1, int pageSize = 5)
        {
            // 1. Query base
            var query = _context.Chamados
                                .Include(c => c.AtribuidoA)
                                .AsQueryable();

            // 2. Aplicar filtros de escolha única
            if (!string.IsNullOrEmpty(status))
            {
                query = query.Where(c => c.Status == status);
            }
            
            if (!string.IsNullOrEmpty(priority))
            {
                query = query.Where(c => c.Prioridade == priority);
            }

            if (assignedToId.HasValue)
            {
                query = query.Where(c => c.AtribuidoAId == assignedToId.Value);
            }

            // 3. Total de itens filtrados
            int totalItems = query.Count();

            // 4. Itens paginados
            var chamadosPaginados = query
                                        .OrderByDescending(c => c.DataAbertura)
                                        .Skip((pageNumber - 1) * pageSize)
                                        .Take(pageSize)
                                        .ToList();

            // 5. Montar ViewModel
            var viewModel = new DashboardBIViewModel
            {
                TotalChamadosAbertos = query.Count(c => c.Status == "Aberto"),
                TotalChamadosFechados = query.Count(c => c.Status == "Fechado"),
                TempoMedioResolucaoHoras = query.Where(c => c.DataFechamento.HasValue)
                                                .Select(c => EF.Functions.DateDiffHour(c.DataAbertura, c.DataFechamento!.Value))
                                                .ToList()
                                                .DefaultIfEmpty(0)
                                                .Average(),
                SlaPercentual = totalItems > 0 ? (double)query.Count(c => c.Status == "Fechado") / totalItems * 100 : 0,
                TabelaDetalhada = chamadosPaginados,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalItems = totalItems,
                FilterOptions = new DashboardFilterOptions
                {
                    Statuses = _context.Chamados.Select(c => c.Status ?? string.Empty).Distinct()
                                                .Select(s => new SelectListItem { Text = s, Value = s }).ToList(),
                    Priorities = _context.Chamados.Select(c => c.Prioridade ?? string.Empty).Distinct()
                                                .Select(p => new SelectListItem { Text = p, Value = p }).ToList(),
                    Analysts = _context.Usuarios
                                                .Select(a => new SelectListItem { Text = a.Username, Value = a.Id.ToString() })
                                                .ToList()
                },
                SelectedStatus = new List<string> { status },
                SelectedPriority = new List<string> { priority },
                SelectedAnalystId = assignedToId.HasValue ? new List<int> { assignedToId.Value } : new List<int>()
            };

            // Retorna a view especificando o caminho correto
            return View("~/Views/Home/Index.cshtml", viewModel);
        }
    }
}