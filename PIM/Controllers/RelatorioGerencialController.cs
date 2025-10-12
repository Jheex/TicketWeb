using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PIM.Data;
using System.Linq;
using System.Threading.Tasks;

namespace PIM.Controllers
{
    public class RelatorioGerencialController : Controller
    {
        private readonly AppDbContext _context;

        public RelatorioGerencialController(AppDbContext context)
        {
            _context = context;
        }

        // Página principal do relatório
        [HttpGet]
        public IActionResult Relatorio()
        {
            return View();
        }

        // Retorna os dados do relatório em JSON
        [HttpGet]
        public async Task<IActionResult> ObterDados()
        {
            var chamados = await _context.Chamados.Include(c => c.AtribuidoA).ToListAsync();

            var totalChamados = chamados.Count;
            var abertos = chamados.Count(c => c.Status != null && c.Status.Trim().ToLower() == "aberto");
            var andamento = chamados.Count(c => c.Status != null && 
                (c.Status.Trim().ToLower() == "em andamento" || 
                 c.Status.Trim().ToLower() == "andamento" || 
                 c.Status.Trim().ToLower() == "em atendimento" || 
                 c.Status.Trim().ToLower() == "em progresso"));
            var finalizados = chamados.Count(c => c.Status != null &&
                (c.Status.Trim().ToLower() == "fechado" || 
                 c.Status.Trim().ToLower() == "concluído" || 
                 c.Status.Trim().ToLower() == "concluido"));

            double taxaConclusao = totalChamados > 0
                ? Math.Round((double)finalizados / totalChamados * 100, 1)
                : 0;

            // Agrupa por técnico
            var tecnicos = chamados
                .Where(c => c.AtribuidoA != null)
                .GroupBy(c => c.AtribuidoA.Username)
                .Select(g => new
                {
                    Tecnico = g.Key,
                    Abertos = g.Count(c => c.Status != null && c.Status.Trim().ToLower() == "aberto"),
                    Andamento = g.Count(c => c.Status != null && 
                        (c.Status.Trim().ToLower() == "em andamento" ||
                         c.Status.Trim().ToLower() == "andamento" ||
                         c.Status.Trim().ToLower() == "em atendimento" ||
                         c.Status.Trim().ToLower() == "em progresso")),
                    Finalizados = g.Count(c => c.Status != null &&
                        (c.Status.Trim().ToLower() == "fechado" ||
                         c.Status.Trim().ToLower() == "concluído" ||
                         c.Status.Trim().ToLower() == "concluido"))
                })
                .ToList();

            // Agrupa por categoria
            var categorias = chamados
                .GroupBy(c => c.Categoria ?? "Outros")
                .Select(g => new
                {
                    Categoria = g.Key,
                    Total = g.Count()
                })
                .ToList();

            return Json(new
            {
                abertos,
                andamento,
                finalizados,
                taxaConclusao,
                tecnicos,
                categorias
            });
        }
    }
}
