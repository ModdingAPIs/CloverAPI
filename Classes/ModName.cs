using System;

namespace CloverAPI.Classes;

public readonly struct ModName(string name)
{
    public string Name { get; } = name ?? throw new ArgumentNullException(nameof(name));
    
    public override string ToString()
    {
        return this.Name;
    }
    
    public static implicit operator string(ModName modName) => modName.Name;
    public static implicit operator ModName(string str) => new(str);
    public static implicit operator ModName(BaseUnityPlugin plugin) => new(plugin.Info?.Metadata?.Name ?? plugin.GetType().Name);
}