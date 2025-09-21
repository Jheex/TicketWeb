using Microsoft.AspNetCore.Mvc.Rendering;
using PIM.Models;

namespace PIM.ViewModels
{
    public class DashboardFilterOptions
    {
        public List<SelectListItem> Statuses { get; set; } = new List<SelectListItem>();
        public List<SelectListItem> Priorities { get; set; } = new List<SelectListItem>();
        public List<SelectListItem> Categories { get; set; } = new List<SelectListItem>();
        public List<SelectListItem> Analysts { get; set; } = new List<SelectListItem>();
        public List<SelectListItem> Requesters { get; set; } = new List<SelectListItem>();
    }

    public class DashboardBIViewModel
    {
        // KPIs
        public int TotalChamadosAbertos { get; set; }
        public int TotalChamadosFechados { get; set; }
        public int TotalChamadosNovos { get; set; }
        public double TempoMedioResolucaoHoras { get; set; }
        public double SlaPercentual { get; set; }
        public int Backlog { get; set; }

        // Gráficos
        public object? AbertosVsFechadosChart { get; set; }
        public ChartData TopCategoriasChart { get; set; } = new ChartData();
        public ChartData StatusChart { get; set; } = new ChartData();
        public ChartData AnalistaChart { get; set; } = new ChartData();
        public ChartData PrioridadeChart { get; set; } = new ChartData();

        // Tabelas
        public List<AnalystPerformanceData> RankingAnalistas { get; set; } = new List<AnalystPerformanceData>();
        public List<Chamado> TabelaDetalhada { get; set; } = new List<Chamado>();

        // Opções para os Filtros
        public DashboardFilterOptions FilterOptions { get; set; } = new DashboardFilterOptions();

        // ==================== PAGINAÇÃO ====================
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 5;
        public int TotalItems { get; set; } = 0;
        public int TotalPages => (int)Math.Ceiling((double)TotalItems / PageSize);

        // ==================== FILTROS SELECIONADOS ====================
        public List<string> SelectedStatus { get; set; } = new List<string>();
        public List<string> SelectedPriority { get; set; } = new List<string>();
        public List<int> SelectedAnalystId { get; set; } = new List<int>();
        public List<string> SelectedCategory { get; set; } = new List<string>();
        public List<int> SelectedRequesterId { get; set; } = new List<int>();
        
    }
}
