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
    /// <summary>
    /// Controlador da API responsável por fornecer endpoints RESTful para consulta e manipulação de Chamados (Tickets).
    /// <para>Utilizado por aplicações front-end para interagir com os dados dos chamados.</para>
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class TicketsApiController : ControllerBase
    {
        private readonly AppDbContext _context;

        /// <summary>
        /// Inicializa uma nova instância do controlador TicketsApiController.
        /// </summary>
        /// <param name="context">O contexto do banco de dados (AppDbContext) injetado via DI.</param>
        public TicketsApiController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/TicketsApi
        /// <summary>
        /// Obtém uma lista de todos os tickets no sistema, incluindo informações do analista atribuído e do solicitante.
        /// </summary>
        /// <returns>Um IActionResult contendo uma lista paginada ou completa de tickets.</returns>
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
                assignedToId = c.AtribuidoAId.HasValue ? c.AtribuidoAId.Value.ToString() : null,
                solicitante = c.Solicitante != null ? c.Solicitante.Username : "Desconhecido",
                dataAbertura = c.DataAbertura,
                dataAtribuicao = c.DataAtribuicao
            });

            return Ok(result);
        }

        // GET: api/TicketsApi/MyTickets
        /// <summary>
        /// Obtém uma lista de tickets que estão atribuídos ao usuário logado (analista).
        /// </summary>
        /// <returns>Um IActionResult contendo a lista de tickets do usuário logado ou 401 Unauthorized.</returns>
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
                assignedToId = c.AtribuidoAId.HasValue ? c.AtribuidoAId.Value.ToString() : null,
                solicitante = c.Solicitante != null ? c.Solicitante.Username : "Desconhecido",
                dataAbertura = c.DataAbertura,
                dataAtribuicao = c.DataAtribuicao
            });

            return Ok(result);
        }

        // GET: api/TicketsApi/5
        /// <summary>
        /// Obtém os detalhes completos de um ticket específico pelo seu ID.
        /// </summary>
        /// <param name="id">O ID do Chamado.</param>
        /// <returns>Um IActionResult contendo os detalhes do ticket ou 404 Not Found.</returns>
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
                assignedToId = ticket.AtribuidoAId.HasValue ? ticket.AtribuidoAId.Value.ToString() : null,
                solicitante = ticket.Solicitante != null ? ticket.Solicitante.Username : "Desconhecido"
            };

            return Ok(result);
        }

        // POST: api/TicketsApi/approve/5
        /// <summary>
        /// Altera o status de um ticket para "Em Andamento" e o atribui ao usuário logado.
        /// </summary>
        /// <param name="id">O ID do Chamado a ser aprovado/assumido.</param>
        /// <returns>200 Ok em sucesso, 404 Not Found, ou 400 Bad Request se o status for inválido.</returns>
        [HttpPost("approve/{id}")]
        public async Task<IActionResult> Approve(int id)
        {
            var ticket = await _context.Chamados.FindAsync(id);
            if (ticket == null) return NotFound();

            if (ticket.Status != "Aberto")
                return BadRequest("Ticket já está em andamento ou concluído.");

            ticket.Status = "Em Andamento";
            ticket.AtribuidoAId = GetCurrentUserId(); // Atribui ao usuário logado
            ticket.DataAtribuicao = DateTime.Now;

            await _context.SaveChangesAsync();
            return Ok();
        }

        // POST: api/TicketsApi/conclude/5
        /// <summary>
        /// Altera o status de um ticket para "Concluído" e registra a data de fechamento.
        /// </summary>
        /// <param name="id">O ID do Chamado a ser concluído.</param>
        /// <returns>200 Ok em sucesso, 404 Not Found, ou 400 Bad Request se o status for inválido.</returns>
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
        /// <summary>
        /// Remove um ticket permanentemente do banco de dados.
        /// <para>Nota: A autorização deve garantir que apenas administradores ou o criador possam deletar.</para>
        /// </summary>
        /// <param name="id">O ID do Chamado a ser deletado.</param>
        /// <returns>204 No Content em sucesso, 404 Not Found.</returns>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTicket(int id)
        {
            var ticket = await _context.Chamados.FindAsync(id);
            if (ticket == null) return NotFound();

            _context.Chamados.Remove(ticket);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        /// <summary>
        /// Método utilitário privado para obter o ID do usuário logado a partir das Claims de autenticação.
        /// </summary>
        /// <returns>O ID do usuário (int) ou <c>null</c> se não estiver logado ou o ID for inválido.</returns>
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