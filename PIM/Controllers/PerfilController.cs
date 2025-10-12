using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PIM.Data;
using PIM.Models;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using System;
using System.Linq; // Necessário para usar .Any() na verificação de senha
using System.Text.RegularExpressions; // Necessário para usar Regex na verificação de senha

namespace PIM.Controllers
{
    // Recomenda-se adicionar o [Authorize] a toda a classe se todas as actions exigirem login
    [Authorize]
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
            var totalTicketsAbertos = await _context.Chamados.CountAsync(c => c.Status == "Em Andamento" && c.AtribuidoAId == usuario.Id);
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
        public async Task<IActionResult> Editar([Bind("Id,Username,Email,Telefone,Endereco,DataNascimento,Status,Observacoes")] Usuario usuario)
        {
            // SOLUÇÃO PARA O ERRO DE VALIDAÇÃO: 
            // Remove erros de validação dos campos [Required] que não estão no formulário de edição.
            ModelState.Remove("SenhaHash");
            ModelState.Remove("Role");
            ModelState.Remove("ConfirmarSenha");

            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "Verifique os erros de validação no formulário e tente novamente.";
                return View(usuario); 
            }

            var currentUser = await GetCurrentUserAsync();
            if (currentUser == null || currentUser.Id != usuario.Id)
            {
                return Unauthorized();
            }

            // Atribuição de dados
            currentUser.Username = usuario.Username;
            currentUser.Email = usuario.Email;
            currentUser.Telefone = usuario.Telefone;
            currentUser.Endereco = usuario.Endereco;
            currentUser.DataNascimento = usuario.DataNascimento;
            currentUser.Status = usuario.Status;
            currentUser.Observacoes = usuario.Observacoes;

            try
            {
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Perfil atualizado com sucesso!";
            }
            catch (DbUpdateConcurrencyException)
            {
                TempData["ErrorMessage"] = "Não foi possível salvar as alterações. O registro pode ter sido alterado por outro usuário.";
                return RedirectToAction("Editar");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Ocorreu um erro inesperado ao salvar: {ex.Message}";
                return RedirectToAction("Editar");
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

            // 1. VERIFICAÇÃO DA SENHA ATUAL (Substituir por IPasswordHasher.VerifyHashedPassword)
            if (senhaAtual != usuario.SenhaHash) 
            {
                TempData["ErrorMessage"] = "A senha atual está incorreta.";
                return View();
            }

            // 2. VERIFICAÇÃO BÁSICA DE CONFERÊNCIA
            if (string.IsNullOrEmpty(novaSenha) || novaSenha != confirmarNovaSenha)
            {
                TempData["ErrorMessage"] = "A nova senha e a confirmação não coincidem ou estão vazias.";
                return View();
            }

            // 3. VERIFICAÇÃO DE CRITÉRIOS DE COMPLEXIDADE
            string erroComplexidade = VerificarComplexidadeSenha(novaSenha);
            if (!string.IsNullOrEmpty(erroComplexidade))
            {
                TempData["ErrorMessage"] = erroComplexidade;
                return View();
            }
            
            // 4. ATUALIZAÇÃO DA SENHA (Lembre-se de aplicar o HASH seguro aqui)
            usuario.SenhaHash = novaSenha; 

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

        // Função auxiliar para verificar os critérios de complexidade da senha
        private string VerificarComplexidadeSenha(string senha)
        {
            if (senha.Length < 8)
            {
                return "A senha deve ter no mínimo 8 caracteres.";
            }
            if (!senha.Any(char.IsUpper))
            {
                return "A senha deve conter pelo menos uma letra maiúscula.";
            }
            if (!senha.Any(char.IsLower))
            {
                return "A senha deve conter pelo menos uma letra minúscula.";
            }
            if (!senha.Any(char.IsDigit))
            {
                return "A senha deve conter pelo menos um número.";
            }
            
            // Verifica se contém um caractere especial (não é letra nem número)
            var simbolos = new Regex("[^a-zA-Z0-9]");
            if (!simbolos.IsMatch(senha))
            {
                return "A senha deve conter pelo menos um caractere especial (símbolo).";
            }

            return string.Empty; // Senha válida
        }
    }
}