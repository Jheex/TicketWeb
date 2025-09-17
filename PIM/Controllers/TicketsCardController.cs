using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using PIM.Data;
using PIM.Models;
using PIM.ViewModels;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace PIM.Controllers
{
    // NOVO: Define a rota base e o comportamento de API
    [Route("api/[controller]")]
    [ApiController]
    public class TicketsCardController : ControllerBase
    {
        private readonly AppDbContext _context;

        public TicketsCardController(AppDbContext context)
        {
            _context = context;
        }
        
        // NOVO: Método para buscar os tickets como API
        [HttpGet("GetTickets")]
        public async Task<IActionResult> GetTickets(TicketFilterViewModel filters)
        {
            var query = _context.Chamados
                                 .Include(c => c.AtribuidoA)
                                 .AsQueryable();

            if (filters.StartDate.HasValue)
                query = query.Where(t => t.DataAbertura >= filters.StartDate.Value);

            if (filters.EndDate.HasValue)
                query = query.Where(t => t.DataAbertura <= filters.EndDate.Value);

            if (!string.IsNullOrEmpty(filters.Status))
                query = query.Where(t => t.Status == filters.Status);

            if (filters.AssignedToId.HasValue)
                query = query.Where(t => t.AtribuidoA_AdminId.HasValue && t.AtribuidoA_AdminId == filters.AssignedToId.Value);

            var tickets = await query.ToListAsync();

            // Retorna a lista como JSON
            return Ok(tickets);
        }

        [HttpPost("Aprovar/{id}")]
        public async Task<IActionResult> Aprovar(int id)
        {
            // Substitua esta lógica para obter o ID do usuário logado do seu sistema
            // Exemplo: var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            // int adminId = int.Parse(userId); 
            int adminId = 1; // ID fixo para teste

            var chamado = await _context.Chamados.FindAsync(id);
            if (chamado == null)
            {
                return NotFound();
            }

            chamado.Status = "Em Andamento";
            chamado.AtribuidoA_AdminId = adminId;
            await _context.SaveChangesAsync();

            return Ok();
        }

        [HttpPost("Rejeitar/{id}")]
        public async Task<IActionResult> Rejeitar(int id)
        {
            var chamado = await _context.Chamados.FindAsync(id);
            if (chamado == null)
            {
                return NotFound();
            }

            chamado.Status = "Rejeitado";
            await _context.SaveChangesAsync();
            return Ok();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var chamado = await _context.Chamados.FindAsync(id);
            if (chamado == null)
            {
                return NotFound();
            }

            _context.Chamados.Remove(chamado);
            await _context.SaveChangesAsync();
            return Ok();
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetTicket(int id)
        {
            var ticket = await _context.Chamados.FindAsync(id);
            if (ticket == null)
            {
                return NotFound();
            }
            return Ok(ticket);
        }
    }
}