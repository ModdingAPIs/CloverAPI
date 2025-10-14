using CloverAPI.Classes;
using CloverAPI.Utils;
using System.Collections.Generic;

namespace CloverAPI.Content.Strings;

public static class LocalizationManager
{
    public static Dictionary<string, StringSource> Translations = new();

    public static string Register(string key, StringSource value)
    {
        value.SetKey(key);
        Translations[key] = value;
        return key;
    }

    public static string Get(string key)
    {
        if (Translations.TryGetValue(key, out StringSource value))
        {
            return value.GetString();
        }

        return key;
    }

    public static bool TryGetValueNoCase(string key, out string value)
    {
        if (Translations.TryGetValueNoCase(key, out StringSource src))
        {
            if (src is VanillaLocalizedString)
            {
                value = null;
                return false; // Let the game handle vanilla localization keys
            }

            value = src.GetString();
            return true;
        }

        value = null;
        return false;
    }
}