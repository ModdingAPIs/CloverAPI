using CloverAPI.Classes;
using CloverAPI.Classes.Interfaces;
using CloverAPI.Internal;
using CloverAPI.Utils;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace CloverAPI.Content.Audio;

public static class AudioManager
{
    internal class AudioClipOverride : IPriority
    {
        public AudioClip Clip;
        public string OwnerGuid;
        public int Priority => ResourceOrdering.GetPriority(this.OwnerGuid);
    }
    
    internal static ConcurrentDictionary<string, OrderedList<AudioClipOverride, PriorityComparer>> SoundOverrides = new();
    internal static ConcurrentDictionary<string, OrderedList<AudioClipOverride, PriorityComparer>> MusicOverrides = new();

    public static void RegisterSoundOverride(string name, AudioClip clip, ModGuid guid)
    {
        _RegisterOverride(name, SoundOverrides, clip, guid);
    }
    
    public static void RegisterMusicOverride(string name, AudioClip clip, ModGuid guid)
    {
        _RegisterOverride(name, MusicOverrides, clip, guid);
    }
    
    private static void _RegisterOverride(string name, ConcurrentDictionary<string, OrderedList<AudioClipOverride, PriorityComparer>> dict, AudioClip clip, ModGuid guid)
    {
        if (!dict.ContainsKey(name))
        {
            dict[name] = new();
        }
        var existing = dict[name].FirstOrDefault(o => o.OwnerGuid == guid);
        if (existing != null)
        {
            existing.Clip = clip;
        }
        else
        {
            dict[name].Add(new AudioClipOverride()
            {
                Clip = clip,
                OwnerGuid = guid
            });
        }
    }

    public static bool TryGetSoundOverride(string name, out AudioClip audioClip)
    {
        return _TryGetOverride(name, SoundOverrides, out audioClip);
    }
    
    public static bool TryGetMusicOverride(string name, out AudioClip audioClip)
    {
        return _TryGetOverride(name, MusicOverrides, out audioClip);
    }
    
    public static bool TryGetSoundOverride(out AudioClip audioClip, params string[] names)
    {
        foreach (var name in names)
        {
            if (TryGetSoundOverride(name, out audioClip))
            {
                return true;
            }
        }
        audioClip = null;
        return false;
    }
    
    public static bool TryGetMusicOverride(out AudioClip audioClip, params string[] names)
    {
        foreach (var name in names)
        {
            if (TryGetMusicOverride(name, out audioClip))
            {
                return true;
            }
        }
        audioClip = null;
        return false;
    }
    
    private static bool _TryGetOverride(string name, ConcurrentDictionary<string, OrderedList<AudioClipOverride, PriorityComparer>> dict, out AudioClip audioClip)
    {
        if (dict.TryGetValueNoCase(name, out var overrides) && overrides.Count > 0)
        {
            var highest = overrides[0];
            audioClip = highest.Clip;
            return true;
        }
        audioClip = null;
        return false;
    }
}