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
    public class FAQController : Controller
    {
        private readonly AppDbContext _context;

        public FAQController(AppDbContext context)
        {
            _context = context;
        }

        // ============================================================
        // GET: FAQ (Com Paginação e Filtro de Busca)
        // ============================================================
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
