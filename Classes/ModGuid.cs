using System;

namespace CloverAPI.Classes;

public readonly struct ModGuid(string guid)
{
    public string Guid { get; } = guid ?? throw new ArgumentNullException(nameof(guid));
    
    public override string ToString()
    {
        return this.Guid;
    }
    
    public static implicit operator string(ModGuid modGuid) => modGuid.Guid;
    public static implicit operator ModGuid(string str) => new(str);
    public static implicit operator ModGuid(BaseUnityPlugin plugin) => new(plugin.Info?.Metadata?.GUID ?? throw new ArgumentNullException(nameof(plugin)));
}