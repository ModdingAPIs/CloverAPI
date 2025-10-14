using BepInEx.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CloverAPI.Internal;

public static class ResourceOrdering
{
    private static readonly object _lock = new();

    private static ConfigEntry<string> _overrideOrdering;
    public static List<string> OrderedPluginUuids { get; private set; } = new();
     
    /// <summary>
    /// Lower number means higher priority.
    /// </summary>
    public static int GetPriority(string pluginUuid)
    {
        lock (_lock)
        {
            if (!OrderedPluginUuids.Contains(pluginUuid))
            {
                OrderedPluginUuids.Add(pluginUuid);
                ApplyToConfig();
            }
            return OrderedPluginUuids.IndexOf(pluginUuid);
        }
    }

    internal static void _ConfigRef(ConfigEntry<string> overrideOrdering)
    {
        lock (_lock)
        {
            if (overrideOrdering == null)
                throw new ArgumentNullException(nameof(overrideOrdering));
            _overrideOrdering = overrideOrdering;
            string[] uuids = overrideOrdering.Value.Split(',').Select(s => s.Trim()).Where(s => !string.IsNullOrEmpty(s)).ToArray();
            OrderedPluginUuids = new List<string>(uuids);
        }
    }
    
    internal static void ApplyToConfig()
    {
        lock (_lock)
        {
            if (_overrideOrdering == null)
                throw new InvalidOperationException("Config reference not set. Call _ConfigRef first.");
            _overrideOrdering.Value = string.Join(", ", OrderedPluginUuids);
        }
    }
}