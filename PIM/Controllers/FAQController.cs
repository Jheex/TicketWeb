using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PIM.Data;
using PIM.Models;
using PIM.ViewModels; // Garanta que seu namespace esteja correto aqui
using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace PIM.Controllers
{
    public class FAQController : Controller
    {
        private readonly AppDbContext _context;

        public FAQController(AppDbContext context)
        {
            _context = context;
        }

        // GET: FAQ (Com Paginação e Filtro de Busca)
        public async Task<IActionResult> Index(string searchString, int pageNumber = 1)
        {
            // Limite de 10 FAQs por página
            int pageSize = 10;
            
            // Query base (ordenada por Ordem)
            var query = _context.Faqs.OrderBy(f => f.Ordem).AsQueryable();

            // Lógica de Filtro por Pesquisa
            if (!string.IsNullOrEmpty(searchString))
            {
                // Filtra Pergunta OU Resposta OU Categoria
                query = query.Where(f => f.Pergunta.Contains(searchString) ||
                                         f.Resposta.Contains(searchString) ||
                                         f.Categoria.Contains(searchString));
            }

            // Conta o total de itens APÓS a aplicação do filtro
            int totalItems = await query.CountAsync();

            // Aplica a Paginação (Skip e Take)
            var faqsPaginados = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // Monta o ViewModel para enviar todos os dados à View
            var viewModel = new FaqListViewModel
            {
                Faqs = faqsPaginados,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalItems = totalItems,
                CurrentFilter = searchString // Passa o filtro atual para manter na navegação
            };

            // Armazena o filtro na ViewData para a caixa de texto
            ViewData["CurrentFilter"] = searchString;

            return View(viewModel);
        }
        
        // GET: Criar FAQ
        public IActionResult Create()
        {
            var maxOrdem = _context.Faqs.Max(f => (int?)f.Ordem) ?? 0;
            var faq = new Faq { Ordem = maxOrdem + 1 };
            
            var categorias = _context.Faqs
                                     .Where(f => !string.IsNullOrEmpty(f.Categoria)) 
                                     .Select(f => f.Categoria)
                                     .Distinct()
                                     .Where(c => c != "Outros") // Remove Outros antes de ordenar
                                     .OrderBy(c => c)
                                     .ToList();
            
            categorias.Add("Outros"); // Adiciona OUTROS por último

            ViewData["CategoriasExistentes"] = new SelectList(categorias);

            return View(faq);
        }

        // POST: Criar FAQ
        [HttpPost]
        public IActionResult Create(Faq faq)
        {
            if (ModelState.IsValid)
            {
                _context.Faqs.Add(faq);
                _context.SaveChanges();
                return RedirectToAction("Index");
            }
            
            // Recarrega categorias em caso de erro de validação
            var categorias = _context.Faqs.Where(f => !string.IsNullOrEmpty(f.Categoria)).Select(f => f.Categoria).Distinct().Where(c => c != "OUTROS").OrderBy(c => c).ToList();
            categorias.Add("OUTROS");
            ViewData["CategoriasExistentes"] = new SelectList(categorias);
            
            return View(faq);
        }

        // GET: Editar FAQ
        public IActionResult Edit(int id)
        {
            var faq = _context.Faqs.Find(id);
            if (faq == null) return NotFound();
            
            var categorias = _context.Faqs
                                     .Where(f => !string.IsNullOrEmpty(f.Categoria))
                                     .Select(f => f.Categoria)
                                     .Distinct()
                                     .Where(c => c != "OUTROS")
                                     .OrderBy(c => c)
                                     .ToList();
            
            categorias.Add("OUTROS");
            ViewData["CategoriasExistentes"] = new SelectList(categorias, faq.Categoria);

            return View(faq);
        }

        // POST: Editar FAQ
        [HttpPost]
        public IActionResult Edit(Faq faq)
        {
            if (ModelState.IsValid)
            {
                faq.DataAtualizacao = DateTime.Now;
                _context.Faqs.Update(faq);
                _context.SaveChanges();
                return RedirectToAction("Index");
            }
            
            // Recarrega categorias em caso de erro de validação
            var categorias = _context.Faqs.Where(f => !string.IsNullOrEmpty(f.Categoria)).Select(f => f.Categoria).Distinct().Where(c => c != "OUTROS").OrderBy(c => c).ToList();
            categorias.Add("OUTROS");
            ViewData["CategoriasExistentes"] = new SelectList(categorias, faq.Categoria);

            return View(faq);
        }

        // Remover FAQ
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