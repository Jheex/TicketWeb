using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PIM.Data;
using System.Linq;
using System.Collections.Generic; // Necessário para List<>

namespace PIM.Controllers
{
    /// <summary>
    /// Controlador responsável por exibir a lista de chamados (tickets) para visualização.
    /// Ele lida principalmente com a paginação dos tickets e carrega opções básicas de filtro.
    /// </summary>
    public class HometicketController : Controller
    {
        private readonly AppDbContext _context;

        /// <summary>
        /// Inicializa uma nova instância do controlador HometicketController.
        /// </summary>
        /// <param name="context">O contexto do banco de dados (AppDbContext) injetado via DI.</param>
        public HometicketController(AppDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Exibe a lista paginada de chamados, ordenada pela data de abertura mais recente.
        /// </summary>
        /// <param name="pageNumber">O número da página atual a ser exibida (padrão é 1).</param>
        /// <param name="pageSize">O número de itens por página (padrão é 6).</param>
        /// <returns>A View Index com o <see cref="PIM.ViewModels.TicketsCardViewModel"/> contendo os tickets paginados.</returns>
        public IActionResult Index(int pageNumber = 1, int pageSize = 6)
        {
            // Aplica paginação e ordena do mais recente para o mais antigo
            var chamados = _context.Chamados
                                     .Include(c => c.AtribuidoA) // Inclui o usuário atribuído para exibição
                                     .OrderByDescending(c => c.DataAbertura)
                                     .Skip((pageNumber - 1) * pageSize)
                                     .Take(pageSize)
                                     .ToList();

            // Total de itens para cálculo de paginação
            var totalItems = _context.Chamados.Count();

            // Monta o ViewModel para a View
            var viewModel = new PIM.ViewModels.TicketsCardViewModel
            {
                Tickets = chamados,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalItems = totalItems,
                FilterOptions = new PIM.ViewModels.TicketFilterOptions
                {
                    // Carrega opções de Status disponíveis
                    Statuses = _context.Chamados
                        .Select(c => c.Status ?? "")
                        .Distinct()
                        .Select(s => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem { Text = s, Value = s })
                        .ToList(),

                    // Carrega opções de Analistas/Usuários
                    Analysts = _context.Usuarios
                        .Select(a => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem { Text = a.Username, Value = a.Id.ToString() })
                        .ToList()
                }
            };

            return View(viewModel);
        }
    }
}