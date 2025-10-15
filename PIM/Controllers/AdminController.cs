using Microsoft.AspNetCore.Mvc;
using PIM.Data;
using PIM.Models;
using System.Linq; // Necessário para .Where().ToList()

namespace PIM.Controllers
{
    /// <summary>
    /// Controlador responsável por gerenciar as operações de CRUD (Create, Read, Update, Delete)
    /// dos usuários que possuem o perfil de "Admin" no sistema.
    /// </summary>
    public class AdminController : Controller
    {
        private readonly AppDbContext _db;

        /// <summary>
        /// Inicializa uma nova instância do controlador AdminController.
        /// </summary>
        /// <param name="db">O contexto do banco de dados (AppDbContext) injetado via DI.</param>
        public AdminController(AppDbContext db)
        {
            _db = db;
        }

        /// <summary>
        /// Exibe a lista de todos os usuários com o perfil "Admin".
        /// </summary>
        /// <returns>A View Index contendo a lista de administradores.</returns>
        // GET: Admin
        public IActionResult Index()
        {
            // Apenas usuários com Role "Admin"
            var admins = _db.Usuarios.Where(u => u.Role == "Admin").ToList();
            return View(admins);
        }

        /// <summary>
        /// Exibe os detalhes de um usuário Admin específico.
        /// </summary>
        /// <param name="id">O ID do usuário Admin.</param>
        /// <returns>A View Details com o objeto Usuario ou NotFound se o ID não for encontrado.</returns>
        // GET: Admin/Details/5
        public IActionResult Details(int id)
        {
            var admin = _db.Usuarios.Find(id);
            if (admin == null) return NotFound();
            return View(admin);
        }

        /// <summary>
        /// Exibe o formulário para criação de um novo usuário Admin.
        /// </summary>
        /// <returns>A View Create.</returns>
        // GET: Admin/Create
        public IActionResult Create()
        {
            return View();
        }

        /// <summary>
        /// Processa o envio do formulário de criação. Define o perfil como "Admin" e salva o novo usuário.
        /// </summary>
        /// <param name="usuario">O objeto Usuario contendo os dados do novo administrador.</param>
        /// <returns>Redireciona para Index em caso de sucesso ou retorna a View Create com erros de validação.</returns>
        // POST: Admin/Create
        [HttpPost]
        public IActionResult Create(Usuario usuario)
        {
            if (!ModelState.IsValid) return View(usuario);

            // Define o Role como Admin
            usuario.Role = "Admin";

            _db.Usuarios.Add(usuario);
            _db.SaveChanges();
            return RedirectToAction(nameof(Index));
        }

        /// <summary>
        /// Exibe o formulário de edição para um usuário Admin existente.
        /// </summary>
        /// <param name="id">O ID do usuário Admin a ser editado.</param>
        /// <returns>A View Edit com o objeto Usuario preenchido, ou NotFound se o ID for inválido.</returns>
        // GET: Admin/Edit/5
        public IActionResult Edit(int id)
        {
            var admin = _db.Usuarios.Find(id);
            if (admin == null) return NotFound();
            return View(admin);
        }

        /// <summary>
        /// Processa o envio do formulário de edição e salva as alterações no banco de dados.
        /// </summary>
        /// <param name="usuario">O objeto Usuario contendo os dados atualizados.</param>
        /// <returns>Redireciona para Index em caso de sucesso ou retorna a View Edit com erros de validação.</returns>
        // POST: Admin/Edit/5
        [HttpPost]
        public IActionResult Edit(Usuario usuario)
        {
            if (!ModelState.IsValid) return View(usuario);

            _db.Update(usuario);
            _db.SaveChanges();
            return RedirectToAction(nameof(Index));
        }

        /// <summary>
        /// Exibe a confirmação de exclusão para um usuário Admin.
        /// </summary>
        /// <param name="id">O ID do usuário Admin a ser excluído.</param>
        /// <returns>A View Delete com os dados do usuário, ou NotFound.</returns>
        // GET: Admin/Delete/5
        public IActionResult Delete(int id)
        {
            var admin = _db.Usuarios.Find(id);
            if (admin == null) return NotFound();
            return View(admin);
        }

        /// <summary>
        /// Confirma a exclusão de um usuário Admin do banco de dados.
        /// </summary>
        /// <param name="id">O ID do usuário Admin a ser excluído.</param>
        /// <returns>Redireciona para Index após a exclusão.</returns>
        // POST: Admin/Delete/5
        [HttpPost, ActionName("Delete")]
        public IActionResult DeleteConfirmed(int id)
        {
            var admin = _db.Usuarios.Find(id);
            if (admin == null) return NotFound();

            _db.Usuarios.Remove(admin);
            _db.SaveChanges();
            return RedirectToAction(nameof(Index));
        }
    }
}