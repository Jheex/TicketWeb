using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PIM.Data;
using PIM.Models;
using System.Security.Claims;

namespace PIM.Controllers
{
    public class UsuariosController : Controller
    {
        private readonly AppDbContext _db;
        private const int PageSize = 10;

        public UsuariosController(AppDbContext db)
        {
            _db = db;
        }

        // =========================
        // CRUD DE USUÁRIOS
        // =========================

        // GET: Index
        public async Task<IActionResult> Index(string searchString, int pageNumber = 1)
        {
            ViewData["CurrentFilter"] = searchString;

            var users = _db.Usuarios.AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
                users = users.Where(u => u.Username != null && u.Username.StartsWith(searchString));

            users = users.OrderBy(u => u.Id);

            var totalUsers = await users.CountAsync();

            var pagedUsers = await users
                .Skip((pageNumber - 1) * PageSize)
                .Take(PageSize)
                .ToListAsync();

            ViewData["TotalPages"] = (int)Math.Ceiling(totalUsers / (double)PageSize);
            ViewData["CurrentPage"] = pageNumber;

            // Esta página também precisa estar preparada para exibir TempData
            return View(pagedUsers);
        }

        // GET: Create
        public IActionResult Create() => View();

        // POST: Create
        [HttpPost]
        public IActionResult Create(Usuario usuario)
        {
            if (!ModelState.IsValid)
            {
                // Se a validação falhar, define a mensagem de erro antes de retornar a View
                TempData["ErrorMessage"] = "Verifique os erros de validação no formulário e tente novamente.";
                return View(usuario);
            }

            usuario.CreatedAt = DateTime.Now;
            _db.Usuarios.Add(usuario);
            _db.SaveChanges();

            // 🚀 MENSAGEM DE SUCESSO DE CRIAÇÃO
            TempData["SuccessMessage"] = $"Usuário '{usuario.Username}' cadastrado com sucesso!";

            return RedirectToAction(nameof(Index));
        }

        // GET: Edit
        public IActionResult Edit(int id)
        {
            var usuario = _db.Usuarios.Find(id);
            if (usuario == null) return NotFound();
            return View(usuario);
        }

        // POST: Edit
        [HttpPost]
        public IActionResult Edit(Usuario usuario)
        {
            // 1. TRATAMENTO DE VALIDAÇÃO (Permite que Senha e DataNascimento fiquem vazios)
            // Remove a validação da senha se o campo vier vazio (para não ser obrigatório na edição)
            if (string.IsNullOrEmpty(usuario.SenhaHash))
            {
                ModelState.Remove(nameof(Usuario.SenhaHash));
                ModelState.Remove(nameof(Usuario.ConfirmarSenha));
            }
            
            // Remove a validação de DataNascimento se vier com o valor padrão (01/01/0001)
            if (usuario.DataNascimento == default(DateTime))
            {
                ModelState.Remove(nameof(Usuario.DataNascimento));
            }

            if (!ModelState.IsValid)
            {
                // 🚀 FEEDBACK DE ERRO: Define o TempData antes de retornar a View
                TempData["ErrorMessage"] = "Verifique os erros de validação no formulário e tente novamente.";
                return View(usuario);
            }

            var userToUpdate = _db.Usuarios.Find(usuario.Id);
            if (userToUpdate == null) 
            {
                TempData["ErrorMessage"] = "Usuário não encontrado.";
                return NotFound();
            }

            // Atualiza os campos do objeto rastreado (userToUpdate)
            userToUpdate.Username = usuario.Username;
            userToUpdate.Email = usuario.Email;
            userToUpdate.Role = usuario.Role;
            userToUpdate.Status = usuario.Status;
            userToUpdate.Telefone = usuario.Telefone;
            userToUpdate.Endereco = usuario.Endereco;
            userToUpdate.DataNascimento = usuario.DataNascimento;
            userToUpdate.Observacoes = usuario.Observacoes;

            // Atualiza a Senha SOMENTE se o campo não estiver vazio
            if (!string.IsNullOrEmpty(usuario.SenhaHash))
                userToUpdate.SenhaHash = usuario.SenhaHash; // aqui você pode aplicar hash
            
            try
            {
                _db.SaveChanges();

                // 🚀 LINHA CRÍTICA: MENSAGEM DE SUCESSO APÓS SALVAMENTO
                TempData["SuccessMessage"] = "Suas alterações foram atualizadas com sucesso!"; 

                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_db.Usuarios.Any(e => e.Id == usuario.Id))
                {
                    TempData["ErrorMessage"] = "O registro foi excluído por outro usuário.";
                    return RedirectToAction(nameof(Index));
                }
                TempData["ErrorMessage"] = "Um erro de concorrência ocorreu. O registro pode ter sido alterado por outro usuário.";
                return View(usuario);
            }
            catch (Exception ex)
            {
                // Tratamento de erro genérico
                TempData["ErrorMessage"] = $"Ocorreu um erro inesperado ao salvar: {ex.Message}";
                return View(usuario);
            }
        }

        // GET: Delete
        public IActionResult Delete(int id)
        {
            var usuario = _db.Usuarios.Find(id);
            if (usuario == null) return NotFound();
            return View(usuario);
        }

        // POST: Delete
        [HttpPost, ActionName("Delete")]
        public IActionResult DeleteConfirmed(int id)
        {
            var usuario = _db.Usuarios.Find(id);
            if (usuario == null) return NotFound();

            _db.Usuarios.Remove(usuario);
            _db.SaveChanges();
            
            // 🚀 MENSAGEM DE SUCESSO DE EXCLUSÃO
            TempData["SuccessMessage"] = $"Usuário '{usuario.Username}' removido com sucesso.";

            return RedirectToAction(nameof(Index));
        }

        // GET: Profile
        public IActionResult Profile(int id)
        {
            var usuario = _db.Usuarios.Find(id);
            if (usuario == null) return NotFound();
            return View(usuario);
        }

        // =========================
        // LOGIN E LOGOUT
        // =========================

        // GET: Login
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        // POST: Login
        [HttpPost]
        public IActionResult Login(string email, string senha)
        {
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(senha))
            {
                ModelState.AddModelError("", "Email e senha são obrigatórios.");
                return View();
            }

            var usuario = _db.Usuarios.FirstOrDefault(u => u.Email == email && u.SenhaHash == senha);

            if (usuario == null)
            {
                ModelState.AddModelError("", "Email ou senha incorretos.");
                return View();
            }

            if (usuario.Status != "Ativo")
            {
                ModelState.AddModelError("", "Seu usuário está inativo e não pode acessar o sistema.");
                return View();
            }

            // Autenticar
            HttpContext.Session.SetString("UsuarioId", usuario.Id.ToString());
            HttpContext.Session.SetString("UsuarioNome", usuario.Username);
            HttpContext.Session.SetString("UsuarioRole", usuario.Role);

            return RedirectToAction("Index", "Home");
        }

        // GET: Logout
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }
    }
}