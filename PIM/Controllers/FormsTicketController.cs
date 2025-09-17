using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using PIM.Data;
using PIM.Models;
using PIM.ViewModels;
using System.Text.Json;

namespace PIM.Controllers
{
    public class FormsTicketController : Controller
    {
        private readonly AppDbContext _db;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public FormsTicketController(AppDbContext db, IWebHostEnvironment webHostEnvironment)
        {
            _db = db;
            _webHostEnvironment = webHostEnvironment;
        }

        // ETAPA 1: Exibe o formulário de criação
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            ViewBag.Usuarios = new SelectList(await _db.Admins.OrderBy(u => u.Username).ToListAsync(), "Id", "Username");
            return View();
        }

        // ETAPA 2: Recebe os dados, valida e redireciona para a confirmação
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ChamadoViewModel model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Usuarios = new SelectList(await _db.Admins.OrderBy(u => u.Username).ToListAsync(), "Id", "Username", model.UserId);
                return View(model);
            }

            // Armazenar o anexo temporariamente se existir
            if (model.Attachment != null)
            {
                using (var memoryStream = new MemoryStream())
                {
                    await model.Attachment.CopyToAsync(memoryStream);
                    TempData["AttachmentBytes"] = memoryStream.ToArray();
                    TempData["AttachmentFileName"] = model.Attachment.FileName;
                }
            }

            TempData["ChamadoModel"] = JsonSerializer.Serialize(model);
            return RedirectToAction(nameof(Confirmacao));
        }


        // ETAPA 3: Exibe a página de confirmação com os dados preenchidos
        [HttpGet]
        public IActionResult Confirmacao()
        {
            var modelJson = TempData["ChamadoModel"] as string;
            if (string.IsNullOrEmpty(modelJson))
            {
                return RedirectToAction(nameof(Create));
            }

            // CORREÇÃO: Adicionamos uma verificação para garantir que 'model' não seja nulo após a desserialização.
            var model = JsonSerializer.Deserialize<ChamadoViewModel>(modelJson);
            if (model == null)
            {
                TempData["ErrorMessage"] = "Ocorreu um erro ao processar os dados do chamado.";
                return RedirectToAction(nameof(Create));
            }
            
            TempData.Keep("ChamadoModel");
            TempData.Keep("AttachmentBytes");
            TempData.Keep("AttachmentFileName");

            var usuario = _db.Admins.Find(model.UserId);
            ViewBag.NomeSolicitante = usuario?.Username ?? "Não encontrado";

            return View(model);
        }


        // ETAPA 4: Efetiva a criação do chamado no banco de dados
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConfirmacaoPost()
        {
            var modelJson = TempData["ChamadoModel"] as string;
            if (string.IsNullOrEmpty(modelJson))
            {
                TempData["ErrorMessage"] = "Sua sessão expirou. Por favor, abra o chamado novamente.";
                return RedirectToAction("Index", "Home");
            }

            // CORREÇÃO: Adicionamos uma verificação para garantir que 'model' não seja nulo.
            var model = JsonSerializer.Deserialize<ChamadoViewModel>(modelJson);
            if (model == null)
            {
                TempData["ErrorMessage"] = "Ocorreu um erro ao processar os dados do chamado.";
                return RedirectToAction("Index", "Home");
            }

            string? nomeArquivoUnico = null; // CORREÇÃO: Declarado como string anulável
            if (TempData["AttachmentBytes"] is byte[] fileBytes)
            {
                // CORREÇÃO: Adicionamos uma verificação para garantir que 'AttachmentFileName' não seja nulo.
                string? nomeOriginal = TempData["AttachmentFileName"]?.ToString();
                if (!string.IsNullOrEmpty(nomeOriginal))
                {
                    string pastaUploads = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", "chamados");
                    Directory.CreateDirectory(pastaUploads);
                    nomeArquivoUnico = Guid.NewGuid().ToString() + "_" + nomeOriginal;
                    string caminhoParaSalvar = Path.Combine(pastaUploads, nomeArquivoUnico);
                    
                    await System.IO.File.WriteAllBytesAsync(caminhoParaSalvar, fileBytes);
                }
            }
            
            var novoChamado = new Chamado
            {
                SolicitanteId = model.UserId,
                Titulo = model.Title,
                Descricao = model.Description, // Já é anulável no ViewModel
                Categoria = model.Category,
                Prioridade = model.Priority,
                NomeArquivoAnexo = TempData["AttachmentFileName"]?.ToString(),
                CaminhoArquivoAnexo = nomeArquivoUnico,
                Status = "Aberto",
                DataAbertura = DateTime.Now
            };

            _db.Chamados.Add(novoChamado);
            await _db.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Chamado Nº {novoChamado.ChamadoId} aberto com sucesso!";
            return RedirectToAction("Index", "Home");
        }
    }
}