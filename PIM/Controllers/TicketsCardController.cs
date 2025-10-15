using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PIM.Data;
using PIM.Models;
using PIM.ViewModels;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System; // Adicionado para System.DateTime.Now

namespace PIM.Controllers
{
    /// <summary>
    /// Controlador da API otimizado para a exibição de tickets em formato de cartões (cards) ou listas resumidas no painel de controle.
    /// <para>Inclui métodos para obter listas de tickets (todos e atribuídos ao usuário), bem como ações rápidas como Aprovar, Concluir e Deletar.</para>
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class TicketsCardController : ControllerBase
    {
        private readonly AppDbContext _context;

        /// <summary>
        /// Inicializa uma nova instância do controlador TicketsCardController.
        /// </summary>
        /// <param name="context">O contexto do banco de dados (AppDbContext) injetado via DI.</param>
        public TicketsCardController(AppDbContext context)
        {
            _context = context;
        }

        // --- Método Auxiliar para Obter o ID do Usuário Logado ---
        /// <summary>
        /// Tenta obter e analisar o ID do usuário logado a partir das Claims de autenticação.
        /// </summary>
        /// <param name="userId">Variável de saída que receberá o ID do usuário se a operação for bem-sucedida.</param>
        /// <returns><c>true</c> se o ID do usuário foi encontrado e é válido; caso contrário, <c>false</c>.</returns>
        private bool TryGetUserId(out int userId)
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return int.TryParse(userIdStr, out userId);
        }

        // --- Método para retornar todos os tickets (Endpoint: /api/ticketscard/GetTickets) ---
        /// <summary>
        /// Obtém uma lista de todos os tickets no sistema, permitindo filtragem por datas e status.
        /// </summary>
        /// <param name="filters">Objeto contendo os parâmetros de filtro (data inicial, status, etc.).</param>
        /// <returns>Uma lista de tickets resumidos (DTO) que correspondem aos filtros aplicados.</returns>
        [HttpGet("GetTickets")]
        public async Task<IActionResult> GetTickets(TicketFilterViewModel filters)
        {
            var query = _context.Chamados
                                .Include(c => c.AtribuidoA)
                                .Include(c => c.Solicitante) 
                                .AsQueryable();

            // Lógica de filtros
            if (filters.StartDate.HasValue)
                query = query.Where(t => t.DataAbertura >= filters.StartDate.Value);
            // ... (restante dos filtros) ...
            if (!string.IsNullOrEmpty(filters.Status))
                query = query.Where(t => t.Status == filters.Status);

            var tickets = await query
                .Select(t => new
                {
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
        /// <summary>
        /// Obtém uma lista de tickets que estão atribuídos ao analista (usuário) logado.
        /// </summary>
        /// <returns>Uma lista de tickets atribuídos ao usuário logado ou 401 Unauthorized.</returns>
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
        /// <summary>
        /// Altera o status de um ticket para "Em Andamento", atribui-o ao usuário logado e registra a data de atribuição.
        /// </summary>
        /// <param name="id">O ID do Chamado a ser aprovado/assumido.</param>
        /// <returns>200 Ok em sucesso, 401 Unauthorized, 404 Not Found, ou 400 Bad Request se o status ou atribuição for inválido.</returns>
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
        /// <summary>
        /// Remove um ticket permanentemente do banco de dados.
        /// <para>Requer autorização adequada, geralmente para Administradores ou o criador original.</para>
        /// </summary>
        /// <param name="id">O ID do Chamado a ser deletado.</param>
        /// <returns>200 Ok em sucesso, 404 Not Found.</returns>
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
        /// <summary>
        /// Obtém os detalhes completos de um ticket específico pelo seu ID.
        /// </summary>
        /// <param name="id">O ID do Chamado.</param>
        /// <returns>Um IActionResult contendo os detalhes do ticket (DTO) ou 404 Not Found.</returns>
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
        /// <summary>
        /// Altera o status de um ticket para "Concluído" e registra a data de fechamento.
        /// <para>Esta ação só é permitida se o ticket estiver "Em Andamento" e atribuído ao usuário logado.</para>
        /// </summary>
        /// <param name="id">O ID do Chamado a ser concluído.</param>
        /// <returns>200 Ok em sucesso, 401 Unauthorized, 404 Not Found, ou 400 Bad Request se o status ou atribuição for inválido.</returns>
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