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

        public string StatusColor => Status switch
        {
            StepStatus.Done   => "#CCFF00",
            StepStatus.Active => "#FFB800",
            _                 => "#2A2A2A"
        };
        public string StatusBackground => Status switch
        {
            StepStatus.Done   => "#1A2A00",
            StepStatus.Active => "#1A1400",
            _                 => "#141414"
        };
        public string StatusIcon => Status switch
        {
            StepStatus.Done   => "✓",
            StepStatus.Active => "●",
            _                 => "○"
        };
        public string TitleColor => Status == StepStatus.Pending ? "#3A3A3A" : "#FFFFFF";
    }
}
