using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using PIM.Data;
using PIM.Models;
using PIM.ViewModels; // Adicionado: presumindo que ForgotPasswordViewModel esteja aqui
using System.Security.Claims;

namespace PIM.Controllers
{ 
    /// <summary>
    /// Controlador responsável por gerenciar as operações de autenticação do usuário, 
    /// incluindo Login e Logout, utilizando autenticação via Cookie no ASP.NET Core.
    /// </summary>
    public class AccountController : Controller
    {
        private readonly AppDbContext _db;

        /// <summary>
        /// Inicializa uma nova instância do controlador AccountController.
        /// </summary>
        /// <param name="db">O contexto do banco de dados (AppDbContext) injetado via DI.</param>
        public AccountController(AppDbContext db)
        {
            _db = db;
        }

        // =========================
        // GET: /Account/Login
        // =========================
        /// <summary>
        /// Exibe a página de Login.
        /// </summary>
        /// <returns>A View de Login.</returns>
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        // =========================
        // POST: /Account/Login
        // =========================
        /// <summary>
        /// Processa o envio do formulário de Login, buscando o usuário, verificando o status e autenticando via Cookie.
        /// </summary>
        /// <param name="Email">O endereço de e-mail do usuário.</param>
        /// <param name="password">A senha do usuário (que deve ser o SenhaHash).</param>
        /// <returns>Redireciona para o Dashboard em caso de sucesso ou retorna a View de Login com mensagem de erro.</returns>
        [HttpPost]
        public async Task<IActionResult> Login(string Email, string password)
        {
            if (string.IsNullOrEmpty(Email) || string.IsNullOrEmpty(password))
            {
                ViewBag.Error = "Preencha todos os campos!";
                return View();
            }

            Email = Email.Trim();
            password = password.Trim();

            // Busca o usuário no banco
            var user = _db.Usuarios.FirstOrDefault(u => u.Email == Email && u.SenhaHash == password);

            if (user == null)
            {
                ViewBag.Error = "Usuário ou senha inválidos!";
                return View();
            }

            // Verifica se o usuário está ativo
            if (user.Status != "Ativo")
            {
                ViewBag.Error = "Seu usuário está inativo e não pode acessar o sistema.";
                return View();
            }

            // Cria claims com base no usuário
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.Username ?? ""),
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Role, user.Role ?? "Usuario")
            };

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            // Autenticar via cookie
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

            // Redireciona para a dashboard
            return RedirectToAction("Index", "Dashboard");
        }

        // =========================
        // GET: /Account/ForgotPassword
        // =========================
        /// <summary>
        /// Exibe a página para solicitação de recuperação de senha.
        /// </summary>
        /// <returns>A View ForgotPassword.</returns>
        [HttpGet]
        public IActionResult ForgotPassword()
        {
            return View();
        }

        // =========================
        // POST: /Account/ForgotPassword
        // =========================
        /// <summary>
        /// Processa a solicitação de recuperação de senha. 
        /// Simula o envio de um link de redefinição de senha por e-mail (a lógica real está a implementar).
        /// </summary>
        /// <param name="model">O ViewModel contendo as informações necessárias para a recuperação de senha.</param>
        /// <returns>Redireciona para a página de Login com uma mensagem informativa.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ForgotPassword(ForgotPasswordViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            // Lógica de envio de email para redefinir senha (a implementar)
            TempData["Message"] = "Se o email existir, enviaremos um link para redefinir a senha.";

            return RedirectToAction("Login");
        }

        // =========================
        // GET: /Account/Logout
        // =========================
        /// <summary>
        /// Realiza o Logout do usuário, encerrando a sessão de autenticação baseada em Cookie.
        /// </summary>
        /// <returns>Redireciona para a página de Login.</returns>
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login");
        }
    }
}
