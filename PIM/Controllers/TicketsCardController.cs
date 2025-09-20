using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PIM.Data;
using PIM.Models;
using PIM.ViewModels;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace PIM.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TicketsCardController : ControllerBase
    {
        private readonly AppDbContext _context;

        public TicketsCardController(AppDbContext context)
        {
            _context = context;
        }

        // Retorna todos os tickets, mesmo os sem atribuição
        [HttpGet("GetTickets")]
        public async Task<IActionResult> GetTickets(TicketFilterViewModel filters)
        {
            var query = _context.Chamados
                                .Include(c => c.AtribuidoA) // Inclui o usuário atribuído
                                .AsQueryable();

            // Filtros opcionais
            if (filters.StartDate.HasValue)
                query = query.Where(t => t.DataAbertura >= filters.StartDate.Value);

            if (filters.EndDate.HasValue)
                query = query.Where(t => t.DataAbertura <= filters.EndDate.Value);

            if (!string.IsNullOrEmpty(filters.Status))
                query = query.Where(t => t.Status == filters.Status);

            if (filters.AssignedToId.HasValue)
                query = query.Where(t => t.AtribuidoAId.HasValue && t.AtribuidoAId == filters.AssignedToId.Value);

            var tickets = await query
                .Select(t => new
                {
                    TicketId = t.ChamadoId,
                    t.Titulo,
                    t.Categoria,
                    t.Prioridade,
                    t.Status,
                    t.DataAbertura,
                    // Mostra nome do atribuído ou null se não tiver
                    AssignedTo = t.AtribuidoA != null ? t.AtribuidoA.Username : null
                })
                .ToListAsync();

            return Ok(tickets);
        }

        // Aprova e atribui o ticket ao usuário logado
        [HttpPost("Aprovar/{id}")]
        public async Task<IActionResult> Aprovar(int id)
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdStr, out int usuarioId))
                return Unauthorized();

            var chamado = await _context.Chamados.FindAsync(id);
            if (chamado == null)
                return NotFound();

            chamado.Status = "Em Andamento";
            chamado.AtribuidoAId = usuarioId; // Atribui ao usuário logado
            chamado.DataAtribuicao = System.DateTime.Now;

            await _context.SaveChangesAsync();

            return Ok();
        }

        // Rejeitar ticket
        [HttpPost("Rejeitar/{id}")]
        public async Task<IActionResult> Rejeitar(int id)
        {
            var chamado = await _context.Chamados.FindAsync(id);
            if (chamado == null)
                return NotFound();

            chamado.Status = "Rejeitado";
            await _context.SaveChangesAsync();
            return Ok();
        }

        // Deletar ticket
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var chamado = await _context.Chamados.FindAsync(id);
            if (chamado == null)
                return NotFound();

            _context.Chamados.Remove(chamado);
            await _context.SaveChangesAsync();
            return Ok();
        }

        // Pega ticket específico
        [HttpGet("{id}")]
        public async Task<IActionResult> GetTicket(int id)
        {
            var ticket = await _context.Chamados
                                       .Include(c => c.AtribuidoA)
                                       .FirstOrDefaultAsync(c => c.ChamadoId == id);

            if (ticket == null)
                return NotFound();

            return Ok(new
            {
                ticket.ChamadoId,
                ticket.Titulo,
                ticket.Categoria,
                ticket.Prioridade,
                ticket.Status,
                ticket.DataAbertura,
                ticket.DataFechamento,
                AssignedTo = ticket.AtribuidoA != null ? ticket.AtribuidoA.Username : null
            });
        }
    }
}
