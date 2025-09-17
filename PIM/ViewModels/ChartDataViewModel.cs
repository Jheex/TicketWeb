namespace PIM.ViewModels
{
    public class ChartData
    {
        public List<string> Labels { get; set; } = new List<string>();
        public List<int> Data { get; set; } = new List<int>();
    }

    public class AnalystPerformanceData
    {
        public string? AnalystName { get; set; }
        public int AssignedTickets { get; set; }
        public int ClosedTickets { get; set; }
        public double AverageResolutionTime { get; set; }
    }
}