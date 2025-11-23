using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PIM.Data;
using PIM.Models;
using System.Security.Claims;
using System.Linq;
using System.Threading.Tasks;
using System;

namespace PIM.Controllers
{
    /// <summary>
    /// Controlador responsável pelo gerenciamento completo dos usuários (CRUD) e pelo fluxo de autenticação (Login/Logout) usando Session.
    /// <para>Utiliza paginação e filtragem na Action Index para a lista de usuários.</para>
    /// </summary>
    public class UsuariosController : Controller
    {
        private readonly AppDbContext _db;
        private const int PageSize = 10;

        /// <summary>
        /// Inicializa uma nova instância do controlador UsuariosController.
        /// </summary>
        /// <param name="db">O contexto do banco de dados (AppDbContext) injetado via DI.</param>
        public UsuariosController(AppDbContext db)
        {
            _db = db;
        }

        // =========================
        // CRUD DE USUÁRIOS
        // =========================

        // GET: Index
        /// <summary>
        /// Exibe a lista paginada de usuários, permitindo busca por nome de usuário.
        /// </summary>
        /// <param name="searchString">String de busca para filtrar usuários por nome de usuário.</param>
        /// <param name="pageNumber">O número da página atual para exibição (padrão é 1).</param>
        /// <returns>A View Index com uma lista paginada de objetos <see cref="Usuario"/>.</returns>
        public async Task<IActionResult> Index(string searchString, int pageNumber = 1)
        {
            ViewData["CurrentFilter"] = searchString;

            var users = _db.Usuarios.AsQueryable();

            // Aplica filtro de busca
            if (!string.IsNullOrEmpty(searchString))
                users = users.Where(u => u.Username != null && u.Username.StartsWith(searchString));

            users = users.OrderBy(u => u.Id);

            var totalUsers = await users.CountAsync();

            // Lógica de Paginação
            var pagedUsers = await users
                .Skip((pageNumber - 1) * PageSize)
                .Take(PageSize)
                .ToListAsync();

            ViewData["TotalPages"] = (int)Math.Ceiling(totalUsers / (double)PageSize);
            ViewData["CurrentPage"] = pageNumber;

            // Esta página precisa estar preparada para exibir mensagens de TempData (Sucesso ou Erro)
            return View(pagedUsers);
        }

        // GET: Create
        /// <summary>
        /// Exibe o formulário para criar um novo usuário.
        /// </summary>
        /// <returns>A View Create.</returns>
        public IActionResult Create() => View();

        // POST: Create
        /// <summary>
        /// Processa o envio do formulário para criar um novo usuário.
        /// </summary>
        /// <param name="usuario">O objeto <see cref="Usuario"/> preenchido pelo formulário.</param>
        /// <returns>Redireciona para Index em caso de sucesso ou retorna a View com erros.</returns>
        [HttpPost]
        public IActionResult Create(Usuario usuario)
        {
            if (!ModelState.IsValid)
            {
                // Define a mensagem de erro antes de retornar a View
                TempData["ErrorMessage"] = "Verifique os erros de validação no formulário e tente novamente.";
                return View(usuario);
            }

            usuario.CreatedAt = DateTime.Now;
            _db.Usuarios.Add(usuario);
            _db.SaveChanges();

            // MENSAGEM DE SUCESSO DE CRIAÇÃO
            TempData["SuccessMessage"] = $"Usuário '{usuario.Username}' cadastrado com sucesso!";

            return RedirectToAction(nameof(Index));
        }

        // GET: Edit
        /// <summary>
        /// Exibe o formulário para editar um usuário existente.
        /// </summary>
        /// <param name="id">O ID do usuário a ser editado.</param>
        /// <returns>A View Edit com o objeto <see cref="Usuario"/> ou 404 Not Found.</returns>
        public IActionResult Edit(int id)
        {
            var usuario = _db.Usuarios.Find(id);
            if (usuario == null) return NotFound();
            return View(usuario);
        }

