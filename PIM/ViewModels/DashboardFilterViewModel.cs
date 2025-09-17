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
    }
}
