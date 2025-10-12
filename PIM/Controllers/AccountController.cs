using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using PIM.Data;
using PIM.Models;
using System.Security.Claims;

namespace PIM.Controllers
{   
    public class AccountController : Controller
    {
        private readonly AppDbContext _db;

        public AccountController(AppDbContext db)
        {
            _db = db;
        }

        // =========================
        // GET: /Account/Login
        // =========================
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        // =========================
        // POST: /Account/Login
        // =========================
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
        [HttpGet]
        public IActionResult ForgotPassword()
        {
            return View();
        }

        // =========================
        // POST: /Account/ForgotPassword
        // =========================
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
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login");
        }
    }
}
