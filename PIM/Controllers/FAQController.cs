using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Rendering;
using PIM.Data;
using PIM.Models;
using PIM.ViewModels;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace PIM.Controllers
{
    /// <summary>
    /// Controlador responsável por gerenciar todas as operações de CRUD (Criação, Leitura, Atualização, Exclusão)
    /// das Perguntas Frequentes (FAQ) no sistema.
    /// Inclui lógica de paginação, busca e manipulação de categorias.
    /// </summary>
    public class FAQController : Controller
    {
        private readonly AppDbContext _context;

        /// <summary>
        /// Inicializa uma nova instância do controlador FAQController.
        /// </summary>
        /// <param name="context">O contexto do banco de dados (AppDbContext) injetado via DI.</param>
        public FAQController(AppDbContext context)
        {
            _context = context;
        }

        // ============================================================
        // GET: FAQ (Com Paginação e Filtro de Busca)
        // ============================================================
        /// <summary>
        /// Exibe a lista paginada de Perguntas Frequentes, permitindo filtragem por string de busca.
        /// Ordena as FAQs pelo campo 'Ordem'.
        /// </summary>
        /// <param name="searchString">String de busca opcional para filtrar Pergunta, Resposta e Categoria.</param>
        /// <param name="pageNumber">O número da página atual a ser exibida (padrão é 1).</param>
        /// <returns>A View Index com o <see cref="FaqListViewModel"/> contendo os resultados paginados.</returns>
        public async Task<IActionResult> Index(string searchString, int pageNumber = 1)
        {
            int pageSize = 10;

            var query = _context.Faqs.OrderBy(f => f.Ordem).AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                query = query.Where(f =>
                    f.Pergunta.Contains(searchString) ||
                    f.Resposta.Contains(searchString) ||
                    f.Categoria.Contains(searchString));
            }

            int totalItems = await query.CountAsync();

            var faqsPaginados = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var viewModel = new FaqListViewModel
            {
                Faqs = faqsPaginados,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalItems = totalItems,
                CurrentFilter = searchString
            };

            ViewData["CurrentFilter"] = searchString;

            return View(viewModel);
        }

        // ============================================================
        // GET: Criar FAQ
        // ============================================================
        /// <summary>
        /// Exibe o formulário para criação de uma nova Pergunta Frequente.
        /// Pré-preenche o campo 'Ordem' com o próximo número sequencial e carrega as categorias existentes.
        /// </summary>
        /// <returns>A View Create, pré-configurada com a próxima ordem e lista de categorias.</returns>
        public IActionResult Create()
        {
            var maxOrdem = _context.Faqs.Max(f => (int?)f.Ordem) ?? 0;
            var faq = new Faq { Ordem = maxOrdem + 1 };

            var categorias = _context.Faqs
                .Where(f => !string.IsNullOrEmpty(f.Categoria))
                .Select(f => f.Categoria)
                .Distinct()
                .Where(c => c != "Outros")
                .OrderBy(c => c)
                .ToList();

            categorias.Add("Outros");
            ViewData["CategoriasExistentes"] = new SelectList(categorias);

            return View(faq);
        }

        // ============================================================
        // POST: Criar FAQ
        // ============================================================
        /// <summary>
        /// Processa o envio do formulário de criação, valida o modelo e salva a nova FAQ no banco de dados.
        /// A <see cref="Faq.DataAtualizacao"/> é definida como a hora atual.
        /// </summary>
        /// <param name="faq">O objeto Faq contendo os dados enviados pelo formulário.</param>
        /// <returns>Redireciona para Index se for bem-sucedido; caso contrário, retorna a View Create com erros e recarrega categorias.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Faq faq)
        {
            if (ModelState.IsValid)
            {
                faq.DataAtualizacao = DateTime.Now; // Corrige erro de NULL

                _context.Faqs.Add(faq);
                _context.SaveChanges();

                return RedirectToAction("Index");
            }

            // Recarrega as categorias em caso de erro de validação
            var categorias = _context.Faqs
                .Where(f => !string.IsNullOrEmpty(f.Categoria))
                .Select(f => f.Categoria)
                .Distinct()
                .Where(c => c != "Outros")
                .OrderBy(c => c)
                .ToList();

            categorias.Add("Outros");
            ViewData["CategoriasExistentes"] = new SelectList(categorias);

            return View(faq);
        }

        // ============================================================
        // GET: Editar FAQ
        // ============================================================
        /// <summary>
        /// Exibe o formulário para edição de uma FAQ existente.
        /// </summary>
        /// <param name="id">O ID da FAQ a ser editada.</param>
        /// <returns>A View Edit com o objeto Faq preenchido, ou NotFound se o ID for inválido.</returns>
        public IActionResult Edit(int id)
        {
            var faq = _context.Faqs.Find(id);
            if (faq == null)
                return NotFound();

            var categorias = _context.Faqs
                .Where(f => !string.IsNullOrEmpty(f.Categoria))
                .Select(f => f.Categoria)
                .Distinct()
                .Where(c => c != "Outros")
                .OrderBy(c => c)
                .ToList();

            categorias.Add("Outros");
            ViewData["CategoriasExistentes"] = new SelectList(categorias, faq.Categoria);

            return View(faq);
        }

        // ============================================================
        // POST: Editar FAQ
        // ============================================================
        /// <summary>
        /// Processa o envio do formulário de edição, atualiza a <see cref="Faq.DataAtualizacao"/> e salva as alterações no banco.
        /// </summary>
        /// <param name="faq">O objeto Faq contendo os dados atualizados.</param>
        /// <returns>Redireciona para Index se for bem-sucedido; caso contrário, retorna a View Edit com erros e recarrega categorias.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(Faq faq)
        {
            if (ModelState.IsValid)
            {
                faq.DataAtualizacao = DateTime.Now;

                _context.Faqs.Update(faq);
                _context.SaveChanges();

                return RedirectToAction("Index");
            }

            // Recarrega as categorias em caso de erro de validação
            var categorias = _context.Faqs
                .Where(f => !string.IsNullOrEmpty(f.Categoria))
                .Select(f => f.Categoria)
                .Distinct()
                .Where(c => c != "Outros")
                .OrderBy(c => c)
                .ToList();

            categorias.Add("Outros");
            ViewData["CategoriasExistentes"] = new SelectList(categorias, faq.Categoria);

            return View(faq);
        }

        // ============================================================
        // DELETE: Remover FAQ
        // ============================================================
        /// <summary>
        /// Remove uma Pergunta Frequente do banco de dados com base no ID fornecido.
        /// </summary>
        /// <param name="id">O ID da FAQ a ser excluída.</param>
        /// <returns>Redireciona o usuário para a lista Index após a exclusão (ou falha).</returns>
        public IActionResult Delete(int id)
        {
            var faq = _context.Faqs.Find(id);
            if (faq != null)
            {
                _context.Faqs.Remove(faq);
                _context.SaveChanges();
            }

            return RedirectToAction("Index");
        }
    }
}