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

        // --- Método Auxiliar para Obter o ID do Usuário Logado ---
        private bool TryGetUserId(out int userId)
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return int.TryParse(userIdStr, out userId);
        }

        // --- Método para retornar todos os tickets (Endpoint: /api/ticketscard/GetTickets) ---
        [HttpGet("GetTickets")]
        public async Task<IActionResult> GetTickets(TicketFilterViewModel filters)
        {
            var query = _context.Chamados
                                .Include(c => c.AtribuidoA)
                                .Include(c => c.Solicitante) 
                                .AsQueryable();

            // Lógica de filtros (mantida)
            if (filters.StartDate.HasValue)
                query = query.Where(t => t.DataAbertura >= filters.StartDate.Value);
            // ... (restante dos filtros) ...
            if (!string.IsNullOrEmpty(filters.Status))
                query = query.Where(t => t.Status == filters.Status);

            var tickets = await query
                .Select(t => new
                {
                    // CORRIGIDO: Retornando como 'id' para evitar problemas de CamelCase/PascalCase no JS
                    id = t.ChamadoId,
                    title = t.Titulo,
                    category = t.Categoria,
                    priority = t.Prioridade,
                    status = t.Status,
                    dataAbertura = t.DataAbertura,
                    dataAtribuicao = t.DataAtribuicao, // Adicionado para o frontend
                    assignedTo = t.AtribuidoA != null ? t.AtribuidoA.Username : null,
                    assignedToId = t.AtribuidoAId.HasValue ? t.AtribuidoAId.Value.ToString() : null,
                    solicitante = t.Solicitante != null ? t.Solicitante.Username : "Desconhecido"
                })
                .ToListAsync();

            return Ok(tickets);
        }
        
        // --- NOVO MÉTODO: Retorna tickets atribuídos ao usuário logado (Endpoint: /api/ticketscard/MyTickets) ---
        [HttpGet("MyTickets")]
        public async Task<IActionResult> GetMyTickets()
        {
            if (!TryGetUserId(out int usuarioId))
                return Unauthorized();

            var tickets = await _context.Chamados
                                .Include(c => c.AtribuidoA)
                                .Include(c => c.Solicitante) 
                                // FILTRO PRINCIPAL: O ticket está atribuído ao usuário logado
                                .Where(t => t.AtribuidoAId == usuarioId) 
                                .Select(t => new
                                {
                                    // CORRIGIDO: Retornando como 'id'
                                    id = t.ChamadoId,
                                    title = t.Titulo,
                                    category = t.Categoria,
                                    priority = t.Prioridade,
                                    status = t.Status,
                                    dataAbertura = t.DataAbertura,
                                    dataAtribuicao = t.DataAtribuicao, 
                                    assignedTo = t.AtribuidoA != null ? t.AtribuidoA.Username : null,
                                    assignedToId = t.AtribuidoAId.HasValue ? t.AtribuidoAId.Value.ToString() : null,
                                    solicitante = t.Solicitante != null ? t.Solicitante.Username : "Desconhecido"
                                })
                                .ToListAsync();

            return Ok(tickets);
        }

        // --- Aprova e atribui o ticket ao usuário logado (Endpoint: /api/ticketscard/Aprovar/{id}) ---
        [HttpPost("Aprovar/{id}")]
        public async Task<IActionResult> Aprovar(int id)
        {
            if (!TryGetUserId(out int usuarioId))
                return Unauthorized("Usuário não autenticado ou ID inválido.");

            var chamado = await _context.Chamados.FindAsync(id);
            if (chamado == null)
                return NotFound($"Ticket ID {id} não encontrado.");

            if (chamado.Status != "Aberto")
                return BadRequest("O ticket não pode ser aprovado, pois não está 'Aberto'.");
            
            if (chamado.AtribuidoAId.HasValue)
                 return BadRequest("O ticket já foi atribuído a outro analista.");

            chamado.Status = "Em Andamento";
            chamado.AtribuidoAId = usuarioId; // Atribui ao usuário logado
            chamado.DataAtribuicao = System.DateTime.Now;

            await _context.SaveChangesAsync();

            return Ok();
        }

        // --- Deletar ticket (Endpoint: /api/ticketscard/{id}) ---
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

        // --- Pega ticket específico (Endpoint: /api/ticketscard/{id}) ---
        [HttpGet("{id}")]
        public async Task<IActionResult> GetTicket(int id)
        {
            var ticket = await _context.Chamados
                                       .Include(c => c.AtribuidoA)
                                       .Include(c => c.Solicitante)
                                       .FirstOrDefaultAsync(c => c.ChamadoId == id);

            if (ticket == null)
                return NotFound();

            return Ok(new
            {
                // CORRIGIDO: Retornando como 'id'
                id = ticket.ChamadoId, 
                title = ticket.Titulo,
                category = ticket.Categoria,
                priority = ticket.Prioridade,
                status = ticket.Status,
                dataAbertura = ticket.DataAbertura,
                dataFechamento = ticket.DataFechamento,
                dataAtribuicao = ticket.DataAtribuicao,
                assignedTo = ticket.AtribuidoA != null ? ticket.AtribuidoA.Username : null,
                assignedToId = ticket.AtribuidoAId.HasValue ? ticket.AtribuidoAId.Value.ToString() : null, 
                solicitante = ticket.Solicitante != null ? ticket.Solicitante.Username : "Desconhecido",
                description = ticket.Descricao // Assumindo que a descrição é necessária no modal de detalhes
            });
        }
        
        // --- NOVO MÉTODO: Concluir ticket (Endpoint: /api/ticketscard/Conclude/{id}) ---
        [HttpPost("Conclude/{id}")]
        public async Task<IActionResult> Conclude(int id)
        {
            if (!TryGetUserId(out int usuarioId))
                return Unauthorized("Usuário não autenticado ou ID inválido.");

            var chamado = await _context.Chamados.FindAsync(id);
            if (chamado == null)
                return NotFound($"Ticket ID {id} não encontrado.");

            // Verifica se o ticket está em andamento e atribuído a este usuário
            if (chamado.Status != "Em Andamento" || chamado.AtribuidoAId != usuarioId)
                return BadRequest("O ticket não pode ser concluído. Verifique o status e a atribuição.");
            
            chamado.Status = "Concluído";
            chamado.DataFechamento = System.DateTime.Now;

            await _context.SaveChangesAsync();

            return Ok();
        }
    }
}