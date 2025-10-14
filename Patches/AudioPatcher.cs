using CloverAPI.Content.Audio;
using Panik;

namespace CloverAPI.Patches;

[HarmonyPatch]
public class AudioPatcher
{
    [HarmonyPatch(typeof(AssetMaster), nameof(AssetMaster.GetSound))]
    [HarmonyPrefix]
    internal static bool AssetMaster_GetSound_Prefix(ref AudioClip __result, ref string clipName)
    {
        if (AudioManager.TryGetSoundOverride(clipName, out AudioClip clip))
        {
            __result = clip;
            return false;
        }
        return true;
    }    
    [HarmonyPatch(typeof(AssetMaster), nameof(AssetMaster.GetMusic))]
    [HarmonyPrefix]
    internal static bool AssetMaster_GetMusic_Prefix(ref AudioClip __result, ref string clipName)
    {
        if (AudioManager.TryGetMusicOverride(clipName, out AudioClip clip))
        {
            __result = clip;
            return false;
        }
        return true;
    }}