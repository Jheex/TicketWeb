using Microsoft.AspNetCore.Mvc.Rendering;
using PIM.Models;
using System.Collections.Generic;
using System;
// Note: As classes ChartData e AnalystPerformanceData devem estar definidas no mesmo namespace para compilar.

namespace PIM.ViewModels
{
    /// <summary>
    /// Contém as listas de opções disponíveis para os filtros do dashboard (Dropdowns).
    /// As listas são preenchidas com objetos <see cref="SelectListItem"/> para uso em formulários HTML.
    /// </summary>
    public class DashboardFilterOptions
    {
        /// <summary>
        /// Lista de opções de Status disponíveis para filtragem.
        /// </summary>
        public List<SelectListItem> Statuses { get; set; } = new List<SelectListItem>();
        
        /// <summary>
        /// Lista de opções de Prioridade disponíveis para filtragem.
        /// </summary>
        public List<SelectListItem> Priorities { get; set; } = new List<SelectListItem>();
        
        /// <summary>
        /// Lista de opções de Categoria disponíveis para filtragem.
        /// </summary>
        public List<SelectListItem> Categories { get; set; } = new List<SelectListItem>();
        
        /// <summary>
        /// Lista de opções de Analistas/Técnicos disponíveis para atribuição e filtragem.
        /// </summary>
        public List<SelectListItem> Analysts { get; set; } = new List<SelectListItem>();
        
        /// <summary>
        /// Lista de opções de Solicitantes/Usuários (Requesters) disponíveis para filtragem.
        /// </summary>
        public List<SelectListItem> Requesters { get; set; } = new List<SelectListItem>();
    }

    /// <summary>
    /// View Model principal utilizada para carregar e exibir todos os dados necessários no Dashboard de Business Intelligence.
    /// Inclui KPIs, dados para gráficos, tabelas de desempenho, e as opções e estados de filtro/paginação.
    /// </summary>
    public class DashboardBIViewModel
    {
        // ==================== KPIs (Key Performance Indicators) ====================
        
        /// <summary>
        /// Total de chamados no status "Aberto".
        /// </summary>
        public int TotalChamadosAbertos { get; set; }
        
        /// <summary>
        /// Total de chamados no status "Em Andamento".
        /// </summary>
        public int TotalChamadosEmAndamento { get; set; }
        
        /// <summary>
        /// Total de chamados Fechados (resolvidos ou concluídos).
        /// </summary>
        public int TotalChamadosFechados { get; set; }
        
        /// <summary>
        /// Total de novos chamados abertos em um período específico (ex: hoje, na semana).
        /// </summary>
        public int TotalChamadosNovos { get; set; }
        
        /// <summary>
        /// Tempo médio de resolução de tickets, calculado e expresso em horas.
        /// </summary>
        public double TempoMedioResolucaoHoras { get; set; }
        
        /// <summary>
        /// Percentual de tickets que cumpriram o SLA (Service Level Agreement).
        /// </summary>
        public double SlaPercentual { get; set; }
        
        /// <summary>
        /// Número de tickets pendentes ou que ultrapassaram o prazo ou estão em atraso (Backlog).
        /// </summary>
        public int Backlog { get; set; }

        // ==================== Gráficos ====================
        
        /// <summary>
        /// Dados para o gráfico de Abertos vs. Fechados ao longo do tempo. O tipo é 'object' para acomodar estruturas de dados específicas de bibliotecas de gráficos (Ex: Chart.js).
        /// </summary>
        public object? AbertosVsFechadosChart { get; set; }
        
        /// <summary>
        /// Dados de contagem de tickets pelas Top Categorias, formatados para um gráfico.
        /// </summary>
        public ChartData TopCategoriasChart { get; set; } = new ChartData();
        
        /// <summary>
        /// Dados de contagem de tickets agrupados por Status.
        /// </summary>
        public ChartData StatusChart { get; set; } = new ChartData();
        
        /// <summary>
        /// Dados de contagem de tickets agrupados por Analista/Técnico.
        /// </summary>
        public ChartData AnalistaChart { get; set; } = new ChartData();
        
        /// <summary>
        /// Dados de contagem de tickets agrupados por Prioridade.
        /// </summary>
        public ChartData PrioridadeChart { get; set; } = new ChartData();

        // ==================== Tabelas ====================
        
        /// <summary>
        /// Lista detalhada do desempenho de cada analista.
        /// </summary>
        public List<AnalystPerformanceData> RankingAnalistas { get; set; } = new List<AnalystPerformanceData>();
        
        /// <summary>
        /// Tabela com detalhes dos objetos <see cref="Chamado"/>, usada para a visualização detalhada, paginada ou filtrada.
        /// </summary>
        public List<Chamado> TabelaDetalhada { get; set; } = new List<Chamado>();

        // ==================== Opções para os Filtros ====================
        
        /// <summary>
        /// Opções disponíveis para serem usadas nos Dropdowns de filtro.
        /// </summary>
        public DashboardFilterOptions FilterOptions { get; set; } = new DashboardFilterOptions();

        // ==================== PAGINAÇÃO ====================
        
        /// <summary>
        /// O número da página atual na Tabela Detalhada.
        /// </summary>
        public int PageNumber { get; set; } = 1;
        
        /// <summary>
        /// O número de itens a serem exibidos por página.
        /// </summary>
        public int PageSize { get; set; } = 5;
        
        /// <summary>
        /// O número total de itens que satisfazem os filtros aplicados.
        /// </summary>
        public int TotalItems { get; set; } = 0;
        
        /// <summary>
        /// Propriedade calculada que retorna o número total de páginas.
        /// </summary>
        public int TotalPages => (int)Math.Ceiling((double)TotalItems / PageSize);

        // ==================== FILTROS SELECIONADOS ====================
        
        /// <summary>
        /// Lista de status selecionados pelo usuário para filtrar os dados.
        /// </summary>
        public List<string> SelectedStatus { get; set; } = new List<string>();
        
        /// <summary>
        /// Lista de prioridades selecionadas pelo usuário para filtrar os dados.
        /// </summary>
        public List<string> SelectedPriority { get; set; } = new List<string>();
        
        /// <summary>
        /// Lista de IDs de analistas/técnicos selecionados para filtrar os dados.
        /// </summary>
        public List<int> SelectedAnalystId { get; set; } = new List<int>();
        
        /// <summary>
        /// Lista de categorias selecionadas pelo usuário para filtrar os dados.
        /// </summary>
        public List<string> SelectedCategory { get; set; } = new List<string>();
        
        /// <summary>
        /// Lista de IDs de solicitantes (requesters) selecionados para filtrar os dados.
        /// </summary>
        public List<int> SelectedRequesterId { get; set; } = new List<int>();
        
    }
}