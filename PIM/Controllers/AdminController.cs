using Microsoft.AspNetCore.Mvc;
using PIM.Data;
using PIM.Models;

namespace PIM.Controllers
{
    public class AdminController : Controller
    {
        private readonly AppDbContext _db;

        public AdminController(AppDbContext db)
        {
            _db = db;
        }

        // GET: Admin
        public IActionResult Index()
        {
            // Apenas usuários com Role "Admin"
            var admins = _db.Usuarios.Where(u => u.Role == "Admin").ToList();
            return View(admins);
        }

        // GET: Admin/Details/5
        public IActionResult Details(int id)
        {
            var admin = _db.Usuarios.Find(id);
            if (admin == null) return NotFound();
            return View(admin);
        }

        // GET: Admin/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Admin/Create
        [HttpPost]
        public IActionResult Create(Usuario usuario)
        {
            if (!ModelState.IsValid) return View(usuario);

            // Define o Role como Admin
            usuario.Role = "Admin";

            _db.Usuarios.Add(usuario);
            _db.SaveChanges();
            return RedirectToAction(nameof(Index));
        }

        // GET: Admin/Edit/5
        public IActionResult Edit(int id)
        {
            var admin = _db.Usuarios.Find(id);
            if (admin == null) return NotFound();
            return View(admin);
        }

        // POST: Admin/Edit/5
        [HttpPost]
        public IActionResult Edit(Usuario usuario)
        {
            if (!ModelState.IsValid) return View(usuario);

            _db.Update(usuario);
            _db.SaveChanges();
            return RedirectToAction(nameof(Index));
        }

        // GET: Admin/Delete/5
        public IActionResult Delete(int id)
        {
            var admin = _db.Usuarios.Find(id);
            if (admin == null) return NotFound();
            return View(admin);
        }

        // POST: Admin/Delete/5
        [HttpPost, ActionName("Delete")]
        public IActionResult DeleteConfirmed(int id)
        {
            var admin = _db.Usuarios.Find(id);
            if (admin == null) return NotFound();

            _db.Usuarios.Remove(admin);
            _db.SaveChanges();
            return RedirectToAction(nameof(Index));
        }
    }
}
