using CloverAPI.Utils;
using Panik;
using System.Collections;

namespace CloverAPI.Patches;

[HarmonyPatch]
public class GamePatcher
{
    [HarmonyPatch(typeof(IntroScript), nameof(IntroScript.Start))]
    [HarmonyPrefix]
    internal static void IntroScript_Start_Prefix()
    {
        GameUtils.TriggerGameReady();
    }
    
    [HarmonyPatch(typeof(Master), nameof(Master._PlatformKind), MethodType.Getter)]
    [HarmonyPrefix]
    internal static bool Master__PlatformKind_Get_Prefix(ref PlatformMaster.PlatformKind __result)
    {
        // For some reason, PlatformMaster.PlatformKindGet returns Undefined before Master is initialized.
        // We're obviously on PC, so just return that. Why does this even matter? No idea. But it'll throw an error
        // if we don't.
        __result = PlatformMaster.PlatformKind.PC;
        return false;
    }
}