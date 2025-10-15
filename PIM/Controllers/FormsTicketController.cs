using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using PIM.Data;
using PIM.Models;
using PIM.ViewModels;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting; // Necessário para IWebHostEnvironment
using System.IO; // Necessário para Path e MemoryStream
using System; // Necessário para DateTime e Guid

namespace PIM.Controllers
{
    /// <summary>
    /// Controlador responsável pelo fluxo de abertura de um novo chamado em múltiplas etapas (formulário e confirmação).
    /// Ele lida com a coleta de dados, validação, armazenamento temporário de anexos e a criação final do ticket.
    /// </summary>
    public class FormsTicketController : Controller
    {
        private readonly AppDbContext _db;
        private readonly IWebHostEnvironment _webHostEnvironment;

        /// <summary>
        /// Inicializa uma nova instância do controlador FormsTicketController.
        /// </summary>
        /// <param name="db">O contexto do banco de dados (AppDbContext) injetado via DI.</param>
        /// <param name="webHostEnvironment">O ambiente de hospedagem para gerenciar caminhos de arquivos (uploads).</param>
        public FormsTicketController(AppDbContext db, IWebHostEnvironment webHostEnvironment)
        {
            _db = db;
            _webHostEnvironment = webHostEnvironment;
        }

        // ETAPA 1: Exibe o formulário de criação
        /// <summary>
        /// [GET] Exibe o formulário inicial para a criação de um novo chamado.
        /// </summary>
        /// <returns>A View Create, pré-populada com a lista de usuários para seleção do solicitante.</returns>
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            // Carrega a lista de usuários para o dropdown (Solicitante)
            ViewBag.Usuarios = new SelectList(await _db.Usuarios.OrderBy(u => u.Username).ToListAsync(), "Id", "Username");
            return View();
        }

        // ETAPA 2: Recebe os dados, valida e redireciona para a confirmação
        /// <summary>
        /// [POST] Processa o formulário de criação. Se válido, armazena os dados e o anexo temporariamente
        /// e redireciona para a página de confirmação.
        /// </summary>
        /// <param name="model">O <see cref="ChamadoViewModel"/> contendo os dados do chamado e o anexo.</param>
        /// <returns>Redireciona para <see cref="Confirmacao"/> se válido, ou retorna a View Create com erros.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ChamadoViewModel model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Usuarios = new SelectList(await _db.Usuarios.OrderBy(u => u.Username).ToListAsync(), "Id", "Username", model.UserId);
                return View(model);
            }

            // Armazenar o anexo temporariamente em TempData
            if (model.Attachment != null)
            {
                using (var memoryStream = new MemoryStream())
                {
                    await model.Attachment.CopyToAsync(memoryStream);
                    TempData["AttachmentBytes"] = memoryStream.ToArray();
                    TempData["AttachmentFileName"] = model.Attachment.FileName;
                }
            }

            // Armazenar o ViewModel completo em TempData como JSON
            TempData["ChamadoModel"] = JsonSerializer.Serialize(model);
            return RedirectToAction(nameof(Confirmacao));
        }

        // ETAPA 3: Exibe a página de confirmação com os dados preenchidos
        /// <summary>
        /// [GET] Exibe a tela de confirmação do chamado, recuperando os dados e o anexo armazenados em TempData.
        /// </summary>
        /// <returns>A View Confirmacao ou redireciona para Create se não houver dados de sessão.</returns>
        [HttpGet]
        public IActionResult Confirmacao()
        {
            var modelJson = TempData["ChamadoModel"] as string;
            if (string.IsNullOrEmpty(modelJson))
            {
                return RedirectToAction(nameof(Create));
            }

            var model = JsonSerializer.Deserialize<ChamadoViewModel>(modelJson);
            if (model == null)
            {
                TempData["ErrorMessage"] = "Ocorreu um erro ao processar os dados do chamado.";
                return RedirectToAction(nameof(Create));
            }
            
            // Mantém os dados no TempData para o POST de confirmação
            TempData.Keep("ChamadoModel");
            TempData.Keep("AttachmentBytes");
            TempData.Keep("AttachmentFileName");

            var usuario = _db.Usuarios.Find(model.UserId);
            ViewBag.NomeSolicitante = usuario?.Username ?? "Não encontrado";

            return View(model);
        }

        // ETAPA 4: Efetiva a criação do chamado no banco de dados
        /// <summary>
        /// [POST] Efetiva a criação do chamado no banco de dados.
        /// Persiste o arquivo anexo no sistema de arquivos do servidor antes de salvar a referência no banco.
        /// </summary>
        /// <returns>Redireciona para o Dashboard com mensagem de sucesso ou erro.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConfirmacaoPost()
        {
            var modelJson = TempData["ChamadoModel"] as string;
            if (string.IsNullOrEmpty(modelJson))
            {
                TempData["ErrorMessage"] = "Sua sessão expirou. Por favor, abra o chamado novamente.";
                return RedirectToAction("Index", "Dashboard");
            }

            var model = JsonSerializer.Deserialize<ChamadoViewModel>(modelJson);
            if (model == null)
            {
                TempData["ErrorMessage"] = "Ocorreu um erro ao processar os dados do chamado.";
                return RedirectToAction("Index", "Dashboard");
            }

            string? nomeArquivoUnico = null;
            if (TempData["AttachmentBytes"] is byte[] fileBytes)
            {
                string? nomeOriginal = TempData["AttachmentFileName"]?.ToString();
                if (!string.IsNullOrEmpty(nomeOriginal))
                {
                    // Lógica para salvar o arquivo físico
                    string pastaUploads = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", "chamados");
                    Directory.CreateDirectory(pastaUploads);
                    nomeArquivoUnico = Guid.NewGuid().ToString() + "_" + nomeOriginal;
                    string caminhoParaSalvar = Path.Combine(pastaUploads, nomeArquivoUnico);
                    
                    await System.IO.File.WriteAllBytesAsync(caminhoParaSalvar, fileBytes);
                }
            }
            
            // Mapeia o ViewModel para o Model de persistência (Chamado)
            var novoChamado = new Chamado
            {
                SolicitanteId = model.UserId,
                Titulo = model.Title,
                Descricao = model.Description,
                Categoria = model.Category,
                Prioridade = model.Priority,
                NomeArquivoAnexo = TempData["AttachmentFileName"]?.ToString(),
                CaminhoArquivoAnexo = nomeArquivoUnico, // Salva o nome único (Guid) no banco
                Status = "Aberto",
                DataAbertura = DateTime.Now
            };

            _db.Chamados.Add(novoChamado);
            await _db.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Chamado Nº {novoChamado.ChamadoId} aberto com sucesso!";
            return RedirectToAction("Index", "Dashboard");
        }
    }
}