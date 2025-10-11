using Microsoft.AspNetCore.Mvc;
using PIM.Data;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace PIM.Controllers
{
    public class ChatbotController : Controller
    {
        private readonly AppDbContext _context;

        public ChatbotController(AppDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Ask([FromBody] ChatRequest request)
        {
            if (string.IsNullOrWhiteSpace(request?.Question))
                return Json(new { answer = "Por favor, digite uma pergunta." });

            string perguntaNormalizada = NormalizeText(request.Question);

            // 1️⃣ Verificar interações rápidas
            var respostasRapidas = new Dictionary<string, string>
            {
                { "ola", "Olá! Eu sou a Inteligência Artificial da InovaTech. Posso te ajudar com dúvidas sobre usuários, relatórios, chamados e muito mais." },
                { "oi", "Oi! Estou aqui para te ajudar com qualquer dúvida." },
                { "bom dia", "Bom dia! Como posso te ajudar hoje?" },
                { "boa tarde", "Boa tarde! Precisa de ajuda com algo?" },
                { "boa noite", "Boa noite! Estou à disposição para esclarecer suas dúvidas." }
            };

            foreach (var rr in respostasRapidas)
            {
                if (perguntaNormalizada.Contains(rr.Key))
                    return Json(new { answer = rr.Value });
            }

            // 2️⃣ Buscar FAQ
            var resposta = BuscarRespostaFAQ(perguntaNormalizada);

            return Json(new { answer = resposta });
        }

        private string BuscarRespostaFAQ(string perguntaUsuario)
        {
            var faqs = _context.Faqs.ToList();

            var faqMaisProxima = faqs
                .OrderByDescending(f => Similaridade(NormalizeText(f.Pergunta ?? ""), perguntaUsuario))
                .FirstOrDefault();

            return faqMaisProxima != null
                ? faqMaisProxima.Resposta
                : "Desculpe, não encontrei uma resposta para isso.";
        }

        // Similaridade simples baseada em palavras em comum
        private int Similaridade(string texto1, string texto2)
        {
            var palavras1 = texto1.Split(' ', '.', ',', '?', '!', ';');
            var palavras2 = texto2.Split(' ', '.', ',', '?', '!', ';');
            return palavras1.Count(p => palavras2.Contains(p));
        }

        private string NormalizeText(string text)
        {
            text = text.ToLower();
            text = text.Normalize(NormalizationForm.FormD);
            text = Regex.Replace(text, @"[\u0300-\u036f]", ""); // remove acentos
            text = Regex.Replace(text, @"[^\w\s]", ""); // remove pontuação
            return text;
        }
    }

    public class ChatRequest
    {
        public string? Question { get; set; }
    }
}
