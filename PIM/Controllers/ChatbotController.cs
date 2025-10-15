using Microsoft.AspNetCore.Mvc;
using PIM.Data;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections.Generic; // Adicionado: Necessário para Dictionary

namespace PIM.Controllers
{
    /// <summary>
    /// Controlador principal responsável por gerenciar a interface do Chatbot e processar
    /// as requisições de perguntas, utilizando lógica de respostas rápidas e busca no FAQ.
    /// </summary>
    public class ChatbotController : Controller
    {
        private readonly AppDbContext _context;

        /// <summary>
        /// Inicializa uma nova instância do controlador ChatbotController.
        /// </summary>
        /// <param name="context">O contexto do banco de dados (AppDbContext) injetado via DI.</param>
        public ChatbotController(AppDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Exibe a página principal da interface do Chatbot.
        /// </summary>
        /// <returns>A View de índice do Chatbot.</returns>
        public IActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// Recebe a pergunta do usuário e retorna a resposta do bot.
        /// A lógica prioriza interações rápidas e, em seguida, a busca por similaridade no FAQ.
        /// </summary>
        /// <param name="request">O objeto contendo a pergunta do usuário.</param>
        /// <returns>Um resultado JSON contendo a propriedade 'answer' com a resposta do bot.</returns>
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

        /// <summary>
        /// Busca no banco de dados a Pergunta Frequente (FAQ) mais próxima da pergunta do usuário.
        /// </summary>
        /// <param name="perguntaUsuario">A string da pergunta do usuário já normalizada.</param>
        /// <returns>A resposta da FAQ mais próxima ou uma mensagem padrão de "não encontrado".</returns>
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

        /// <summary>
        /// Calcula a similaridade entre duas strings contando o número de palavras em comum.
        /// A similaridade é usada para classificar qual FAQ é a mais relevante.
        /// </summary>
        /// <param name="texto1">O primeiro texto normalizado (geralmente a pergunta do FAQ).</param>
        /// <param name="texto2">O segundo texto normalizado (a pergunta do usuário).</param>
        /// <returns>Um valor inteiro representando o número de palavras em comum.</returns>
        private int Similaridade(string texto1, string texto2)
        {
            // Divide as frases em palavras, tratando diversos delimitadores (espaço, pontuação)
            var delimitadores = new char[] { ' ', '.', ',', '?', '!', ';' };
            
            var palavras1 = texto1.Split(delimitadores, System.StringSplitOptions.RemoveEmptyEntries);
            var palavras2 = texto2.Split(delimitadores, System.StringSplitOptions.RemoveEmptyEntries);
            
            // Retorna o número de palavras que aparecem em ambos os textos
            return palavras1.Count(p => palavras2.Contains(p));
        }

        /// <summary>
        /// Normaliza o texto, removendo acentos, pontuação e convertendo para minúsculas.
        /// Isso é crucial para a comparação de similaridade.
        /// </summary>
        /// <param name="text">O texto original a ser normalizado.</param>
        /// <returns>O texto processado e normalizado.</returns>
        private string NormalizeText(string text)
        {
            text = text.ToLower();
            text = text.Normalize(NormalizationForm.FormD);
            text = Regex.Replace(text, @"[\u0300-\u036f]", ""); // remove acentos
            text = Regex.Replace(text, @"[^\w\s]", ""); // remove pontuação e caracteres não-alfanuméricos
            return text;
        }
    }

    /// <summary>
    /// Modelo de dados simples usado para receber a requisição JSON de pergunta do front-end.
    /// </summary>
    public class ChatRequest
    {
        /// <summary>
        /// A pergunta digitada e enviada pelo usuário.
        /// </summary>
        public string? Question { get; set; }
    }
}
