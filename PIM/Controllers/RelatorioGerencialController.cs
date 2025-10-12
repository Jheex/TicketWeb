using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PIM.Data;
using System.Linq;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;

namespace PIM.Controllers
{
    public class RelatorioGerencialController : Controller
    {
        private readonly AppDbContext _context;

        public RelatorioGerencialController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Relatorio()
        {
            return View();
        }

        // ------------------------------------------------------------------
        // ENDPOINT PARA DADOS AGREGADOS (GRÁFICOS)
        // ------------------------------------------------------------------

        [HttpGet]
        public async Task<IActionResult> ObterDados(string periodo = "30d", string tecnico = "todos")
        {
            // Nota: Adicionar lógica de filtragem de 'periodo' e 'tecnico' aqui, se necessário.
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
                // CORREÇÃO CS8602: Usando o operador ?? para garantir que a chave de agrupamento não seja nula
                .GroupBy(c => c.AtribuidoA.Username ?? "Não Atribuído") 
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

        // ------------------------------------------------------------------
        // NOVO ENDPOINT PARA EXPORTAÇÃO COMPLETA DE DADOS BRUTOS (CSV)
        // ------------------------------------------------------------------
        
        [HttpGet]
        public async Task<IActionResult> ExportarChamados(string periodo = "30d", string tecnico = "todos")
        {
            // PASSO 1: Inclui o Solicitante (o 'Cliente' ou usuário que abriu o chamado)
            var chamadosBrutos = await _context.Chamados
                .Include(c => c.AtribuidoA) 
                .Include(c => c.Solicitante) // CORRIGIDO: Usa 'Solicitante' em vez de 'Cliente'
                .ToListAsync();

            if (chamadosBrutos == null || !chamadosBrutos.Any())
            {
                return Ok(new List<object>()); 
            }

            // PASSO 2: Selecionar os dados exatos que você quer no CSV.
            var dadosExportacao = chamadosBrutos.Select(c => new
            {
                c.ChamadoId, // Alterado para ChamadoId conforme modelo
                c.Titulo,    // Adicionado Título
                DataAbertura = c.DataAbertura.ToString("dd/MM/yyyy HH:mm"), 
                c.Categoria,
                c.Status,
                // CORREÇÃO CS1061: Usa 'Solicitante' em vez de 'Cliente'
                ClienteSolicitante = c.Solicitante?.Username, 
                AtribuidoA = c.AtribuidoA?.Username,
                c.Prioridade, 
                c.Descricao // Adicionado Descrição para o relatório completo
            }).ToList();

            return Ok(dadosExportacao);
        }
    }
}