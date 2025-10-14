using CloverAPI.Classes.Interfaces;
using System.Collections.Generic;

namespace CloverAPI.Classes;

internal class PriorityComparer : IComparer<IPriority>
{
    public int Compare(IPriority x, IPriority y)
    {
        if (x == null && y == null) return 0;
        if (x == null) return 1;
        if (y == null) return -1;
        return x.Priority.CompareTo(y.Priority);
    }
}