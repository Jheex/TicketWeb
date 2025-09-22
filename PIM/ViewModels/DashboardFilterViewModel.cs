using System;
using System.Collections.Generic;

namespace PIM.ViewModels
{
    public class DashboardFilterViewModel
    {
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public List<string>? Status { get; set; }
        public List<string>? Priority { get; set; }
        public List<int>? AssignedToId { get; set; }
        public List<int>? RequesterId { get; set; }

        // Propriedades para paginação
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 5;
    }
}
