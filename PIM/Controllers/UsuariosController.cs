using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PIM.Data;
using PIM.Models;

namespace PIM.Controllers
{
    public class UsuariosController : Controller
    {
        private readonly AppDbContext _db;
        private const int PageSize = 10; // Número de itens por página

        public UsuariosController(AppDbContext db)
        {
            _db = db;
        }

        // GET: Index
        public async Task<IActionResult> Index(string searchString, int pageNumber = 1)
        {
            ViewData["CurrentFilter"] = searchString;

            var users = _db.Usuarios.AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                users = users.Where(u => u.Username != null && u.Username.StartsWith(searchString));
            }

            users = users.OrderBy(u => u.Id);

            var totalUsers = await users.CountAsync();

            var pagedUsers = await users
                .Skip((pageNumber - 1) * PageSize)
                .Take(PageSize)
                .ToListAsync();

            ViewData["TotalPages"] = (int)Math.Ceiling(totalUsers / (double)PageSize);
            ViewData["CurrentPage"] = pageNumber;

            return View(pagedUsers);
        }

        // GET: Create
        public IActionResult Create() => View();

        // POST: Create
        [HttpPost]
        public IActionResult Create(Usuario usuario)
        {
            if (!ModelState.IsValid) return View(usuario);

            usuario.CreatedAt = DateTime.Now; // Definindo data de criação
            _db.Usuarios.Add(usuario);
            _db.SaveChanges();

            return RedirectToAction(nameof(Index));
        }

        // GET: Edit
        public IActionResult Edit(int id)
        {
            var usuario = _db.Usuarios.Find(id);
            if (usuario == null) return NotFound();
            return View(usuario);
        }

        // POST: Edit
        [HttpPost]
        public IActionResult Edit(Usuario usuario)
        {
            if (!ModelState.IsValid) return View(usuario);

            var userDb = _db.Usuarios.Find(usuario.Id);
            if (userDb == null) return NotFound();

            // Atualiza apenas os campos normais
            userDb.Username = usuario.Username;
            userDb.Email = usuario.Email;
            userDb.Role = usuario.Role;
            userDb.Status = usuario.Status;
            userDb.Telefone = usuario.Telefone;
            userDb.Endereco = usuario.Endereco;
            userDb.DataNascimento = usuario.DataNascimento;
            userDb.Observacoes = usuario.Observacoes;

            // Atualiza a senha somente se o usuário digitou algo
            if (!string.IsNullOrEmpty(usuario.SenhaHash))
            {
                userDb.SenhaHash = usuario.SenhaHash; // aqui você pode aplicar hash
            }

            _db.SaveChanges();

            return RedirectToAction(nameof(Index));
        }


        // GET: Delete
        public IActionResult Delete(int id)
        {
            var usuario = _db.Usuarios.Find(id);
            if (usuario == null) return NotFound();
            return View(usuario);
        }

        // POST: Delete
        [HttpPost, ActionName("Delete")]
        public IActionResult DeleteConfirmed(int id)
        {
            var usuario = _db.Usuarios.Find(id);
            if (usuario == null) return NotFound();

            _db.Usuarios.Remove(usuario);
            _db.SaveChanges();
            return RedirectToAction(nameof(Index));
        }

        // GET: Profile
        public IActionResult Profile(int id)
        {
            var usuario = _db.Usuarios.Find(id);
            if (usuario == null) return NotFound();
            return View(usuario);
        }
    }
}