        // POST: Edit
        /// <summary>
        /// Processa o envio do formulário para atualizar um usuário existente.
        /// </summary>
        /// <param name="usuario">O objeto <see cref="Usuario"/> com os dados atualizados.</param>
        /// <returns>Redireciona para Index em caso de sucesso ou retorna a View com erros/conflitos.</returns>
        [HttpPost]
        public IActionResult Edit(Usuario usuario)
        {
            // Trata a validação de campos opcionais na edição (Senha e DataNascimento)
            if (string.IsNullOrEmpty(usuario.SenhaHash))
            {
                ModelState.Remove(nameof(Usuario.SenhaHash));
                ModelState.Remove(nameof(Usuario.ConfirmarSenha));
            }
            if (usuario.DataNascimento == default(DateTime))
            {
                ModelState.Remove(nameof(Usuario.DataNascimento));
            }

            if (!ModelState.IsValid)
            {
                // FEEDBACK DE ERRO: Define o TempData antes de retornar a View
                TempData["ErrorMessage"] = "Verifique os erros de validação no formulário e tente novamente.";
                return View(usuario);
            }

            var userToUpdate = _db.Usuarios.Find(usuario.Id);
            if (userToUpdate == null) 
            {
                TempData["ErrorMessage"] = "Usuário não encontrado.";
                return NotFound();
            }

            // Atualiza os campos do objeto rastreado
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
                userToUpdate.SenhaHash = usuario.SenhaHash; // Deve-se aplicar hash aqui

            try
            {
                _db.SaveChanges();

                // MENSAGEM DE SUCESSO APÓS SALVAMENTO
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
        /// <summary>
        /// Exibe a tela de confirmação de exclusão para um usuário.
        /// </summary>
        /// <param name="id">O ID do usuário a ser excluído.</param>
        /// <returns>A View Delete com o objeto <see cref="Usuario"/> ou 404 Not Found.</returns>
        public IActionResult Delete(int id)
        {
            var usuario = _db.Usuarios.Find(id);
            if (usuario == null) return NotFound();
            return View(usuario);
        }

        // POST: Delete
        /// <summary>
        /// Confirma e executa a exclusão de um usuário.
        /// </summary>
        /// <param name="id">O ID do usuário a ser excluído.</param>
        /// <returns>Redireciona para Index em caso de sucesso ou 404 Not Found.</returns>
        [HttpPost, ActionName("Delete")]
        public IActionResult DeleteConfirmed(int id)
        {
            var usuario = _db.Usuarios.Find(id);
            if (usuario == null) return NotFound();

            try
            {
                _db.Usuarios.Remove(usuario);
                _db.SaveChanges();

                TempData["SuccessMessage"] = $"Usuário '{usuario.Username}' removido com sucesso.";
            }
            catch (DbUpdateException)
            {
                TempData["ErrorMessage"] =
                    "Este usuário não pode ser excluído porque existe um chamado vinculado a ele.";

                return RedirectToAction(nameof(Index));
            }

            return RedirectToAction(nameof(Index));
        }


        // GET: Profile
        /// <summary>
        /// Exibe o perfil (detalhes) de um usuário.
        /// </summary>
        /// <param name="id">O ID do usuário cujo perfil deve ser exibido.</param>
        /// <returns>A View Profile com o objeto <see cref="Usuario"/> ou 404 Not Found.</returns>
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
        /// <summary>
        /// Exibe a tela de Login.
        /// </summary>
        /// <returns>A View Login.</returns>
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        // POST: Login
        /// <summary>
        /// Processa a autenticação do usuário.
        /// </summary>
        /// <param name="email">O email fornecido.</param>
        /// <param name="senha">A senha fornecida (deve corresponder ao SenhaHash, que teoricamente já conteria o hash).</param>
        /// <returns>Redireciona para Home/Index em caso de sucesso ou retorna a View Login com erros.</returns>
        [HttpPost]
        public IActionResult Login(string email, string senha)
        {
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(senha))
            {
                ModelState.AddModelError("", "Email e senha são obrigatórios.");
                return View();
            }

            // Nota: Em um sistema real, 'senha' seria um hash. Aqui, ele está sendo comparado diretamente.
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

            // Autenticar: Usa a Session do ASP.NET Core para manter o estado do usuário logado
            HttpContext.Session.SetString("UsuarioId", usuario.Id.ToString());
            HttpContext.Session.SetString("UsuarioNome", usuario.Username);
            HttpContext.Session.SetString("UsuarioRole", usuario.Role);

            return RedirectToAction("Index", "Home");
        }

        // GET: Logout
        /// <summary>
        /// Efetua o Logout do usuário, limpando a Session.
        /// </summary>
        /// <returns>Redireciona para a tela de Login.</returns>
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }
    }
}