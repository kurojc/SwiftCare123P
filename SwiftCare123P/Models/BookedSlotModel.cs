using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SwiftCare123P.Models;

public class BookedSlotModel
{
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }

    public string TimeRangeDisplay => $"{DateTime.Today.Add(StartTime):h:mm tt} – {DateTime.Today.Add(EndTime):h:mm tt}";
}