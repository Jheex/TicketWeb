using Microsoft.AspNetCore.Mvc;

namespace PIM.Controllers
{
    /// <summary>
    /// Controlador responsável por servir páginas estáticas de conteúdo, como FAQ, Política de Privacidade e Termos de Uso.
    /// Essas páginas geralmente não requerem lógica de negócios ou acesso a dados.
    /// </summary>
    public class StaticPagesController : Controller
    {
        /// <summary>
        /// Exibe a página de Perguntas Frequentes (FAQ).
        /// </summary>
        /// <returns>A View FAQ (procura Views/StaticPages/FAQ.cshtml).</returns>
        public IActionResult FAQ()
        {
            return View(); // procura Views/StaticPages/FAQ.cshtml
        }
        
        /// <summary>
        /// Exibe a página de Política de Privacidade.
        /// </summary>
        /// <returns>A View Privacy (procura Views/StaticPages/Privacy.cshtml).</returns>
        public IActionResult Privacy()
        {
            return View();
        }

        /// <summary>
        /// Exibe a página de Termos de Uso.
        /// </summary>
        /// <returns>A View Terms (procura Views/StaticPages/Terms.cshtml).</returns>
        public IActionResult Terms()
        {
            return View();
        }

        /// <summary>
        /// Exibe a página de Suporte ou Contato (informações de contato).
        /// </summary>
        /// <returns>A View Support (procura Views/StaticPages/Support.cshtml).</returns>
        public IActionResult Support()
        {
            return View();
        }
    }
}