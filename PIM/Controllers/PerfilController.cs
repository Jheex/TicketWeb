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
    /// <summary>
    /// Controlador responsável por gerenciar o perfil do usuário logado, incluindo
    /// visualização de informações, edição de dados cadastrais e alteração de senha.
    /// <para>Requer autenticação para todas as Actions.</para>
    /// </summary>
    [Authorize]
    public class PerfilController : Controller
    {
        private readonly AppDbContext _context;

        /// <summary>
        /// Inicializa uma nova instância do controlador PerfilController.
        /// </summary>
        /// <param name="context">O contexto do banco de dados (AppDbContext) injetado via DI.</param>
        public PerfilController(AppDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Método utilitário assíncrono para obter o objeto <see cref="Usuario"/>
        /// correspondente ao usuário logado, usando o <c>ClaimTypes.NameIdentifier</c>.
        /// </summary>
        /// <returns>O objeto Usuario logado ou <c>null</c> se o usuário não for encontrado ou não estiver autenticado.</returns>
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
        /// <summary>
        /// Exibe a página principal do perfil, mostrando os dados do usuário e estatísticas de tickets.
        /// </summary>
        /// <returns>A View Index com o objeto Usuario ou redireciona para o login.</returns>
        public async Task<IActionResult> Index()
        {
            var usuario = await GetCurrentUserAsync();
            if (usuario == null)
            {
                return RedirectToAction("Login", "Account");
            }

            // Contagem de tickets do usuário (como analista)
            var totalTicketsAbertos = await _context.Chamados.CountAsync(c => c.Status == "Em Andamento" && c.AtribuidoAId == usuario.Id);
            var totalTicketsFechados = await _context.Chamados.CountAsync(c => (c.Status == "Concluído" || c.Status == "Rejeitado") && c.AtribuidoAId == usuario.Id);

            ViewBag.TotalTicketsAbertos = totalTicketsAbertos;
            ViewBag.TotalTicketsFechados = totalTicketsFechados;

            return View(usuario);
        }

        // Action para a visualização de edição do perfil
        /// <summary>
        /// Exibe o formulário de edição de dados cadastrais do perfil.
        /// </summary>
        /// <returns>A View Editar com o objeto Usuario preenchido ou redireciona para o login.</returns>
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
        /// <summary>
        /// Processa o envio do formulário de edição de perfil, valida os dados e salva no banco.
        /// <para>Nota: Campos sensíveis como Senha e Role são ignorados na edição.</para>
        /// </summary>
        /// <param name="usuario">O objeto <see cref="Usuario"/> com os novos dados do formulário.</param>
        /// <returns>Redireciona para Index em caso de sucesso ou retorna a View Editar com erros.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Editar([Bind("Id,Username,Email,Telefone,Endereco,DataNascimento,Status,Observacoes")] Usuario usuario)
        {
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
            // Verifica se o usuário logado corresponde ao ID que está sendo editado
            if (currentUser == null || currentUser.Id != usuario.Id)
            {
                return Unauthorized();
            }

            // Atribuição e atualização dos dados permitidos
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
        /// <summary>
        /// Exibe o formulário para alteração de senha.
        /// </summary>
        /// <returns>A View MudarSenha ou redireciona para o login.</returns>
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
        /// <summary>
        /// Processa a requisição de alteração de senha, verificando a senha atual, a conformidade
        /// e a complexidade da nova senha antes de salvar.
        /// <para>Atenção: A SenhaHash deve ser substituída por uma função de Hash segura (ex: BCrypt).</para>
        /// </summary>
        /// <param name="senhaAtual">A senha atual do usuário (para verificação).</param>
        /// <param name="novaSenha">A nova senha digitada.</param>
        /// <param name="confirmarNovaSenha">Confirmação da nova senha.</param>
        /// <returns>Redireciona para Index em caso de sucesso ou retorna a View MudarSenha com erros.</returns>
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

            // 1. VERIFICAÇÃO DA SENHA ATUAL (DEVE SER FEITA COM HASH SEGURO NA IMPLEMENTAÇÃO FINAL)
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
            
            // 4. ATUALIZAÇÃO DA SENHA (Aplicar HASH seguro aqui)
            usuario.SenhaHash = novaSenha; // Placeholder - DEVE SER HASHED

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

        /// <summary>
        /// Função auxiliar para verificar os critérios de complexidade da senha:
        /// Mínimo 8 caracteres, uma maiúscula, uma minúscula, um número e um caractere especial.
        /// </summary>
        /// <param name="senha">A senha a ser verificada.</param>
        /// <returns>Uma string vazia se a senha for válida; caso contrário, uma mensagem de erro.</returns>
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