using CloverAPI.Content.Strings;
using Panik;

namespace CloverAPI.Patches;

[HarmonyPatch]
internal class TranslationPatcher
{
    [HarmonyPatch(typeof(Translation), nameof(Translation.Get))]
    [HarmonyPrefix]
    internal static bool Translation_Get_Prefix(ref string key, ref string __result)
    {
        if (string.IsNullOrEmpty(key))
        {
            return true;
        }

        if (LocalizationManager.TryGetValue(key, out string value))
        {
            __result = value;
            return false;
        }

        return true;
    }

    [HarmonyPatch(typeof(Strings), nameof(Strings.Sanitize))]
    [HarmonyPrefix]
    internal static void Strings_Sanitize_Prefix(ref string input, Strings.SantizationKind santizationKind,
        Strings.SanitizationSubKind subKind)
    {
        StringManager.Sanitize(ref input, santizationKind, subKind);
    }

    [HarmonyPatch(typeof(Strings), nameof(Strings.Sanitize))]
    [HarmonyPostfix]
    internal static void Strings_Sanitize_Postfix(ref string __result, Strings.SantizationKind santizationKind,
        Strings.SanitizationSubKind subKind)
    {
        StringManager.SanitizeLate(ref __result, santizationKind, subKind);
    }
}