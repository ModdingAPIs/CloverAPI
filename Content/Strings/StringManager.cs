using CloverAPI.Classes;
using System;
using System.Collections.Generic;
using System.Text;
using static Strings;

namespace CloverAPI.Content.Strings;

public class StringManager
{
    private static readonly Dictionary<string, StringSource> Strings = new();

    private static readonly Dictionary<string, Tuple<StringSource, SantizationKind, SanitizationSubKind>>
        ConditionalStrings = new();

    private static readonly Dictionary<string, StringSource> StringsLate = new();

    private static readonly Dictionary<string, Tuple<StringSource, SantizationKind, SanitizationSubKind>>
        ConditionalStringsLate = new();

    public static void RegisterString(string key, StringSource value, bool late = false)
    {
        if (late)
        {
            StringsLate[key] = value;
        }
        else
        {
            Strings[key] = value;
        }
    }

    public static void RegisterConditional(string key, StringSource value, SantizationKind kind,
        SanitizationSubKind subKind, bool late = false)
    {
        if (late)
        {
            ConditionalStringsLate[key] =
                new Tuple<StringSource, SantizationKind, SanitizationSubKind>(value, kind, subKind);
        }
        else
        {
            ConditionalStrings[key] =
                new Tuple<StringSource, SantizationKind, SanitizationSubKind>(value, kind, subKind);
        }
    }

    public static void Sanitize(ref string s, SantizationKind santizationKind, SanitizationSubKind subKind)
    {
        StringBuilder sb = new(s);
        foreach (string key in Strings.Keys)
        {
            sb.Replace(key, Strings[key].GetString());
        }

        foreach (string key in ConditionalStrings.Keys)
        {
            var (value, kind, sub) = ConditionalStrings[key];
            if ((santizationKind == SantizationKind.all || santizationKind == kind) &&
                (subKind == SanitizationSubKind.none || subKind == sub))
            {
                sb.Replace(key, value.GetString());
            }
        }

        s = sb.ToString();
    }

    public static void SanitizeLate(ref string s, SantizationKind santizationKind, SanitizationSubKind subKind)
    {
        StringBuilder sb = new(s);
        foreach (string key in StringsLate.Keys)
        {
            sb.Replace(key, StringsLate[key].GetString());
        }

        foreach (string key in ConditionalStringsLate.Keys)
        {
            var (value, kind, sub) = ConditionalStringsLate[key];
            if ((santizationKind == SantizationKind.all || santizationKind == kind) &&
                (subKind == SanitizationSubKind.none || subKind == sub))
            {
                sb.Replace(key, value.GetString());
            }
        }

        s = sb.ToString();
    }
}