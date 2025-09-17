using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PIM.Data;

namespace PIM.Controllers
{
    public class HometicketController : Controller
    {
        private readonly AppDbContext _context;

        public HometicketController(AppDbContext context)
        {
            _context = context;
        }

        public IActionResult Index(int pageNumber = 1, int pageSize = 6)
        {
            var chamados = _context.Chamados
                                   .Include(c => c.AtribuidoA)
                                   .OrderByDescending(c => c.DataAbertura)
                                   .Skip((pageNumber - 1) * pageSize)
                                   .Take(pageSize)
                                   .ToList();

            var totalItems = _context.Chamados.Count();

            var viewModel = new PIM.ViewModels.TicketsCardViewModel
            {
                Tickets = chamados,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalItems = totalItems,
                FilterOptions = new PIM.ViewModels.TicketFilterOptions
                {
                    Statuses = _context.Chamados
                        .Select(c => c.Status ?? "")
                        .Distinct()
                        .Select(s => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem { Text = s, Value = s })
                        .ToList(),

                    Analysts = _context.Admins
                        .Select(a => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem { Text = a.Username, Value = a.Id.ToString() })
                        .ToList()
                }
            };

            return View(viewModel);
        }
    }
}
