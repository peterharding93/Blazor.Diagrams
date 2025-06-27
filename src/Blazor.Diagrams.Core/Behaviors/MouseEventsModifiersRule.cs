using Blazor.Diagrams.Core.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Blazor.Diagrams.Core.Behaviors;
public record MouseEventsModifierRule
{
    public bool CtrlKey { get; set; }
    public bool ShiftKey { get; set; }
    public bool AltKey { get; set; }
    public bool TestMatch(PointerEventArgs e)
    {
        return (e.ShiftKey == ShiftKey & e.CtrlKey == CtrlKey && e.AltKey == AltKey);
    }
    public bool TestMatch(WheelEventArgs e)
    {
        return (e.ShiftKey == ShiftKey && e.CtrlKey == CtrlKey && e.AltKey == AltKey);
    }
}