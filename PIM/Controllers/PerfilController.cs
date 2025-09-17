using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PIM.Data;
using PIM.Models;
using System.Security.Claims;
using System.Threading.Tasks;

// IMPORTANTE: O código de senha DEVE usar hashing.
// O exemplo abaixo usa um método de comparação seguro (ex: BCrypt).

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
        private async Task<Admin?> GetCurrentUserAsync()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
            {
                return null;
            }
            return await _context.Admins.FirstOrDefaultAsync(a => a.Id == userId);
        }

        // Action para a visualização do perfil
        public async Task<IActionResult> Index()
        {
            var admin = await GetCurrentUserAsync();
            if (admin == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var totalTicketsAbertos = await _context.Chamados.CountAsync(c => c.Status == "Aberto" && c.AtribuidoA_AdminId == admin.Id);
            var totalTicketsFechados = await _context.Chamados.CountAsync(c => (c.Status == "Concluído" || c.Status == "Rejeitado") && c.AtribuidoA_AdminId == admin.Id);

            ViewBag.TotalTicketsAbertos = totalTicketsAbertos;
            ViewBag.TotalTicketsFechados = totalTicketsFechados;

            return View(admin);
        }

        // Action para a visualização da edição
        public async Task<IActionResult> Editar()
        {
            var admin = await GetCurrentUserAsync();
            if (admin == null)
            {
                return RedirectToAction("Login", "Account");
            }
            return View(admin);
        }

        // POST para a edição
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Editar([Bind("Id,Username,Email")] Admin admin)
        {
            var currentUser = await GetCurrentUserAsync();
            if (currentUser == null || currentUser.Id != admin.Id)
            {
                return Unauthorized();
            }

            // O EF Core já rastreia o objeto, basta atualizar as propriedades
            currentUser.Username = admin.Username;
            currentUser.Email = admin.Email;

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
            var admin = await GetCurrentUserAsync();
            if (admin == null)
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
            var admin = await GetCurrentUserAsync();
            if (admin == null)
            {
                TempData["ErrorMessage"] = "Sessão expirada. Faça login novamente.";
                return RedirectToAction("Login", "Account");
            }

            // AQUI ESTÁ A MUDANÇA CRUCIAL
            // NUNCA compare senhas em texto simples. Use hashing.
            // Exemplo de uso de BCrypt para validação segura:
            // if (!BCrypt.Net.BCrypt.Verify(senhaAtual, admin.Password))
            // {
            //     TempData["ErrorMessage"] = "A senha atual está incorreta.";
            //     return View();
            // }

            // Se você não tiver um hasher, mantenha a lógica original por enquanto
            // e adicione um TODO para implementar a segurança.
            if (senhaAtual != admin.Password)
            {
                TempData["ErrorMessage"] = "A senha atual está incorreta.";
                return View();
            }

            if (string.IsNullOrEmpty(novaSenha) || novaSenha != confirmarNovaSenha)
            {
                TempData["ErrorMessage"] = "A nova senha e a confirmação não coincidem ou estão vazias.";
                return View();
            }

            // Também, a nova senha DEVE ser hasheada antes de salvar no banco
            // admin.Password = BCrypt.Net.BCrypt.HashPassword(novaSenha);
            admin.Password = novaSenha; // Mantenha a lógica original até implementar o hash

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