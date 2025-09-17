using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PIM.Data;
using PIM.Models;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace PIM.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TicketsApiController : ControllerBase
    {
        private readonly AppDbContext _context;

        public TicketsApiController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetTickets()
        {
            var tickets = await _context.Chamados
                .Include(c => c.AtribuidoA)
                .OrderByDescending(c => c.ChamadoId)
                .ToListAsync();

            var result = tickets.Select(c => new
            {
                id = c.ChamadoId,
                title = c.Titulo,
                category = c.Categoria,
                priority = c.Prioridade,
                status = c.Status,
                assignedTo = c.AtribuidoA != null ? c.AtribuidoA.Username : null,
                dataAbertura = c.DataAbertura
            });

            return Ok(result);
        }

        // NOVO ENDPOINT: Pega tickets atribuídos ao usuário logado
        [HttpGet("MyTickets")]
        public async Task<IActionResult> GetMyTickets()
        {
            var userId = GetCurrentUserId();
            if (userId == null)
            {
                return Unauthorized();
            }

            var tickets = await _context.Chamados
                .Include(c => c.AtribuidoA)
                .Where(c => c.AtribuidoA_AdminId == userId)
                .OrderByDescending(c => c.ChamadoId)
                .ToListAsync();

            var result = tickets.Select(c => new
            {
                id = c.ChamadoId,
                title = c.Titulo,
                category = c.Categoria,
                priority = c.Prioridade,
                status = c.Status,
                assignedTo = c.AtribuidoA != null ? c.AtribuidoA.Username : null,
                dataAbertura = c.DataAbertura
            });

            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetTicket(int id)
        {
            var ticket = await _context.Chamados
                .Include(c => c.AtribuidoA)
                .FirstOrDefaultAsync(c => c.ChamadoId == id);

            if (ticket == null)
            {
                return NotFound();
            }

            var result = new
            {
                id = ticket.ChamadoId,
                title = ticket.Titulo,
                description = ticket.Descricao,
                category = ticket.Categoria,
                priority = ticket.Prioridade,
                status = ticket.Status,
                dataAbertura = ticket.DataAbertura,
                dataFechamento = ticket.DataFechamento,
                assignedTo = ticket.AtribuidoA != null ? ticket.AtribuidoA.Username : "Não atribuído",
            };

            return Ok(result);
        }

        [HttpPost("approve/{id}")]
        public async Task<IActionResult> Approve(int id)
        {
            var ticket = await _context.Chamados.FindAsync(id);
            if (ticket == null)
            {
                return NotFound();
            }

            ticket.Status = "Em Andamento";
            ticket.AtribuidoA_AdminId = GetCurrentUserId();
            await _context.SaveChangesAsync();
            
            return Ok();
        }

        [HttpPost("reject/{id}")]
        public async Task<IActionResult> Reject(int id)
        {
            var ticket = await _context.Chamados.FindAsync(id);
            if (ticket == null)
            {
                return NotFound();
            }
            
            ticket.Status = "Rejeitado";
            await _context.SaveChangesAsync();
            
            return Ok();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTicket(int id)
        {
            var ticket = await _context.Chamados.FindAsync(id);
            
            if (ticket == null)
            {
                return NotFound();
            }

            _context.Chamados.Remove(ticket);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private int? GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim != null && int.TryParse(userIdClaim.Value, out int userId))
            {
                return userId;
            }
            return null;
        }
    }
}