using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PIM.Data;
using PIM.Models;
using System.Security.Claims;
using System.Threading.Tasks;

namespace PIM.Controllers
{
    public class PerfilController : Controller
    {
        private readonly AppDbContext _context;

        public PerfilController(AppDbContext context)
        {
            _context = context;
        }

        // Método utilitário para obter o usuário logado
        private async Task<Usuario?> GetCurrentUserAsync()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
            {
                return null;
            }
            return await _context.Usuarios.FirstOrDefaultAsync(u => u.Id == userId);
        }

        // Action para visualização do perfil
        public async Task<IActionResult> Index()
        {
            var usuario = await GetCurrentUserAsync();
            if (usuario == null)
            {
                return RedirectToAction("Login", "Account");
            }

            // Contagem de tickets do usuário
            var totalTicketsAbertos = await _context.Chamados.CountAsync(c => c.Status == "Aberto" && c.AtribuidoAId == usuario.Id);
            var totalTicketsFechados = await _context.Chamados.CountAsync(c => (c.Status == "Concluído" || c.Status == "Rejeitado") && c.AtribuidoAId == usuario.Id);

            ViewBag.TotalTicketsAbertos = totalTicketsAbertos;
            ViewBag.TotalTicketsFechados = totalTicketsFechados;

            return View(usuario);
        }

        // Action para a visualização de edição do perfil
        public async Task<IActionResult> Editar()
        {
            var usuario = await GetCurrentUserAsync();
            if (usuario == null)
            {
                return RedirectToAction("Login", "Account");
            }
            return View(usuario);
        }

        // POST para edição do perfil
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Editar([Bind("Id,Username,Email")] Usuario usuario)
        {
            var currentUser = await GetCurrentUserAsync();
            if (currentUser == null || currentUser.Id != usuario.Id)
            {
                return Unauthorized();
            }

            currentUser.Username = usuario.Username;
            currentUser.Email = usuario.Email;

            try
            {
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Perfil atualizado com sucesso!";
            }
            catch (DbUpdateConcurrencyException)
            {
                TempData["ErrorMessage"] = "Não foi possível salvar as alterações. O registro pode ter sido alterado por outro usuário.";
            }

            return RedirectToAction("Index");
        }

        // Action para a visualização de mudar senha
        public async Task<IActionResult> MudarSenha()
        {
            var usuario = await GetCurrentUserAsync();
            if (usuario == null)
            {
                return RedirectToAction("Login", "Account");
            }
            return View();
        }

        // POST para a mudança de senha
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MudarSenha(string senhaAtual, string novaSenha, string confirmarNovaSenha)
        {
            var usuario = await GetCurrentUserAsync();
            if (usuario == null)
            {
                TempData["ErrorMessage"] = "Sessão expirada. Faça login novamente.";
                return RedirectToAction("Login", "Account");
            }

            // TODO: Implementar hashing seguro da senha
            if (senhaAtual != usuario.SenhaHash)
            {
                TempData["ErrorMessage"] = "A senha atual está incorreta.";
                return View();
            }

            if (string.IsNullOrEmpty(novaSenha) || novaSenha != confirmarNovaSenha)
            {
                TempData["ErrorMessage"] = "A nova senha e a confirmação não coincidem ou estão vazias.";
                return View();
            }

            usuario.SenhaHash = novaSenha; // TODO: aplicar hash seguro

            try
            {
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Senha alterada com sucesso!";
                return RedirectToAction("Index");
            }
            catch (DbUpdateConcurrencyException)
            {
                TempData["ErrorMessage"] = "Ocorreu um erro ao salvar a senha. Tente novamente.";
                return View();
            }
        }
    }
}
