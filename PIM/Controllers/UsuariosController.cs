using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PIM.Data;
using PIM.Models;

namespace PIM.Controllers
{
    public class UsuariosController : Controller
    {
        private readonly AppDbContext _db;
        private const int PageSize = 15;

        public UsuariosController(AppDbContext db)
        {
            _db = db;
        }

        // GET: Index
        public async Task<IActionResult> Index(string searchString, int pageNumber = 1)
        {
            ViewData["CurrentFilter"] = searchString;

            var users = _db.Admins
                .Where(a => a.Username != null && a.Email != null && a.Password != null);

            if (!string.IsNullOrEmpty(searchString))
            {
                users = users.Where(u => u.Username!.StartsWith(searchString));
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
        public IActionResult Create(Admin admin)
        {
            if (!ModelState.IsValid) return View(admin);

            _db.Admins.Add(admin);
            _db.SaveChanges();
            return RedirectToAction(nameof(Index));
        }

        // GET: Edit
        public IActionResult Edit(int id)
        {
            var admin = _db.Admins.Find(id);
            if (admin == null) return NotFound();
            return View(admin);
        }

        // POST: Edit
        [HttpPost]
        public IActionResult Edit(Admin admin)
        {
            if (!ModelState.IsValid) return View(admin);

            _db.Update(admin);
            _db.SaveChanges();
            return RedirectToAction(nameof(Index));
        }

        // GET: Delete
        public IActionResult Delete(int id)
        {
            var admin = _db.Admins.Find(id);
            if (admin == null) return NotFound();
            return View(admin);
        }

        // POST: Delete
        [HttpPost, ActionName("Delete")]
        public IActionResult DeleteConfirmed(int id)
        {
            var admin = _db.Admins.Find(id);
            if (admin == null) return NotFound();

            _db.Admins.Remove(admin);
            _db.SaveChanges();
            return RedirectToAction(nameof(Index));
        }

        // GET: Profile
        public IActionResult Profile(int id)
        {
            var admin = _db.Admins.Find(id);
            if (admin == null) return NotFound();
            return View(admin);
        }
    }
}
