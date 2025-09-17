using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using PIM.Data;
using PIM.Models;
using PIM.ViewModels;
using System.Linq;
using PIM.ViewModels;

namespace PIM.Controllers
{
    public class TicketsCardController : Controller
    {
        private readonly AppDbContext _context;

        public TicketsCardController(AppDbContext context)
        {
            _context = context;
        }

        public IActionResult Index(TicketFilterViewModel filters, int pageNumber = 1, int pageSize = 6)
        {
            // 1. Query base
            var query = _context.Chamados
                                .Include(c => c.AtribuidoA)
                                .AsQueryable();

            // 2. Aplicar filtros
            if (filters.StartDate.HasValue)
                query = query.Where(t => t.DataAbertura >= filters.StartDate.Value);

            if (filters.EndDate.HasValue)
                query = query.Where(t => t.DataAbertura <= filters.EndDate.Value);

            if (!string.IsNullOrEmpty(filters.Status))
                query = query.Where(t => t.Status == filters.Status);

            if (filters.AssignedToId.HasValue)
                query = query.Where(t => t.AtribuidoA_AdminId.HasValue && t.AtribuidoA_AdminId == filters.AssignedToId.Value);

            // 3. Total de itens filtrados
            int totalItems = query.Count();

            // 4. Paginado
            var chamadosPaginados = query
                                    .OrderByDescending(t => t.DataAbertura)
                                    .Skip((pageNumber - 1) * pageSize)
                                    .Take(pageSize)
                                    .ToList();

            // 5. Montar ViewModel
            var viewModel = new TicketsCardViewModel
            {
                Tickets = chamadosPaginados,  // Aqui usamos Chamados
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalItems = totalItems,
                FilterOptions = new TicketFilterOptions
                {
                    Statuses = _context.Chamados
                                       .Select(t => t.Status ?? string.Empty)
                                       .Distinct()
                                       .Select(s => new SelectListItem { Text = s, Value = s })
                                       .ToList(),
                    Analysts = _context.Admins
                                       .Select(a => new SelectListItem { Text = a.Username, Value = a.Id.ToString() })
                                       .ToList()
                },
                SelectedStatus = filters.Status,
                SelectedAnalystId = filters.AssignedToId
            };

            return View(viewModel);
        }

        // Aprovar chamado
        public IActionResult Aprovar(int id)
        {
            var chamado = _context.Chamados.Find(id);
            if (chamado != null)
            {
                chamado.Status = "Aprovado";
                _context.SaveChanges();
            }
            return RedirectToAction("Index");
        }

        // Rejeitar chamado
        public IActionResult Rejeitar(int id)
        {
            var chamado = _context.Chamados.Find(id);
            if (chamado != null)
            {
                chamado.Status = "Rejeitado";
                _context.SaveChanges();
            }
            return RedirectToAction("Index");
        }
    }
}
