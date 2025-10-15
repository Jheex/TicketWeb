using System.Collections.Generic;

namespace PIM.ViewModels
{
    /// <summary>
    /// Estrutura utilizada para formatar dados de gráficos simples (eixo X e eixo Y),
    /// como a contagem de tickets por categoria ou status.
    /// </summary>
    public class ChartData
    {
        /// <summary>
        /// Rótulos ou nomes para os pontos de dados (eixo X do gráfico). Ex: ["Aberto", "Fechado", "Pendente"].
        /// </summary>
        public List<string> Labels { get; set; } = new List<string>();
        
        /// <summary>
        /// Valores numéricos correspondentes aos rótulos (eixo Y do gráfico). Ex: [15, 25, 5].
        /// </summary>
        public List<int> Data { get; set; } = new List<int>();
    }

    /// <summary>
    /// Estrutura utilizada para exibir o desempenho individual de um analista/técnico no dashboard.
    /// </summary>
    public class AnalystPerformanceData
    {
        /// <summary>
        /// Nome do analista/técnico.
        /// </summary>
        public string? AnalystName { get; set; }
        
        /// <summary>
        /// Número total de tickets atribuídos a este analista.
        /// </summary>
        public int AssignedTickets { get; set; }
        
        /// <summary>
        /// Número total de tickets que o analista fechou.
        /// </summary>
        public int ClosedTickets { get; set; }
        
        /// <summary>
        /// Tempo médio de resolução dos tickets fechados por este analista, geralmente em horas ou dias.
        /// </summary>
        public double AverageResolutionTime { get; set; }
    }
}