using CloverAPI.Classes;
using CloverAPI.Classes.Interfaces;
using CloverAPI.Internal;
using CloverAPI.Utils;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace CloverAPI.Content.Textures;

public class TextureManager
{
    internal class TextureOverride : IPriority
    {
        public Texture2D Texture;
        public string OwnerGuid;
        public int Priority => ResourceOrdering.GetPriority(this.OwnerGuid);
    }
    
    internal static ConcurrentDictionary<string, OrderedList<TextureOverride, PriorityComparer>> TextureOverrides = new();

    public static void RegisterTextureOverride(string name, Texture2D texture, ModGuid guid)
    {
        _RegisterOverride(name, TextureOverrides, texture, guid);
    }
    
    private static void _RegisterOverride(string name, ConcurrentDictionary<string, OrderedList<TextureOverride, PriorityComparer>> dict, Texture2D texture, ModGuid guid)
    {
        if (!dict.ContainsKey(name))
        {
            dict[name] = new();
        }
        var existing = dict[name].FirstOrDefault(o => o.OwnerGuid == guid);
        if (existing != null)
        {
            existing.Texture = texture;
        }
        else
        {
            dict[name].Add(new TextureOverride
            {
                Texture = texture,
                OwnerGuid = guid
            });
        }
    }

    public static bool TryGetTextureOverride(string name, out Texture2D texture)
    {
        return _TryGetOverride(name, TextureOverrides, out texture);
    }
    
    public static bool TryGetTextureOverride(out Texture2D texture, params string[] names)
    {
        foreach (var name in names)
        {
            if (_TryGetOverride(name, TextureOverrides, out texture))
            {
                return true;
            }
        }
        texture = null;
        return false;
    }
    
    private static bool _TryGetOverride(string name, ConcurrentDictionary<string, OrderedList<TextureOverride, PriorityComparer>> dict, out Texture2D texture)
    {
        if (dict.TryGetValueNoCase(name, out var overrides) && overrides.Count > 0)
        {
            var highest = overrides[0];
            texture = highest.Texture;
            return true;
        }
        texture = null;
        return false;
    }
}