using System;
using System.Collections.Generic;
using System.Text;

namespace Tienda.Models
{
    public enum StepStatus { Done, Active, Pending }
    public class OrderStep
    {
        public string Title { get; set; } = string.Empty;
        public string Subtitle { get; set; } = string.Empty;
        public StepStatus Status { get; set; }
    }
}
