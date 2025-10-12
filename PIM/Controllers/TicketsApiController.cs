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

        // GET: api/TicketsApi
        [HttpGet]
        public async Task<IActionResult> GetTickets()
        {
            var tickets = await _context.Chamados
                .Include(c => c.AtribuidoA)
                .Include(c => c.Solicitante)
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
                assignedToId = c.AtribuidoAId.HasValue ? c.AtribuidoAId.Value.ToString() : null, // <-- ADICIONADO/CORRIGIDO
                solicitante = c.Solicitante != null ? c.Solicitante.Username : "Desconhecido",
                dataAbertura = c.DataAbertura,
                dataAtribuicao = c.DataAtribuicao
            });

            return Ok(result);
        }

        // GET: api/TicketsApi/MyTickets
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
                .Include(c => c.Solicitante)
                .Where(c => c.AtribuidoAId == userId)
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
                assignedToId = c.AtribuidoAId.HasValue ? c.AtribuidoAId.Value.ToString() : null, // <-- ADICIONADO/CORRIGIDO
                solicitante = c.Solicitante != null ? c.Solicitante.Username : "Desconhecido",
                dataAbertura = c.DataAbertura,
                dataAtribuicao = c.DataAtribuicao
            });

            return Ok(result);
        }

        // GET: api/TicketsApi/5
        [HttpGet("{id}")]
        public async Task<IActionResult> GetTicket(int id)
        {
            var ticket = await _context.Chamados
                .Include(c => c.AtribuidoA)
                .Include(c => c.Solicitante)
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
                dataAtribuicao = ticket.DataAtribuicao,
                assignedTo = ticket.AtribuidoA != null ? ticket.AtribuidoA.Username : "Não atribuído",
                assignedToId = ticket.AtribuidoAId.HasValue ? ticket.AtribuidoAId.Value.ToString() : null, // <-- ADICIONADO/CORRIGIDO
                solicitante = ticket.Solicitante != null ? ticket.Solicitante.Username : "Desconhecido"
            };

            return Ok(result);
        }

        // POST: api/TicketsApi/approve/5
        [HttpPost("approve/{id}")]
        public async Task<IActionResult> Approve(int id)
        {
            var ticket = await _context.Chamados.FindAsync(id);
            if (ticket == null) return NotFound();

            if (ticket.Status != "Aberto")
                return BadRequest("Ticket já está em andamento ou concluído.");

            // Lógica CORRETA: Atribui o ID do usuário logado (Analista) e muda o status.
            ticket.Status = "Em Andamento";
            ticket.AtribuidoAId = GetCurrentUserId();
            ticket.DataAtribuicao = DateTime.Now;

            await _context.SaveChangesAsync();
            return Ok();
        }

        // POST: api/TicketsApi/conclude/5
        [HttpPost("conclude/{id}")]
        public async Task<IActionResult> Conclude(int id)
        {
            var ticket = await _context.Chamados.FindAsync(id);
            if (ticket == null) return NotFound();

            if (ticket.Status != "Em Andamento")
                return BadRequest("Só é possível concluir tickets em andamento.");

            ticket.Status = "Concluído";
            ticket.DataFechamento = DateTime.Now;

            await _context.SaveChangesAsync();
            return Ok();
        }

        // DELETE: api/TicketsApi/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTicket(int id)
        {
            var ticket = await _context.Chamados.FindAsync(id);
            if (ticket == null) return NotFound();

            _context.Chamados.Remove(ticket);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // Método utilitário para pegar o ID do usuário logado
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