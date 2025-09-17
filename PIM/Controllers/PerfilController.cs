using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PIM.Data;
using PIM.Models;
using System.Security.Claims;
using System.Threading.Tasks;

// IMPORTANTE: O código de senha não é seguro.
// É fornecido apenas para demonstração.

namespace PIM.Controllers
{
    public class PerfilController : Controller
    {
        private readonly AppDbContext _context;

        public PerfilController(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
            {
                return RedirectToAction("Login", "Account");
            }

            var admin = await _context.Admins.FirstOrDefaultAsync(a => a.Id == userId);

            if (admin == null)
            {
                return NotFound();
            }

            var totalTicketsAbertos = await _context.Chamados.CountAsync(c => c.Status == "Aberto" && c.AtribuidoA_AdminId == userId);
            var totalTicketsFechados = await _context.Chamados.CountAsync(c => (c.Status == "Concluído" || c.Status == "Rejeitado") && c.AtribuidoA_AdminId == userId);

            ViewBag.TotalTicketsAbertos = totalTicketsAbertos;
            ViewBag.TotalTicketsFechados = totalTicketsFechados;

            return View(admin);
        }

        public async Task<IActionResult> Editar()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
            {
                return RedirectToAction("Login", "Account");
            }

            var admin = await _context.Admins.FindAsync(userId);
            if (admin == null)
            {
                return NotFound();
            }

            return View(admin);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Editar([Bind("Id,Username,Email")] Admin admin)
        {
            if (admin == null || !ModelState.IsValid)
            {
                return View(admin);
            }
            
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId) || userId != admin.Id)
            {
                return Unauthorized();
            }

            var adminToUpdate = new Admin { Id = admin.Id, Username = admin.Username, Email = admin.Email };
            _context.Admins.Attach(adminToUpdate);
            _context.Entry(adminToUpdate).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Perfil atualizado com sucesso!";
                return RedirectToAction("Index");
            }
            catch (DbUpdateConcurrencyException)
            {
                ModelState.AddModelError("", "Não foi possível salvar as alterações. O registro foi alterado por outro usuário.");
                return View(admin);
            }
        }

        public IActionResult MudarSenha()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
            {
                return RedirectToAction("Login", "Account");
            }
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MudarSenha(string senhaAtual, string novaSenha, string confirmarNovaSenha)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
            {
                TempData["ErrorMessage"] = "Sessão expirada. Faça login novamente.";
                return RedirectToAction("Login", "Account");
            }

            var admin = await _context.Admins.FindAsync(userId);
            if (admin == null)
            {
                TempData["ErrorMessage"] = "Usuário não encontrado.";
                return RedirectToAction("Index");
            }

            if (senhaAtual != admin.Password)
            {
                TempData["ErrorMessage"] = "A senha atual está incorreta.";
                return RedirectToAction("MudarSenha");
            }

            if (novaSenha != confirmarNovaSenha)
            {
                TempData["ErrorMessage"] = "A nova senha e a confirmação não coincidem.";
                return RedirectToAction("MudarSenha");
            }

            admin.Password = novaSenha;
            
            try
            {
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Senha alterada com sucesso!";
                return RedirectToAction("Index");
            }
            catch (DbUpdateConcurrencyException)
            {
                TempData["ErrorMessage"] = "Ocorreu um erro ao salvar a senha. Tente novamente.";
                return RedirectToAction("MudarSenha");
            }
        }
    }
}