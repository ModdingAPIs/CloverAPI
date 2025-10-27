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

        var cont = true;
        if (LocalizationManager.TryGetValueNoCase(key, out string value))
        {
            __result = value;
            cont = false;
        };
        if (LanguageManager.IsCustomLanguage && LanguageManager.TryGetTerm(key, out string customValue))
        {
            __result = customValue;
            cont = false;
        }

        return cont;
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
    
    // This one is only used for the dynamic sprites for the charms & phone texts
    // We'll fall back to the English one if a custom language is used since localized sprites
    // are not supported yet.
    [HarmonyPatch(typeof(Translation), nameof(Translation.LanguageGet))]
    [HarmonyPrefix]
    internal static bool Translation_LanguageGet_Prefix(ref Translation.Language __result)
    {
        if (LanguageManager.IsCustomLanguage)
        {
            __result = Translation.Language.English;
            return false;
        }

        return true;
    }
}