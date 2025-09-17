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
            var admins = _db.Admins.ToList();
            return View(admins);
        }

        // GET: Admin/Details/5
        public IActionResult Details(int id)
        {
            var admin = _db.Admins.Find(id);
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
        public IActionResult Create(Admin admin)
        {
            if (!ModelState.IsValid) return View(admin);

            _db.Admins.Add(admin);
            _db.SaveChanges();
            return RedirectToAction(nameof(Index));
        }

        // GET: Admin/Edit/5
        public IActionResult Edit(int id)
        {
            var admin = _db.Admins.Find(id);
            if (admin == null) return NotFound();
            return View(admin);
        }

        // POST: Admin/Edit/5
        [HttpPost]
        public IActionResult Edit(Admin admin)
        {
            if (!ModelState.IsValid) return View(admin);

            _db.Update(admin);
            _db.SaveChanges();
            return RedirectToAction(nameof(Index));
        }

        // GET: Admin/Delete/5
        public IActionResult Delete(int id)
        {
            var admin = _db.Admins.Find(id);
            if (admin == null) return NotFound();
            return View(admin);
        }

        // POST: Admin/Delete/5
        [HttpPost, ActionName("Delete")]
        public IActionResult DeleteConfirmed(int id)
        {
            var admin = _db.Admins.Find(id);
            if (admin == null) return NotFound();

            _db.Admins.Remove(admin);
            _db.SaveChanges();
            return RedirectToAction(nameof(Index));
        }
    }
}
