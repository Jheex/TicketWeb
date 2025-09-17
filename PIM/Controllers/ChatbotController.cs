using Microsoft.AspNetCore.Mvc;

namespace PIM.Controllers
{
    public class ChatbotController : Controller
    {
        // Exibe a página do chatbot
        public IActionResult Index()
        {
            return View();
        }

        // Endpoint de perguntas
        [HttpPost]
        public IActionResult Ask([FromBody] ChatRequest request)
        {
            // Resposta simulada por enquanto
            string resposta = $"Você perguntou: {request.Question}. (Em breve vou buscar no banco e responder com IA!)";

            return Json(new { answer = resposta });
        }
    }

    public class ChatRequest
    {
        public string? Question { get; set; }
    }
}
